# 0.48 · 01 Post-stack per värld (wire-brief till GameBot)

Bas: `main 7a64598`. Unity `6000.6.0f1`.

## 0. VIKTIGT: projektet kör INTE URP
`Packages/manifest.json` saknar `com.unity.render-pipelines.universal`. Det finns ingen pipeline-asset och ingen Volume. All kod använder `Shader.Find("Standard")`, `"Unlit/Color"`, `"Particles/Additive"` osv. (built-in). URP-API:er (`UnityEngine.Rendering.Universal.Bloom`, `Volume`) **kompilerar inte**. Ett byte till URP i 0.48 gör alla runtime-material rosa och är en egen stor migrering. Det avråds.

**Beslut (efter GameBots korrigering):** PPv2 finns **inte** i `Packages/manifest.json`. Huvudvägen är därför GameBots egen stack i P6: `Assets/Scripts/Content/PostStack.cs` på gameplay-kameran, med `OnRenderImage(src, dst)` och `Graphics.Blit` mot en shader `Assets/Resources/Shaders/AgrPost.shader` (built-in, `Shader.Find("AGR/Post")`). Alla värden nedan är skrivna för den stacken, och §10 har exakt mappning. Alternativ, om ni hellre vill ha ett paket: lägg till `com.unity.postprocessing` 3.4.0 (released för 6000.x). Typerna mappar 1:1 (§10 sista tabellen), men då måste `Tools/roslyn/UnityStub.cs` få stubbar. Inga URP- eller Volume-API:er får användas någonstans.

## 1. Koppling
- Ny `Assets/Scripts/Content/PostStack.cs` (MonoBehaviour), byggs i kod precis som `ArenaEnv.Ensure`.
- `PostStack` (OnRenderImage) sitter på **gameplay-kameran** (depth 0, sist i `DecorCameraStack`, `clearFlags=Depth`), så den processar decor och spel i samma buffer. Lägg den INTE på decor- eller backdrop-kameran, annars blir det dubbel bloom och dubbel grade. Ingen ny layer behövs. Verifiera i Editor att decor-lagret syns under effekten, eftersom built-in-stacking med HDR kan ge varje kamera ett eget target. Om decor försvinner: lägg en `CommandBuffer` på `CameraEvent.AfterEverything` på gameplay-kameran i stället.
- `gameplay.allowHDR = true` krävs för bloom-threshold > 1. `DecorCameraStack` kopierar redan `allowHDR` till decor-kameran. Kolla också `ProjectSettings/GraphicsSettings` HDR-tier (standard är på).
- Värdedata lever Unity-fritt i ny `Assets/Scripts/Core/PostGrade.cs` (`static PostGradeRow Row(int world, bool reduce)`), i samma stil som `WorldRules`/`EffectScale`. `PostStack` översätter raden till profilen. Testbar i roslyn.
- Värld 8+ använder `WorldCatalog.LayoutNumber(world)`.
- HUD/UI: Canvas i Screen Space Overlay påverkas inte av post. Det är meningen, HUD ska förbli ograderad och skarp. Kontrollera att ingen canvas står i Screen Space Camera på gameplay-kameran.

## 2. Gemensamt för alla världar
| Effekt | Inställning |
|---|---|
| Bloom | `threshold 1.10` (linjär HDR, alltså bara emission: `EffectScale.MeshEmissionFull 1.15`, projektiler, telegraf). `softKnee 0.35`, `clamp 8`, `fastMode true`, `anamorphicRatio 0`, `color` vit, ingen dirt-texture |
| ColorGrading | `gradingMode HDR`, `tonemapper Neutral` (inte ACES, eftersom ACES drar amber mot gult/orange och ger epa) |
| Vignette | `mode Classic`, `color #070B12` (`UiTheme.VoidHex`), `center (0.5,0.5)`, `rounded false` |
| Grain | `colored false`, `size 0.9`, `lumContrib 0.85` (grain mest i mörkret, ren i ljuset) |
| Av | AO, SSR, DoF, Motion Blur, Chromatic Aberration, Lens Distortion, Auto Exposure (ger fladder vid explosioner) |

