# MANIFEST — AsteroidsGoneRogue_Week1_All

Artist pack checklist for **Asteroids gone rogue** Week 1.
FBX in this folder are instantiated on Press Play by `ArtImport` / `ContentFactory`.

## Pack

- [x] `AsteroidsGoneRogue_Week1_All` (combined FBX, not spawned)
- [x] This `MANIFEST`
- [x] `MANIFEST_BlenderBot.txt`

## Ship meshes (FBX, scale 1u = 1m, pivot 0,0,0)

- [x] `Ship_Nose` — Play Mode
- [x] `Ship_Body` — Play Mode
- [x] `Ship_Engine` — Play Mode
- [x] `Ship_Complete` (preview only)
- [x] `Ship_Nose_Upgrade01` (hangar nose slot)
- [x] `Ship_Engine_Upgrade01` (hangar engine slot)
- [x] `Ship_Body_Upgrade01` (imported, no shop swap)
- [x] `Ship_Complete_Upgrade01` (preview only)

## Combat / arena meshes

- [x] `Asteroid_Large` (~4.8 m) — type A visual, Week 1 split rules
- [x] `Asteroid_Small` (~1.8 m)
- [x] `Asteroid_VariantB_Large` / `Asteroid_VariantB_Small` — type B visual only (same split)
- [x] `Enemy_01` (~2 m) — Week 1 playable enemy
- [x] `Arena_Blockout` (play radius ~22 m) — World 1
- [x] `Projectile_Bolt`

## Hangar dressing (pivots at base)

- [x] `Hangar_Crate`
- [x] `Hangar_Terminal`
- [x] `Hangar_LightPillar`
- [x] `Hangar_Console` / `Hangar_PowerBox` / `Hangar_FireExtinguisher` (0.31 hangar-wire)

## Present but not Week 1 gameplay

- [x] `Enemy_Scout` / `Enemy_Gunner` / `Enemy_Drone` (loadable if spawned)
- [x] `Arena_World2_Blockout` / `Arena_World3_Blockout`
- [x] `Pickup_Score` / `Pickup_Shield` (`CreatePickup` only)

## Materials

Ship:

- [x] `Mat_Ship_Hull`
- [x] `Mat_Ship_Accent`
- [x] `Mat_Ship_Glass`
- [x] `Mat_Ship_Glow`

World:

- [x] `Mat_Asteroid`
- [x] `Mat_Enemy`
- [x] `Mat_Arena`

## Notes

- Ship parts **and upgrade meshes** must share origin so hangar slot swaps stay aligned.
- Week 1 ships may be replaced; do not rely on unique per-part offsets in engine code.
- Press Play loads `Assets/Art/Import/*.fbx` automatically — see `IMPORT.md`.
