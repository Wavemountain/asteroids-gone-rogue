# BlenderBot brief — hangar preview parts (0.47 F1)

No new FBX in this pass. The preview tints meshes that already spawn. A later model pass should name parts and sockets exactly as below so the hangar code can keep finding them by name. Pivot stays at the origin. Scale is 1 unit = 1 metre. Do not rename the existing Play Mode meshes.

## Slots (already in `ContentFactory`)

| Transform | What hangs on it |
|---|---|
| `Ship_Body` | `Ship_Body`, `Ship_Body_Upgrade01` |
| `Ship_Nose` | `Ship_Nose`, `Ship_Nose_Upgrade01`, `Ship_Nose_Upgrade02`, `RailHardpoint` |
| `Ship_Engine` | `Ship_Engine`, `Ship_Engine_Upgrade01`, `Ship_Engine_Upgrade02` |

The preview paints a property-block tint when a node name contains `Nose_Upgrade`, `RailHardpoint`, `Engine_Upgrade`, `Overcharger`, or `Afterburner`, and it parents a code-built `Mk2Marker` and `TrailRibbon` under the slot root. Those two are not meshes to author.

## Sockets a future pass should add

Empty transforms, identity rotation, no scale, parented to the slot. The game does not require them yet.

| Socket | Parent | Purpose |
|---|---|---|
| `Socket_Hardpoint` | `Ship_Nose` | Nose gun / rail muzzle. Local +Z is fire. |
| `Socket_Engine_L` | `Ship_Engine` | Left plume. |
| `Socket_Engine_R` | `Ship_Engine` | Right plume. |
| `Socket_Mk2` | `Ship_Body` | Mk II marker, about 1.15 m above the deck. |
| `Socket_Trail` | `Ship_Engine` | Trail ribbon origin, aft of the nozzles. |

## Materials to keep split

`Mat_Ship_Hull`, `Mat_Ship_Accent`, `Mat_Ship_Glass`, `Mat_Ship_Glow`. Hull and accent need `_Color` and `_EmissionColor` on the Standard shader so the bay paint and the hardpoint / engine tint can show without a new material asset. Glow should stay a small surface (nozzle, hardpoint tip), not the whole hull, or the rim light will not read the silhouette.

## Bounds

The studio camera is fixed (FOV 40, local `(0.2, 4.55, -10)`, look `(0, 0.08, 0)`, showcase scale 1.25, render target 768×960). Keep the assembled ship inside a sphere of radius **2.0 m** at scale 1 (2.5 m after showcase scale) or the portrait frame will crop it. No part should extend past that sphere, including plumes.
