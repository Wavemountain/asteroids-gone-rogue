# 0.48 · 06 Arenadjup

Det som finns: `ArenaEnv` med dome-lagren `StarFar` (r×8.8), `StarMid` (×7.5), `StarNear` (×6.4), `NebulaDome`, `NebulaInner`, `Mat_Env_Grid` (golvgrid, queue 2900), belt, `Scroll(material,x,y)`-hjälpare och `DecorCameraStack` (decor-layer).

## 1. Parallax-stjärnor
Det är domes, så parallax görs som UV-offset relativt kamerans position: `offset = cam.xz * k`.
| Lager | k (UV per världsenhet) | Drift (UV/s) | Alfa |
|---|---|---|---|
| StarFar | 0.0004 | 0.0010, 0 | idag |
| StarMid | 0.0010 | 0.0016, 0.0004 | idag |
| StarNear | 0.0022 | 0.0024, 0.0008 | idag |
Görs i `ArenaEnv.LateUpdate` via `Scroll`. Reduce: k ×0.5, drift ×0.5.

## 2. Nebulosa-drift
`NebulaDome` 0.0020 UV/s och `NebulaInner` −0.0012 UV/s (motsatt riktning ger djup). Långsam alfa-andning ±0.03 över 24 s (sinus). Reduce: drift ×0.5, ingen andning.

## 3. Närkameradamm
Ny `ParticleSystem` "NearDust" på decor-layern mellan kamera och spelplan. 40 partiklar (Deck: 30), size 0.03–0.06, färg `UiTheme.Accent #C8CED6` α0.10–0.18, livslängd 6 s, rör sig med −0.15× kamerahastigheten (parallax framför). Material `Particles/Standard Unlit` additivt. Får **aldrig** vara ljusare än α0.18 och inga partiklar får ligga inom 1.5 enheter från skeppet (fade-near), så att de inte kan förväxlas med projektiler. Reduce: 12 partiklar, α ×0.6.

## 4. Golvgrid-puls
`Mat_Env_Grid` α pulserar på takten: `GridAlpha * (1 + 0.12 * pulse)`, där `pulse = exp(-8 * fasefrac)` (kort attack, mjukt släpp). Bara på slag 1 av 4 (beat 1) med 0.12, de andra slagen 0.04. BPM är fast per spår i ny `MusicPlan.WorldTrackBpm[]` (index = värld 1–7, boss). Värdena är **inte uppmätta**, så sätt 0 tills de är verifierade. 0 betyder ingen puls, och då ska `MusicPlan` falla tillbaka till en statisk grid. Fasen tas från `_music.timeSamples / clip.frequency` för det dominerande spåret (följer crossfade-vikten). Reduce: amplitud 0, alltså statisk grid. Rytmisk blink är just vad Reduce ska ta bort.
Ett BPM-mätverktyg (ffmpeg onset + autokorrelation) kan AtmosBot köra i nästa steg på `world1_launch_belt…world7_spoke_ring` och `boss_guardian`. Det ingår inte i den här leveransen.

## 5. Läsbarhet
Ingen djupeffekt får hamna i projektil- eller fiendefärgerna (Primary/Danger). Damm och grid är Accent eller Steel med låg alfa. Kör `tools/contrast_worlds.py` efter ändring av grid-alfa.
