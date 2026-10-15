# SpeltestBot side-by-side captures

Editor-only batch tool: `Assets/Editor/AgrCapture.cs` (`AsteroidsGoneRogue.EditorTools.AgrCapture.Run`). It is not in the player build. One run writes six PNGs at **1920×1080** for a single Reduce effects setting. The same file compiles on 0.47 (`7a64598`) and 0.48: missing pieces (post stack, death slow-mo) are logged and skipped.

Do **not** pass `-nographics`. The post stack needs a GPU.

Temporary settings do not stick. Before anything else, PlayerPrefs keys and `run_save.json` / `meta_save.json` (plus the rest of `persistentDataPath`, including `.bak` / `.tmp`) are copied to a timestamped folder under the system temp directory. The log line `AgrCapture backup <path>` is that folder. Restore runs on the normal exit, on play-mode exit, on timeout, and if startup throws. A finished run logs `AgrCapture restore OK <path>` and leaves the folder in place so it can be copied back by hand. Difficulty is forced to Normal for the run, then restored. The spawn seed is the `-seed` argument (not the UTC daily seed).

Each phase is logged (`enter play mode`, `scene loaded`, `GameSession missing/found` with the frame count, each shot). If play mode never starts, or the session is still missing after about 15 seconds, the run fails with `AgrCapture FAIL:` instead of waiting out the 300 second backstop. Play mode is entered with `EditorApplication.EnterPlaymode`. Unity 6000.6 reloads the scene and keeps the scripting domain unless Enter Play Mode Options are enabled; the log line `playmode optionsEnabled=` records which one this editor used.

## Windows (HEVONEN)

Reduce effects off:

```
"C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe" -batchmode -projectPath . -executeMethod AsteroidsGoneRogue.EditorTools.AgrCapture.Run -logFile - -captureOut Captures\compare -reduce 0 -seed 48048
```

Reduce effects on (same seed):

```
"C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe" -batchmode -projectPath . -executeMethod AsteroidsGoneRogue.EditorTools.AgrCapture.Run -logFile - -captureOut Captures\compare -reduce 1 -seed 48048
```

Run the command from the project folder (the one that contains `Assets/`). Exit code **0** and a log line `AgrCapture OK` means all six files were written. Any failure logs `AgrCapture FAIL: …` and exits **non-zero**. A missing game session fails in seconds. 300 seconds is only the last backstop.

## Expected files

Under `-captureOut` (here `Captures\compare`):

| File | Frame |
| --- | --- |
| `hangar_reduceoff.png` / `hangar_reduceon.png` | Hangar, camera `(0, 31, -19)` looking at `(0.25, 0.4, 0)`, FOV 54 |
| `w1_reduceoff.png` / `w1_reduceon.png` | World 1 wave 1 |
| `w3_reduceoff.png` / `w3_reduceon.png` | World 3 Far Drift, wave 11 |
| `w4_reduceoff.png` / `w4_reduceon.png` | World 4 Mine Fields, wave 16 |
| `death_reduceoff.png` / `death_reduceon.png` | Wave 1, inside the death sequence: slow-mo clock at half of 0.60 s (timescale 0.35, or 1.00 when Reduce skips slow-mo), vignette clock at the full 0.40 s (desat complete, drawn vignette 0.78 / 0.39), white-to-Danger flash painted at peak alpha. The log prints timescale, vignette, saturation, and flash |
| `waveclear_reduceoff.png` / `waveclear_reduceon.png` | Wave 1, mid wave-clear: shock ring, bloom bump, `WAVE CLEAR` / `VÅG KLAR` |

Combat shots use camera `(0, 35, -22)` looking at the origin, FOV 54, ship held at the origin. Each shot waits 60 fixed frames (`Time.captureDeltaTime` 1/60). `Captures/` is gitignored.

On a build without `PostStack` or the death timeline, that shot is still written and the log says what was skipped. With Reduce effects on, slow-mo stays off and the vignette, desaturation, and flash use the reduced amounts. The vignette clock is still the full 0.40 s.
