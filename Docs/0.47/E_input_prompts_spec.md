# E — Knappikoner per kontrolltyp (0.47)

Källa: Kenney **Input Prompts 1.5A** (CC0, `License.txt` verifierad i paketet, nedladdat från kenney.nl). Alla ikoner är vita/neutrala, 64 px (`Default`) + 128 px (`Double`, i `hi/`). Inget ligger i repot ännu — kopiera `Assets/Resources/UI/InputPrompts/` rakt in.

## 1. Spelets actions (läst ur `GamepadInput.cs`, `ProjectSettings/InputManager.asset`, `ShipController`, `GameUi`)

Legacy Input Manager, joystick-knappar numrerade Xbox-style. **Ingen dash/boost finns** — bara dessa:

| Action (filnamn) | Tangentbord/mus | Xbox | PlayStation | Steam Deck |
|---|---|---|---|---|
| `move` flyg | WASD (piltangenter fungerar också) | LS | L-stick | vänster stick |
| `aim` sikta | mus | RS | R-stick | höger stick |
| `fire` primär (håll = Rail-laddning) | LMB (Space/LCtrl alt) | RT | R2 | R2 |
| `fire_alt` | Space | A (`FirePad`, knapp 0 skjuter också) | Kryss | A |
| `utility` | RMB (E alt) | LT | L2 | L2 |
| `cycle` byt primärvapen | Q | LB (btn 4) | L1 | L1 |
| `cycle_prev` | – | RB (btn 5) | R1 | R1 |
| `cycle_alt` | – | X (btn 2) | Fyrkant | X |
| `pause` / start wave / tillbaka till hangar | Esc | Start/Menu (btn 7) | Options | Options (☰) |
| `settings` | F1 | View (btn 6) | Create | View |
| `confirm` (meny, doktrin/boon-val) | Enter | A (btn 0) | Kryss | A |
| `cancel` | Esc | B (btn 1) | Cirkel | B |
| `nav` meny/hangar/doktrin | piltangenter (+ mus) | D-pad (btn 11–14, axel 6/7) eller LS | D-pad | D-pad |

`utility_alt` (keyboard only) = E. Gamepad-nav kan också göras med LS; visa D-pad som primär.

## 2. Mappning action → källfil (alla i `Default/`, kopierade 1:1 om inget annat anges)

| Action | xbox | playstation | deck | keyboard |
|---|---|---|---|---|
| move | `xbox_stick_l` | `playstation_stick_l` | `steamdeck_stick_l` | **sammansatt** W/A/S/D från `keyboard_w/a/s/d` (3×2 tiles, PIL) |
| aim | `xbox_stick_r` | `playstation_stick_r` | `steamdeck_stick_r` | `mouse_move` |
| fire | `xbox_rt` | `playstation_trigger_r2` | `steamdeck_button_r2` | `mouse_left` (**omfärgad**, se 4) |
| fire_alt | `xbox_button_a` | `playstation_button_cross` | `steamdeck_button_a` | `keyboard_space` |
| utility | `xbox_lt` | `playstation_trigger_l2` | `steamdeck_button_l2` | `mouse_right` (**omfärgad**) |
| utility_alt | – | – | – | `keyboard_e` |
| cycle | `xbox_lb` | `playstation_trigger_l1` | `steamdeck_button_l1` | `keyboard_q` |
| cycle_prev | `xbox_rb` | `playstation_trigger_r1` | `steamdeck_button_r1` | – |
| cycle_alt | `xbox_button_x` | `playstation_button_square` | `steamdeck_button_x` | – |
| pause | `xbox_button_menu` | `playstation5_button_options` | `steamdeck_button_options` | `keyboard_escape` |
| settings | `xbox_button_view` | `playstation5_button_create` | `steamdeck_button_view` | `keyboard_f1` |
| confirm | `xbox_button_a` | `playstation_button_cross` | `steamdeck_button_a` | `keyboard_enter` |
| cancel | `xbox_button_b` | `playstation_button_circle` | `steamdeck_button_b` | `keyboard_escape` |
| nav | `xbox_dpad` | `playstation_dpad` | `steamdeck_dpad` | `keyboard_arrows` |

