# Art import — Asteroids gone rogue

Week 1 FBX files live in this folder. **Press Play instantiates them automatically.**
Wagge does not need to drag meshes onto prefabs or Inspector fields.

`ContentFactory` is added at runtime, so visuals load by path via `ArtImport`:
`Resources.Load("Art/Import/<Name>")` first (copies under `Assets/Resources/Art/Import/`),
then `Assets/Art/Import/<Name>.fbx` in the Editor. If an FBX is missing, that object
falls back to a primitive. No Inspector mesh assignment.

## Scale and pivots

- **1 Unity unit = 1 meter**
- **Ship / combat meshes:** pivots must be `0, 0, 0` (especially `Ship_*`)
- **Hangar props:** pivots sit on the **base** (floor contact), not the mesh center
- Forward is **+Z**, up is **+Y** (FBX export: Forward **-Z**, Up Y)
- Ship part meshes share one origin so hangar upgrades can enable/disable slots without re-centering

## Target sizes (authoring)

| Asset | Rough size |
| --- | --- |
| Ship (assembled) | ~3 m long |
| `Asteroid_Large` | ~4.8 m |
| `Asteroid_Small` | ~1.8 m |
| `Enemy_01` | ~2 m |
| Arena play radius | ~22 m |

## Wired on Press Play

These filenames are instantiated by `ContentFactory` / `ArtImport` when the file exists:

### Ship (BlenderBot v4) — hangar slot swaps

- `Ship_Nose.fbx` / `Ship_Body.fbx` / `Ship_Engine.fbx`
- `Ship_Nose_Upgrade01.fbx` — hangar **Nose Hardpoint**
- `Ship_Engine_Upgrade01.fbx` — hangar **Rapid Fire**

`Ship_Body_Upgrade01.fbx` is the hangar **Body Upgrade** slot swap.
`Ship_Complete.fbx` (Buffer v5) is a hangar bay display via `ContentFactory`.
`Ship_Complete_Upgrade01.fbx` stays a reference assemble only.

### Combat / arena

- `Asteroid_Large.fbx` / `Asteroid_Small.fbx` — type A visual (Week 1 split rules)
- `Asteroid_VariantB_Large.fbx` / `Asteroid_VariantB_Small.fbx` — type B visual only (~45% mix)
- `Enemy_01.fbx` — wave 1 Mid mesh (Buffer v8 bytes; also accepts `Enemy_01_Buffer_v8`)
- `Enemy_Scout.fbx` / `Enemy_Gunner.fbx` (Buffer v7 bytes under the canonical names) — Play waves 2+ via `CreateEnemy` / `EnemyCatalog.VisualName` (primary name first; also accepts `Enemy_*_Buffer_v7` then `Enemy_*_Buffer_v6` / `Enemy_*_Buffer_v5` / `Enemy_*_Buffer_v4`)
- `Enemy_Bomber.fbx` / `Enemy_Sniper.fbx` — Play waves 7+ via `CreateEnemy` / `EnemyCatalog.VisualName` (Bomber Buffer v8 then v6/v5; Sniper Buffer v8 then v5). Bomber gets a warm emission dress on the v8 mesh. Sniper gets the same spawn-fallback + palette remap as Scout/Gunner, plus a cyan emission dress on the v8 mesh.
- `Enemy_SwarmPod.fbx` (Buffer v6 bytes under the canonical name) — Play waves 9+ (also accepts `Enemy_SwarmPod_Buffer_v6`)
- `Enemy_Drone.fbx` (Buffer v6 bytes under the canonical name) — waves 5+ via `CreateEnemy` / `EnemyCatalog.VisualName` (also accepts `Enemy_Drone_Buffer_v6` then `Enemy_Drone_Buffer_v5` / `Enemy_Drone_Buffer_v4`)
- `Monster_Brute.fbx` — wave 8+ close-range charge tank. `DressMonsterPresence` adds idle aura + charge glow (AAA read, not a raw FBX drop). Sculpted primitive fallback if missing.
- `Monster_Swarm.fbx` — wave 9+ swarm/spawner. Same presence dress; Swarmlings reuse this mesh at 0.45 scale (`Monster_Swarmling` alias).
- `Arena_Hazard_Spike.fbx` — layout pylons / mine-belt / island spikes. `DressHazardSpike` adds ring, well, light, and pulse (professional, not prototype). Damaging on the mine belt + one debris spike.
- `Arena_Blockout.fbx` — World 1 hangar / arena
- `Projectile_Bolt.fbx` — player shot visual (GameBot Bolt Buffer v2; also accepted as `Projectile_Bolt_Buffer_v2`)
- `Projectile_EnemyBolt.fbx` — Gunner / Sniper shot visual (also accepted as `Projectile_EnemyBolt_Buffer`)

`Arena_World2_Blockout.fbx` / `Arena_World3_Blockout.fbx` are not Week 1 play worlds.

### Hangar dressing

Shown in hangar / results and hidden during a wave. Pivots on the **base** — placed at floor Y = 0.

- `Hangar_Crate.fbx`
- `Hangar_Terminal.fbx`
- `Hangar_LightPillar.fbx`
- `Hangar_Workbench.fbx` / `Hangar_FuelCell.fbx` / `Hangar_ShopKiosk.fbx`
- `Hangar_Console.fbx` / `Hangar_PowerBox.fbx` / `Hangar_FireExtinguisher.fbx` — 0.31 hangar-wire from BlenderBot buffer (no `_Buffer` suffix)
- `Hangar_Locker.fbx` — 0.32 hangar-wire from BlenderBot buffer (no `_Buffer` suffix)
- `Hangar_LaunchSign.fbx` — Start Wave landmark from Buffer v2 (canonical name first); 0.36 keeps the emissive GO plate / glow pulse and adds a mesh GO decal if TextMesh flickers

### Pickups (not in the Week 1 loop)

`Pickup_Score.fbx` / `Pickup_Shield.fbx` load through `ContentFactory.CreatePickup`.
Nothing in the wave loop calls that yet.

## Materials remapped at instantiate

Imported `Mat_*` names are remapped onto the Week 1 palette (including Accent Hot/Warm, Glow, Glass, `Mat_Asteroid_B`, hangar / bolt / pickup names).

Ship:

- `Mat_Ship_Hull`
- `Mat_Ship_Accent` (+ Hot / Warm)
- `Mat_Ship_Glass`
- `Mat_Ship_Glow`

World / threats:

- `Mat_Asteroid` / `Mat_Asteroid_B`
- `Mat_Enemy`
- `Mat_Arena`
- `Mat_Monster_Brute` / `Mat_Monster_Swarm` / `Mat_Arena_Hazard` (runtime; look-bible polish)

Stubs already live in `Assets/Art/Materials/`.

## How Play Mode assembles the ship

```
Ship                          capsule collider on root
  Slots                       origin 0,0,0
    Ship_Body                 Ship_Body.fbx
    Ship_Nose                 Ship_Nose.fbx
      Ship_Nose_Upgrade01     toggled by Nose Hardpoint
    Ship_Engine               Ship_Engine.fbx
      Ship_Engine_Upgrade01   toggled by Rapid Fire
    ShieldBubble
```

Imported visuals are children of the named slots. Gameplay colliders stay on the roots.
Do not generate colliders on the FBX (metas set `addColliders: 0`).

## Import settings (Unity)

Committed `.meta` files already set:

- Scale Factor: `1`
- Generate Colliders: off
- Animation Type: None
- Import Cameras / Lights: off
- Bake Axis Conversion: off (export is already Unity -Z / Y-up)

See `MANIFEST.md` / `MANIFEST_BlenderBot.txt` for the file list.
