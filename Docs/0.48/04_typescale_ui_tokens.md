# 0.48 · 04 Typskala, spacing och motion-tokens (UiTheme)

Idag har `UiTheme` intervall-par (`HeaderMin/Max 28/46`, `BodyMin/Max 14/22`, `HintMin 18`, `PanelPad 16–24`, `ButtonPad 8–12`) och ad hoc-storlekar (`ShopNameSize 18`, `DoctrineCardSize 16`). 0.48 lägger till en fast 4-stegs-skala vid 1080p-referens. Skalning via `UiScreenFit` som idag. Behåll alla gamla konstanter så att inget går sönder, och migrera call-sites gradvis.

## 1. Typskala (lägg till i `UiTheme`)
| Token | px @1080p | Vikt | Line spacing | Används |
|---|---|---|---|---|
| `TypeDisplay` | 44 | Bold, tracking +2% | 1.0 | world-banner, GAME OVER, rubriker |
| `TypeTitle` | 28 | Bold | 1.05 | panelrubrik, kortnamn |
| `TypeBody` | 18 | Regular | 1.15 | brödtext, shop, settings |
| `TypeCaption` | 14 | Medium, versaler, tracking +6% | 1.1 | HUD-etiketter, hints, prompts |
Ratio ≈1.25–1.5. Golv vid 800p (Deck): Caption ≥ 12 px och Body ≥ 15 px (clamp i `UiScreenFit`). HUD-siffror (score, kombo, våg) ska använda Title/Display med monospace-siffror om `UiFonts` har dem, annars `TypeTitle` med fast bredd per box.

## 2. Spacing-tokens (4-px-grid)
`Space1 4`, `Space2 8`, `Space3 12`, `Space4 16`, `Space5 24`, `Space6 32`, `Space7 48`.
Mappning: `ButtonPad` = Space2–Space3, `PanelPad` = Space4–Space5, HUD-marginal mot skärmkant = Space5 (Deck: Space4), avstånd mellan HUD-block = Space3.

## 3. Motion-tokens
| Token | ms | Curve (cubic-bezier → AnimationCurve) | Används |
|---|---|---|---|
| `MotionFast` | 150 | ease-out (0.2,0,0,1) | hover, fokusring, knapptryck |
| `MotionBase` | 200 | ease-out (0.2,0,0,1) | panel in, kort in, tooltip |
| `MotionSlow` | 250 | ease-in-out (0.4,0,0.2,1) | skärmbyte, meny ↔ hangar |
| `MotionExit` | 150 | ease-in (0.4,0,1,1) | panel ut, stäng |
Unity-fri implementering i `UiTheme`: `public static float EaseOut(float t)` och `EaseInOut(float t)` (polynom-approximation, ingen AnimationCurve behövs). All UI-motion går på `Time.unscaledDeltaTime`.
Rörelseomfång: panel in = fade 0→1 + y-offset Space4→0 + skala 0.98→1. Inget studs och ingen overshoot (premium, inte epa).

## 4. Count-up-siffror
- Score, krediter och run-summary räknar upp från gammalt till nytt värde: tid = clamp(0.25 + 0.05·log10(Δ+1), 0.25, 0.6) s, `EaseOut`.
- Vid avslut: skala 1.0→1.08→1.0 över MotionFast, färg kort Primary→Accent.
- HUD-score under strid: om Δ < 50 snappa direkt (inget brus), annars count-up 0.25 s.
- Ljud: inget tick per siffra (det blir kakofoni). Endast `agr_ui_confirm` vid run-summary-total (se 05).

## 5. `UiTransition` (GameBots nya klass) – durations
| Övergång | Ut | In | Overlap | Kurva |
|---|---|---|---|---|
| Meny → meny (settings, credits) | MotionExit 150 | MotionBase 200 | 50 ms | ut ease-in, in ease-out |
| Meny ↔ hangar, start-run | MotionSlow 250 fade till `UiTheme.Void` | MotionSlow 250 | 0 (svart hålls 60 ms) | ease-in-out |
| Våg klar → boon-kort | – | MotionBase 200 per kort, stagger 60 ms | – | ease-out |
| Död → run-summary | se 05 (slow-mo 0.6 s), sedan MotionSlow 250 | MotionSlow 250 | – | ease-in-out |
Reduce effects: behåll fade men ta bort skala och y-offset, och sätt stagger till 0. Durations oförändrade.
Ljud: `UiTransition` triggar `agr_ui_open` / `agr_ui_close` vid start av "in"/"ut" (se 05).
