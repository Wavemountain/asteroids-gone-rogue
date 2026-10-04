# AtmosBot 0.47 – leverans till GameBot (B → C → E)

Kopiera in i repot: `Assets/Resources/Audio/Sfx/*.ogg` (se B §1) och `Assets/Resources/UI/InputPrompts/`. **Inte** `_src/` (ögonblicksbild av main @ 8d86a3f) eller `_work/` (scratch).

| Del | Fil | Innehåll |
|---|---|---|
| B | `B_hit_feedback_spec.md` | hit/sköld/pickup/kill-feedback, 5 telegrafer, röst-/cooldown-regler, C#-konstanter, flaggade fel |
| B | `Assets/Resources/Audio/Sfx/agr_*.ogg` (17 st) | nya CC0-SFX (OggS) |
| C | `C_palettes_contrast_reduce_mix.md` | 7 världar, kontrast + färgblind, Reduce effects, mix/duck/slider |
| D | `D_display.md` | upplösning/Steam Deck, fönster/VSync/FPS, letterbox |
| C | `tools/contrast_worlds.py` + `contrast_tables.generated.md`, `contrast_min.json` | reproducerbara mått |
| E | `E_input_prompts/E_input_prompts_spec.md` | actions, mappning, wiring, credits |
| E | `Assets/Resources/UI/InputPrompts/{xbox,playstation,deck,keyboard}/` | 51 ikoner @64 px + `hi/` @128 px |
| – | `CREDITS_0.47_fragment.md` | credits-rader |
| – | `tools/bake.py`, `measure.py`, `ladder.py`, `icons.py` | hur filerna gjordes |
