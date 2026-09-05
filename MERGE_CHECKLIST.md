# Merge checklist — PR #1 → `main`

**Do not merge until Wagge says yes.**

This is the Week 1–2 playable core for **Asteroids gone rogue**. Branch: `cursor/week1-unity-core-0000`.

## Unity

- **2022.3.21f1 LTS** (changeset `bf09ca542b87`)
- Built-in RP, old Input Manager (no Input System / URP / TMP)
- Product name must stay exactly `Asteroids gone rogue`

## After `git pull`

1. Open the repo folder in Unity Hub (the folder with `Assets/`, `Packages/`, `ProjectSettings/`).
2. Wait for FBX import (`Library/` is local / gitignored).
3. Open `Assets/Scenes/Play.unity` (also in File → Build Settings).
4. Press **Play**. Do not assign meshes in the Inspector.

## Play verify

- Hangar shows BlenderBot ship + crate / terminal / pillar / workbench / kiosk / banner.
- Top-right badge reads **WORLD 1** on wave 1. After 5 clears it becomes **WORLD 2**, then 3–6, then loops.
- Shop buttons you cannot afford (or that are locked / owned) are **greyed and disabled**.
- Wave 1: `Enemy_01`. Later: Scout / Gunner / Drone. Waves 7+: Bomber / Sniper / SwarmPod if those FBX imported.
- Buy Body Upgrade → `Ship_Body_Upgrade01`. Nose/Engine Upgrade 02 after their prereqs.
- Shoot → muzzle flash. Kill → explosion. Asteroids mix A/B/C/D visuals; split rules unchanged.
- Console: `ArtImport: N/N Play Mode FBX ready`.

Repo checks (no Editor):

```
python3 Tools/validate_week1_project.py
python3 Tools/test_week1_logic.py
```

## Known stubs / out of scope

- Primitive fallbacks if an FBX is missing
- `Ship_Complete*`, Body Upgrade 02, extra pickup gameplay beyond Score/Shield/Health/RapidFire
- World 2–6 are **mesh swaps only** (same arena radius and rules), not new campaigns
- No extra ships, no 30-wave campaign, no multiplayer, no Input System / URP / TMP

## Do not merge until

- [ ] Wagge playtested and said **yes**
- [ ] SpelPM is fine with the Week 2 + arena-world scope on `main`
- [ ] Product name is still exactly **Asteroids gone rogue**
