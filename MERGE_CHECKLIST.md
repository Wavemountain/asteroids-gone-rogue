# Merge checklist — PR #1 → `main`

**Do not merge until Wagge says yes.**

This is the Week 1–2 playable core for **Asteroids gone rogue**. Branch: `0.40-monsters-arenas` (base `0.39-ui-fonts` / tag `0.39`; do not merge into older version tags; do not rewrite `0.39-ui-fonts`; no 0.41; no tag/Release yet — future tag is `0.40` after Speltest PASS + SpelPM).

## Unity

- **Unity 6.6** (`6000.6.0f1`, changeset `f7f8ed4d1e24`)
- Built-in RP, old Input Manager (no Input System / URP / TMP)
- Product name must stay exactly `Asteroids gone rogue`
- `Packages/manifest.json` is slim (ugui + IDE + used builtins only). No `modules.vr` / `modules.xr` (Hub must open without Continue). No `com.unity.textmeshpro`.
- Keep Unity 6.6 APIs: `GetEntityId` (not `GetInstanceID`); bundled Kenney Future / Future Narrow via `Font` (LegacyRuntime fallback only; never Arial).
- **Hub-open smoke:** Add this folder in Unity Hub → project opens without a Continue / VR-XR dialog → `Assets/Scenes/Play.unity` → Press Play.

## After `git pull`

1. **Hub-open smoke:** add the repo folder in Unity Hub (the folder with `Assets/`, `Packages/`, `ProjectSettings/`). Confirm it opens without Continue (no VR/XR modules).
2. Wait for FBX import (`Library/` is local / gitignored).
3. Open `Assets/Scenes/Play.unity` (also in File → Build Settings).
4. Press **Play**. Do not assign meshes in the Inspector.

## Play verify

**ArtImport: 54/54 Play Mode FBX ready** — `ArtImport.PlayModeAssets` has 54 names; all 54 exist under `Assets/Resources/Art/Import/` and `Assets/Art/Import/`. `Ship_Body_Upgrade02` is imported only (not warmed, not in the shop). 0.40 wires `Monster_Brute` / `Monster_Swarm` / `Arena_Hazard_Spike` (real binaries from `0.40-fbx-seed`, not LFS). 0.37 art-wire stays (Scout v7 / Gunner v7 / Drone v6 / Sniper v8). `Ship_Complete` v4, `Enemy_SwarmPod` v6, `Enemy_Bomber` v6, and Mid/`Enemy_01` v8 stay. Buffer_* names are aliases only; primary FBX names resolve first. Resources FBX must be real binaries, not LFS pointer text.