## 3. Värden per värld (Full)
Palettstämning hämtad från `WorldRules.PaletteMilli`: W1 kallblå, W2 violett, W3 teal, W4 amber, W5 skiffer, W6 rost/röd (dim), W7 magenta.
Strategin är "desaturated void": mättnaden dras ned så att Danger-orange (`#B85A28`) och Primary-amber (`#D4A04A`) blir de enda starka färgerna. Lift kyls mot stål, gain värms lätt.

| W | Namn | Bloom int. (§9) | Diffusion | postExposure | temp | tint | saturation | contrast | lift (rgb,w) | gamma (rgb,w) | gain (rgb,w) | Vign. int./smooth | Grain int. |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 1 | Launch Belt | 0.60 | 6.0 | +0.10 | -4 | 0 | -12 | +8 | (0.98,1.00,1.03,-0.010) | (1,1,1,0) | (1.02,1.00,0.97,+0.02) | 0.28/0.40 | 0.12 |
| 2 | Deep Orbit | 0.65 | 6.0 | +0.15 | -2 | +3 | -18 | +8 | (1.00,0.98,1.03,-0.010) | (1,1,1,0) | (1.03,1.00,0.96,+0.02) | 0.30/0.40 | 0.12 |
| 3 | Far Drift | 0.60 | 5.5 | +0.05 | 0 | -2 | -20 | +10 | (0.98,1.01,1.02,-0.015) | (1,1,1,0) | (1.02,1.00,0.97,0.00) | 0.26/0.38 | 0.10 |
| 4 | Mine Fields | 0.60 | 5.5 | 0.00 | -6 | 0 | -25 | +10 | (0.97,1.00,1.04,-0.015) | (1,1,1,0) | (1.00,1.00,1.00,0.00) | 0.26/0.38 | 0.10 |
| 5 | Cross Gates | 0.65 | 6.0 | +0.20 | -3 | 0 | -10 | +6 | (0.98,1.00,1.03,-0.005) | (1,1,1,0) | (1.03,1.01,0.97,+0.03) | 0.30/0.42 | 0.14 |
| 6 | Debris Islands | 0.60 | 6.0 | +0.15 | -8 | 0 | -25 | +8 | (0.97,1.00,1.04,-0.010) | (1,1,1,0) | (1.02,1.00,0.98,+0.02) | 0.24/0.38 | 0.12 |
| 7 | Spoke Ring | 0.65 | 6.0 | +0.10 | -2 | -3 | -20 | +8 | (0.99,1.00,1.03,-0.010) | (1,1,1,0) | (1.03,1.00,0.97,+0.02) | 0.30/0.40 | 0.12 |

Kommentarer:
- W4 och W6 får kallare `temp` och mest desat, eftersom golven där är amber eller rost och annars konkurrerar med Danger-fiender.
- W3 och W4 är `ReadabilityPalette.WatchWorld`, så de har lägst bloom och högst contrast.
- W6 har "dim visibility" som regel. Vinjetten är svagare där (0.24) så att kanterna inte blir svarta.
- Ingen Split toning finns i PPv2. Det görs med Lift/Gamma/Gain enligt tabellen.

### Läsbarhet (`tools/grade_check.py`, approximation)
WCAG-ratio objekt mot belyst golv. Till vänster står ograderat genom samma Neutral-tonemap, till höger graderat:

| objekt | W1 | W2 | W3 | W4 | W5 | W6 | W7 |
|---|---|---|---|---|---|---|---|
| fiendeskott | 4.1→5.2 | 4.6→5.7 | 3.2→4.1 | 3.3→4.4 | 4.9→6.0 | 4.9→6.0 | 4.1→5.0 |
| Mid01 | 2.6→3.2 | 2.9→3.5 | 2.0→2.5 | 2.1→2.7 | 3.1→3.7 | 3.1→3.7 | 2.6→3.1 |
| spelarskott | 8.7→11.4 | … | 6.8→9.2 | 7.0→9.8 | … | … | 8.8→11.0 |

