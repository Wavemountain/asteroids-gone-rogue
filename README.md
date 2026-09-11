# Asteroids gone rogue

Week 1 playable Unity core: one solid wave loop (hangar → fight → clear or fail → score → hangar shop). Not a ten-system vertical slice.

## Unity version

**Unity 6.6** (`6000.6.0f1`, changeset `f7f8ed4d1e24`)

Built-in render pipeline. Old Input Manager (no Input System package, so first open should not show the Input System dialog). Package manifest is slim — no VR/XR modules — so Hub should open without Continue.

**Hub-open smoke:** Add this folder in Unity Hub → opens without Continue → `Assets/Scenes/Play.unity` → Press Play.

Hub may offer a newer 6000.6 patch — that is fine.

## Open the project

1. Install **Unity 6.6** (`6000.6.0f1`) via Unity Hub.
2. **Add** this repository folder (the folder that contains `Assets/`, `Packages/`, and `ProjectSettings/`).
3. Open the project and wait for the first import (`Library/` is generated locally and is gitignored).
4. Open `Assets/Scenes/Play.unity` if it is not already loaded (it is in **File → Build Settings**).
5. Press **Play**.

Product name in Player Settings is exactly **Asteroids gone rogue**.

HUD / hangar typography: bundled **Kenney Future** (titles, world badge, medals) and **Kenney Future Narrow** (HUD, shop, body). CC0, see CREDITS.md. LegacyRuntime is the Unity 6.6 fallback only.

## Look bible (0.40+)

Visuals stay **modern with a bit of retro arcade**. Audio stays **retro-modern chip/arcade** (AtmosBot Kenney list — clip names do not change). 0.40+ **UI, audio mix, and art wire** aim for **AAA / big-studio polish** even while the chip language stays. Monster and hazard presentation is tight and professional — aura, **charge glow / spawn rings**, spike rings, layout wash/rails — **not prototype-placeholder**.

Editor helpers: menu **Asteroids gone rogue → Open Play Scene** / **Validate Week 1 Setup**.

## Controls

| Action | Input |
| --- | --- |
| Thrust / strafe | **WASD** or arrow keys |
| Aim | Mouse (on the play plane) |
| Fire | **Left mouse** or **Space** |
| Cycle fire mode | **Q** or **right mouse** (after buying Spread / Twin / Pierce / Seeker / Ricochet) |
| Abort wave | **Abort → Hangar** or **Esc** (Playing only; keeps loadout, no clear bonus) |
| Start / next / retry wave | Hangar **Start Wave** / **Next Wave** / **Retry Wave** |
| Buy upgrade | Hangar shop buttons |

## Week 1 loop

`GameSession` / `GameManager` / `WaveManager` states:

1. **Hangar** — title, start button, shop (if you have credits). First session shows a dismissable **First flight** card (WASD / Abort Esc / Q·RMB fire modes / shop / Start Wave / medal ladder → ★ Scout Wing at wave 3; PlayerPrefs). Hangar dressing includes Console, PowerBox, FireExtinguisher, Locker, and a **LaunchSign** landmark (emissive GO plate + mesh GO decal) on the pad’s camera-front edge. The **medal ladder** is labeled **MEDALS** (Scout Wing / Deep Orbit / Far Drift) in hangar **and** play (`★` earned / `○` locked; `agr.hangar.medals`, capacity 3). Hangar status shows the next-medal hook.
2. **Playing** — fly the 3D ship, shoot bolt / spread / twin / pierce / seeker / ricochet (if bought), split asteroids. Asteroids **wrap** at the arena edge so waves cannot soft-lock. **Abort → Hangar** leaves the wave without the clear bonus.
3. **Wave Clear** — short **Run summary** card (score / wave / world / credits / upgrades) plus **150 credits**, then shop. After waves 1–9, one short continue line (★ Scout Wing at 3 / Gunner at 4 / **Buy X before Gunner** / **★ Deep Orbit** teaser before wave 6 / **★ Far Drift** teaser before wave 10). Wave 3 awards **★ Scout Wing**. Entering World 2 (wave 6) is a short **WORLD 2 ONLINE · ★ Deep Orbit** beat **and** the hangar summary announces it. Wave 10 awards **★ Far Drift** (jingle `PIZZA16`, not the wave-clear sting) and points at World 3 at wave 11. World 3 entry (wave 11) is a short **WORLD 3 ONLINE · New sector** beat **without a medal**; hangar summary says **World 3 online · New sector**. Hangar HUD always shows **Best** score / wave / world. During play the score line compares against Best (`/ Best N` or `NEW BEST`) without a fourth HUD line.
4. **Fail** — ship destroyed; retry the same wave. Bought upgrades stay. Fail uses the same summary card. Hangar HUD keeps **Best** visible.

