# 0.48 · 02 Ljus: key, rim, fill + fake rim + hangar-studio

Built-in RP (se 01 §0). Ljus som finns idag:
- sun: directional 1.15, `(0.92,0.95,1)`
- `ArenaEnv.CreateRimLight`: "ArenaRimLight", directional 0.32, Euler(16,214,0), tintas via `TintNamedLight` till golvfärgen
- `ArenaEnv.CreateUnderGlow`: "ArenaUnderGlow", point 2.1, range r×1.35
- `RenderSettings.ambientLight`: `ContentFactory.CombatAmbient` / `HangarAmbient`

## 1. Key / rim / fill per värld
Key är sun, rim är ArenaRimLight och fill är ambient. Intensiteterna är vad `ArenaEnv.Retint(world)` sätter. Lägg tabellen i ny Unity-fri `Core/WorldLight.cs`.

| W | Key int. | Key färg | Rim int. | Rim färg | Fill (ambient) | UnderGlow int. |
|---|---|---|---|---|---|---|
| 1 | 1.15 | #F2F2FF | 0.40 | golvtint (idag) | (0.12,0.14,0.18) | 2.1 |
| 2 | 1.10 | #F4EEFF | 0.40 | golvtint | (0.13,0.12,0.18) | 2.0 |
| 3 | 1.20 | #F0FAF6 | 0.45 | golvtint | (0.12,0.15,0.16) | 1.8 |
| 4 | 1.20 | #FFF6EA | 0.45 | **#6AA8C8** (stål, inte amber) | (0.14,0.14,0.16) | 1.6 |
| 5 | 1.15 | #EEF2FF | 0.40 | golvtint | (0.11,0.12,0.16) | 2.2 |
| 6 | 1.05 | #FFF0EA | 0.45 | **#6AA8C8** | (0.12,0.11,0.13) | 1.6 |
| 7 | 1.10 | #FFF0F6 | 0.40 | golvtint | (0.13,0.11,0.15) | 2.0 |

W4 och W6 har rim i Secondary-stål. Annars blir rim amber eller rost och fiendesiluetterna smälter in i golvet. Skuggor: oförändrat `LightShadows.None`.

## 2. Fake rim (Fresnel) på skepp och fiender
- Asset: `Assets/Resources/Shaders/AgrRimLit.shader` (staged). Det är en surface shader, `Standard` + emission `_RimColor * pow(1-N·V, _RimPower) * _RimStrength`. Shader.Find-namn: `"AGR/RimLit"`. Den ligger i Resources så att den följer med i build. Om `Shader.Find` ger null faller koden tillbaka på `Shader.Find("Standard")`, alltså samma beteende som idag.
- `ContentFactory`: där skepp och fiender idag får `new Material(Shader.Find("Standard"))` byter du till en hjälpare `RimMaterial(Color albedo, Color rim, float strength)`. `_EmissionColor` sätts som idag (MeshHitFlash och Danger-emission fortsätter fungera, eftersom de skriver `_EmissionColor`). Glöm inte `EnableKeyword("_EMISSION")` om den används.

| Mål | `_RimColor` | `_RimPower` | `_RimStrength` Full / Reduce |
|---|---|---|---|
| Spelarskrov | Secondary `#6AA8C8` (= `ReadabilityPalette.SecondaryRgb`) | 3.0 | 0.60 / 0.60 |
| Fiender generiska + Mid01 | Danger `#B85A28` (`DangerRgb`) | 2.5 | 0.80 / 0.80 |
| Brute / Boss | Danger ×1.2 | 2.0 | 1.00 / 1.00 |
| Asteroider | Secondary ×0.5 | 4.0 | 0.25 / 0.25 |

Rim ändras **inte** med Reduce effects, eftersom den är läsbarhet. Rimmen ligger under bloom-threshold 1.10 (strength ≤1, färg <1), så den ger ingen bloom-halo. Det är avsiktligt.
- Ersätter `ReadabilityPalette.HullRimScale 0.35` (emission-rim) om shadern laddas, annars gäller gamla vägen.

## 3. Projektil-ljus (point-light-budget)
- Max **4** dynamiska point lights från projektiler och explosioner samtidigt, utöver UnderGlow och SpikeLight. Pool: `ProjectileLightPool` (ny, 4 Light-objekt, återanvänds).
- Prioritet: Rail-skott (intensitet 2.0, range 4, 0.12 s) > tung kill (2.5, range 6, 0.18 s) > Pierce (1.2, range 3) > övriga skott: **inget ljus**, de klarar sig med emission + bloom.
- Färg: spelare Primary `#D4A04A`, fiende Danger `#B85A28`.
- `QualitySettings.pixelLightCount` ≥ 6, annars blir de vertex-lit.
- Reduce effects: max 2, intensitet ×0.5 (`EffectScale.ProjectileLightMul = 0.5f`, `ProjectileLightMaxReduced = 2`).

## 4. Hangar-studio, 3-punkt (stor preview)
`HangarPreviewRig` har redan Key/Fill/Rim-konstanter. Nya värden för en större preview i premiumkänsla (varm key, kall rim, svag fill), plus en golvbounce:

| Ljus | Typ | Färg | Int. | Position relativt skeppet (preview-yta) | Konstanter |
|---|---|---|---|---|---|
| Key | Spot 40°, soft | (1.00,0.86,0.66) | 2.2 | (-3.0, 4.0, -3.5), mot skeppet | `KeyIntensity 2.05→2.2`, `KeyR/G/B 1/0.84/0.62→1/0.86/0.66` |
| Fill | Point, range 10 | (0.42,0.62,0.84) | 0.40 | (3.5, 1.5, -2.5) | `FillIntensity 0.48→0.40` |
| Rim | Spot 30° | (0.62,0.78,1.00) | 2.0 | (0.5, 3.0, 4.0), bakom | `RimIntensity 1.7→2.0` |
| Bounce | Point, range 4 | Primary ×0.3 | 0.30 | (0, -1.2, 0) | ny `BounceIntensity 0.3` |

- Bakgrund: `UiTheme.Surface #0E1520` med radial gradient (mitt Surface2). Ingen ren svart.
- Turntable som idag (`TurntableScale`). Rim-shadern är aktiv även här.
- Post-rad H från 01 gäller i hangaren.
- Reduce: oförändrat (statisk scen).
