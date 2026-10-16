# Layer gate

A layer bug (the dimmed W3/W4 beam plate, a ring drawn over enemies, a HUD line sitting on another line) must fail a gate before it is treated as a passing capture.

The decision lives in `Assets/Scripts/Core/LayerGate.cs`. `LayerGate.SelfCheck` injects each failure class and requires a FAIL string, then requires a clean scene to pass. `Tools/test_week1_logic.py` runs that self-check. `Tools/layer_gate_check.py` injects bad shader text and a `renderQueue = 4000` literal and requires those to FAIL as well.

Whitelist data: `Tools/layer_whitelist.json`.

## What fails

1. **Unknown renderer.** A visible renderer must match a renderer-name glob and a material-name glob. Patterns come from `renderers` / `materials` plus the shot's world (`hangar`, `1`, `3`, `4`, `death`, `waveclear`). Death and wave-clear also accept world `1`. A disabled renderer is ignored. `*_*` admits compound names; a stray single token such as `OlivePlate` with `Default-Material` does not.
2. **Dead plate.** A visible opaque renderer (queue under 2500) with emission luma ≤ 0.45 and albedo luma ≤ 0.45 fails when it is unlit or its name/material contains a `deadNameHints` token (`Beam`, `W3_Beam`, `W4_Glow`). That is the dimmed olive plate. An additive beam with alpha 0 is not visible, so it does not fail. `deadExempt` globs (belt asteroids, debris, smoke, bolt halo, telegraph rings) skip this check. A beam name still fails. `AGR/BlobShadow` is a shadow, not a dead plate: black RGB and an unlit queue under 2500 are how the blob darkens the floor (`KeepGameplay` rewrites the authored 2990 to 2450 on the shared material). The shader name is the class. The object name is not a waiver, and alpha must clear `VisibleAlpha` (0.04). The capture still requires the blob on the floor plate and under bodies.
3. **ZTest Always / Overlay queue.** A shader or material that depth-tests with Always, or sits at queue ≥ 4000, fails when it can draw over the ship, enemies, asteroids, pickups, or pillars. `AGR/Post` and `Hidden/*` are the post exemption. HUD canvases are not mesh renderers.
4. **Queue and sort.** Gameplay roles must use queue 2000–3999 (Geometry 2000–2449, AlphaTest 2450–2499, Transparent 2500–3999). A negative queue is the shader default, Geometry 2000. The dome ring (`ArenaLip` / `Mat_Env_Lip`) must use ZTest Less (2) or LEqual (4). `AgrDomeRing.shader` exposes `_ZTest` and samples it. Enabled canvases must use a `CanvasOrder` value (`Root` 0, `ShipPreview` 80, `HangarShop` 120, `Overlay` 150, `Rebind` 160, `Boon` 200, `Toast` 250).
5. **UI overlap.** Rect overlap and glyph boxes (`preferredWidth` / `preferredHeight`) fail the shot. That covers HUD-vs-HUD, HUD vs the hint row, and the hangar title / world / medal rows. The same measurement already logged `AgrCapture FAIL:`; the layer gate records it as `ui-overlap` too. `LayerGate.IsUiBackgroundPair` skips one explicit list, in both orders: `Hint x HintScrim` and `HintScrim x Hint`. HintScrim is the plate drawn behind the hint line, so that pair is a control on its own background, not two controls. `AgrCapture` skips the same pairs before it logs `hud overlap` or records a layer hit. Any other pair, including `WorldBadge x BadgeRow`, still fails. `Hint x ShipPreviewFrame` is not on the list. The hangar hint ends at `GameUi.HangarHintMaxY` (0.064) and `ShipPreviewMin.y` is 0.080, a gap of 0.016 (12.8 px at 800, 17.3 at 1080, 23.0 at 1440). Prompt segments are clamped inside the hint, so a real overlap is fixed in the rects rather than waived.
6. **Contact shadow.** Every active `ShipController`, `EnemySeeker`, and `Asteroid` needs an enabled child renderer named `ContactBlob`, on that body's XZ, just above the floor (`FloorSurface` raycast or renderer bounds, then 0.02–0.05 m). The quad radius is 2× the footprint and the centre alpha is 0.9. Spawn paths call `ContactBlob.Attach` (`CastFormShadow`, plus a LateUpdate ensure on the ship, enemies, and asteroids) so a pooled or late mesh still gets a blob. `AgrCapture` then reads the frame. The local floor is a ring just outside the blob. When that luma is above 0.06, the shadow sample must be at least 15% darker. When the local floor luma is 0.06 or lower, a percentage is not visible, so the shot does not use a pixel threshold: the blob renderer must be enabled, active, in view when the body is near the ship, and sitting on the body. W3's floor is in that dark band. W4 is bright enough for the 15% test. W1's spawn disc can be the local surface; if that disc is at or under 0.06, W1 uses the same renderer check, and the separate spawn-disc contrast sample (framebuffer pixels on the disc, outside the blob) still has to reach 1.3.
7. **UI camera and canvas layer.** Every enabled canvas must sit on the built-in UI layer (`LayerGate.UiLayer`, 5). `GameUi.UseUiLayer` assigns that layer wherever a canvas is created, including nested canvases (`RaiseCanvas`, the shop, the prompt, the rebind overlay). The capture overlay camera (`AgrCaptureUiCamera`) must use `cullingMask = LayerGate.UiCameraMask` (`1 << 5`) and must sit at `(0, -10000, 0)`, so it cannot redraw the ship or other Default-layer objects on top of the HUD. A mask that includes any other layer fails as `ui-camera`. A canvas on another layer fails as `canvas-layer`. The hangar preview camera stays on layer 8 and is not a UI camera.
8. **Decor near spawn.** Each arena blockout is one mesh. `SpawnClear.ClearVisual`, called from `ApplyArenaForWave`, copies that mesh and drops a connected island when its closest XZ point is within 4 of the origin and its farthest XZ point is within 8. That removes the W3 centre plate and glow pillar, the W2 pads, the W4 pillar, the W5 plinth and pillar, and the W6 plinth. The floor and the W4 lane walls stay, because they reach farther than 8. The same pass drops a compact corner island that straddles the lip (at least 3 inside the arena radius and at least 4 outside, both spans between 3 and 12) so the W4 ring stays continuous. A capture that still finds a spawn island fails as `spawn-decor`. An unreadable mesh reports the renderer's world bounds, not a zero box at the origin. Children under `Arena_Layout` (hazard spikes on the cross) are not blockout islands. A death shot with live `Smoke` particles fails as `death-remnant Smoke`. A ship with no enabled hull renderer is not an active body, so a shattered ship does not fail `contact-shadow`.

