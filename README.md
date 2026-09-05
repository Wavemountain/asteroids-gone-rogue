# Asteroids gone rogue

Week 1 playable Unity core: one solid wave loop (hangar → fight → clear or fail → score → hangar shop). Not a ten-system vertical slice.

## Unity version

**Unity 6.6** (`6000.6.0f1`, changeset `f7f8ed4d1e24`)

Built-in render pipeline. Old Input Manager (no Input System package, so first open should not show the Input System dialog).

Hub may offer a newer 6000.6 patch — that is fine.

## Open the project

1. Install **Unity 6.6** (`6000.6.0f1`) via Unity Hub.
2. **Add** this repository folder (the folder that contains `Assets/`, `Packages/`, and `ProjectSettings/`).
3. Open the project and wait for the first import (`Library/` is generated locally and is gitignored).
4. Open `Assets/Scenes/Play.unity` if it is not already loaded (it is in **File → Build Settings**).
5. Press **Play**.

Product name in Player Settings is exactly **Asteroids gone rogue**.

Editor helpers: menu **Asteroids gone rogue → Open Play Scene** / **Validate Week 1 Setup**.

## Controls

| Action | Input |
| --- | --- |
| Thrust / strafe | **WASD** or arrow keys |
| Aim | Mouse (on the play plane) |
| Fire | **Left mouse** or **Space** |
| Cycle fire mode | **Q** or **right mouse** (after buying Spread Bolt / Pierce) |
| Abort wave | **Abort → Hangar** or **Esc** (Playing only; keeps loadout, no clear bonus) |
| Start / next / retry wave | Hangar **Start Wave** / **Next Wave** / **Retry Wave** |
| Buy upgrade | Hangar shop buttons |

## Week 1 loop

`GameSession` / `GameManager` / `WaveManager` states:

1. **Hangar** — title, start button, shop (if you have credits). First session shows a dismissable **First flight** card (WASD / Abort Esc / Q·RMB fire modes / shop / Start Wave; PlayerPrefs). Hangar dressing includes Console, PowerBox, FireExtinguisher, Locker, and a **LaunchSign** landmark on the pad’s camera-front edge.
2. **Playing** — fly the 3D ship, shoot bolt / spread / pierce (if bought), split asteroids. Asteroids **wrap** at the arena edge so waves cannot soft-lock. **Abort → Hangar** leaves the wave without the clear bonus.
3. **Wave Clear** — short **Run summary** card (score / wave / world / credits / upgrades) plus **150 credits**, then shop. After waves 1–3, one short continue line (World 2 / Gunner at 4 / **Buy X before Gunner**). Wave 3 also shows a light **★ Scout Wing** medal with the World 2 unlock. Hangar HUD always shows **Best** score / wave / world. During play the score line compares against Best (`/ Best N` or `NEW BEST`) without a fourth HUD line.
4. **Fail** — ship destroyed; retry the same wave. Bought upgrades stay. Fail uses the same summary card. Hangar HUD keeps **Best** visible.

Wave 1: 4 large asteroids + 1 `Enemy_01`. Waves 2–10 add Scout v5 / Gunner v5 / Drone v4, then Bomber / Sniper / SwarmPod when those FBX are present. After wave 10 the roster plateaus and large asteroids tick +1 per wave (7 → 8… cap 10). After every 5 cleared waves the arena mesh swaps World 1→6 (loop). Same radius and rules.

## Shop

Hangar shop is grouped **HULL / NOSE / ENGINE** | **WEAPONS** | **DEFENSE**. Buy buttons show title + cost (or OWNED / LOCKED). Longer descriptions sit in the status line on hover.

Upgrades persist into the next wave.

| Item | Cost | Effect |
| --- | --- | --- |
| Rapid Fire | 100 | Cannon cooldown 0.38s → 0.16s; swaps **Ship_Engine** → `Ship_Engine_Upgrade01` |
| Shield Cell | 80 | +1 visible shield hit before hull (max 2) |
| Nose Hardpoint | 120 | Swaps **Ship_Nose** → `Ship_Nose_Upgrade01`; faster, 2-damage shots |
| Body Upgrade | 90 | Swaps **Ship_Body** → `Ship_Body_Upgrade01`; +1 hull |
| Nose Upgrade 02 | 150 | Requires Nose Hardpoint; `Ship_Nose_Upgrade02`; 3 damage |
| Engine Upgrade 02 | 140 | Requires Rapid Fire; `Ship_Engine_Upgrade02`; faster gun |
| Spread Bolt | 110 | Second shot mode: 3 lower-damage amber pellets (fat silhouette + SpreadCore). Q / RMB to switch |
| Pierce | 130 | Second shot mode: cyan needle that passes through targets (PierceNeedle). Q / RMB to switch |

Hull is 3 hits. Large asteroids take 2 hits then split into 3 small shards. Small shards and enemies are destroyable. Gunner is 4 HP, Bomber is 5 HP. Fail screen names the enemy kind (`Enemy contact (Scout)`).

## Project layout