Wave 1: 4 large asteroids + 1 `Enemy_01` (Mid v8 mesh). Waves 2–10 add Scout v7 / Gunner v7 / Drone v6, then Bomber v8 / Sniper v8 / SwarmPod v6 when those FBX are present. Wave 8 adds **Brute** (`Monster_Brute`, close-range charge tank). Wave 9 adds **Swarm** (`Monster_Swarm`, spawner that drops Swarmling minions). Sniper uses the same spawn fallback as Scout/Gunner. After wave 10 the roster plateaus (Brute + Swarm stay) and large asteroids tick +1 per wave (7 → 8… cap 10). After every 5 cleared waves the arena mesh **and layout** swap World 1→7 (Open / Pylon ring / Split trench / Mine belt / Cross gates / Debris islands / Spoke ring). Radius 30 (same wrap / roster rules).

## Shop

Hangar shop is grouped **HULL / NOSE / ENGINE** | **WEAPONS** | **DEFENSE**. Buy buttons show title + cost (or OWNED / LOCKED). Longer descriptions sit in the status line on hover.

Upgrades persist into the next wave.

| Item | Cost | Effect |
| --- | --- | --- |
| Rapid Fire | 100 | Cannon cooldown 0.38s → 0.16s; swaps **Ship_Engine** → `Ship_Engine_Upgrade01` |
| Shield Cell | 80 | +1 visible shield hit before hull (max 2, or 3 with Matrix) |
| Nose Hardpoint | 120 | Swaps **Ship_Nose** → `Ship_Nose_Upgrade01`; faster, 2-damage shots |
| Body Upgrade | 90 | Swaps **Ship_Body** → `Ship_Body_Upgrade01`; +1 hull |
| Hull Plate 02 | 160 | Requires Body Upgrade; +1 hull (5 hits). Reuses Upgrade01 mesh |
| Nose Upgrade 02 | 150 | Requires Nose Hardpoint; `Ship_Nose_Upgrade02`; 3 damage |
| Nose Upgrade 03 | 185 | Requires Nose 02; 4 damage. Reuses Nose 02 mesh |
| Engine Upgrade 02 | 140 | Requires Rapid Fire; `Ship_Engine_Upgrade02`; faster gun |
| Engine Upgrade 03 | 175 | Requires Engine 02; faster cannon. Reuses Engine 02 mesh |
| Overcharger | 210 | Nose branch (+1 dmg, slower gun). Locks Afterburner |
| Afterburner | 210 | Engine branch (fastest gun). Locks Overcharger |
| Spread Bolt | 110 | Shot mode: 3 lower-damage amber pellets (SpreadCore). Q / RMB |
| Pierce | 130 | Shot mode: cyan needle through targets (PierceNeedle). Q / RMB |
| Twin Guns | 125 | Shot mode: two parallel full-damage bolts (not a fan) |
| Seeker | 145 | Shot mode: magenta missile homes on the nearest threat |
| Ricochet | 155 | Shot mode: lime bolt bounces off the arena rim |
| Shield Matrix | 165 | Requires two Shield Cells; shield cap 3 |

