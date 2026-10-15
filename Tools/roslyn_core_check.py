#!/usr/bin/env python3
"""Semantic Roslyn compile of every Assets/Scripts and Assets/Editor script.

UnityEngine in Tools/roslyn/UnityStub.cs and UnityEditor in
Tools/roslyn/UnityEditorStub.cs stand in for the Unity 6000.6 assemblies.
The compile defines UNITY_EDITOR so editor-only branches are bound.
Any C# error fails the gate, including a missing or invented Unity member.
CS0162 unreachable-code warnings in project scripts are counted and fail
the gate when the count is greater than zero.
"""

from __future__ import annotations

import os
import re
import shutil
import subprocess
import tempfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
STUB = ROOT / "Tools" / "roslyn" / "UnityStub.cs"
EDITOR_STUB = ROOT / "Tools" / "roslyn" / "UnityEditorStub.cs"
_DECL = re.compile(r"\b(?:class|struct|enum|interface)\s+([A-Za-z_][A-Za-z0-9_]*)\b")
_DIAG = re.compile(
    r"^(?P<file>.+)\((?P<line>\d+),(?P<col>\d+)\): "
    r"(?P<kind>error|warning) (?P<code>CS\d+): (?P<msg>.*)$"
)
_CSPROJ = re.compile(r" \[[^\]]+\.csproj\]$")
_NON_CS = re.compile(r"error (MSB\d+|NETSDK\d+)\b")
_QUOTED = re.compile(r"'([^']+)'")
_SCRIPT_TOKENS = ("Assets/Editor/", "Assets/Scripts/")


def find_dotnet() -> str | None:
    found = shutil.which("dotnet")
    if found:
        return found
    roots = [os.environ.get("DOTNET_ROOT", ""), "/tmp/dotnet", str(Path.home() / ".dotnet")]
    for root in roots:
        if not root:
            continue
        candidate = Path(root) / "dotnet"
        if candidate.is_file() and os.access(candidate, os.X_OK):
            return str(candidate)
    return None


def stub_type_names() -> set[str]:
    if not STUB.is_file():
        return set()
    return set(_DECL.findall(STUB.read_text(encoding="utf-8")))


def compiled_sources() -> list[Path]:
    """Project scripts the gate compiles, stable path order."""
    found: list[Path] = []
    for folder in (ROOT / "Assets" / "Scripts", ROOT / "Assets" / "Editor"):
        if folder.is_dir():
            found.extend(sorted(path for path in folder.rglob("*.cs") if path.is_file()))
    return found


# Errors that mean a type was never bound. Roslyn then skips method bodies,
# so a later DailySeed.Mix shadow can pass with an empty report.
_DECLARATION_CODES = frozenset(
    {
        "CS0246",
        "CS0234",
        "CS0535",
        "CS0534",
        "CS0115",
        "CS0506",
        "CS0101",
        "CS0102",
        "CS0111",
        "CS0260",
        "CS0616",
        "CS0501",
        "CS0012",
        "CS0542",
    }
)


def is_unity_stub_artefact(code: str, message: str, stub_types: set[str]) -> bool:
    """True when the diagnostic is a hole in the Unity base-class stub.

    The compile gate does not drop these. Stubs are complete, and a missing
    member such as HorizontalWrapMode.Truncate must fail the build.
    """
    if "UnityEngine" in message and code in {"CS0234", "CS0246"}:
        return True
    quoted = _QUOTED.findall(message)
    if not quoted:
        return False
    receiver = quoted[0]
    if "." in receiver:
        receiver = receiver.rsplit(".", 1)[-1]
    if code in {"CS1061", "CS0117", "CS0246", "CS1729", "CS0311"} and receiver in stub_types:
        return True
    return False


def _script_rel(path: str) -> str | None:
    for token in _SCRIPT_TOKENS:
        index = path.find(token)
        if index >= 0:
            return path[index:]
    return None


def _parse_kind(output: str, kind: str) -> list[tuple[str, str, str, str]]:
    found: list[tuple[str, str, str, str]] = []
    seen: set[tuple[str, str, str, str]] = set()
    for line in output.splitlines():
        match = _DIAG.match(line.strip())
        if not match or match.group("kind") != kind:
            continue
        message = _CSPROJ.sub("", match.group("msg")).strip()
        key = (match.group("file"), match.group("line"), match.group("code"), message)
        if key in seen:
            continue
        seen.add(key)
        found.append(key)
    return found


def _parse_errors(output: str) -> list[tuple[str, str, str, str]]:
    return _parse_kind(output, "error")


def _parse_warnings(output: str) -> list[tuple[str, str, str, str]]:
    return _parse_kind(output, "warning")