- Hangar shows BlenderBot ship + crate / terminal / pillar / workbench / kiosk / banner / ammo rack + **Console / PowerBox / FireExtinguisher / Locker / LaunchSign** + parked **Ship_Complete** v4. LaunchSign sits on the pad’s camera-front edge as the Start Wave landmark, with an emissive **GO** plate / glow pulse plus a mesh **GO** decal (Kenney Future TextMesh, plus mesh letters if TextMesh flickers).
- First hangar (wave 1, first session): left **First flight** card — WASD / aim / shoot, **Abort (Esc)**, **Q / RMB** fire modes (discover Spread / Pierce when owned), “Clear a wave to earn credits and upgrades.”, **Medal ladder (top-left): ★ Scout Wing at wave 3**, shop + Start Wave. **Got it** or Start Wave dismisses it (PlayerPrefs; no spam later). Hangar status line before Start Wave mirrors Abort / Q and the next-medal hook.
- Top-right badge reads **WORLD 1 · OPEN** on wave 1. After 5 clears it becomes **WORLD 2 · PYLONS**, then TRENCH / MINES / CROSS / ISLANDS, then loops. Each world is a floor swap **plus** a layout (pylons, split walls, damaging spike belt, cross gates, debris islands). Arena-swap flashes **WORLD N ONLINE** plus the layout name and plays `maximize_008` (not the hangar purchase cue). First World 2 entry (wave 6) is a short **★ Deep Orbit** beat (world flash + hangar summary). Wave 10 awards **★ Far Drift** (unique `PIZZA16` jingle) and the hangar line points at **World 3 at wave 11**. World 3 entry (wave 11) is a short **New sector** beat (cool WORLD 3 ONLINE flash + hangar “World 3 online · New sector”) — **no medal**. Medal ladder is labeled **MEDALS** (★ earned / ○ locked) in hangar and early play.
- Shop is grouped Hull/Nose/Engine | Weapons | Defense below Start / credits (no overlap). Buttons show title + cost; hover writes the long description on the status line. Owned / locked / too-poor plates are **distinct greys** (teal OWNED, charcoal LOCKED).
- Hangar HUD (upper left) always shows **Best score · Wave · World** with the live wave / score / hull line. During play the score line compares against Best (`/ Best N` or `NEW BEST`); audio sliders hide so they do not clash with the World badge. `IsBetter` can tie-break on world after score and wave. HUD/hangar use bundled **Kenney Future** (titles / world / medals) and **Kenney Future Narrow** (body / shop / HUD), with a dark HUD plate + outline for contrast. LegacyRuntime is fallback only.
- Arena radius is **30** (was 22). Arena FBX visuals scale to match; spawn rings scale; camera pulls back (`Offset` 0,35,-22 · FOV 54 · far 280). Same wrap / roster / world-swap rules — first world-scale pass, not a redesign.
- Wave clear / fail show a short **Run summary** card: score / wave / world / credits (+ awarded) / upgrades. New record appends **NEW BEST**. After waves 1–9, one short continue line (★ Scout Wing at 3 / Gunner at 4 / **Buy X before Gunner** / **★ Deep Orbit** teaser before wave 6 / **★ Far Drift** teaser before wave 10). Wave 3 awards **★ Scout Wing**; wave 6 hangar summary also announces **★ Deep Orbit**; wave 10 awards **★ Far Drift** · World 3 at 11; wave 11 hangar announces **World 3 online · New sector** (no ★). Medals write to hangar persist / the ladder — no text wall.
- First Play audio: SFX audible (0.8), music not blasting (0.28). Hangar bed is denser (layered spacelife, clearer pitch); arena bed is fuller. Abort ducks the bed under the whoosh. Hits layer `impactMetal_000` under `impactMetal_003`. Mute / sliders persist in PlayerPrefs.
- Distinct SFX: UI click (`click_002`, not purchase) on Start / Got it **and Mute**, shop buy (`confirmation_002`), abort whoosh (`minimize_005`), spread (`laserRetro_000`) vs pierce (`laserLarge_000`) vs bolt (`laserSmall_000`). Enemy death is `explosionCrunch_003` + metal punch; asteroid split stays `explosionCrunch_000` (no punch). SwarmPod / Mid keep `explosionCrunch_001` death and `impactMetal_001` hit (no punch). SwarmPod spawn plays `phaserUp5` (0.62s gap / 0.86 scale + deeper bed duck so it cuts clutter). Wave 10 Far Drift plays `jingles_PIZZA16` instead of the wave-clear sting. World 3 entry reuses `maximize_008` hotter with a short bed duck (not a new jingle). 0.40 requires distinct monster banks (not UI clicks): Brute spawn `lowFrequency_explosion_000` / hit `impactMetal_002` / death `explosionCrunch_002`; Swarm spawn `phaserUp2` / hit `forceField_001` / death `phaserDown3`. Placeholders until AtmosBot remasters — no full ear-pass. Abort-duck and hit-punch stay from 0.34. Other 0.38 audio is unchanged.
- Hit flash + light camera shake on damage / explosion. Player death stays quiet (no extra flash/shake).
- Spread bolts are amber / fat / short-trail + SpreadCore; pierce bolts are cyan / long / thin + PierceNeedle. Gunner and Sniper fire red `Projectile_EnemyBolt`.
- Wave 1: `Enemy_01` v8 Mid mesh. Later: Scout v7 / Gunner v7 / Drone v6. Waves 7+: Bomber v6 / Sniper v8 / SwarmPod v6 if those FBX imported. Wave 8+ adds **Brute** (`Monster_Brute` charge tank). Wave 9+ adds **Swarm** (`Monster_Swarm` spawner → Swarmling minions). Sniper spawn-fallback matches Scout/Gunner. After wave 10: +1 large asteroid per wave (cap 10). Fail text `Enemy contact (Kind)` including Brute / Swarm. Arena hazard contact reads **Arena hazard**.
- 0.3 loop: asteroids wrap at `ArenaRadius`; stranded threats outside `radius+2` for >3s force-wrap or despawn. Playing HUD has **Abort → Hangar** (Esc). Shop adds Spread Bolt + Pierce (Q / RMB cycle). Gunner 4 HP, Bomber 5 HP. Fail text `Enemy contact (Kind)`.
- Buy Body Upgrade → `Ship_Body_Upgrade01`. Nose/Engine Upgrade 02 after their prereqs.
- Shoot → muzzle flash. Kill → explosion. Asteroids mix A/B/C/D visuals; split rules unchanged.
- Console on Play: `ArtImport: 54/54 Play Mode FBX ready`. Resources FBX must be real binaries, not LFS pointer text. `Monster_Brute` / `Monster_Swarm` / `Arena_Hazard_Spike` are wired (placeholder meshes only if an FBX is missing).

Repo checks (no Editor):

```
python3 Tools/validate_week1_project.py
python3 Tools/test_week1_logic.py
```

## Known stubs / out of scope

- Primitive fallbacks if an FBX is missing
- `Ship_Complete_Upgrade01`, Body Upgrade 02, extra pickup gameplay beyond Score/Shield/Health/RapidFire
- `Ship_Complete` is hangar dressing only (not the playable part-slot ship)
- World 2–6 are **mesh swaps + layout/hazard sets** (same wrap / roster rules; arena radius 30, visuals scaled from the 22-unit design) — not new campaigns
- No extra ships, no 30-wave campaign, no multiplayer, no Input System / URP / TMP (Kenney CC0 fonts via `Font`)

## Do not merge until

- [ ] Wagge playtested and said **yes**
- [ ] SpelPM is fine with the Week 2 + arena-world scope on `main`
- [ ] Product name is still exactly **Asteroids gone rogue**
