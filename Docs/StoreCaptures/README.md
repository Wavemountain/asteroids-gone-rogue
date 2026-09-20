# Store captures — Atmos 0.44 pass

Placeholders for Steam capsule / screens. **No real PNGs required** if the Editor is unavailable — poses and paths are the contract for Atmos/Blender.

Capsule: **3/4 `Ship_Complete`**, title amber **`#D4A04A`** (anti-epa), dark void. Do not use EPA lime.

## F12 / scripted path

Play Mode (`Assets/Scenes/Play.unity`):

| Key / menu | What |
| --- | --- |
| **F9** | Cycle the five screen poses |
| **F12** | Write PNG to `Docs/StoreCaptures/out/` (Editor) or `persistentDataPath/StoreCaptures/` (player) |
| **Asteroids gone rogue → Store Captures → Pose …** | Apply a named cam hold |
| **Release pose hold** | Restore FollowCamera hangar/play framing |

`StoreCaptureDirector` boots from `GameBootstrap`. `StoreCapturePoses` holds the numbers.

## Shot list → files

Write these names (placeholders live in `placeholders/` until Atmos drops PNG):

| Id | File | Pose (cam → look, FOV) | Frame |
| --- | --- | --- | --- |
| `01_hangar_shop_health_launchsign` | `out/01_hangar_shop_health_launchsign.png` | `(2.4, 12.5, -11.5)` → LaunchSign `(1.95, 0.8, -2.55)` FOV 46 | Hangar shop left, HEALTH rack, LaunchSign GO |
| `02_play_void_astrofloor` | `out/02_play_void_astrofloor.png` | `(0, 38, -26)` → origin FOV 54 | World 1 Open / AstroFloor_v2 void |
| `03_combat_juice_bolt_spread` | `out/03_combat_juice_bolt_spread.png` | `(4.2, 18, -16)` → `(0.4, 0.2, 1.2)` FOV 48 | Bolt + Spread juice, hit spark |
| `04_brute_swarm_beat` | `out/04_brute_swarm_beat.png` | `(-6, 22, -18)` → `(2, 0.4, 4)` FOV 50 | Wave 5 Brute / wave 6 Swarm (pose only if you spawn) |
| `05_fail_or_win` | `out/05_fail_or_win.png` | hangar fail/win card FOV 54 | **SHIP LOST** fail chrome **or** SECTOR CLEAR + HIT07 |
| `capsule_ship_complete_34` | `out/capsule_ship_complete_34.png` | `(-6.4, 3.1, -4.8)` → parked Ship_Complete `(-8.6, 0.55, 8.2)` FOV 32 | Steam capsule 3/4 hero |

## Capsule notes (Atmos)

- Hero: hangar **`Ship_Complete`** v5, three-quarter, slightly above pad.
- Title color **`#D4A04A`** (HUD amber `UiAmber` 0.831, 0.627, 0.29). Body `#C8CED6`. No EPA green.
- Void / AstroFloor, not hangar scrim, if the capsule is “in space”.
- Safe area: leave top ~18% for Steam library title overlay.

## Placeholders

`placeholders/*.txt` stand in until PNG exists. Copy Atmos output into `out/` using the file names above. Do not commit huge binaries unless SpelPM asks.