Gradern höjer kontrasten i alla världar. Notera att Neutral-tonemap ensam sänker ratio jämfört med 0.47 utan post. Graderade värden landar ungefär på 0.47-nivå (fiendeskott W3 4.1, Mid01 W3 2.5), alltså ingen regression. **Kör aldrig tonemap utan gradern.** SpeltestBot verifierar med capture i Editor.

## 4. Reduce effects-variant (`SettingsState.ReduceEffects`, nyckel `agr.settings.reduceEffects`)
Lägg konstanter i `EffectScale`:
- `PostBloomMul = 0.5f` (bloom intensity ×0.5, threshold +0.15)
- `PostVignetteMul = 0.5f`
- `PostGrainMul = 0f` (grain av)
- Grade, tonemap och exposure **oförändrade**. Det är läsbarhet och inte en effekt, så kontrasten ska vara samma med Reduce.
- När inställningen växlas: applicera direkt, ingen blend.

## 5. Blend vid världsbyte (PPv2-variant; egen stack: se §10)
- Två globala `PostProcessVolume` (`isGlobal = true`), `_volA` och `_volB`, med var sin runtime-profil (`ScriptableObject.CreateInstance<PostProcessProfile>()` + `AddSettings<T>()`, och `.Override(v)` på varje parameter). Inga .asset-filer behövs.
- Vid `ArenaEnv.Retint(world)`: skriv den nya raden till den inaktiva volymen. Lerpa `weight` 0→1 på den nya och 1→0 på den gamla över **1.2 s**, smoothstep, med `Time.unscaledDeltaTime` (hitstop och slow-mo får inte frysa blenden). PPv2 blendar parametrar per weight av sig själv.
- Avbruten blend (snabbt nytt byte): ta den volym som har högst weight som "ut", skriv den nya raden i den andra och fortsätt från nuvarande weights. Ingen hård reset. Samma mönster som `MusicCrossfade.Retarget`.
- Run-start och hangar: snap utan blend. Hangar får egen rad H: bloom 0.55, sat -8, contrast +6, temp +4 (varm studio), vinjett 0.32, grain 0.08.

## 6. Perf (Steam Deck, 1280×800)
- Uppskattat ≈0.8–1.2 ms GPU totalt (fastMode-bloom ≈0.4 ms, grade-LUT ≈0.2 ms, vinjett och grain ≈0.1 ms). Inte mätt.
- Diffusion högst 6 (färre mip-pass), `fastMode true`, inga extra effekter.
- Om Deck går under 60 fps: sätt diffusion 4 och stäng av grain på `SystemInfo.graphicsMemorySize <= 1024` eller via en Display-quality-flagga.
- Bloom-kostnaden är upplösningsberoende. Med en framtida render-scale-option körs post efter scale.

## 7. Emission-kalibrering mot BlenderBots nya FBX (`Assets/Art/Import/`)
Läst ur FBX-binärerna (ingen materialnot fanns): `Ship_Hero`/`Weapon_*` har `Mat_Ship_Glow` (vit, factor ≈3.0), `Mat_Ship_AccentHot_Hero` (orange 1,0.38,0.04), samt en **neon-cyan (0.05,0.95,1)** och en **magenta (1,0.08,0.8)**. `Enemy_Brute_v2` har röd glow med factor ≈3.5. `FX_ShockRing` heter `Mat_FX_Cyan`, cyan med factor 4.0 (luminans ≈3.0). Med threshold 1.10 ger det kraftig neon-halo, alltså epa.
**Regel:** skriv över emission i kod per materialnamn (`ContentFactory`/`ArtImport`, efter import). Lita inte på FBX-emission. Mål-luminans Y (linjär HDR) och färg från tokens:

| Materialnamn | Färg | HDR-multiplikator | Y | Läsning |
|---|---|---|---|---|
| `Mat_Ship_Glow` (motorkärna) | Primary #D4A04A | ×4.0 | 1.6 | tydlig varm glöd |
| `Mat_Ship_AccentHot_Hero` | Primary | ×3.2 | 1.25 | svag glöd |
| cyan-accent på skepp/vapen | **Steel #6AA8C8** (ersätter cyan) | ×3.5 | 1.25 | svag kall glöd |
| magenta-accent | **Primary** ×2.0 (Y 0.8, ingen bloom) eller ta bort | – | <1 | ingen halo |
| `Mat_Enemy_*_Glow` / Brute-glow | Danger #B85A28 | ×7.0 (Brute/Boss ×9.0) | 1.25 / 1.6 | läsbar hotglöd |
| `Mat_FX_Cyan` (ShockRing) → byt namn `Mat_FX_Ring` | Steel | ×4.5, fade med alfa | 1.6 → 0 | |
| `Mat_FX_Pickup_Heart` | VfxHeal-röd behålls men ×1.5 | – | ≈1.1 | knappt |
| projektiler spelare | Primary ×4.0 (Rail ×5.0) | | 1.6–2.0 | |
| fiendeskott | Danger ×9.0 | | 1.6 | |
Tak: Y ≤ 2.0 på allt utom Rail och tunga explosioner (flash `VfxHot` ×2.2, Y 2.0, i ≤0.18 s). Kroppar och rim (02) ligger under 1.0 och bloomar aldrig. Med bloom 0.60–0.65 och diffusion 5.5–6 blir glöden en mjuk halo på ≈2× objektets storlek, utan att fylla skärmen. Reduce: multiplikatorerna oförändrade (läsbarhet), bloomen halveras enligt §4.
Hit-flash (`MeshHitFlash`, `EffectScale.MeshEmissionFull 1.15`) hamnar precis över threshold, alltså en kort blink-glow. Den lämnas som den är.

## 8. Före/efter-mål för SpeltestBots side-by-side (`7a64598` mot 0.48-grenen, samma seed och kamera)
| Scen | Före (idag) | Efter-mål (PASS om …) |
|---|---|---|
| Hangar | platt ambient, ingen glöd, lite preview | varm key + kall rim syns på skrovkanten, motorkärnan har mjuk amber-halo, bakgrund Surface-gradient, ingen ren svart, ingen cyan/magenta-halo |
| W1 våg 1 | mättad blå, platt, skott utan halo | blått dämpat mot stålgrå, skott och motor har tydlig men liten glöd, svag vinjett, HUD oförändrat skarp, fiender lättare att hitta än före |
| W3 våg (Far Drift) | teal golv, generiska fiender smälter in (WCAG 1.0) | teal desat, fiender har Danger-rim + glöd och syns direkt mot golvet, fiendeskott ratio ≥ 4.1, ingen grön neon |
| W4 våg (Mine Fields) | amber golv konkurrerar med Primary/Danger | golvet kallare och desat, Primary-skott och Danger-fiender är de enda varma ytorna, rim i stål |
| Död | hårt klipp | slow-mo + low-pass (05), L-explosion flash→chunks→rök, post blendar mot vinjett 0.45 under 0.6 s, inget vitt fullskärmsblink med Reduce |
FAIL om något i 0.48 ger lägre fiende- eller projektilkontrast än före, om halo gör fiendeskott svårare att se, eller om färgerna läser som neon.

