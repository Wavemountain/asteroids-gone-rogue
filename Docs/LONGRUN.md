# Long run (Speltest)

DO NOT MERGE. This is a test-only Editor harness. It is not a player build, and none of it has been verified by running Unity in this change. Autopilot does not test human fun.

## Command

From the project folder, Unity 6000.6, with a GPU (do not pass `-nographics`).

```
"C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe" -batchmode -projectPath . -executeMethod AsteroidsGoneRogue.EditorTools.LongRun.Run -logFile - -longRunMinutes 3 -seed 48048
```

`-longRunMinutes` defaults to 30 and accepts 1–720. With `-targetWave` and no `-longRunMinutes`, the budget is 90 minutes. `-seed` calls `Random.InitState` before each wave start and does not make the run a daily. `-resumeAfterWorld` defaults to 1. `-resumeAfterMinutes` defaults to 0. `-longRunOut` is an optional parent; each run still writes `Captures\LongRun\<yyyyMMdd-HHmmss>\`.

`-targetWave 10` ends when 10 waves have been cleared through normal transitions (the wave index advances past 10). That result can be PASS before the clock ends. It is not PASS if a diagnostic flag is set.

`-diagnosticInvulnerable` is a labelled diagnostic. The harness does not write hull, shield, or invulnerability. The verdict is `DIAGNOSTIC` (exit 3), never PASS.

`-startWorld N -startWave M -loadout <preset>` marks the run as skipped start. This branch has no start-at-wave control (PR #163 is not included), so the request is recorded and not applied. The verdict says `skipped start` and is `DIAGNOSTIC`, never PASS. A run without those flags says `natural`.

`-menuProbe` is the only path that calls `StartWave` after game over, and only when continue is absent. The level-10 command does not pass it.

The autopilot flies only through the existing move, aim, and fire axes, plus the utility button while a body is inside its keep distance. It sees objects inside the gameplay camera viewport and uses their observed motion. Perception delay is 220 ms. A new stick is held for at least 200 ms. One ladder picks the stick: an incoming shot (sidestep across its path), then a body inside 6 m (back away and strafe across its approach), then a shooter inside 12 m (standoff), then any other body inside its keep, then approach, and patrol only when the viewport has no body and no incoming shot. Close bodies outrank distant shooters for aim. It does not write transforms, bodies, health, or credits.

Hangar purchases, Continue, and New run Yes/No are Confirm, Cancel, Start, and d-pad nav through `GamepadInput`, the same reads the hangar already uses. A Confirm that should press the focused button also sends the EventSystem submit, because batch mode cannot toggle the Input Manager `Submit` button. The harness does not call `ConfirmFirstStart`, `TryBuy`, `StartWave`, or `AcceptContinue`.

Game over does not offer Continue. `FailRun` sets `PendingContinue = null` and deletes the run. That is the game's rule, not a missing harness wait. Continue is only for a parked save.

## Level 10 command

```
"C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe" -batchmode -projectPath . -executeMethod AsteroidsGoneRogue.EditorTools.LongRun.Run -logFile - -targetWave 10 -seed 48048
```

Diagnostic commands, not balance runs:

```
"C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe" -batchmode -projectPath . -executeMethod AsteroidsGoneRogue.EditorTools.LongRun.Run -logFile - -diagnosticInvulnerable -longRunMinutes 3 -seed 48048
```

```
"C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe" -batchmode -projectPath . -executeMethod AsteroidsGoneRogue.EditorTools.LongRun.Run -logFile - -startWorld 1 -startWave 10 -loadout starter -longRunMinutes 3 -seed 48048
```

```
"C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe" -batchmode -projectPath . -executeMethod AsteroidsGoneRogue.EditorTools.LongRun.Run -logFile - -menuProbe -longRunMinutes 3 -seed 48048
```

## OK line

A successful attach logs exactly one line of this form:

```
LongRun death recorder recording ok attempt=1 isPlaying=True frameCount=<n> sceneLoaded=True
```

`attempt=1` is the first real attach. `frameCount` is at least 10 frames after play mode began. Waiting for play mode, a loaded scene, `GameSession`, and those frames does not spend an attempt. That wait is bounded by `BootLimitSeconds` (45) and faults as `play not ready`. There must be no `budget-exhausted` line after this OK line.

## Abort and fault strings

Each of these is logged as `LongRun FAIL: ...` and copied to `summary.md` `Fault` when the run ends.

| String | Meaning |
| --- | --- |
| `play not ready` | Still waiting for play mode, a loaded scene, `GameSession`, or 10 frames after play began when the 45 s boot limit expires. No attach was attempted. |
| `LongRun recorder diag component null` | Cached recorder reference is missing. Logged once. |
| `LongRun recorder diag destroyed (Unity-null but C# ref alive)` | The cached Unity object was destroyed. |
| `LongRun recorder diag Recording=false` | The recorder exists but `OnEnable` has not marked it recording, even after a disable/enable. |
| `LongRun recorder diag host object changed` | The recorder's game object is not the cached host. |
| `LongRun recorder diag active scene changed` | The active scene name changed. Re-arm does not spend an attempt. |
| `LongRun recorder diag domain reload occurred` | The static generation counter reset while the saved generation did not. Re-arm does not spend an attempt. |
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
| `watchdog 60s` | A menu or hangar made no phase, credit, or focus progress for 60 s. |
| `watchdog no-combat-progress` | Playing for 90 s with no wave change, enemy or asteroid count change, or hull or shield change. Ship movement alone does not count. |
| `hangar input repeated without a phase change` | Confirm did not leave the hangar. `hangar-input.txt` lists the focus path. Start is pressed before this failure. A `credits` event records every credit change and the focused control. |
| `continue offer missing after game over` | The game-over continue step waited out its 20 s timeout. |

A resume field mismatch is also stored in `Fault` and logged as a resume `FAIL`. A save or prefs hash mismatch sets `Fault` from the hash detail. A tick exception stores the exception text.

## Output files

`<run>\summary.md` — verdict, times, world, wave, purchases, deaths, resume, save and prefs result, frametime, native error counts, progression (`natural` or `skipped start`), and `Fault`.

`<run>\summary.json` and `<run>\summary.txt` — the same verdict plus one row per sub-goal: `watchdog`, `level-10`, `life-loss`, `invulnerable`, `skipped-start`, `resume`, `game-over-continue`, `new-run`, `purchases`, `prefs`. Each row is `PASS`, `FAIL`, `INCOMPLETE`, or `DIAGNOSTIC`, and a reason. `DIAGNOSTIC` is never a balance PASS.

`<run>\life-loss.txt` — one line per life loss: nearest threat, distance, whether that threat was in the delayed perception buffer, maneuver, projectile name, and observed speed.

`<run>\log-tail.txt` — last 64 KB of `events.jsonl`, written when the 60 s watchdog trips. Repeated error lines are dropped for 2 s. `events.jsonl` stops growing at 20 MB.

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

Exit `0` PASS, `1` FAIL, `2` INCOMPLETE, `3` DIAGNOSTIC. The log line is `LongRun verdict=PASS|FAIL|INCOMPLETE|DIAGNOSTIC`.

A `-targetWave` run can PASS when that many waves are cleared, the resume and prefs checks pass, and no diagnostic flag is set. The 30 minute clock rule still applies when `-targetWave` is omitted. An early death is INCOMPLETE, never PASS. A diagnostics failure or a watchdog is FAIL. Invulnerable mode and skipped start are DIAGNOSTIC, never PASS.

PASS requires the time budget, wave at least 2, no early game over, a completed resume with matching fields, untouched live save hashes, and a prefs `MATCH`. An early death is INCOMPLETE, never PASS. A diagnostics failure is FAIL.

Frame samples use `realtimeSinceStartup` while the phase is Playing. `Playing seconds` and `Frame sum seconds` are both seconds. Avg, p95, p99, and max are milliseconds. A frames/s line is the batchmode player loop, not display FPS.

None of this has been executed in Unity for this document update.
