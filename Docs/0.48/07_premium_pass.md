# 0.48 premium visual pass

Reconciled against the code on `0.48-premium` (`0be9e27`), not against the raw numbers in briefs 01–06. Locked tables stay the source of truth: `WorldLight`, `PostGrade`, `PlayHudLayout` (420 px / alpha 0.40), `EmissionTargets`, `HangarPreviewRig`, `HitstopBudget`, and `PostGrade.DeathVignetteSeconds` (0.40 s). Deltas live in `PremiumLook` and the call sites.

Albedo ratios below are relative luminance (0.2126, 0.7152, 0.0722) of the material colour under the same light. They are not a rendered measurement. Shadows darken the unlit side of a mesh; the lit face keeps the ratio.

## Chosen vs brief

| Topic | Brief | Chosen | Why |
|---|---|---|---|
| Ambient | −30% | `AmbientMul` 0.70 on combat fill and the bootstrap ambient | Applied at the call site. `WorldLight.Row` is unchanged so its self-check still passes. Hangar ambient `(0.20, 0.17, 0.13)` is not darkened; the studio 3-point is the hangar light. |
| Key | Warm, 35° elevation, soft shadows, steel rim | Elevation 35°, yaw −35°, 45% mix toward `(1, 0.86, 0.62)`, soft shadows, strength 0.65 | Per-world intensities stay in `WorldLight`. Rim and underglow stay `LightShadows.None`. The same `ApplyKey` path also warms the hangar sun. |
| Asteroid albedo | ×1.5 → ×1.2 once the key exists | `AsteroidAlbedoScale` 1.5 kept, times `AsteroidAlbedoKeep` 0.8 → effective ×1.2 | The named constant and the `ReadabilityPalette.AsteroidAlbedoScale` call stay, because tests lock both. |
| Craters | Darker contrast | Variant B extra × `CraterDark` 0.78 | Variant A is the lighter stone. |
| W3/W4 watch lift | (not in the art note) | `WatchAsteroidMul` 1.15 still applies on top | Effective W3/W4 rock is ×1.38 (was ×1.725). Still a reduction, and worlds 3 and 4 stay readable. |
| Floor | Dark matte, smoothness 0.2, light noise | Surface `#0E1520`, metallic 0.04, smoothness 0.2, 64² hash noise (not `RunRng`), 18% mix with the world floor tint | Worlds still differ. W3 is then desaturated (chroma keep 0.60). The 0.40 literal `0.039, 0.063, 0.086` stays in `ContentFactory` as the readability anchor; it is not the drawn albedo. |
| Grid | Alpha ~0.12 steel, fade only at the edge | Visible alpha 0.12. Fade starts at radius 0.78. Declaration `GridAlpha = 0.045f` remains | The 0.045 constant is locked and unused for the drawn grid. Beat pulse still multiplies the visible alpha (`GridAlphaMul` peaks at 1.12 → 0.134). |
| Glass squares | Remove | Layout wash alpha 0.16–0.20 → 0.05. Arena/floor glass and tile names remap to the opaque floor | Ship canopy glass still returns `_glass`. |
| W3 beams | Desaturate green floor and yellow beams 40%. Beams Primary, emission ≤ 1.5 | Floor desat as above. Accent and Glow strips (`Mat_Arena_W3_Accent`, `Mat_Arena_W3_Glow`) use albedo desaturated from the FBX green `(0.20, 0.95, 0.55)` and emission Primary × 1.5 | The FBX beams are green, not yellow. Walls stay on the matte floor material. Emission luma ≈ 0.97, under the bloom threshold 1.10. |
| HUD column | Compact corner, or plates at alpha 0.35 with a 1 px steel border | Ghost plates at alpha 0.35 (the min of 0.35 and the locked 0.40). 1 px Secondary outline. Score Display 28 amber, caption Body 14 | `ColumnMaxX` still caps the corner so the 420 px constant stays live. |
| Bars | Thin, segmented, bottom-center, max 280 px. Amber on key numbers only | `BarSpan` caps at 280 px (also 0.46 of width). Five segments. Fills are Accent / Secondary / Danger. Counts and the big score are amber | Hull fill is no longer Primary. |
| Title | Remove in-combat title and the top info line | `ASTEROIDS GONE ROGUE` and the world badge hide while `Playing`. World, medal, and elite lines are a center toast | Hangar still shows the title and the badge. |
| Abort | Ghost, not filled orange | `ApplyButton(..., danger: true)` stays (locked call). `PaintAbortGhost` paints Surface 0.35, Accent→Secondary label, 1 px steel ring. `UiTheme.DangerTint` stays referenced and is not the plate colour | |
| Death | 0.25 s unscaled hitstop → white-to-Danger flash (0.20, Reduce 0.08) → large explosion + debris → 0.6 s slow-mo + 70% desat + vignette → fail panel 220 ms | Hitstop is its own unscaled arm at `FeelClock` scale 0.02 (not `HitstopBudget`, whose cap is 0.12 s). Flash peaks at 0.20 / 0.08. Explosion is `VfxBudget.Large` at the end of the arm. `DeathSlowSeconds` 0.6, `DeathFadeSeconds` 0.220. Desat moves the grade 70% of the way toward −100 (Reduce 0.28). Vignette target stays 0.45 over the locked 0.40 s | Hitstop duration does not shrink on Reduce. Slow-mo still skips when `AllowsSlowMo` is false. Shatter is marked when the explosion plays, so it does not fire again at the end of the slow-mo. |
| Wave clear | Radial shock, bloom +0.15 for 0.4 s, `VÅG KLAR` / `WAVE CLEAR` at Display 44, score count-up | `VfxKit.Shock` from the ship (0.45 → 7.5 over 0.40 s). Bloom add 0.15, times `PostBloomMul` 0.5 on Reduce. Banner uses `run.wave_clear`. Count-up is display-only over 0.40 s (`UiMotion` ease-out) from the current score to score + the mutator-scaled wave bonus | The real grant still happens in `CompleteWave`. `WaveBreathSeconds` stays 0.25 s. |
| Capture | Pinned death and wave-clear shots show the sequence | Death pin calls `CapturePinDeath` first (finishes the arm, spawns debris), then pins elapsed at half of 0.60 s. `PresentFresh` ages that burst so the still is not a point. Wave-clear is shot index 5 (`waveclear_reduceoff/on.png`) | The death PNG disables `GameUi`, so the hitstop flash (which has already ended at mid slow-mo) is not in that still. Debris, desat, and vignette are. The wave-clear banner is UI and stays enabled. |
| Ship | Hero mesh, `Mat_Ship_AccentHot_Hero`, hull steel, accents amber, no pink | Already the arena path: `TryAdoptHero("Ship_Hero")`, magenta → Primary, cyan → steel. Soft shadows added on the ship, enemies, and asteroids | No second mesh path. |
| Spike | Keep the desaturated look | `EmissionTargets.SniperDesat` / sniper desat left as-is. Hazard spikes stay on `_hazardSpike` | |
| Hangar | Preview ~40%, 1 px steel frame, dark gradient, slow spin, 3-point. Shop: fewer cards, more space, bigger type | Preview width is already 0.424 of the screen (`ShipPreviewMin/Max`). Well has a 1 px steel outline. Backdrop gradient, 18° × `TurntableScale` 0.5, and key 0.85 / rim 0.70 stay | Those rig numbers are locked and already quieter than brief 02 (2.2 / 2.0). Shop grid is unchanged: `hullIndex % 4`, `ShopNameSize` 18, and `HangarPadNav` slot 50 are locked. |
| Outside the dome | Faint nebula, not pure black. Dome rim steel alpha 0.3 | Clear colour `(0.015, 0.022, 0.045)` plus an outer unlit gradient dome. Lip alpha 0.30 | |
| Enemy shots | Brightest small objects | `EnemyShot` stays 9.0, times `EnemyCoreLift` 1.12 | See emission luma below. |