## 9. Emissionsmål per material (bekräftelse/korrigering av de provisoriska målen till BlenderBot)
Art-gren: `assets-0.48-art` (`f9a54b9`). Provisoriskt: threshold 1.1, intensity 0.6, scatter 0.65, ship accents 2.0–2.5, enemy cores 2.5–3.0, projektiler 3.0, asteroider ≤0.3.
- **Threshold 1.1: bekräftad.**
- **Scatter 0.65:** det är en URP-parameter och finns inte i PPv2. Motsvarigheten är `diffusion` 1–10, och 0.65 ≈ **6.0**, vilket redan står i tabell §3. Bekräftad i den formen.
- **Intensity 0.6:** godtas som basnivå för W3, W4 och W6, som är läsbarhetskänsliga. Tabellen i §3 ersätts med 0.60/0.65/0.60/0.60/0.65/0.60/0.65 (W1–W7), hangar 0.55. Vinsten kommer från rätt emission, inte från hög bloom-intensitet.
- **Emission: korrigeras.** En multiplikator betyder olika mycket för olika färger. Danger är mörk (Y 0.18), Primary har Y 0.40 och Steel Y 0.35. Danger ×3 ger Y 0.53, och det bloomar **inte** vid threshold 1.1, så fiendekärnorna skulle bli matta. Målet sätts därför som luminans Y, och multiplikatorn följer av färgen:

| Material | Färg (token) | Mål-Y (linjär) | HDR-multiplikator i Unity (`_EmissionColor = färg × m`) | Provisoriskt → beslut |
|---|---|---|---|---|
| `Mat_Ship_AccentHot_Hero` (accent) | Primary #D4A04A | 1.25 | **3.2** | 2.0–2.5 → 3.2 (2.5 ger Y 0.99, ingen glöd) |
| `Mat_Ship_Glow` (motorkärna) | Primary | 1.6 | **4.0** | ny |
| skepp/vapen cyan-accent | Steel #6AA8C8 (ersätter neon-cyan) | 1.25 | **3.5** | – |
| `Mat_Enemy_*` kärna/glow | Danger #B85A28 | 1.25 | **7.0** | 2.5–3.0 → 7.0 |
| `Mat_Enemy_Brute*` / Boss | Danger | 1.6 | **9.0** | 3.5 → 9.0 |
| `Mat_Enemy_*` kropp | – | <0.3 (rim via shader, 02) | – | – |
| spelarprojektil | Primary | 1.6 (Rail 2.0) | **4.0** (Rail 5.0) | 3.0 → 4.0 |
| fiendeprojektil | Danger | 1.6 | **9.0** | 3.0 → 9.0 |
| `Mat_Asteroid*_Hero` | Steel ×0.15-rim (`ReadabilityPalette.AsteroidEmissionScale`) | ≤0.10 | ≤**0.3** | ≤0.3 bekräftad |
| `Mat_FX_*` ring/shock | Steel, fade med alfa | 1.6→0 | **4.5** | 4.0 → 4.5 i Steel (inte cyan) |
| magenta-material | ta bort / Primary ×2.0 | <1 | – | ingen magenta |
Tak: Y ≤ 2.0 utanför Rail och L-explosionsflash. Om BlenderBot hellre exporterar FBX med ×3–4 spelar det ingen roll, eftersom GameBot skriver över per materialnamn i kod. Tabellen ovan är källan för `ContentFactory`/`ArtImport`. Lägg den Unity-fritt i `Core/EmissionTargets.cs`.

## 10. Spec för egen stack (built-in, OnRenderImage) – detta är huvudvägen
**Pass-ordning** (HDR-src, `RenderTextureFormat.DefaultHDR`, temporära RT via `RenderTexture.GetTemporary`):
1. Prefilter: `max(0, Y - threshold + knee)` (quadratic soft-knee), downsample ½.
2. Down-chain: `iterations` steg, 13-tap eller enkel 4-tap box per steg.
3. Up-chain: tent 3×3 med additiv blandning, `scatter`-vikt per steg (lerp(low, high, scatter)).
4. Uber: `col + bloom*intensity` → exposure → white balance → saturation/contrast → tonemap Neutral → lift/gamma/gain → vinjett → grain → LDR `dst`.
Alt för grade: bygg en 32³-LUT (`Texture3D`, RGBA32) per värld på CPU vid världsbyte från parametrarna nedan och sampla i uber. Det är billigare och gör blend trivial (lerp mellan 2 LUT:ar). **Rekommenderat.**