```
Assets/Scenes/Play.unity          Play scene (camera, light, EventSystem, GameBootstrap)
Assets/Scripts/Core/              GameSession, GameManager, WaveManager
Assets/Scripts/Player/            Ship fly / aim / shoot / health
Assets/Scripts/Combat/            Asteroid split, enemy seeker
Assets/Scripts/Hangar/            Shop
Assets/Scripts/UI/                Code-built hangar + HUD
Assets/Scripts/Content/           Runtime factory + ArtImport (FBX by path)
Assets/Art/Materials/             Mat_Ship_Hull, Mat_Ship_Accent, Mat_Asteroid, Mat_Enemy, Mat_Arena
Assets/Art/Prefabs/               Named visual templates (Play Mode does not require wiring them)
Assets/Art/Import/                BlenderBot FBX source — see IMPORT.md
Assets/Resources/Art/Import/      Same playable FBX for Resources.Load on Press Play
Assets/Resources/Audio/           CC0 SFX + music (see CREDITS.md)
```

## Audio (CC0)

Exact files and licenses are in **[CREDITS.md](CREDITS.md)**. Mute / SFX / Music controls sit in the top-right of the HUD (PlayerPrefs). First Play: SFX 0.8, music 0.28 (mixed quieter so the bed does not blast); mute starts off.

| Cue | Pack | File |
| --- | --- | --- |
| UI click | Kenney Interface Sounds | `click_002.ogg` |
| Hangar purchase | Kenney Interface Sounds | `confirmation_002.ogg` |
| Abort whoosh | Kenney Interface Sounds | `minimize_005.ogg` |
| Shoot (bolt) | Kenney Sci-Fi Sounds | `laserSmall_000.ogg` |
| Shoot (spread) | Kenney Sci-Fi Sounds | `laserRetro_000.ogg` |
| Shoot (pierce) | Kenney Sci-Fi Sounds | `laserLarge_000.ogg` |
| Enemy bolt | Kenney Sci-Fi Sounds | `laserSmall_001.ogg` |
| Hit | Kenney Sci-Fi Sounds | `impactMetal_003.ogg` (+ `impactMetal_000.ogg` punch layer) |
| SwarmPod / Mid hit | Kenney Sci-Fi Sounds | `impactMetal_001.ogg` (no punch) |
| Asteroid split | Kenney Sci-Fi Sounds | `explosionCrunch_000.ogg` |
| Enemy death | Kenney Sci-Fi Sounds | `explosionCrunch_003.ogg` + `impactMetal_000.ogg` punch |
| SwarmPod / Mid death | Kenney Sci-Fi Sounds | `explosionCrunch_001.ogg` (no punch) |
| Player damage | Kenney Sci-Fi Sounds | `forceField_000.ogg` |
| Arena world swap | Kenney Interface Sounds | `maximize_008.ogg` |
| Wave clear | Kenney Music Jingles | `jingles_PIZZA07.ogg` |
| Arena loop | yd — Space Music: Out There | `OutThere.ogg` (fuller mix, pitch 1.0) |
| Hangar ambience | yd — Spacelife #14 | `spacelifeNo14.ogg` (denser layered bed, pitch 0.94 + 1.02) |

## What is stubbed

- **Meshes** come from `Assets/Art/Import/` FBX on Press Play (`ArtImport` loads by path — no Inspector mesh swap). Primitive fallbacks stay if an FBX is missing.
- **Arena World 2–6** meshes **do spawn** — after every 5 cleared waves the floor swaps `Arena_Blockout` → World2 → … → World6 (then loops). Same radius and rules; not separate campaigns.
- **Hangar only:** `Ship_Complete` v3 (parked bay display). **Not in Play:** `Ship_Complete_Upgrade01`, **`Ship_Body_Upgrade02`** (imported only; shop stops at Body Upgrade 01 + Nose/Engine 02).
- Combat juice: light screen flash + camera shake on hits / explosions. Player death stays quiet.
- **Ship_*** part slots share origin `0,0,0` so Rapid Fire / Nose Hardpoint / Body Upgrade stay a SetActive swap.
- No extra ships, no 30-wave campaign, no extra worlds, no large shop, no polish pass, no multiplayer.
- No Input System / URP / TextMeshPro (avoids extra first-open prompts).

`ContentFactory` builds the live ship / rocks / enemy at runtime so Play Mode does not depend on prefab field wiring.

## Repo checks (no Editor required)

```
python3 Tools/validate_week1_project.py
python3 Tools/test_week1_logic.py
```

`Tools/generate_unity_assets.py` recreates `.meta` GUIDs, stub materials/prefabs, and `Play.unity`. Re-run it only if you intentionally change that generator.

## Merge to `main`

See **[MERGE_CHECKLIST.md](MERGE_CHECKLIST.md)**. Do **not** merge PR #1 until Wagge says yes.

## Success check

Press Play → hangar FBX (crate/terminal/pillar + workbench/kiosk/banner/ammo rack + Console/PowerBox/extinguisher/Locker/LaunchSign + parked `Ship_Complete`). Wave 1 `Enemy_01` → later Scout/Gunner v5 / Drone v4 / Bomber+Sniper v5 / SwarmPod. Gunner/Sniper fire `Projectile_EnemyBolt`. Shop Body + Nose/Engine 02 + Spread/Pierce. Worlds 2–6 swap every 5 clears. Play HUD compares score vs Best; clear/fail show the run summary card (wave 3: Buy X before Gunner + ★ Scout Wing). Pickups, muzzle/explosion VFX, hit flash + light shake.
