# Asteroids gone rogue

Week 1 playable Unity core: one solid wave loop (hangar → fight → clear or fail → score → hangar shop). Not a ten-system vertical slice.

## Unity version

**Unity 2022.3.21f1 LTS** (changeset `bf09ca542b87`)

Built-in render pipeline. Old Input Manager (no Input System package, so first open should not show the Input System dialog).

Hub may offer a newer 2022.3 LTS patch — that is fine.

## Open the project

1. Install **Unity 2022.3 LTS** via Unity Hub.
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
| Start / next / retry wave | Hangar **Start Wave** / **Next Wave** / **Retry Wave** |
| Buy upgrade | Hangar shop buttons |

## Week 1 loop

`GameSession` / `GameManager` / `WaveManager` states:

1. **Hangar** — title, start button, shop (if you have credits).
2. **Playing** — fly the 3D ship, shoot one projectile type, split asteroids, one seeking enemy.
3. **Wave Clear** — score (including clear bonus) and **150 credits**, then shop.
4. **Fail** — ship destroyed; retry the same wave. Bought upgrades stay.

Wave 1: 4 large asteroids + 1 `Enemy_01`. Later waves add a few more large rocks (capped), still one enemy type.

## Shop (2–3 purchases that matter)

Upgrades persist into the next wave.

| Item | Cost | Effect |
| --- | --- | --- |
| Rapid Fire | 100 | Cannon cooldown 0.38s → 0.16s; swaps **Ship_Engine** → `Ship_Engine_Upgrade01` |
| Shield Cell | 80 | +1 visible shield hit before hull (max 2) |
| Nose Hardpoint | 120 | Swaps **Ship_Nose** → `Ship_Nose_Upgrade01`; faster, 2-damage shots |

Hull is 3 hits. Large asteroids take 2 hits then split into 3 small shards. Small shards and the enemy are destroyable.

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

Exact files and licenses are in **[CREDITS.md](CREDITS.md)**. Mute / SFX / Music controls sit in the top-right of the HUD (PlayerPrefs).

| Cue | Pack | File |
| --- | --- | --- |
| Shoot | Kenney Sci-Fi Sounds | `laserSmall_000.ogg` |
| Hit | Kenney Sci-Fi Sounds | `impactMetal_000.ogg` |
| Asteroid split | Kenney Sci-Fi Sounds | `explosionCrunch_000.ogg` |
| Enemy death | Kenney Sci-Fi Sounds | `explosionCrunch_003.ogg` |
| Player damage | Kenney Sci-Fi Sounds | `forceField_000.ogg` |
| Hangar purchase | Kenney Interface Sounds | `confirmation_002.ogg` |
| Wave clear | Kenney Music Jingles | `jingles_PIZZA07.ogg` |
| Arena loop | yd — Space Music: Out There | `OutThere.ogg` |
| Hangar ambience | yd — Spacelife #14 | `spacelifeNo14.ogg` |

## What is stubbed

- **Meshes** come from `Assets/Art/Import/` FBX on Press Play (`ArtImport` loads by path — no Inspector mesh swap). Primitive fallbacks stay if an FBX is missing.
- **Not spawned in Week 1:** `Arena_World2/3_Blockout`, `Ship_Complete*`, `Ship_Body_Upgrade01`, `Pickup_Score` / `Pickup_Shield`. Scout / Gunner / Drone FBX load if spawned; the wave still only creates `Enemy_01`.
- **Ship_*** part slots share origin `0,0,0` so Rapid Fire / Nose Hardpoint stay a SetActive swap.
- No extra ships, no 30-wave campaign, no extra worlds, no large shop, no polish pass, no multiplayer.
- No Input System / URP / TextMeshPro (avoids extra first-open prompts).

`ContentFactory` builds the live ship / rocks / enemy at runtime so Play Mode does not depend on prefab field wiring.

## Repo checks (no Editor required)

```
python3 Tools/validate_week1_project.py
python3 Tools/test_week1_logic.py
```

`Tools/generate_unity_assets.py` recreates `.meta` GUIDs, stub materials/prefabs, and `Play.unity`. Re-run it only if you intentionally change that generator.

## Success check

Press Play → hangar shows the BlenderBot ship + crate/terminal/pillar (Console: `ArtImport: 20/20 Play Mode FBX ready`). Start a wave → fly, shoot, split a large asteroid, kill the chaser → clear (or die) → see score → buy Rapid Fire or Nose Hardpoint → the engine/nose FBX swap → next wave uses it.
