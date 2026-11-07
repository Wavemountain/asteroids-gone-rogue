# Long run (Speltest)

Editor-only play of a real run. It does not ship in a player build. It does not skip waves, grant credits, or change balance. Autopilot does not test human fun.

## Command

From the project folder, Unity 6000.6, with a GPU (do not pass `-nographics`):

```
"C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe" -batchmode -projectPath . -executeMethod AsteroidsGoneRogue.EditorTools.LongRun.Run -logFile - -longRunMinutes 30 -seed 48048
```

Flags:

- `-longRunMinutes N` — wall-clock budget. Default 30. Range 1–720.
- `-seed N` — optional. `Random.InitState` immediately before each normal wave start. It does not turn the run into a daily. Omit it to leave the engine stream alone.
- `-resumeAfterWorld N` — save, quit, and resume after this world is cleared. Default 1. `0` turns the world trigger off.
- `-resumeAfterMinutes N` — optional extra trigger, once the run has left wave 1 and is in the hangar or on a wave-clear. Default 0 (off).
- `-longRunOut <dir>` — parent folder. Default `Captures\LongRun`. Each run writes `Captures\LongRun\<yyyyMMdd-HHmmss>\`.

## Where the logs go

`<run>\events.jsonl` — one JSON object per line: transitions (hangar, wave, world, shop, boon, death / continue, game over, save, resume), purchases (`item`, `cost`, `creditsBefore`, `creditsAfter`), credits and lives on every line, exceptions and error logs, and the screenshot path.

`<run>\summary.md` — verdict, elapsed seconds, world, wave, purchases, deaths, resume, save check, frametime (avg, p95, p99, max, hitches over 50 ms).

`<run>\shots\` — PNG at each transition and each error (cap 200).

`<run>\profile\` — the only place run saves are written.

`<run>\backup\` — copies of Wagge's save files taken before play, plus `manifest.txt` and `prefs.txt`.

The Unity log line is `LongRun verdict=PASS|FAIL|INCOMPLETE`.

## PASS, FAIL, INCOMPLETE

Exit code `0` PASS, `1` FAIL, `2` INCOMPLETE.

- **PASS** — the budget elapsed, the run reached at least wave 2, it did not run out of lives early, the save/quit/resume fields matched, and the live save hashes still match the backup.
- **FAIL** — the backup hash check failed, a live save or PlayerPrefs restore did not match, the resume comparison mismatched, or the harness threw.
- **INCOMPLETE** — the run stopped before the time limit, never reached wave 2, ran out of lives first, or never reached the save/quit/resume point. An early death is never PASS.

Resume compares world, wave, credits, lives, score, upgrade mask, Mk II mask, doctrine, boon levels, run id, daily seed, and difficulty. A mismatch line is in `events.jsonl` (`kind` `resume`, `result` `FAIL`) and in the summary.

## Saves must stay untouched

Before play the harness hashes `run_save.json`, `meta_save.json`, and their `.bak` / `.tmp` siblings under `Application.persistentDataPath`, copies them into `<run>\backup\`, and checks the copy hash. It sets `AGR_LONGRUN_PROFILE` to `<run>\profile`. `RunSaveStore` reads that variable only inside `#if UNITY_EDITOR`, so a player build still uses the normal folder, and a normal editor session does not set the variable.

After play it clears the variable and hashes the live files again. `summary.md` says `Save files: untouched` only when every hash matches.

PlayerPrefs are not redirected. There is no single prefs wrapper in the game. Before play the harness reads each watched key. A key is recorded only when exactly one of int, float, or string is unambiguous. Absent keys are stored as missing. The fsync'd `<run>\backup\prefs.json` is one JSON array carrying that full set plus `count` and `checksum`. A missing `]` or `}`, or any trailing garbage, is rejected before prefs are changed or the marker is removed. If any watched key is ambiguous, the run stops before the backup or the marker is written. Restore checks the file first. An empty, truncated, partial, duplicate, missing, invalid, or ambiguous backup writes `RESULT MISMATCH`, does not change prefs, and does not remove `Captures\LongRun\prefs-unrestored.txt`. A complete backup restores each saved type and deletes keys that were missing. `<run>\backup\prefs-verify.txt` then has `MATCH` or `MISMATCH` for every original key. The marker is removed only when every one of those lines is `MATCH`. `LongRun.Run` refuses to start while that marker exists.

## Crash recovery

Do not start another long run until prefs are restored. From the project folder:

```
"C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe" -batchmode -projectPath . -executeMethod AsteroidsGoneRogue.EditorTools.LongRun.RestorePrefs -logFile - -backup Captures\LongRun\<yyyyMMdd-HHmmss>\backup\prefs.json
```

Use the path stored in `Captures\LongRun\prefs-unrestored.txt`. Exit code 0 and `prefs-verify.txt` ending in `RESULT MATCH` means the keys match the backup. Exit code 1 leaves the marker in place.

File saves: if Unity is killed before the summary, do not copy `<run>\profile` back. Compare the live files with `backup\manifest.txt` (tab-separated name and SHA-256, or `ABSENT`). A matching manifest means Wagge's save files were not written.

## Limits

- Aim, fire, and evade are the harness policy. There is no older autopilot type on `a0abecd`. Sticks are written through `GamepadInput` (`Horizontal`, `Vertical`, `AimX`, `AimY`, Fire) because batch mode has no focused window. `InputManager.asset` is unchanged.
- PlayerPrefs stay on the real store during the run. The fsync'd `prefs.json` plus `LongRun.RestorePrefs` put them back. File saves are redirected and not written in the normal folder.
- Shots are the world cameras. Screen-space UI is not composited.
- `-seed` does not enable daily rules. Resume compares the saved fields, not the RNG cursor.
- Screenshot cap is 200. Minutes cap is 720.
- Frame time is `realtimeSinceStartup` while the phase is Playing. The summary lists Playing seconds and the sum of those samples in seconds, and avg/p95/p99/max in ms. Any frames/s line is the batchmode player loop, not display FPS.
