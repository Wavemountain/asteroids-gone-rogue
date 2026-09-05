# Merge checklist — PR #1 → `main`

**Do not merge until Wagge says yes.**

This is the Week 1–2 playable core for **Asteroids gone rogue**. Branch: `0.341-scout-drone-polish` (do not merge into older version tags; do not rewrite `0.34-enemies-launch`; no 0.35).

## Unity

- **Unity 6.6** (`6000.6.0f1`, changeset `f7f8ed4d1e24`)
- Built-in RP, old Input Manager (no Input System / URP / TMP)
- Product name must stay exactly `Asteroids gone rogue`

## After `git pull`

1. Open the repo folder in Unity Hub (the folder with `Assets/`, `Packages/`, `ProjectSettings/`).
2. Wait for FBX import (`Library/` is local / gitignored).
3. Open `Assets/Scenes/Play.unity` (also in File → Build Settings).
4. Press **Play**. Do not assign meshes in the Inspector.

## Play verify

**ArtImport: 51/51 Play Mode FBX ready** — `ArtImport.PlayModeAssets` has 51 names; all 51 exist under `Assets/Resources/Art/Import/` and `Assets/Art/Import/`. `Ship_Body_Upgrade02` is imported only (not warmed, not in the shop). 0.341 art-wire uses Scout / Gunner v5 and Drone v4 under the canonical `Enemy_Scout` / `Enemy_Gunner` / `Enemy_Drone` names (Buffer_v5 / Buffer_v4 aliases). Bomber / Sniper stay v5. Buffer_* names are aliases only; primary FBX names resolve first.

- Hangar shows BlenderBot ship + crate / terminal / pillar / workbench / kiosk / banner / ammo rack + **Console / PowerBox / FireExtinguisher / Locker / LaunchSign** + parked **Ship_Complete**. LaunchSign sits on the pad’s camera-front edge as the Start Wave landmark.
- First hangar (wave 1, first session): left **First flight** card — WASD / aim / shoot, **Abort (Esc)**, **Q / RMB** fire modes (discover Spread / Pierce when owned), “Clear a wave to earn credits and upgrades.”, shop + Start Wave. **Got it** or Start Wave dismisses it (PlayerPrefs; no spam later). Hangar status line before Start Wave mirrors the same Abort / Q hint.
- Top-right badge reads **WORLD 1** on wave 1. After 5 clears it becomes **WORLD 2**, then 3–6, then loops. Arena-swap flashes **WORLD N ONLINE** and plays `maximize_008` (not the hangar purchase cue).
- Shop is grouped Hull/Nose/Engine | Weapons | Defense below Start / credits (no overlap). Buttons show title + cost; hover writes the long description on the status line. Owned / locked / too-poor plates are **distinct greys** (teal OWNED, charcoal LOCKED).
- Hangar HUD (upper left) always shows **Best score · Wave · World** with the live wave / score / hull line. During play the score line compares against Best (`/ Best N` or `NEW BEST`); audio sliders hide so they do not clash with the World badge. `IsBetter` can tie-break on world after score and wave.
- Wave clear / fail show a short **Run summary** card: score / wave / world / credits (+ awarded) / upgrades. New record appends **NEW BEST**. After waves 1–3, one short continue line (World 2 / Gunner at 4 / **Buy X before Gunner**). Wave 3 also shows a light **★ Scout Wing** medal (World 2 at wave 6) — no text wall.
- First Play audio: SFX audible (0.8), music not blasting (0.28). Hangar bed is denser (layered spacelife, clearer pitch); arena bed is fuller. Abort ducks the bed under the whoosh. Hits layer `impactMetal_000` under `impactMetal_003`. Mute / sliders persist in PlayerPrefs.
- Distinct SFX: UI click (`click_002`, not purchase), shop buy (`confirmation_002`), abort whoosh (`minimize_005`), spread (`laserRetro_000`) vs pierce (`laserLarge_000`) vs bolt (`laserSmall_000`). Enemy death is `explosionCrunch_003` + metal punch; asteroid split stays `explosionCrunch_000` (no punch). SwarmPod / Mid use `explosionCrunch_001` death and `impactMetal_001` hit (no punch). Abort-duck and hit-punch stay from 0.34.
- Hit flash + light camera shake on damage / explosion. Player death stays quiet (no extra flash/shake).
- Spread bolts are amber / fat / short-trail + SpreadCore; pierce bolts are cyan / long / thin + PierceNeedle. Gunner and Sniper fire red `Projectile_EnemyBolt`.
- Wave 1: `Enemy_01`. Later: Scout v5 / Gunner v5 / Drone v4. Waves 7+: Bomber / Sniper / SwarmPod if those FBX imported. After wave 10: +1 large asteroid per wave (cap 10).
- 0.3 loop: asteroids wrap at `ArenaRadius`; stranded threats outside `radius+2` for >3s force-wrap or despawn. Playing HUD has **Abort → Hangar** (Esc). Shop adds Spread Bolt + Pierce (Q / RMB cycle). Gunner 4 HP, Bomber 5 HP. Fail text `Enemy contact (Kind)`.
- Buy Body Upgrade → `Ship_Body_Upgrade01`. Nose/Engine Upgrade 02 after their prereqs.
- Shoot → muzzle flash. Kill → explosion. Asteroids mix A/B/C/D visuals; split rules unchanged.
- Console on Play: `ArtImport: 51/51 Play Mode FBX ready`. Resources FBX must be real binaries, not LFS pointer text.

Repo checks (no Editor):

```
python3 Tools/validate_week1_project.py
python3 Tools/test_week1_logic.py
```

## Known stubs / out of scope

- Primitive fallbacks if an FBX is missing
- `Ship_Complete_Upgrade01`, Body Upgrade 02, extra pickup gameplay beyond Score/Shield/Health/RapidFire
- `Ship_Complete` is hangar dressing only (not the playable part-slot ship)
- World 2–6 are **mesh swaps only** (same arena radius and rules), not new campaigns
- No extra ships, no 30-wave campaign, no multiplayer, no Input System / URP / TMP

## Do not merge until

- [ ] Wagge playtested and said **yes**
- [ ] SpelPM is fine with the Week 2 + arena-world scope on `main`
- [ ] Product name is still exactly **Asteroids gone rogue**
