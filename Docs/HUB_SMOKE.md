# Hub-open smoke — Asteroids gone rogue 0.44 steam-slice

Short checklist for Unity Hub / Speltest on `main`. No VR/XR Continue dialog.

## Open

1. Unity Hub → **Add** this folder (`Assets/`, `Packages/`, `ProjectSettings/`).
2. Unity **6000.6.0f1** (Hub may offer a newer 6000.6 patch — fine).
3. Project opens **without Continue** (manifest has no `modules.vr` / `modules.xr`, no Input System, no TMP).
4. Open `Assets/Scenes/Play.unity` (also in File → Build Settings).
5. Press **Play**. Console: `ArtImport: 54/54 Play Mode FBX ready`.

## Hangar

- First-flight card on first session (Got it / Start Wave / B).
- Shop left, LOADOUT preview right (diagonal cam, idle spin).
- D-pad **and** LS move Start Wave / Continue ↔ hull 4-col ↔ weapons ↔ defense (ghost preview on LOCKED) **and** Easy/Normal/Hard, LANG, Mute, Credits, Got it.
- **A** confirm, **B** back, **Start** pause. Focus-ring on pad-selected shop rows. D-pad is **not** fly (not aliased onto Horizontal).
- HUD plates (`HudPlate` / `HealthRack`) share surface `#0E1520` @ 0.72 + primary header rules. Fail = danger header; wave-clear / win = primary. Abort = danger-tint.
- LANG chips: inactive desat 40%, selected secondary ring, pad focus-ring.
- HUD: Session highscore + Best. ACHIEVEMENTS ladder under medals.
- LANG + difficulty (Easy/Normal/Hard) + Mute + Credits.

## Play

- Wave 1 → 5 is the **finished World 1 loop**. Clear wave 5 (Brute) → **SECTOR CLEAR** + HIT07. Not endless.
- Abort (Esc / Start) if a rock strands. HEALTH rack + lives stay on.
- Death with lives left = respawn + LIFE LOST (session score visible). 0 lives = SHIP LOST + **RETRY · NEW RUN**.
- Extra-life heart: `powerUp7` pickup / `phaserDown3` miss. MissionPlausible mix **0.65**.

## Store F12

Play Mode: **F9** cycle poses, **F12** PNG → `Docs/StoreCaptures/out/`. Menu **Asteroids gone rogue → Store Captures**. See `Docs/StoreCaptures/README.md`. Capsule amber `#D4A04A`, 3/4 `Ship_Complete`.

Repo checks (no Editor): `python3 Tools/validate_week1_project.py` and `python3 Tools/test_week1_logic.py`.
