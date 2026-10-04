# 0.47 Part E — input prompts and rebinding

## E1 button prompts

Merged on main before this part. Scheme detection, the prompt catalog, `{token}` hints, and settings version 6 (icon scheme) are covered by `test_input_prompts_047e`.

**Verified by tests:** scheme choice, catalog presence, token split, settings v6 migration, hint width on the resolution matrix.

**Not verified here:** a real gamepad flipping the scheme, or the Kenney sprites in Play Mode. This environment does not run the Unity editor.

## E2 rebinding

Settings version is **7**. A save older than 7 still loads its other settings. Bindings on those saves are ignored and the defaults are written the next time settings save. A missing or corrupt binding blob also loads the defaults.

Rebindable actions: move (four keys), fire, alt fire, utility, cycle, cycle back, alt cycle, pause, confirm, cancel. Keyboard/mouse and gamepad buttons are separate. Sticks and triggers stay on the Input Manager axes. Escape always pauses and cancels and cannot be assigned to another action. Assigning a control that another action already uses swaps the two.

The controls list is a row under button icons in settings. It has its own canvas sort (`CanvasOrder.Rebind`, above the settings overlay). Confirm or a click listens for five seconds. Escape or cancel aborts. A timeout returns to the list. Reset and Back are on the list. The list scrolls with pad focus. There is no pulse on the new rows.

Default bindings keep the previous fire, utility, cycle, pause, confirm, cancel, and move behaviour. Move uses Horizontal/Vertical while the four keys are still WASD, so the arrow keys stay in that path.

**Verified by tests:** default move and fire equivalence, swap, Escape reserved, corrupt blob, saves older than v7, prompt text after a rebind, English/Swedish key parity, and line width from 1280×800 through 3440×1440. `python3 Tools/validate_week1_project.py`, `python3 Tools/test_week1_logic.py`, and `python3 Tools/audit_loc.py`.

**Not verified:** Play Mode capture on a keyboard or a real pad, focus chrome on the controls list, or that a rebound key survives a domain reload in the editor. This environment does not run the Unity editor.