Filer (51 st @64 px + 51 st @128 px):
```
Assets/Resources/UI/InputPrompts/{xbox,playstation,deck,keyboard}/<action>.png      (64 px)
Assets/Resources/UI/InputPrompts/{xbox,playstation,deck,keyboard}/hi/<action>.png   (128 px, för 4K/Deck-skalning)
```
Samma action-namn i alla scheman → koden slår bara upp `"UI/InputPrompts/" + scheme + "/" + action`. Saknad fil (t.ex. `cycle_prev` på tangentbord) = visa ingen glyf/fallback till text.

## 3. Wiring (kort)

1. **Schema från senast använda enhet.** Håll `static InputScheme Current` (Keyboard/Xbox/PlayStation/Deck) i t.ex. `GamepadInput`. Varje frame:
   - `Input.anyKeyDown` (utan joystick-knapp) eller `GamepadInput.MouseDelta() > 0` / musknapp → `Keyboard`.
   - joystick-knapp/axel > 0.5 (`KeyCode.JoystickButton0..19` eller trigger/stick-axlar) → pad-schemat (se 2).
   - Ändra bara vid byte; skicka event `OnSchemeChanged` så HUD/hint-texter ritas om. Debounce: ignorera pad-axel-brus < 0.3 så en stillastående stick inte flippar schemat.
2. **Pad-typ.** `Input.GetJoystickNames()` innehåller "Wireless Controller" / "DualSense" / "DualShock" / "PS4" / "PS5" → `playstation`. Steam Deck/Steam Input rapporterar sig som Xbox 360 → `deck` om `SteamUtils.IsSteamRunningOnSteamDeck()` (Steamworks.NET/Facepunch) eller env `SteamDeck=1`; annars `xbox`. Spara override i `SettingsState` (Auto/Xbox/PlayStation/Deck) om ni vill ge spelaren ett val (valfritt).
3. **Hint-strängar.** Bokstavliga "RT / LT / LB / A / B / Start / Esc / WASD / LMB / E / Q" i `GameUi.cs` (~246–255, 613, 2357) och `Loc.cs` (24, 53, 65, 81–87, 145–187, 219–220, 385) ersätts med token, t.ex. `{fire}` `{utility}` `{cycle}` `{confirm}` `{cancel}` `{pause}`. En `PromptText.Resolve(string)` splittar texten och lägger en inline-`Image` (16–20 px hög, `UiTheme.Accent #C8CED6`-tint tillåten) per token; rich-text `<sprite>` kräver TMP-atlas — om UI är uGUI-text, bygg små `Image`-barn.
4. **Import.** Texture Type = *Sprite (2D and UI)*, filter Bilinear, mipmaps av, `alphaIsTransparency` på, ingen komprimering för 64 px. Ikonerna är vita → tinta vid behov med UiTheme-token (`Accent` normalt, `Primary #D4A04A` på aktiva/viktiga prompter, `Disabled` när action är låst).
5. `Resources.Load<Sprite>` cachas i en `Dictionary<string,Sprite>`; scheman växlar sällan.

## 4. Omfärgning (gjord)
`mouse_left`/`mouse_right` har i originalet röd markering (231,50,70) — off-palette. I `keyboard/fire.png` och `keyboard/utility.png` (64 + hi) är den röda ersatt med **Primary `#D4A04A`** (alfa/kantutjämning bevarad). Övriga ikoner orörda (vita).

## 5. CREDITS (kopiera till projektets CREDITS)
```
Input prompts: "Input Prompts" 1.5A by Kenney (www.kenney.nl) — CC0 1.0. Used: Xbox Series, PlayStation Series, Steam Deck, Keyboard & Mouse sets; keyboard W/A/S/D composite and amber recolour of mouse buttons are derivatives made for this project.
```

## 6. Caveats
- Ej testat i Unity/på riktig hårdvara; PS/Deck-mappningen bygger på att Steam Input/PS-pad exponerar Xbox-numrering (som `GamepadInput` redan antar).
- PS "Create" som View-motsvarighet och Options som Start — standard, men verifiera på en riktig DualSense.
- Ikonernas konturvarianter (`*_outline`) finns i paketet om ni vill ha dem på ljus bakgrund; ej kopierade.
