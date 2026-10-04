#!/usr/bin/env python3
"""Semantic Roslyn compile of every Assets/Scripts/Core script.

UnityEngine in Tools/roslyn/UnityStub.cs is a stand-in for the editor
assemblies. Errors whose receiver is one of those stub types are stub
artefacts. Every other error reported in a Core script fails the check.
Types that Core uses from the rest of the game are compiled from their
real source so a missing member on a project type is a real error.
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
_DECL = re.compile(r"\b(?:class|struct|enum|interface)\s+([A-Za-z_][A-Za-z0-9_]*)\b")
_ERROR = re.compile(
    r"^(?P<file>.+)\((?P<line>\d+),(?P<col>\d+)\): error (?P<code>CS\d+): (?P<msg>.*)$"
)
_QUOTED = re.compile(r"'([^']+)'")
_MISSING_NAME = re.compile(
    r"The type or namespace name '([^']+)'|The name '([^']+)' does not exist"
)


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


def game_type_index() -> dict[str, Path]:
    """First non-Editor script that declares each type name."""
    index: dict[str, Path] = {}
    scripts = ROOT / "Assets" / "Scripts"
    for path in sorted(scripts.rglob("*.cs")):
        if "Assets/Editor/" in path.as_posix() or path.as_posix().endswith("/Editor/" + path.name):
            continue
        if "/Editor/" in path.as_posix():
            continue
        text = path.read_text(encoding="utf-8")
        for name in _DECL.findall(text):
            index.setdefault(name, path)
    return index


def is_unity_stub_artefact(code: str, message: str, stub_types: set[str]) -> bool:
    """True when the diagnostic is a hole in the Unity base-class stub."""
    if "UnityEngine" in message and code in {"CS0234", "CS0246"}:
        return True
    quoted = _QUOTED.findall(message)
    if not quoted:
        return False
    receiver = quoted[0]
    if code in {"CS1061", "CS0117", "CS0246", "CS1729", "CS0311"} and receiver in stub_types:
        return True
    return False


def _core_rel(path: str) -> str | None:
    token = "Assets/Scripts/Core/"
    if token not in path:
        return None
    return path[path.index(token) :]


def _parse_errors(output: str) -> list[tuple[str, str, str, str]]:
    found: list[tuple[str, str, str, str]] = []
    seen: set[tuple[str, str, str]] = set()
    for line in output.splitlines():
        match = _ERROR.match(line.strip())
        if not match:
            continue
        key = (match.group("file"), match.group("line"), match.group("code"), match.group("msg"))
        if key in seen:
            continue
        seen.add(key)
        found.append(key)
    return found


def _missing_type(message: str) -> str | None:
    match = _MISSING_NAME.search(message)
    if not match:
        return None
    return match.group(1) or match.group(2)


def _project(work: Path, sources: list[Path]) -> None:
    includes = ['    <Compile Include="UnityStub.cs" />']
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


def _build(dotnet: str, work: Path) -> str:
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
    return result.stdout


def core_roslyn_errors() -> list[str]:
    dotnet = find_dotnet()
    if dotnet is None:
        return ["Roslyn Core compile needs the .NET SDK (dotnet on PATH, DOTNET_ROOT, or /tmp/dotnet)"]
    if not STUB.is_file():
        return ["missing Tools/roslyn/UnityStub.cs"]

    index = game_type_index()
    stub_types = stub_type_names()
    core_dir = ROOT / "Assets" / "Scripts" / "Core"
    sources: list[Path] = sorted(core_dir.rglob("*.cs"))
    included = {path.resolve() for path in sources}

    work = Path(tempfile.mkdtemp(prefix="agr-roslyn-"))
    try:
        shutil.copyfile(STUB, work / "UnityStub.cs")
        last_output = ""
        for _round in range(8):
            _project(work, sources)
            last_output = _build(dotnet, work)
            errors = _parse_errors(last_output)
            added = False
            for _file, _line, code, message in errors:
                if code not in {"CS0246", "CS0103"}:
                    continue
                name = _missing_type(message)
                if not name or name in stub_types:
                    continue
                path = index.get(name)
                if path is None:
                    continue
                resolved = path.resolve()
                if resolved in included:
                    continue
                if "Assets/Scripts/Core/" in path.as_posix():
                    continue
                sources.append(path)
                included.add(resolved)
                added = True
            if not added:
                break

        problems: list[str] = []
        for path, line, code, message in _parse_errors(last_output):
            relative = _core_rel(path)
            if relative is None:
                continue
            if is_unity_stub_artefact(code, message, stub_types):
                continue
            problems.append(f"{relative}:{line}: {code}: {message}")
        return problems
    finally:
        shutil.rmtree(work, ignore_errors=True)
