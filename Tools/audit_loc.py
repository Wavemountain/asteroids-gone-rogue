#!/usr/bin/env python3
"""SV/EN completeness for Loc.T / Loc.Tf call sites.

Kenney Future omits U+2699. Button captions stay within 28 characters.
"""

from __future__ import annotations

import re
from pathlib import Path

# Fallback when fontTools cannot read the Kenney cmaps.
MISSING_CODEPOINTS = {0x2699, 0x2605}
_FONT_FILES = (
    "Assets/Resources/Fonts/KenneyFuture.ttf",
    "Assets/Resources/Fonts/KenneyFutureNarrow.ttf",
)
_UI_LITERAL_FILES = (
    "Assets/Scripts/Core/Loc.cs",
    "Assets/Scripts/Core/MedalCatalog.cs",
    "Assets/Scripts/Core/RunSummary.cs",
    "Assets/Scripts/Core/WorldCatalog.cs",
    "Assets/Scripts/Core/AchievementCatalog.cs",
    "Assets/Scripts/UI/GameUi.cs",
    "Assets/Scripts/UI/UiTheme.cs",
)

# Compact button captions. Body copy, hints, and shop descriptions are not buttons.
BUTTON_KEYS = {
    "ui.start_wave",
    "ui.next_wave",
    "ui.retry_wave",
    "ui.new_run",
    "ui.new_run_reset",
    "ui.one_more_try",
    "ui.skip_tutorial",
    "ui.first_start",
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
    "sink.entry",
    "sink.close",
    "ui.mute",
    "ui.unmute",
    "ui.sfx",
    "ui.music",
}

BUTTON_LIMIT = 28
_FILL = ("12", "120", "9999", "99")

# SV values that are allowed to equal the EN value.
# Proper names and loanwords (Hangar, Session, Brute), control labels (D-pad, Analog),
# weapon names kept in both languages (Rail, Pierce, Spread, Storm), and the
# difficulty word Normal. Legacy is listed so a value that is only that word
# stays allowed. Numbers and glyph-only strings are not listed: they have
# fewer than 4 letters and the equality check skips them.
SAME_LANGUAGE_ALLOW = {
    "Normal",
    "Rail",
    "Legacy",
    "Assist",
    "Asteroid",
    "Hangar",
    "Analog",
    "D-pad",
    "Pierce",
    "Spread",
    "Storm",
    "Brute",
    "Session —",
    "Mk II",
    "Session {0}",
    " / Sess {0}",
    "Xbox",
    "PlayStation",
}
_TOKEN = re.compile(r"\{[a-z0-9_]+\}")
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


def _letters(text: str) -> int:
    return sum(1 for char in text if char.isalpha())


def _dynamic_english(root: Path) -> dict[str, str]:
    """EN for keys built at runtime: shop title/desc, enemy kind, fire mode."""
    scripts = root / "Assets" / "Scripts"
    found: dict[str, str] = {}
    shop = (scripts / "Core" / "ShopCatalog.cs").read_text(encoding="utf-8")
    item = re.compile(
        r'UpgradeId\.(\w+)\s*,\s*"((?:\\.|[^"\\])*)"\s*,\s*"((?:\\.|[^"\\])*)"'
    )
    for match in item.finditer(shop):
        found["shop.title." + match.group(1)] = _unescape(match.group(2))
        found["shop.desc." + match.group(1)] = _unescape(match.group(3))

    def enum_names(path: Path, enum_name: str) -> list[str]:
        body = path.read_text(encoding="utf-8").split(f"enum {enum_name}")[1].split("}")[0]
        return re.findall(r"^\s*([A-Z][A-Za-z0-9_]*)\s*,?\s*$", body, re.M)

    for name in enum_names(scripts / "Combat" / "EnemyKind.cs", "EnemyKind"):
        found["enemy." + name] = name
    for name in enum_names(scripts / "Player" / "FireMode.cs", "FireMode"):
        found["mode." + name] = name
    return found


def _font_allowed(root: Path) -> set[int] | None:
    try:
        from fontTools.ttLib import TTFont
    except ImportError:
        return None
    shared: set[int] | None = None
    for relative in _FONT_FILES:
        path = root / relative
        if not path.is_file():
            return None
        font = TTFont(str(path))
        cmap: set[int] = set()
        for table in font["cmap"].tables:
            cmap.update(int(code) for code in table.cmap.keys())
        shared = cmap if shared is None else shared & cmap
    return shared


def _glyphs(text: str, allowed: set[int] | None) -> list[str]:
    hits = []
    for char in text:
        code = ord(char)
        if code < 32:
            continue
        missing = code not in allowed if allowed is not None else code in MISSING_CODEPOINTS
        if missing:
            hits.append(f"U+{code:04X}")
    return hits


def _string_literals(source: str) -> list[str]:
    return [_unescape(match.group(1)) for match in re.finditer(r'"((?:\\.|[^"\\])*)"', source)]


def check(root: Path) -> list[str]:
    scripts = root / "Assets" / "Scripts"
    loc_path = scripts / "Core" / "Loc.cs"
    loc_src = loc_path.read_text(encoding="utf-8")
    swedish = _entries(_dict_block(loc_src, 1))
    english = _entries(_dict_block(loc_src, 2))
    errors: list[str] = []
    allowed = _font_allowed(root)

    for table_name, table in (("SV", swedish), ("EN", english)):
        for key, values in table.items():
            if len(values) > 1:
                errors.append(f"duplicate {table_name} key {key}")
            for value in values:
                for glyph in _glyphs(value, allowed):
                    errors.append(f"{table_name} {key} contains {glyph}")
                if table_name == "SV":
                    lowered = _TOKEN.sub(" ", value).casefold()
                    for word in ("primary", "utility"):
                        if word in lowered:
                            errors.append(f"SV {key} contains English {word}")

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
            for glyph in _glyphs(fallback, allowed):
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

    for relative in _UI_LITERAL_FILES:
        path = root / relative
        if not path.is_file():
            continue
        for literal in _string_literals(path.read_text(encoding="utf-8")):
            for glyph in _glyphs(literal, allowed):
                errors.append(f"{relative} literal contains {glyph}: {literal!r}")

    dynamic = _dynamic_english(root)
    for key, values in swedish.items():
        english_values = set(english.get(key, []))
        english_values.update(fallbacks.get(key, set()))
        if key in dynamic:
            english_values.add(dynamic[key])
        if not english_values:
            continue
        for value in values:
            if value not in english_values:
                continue
            if _letters(value) < 4:
                continue
            if value in SAME_LANGUAGE_ALLOW:
                continue
            errors.append(f"SV {key} matches EN ({value!r}); translate it or add it to SAME_LANGUAGE_ALLOW")

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
