# Long run (Speltest)

DO NOT MERGE. This is a test-only Editor harness. It is not a player build, and none of it has been verified by running Unity in this change. Autopilot does not test human fun.

## Command

From the project folder, Unity 6000.6, with a GPU (do not pass `-nographics`).

```
"C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe" -batchmode -projectPath . -executeMethod AsteroidsGoneRogue.EditorTools.LongRun.Run -logFile - -longRunMinutes 3 -seed 48048
```

`-longRunMinutes` defaults to 30 and accepts 1–720. `-seed` calls `Random.InitState` before each wave start and does not make the run a daily. `-resumeAfterWorld` defaults to 1. `-resumeAfterMinutes` defaults to 0. `-longRunOut` is an optional parent; each run still writes `Captures\LongRun\<yyyyMMdd-HHmmss>\`.

## OK line

A successful attach logs exactly one line of this form:

```
LongRun death recorder recording ok attempt=0 isPlaying=True frameCount=<n> sceneLoaded=True
```

`attempt=0` means no failed attach was spent. Waiting for play mode, a loaded scene, and `frameCount > 0` does not spend an attempt. That wait is bounded by `BootLimitSeconds` (45).

## Abort and fault strings

Each of these is logged as `LongRun FAIL: ...` and copied to `summary.md` `Fault` when the run ends.

| String | Meaning |
| --- | --- |
| `play not ready` | Still waiting for play mode, a loaded scene, or `frameCount > 0` when the 45 s boot limit expires. No attach was attempted. |
| `play mode did not start` | Editor play mode never became active within 45 s. |
| `GameSession missing` | Play mode is up, but `GameManager` or `GameSession` is still missing after 45 s. |
| `found=null attempt=<n> isPlaying=<bool> frameCount=<n> sceneLoaded=<bool>` | No host object could be created for the recorder. |
| `found-not-recording attempt=...` | A recorder exists, re-enable did not set `Recording`, or the new component is not recording. |
| `addcomponent-null attempt=...` | `AddComponent<LongRunDeathLog>()` returned null. |
| `addcomponent-exception:<message> attempt=...` | `AddComponent` threw. |
| `budget-exhausted attempt=<n> ...` | Two failed attach attempts were already recorded. `ProbeTries` is reset only at `Run` start. |
| `continue offer missing` | Save/quit/resume reloaded without a continue offer. |
| `diagnostics failed` | A life was lost and no `death-*.txt` was written, or a native error came from this harness. |
| `state file missing` | The harness state file disappeared. Exit 1. |
| `-backup <path> is required` | `RestorePrefs` was started without `-backup`. |
| `unrestored PlayerPrefs marker` | `Captures\LongRun\prefs-unrestored.txt` still exists, so `Run` refuses to start. |
| `PlayerPrefs restore mismatch` | `RestorePrefs` re-read did not match the backup. |
| `a long run is already active` | A second `Run` started while one is in progress. |

A resume field mismatch is also stored in `Fault` and logged as a resume `FAIL`. A save or prefs hash mismatch sets `Fault` from the hash detail. A tick exception stores the exception text.

## Output files

`<run>\summary.md` — verdict, times, world, wave, purchases, deaths, resume, save and prefs result, frametime, native error counts, and `Fault`.

`<run>\events.jsonl` — transitions, purchases, credits, lives, and errors.

`<run>\shots\` — PNG at each transition and error, cap 200. World cameras only.

`<run>\profile\` — the only directory `RunSaveStore` writes during the run.

`<run>\backup\manifest.txt` — SHA-256 of the live save files from before play (`ABSENT` if missing).

`<run>\backup\prefs.json` — full watched key set, including keys that were absent, plus `count` and `checksum`. One JSON array. A missing `]` or `}`, or trailing garbage, is rejected before any prefs write.

`<run>\backup\prefs-verify.txt` — `MATCH` or `MISMATCH` per key, then `RESULT MATCH` or `RESULT MISMATCH`.

`<run>\death-NN.txt` — last 100 physics steps and at most 8 threats, written on each life loss.

`<run>\damage.log` — each damage attempt with `t`, `frame`, `step`, and `wave`.

`<run>\waves.txt` — build summary and a `Random.state` digest that does not draw.

`Captures\LongRun\prefs-unrestored.txt` — crash marker. It holds the absolute path of `prefs.json`. It is removed only when every key is `MATCH`.

## RestorePrefs

Do not start another long run while the marker exists. From the project folder, pass the path inside the marker:

```
"C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe" -batchmode -projectPath . -executeMethod AsteroidsGoneRogue.EditorTools.LongRun.RestorePrefs -logFile - -backup Captures\LongRun\<yyyyMMdd-HHmmss>\backup\prefs.json
```

Exit 0 and `RESULT MATCH` means the keys match the backup and the marker was removed. Exit 1 leaves the marker in place and does not treat the restore as done. Do not copy `<run>\profile` back over Wagge's saves. Compare the live `run_save.json` and `meta_save.json` files with `backup\manifest.txt`.

## Verdict

Exit `0` PASS, `1` FAIL, `2` INCOMPLETE. The log line is `LongRun verdict=PASS|FAIL|INCOMPLETE`.

PASS requires the time budget, wave at least 2, no early game over, a completed resume with matching fields, untouched live save hashes, and a prefs `MATCH`. An early death is INCOMPLETE, never PASS. A diagnostics failure is FAIL.

Frame samples use `realtimeSinceStartup` while the phase is Playing. `Playing seconds` and `Frame sum seconds` are both seconds. Avg, p95, p99, and max are milliseconds. A frames/s line is the batchmode player loop, not display FPS.

None of this has been executed in Unity for this document update.
