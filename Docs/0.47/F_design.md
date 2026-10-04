# 0.47 Part F — variety and late game

Design for F1 hangar preview, F2 daily seed runs, F3 mutators, and F4 credit sinks. Implementation is split across PRs. This note is the contract those PRs follow.

Base behaviour stays the default. No mutators, no daily seed, and no purchased sinks means the same shop prices, the same earnings, and the same wave-35 credit ratios (Easy 84.7 / Normal 75.5 / Hard 64.1, catalogue 9883). Assist runs stay off every board.

## F1 — Hangar preview

The studio camera, FOV 40, pose `(0.2, 4.55, -10)` looking at `(0, 0.08, 0)`, showcase scale 1.25, `ShipPreviewCanvas` sort `CanvasOrder.ShipPreview` (80), and both `RectMask2D` clips stay as they are. The RenderTexture stays 768×960 on layer `HangarPreview` (8).

`HangarPreviewRig` (Unity-free) decides the motion and the lights:

- Turntable is half of the old 18°/s idle (`9°/s`) so a full turn takes about 40 seconds.
- Pointer over the viewport, or a shop hover / pad focus that ghosts a part, pauses the spin.
- Reduce effects freezes the spin. Lights stay on and steady. No new pulses.
- Key light is warm and in front; fill is dim and cool; rim is a cooler light behind the hull so nose, body, and engine separate from the studio clear.
- Installed nose / engine / overcharger / afterburner tints use a property block (shared materials are not rewritten). Any Mk II bit shows a small steady amber marker. Equipped bay paint and trail show on the same preview. A future mesh pass is specified in `F_hangar_model_brief.md` and is not required to run.

A fit check uses the locked camera and the portrait render target. A hull sphere of radius 2.5 sits inside both frustum halves.

## F2 — Daily seed runs

`DailySeed.ForDate(year, month, day)` is a pure function of the UTC calendar date. The same date always yields the same positive seed. A different date yields a different seed.

At New Run the player can pick **Daily Run** or a normal run. The daily seed is the gameplay seed for that UTC day. It is stored on the run (`RunSave` version bump) so Continue keeps it. `RunId` stays the legacy counter. A normal run leaves the daily seed at 0 and keeps today's spawn angles, boon mix, and `UnityEngine.Random` drops.

Seeded when the daily seed is non-zero:

- Boon offers (`BoonCatalog.Draw` already takes a seed; the daily seed replaces the run id for that draw).
- Spawn ring phase (rocks, roster, extras, boss, elite). Unseeded angles stay the current formulas.
- Pickup drops, extra-life rolls, and asteroid visual picks.

Not deterministic, even on a daily run: audio pitch, camera shake, asteroid tumble, enemy aim wander, seeker lead, and VFX. Those stay on `UnityEngine.Random`.

Local daily board is a separate store (`agr.daily.board`), not the normal `LocalBest` keys. One best score and best wave per UTC date, cap 32 days, oldest dropped. Assist runs are not recorded. A corrupt blob loads as an empty board. The HUD, hangar status, and death card show the date and seed while a daily run is active.

## F3 — Mutators

At New Run the player may toggle 0, 1, or 2 mutators, then confirm. The chooser is pad-navigable (D-pad / stick, `{confirm}` / `{cancel}`) and mouse-clickable. Copy is EN and SV, icon-free (bullet U+2022 is allowed).

| Id | Name | Rule | Score | Credits |
|---|---|---|---|---|
| 0 | Glass Cannon | Player and enemy damage ×1.5 | ×1.15 | ×1.00 |
| 1 | Swarm Season | More swarmlings, fewer heavies | ×1.10 | ×1.00 |
| 2 | Heavy Rocks | Asteroids +50% HP, one extra split | ×1.10 | ×1.00 |
| 3 | Quiet Space | Fewer pickups, credit rewards ×1.25 | ×1.00 | ×1.25 |
| 4 | Overclock | Enemy fire cooldown ×0.80 | ×1.20 | ×1.00 |
| 5 | Long Haul | Roster uses the next wave rung | ×1.10 | ×1.00 |

Swarm Season is incompatible with Heavy Rocks. Glass Cannon is incompatible with Quiet Space. Stacked score multipliers multiply, then clamp to 1.00–1.60. Credit multipliers clamp to 1.00–1.40. Zero mutators leaves every multiplier at 1.

`RunSave` stores `MutatorMask`. Older versions migrate to 0. A mask with a forbidden pair or more than two bits fails validation and the run loads as none. Mutator runs set a flag and do **not** write the normal highscore board (same exclusion as assist). There is no separate mutator board in this pass.

## F4 — Credit sinks after Mk II

Sinks are not `ShopCatalog` rows. The 9883 catalogue and every existing sticker stay put. They live in `ShopSinkCatalog` and show as a **Bay** cell in the empty hull slot after Overcharger / Afterburner (the Mk II hull tiles), opening an overlay on `CanvasOrder.Overlay`.

| Id | Kind | Price | Cap |
|---|---|---|---|
| 0 Amber hull | cosmetic, equip | 420 | own once |
| 1 Steel hull | cosmetic, equip | 420 | own once |
| 2 Cyan trail | cosmetic, equip | 380 | own once |
| 3 Amber trail | cosmetic, equip | 380 | own once |
| 4 Start shield | next run +1 shield cell | 640 | 1 |
| 5 Pickup reach | next spawn radius +5% per rank | 520, then 980 | 2 |

Prices are never negative. The reach curve is strictly increasing. At the cap the tile reads **CAPPED**, not "Mk II  MAX". Owned cosmetics read **OWNED**; the equipped one reads **FITTED**. Re-selecting an owned paint or trail is free and only changes the equipped id.

Profile JSON (`agr.sink.profile`, version 1) is separate from the run file: `OwnedMask`, `Paint`, `Trail`, `ShieldRank`, `ReachRank`. Missing key = nothing owned. Unreadable blob = empty profile in memory (the blob is left on disk if the version is newer). Run-start perks apply in `ApplyLegacyToNewRun` only. Rank 0 adds no shield and a reach multiplier of 1, so a profile with nothing purchased does not move combat or the wave-35 ratios.

## Save format

| Store | This pass | Migration |
|---|---|---|
| Run file | F2 adds `DailySeed` + `DailyDate` (version +1). F3 adds `MutatorMask` (version +1). | Missing fields become 0. |
| `agr.sink.profile` | New in F4, version 1. | Absent key = empty. |
| `agr.daily.board` | New in F2, version 1. | Corrupt = empty board. |
| `agr.best.*` | Unchanged. | Assist, daily, and mutator runs do not write it. Daily has its own board. |

## What is tested vs not

**Tested without Play Mode:** preview spin / pause / reduce freeze, frustum fit, sink prices, caps, monotonic reach curve, purchase and equip rules, profile codec (round-trip, missing, corrupt, newer version), sink pad grid, bay cell inside the hangar and clear of the bank button, EN/SV lines inside the overlay from 1280×800 through 3440×1440 at font 18, catalogue prices unchanged, existing `LayoutSelfCheck` strings and gear row coords, loc parity (`audit_loc.py`).

**Not tested here:** Unity Play Mode, a real gamepad, the studio lights on a GPU, FBX material emission, or `Screen` layout on a Deck. Roslyn does not compile `GameUi.cs` or `GameManager.cs`; those edits are read back line by line.
