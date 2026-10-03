#!/usr/bin/env python3
"""SV/EN completeness for Loc.T / Loc.Tf call sites.

Kenney Future omits U+2699. Button captions stay within 28 characters.
"""

from __future__ import annotations

import re
from pathlib import Path

# Same gap as Tools/validate_week1_project.py `_MISSING_UI_FONT`.
MISSING_CODEPOINTS = {0x2699}

# Compact button captions. Body copy, hints, and shop descriptions are not buttons.
BUTTON_KEYS = {
    "ui.start_wave",
    "ui.next_wave",
    "ui.retry_wave",
    "ui.new_run",
    "ui.new_run_reset",
    "ui.continue_world",
    "ui.continue_run",
    "ui.continue",
    "ui.abort",
    "ui.credits",
    "ui.got_it",
    "ui.diff.easy",
    "ui.diff.normal",
    "ui.diff.hard",
    "ui.settings",
    "ui.settings.close",
    "ui.settings.en",
    "ui.settings.sv",
    "ui.confirm.yes",
    "ui.confirm.no",
    "ui.settings.confirm_new_run",
    "ui.mute",
    "ui.unmute",
    "ui.sfx",
    "ui.music",
}

BUTTON_LIMIT = 28
_FILL = ("12", "120", "9999", "99")
_CALL = re.compile(
    r'Loc\.T(f)?\(\s*"([^"]+)"\s*,\s*"((?:\\.|[^"\\])*)"',
    re.S,
)
_ENTRY = re.compile(r'\{\s*"([^"]+)"\s*,\s*"((?:\\.|[^"\\])*)"')


def _unescape(raw: str) -> str:
    text = raw.replace(r"\n", "\n").replace(r"\"", '"').replace(r"\\", "\\")
    return re.sub(r"\\u([0-9a-fA-F]{4})", lambda match: chr(int(match.group(1), 16)), text)


def _dict_block(source: str, which: int) -> str:
    parts = source.split("private static readonly Dictionary")
    if len(parts) <= which:
        return ""
    return parts[which].split("};")[0]


def _entries(block: str) -> dict[str, list[str]]:
    found: dict[str, list[str]] = {}
    for match in _ENTRY.finditer(block):
        found.setdefault(match.group(1), []).append(_unescape(match.group(2)))
    return found


def _filled(template: str) -> str:
    text = template
    for index, sample in enumerate(_FILL):
        text = text.replace("{" + str(index) + "}", sample)
    return text


def _glyphs(text: str) -> list[str]:
    hits = []
    for char in text:
        if ord(char) in MISSING_CODEPOINTS:
            hits.append(f"U+{ord(char):04X}")
    return hits


def check(root: Path) -> list[str]:
    scripts = root / "Assets" / "Scripts"
    loc_path = scripts / "Core" / "Loc.cs"
    loc_src = loc_path.read_text(encoding="utf-8")
    swedish = _entries(_dict_block(loc_src, 1))
    english = _entries(_dict_block(loc_src, 2))
    errors: list[str] = []

    for table_name, table in (("SV", swedish), ("EN", english)):
        for key, values in table.items():
            if len(values) > 1:
                errors.append(f"duplicate {table_name} key {key}")
            for value in values:
                for glyph in _glyphs(value):
                    errors.append(f"{table_name} {key} contains {glyph}")

    fallbacks: dict[str, set[str]] = {}
    for path in sorted(scripts.rglob("*.cs")):
        text = path.read_text(encoding="utf-8")
        rel = str(path.relative_to(root))
        for match in _CALL.finditer(text):
            key = match.group(2)
            fallback = _unescape(match.group(3))
            if key.endswith("."):
                continue
            if fallback.strip() == "":
                errors.append(f"{rel} {key} has an empty EN fallback")
            fallbacks.setdefault(key, set()).add(fallback)
            for glyph in _glyphs(fallback):
                errors.append(f"{rel} {key} contains {glyph}")
            if key not in swedish:
                errors.append(f"SV loc table missing {key}")

    for key, texts in fallbacks.items():
        if len(texts) > 1:
            shown = " | ".join(sorted(texts))
            errors.append(f"duplicate EN fallback for {key}: {shown}")

    shop = (scripts / "Core" / "ShopCatalog.cs").read_text(encoding="utf-8")
    for upgrade in re.findall(r"UpgradeId\.(\w+)", shop):
        for prefix in ("shop.title.", "shop.desc."):
            key = prefix + upgrade
            if key not in swedish:
                errors.append(f"SV loc table missing {key}")
    for enum_file, enum_name, prefix in (
        (scripts / "Combat" / "EnemyKind.cs", "EnemyKind", "enemy."),
        (scripts / "Player" / "FireMode.cs", "FireMode", "mode."),
    ):
        body = enum_file.read_text(encoding="utf-8").split(f"enum {enum_name}")[1].split("}")[0]
        for name in re.findall(r"^\s*([A-Z][A-Za-z0-9_]*)\s*,?\s*$", body, re.M):
            key = prefix + name
            if key not in swedish:
                errors.append(f"SV loc table missing {key}")

    for key in BUTTON_KEYS:
        en_values = set(fallbacks.get(key, set()))
        en_values.update(english.get(key, []))
        sv_values = swedish.get(key, [])
        if not en_values:
            errors.append(f"button {key} has no EN fallback")
            continue
        if not sv_values:
            errors.append(f"button {key} has no SV text")
            continue
        for label in list(en_values) + sv_values:
            shown = _filled(label).replace("\n", " ")
            if len(shown) > BUTTON_LIMIT:
                errors.append(f"button {key} is {len(shown)} chars: {shown}")

    # Shop buy buttons use the title, not the description.
    for key, values in swedish.items():
        if not key.startswith("shop.title."):
            continue
        for label in values:
            if len(label) > BUTTON_LIMIT:
                errors.append(f"button {key} is {len(label)} chars: {label}")
    for key, texts in fallbacks.items():
        if not key.startswith("shop.title."):
            continue
        for label in texts:
            if len(_filled(label)) > BUTTON_LIMIT:
                errors.append(f"button {key} EN is long: {label}")

    return errors


def main() -> int:
    root = Path(__file__).resolve().parents[1]
    errors = check(root)
    if errors:
        for line in errors:
            print(line)
        print(f"{len(errors)} localisation error(s)")
        return 1
    print("localisation audit passed")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
