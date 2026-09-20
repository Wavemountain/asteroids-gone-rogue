# Asteroids gone rogue

Week 1 playable Unity core: one solid wave loop (hangar → fight → clear or fail → score → hangar shop). Not a ten-system vertical slice.

## Unity version

**Unity 6.6** (`6000.6.0f1`, changeset `f7f8ed4d1e24`)

Built-in render pipeline. Old Input Manager (no Input System package, so first open should not show the Input System dialog). Xbox pad uses Legacy axes (`Horizontal`/`Vertical`/`PadMoveX`/`PadMoveY`/`PadDpadX`/`PadDpadY`/`DpadUp`/`DpadDown`/`DpadLeft`/`DpadRight`/`AimX`/`AimY`/`FireTrigger`/`FireTrigger3`/`FireTrigger6`/`UtilityTrigger`/`FirePad`/`CycleFire`/`Pause`) in parallel with WASD/mouse. Package manifest is slim — no VR/XR modules — so Hub should open without Continue.

**Hub-open smoke:** Add this folder in Unity Hub → opens without Continue → `Assets/Scenes/Play.unity` → Press Play. Short copy: **[Docs/HUB_SMOKE.md](Docs/HUB_SMOKE.md)**. Store poses / F12: **[Docs/StoreCaptures/README.md](Docs/StoreCaptures/README.md)**.

Hub may offer a newer 6000.6 patch — that is fine.

## Open the project

1. Install **Unity 6.6** (`6000.6.0f1`) via Unity Hub.
2. **Add** this repository folder (the folder that contains `Assets/`, `Packages/`, and `ProjectSettings/`).
3. Open the project and wait for the first import (`Library/` is generated locally and is gitignored).
4. Open `Assets/Scenes/Play.unity` if it is not already loaded (it is in **File → Build Settings**).
5. Press **Play**.

Product name in Player Settings is exactly **Asteroids gone rogue**.

HUD / hangar typography: bundled **Kenney Future** (titles, world badge, medals, 28–46) and **Kenney Future Narrow** (HUD, shop, body, 14–22). CC0, see CREDITS.md. LegacyRuntime is the Unity 6.6 fallback only. Never Arial.

`UiTheme` holds locked Atmos tokens: void `#070B12`, surface `#0E1520` / `#141C28`, primary `#D4A04A`, secondary `#6AA8C8`, accent `#C8CED6`, danger `#B85A28`, disabled `#3A4450`@0.45, focus `#E8C878`. Hangar / title / difficulty / language / shop / pause Abort / fail / wave-clear / HUD plates share that chrome (header + amber rule). Fail uses a danger header; win / wave-clear uses primary; Abort is danger-tint. Shop idle is a secondary outline; owned is a muted check; unaffordable is disabled. Language chips desaturate when idle, secondary-ring when selected, focus-ring when pad-focused.

## Look bible (0.40+)

Visuals stay **modern with a bit of retro arcade**. Audio stays **retro-modern chip/arcade** (AtmosBot Kenney list — clip names do not change). 0.40+ **UI, audio mix, and art wire** aim for **AAA / big-studio polish** even while the chip language stays. Monster and hazard presentation is tight and professional — aura, **charge glow / spawn rings**, spike rings, layout wash/rails — **not prototype-placeholder**.

Editor helpers: menu **Asteroids gone rogue → Open Play Scene** / **Validate Week 1 Setup** / **Store Captures** (F9 cycle / F12 PNG).

## Controls

