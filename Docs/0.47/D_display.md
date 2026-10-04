# 0.47 Part D — resolution, Steam Deck, window settings

Settings version is **5**. A save older than 5 still loads. Display keys are ignored until that migration and the launch defaults are written on the next save. Defaults keep the previous behaviour: borderless, VSync off, FPS cap 60. The stored windowed size starts at 1920×1080 and is used only in windowed mode.

## Resolution matrix

Checked in `Tools/test_week1_logic.py` (`test_display_047d` and the settings shell) for both English and Swedish. The Steam Deck size in that matrix is 1280x800:

| Size | Aspect |
|---|---|
| 1280×800 | Steam Deck / 16:10 |
| 1366×768 | laptop 16:9 |
| 1440×900 | 16:10 |
| 1920×1080 | reference |
| 1920×1200 | 16:10 |
| 2560×1080 | ultrawide |
| 2560×1440 | 16:9 |
| 3440×1440 | ultrawide |

The canvas scaler stays 1920×1080, match 0.5. On-screen size is `fontSize × canvasScale`. At 1280×800 the scale is about 0.70, so canvas font **18** is about **12.6 px**. Font 14 is about 9.8 px and fails that floor.

## What the tests check

- Each settings row, when focused, sits fully inside the masked viewport (`SettingsScroll.ContainsBand`). Music, SFX, and Reduce effects are fully inside at focus 0 on every matrix size, including the sliders.
- Reduce-effects copy (EN and SV) wraps inside the row at font 18. VSync and FPS value strings stay on one line.
- Confirm body and buttons, first-start choice labels, the death-card summary, the abort button, and credits body use font 18.
- `UiScreenFit` is the pure inside-screen / font-floor helper. `ArenaFraming.Viewport` keeps a 16:9 frustum on every matrix size.
- `DisplaySettings` clamps window mode, resolution, VSync, and FPS. An unknown mode or cap falls back to the defaults. A windowed size missing from the machine list falls back to the current size, then to 1920×1080. An empty list (editor) accepts the request.
- Old settings versions 1–4 do not apply stored display ints.

## Settings scroll

Eighteen rows do not fit in the panel at font 18, so the list scrolls. Row height is 0.076 of the panel (tall enough for two lines of font 18 on 3440×1440). The controls block is a 0.26 section. Pad up/down moves `SettingsRows.Move` and `ShiftForFocus` slides the focused row fully into the viewport. Stick and D-pad both feed that move. Left/right nudges the focused value, including Reduce effects and the two sliders. B / Esc / Start closes the overlay. Focus uses `UiTheme.SetPadFocus`. Rows outside the mask are not interactable.

## Display rows

Window (Windowed / Borderless / Fullscreen), resolution (windowed picker), VSync, FPS cap (30 / 60 / 120 / Uncapped). Applied immediately by `DisplayRuntime.Apply`: `Screen.SetResolution` with `FullScreenMode`, `QualitySettings.vSyncCount`, and `Application.targetFrameRate`. Uncapped writes −1. VSync on sets `vSyncCount` to 1. Unity then ignores `targetFrameRate`; the row says the cap is ignored. `Time.captureFramerate` is not used.

Borderless and exclusive fullscreen use `Screen.currentResolution` when it is at least 640×480, so a Steam Deck stays 1280×800 instead of being forced to 1920×1080. The resolution picker is the windowed size only.

## Arena framing

Vertical FOV stays **54**. `ArenaFraming` keeps a 16:9 frustum:

- Wider than 16:9: pillarbox. The camera rect is narrower and centered. Side bars are outside the play camera, so ultrawide does not show extra arena.
- Taller than 16:9 (including 1280×800): letterbox. The rect is shorter and centered, so the playfield is not cropped.
- Exactly 16:9: the rect is full screen.

The decor camera copies the play camera rect in `LateUpdate` (execution order 20000), otherwise the side bars would show extra scenery. UI canvases are screen-space overlays and stay full screen. Store capture hold resets the play rect to full screen so the capture is not letterboxed.

## Known sizes the tests do not raise

Shop hull names stay at `ShopGridLayout.HullFont` 14. The hull cell fails a two-line wrap at font 18, and the shop width test locks 14. On the Deck that is about 9.8 px. Boon card copy may shrink to `BoonCardLayout.MinFont` 10 when the card text does not fit at 16. Those cells are unchanged.

## Verified by tests vs by hand

**Tests (this repo, no Play Mode):** layout math above, display clamp/fallback, settings version migration, music same-clip hold, assist absorb decision, contrast floors after the W3/W4 bump, localisation parity via `audit_loc.py`.

**Hand only:** a real Steam Deck, `Screen.SetResolution` on a desktop, pad focus chrome in Play Mode, and that the letterbox bars match the camera background. This environment does not run the Unity editor.
