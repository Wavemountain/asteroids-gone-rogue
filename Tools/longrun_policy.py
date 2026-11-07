"""Long-run harness policy. Unity-free.

Mirrors Assets/Editor/LongRun/LongRunVerdict.cs and the hash rules in
Assets/Editor/LongRun/LongRunSaveGuard.cs. Box tests call this module.
"""

from __future__ import annotations

import hashlib
from pathlib import Path

ABSENT = "ABSENT"
MIN_WAVE_INDEX_FOR_PASS = 2
DEFAULT_MINUTES = 30
HITCH_SECONDS = 0.05
ENV_PROFILE = "AGR_LONGRUN_PROFILE"
SAVE_NAMES = (
    "run_save.json",
    "meta_save.json",
    "run_save.json.bak",
    "meta_save.json.bak",
    "run_save.json.tmp",
    "meta_save.json.tmp",
)


def classify(
    save_failed: bool,
    resume_failed: bool,
    harness_fault: bool,
    ran_full_time: bool,
    progressed: bool,
    died_early: bool,
    resume_not_run: bool = False,
) -> str:
    """FAIL beats everything. A short, stuck, early-dead, or unresumed run is never PASS."""
    if save_failed or resume_failed or harness_fault:
        return "FAIL"
    if (not ran_full_time) or (not progressed) or died_early or resume_not_run:
        return "INCOMPLETE"
    return "PASS"


def progressed(max_wave_index: int) -> bool:
    return max_wave_index >= MIN_WAVE_INDEX_FOR_PASS


def profile_isolated(live_dir: str, profile_dir: str) -> bool:
    if not live_dir or not profile_dir:
        return False
    live = _norm(live_dir)
    profile = _norm(profile_dir)
    if live == profile:
        return False
    if profile.startswith(live + "/") or profile.startswith(live + "\\"):
        return False
    if live.startswith(profile + "/") or live.startswith(profile + "\\"):
        return False
    return True


def hash_file(path: str) -> str:
    file_path = Path(path)
    if not file_path.is_file():
        return ABSENT
    digest = hashlib.sha256(file_path.read_bytes()).hexdigest()
    return digest


def backup_verified(source_hash: str, backup_hash: str) -> bool:
    if not source_hash or not backup_hash:
        return False
    return source_hash.lower() == backup_hash.lower()


def copy_and_verify(source: str, backup: str) -> str:
    """Copy one save file, or record ABSENT. Raise if the backup hash differs."""
    source_hash = hash_file(source)
    backup_path = Path(backup)
    if source_hash == ABSENT:
        if backup_path.exists():
            backup_path.unlink()
    else:
        backup_path.parent.mkdir(parents=True, exist_ok=True)
        backup_path.write_bytes(Path(source).read_bytes())
    backup_hash = hash_file(backup)
    if not backup_verified(source_hash, backup_hash):
        raise RuntimeError("backup hash mismatch for " + source)
    return source_hash


def live_untouched(live_path: str, original_hash: str) -> bool:
    return backup_verified(original_hash, hash_file(live_path))


def _norm(path: str) -> str:
    text = str(Path(path))
    while text.endswith("/") or text.endswith("\\"):
        text = text[:-1]
    return text


def classify_pref(
    exists: bool,
    int_stable: bool,
    int_value: int,
    float_stable: bool,
    float_value: float,
    string_stable: bool,
    string_value: str,
) -> tuple[str, str]:
    """Exactly one stable getter. Anything else is ambiguous and must not be stored."""
    if not exists:
        return "", ""
    stable = int(int_stable) + int(float_stable) + int(string_stable)
    if stable != 1:
        return "ambiguous", ""
    if int_stable:
        return "int", str(int_value)
    if float_stable:
        return "float", repr(float_value)
    return "string", string_value


def prefs_checksum(entries: list[tuple[str, bool, str, str]]) -> str:
    lines = [
        f"{name}\t{1 if exists else 0}\t{kind}\t{value}"
        for name, exists, kind, value in entries
    ]
    blob = "\n".join(lines)
    if lines:
        blob += "\n"
    return hashlib.sha256(blob.encode("utf-8")).hexdigest()


def render_prefs(entries: list[tuple[str, bool, str, str]]) -> str:
    header = '{"count":%d,"checksum":"%s"}' % (len(entries), prefs_checksum(entries))
    lines = [header]
    for name, exists, kind, value in entries:
        lines.append(
            '{"name":"%s","exists":%s,"type":"%s","value":"%s"}'
            % (name, "true" if exists else "false", kind, value)
        )
    return "\n".join(lines) + "\n"