## Static vs runtime

| Check | Static (`validate_week1_project.py` → `layer_gate_check.py`) | Runtime (`AgrCapture.CheckLayer`, every shot) |
| --- | --- | --- |
| Whitelist file, keys, `Mat_*`, `Beam` hint, `canvasSorts` == `CanvasOrder` | Yes | Reads the same JSON. A missing or broken file is a FAIL. |
| Unknown renderer / material | Self-check only (injected facts) | Yes, per visible renderer |
| Dead plate | Self-check only | Yes |
| `ZTest Always` in `*.shader` | Yes, unless the shader is exempt | Yes, when the material has `_ZTest` == 8 |
| `Queue="Overlay"` and `renderQueue >= 4000` | Yes | Yes, queue ≥ 4000 on a non-post renderer |
| Gameplay queue outside 2000–3999 | Self-check only | Yes |
| Dome ring depth test | `AgrDomeRing.shader` must use `ZTest [_ZTest]` and must not say `ZTest Always` | Yes, role `dome` |
| Canvas sort uses `CanvasOrder` | Source assignments (existing canvas check) plus the whitelist list | Yes, every enabled `Canvas` |
| UI overlap, including glyphs and hangar rows | Self-check only | Yes, all six shots |
| Contact shadow present | Self-check only | Yes. Darkness is separate: luma above 0.06 needs 15% darkening; a darker floor needs the blob renderer enabled and placed |
| Canvas on the UI layer (5) | `GameUi.cs` and `RebindOverlay.cs` must call `UseUiLayer` next to every `AddComponent<Canvas>`. An injected canvas without that call FAILs | Yes, every enabled `Canvas` |
| Capture/UI camera mask is UI-only | `AgrCapture` must set `cullingMask = LayerGate.UiCameraMask` and park the camera at `(0, -10000, 0)`. A mask built from canvas layers FAILs | Yes, the overlay camera used for that shot |
| Decor island within 4 of spawn and no farther than 8 | `ContentFactory` must call `SpawnClear.ClearVisual`. `SpawnClear` must use `IsSpawnClutter` and `IsRingCorner`. `AgrCapture` must call `CollectSpawnIslands`. An injected source missing those calls FAILs | Yes, every shot, from the live blockout mesh |

A runtime hit logs `AgrCapture layer FAIL:` and does **not** stop the remaining shots. The process still exits non-zero (`CaptureContinue.ExitCode` with the layer flag). A fatal capture error (missing session, empty frame) still stops the run.

## Adding a legal object

Add the renderer glob and, if it is not `Mat_*`, the material glob to `Tools/layer_whitelist.json`. World-only props go under `renderers.<world>` or `materials.<world>`. Then run:

```
python3 Tools/validate_week1_project.py
python3 Tools/test_week1_logic.py
```
