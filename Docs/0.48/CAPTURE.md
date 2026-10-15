# SpeltestBot side-by-side captures

Editor-only batch tool: `Assets/Editor/AgrCapture.cs` (`AsteroidsGoneRogue.EditorTools.AgrCapture.Run`). It is not in the player build. One run writes five PNGs at **1920×1080** for a single Reduce effects setting. The same file compiles on 0.47 (`7a64598`) and 0.48: missing pieces (post stack, death slow-mo) are logged and skipped.

Do **not** pass `-nographics`. The post stack needs a GPU.

Temporary settings do not stick. PlayerPrefs keys and `run_save.json` / `meta_save.json` (plus `.bak` / `.tmp`) are copied aside before play and restored before exit. Difficulty is forced to Normal for the run, then restored. The spawn seed is the `-seed` argument (not the UTC daily seed).

## Windows (HEVONEN)

Reduce effects off:

```
"C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe" -batchmode -projectPath . -executeMethod AsteroidsGoneRogue.EditorTools.AgrCapture.Run -logFile - -captureOut Captures\compare -reduce 0 -seed 48048
```

Reduce effects on (same seed):

```
"C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe" -batchmode -projectPath . -executeMethod AsteroidsGoneRogue.EditorTools.AgrCapture.Run -logFile - -captureOut Captures\compare -reduce 1 -seed 48048
```

Run the command from the project folder (the one that contains `Assets/`). Exit code **0** and a log line `AgrCapture OK` means all five files were written. Any failure logs `AgrCapture FAIL: …` and exits **non-zero**. The run gives up after 300 seconds.

## Expected files

Under `-captureOut` (here `Captures\compare`):

| File | Frame |
| --- | --- |
| `hangar_reduceoff.png` / `hangar_reduceon.png` | Hangar, camera `(0, 31, -19)` looking at `(0.25, 0.4, 0)`, FOV 54 |
| `w1_reduceoff.png` / `w1_reduceon.png` | World 1 wave 1 |
| `w3_reduceoff.png` / `w3_reduceon.png` | World 3 Far Drift, wave 11 |
| `w4_reduceoff.png` / `w4_reduceon.png` | World 4 Mine Fields, wave 16 |
| `death_reduceoff.png` / `death_reduceon.png` | Wave 1, mid death slow-mo (half of 0.40 s) with the death vignette |

Combat shots use camera `(0, 35, -22)` looking at the origin, FOV 54, ship held at the origin. Each shot waits 60 fixed frames (`Time.captureDeltaTime` 1/60). `Captures/` is gitignored.

On a build without `PostStack` or the death timeline, that shot is still written and the log says what was skipped. With Reduce effects on, slow-mo stays off and the vignette uses the reduced grade; the clock is still pinned at the same mid time.
