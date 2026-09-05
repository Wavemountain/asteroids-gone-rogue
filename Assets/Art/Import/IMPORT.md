# Art import — Asteroids gone rogue

Drop Week 1 FBX files here. The playable loop already runs on primitive stubs.
These notes exist so ship parts and threats can be swapped without rewriting gameplay.

## Scale and pivots

- **1 Unity unit = 1 meter**
- **Pivots must be `0, 0, 0`** on every imported mesh (especially `Ship_*`)
- Forward is **+Z**, up is **+Y**
- Ship part meshes share one origin so hangar upgrades can enable/disable slots without re-centering

## Target sizes (authoring)

| Asset | Rough size |
| --- | --- |
| Ship (assembled) | ~3 m long |
| `Asteroid_Large` | ~4.8 m |
| `Asteroid_Small` | ~1.8 m |
| `Enemy_01` | ~2 m |
| Arena play radius | ~22 m |

## Expected filenames

Place these next to this file (or inside a pack folder — keep the names):

- `Ship_Nose.fbx`
- `Ship_Body.fbx`
- `Ship_Engine.fbx`
- `Ship_Complete.fbx` (reference assemble only; gameplay uses the three slots)
- `Asteroid_Large.fbx`
- `Asteroid_Small.fbx`
- `Enemy_01.fbx`
- `Arena_Blockout.fbx`
- `AsteroidsGoneRogue_Week1_All.fbx` (optional combined pack)
- `MANIFEST` (artist checklist — this repo also keeps `MANIFEST.md`)

`Ship_*` may be refreshed soon. Keep prefabs swap-friendly: do not bake unique offsets into gameplay scripts.

## Materials to assign on import

Create / assign these Standard materials (already stubbed in `Assets/Art/Materials/`):

- `Mat_Ship_Hull`
- `Mat_Ship_Accent`
- `Mat_Asteroid`
- `Mat_Enemy`
- `Mat_Arena`

## How to swap without breaking Week 1

Gameplay builds a live hierarchy at Play:

```
Ship
  Slots                    (origin 0,0,0)
    Ship_Body              PartSlot "Body"
    Ship_Nose              PartSlot "Nose"  (default + upgraded hardpoint)
    Ship_Engine            PartSlot "Engine"
    ShieldBubble
```

1. Import the FBX with scale 1, bake axis conversion if the DCC used Z-up.
2. Open the matching prefab in `Assets/Art/Prefabs/` **or** parent the FBX under the named `PartSlot`.
3. Leave the slot transform at `0,0,0`. Move mesh verts in DCC, not the slot.
4. Assign the materials above.
5. For the nose upgrade, keep a second child named `Mesh_Upgraded` (dual barrels). `ShipVisuals` toggles it when **Nose Hardpoint** is bought.

Prefab stubs already exist at `Assets/Art/Prefabs/` with the same names and materials. They are visual templates; `ContentFactory` currently builds primitives at runtime so Play Mode does not depend on prefab wiring.

## Import settings (Unity)

- Scale Factor: `1`
- Convert Units: off (or confirm 1u = 1m)
- Mesh Compression: off for first drop
- Read/Write: on if you later generate colliders from mesh
- Generate Colliders: off (gameplay already adds simple colliders)
- Animation Type: None
- Material Creation Mode: None (or remap to the `Mat_*` assets)

See `MANIFEST.md` for the checkbox list.
