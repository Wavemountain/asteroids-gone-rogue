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

### Ship (BlenderBot v2 / v4 — may be refreshed)

- `Ship_Nose.fbx`
- `Ship_Body.fbx`
- `Ship_Engine.fbx`
- `Ship_Complete.fbx` (reference assemble only; gameplay uses the slots)
- `Ship_Nose_Upgrade01.fbx` (hangar **Nose Hardpoint** slot swap)
- `Ship_Engine_Upgrade01.fbx` (hangar **Rapid Fire** slot swap)

All `Ship_*` parts, including upgrades, share origin `0,0,0` with the base slots.

### Combat / arena (unchanged)

- `Asteroid_Large.fbx`
- `Asteroid_Small.fbx`
- `Enemy_01.fbx` — **Week 1 uses this one enemy type only**
- `Arena_Blockout.fbx`
- `AsteroidsGoneRogue_Week1_All.fbx` (optional combined pack)
- `MANIFEST` (artist checklist — this repo also keeps `MANIFEST.md`)

### Later wave ladder (do not implement in Week 1)

Documented for a future enemy roster. Week 1 stays on `Enemy_01`.

- `Enemy_Scout.fbx` — early-wave chaser (later)
- `Enemy_Gunner.fbx` — later-wave shooter (later)

`Ship_*` may be refreshed soon. Keep prefabs swap-friendly: do not bake unique offsets into gameplay scripts.

## Materials to assign on import

Ship (current BlenderBot set):

- `Mat_Ship_Hull`
- `Mat_Ship_Accent`
- `Mat_Ship_Glass`
- `Mat_Ship_Glow`

World / threats:

- `Mat_Asteroid`
- `Mat_Enemy`
- `Mat_Arena`

Stubs already live in `Assets/Art/Materials/`.

## How to swap without breaking Week 1

Gameplay builds a live hierarchy at Play:

```
Ship
  Slots                         (origin 0,0,0)
    Ship_Body                   PartSlot "Body"  (+ canopy uses Mat_Ship_Glass)
    Ship_Nose                   PartSlot "Nose"
      Mesh_Default
      Ship_Nose_Upgrade01       toggled by Nose Hardpoint
    Ship_Engine                 PartSlot "Engine"
      Mesh_Default
      Ship_Engine_Upgrade01     toggled by Rapid Fire
    ShieldBubble
```

1. Import the FBX with scale 1, bake axis conversion if the DCC used Z-up.
2. Parent the FBX under the named `PartSlot` (or replace the matching prefab mesh in `Assets/Art/Prefabs/`).
3. Leave the slot transform at `0,0,0`. Move mesh verts in DCC, not the slot.
4. Assign the materials above.
5. Upgrade meshes stay as siblings under the same slot so hangar purchases are a SetActive swap.

Prefab stubs already exist at `Assets/Art/Prefabs/` with the same names. They are visual templates; `ContentFactory` currently builds primitives at runtime so Play Mode does not depend on prefab wiring.

## Import settings (Unity)

- Scale Factor: `1`
- Convert Units: off (or confirm 1u = 1m)
- Mesh Compression: off for first drop
- Read/Write: on if you later generate colliders from mesh
- Generate Colliders: off (gameplay already adds simple colliders)
- Animation Type: None
- Material Creation Mode: None (or remap to the `Mat_*` assets)

See `MANIFEST.md` for the checkbox list.