| Action | Input |
| --- | --- |
| Thrust / strafe | **WASD** / arrows, or Xbox **left stick** |
| Aim | Mouse (on the play plane), or Xbox **right stick**. Pad flight with no RS faces the left-stick vector (shots go forward). |
| Fire primary | **Left mouse** / **Space**, or Xbox **RT** (`FireTrigger` 10th axis / Linux `FireTrigger6` 6th). **A** still fires. |
| Fire utility | Hold **E** / **right mouse**, or Xbox **LT** (`FireTrigger3` 3rd axis / `UtilityTrigger` 9th). Empty slot is a no-op (HUD **empty** / —). |
| Cycle primary | **Q**, or Xbox **LB** / **RB** (`joystick button 4` / `5`; **X** still cycles) among Bolt + owned Spread / Twin / Pierce. Seeker / Ricochet are utility-only. |
| Abort / menu | **Abort → Hangar** / **Esc**, or Xbox **Start** (Playing abort; hangar Start Wave) |
| Hangar UI | Mouse, or pad **left stick** + **D-pad** (Start Wave / Continue, shop rows, Easy/Normal/Hard, LANG flags, Mute, Credits, Got it). **A** confirm / **B** back / **Start** pause (Playing abort; hangar Start Wave). Focus ring is mandatory on pad-selected shop rows. D-pad is **not** aliased onto move. |
| Start / next / retry wave | Hangar **Start Wave** / **Next Wave**. Wave 5 clear is **SECTOR CLEAR** (World 1 cap) → **New Run**. 0 lives → hangar **RETRY · NEW RUN** (full run reset). Session highscore stays on hangar/HUD. |
| Health | Play **HEALTH** rack — real HULL + SHIELD `fillAmount` bars (Kenney Future, EN/SV) |
| Language | Hangar **US flag** → English, **Swedish flag** → Swedish (PlayerPrefs `agr.ui.language`) |
| Difficulty | Hangar **Easy / Normal / Hard** (SV: Easy / Normal / Svår). PlayerPrefs `agr.difficulty`. Scales HP / spawn / damage / credits / lives |
| Lives | Start **3** on all difficulties, cap **5**. Extra-life drops are a **heart** pickup (sparse, timeout). 0 lives = full hangar / start reset. Mid-run respawn while lives remain. HUD + hangar |
| Buy upgrade | Hangar shop buttons. Ship sits on the **right** in a framed RenderTexture viewport (`ShipPreviewFrame` 0.562–0.986, `ShowcaseScale` 1.25, studio FOV 40 / camera y 4.55 z −10, look y 0.08), idle-spins, shows bought parts; hover / pad-focus previews the upgrade |

## Week 1 loop

`GameSession` / `GameManager` / `WaveManager` states:

1. **Hangar** — title, start button, shop (if you have credits). First session shows a short dismissable **First flight** card (LS/WASD fly, RT/LMB shoot, Start Wave (A), Abort Esc/Start, clear a wave for shop; PlayerPrefs — no text wall). Start Wave pulses on first hangar. First wave fades a one-line coach. A **LANG** plate sits left of the audio mixers: click the smaller, quieter **US flag** for English or the **Swedish flag** for Swedish (PlayerPrefs `agr.ui.language`, `en` / `sv`). HUD, shop, hints, credits, fail UI, hangar status, and layout flash all read `Loc` keys. Hangar dressing includes Console, PowerBox, FireExtinguisher, Locker, and a **LaunchSign** landmark (emissive GO plate + mesh GO decal) on the pad’s camera-front edge. Shop sits on the **left** (`HangarPanel` 0.014–0.55); the playable ship is on the **right** in a framed RenderTexture `ShipPreviewFrame` (0.562–0.986 × 0.080–0.708, `ShowcaseScale` 1.25, studio FOV 40 / camera y 4.55 z −10, look y 0.08, idle spin, bought parts + hover ghost). A dedicated overlay canvas (`ShipPreviewCanvas`, sort 80) keeps the amber box visible. The studio parks behind the hangar camera (`StudioZ` −140, layer 8) and Default-layer ship renderers are disabled so the world ship cannot leak through Weapons/Defense. The **medal ladder** is labeled **MEDALS** (Scout Wing / Deep Orbit / Far Drift) in hangar **and** play (`★` earned / `○` locked; `agr.hangar.medals`, capacity 3). Hangar status shows the next-medal hook.
2. **Playing** — fly the 3D ship, dual-fire primary (RT) + utility (LT) with separate cooldowns, split asteroids. Asteroids **wrap** at the arena edge. If rocks/enemies stay out of play, **Abort → Hangar** pulses and the wave auto-aborts (no clear bonus). **Abort → Hangar** (Esc / Start) also leaves the wave without the clear bonus.
3. **Wave Clear** — short **Run summary** card (score / wave / world / credits / upgrades / **This run · Session** / **Best**). New record appends **NEW BEST**. After waves 1–4, one short continue line (★ Scout Wing at 3 / Gunner at 4 / **Buy X before Gunner**). Wave 3 awards **★ Scout Wing**. **Wave 5 (Brute) ends the steam-slice loop** — **SECTOR CLEAR · WORLD 1** + HIT07, not endless World wrap. Hangar HUD always shows **Session** + **Best**. During play the score line compares against Best (`/ Best N` or `NEW BEST`) and **Session**. Achievements toast + hangar ladder: First Clear / No-Hit Wave / Hard Clear / Extra-Life Streak (local unlock; Steamworks API names ready, no SDK).
4. **Fail** — ship destroyed; **RETRY · NEW RUN** from hangar (0 lives). Bought upgrades reset with the run. Fail names the cause as **your hull** (`SHIP LOST · {reason}` + “that was you”), adds an **Almost had it** line when 1–3 threats remain, shows **This run · Session · Best**, and uses a steel/amber fail card. The **HEALTH** rack stays visible. Mid-run death with lives left respawns and flashes LIFE LOST + session score.