**Bloom (alla världar)**
| Param | Full | Reduce |
|---|---|---|
| threshold (linjär Y) | 1.10 | 1.25 |
| softKnee | 0.35 (knee = threshold×0.35) | 0.35 |
| intensity | per värld 0.60–0.65 (§3) / hangar 0.55 | ×0.5 |
| iterations | 5 (Deck: 4) | 4 |
| downsample (första) | ½ | ½ |
| scatter (up-vikt) | 0.65 | 0.5 |
| clamp (max Y in i prefilter) | 8.0 | 8.0 |

**Grade per värld**: samma värden som tabell §3. `postExposure` är EV (`col *= exp2(ev)`). `temp`/`tint` är −100..100 och mappas som PPv2 (Bradford/LMS-skift): ±100 ≈ ±1500 K, och temp −4 ≈ 60 K kallare. Saturation −100..100 → `lerp(Y, col, 1+sat/100)`. Contrast −100..100 kring 0.18 i log2: `0.18*exp2(log2(c/0.18)*(1+con/100))`. Tonemap Neutral (Hable-variant, white 5.3, se `tools/grade_check.py`). LGG i LDR: `lift` adderar `w + (rgb-1)` i skuggor, `gain` multiplicerar `1 + w + (rgb-1)`, gamma = 1 (ingen ändring). Grade-funktionen i `tools/grade_check.py` är referensimplementationen. Gör samma matte i C# när LUT:en byggs, så blir CPU och tabell identiska.

**Vinjett**: `d = length((uv-0.5)*float2(aspect,1))/0.85; v = 1 - intensity*smoothstep(1-smoothness, 1, d)`, färg mot `#070B12`. Värden per värld enligt §3.
**Grain**: hash-brus per pixel och frame (`frac(sin(dot(uv*time…)))`), monokromt, amplitud `intensity*0.06*(1-lumContrib*Y)`, size 0.9 = 1 px. Ingen textur behövs.

**Byte per värld**: `ArenaEnv.Retint(world)` anropar `PostStack.SetWorld(world, reduce)`. Bygg en ny LUT i en bakgrundsframe (32³ ≈ 0.3 ms CPU) och lerpa `_LutBlend` 0→1 över 1.2 s smoothstep på `Time.unscaledDeltaTime`. Bloom-intensitet, vinjett och grain lerpas som skalärer samtidigt. Om ett nytt byte kommer mitt i: frys nuvarande blend genom att baka mixen till "från"-LUT (eller använd den dominerande) och starta om mot det nya målet. Ingen hård reset. Run-start och hangar: snap.
**Reduce** (`SettingsState.ReduceEffects`): bloom enligt tabellen, vinjett ×0.5, grain 0. Grade, tonemap och LUT oförändrade. Växla direkt.
**Steam Deck (1280×800) budget: ≤ 1.2 ms GPU totalt.** Bloom 5 iter ≈ 0.5 ms (4 iter ≈ 0.4), uber med LUT ≈ 0.25 ms, RT-minne ≈ 6 MB. Inga extra pass utöver de som står här. Fallback om >1.2 ms: iterations 3, grain av. `PostStack.enabled=false` måste ge exakt dagens bild (säkerhetsbrytare, gärna i Display-settings).

**Mappning om PPv2 används i stället**: threshold, softKnee, intensity, clamp, `fastMode=true` → 1:1. Scatter ≈ `diffusion 6`. Iterations och downsample styrs av diffusion. Grade: `ColorGrading` HDR + `Tonemapper.Neutral`, med samma tal. LGG är Vector4 (rgb, w). Vinjett Classic, `Grain`. Blend: två globala `PostProcessVolume` med weight-lerp (§5).
