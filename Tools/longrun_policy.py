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
    """Type comes from which getter is stable, not from the key name."""
    if not exists:
        return "", ""
    if float_stable and not int_stable:
        return "float", repr(float_value)
    if int_stable and not float_stable:
        return "int", str(int_value)
    if string_stable and not int_stable and not float_stable:
        return "string", string_value
    if float_stable and int_stable:
        integral = float_value == float(round(float_value))
        if integral and int(round(float_value)) == int_value and not _fraction_text(string_value):
            return "int", str(int_value)
        return "float", repr(float_value)
    if string_stable:
        return "string", string_value
    return "string", ""


def _fraction_text(text: str) -> bool:
    return "." in text or "e" in text or "E" in text


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
