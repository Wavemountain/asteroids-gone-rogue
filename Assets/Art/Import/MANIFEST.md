# MANIFEST — AsteroidsGoneRogue_Week1_All

Artist pack checklist for **Asteroids gone rogue** Week 1.

## Pack

- [ ] `AsteroidsGoneRogue_Week1_All` (single archive or FBX)
- [ ] This `MANIFEST`

## Ship meshes (FBX, scale 1u = 1m, pivot 0,0,0)

- [ ] `Ship_Nose`
- [ ] `Ship_Body`
- [ ] `Ship_Engine`
- [ ] `Ship_Complete` (preview only)
- [ ] `Ship_Nose_Upgrade01` (hangar nose slot)
- [ ] `Ship_Engine_Upgrade01` (hangar engine slot)

## Combat / arena meshes

- [ ] `Asteroid_Large` (~4.8 m)
- [ ] `Asteroid_Small` (~1.8 m)
- [ ] `Enemy_01` (~2 m) — Week 1 playable enemy
- [ ] `Arena_Blockout` (play radius ~22 m)

## Later (not Week 1 gameplay)

- [ ] `Enemy_Scout` (early-wave chaser, later ladder)
- [ ] `Enemy_Gunner` (later-wave shooter, later ladder)

## Materials

Ship:

- [ ] `Mat_Ship_Hull`
- [ ] `Mat_Ship_Accent`
- [ ] `Mat_Ship_Glass`
- [ ] `Mat_Ship_Glow`

World:

- [ ] `Mat_Asteroid`
- [ ] `Mat_Enemy`
- [ ] `Mat_Arena`

## Notes

- Ship parts **and upgrade meshes** must share origin so hangar slot swaps stay aligned.
- Week 1 ships may be replaced; do not rely on unique per-part offsets in engine code.
- Drop files into `Assets/Art/Import/` and follow `IMPORT.md`.