def reject_prefs_backup(text: str, names: list[str]) -> str:
    """Empty string means the backup is complete. Any other code must not restore."""
    if text is None or text.strip() == "":
        return "empty"
    saved: list[tuple[str, bool, str, str]] = []
    saw_header = False
    declared = -1
    declared_sum = ""
    for raw in text.split("\n"):
        line = raw.strip().rstrip(",")
        if line == "" or line in ("[", "]"):
            continue
        if not line.startswith("{") or not line.endswith("}"):
            return "truncated"
        if '"name"' not in line and '"count"' in line:
            if saw_header:
                return "truncated"
            count_match = __import__("re").search(r'"count":(\d+)', line)
            sum_match = __import__("re").search(r'"checksum":"([0-9a-fA-F]+)"', line)
            if count_match is None or sum_match is None:
                return "truncated"
            saw_header = True
            declared = int(count_match.group(1))
            declared_sum = sum_match.group(1)
            continue
        parsed = _parse_pref_line(line)
        if parsed is None:
            return "truncated"
        saved.append(parsed)
    if not saw_header or declared_sum == "":
        return "empty"
    seen: list[str] = []
    for name, _exists, _kind, _value in saved:
        if name in seen:
            return "duplicated"
        seen.append(name)
    for name in names:
        if name not in seen:
            return "missing key"
    if declared != len(names) or len(saved) != len(names):
        return "count"
    for name, exists, kind, value in saved:
        error = _pref_type_error(exists, kind, value)
        if error:
            return error
    by_name = {name: (name, exists, kind, value) for name, exists, kind, value in saved}
    ordered = [by_name[name] for name in names]
    if prefs_checksum(ordered).lower() != declared_sum.lower():
        return "checksum"
    return ""


def prefs_restore_plan(reason: str) -> dict[str, bool]:
    if reason:
        return {"mutate": False, "match": False, "clear_marker": False}
    return {"mutate": True, "match": True, "clear_marker": True}


def _parse_pref_line(line: str) -> tuple[str, bool, str, str] | None:
    if '"exists":true' in line and '"exists":false' in line:
        return None
    if '"exists":true' not in line and '"exists":false' not in line:
        return None
    name = _json_field(line, "name")
    kind = _json_field(line, "type")
    value = _json_field(line, "value")
    if name is None or kind is None or value is None or name == "":
        return None
    return name, '"exists":true' in line, kind, value


def _json_field(line: str, key: str) -> str | None:
    token = f'"{key}":"'
    start = line.find(token)
    if start < 0:
        return None
    cursor = start + len(token)
    chars: list[str] = []
    while cursor < len(line):
        ch = line[cursor]
        if ch == '"':
            return "".join(chars)
        if ch == "\\" and cursor + 1 < len(line):
            nxt = line[cursor + 1]
            chars.append({"n": "\n", "r": "\r"}.get(nxt, nxt))
            cursor += 2
            continue
        chars.append(ch)
        cursor += 1
    return None


def _pref_type_error(exists: bool, kind: str, value: str) -> str:
    if not exists:
        if kind or value:
            return "invalid type"
        return ""
    if kind in ("", "ambiguous"):
        return "ambiguous"
    if kind == "int":
        if value.startswith("+") or (len(value) > 1 and value.startswith("0")) or (value.startswith("-0")):
            return "invalid type"
        try:
            parsed = int(value, 10)
        except ValueError:
            return "invalid type"
        if str(parsed) != value:
            return "invalid type"
        return ""
    if kind == "float":
        if value == "" or value != value.strip():
            return "invalid type"
        try:
            float(value)
        except ValueError:
            return "invalid type"
        return ""
    if kind == "string":
        return ""
    return "invalid type"


def restore_action(exists: bool, kind: str) -> str:
    if not exists:
        return "delete"
    return "set-" + kind


def verify_pref(
    exists: bool,
    kind: str,
    value: str,
    now_exists: bool,
    now_kind: str,
    now_value: str,
) -> str:
    if not exists:
        return "MATCH" if not now_exists else "MISMATCH"
    if now_exists and now_kind == kind and now_value == value:
        return "MATCH"
    return "MISMATCH"


def prefs_run_allowed(marker_exists: bool) -> bool:
    return not marker_exists