def _project(work: Path, sources: list[Path]) -> None:
    includes = [
        '    <Compile Include="UnityStub.cs" />',
        '    <Compile Include="UnityEditorStub.cs" />',
    ]
    for path in sources:
        includes.append(f'    <Compile Include="{path.as_posix()}" />')
    body = "\n".join(includes)
    (work / "core.csproj").write_text(
        "\n".join(
            (
                '<Project Sdk="Microsoft.NET.Sdk">',
                "  <PropertyGroup>",
                "    <TargetFramework>net8.0</TargetFramework>",
                "    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>",
                "    <Nullable>disable</Nullable>",
                "    <ImplicitUsings>disable</ImplicitUsings>",
                "    <LangVersion>latest</LangVersion>",
                "    <DefineConstants>TRACE;DEBUG;UNITY_EDITOR</DefineConstants>",
                "  </PropertyGroup>",
                "  <ItemGroup>",
                body,
                "  </ItemGroup>",
                "</Project>",
                "",
            )
        ),
        encoding="utf-8",
    )


def _build(dotnet: str, work: Path) -> tuple[int, str]:
    env = os.environ.copy()
    env["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1"
    env["DOTNET_NOLOGO"] = "1"
    env["DOTNET_ROOT"] = str(Path(dotnet).resolve().parent)
    result = subprocess.run(
        [dotnet, "build", "core.csproj", "--nologo", "-v", "q"],
        cwd=work,
        env=env,
        text=True,
        stdout=subprocess.PIPE,
        stderr=subprocess.STDOUT,
        check=False,
    )
    return result.returncode, result.stdout


def non_cs_build_failure(returncode: int, output: str) -> list[str]:
    """MSB/NETSDK failures, or a non-zero exit with no C# diagnostics, are incomplete.

    A non-zero exit that still printed error CS lines is a C# compile failure.
    Those CS errors are reported by the caller; this helper must not hide them
    behind a second incomplete result.
    """
    lines: list[str] = []
    seen: set[str] = set()
    for line in output.splitlines():
        if _NON_CS.search(line) is None:
            continue
        text = line.strip()
        if text in seen:
            continue
        seen.add(text)
        lines.append(text)
    if lines:
        return ["INCOMPLETE: dotnet build failed outside the C# compiler:", *lines]
    if returncode != 0 and not _parse_errors(output):
        tail = output.strip().splitlines()
        shown = tail[-8:] if tail else ["(no output)"]
        return [f"INCOMPLETE: dotnet build exited {returncode} with no C# diagnostics.", *shown]
    return []


def _overlay_path(path: Path, work: Path, overlays: dict[str, str]) -> Path:
    try:
        relative = path.resolve().relative_to(ROOT.resolve()).as_posix()
    except ValueError:
        return path
    replacement = overlays.get(relative)
    if replacement is None:
        return path
    dest = work / "overlay" / relative
    dest.parent.mkdir(parents=True, exist_ok=True)
    dest.write_text(replacement, encoding="utf-8")
    return dest


def _cs0162_hits(output: str) -> list[str]:
    warnings: list[str] = []
    for path, line, code, message in _parse_warnings(output):
        if code != "CS0162":
            continue
        relative = _script_rel(path)
        if relative is None:
            continue
        warnings.append(f"{relative}:{line}: warning {code}: {message}")
    if not warnings:
        return []
    return [*warnings, f"CS0162 warning count: {len(warnings)}"]


def core_roslyn_errors(overlays: dict[str, str] | None = None) -> list[str]:
    dotnet = find_dotnet()
    if dotnet is None:
        return ["Roslyn script compile needs the .NET SDK (dotnet on PATH, DOTNET_ROOT, or /tmp/dotnet)"]
    if not STUB.is_file():
        return ["missing Tools/roslyn/UnityStub.cs"]
    if not EDITOR_STUB.is_file():
        return ["missing Tools/roslyn/UnityEditorStub.cs"]

    replacements = overlays or {}
    sources = compiled_sources()
    if not sources:
        return ["Roslyn script compile found no Assets/Scripts or Assets/Editor sources"]

    work = Path(tempfile.mkdtemp(prefix="agr-roslyn-"))
    try:
        shutil.copyfile(STUB, work / "UnityStub.cs")
        shutil.copyfile(EDITOR_STUB, work / "UnityEditorStub.cs")
        compiled = [_overlay_path(path, work, replacements) for path in sources]
        _project(work, compiled)
        last_code, last_output = _build(dotnet, work)

        incomplete = non_cs_build_failure(last_code, last_output)
        if incomplete:
            return incomplete

        problems: list[str] = []
        blockers: list[str] = []
        for path, line, code, message in _parse_errors(last_output):
            relative = _script_rel(path)
            shown = relative if relative is not None else path
            entry = f"{shown}:{line}: {code}: {message}"
            if code in _DECLARATION_CODES:
                blockers.append(entry)
                continue
            problems.append(entry)
        warnings = _cs0162_hits(last_output)
        if blockers:
            banner = (
                "INCOMPLETE: Roslyn did not fully bind method bodies. "
                "Declaration errors remain:"
            )
            return [banner, *blockers, *problems, *warnings]
        return [*problems, *warnings]
    finally:
        shutil.rmtree(work, ignore_errors=True)