## Estimated contrast

W3 floor tint is palette milli `(20, 300, 220)` × brightness 0.760. W4 is `(420, 220, 20)` × 0.680. Floor colour is surface mixed 18% toward that tint. W3 is then desaturated (keep 0.60).

| Surface | Luma | vs W3 floor (0.097) | vs W4 floor (0.096) |
|---|---|---|---|
| W3 floor | 0.097 | 1 | — |
| W4 floor | 0.096 | — | 1 |
| Asteroid A ×1.2 | 0.396 | 4.06 | 4.14 |
| Asteroid A ×1.2 ×1.15 watch | 0.455 | 4.67 | — |
| Asteroid B ×1.2 ×0.78 | 0.307 | 3.15 | — |
| Asteroid B with watch | 0.353 | 3.63 | — |
| Ship hull `(0.50, 0.58, 0.66)` | 0.569 | 5.84 | 5.95 |

Both floors clear the 2:1 asteroid target and the 2.5:1 ship target on albedo alone.

Emission luma (albedo token × multiplier), same weights:

| Source | Luma |
|---|---|
| Enemy projectile (Danger × 9.0 × 1.12) | 4.20 |
| Brute core (Danger × 9.0, unchanged) | 3.75 |
| Player rail (Primary × 5.0) | 3.23 |
| Enemy body glow (Danger × 7.0) | 2.92 |
| Player bolt (Primary × 4.0) | 2.59 |
| W3 beam (Primary × 1.5) | 0.97 |

Enemy projectiles are the brightest of these. The beam stays under the 1.10 bloom threshold.

Death grade example: a saturation of −12 moves to −73.6 at full desat (70% of the way to −100) and to −36.6 on Reduce.

## Deferred

- Shop grid density, gutters, and `ShopNameSize`. `HangarPadNav.SlotCount` 50 and `hullIndex % 4` stay.
- Hangar well inset (`WellMin` 0.048), spin, and 3-point intensities. The 1 px ring is on the well; the outer bezel is still the existing frame, recolored steel.
- `WorldLight` per-world intensities, `PostGrade` vignette seconds (0.40) and vignette amount (0.45), `HitstopBudget`, and `EmissionTargets` multipliers other than the 1.12 projectile lift.
- Death hitstop stays 0.25 s when Reduce is on. Slow-mo, flash alpha, desat, bloom add, and debris counts still scale.
