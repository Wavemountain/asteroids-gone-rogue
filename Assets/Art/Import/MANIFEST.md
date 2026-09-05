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
- [x] `Ship_Complete` (hangar bay display, Buffer v4 bytes)
- [x] `Ship_Nose_Upgrade01` (hangar nose slot)
- [x] `Ship_Engine_Upgrade01` (hangar engine slot)
- [x] `Ship_Body_Upgrade01` (imported, no shop swap)
- [x] `Ship_Complete_Upgrade01` (preview only)

## Combat / arena meshes

- [x] `Asteroid_Large` (~4.8 m) — type A visual, Week 1 split rules
- [x] `Asteroid_Small` (~1.8 m)
- [x] `Asteroid_VariantB_Large` / `Asteroid_VariantB_Small` — type B visual only (same split)
- [x] `Enemy_01` (~2 m) — Week 1 playable Mid enemy (Buffer v8 mesh)
- [x] `Arena_Blockout` (play radius ~22 m) — World 1
- [x] `Projectile_Bolt` (Buffer v2 mesh)
- [x] `Projectile_EnemyBolt`

## Hangar dressing (pivots at base)

- [x] `Hangar_Crate`
- [x] `Hangar_Terminal`
- [x] `Hangar_LightPillar`
- [x] `Hangar_Console` / `Hangar_PowerBox` / `Hangar_FireExtinguisher` (0.31 hangar-wire)
- [x] `Hangar_Locker` (0.32 hangar-wire)
- [x] `Hangar_LaunchSign` (Start Wave landmark, emissive GO plate + mesh GO decal from hangar camera)

## Present but not Week 1 gameplay

- [x] `Enemy_Scout` / `Enemy_Gunner` (Play waves 2+, Buffer v6 bytes under canonical names)
- [x] `Enemy_Bomber` / `Enemy_Sniper` (Play waves 7+, Bomber Buffer v6 / Sniper Buffer v5 under canonical names)
- [x] `Enemy_SwarmPod` (Play waves 9+, Buffer v6 bytes under the canonical name)
- [x] `Enemy_Drone` (Play waves 5+, Buffer v5 bytes under the canonical name)
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