Wave 1: 5 large asteroids + 2 `Enemy_01` (Mid v8 mesh). Waves 2–10 add Scout v7 / Gunner v7 / Drone v6, then Bomber v8 / Sniper v8 / SwarmPod v6 when those FBX are present. Wave 5 adds **Brute** (`Monster_Brute`, close-range charge tank). Wave 6 adds **Swarm** (`Monster_Swarm`, teaser nest); full Brute+Swarm mix from wave 8. Sniper uses the same spawn fallback as Scout/Gunner. After wave 10 the roster plateaus (Brute + Swarm stay) and large asteroids tick +1 per wave (7 → 8… cap 10). After every 5 cleared waves the arena mesh **and layout** swap World 1→7 (Open / Pylon ring / Split trench / Mine belt / Cross gates / Debris islands / Spoke ring). Radius 30 (same wrap / roster rules).

## Shop

Hangar shop is grouped **HULL / NOSE / ENGINE** | **WEAPONS** | **DEFENSE**. Buy buttons show title + cost (or OWNED / LOCKED). **A** / click on a weapon equips it to its legal slot (Spread / Twin / Pierce → Primary, Seeker / Ricochet → Utility). Max one equipped primary + one utility; swap in hangar (pause Abort returns here). First utility purchase auto-equips if the slot is empty. Start loadout is **Bolt** primary, **empty** utility. Longer descriptions sit in the status line on hover. Weapons header shows `P Bolt  ·  U —`.

Upgrades persist into the next wave.

| Item | Cost | Effect |
| --- | --- | --- |
| Rapid Fire | 100 | Cannon cooldown 0.38s → 0.16s; swaps **Ship_Engine** → `Ship_Engine_Upgrade01` |
| Shield Cell | 80 | +1 visible shield hit before hull (max 2, or 3 with Matrix) |
| Nose Hardpoint | 120 | Swaps **Ship_Nose** → `Ship_Nose_Upgrade01`; faster, 2-damage shots |
| Body Upgrade | 90 | Swaps **Ship_Body** → `Ship_Body_Upgrade01`; +1 hull |
| Hull Plate 02 | 175 | Requires Body Upgrade; +1 hull (5 hits). Reuses Upgrade01 mesh |
| Nose Upgrade 02 | 150 | Requires Nose Hardpoint; `Ship_Nose_Upgrade02`; 3 damage |
| Nose Upgrade 03 | 200 | Requires Nose 02; 4 damage. Reuses Nose 02 mesh |
| Engine Upgrade 02 | 140 | Requires Rapid Fire; `Ship_Engine_Upgrade02`; faster gun |
| Engine Upgrade 03 | 190 | Requires Engine 02; faster cannon. Reuses Engine 02 mesh |
| Overcharger | 230 | Nose branch (+1 dmg, slower gun). Locks Afterburner |
| Afterburner | 230 | Engine branch (fastest gun). Locks Overcharger |
| Spread Bolt | 110 | **Primary** only. 3 lower-damage amber pellets (SpreadCore). LB / Q cycle. CD ×1.35, pellet `max(1, dmg/3)` |
| Pierce | 155 | **Primary** only. Cyan needle through targets (PierceNeedle). LB / Q cycle |
| Twin Guns | 140 | **Primary** only. Two parallel full-damage bolts (not a fan). CD ×1.1 |
| Seeker | 125 | **Utility** only. Magenta missile; hold LT / E. Own CD `0.38×2.4` (~0.91s), speed 0.58, −1 damage, turn 140. First utility purchase auto-equips if empty |
| Ricochet | 170 | **Utility** only. Lime bolt, **2** rim bounces. Hold LT / E. Own CD `0.38×1.5` (~0.57s) |
| Shield Matrix | 185 | Requires two Shield Cells; shield cap 3 |