Hull is 3 hits. Large asteroids take 2 hits then split into 3 small shards. Small shards and enemies are destroyable. Gunner is 4 HP, Bomber is 5 HP, Brute is 8 HP, Swarm is 6 HP. Fail screen names the enemy kind (`Enemy contact (Scout)` / `Enemy contact (Brute)`) and frames it as **your hull** — credits and upgrades stay on **Retry Wave**. Arena spike contact reads `Arena hazard`. Hangar status teases **Wave 8 Brute** (sidestep the charge) and **Wave 9 Swarm** (break the nest).

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
Assets/Resources/Fonts/           Kenney Future + Future Narrow (CC0)
```

## Audio (CC0)

Exact files and licenses are in **[CREDITS.md](CREDITS.md)**. Mute / SFX / Music controls sit in the top-right of the HUD (PlayerPrefs). First Play: SFX 0.8, music 0.28 (mixed quieter so the bed does not blast); mute starts off.

| Cue | Pack | File |
| --- | --- | --- |
| UI click | Kenney Interface Sounds | `click_002.ogg` (Start / Got it / Mute) |
| Hangar purchase | Kenney Interface Sounds | `confirmation_002.ogg` |
| Abort whoosh | Kenney Interface Sounds | `minimize_005.ogg` |
| Shoot (bolt) | Kenney Sci-Fi Sounds | `laserSmall_000.ogg` |
| Shoot (spread) | Kenney Sci-Fi Sounds | `laserRetro_000.ogg` |
| Shoot (pierce) | Kenney Sci-Fi Sounds | `laserLarge_000.ogg` |
| Shoot (twin) | Kenney Digital Audio | `twoTone1.ogg` |
| Shoot (seeker) | Kenney Digital Audio | `phaserUp2.ogg` |
| Shoot (ricochet) | Kenney Digital Audio | `pepSound1.ogg` |
| Enemy bolt | Kenney Sci-Fi Sounds | `laserSmall_001.ogg` |
| Hit | Kenney Sci-Fi Sounds | `impactMetal_003.ogg` (+ `impactMetal_000.ogg` punch layer) |
| SwarmPod / Mid hit | Kenney Sci-Fi Sounds | `impactMetal_001.ogg` (no punch) |
| Asteroid split | Kenney Sci-Fi Sounds | `explosionCrunch_000.ogg` |
| Enemy death | Kenney Sci-Fi Sounds | `explosionCrunch_003.ogg` + `impactMetal_000.ogg` punch |
| SwarmPod / Mid death | Kenney Sci-Fi Sounds | `explosionCrunch_001.ogg` (no punch) |
| SwarmPod spawn | Kenney Digital Audio | `phaserUp5.ogg` |
| Brute spawn | Kenney Digital Audio | `lowThreeTone.ogg` |
| Brute hit (pool) | Kenney Sci-Fi Sounds | `impactMetal_000`–`002.ogg` |
| Brute death | Kenney Sci-Fi Sounds | `explosionCrunch_003.ogg` + `lowFrequency_explosion_000.ogg` (unique heavy layer) |
| Swarm spawn | Kenney Digital Audio | `phaseJump1.ogg` (+ optional `slime_000.ogg`) |
| Swarm hit (pool) | Kenney Sci-Fi Sounds | `laserSmall_000`–`004.ogg` (pitch ±6%, not player bolt pitch) |
| Swarm death (pool) | Kenney Digital Audio | `zap1.ogg` / `spaceTrash1`–`3.ogg` |
| Spike activate | Kenney Sci-Fi Sounds | `forceField_001.ogg` (hotter + short bed duck) |
| Spike player hit (pool) | Kenney Sci-Fi Sounds | `laserRetro_000`–`002.ogg` |
| Player damage | Kenney Sci-Fi Sounds | `forceField_000.ogg` |
| Arena world swap | Kenney Interface Sounds | `maximize_008.ogg` |
| World 3 entry | Kenney Interface Sounds | `maximize_008.ogg` (hotter + short bed duck) |
| Wave clear | Kenney Music Jingles | `jingles_PIZZA07.ogg` |
| Far Drift award | Kenney Music Jingles | `jingles_PIZZA16.ogg` |
| Arena loop | yd — Space Music: Out There | `OutThere.ogg` (fuller mix, pitch 1.0) |
| Hangar ambience | yd — Spacelife #14 | `spacelifeNo14.ogg` (denser layered bed, pitch 0.94 + 1.02) |
| Credits loop | Kenney Music Loops | `SpaceCadet.ogg` (0.55; duck 0.35s @ 0.4 under NES07) |
| Credits open | Kenney Music Jingles | `jingles_NES07.ogg` |
| Credits close | Kenney Music Jingles | `jingles_NES12.ogg` (PIZZA16 fallback) |

## What is stubbed

- **Meshes** come from `Assets/Art/Import/` FBX on Press Play (`ArtImport` loads by path — no Inspector mesh swap). Primitive fallbacks stay if an FBX is missing.
- **Arena World 2–7** meshes **do spawn** — after every 5 cleared waves the floor swaps `Arena_Blockout` → World2 → … → World6, then World 7 **Spoke ring** reuses the World 1 floor with a new layout (pylons / trench / mine belt / cross / islands / spokes). `Arena_Hazard_Spike` FBX marks hazards. Same wrap / roster rules; floor scales with arena radius 30.
- **Hangar only:** `Ship_Complete` v5 (parked bay display). **Not in Play:** `Ship_Complete_Upgrade01`, **`Ship_Body_Upgrade02`** (imported only, not warmed). Shop **Hull Plate 02** / Nose 03 / Engine 03 reuse the prior meshes.
- Combat juice: light screen flash + camera shake on hits / explosions. Player death stays quiet.
- **Ship_*** part slots share origin `0,0,0` so Rapid Fire / Nose Hardpoint / Body Upgrade stay a SetActive swap.
- No extra ships, no 30-wave campaign, no extra worlds, no multiplayer. Hangar shop is the 0.40 Step B tree (hull tiers + weapon modes + defense branch), not a separate campaign system. Look-bible polish stays on 0.40+ UI / audio mix / art wire (monsters and hazards).
- No Input System / URP / TextMeshPro (avoids extra first-open prompts). HUD uses bundled Kenney CC0 fonts via `Font` (LegacyRuntime fallback only; never Arial).

`ContentFactory` builds the live ship / rocks / enemy at runtime so Play Mode does not depend on prefab field wiring.

## Fonts (CC0)

Kenney Fonts pack (CC0). Files live in `Assets/Resources/Fonts/` so Play Mode loads them without Inspector wiring.

| Use | File |
| --- | --- |
| Display (title, world, medals, headers) | `KenneyFuture.ttf` |
| Body (HUD, shop, status) | `KenneyFutureNarrow.ttf` |

## Repo checks (no Editor required)

```
python3 Tools/validate_week1_project.py
python3 Tools/test_week1_logic.py
```

`Tools/generate_unity_assets.py` recreates `.meta` GUIDs, stub materials/prefabs, and `Play.unity`. Re-run it only if you intentionally change that generator.

## Merge to `main`

See **[MERGE_CHECKLIST.md](MERGE_CHECKLIST.md)**. Do **not** merge PR #1 until Wagge says yes.

## Success check

Press Play → hangar FBX (crate/terminal/pillar + workbench/kiosk/banner/ammo rack + Console/PowerBox/extinguisher/Locker/LaunchSign GO + parked `Ship_Complete` v5). Hangar **Credits** opens the end-credits roll (`SpaceCadet` + NES07/NES12). Wave 1 `Enemy_01` v8 → later Scout/Gunner v7 / Drone v6 / Bomber v8 + Sniper v8 / SwarmPod v6 + wave 8 `Monster_Brute` / wave 9 `Monster_Swarm`. Gunner/Sniper fire `Projectile_EnemyBolt`. Shop Body/Hull 02 + Nose/Engine 03 + Overcharger/Afterburner + Spread/Twin/Pierce/Seeker/Ricochet + Shield Matrix. Worlds 2–7 swap every 5 clears with a new layout (World 2: ★ Deep Orbit + pylon ring; wave 10: ★ Far Drift; World 3: New sector beat + split trench, no medal; World 7: spoke ring). Play HUD compares score vs Best; **MEDALS** ladder stays visible (★ / ○). Clear/fail show the run summary card (wave 3: Buy X before Gunner + ★ Scout Wing; wave 6: ★ Deep Orbit; waves 8–9: Far Drift teaser; wave 10: ★ Far Drift · World 3 at 11; wave 11: World 3 online · New sector). Fail names the cause as your hull; Retry Wave keeps loadout. Kenney Future HUD/hangar type, arena radius 30, Mute click. Pickups, muzzle/explosion VFX, hit flash + light shake.
