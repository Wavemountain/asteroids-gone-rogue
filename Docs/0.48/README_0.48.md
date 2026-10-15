# AtmosBot 0.48 premium-pass – wire-briefs till GameBot
Bas `main 7a64598` (Unity 6000.6.0f1, **Built-in RP**, ingen URP, ingen PPv2 i manifest). Allt läses via gh api, ingen push.
Regler: allt skalas av `SettingsState.ReduceEffects` (`agr.settings.reduceEffects`) via `EffectScale`, och läsbarheten går först. Fiender och projektiler behåller högst kontrast, och det som är läsbarhet (grade, rim, emission) dämpas aldrig av Reduce. Editor-compile på HEVONEN är gaten.

| Fil | Innehåll |
|---|---|
| `01_post_stack.md` | egen OnRenderImage-stack (bloom/LUT-grade/vinjett/grain), värden W1–7 + hangar, Reduce, blend, Deck ≤1.2 ms, **emissionsmål per material** (§9), före/efter-mål (§8) |
| `02_lighting_rim_key.md` | key/rim/fill per värld, `AGR/RimLit` Fresnel-shader, projektil-ljus max 4, hangar 3-punkt + bounce |
| `03_vfx_language.md` | VFX-tokens, explosion S/M/L, mynning per `FireMode`, hit/sköld/pickup, Reduce |
| `04_typescale_ui_tokens.md` | 4-stegs typskala, spacing, motion 150–250 ms, count-up, `UiTransition`-tider |
| `05_audio_death_win_menu.md` | 7 nya SFX, död/vinst/meny-sekvenser, musik-low-pass, voice-limit. Crossfade-resterna är redan fixade |
| `06_arena_depth.md` | parallax, nebulosa-drift, närdamm, grid-puls (BPM = 0 tills mätt) |
| `Assets/Resources/Shaders/AgrRimLit.shader` | surface shader, fallback Standard |
| `Assets/Resources/Audio/Sfx/agr_*.ogg` | 7 st, OggS, CC0. `CREDITS_0.48.txt` |
| `tools/grade_check.py`, `tools/contrast_worlds.py` | läsbarhetskontroll av graden |

## Föreslagen PR-ordning
1. **P-emission + rim** (02 §2 + 01 §9): `EmissionTargets.cs`, `AgrRimLit`, override per materialnamn. Ger störst synligt lyft och ingen risk.
2. **P6 post-stack** (01): `PostStack.cs` + `AgrPost.shader` + `PostGrade.cs`, med `enabled=false`-brytare.
3. **Ljus per värld + hangar-studio** (02 §1, §3, §4).
4. **VFX-pool** (03): `VfxKit.cs`, mynning, explosioner.
5. **Ljud** (05): SFX-filer, low-pass, `CueVoiceBudget`-kategorier. Samordna med GameBots död/`UiTransition`.
6. **UI-tokens** (04) parallellt med GameBots `UiTransition`.
7. **Arenadjup** (06). Grid-puls sist, efter BPM-mätning.