Hull is 3 hits. Large asteroids take 2 hits then split into 3 small shards. Small shards and enemies are destroyable. Mid is 4 HP, Scout/Drone are 3 HP, Gunner is 4 HP, Bomber is 5 HP, Brute is 10 HP, Swarm is 6 HP. Damaging arena spikes deal 2. Fail screen names the enemy kind (`Enemy contact (Scout)` / `Enemy contact (Brute)`) and frames it as **your hull**. 0 lives is a full run reset (hangar / start, wave 1, empty loadout) — not Retry Wave. Mid-run death with lives left respawns in-wave. When 1–3 threats remain it adds **Almost had it**. Arena spike contact reads `Arena hazard`. Hangar status teases **Wave 5 Brute** (sidestep the charge) and **Wave 6 Swarm** (break the nest).

## Project layout

```
Assets/Scenes/Play.unity          Play scene (camera, light, EventSystem + StandaloneInputModule, GameBootstrap)
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
| Extra-life pickup | Kenney Digital Audio | `powerUp7.ogg` @ 0.82 (alt `threeTone2` @ 0.78); duck 0.22s @ 0.5 |
| Extra-life timeout | Kenney Digital Audio | `phaserDown3.ogg` @ 0.48 (no duck) |
| Abort whoosh | Kenney Interface Sounds | `minimize_005.ogg` |
| Shoot (bolt) | Kenney Sci-Fi Sounds | `laserSmall_000`–`002` pool ±3% pitch |
| Shoot (spread) | Kenney Sci-Fi Sounds | `laserRetro_000`–`002` pool, scale 1.05 |
| Shoot (pierce) | Kenney Sci-Fi Sounds | `laserLarge_000` scale 1.12 |
| Shoot (twin) | Kenney Sci-Fi + Digital | `laserSmall_001` + `twoTone1` layer @ 0.45 |
| Shoot (seeker) | Kenney Digital Audio | `phaserUp5` @ 0.72 |
| Shoot (ricochet) | Kenney Digital Audio | `zap1` @ 0.88 ±4% pitch |
| Enemy bolt | Kenney Sci-Fi Sounds | `laserSmall_001.ogg` |
| Hit | Kenney Sci-Fi Sounds | `impactMetal_000`–`003` pool ±4% pitch; punch `impactMetal_000` @ 0.55 |
| SwarmPod / Mid hit | Kenney Sci-Fi Sounds | `impactMetal_001.ogg` (no punch) |
| Asteroid split | Kenney Sci-Fi Sounds | `explosionCrunch_000.ogg` |
| Enemy death | Kenney Sci-Fi Sounds | `explosionCrunch_003.ogg` + `impactMetal_000.ogg` punch |
| SwarmPod / Mid death | Kenney Sci-Fi Sounds | `explosionCrunch_001.ogg` ±5% @ 0.9 |
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
| Fail | Kenney Music Loops | `GameOver.ogg` @ 0.78 + `lowDown` @ 0.4 (duck 0.55s @ 0.3; `phaserDown3` fallback only) |
| Retry / fail confirm | Kenney Digital Audio | `twoTone1.ogg` (0.75, no duck) |
| Arena world swap | Kenney Interface Sounds | `maximize_008.ogg` |
| World 3 entry | Kenney Interface Sounds | `maximize_008.ogg` (hotter + short bed duck) |
| Wave clear | Kenney Music Jingles | `jingles_HIT07.ogg` @ 0.88 (`HIT04` alt, `PIZZA07` fallback) |
| Far Drift award | Kenney Music Jingles | `jingles_PIZZA16.ogg` |
| Arena loop | Kenney Music Loops | `MissionPlausible.ogg` (`_arenaLoop`, scale 0.65). Optional `TimeDriving` from wave 8. `OutThere` fallback |
| Hangar ambience | yd — Spacelife #14 | `spacelifeNo14.ogg` (denser layered bed, pitch 0.94 + 1.02) |
| Credits loop | Kenney Music Loops | `SpaceCadet.ogg` (0.55; duck 0.35s @ 0.4 under NES07) |
| Credits open | Kenney Music Jingles | `jingles_NES07.ogg` |
| Credits close | Kenney Music Jingles | `jingles_NES12.ogg` (PIZZA16 fallback) |

## What is stubbed

- **Content-cap (0.44 steam-slice):** World 1 waves 1–5 is the finished loop. Clear wave 5 (Brute) → **SECTOR CLEAR** + HIT07 → **New Run**. World 2–7 layout code stays (not spawned in this slice). Achievements are local unlocks with a Steamworks-ready API surface (no Steam SDK in Packages).
- **Meshes** come from `Assets/Art/Import/` FBX on Press Play (`ArtImport` loads by path — no Inspector mesh swap). Primitive fallbacks stay if an FBX is missing.
- **Arena World 2–7** meshes **do spawn** if you lift the cap — after every 5 cleared waves the floor swaps `Arena_AstroFloor_v2` … This slice stops after World 1.
- **Hangar only:** `Ship_Complete` v5 (parked bay display / store capsule 3/4). **Not in Play:** `Ship_Complete_Upgrade01`, **`Ship_Body_Upgrade02`** (imported only, not warmed). Shop **Hull Plate 02** / Nose 03 / Engine 03 reuse the prior meshes.
- Combat juice: hit spark + kill bloom (`JuiceBurst`) + screen flash + camera shake. Extra-life heart is a must-pick (beacon, pip, timeout pulse). Player death stays quiet on flash/shake; fail plays `GameOver` + `lowDown` (`phaserDown3` fallback).
- **Ship_*** part slots share origin `0,0,0` so Rapid Fire / Nose Hardpoint / Body Upgrade stay a SetActive swap.
- No extra ships, no 30-wave campaign, no multiplayer. Hangar shop is the 0.40 Step B tree (hull tiers + weapon modes + defense branch), not a separate campaign system. Look-bible polish stays on 0.40+ UI / audio mix / art wire (monsters and hazards).
- No Input System / URP / TextMeshPro (avoids extra first-open prompts). HUD uses bundled Kenney CC0 fonts via `Font` (LegacyRuntime fallback only; never Arial).
- Store PNGs: paths in **[Docs/StoreCaptures](Docs/StoreCaptures/README.md)** (placeholders until Atmos). Hub smoke: **[Docs/HUB_SMOKE.md](Docs/HUB_SMOKE.md)**.

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

Press Play → hangar FBX (crate/terminal/pillar + workbench/kiosk/banner/ammo rack + Console/PowerBox/extinguisher/Locker/LaunchSign GO + parked `Ship_Complete` v5). Hangar **Credits** opens the end-credits roll (`SpaceCadet` + NES07/NES12). Wave 1 `Enemy_01` v8 → Scout/Gunner v7 / Drone v6 / wave 5 `Monster_Brute`. **Clear wave 5 = SECTOR CLEAR** (World 1 cap, HIT07). Shop Body/Hull 02 + Nose/Engine 03 + Overcharger/Afterburner + Spread/Twin/Pierce/Seeker/Ricochet + Shield Matrix. Play HUD compares score vs Best **and Session**; **MEDALS** + **ACHIEVEMENTS** ladders stay visible. World 2–7 layout code stays (pylon ring / trench / mines / **New sector** beat **without a medal** / spokes; `ArenaEnv` + `Arena_AstroFloor_v2`). Bomber v8 / Sniper / SwarmPod / `Monster_Swarm` remain in the roster for a later cap lift. Fail names the cause as your hull, shows **Almost had it** + **RETRY · NEW RUN**, and keeps the HEALTH rack. Kenney Future HUD/hangar type, arena radius 30, Mute click. Extra-life heart uses `powerUp7` (only power-up fanfare). F12 store poses in Docs/StoreCaptures.
