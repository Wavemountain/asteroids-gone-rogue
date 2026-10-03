# B — Hit-feedback & telegraf-spec (0.47)

Allt är **ej avlyssnat** (mätt med ffmpeg/FFT, inte lyssnat). Identifierare och värden är lästa ur `main` @ `8d86a3f` (`AudioCues.cs`, `CombatJuice.cs`, `MeshHitFlash.cs`, `GameUi.FlashHit`, `JuiceBurst.cs`, `ShipHealth.cs`, `ShipVisuals.cs`, `MonsterPresence.cs`, `BossRules`). Färger som UiTheme-token: Primary `#D4A04A`, Secondary `#6AA8C8`, Accent `#C8CED6`, Danger `#B85A28`, Focus `#E8C878`, Void `#070B12`.

## 0. Sammanfattning – vad som ändras
1. **Sköld skiljs från skrov.** Idag spelar *varje* träff `PlayPlayerDamage` + `CombatJuice.PlayerDamaged(false)` – även när skölden tar smällen. Nya: `agr_shield_hit`, `agr_shield_break`, `agr_armor_hit`.
2. **Pickups får egna ljud.** Shield/Health/RapidFire/Score spelar alla `PlayPickupMinor` (pepSound1 @0.38 ≈ **−35.8 dBFS**, i praktiken ohörbart). Nu 4 egna cues på −14…−22.
3. **Telegrafer får ljud-tell.** Brute har ingen ljud/ingen riktig wind-up; boss-telegraf (aimed 0.4 s / radial 0.8 s) använder `PlayEnemyShoot` = *samma ljud som själva skottet*; nest-puls är tyst; spikar har ingen närhetsvarning. 5 nya tells.
4. **Flash-regler (≤3 Hz, α-tak)** + två befintliga flimmer-källor att fixa (invuln-blink 5,6 Hz, `FlashHit` vid snabba träffar).
5. **Mix-fix:** Ricochet (zap1, −29,6) och Swarm-död (−30,0) var 15–18 dB under sina grannar *och* tystare än Swarm-träff (−16,4) → ersätts med nivå-bakade filer.

## 1. Nya filer (riktiga OggS, `Assets/Resources/Audio/Sfx/`, mono/stereo som källan, Vorbis q5, trim + fades + `alimiter`; gain är **inbakad**, scale 1.0 om inget annat anges)

| Fil | Källa (Kenney, CC0) | Längd | loud100 / peak dBFS | Roll |
|---|---|---|---|---|
| `agr_shield_hit` | interface `glass_001` | 0,276 s | −11,7 / −1,4 | sköld tar träff |
| `agr_shield_break` | impact `impactGlass_heavy_001` | 0,426 s | −10,2 / −1,8 | sköld tar slut |
| `agr_armor_hit` | impact `impactPlate_heavy_001` ×0.9 pitch | 0,389 s | −15,0 / −3,9 | Brute/boss-pansar, lager |
| `agr_pickup_shield` | digital `phaserUp7` (trim) | 0,335 s | −14,1 / −5,4 | Shield pickup |
| `agr_pickup_health` | interface `confirmation_001` | 0,287 s | −14,0 / −7,0 | Health pickup |
| `agr_pickup_rapid` | digital `powerUp2` (trim) | 0,364 s | −14,0 / −2,5 | RapidFire pickup |
| `agr_pickup_score` | digital `pepSound3` (trim) | 0,368 s | −21,8 / −17,0 | Score pickup (mjuk) |
| `agr_tell_brute` | digital `phaserUp4` ×0.72 pitch | 0,457 s | −11,9 / −1,4 | Brute wind-up |
| `agr_tell_nest` | interface `drop_002` ×0.85 pitch | 0,222 s | −20,1 / −4,9 | Swarm-nest puls |
| `agr_tell_boss_aimed` | interface `confirmation_003` ×0.8 pitch | 0,372 s | −12,9 / −4,0 | boss aimed 0,4 s |
| `agr_tell_boss_radial` | digital `zapThreeToneUp` (trim) | 0,829 s | −10,9 / −8,0 | boss radial 0,8 s (stigande, svagt 236→312 Hz) |
| `agr_tell_spike_near` | interface `tick_004` | 0,055 s | −25,7 / −8,3 | spik-närhet (tick) |
| `agr_invuln_end` | interface `pluck_001` | 0,10 s | −22,1 / −1,2 | valfri: i-frames slut |
| `agr_ricochet` | digital `zap1` (nivåbakad, ersätter repo-zap1) | 0,853 s | −15,9 / −10,7 | Ricochet-skott |
| `agr_swarm_death_0/1/2` | digital `zap1`, `spaceTrash1`, `spaceTrash2` | 0,70–0,75 s | −10,9 / −12,3 / −13,8 | Swarm-död pool (ersätter 4-poolen) |

Observera: repots Kenney-kopior är förminskade jämfört med paketet (`zap1` −28,5 mot −5,3, `pepSound1` −27,4 mot −7,4); digital-audio-klipp har ~0,1 s inledande tystnad (trimmad i de bakade filerna).

## 2. Hit-feedback per event

| Event | Trigger (kod) | Ljud | Visuellt | Duck |
|---|---|---|---|---|
| **Skrovträff** (player hit) | `ShipHealth.ApplyDamage`, skada når skrovet (`shieldOnly`/`shieldBroke` = nya lokala flaggor, härleds ur sköldvärdet före/efter); **ej** dödande träff | behåll `PlayPlayerDamage`: `forceField_000` @1.15 + `impactMetal_003` @0.85 (eff −3,2; ska förbli högst av vanliga cues). Ingen pitch-jitter (identitet) | `FlashHit`: byt (1,0.96,0.92) → **Danger `#B85A28`**, α **0,20**, decay 0,18 s (idag 0,12 s). Shake 0,14 (oförändrat). i-frames: se §6 blink | `DuckMusic(0.15, 0.7)` (ny, mild) |
| **Sköldträff** (`shieldOnly`) | samma metod, skada helt absorberad | **ny** `PlayShieldHit()`: `agr_shield_hit` @1.0 + `impactMetal_001` @0.5 lager; pitch ±4 % | **Secondary `#6AA8C8`**: skepps-`MeshHitFlash` 90 ms i Secondary + sköld-ring α0,5→0 på 140 ms; **ingen** helskärmsflash; shake 0,07 | – |
| **Sköld bryts** (`shieldBroke`: skölden går 1→0) | samma | **ny** `PlayShieldBreak()`: `agr_shield_break` @1.0 + `forceField_001` @0.5 | Secondary-flash α **0,16** 0,14 s → ring expanderar 0,25 s; shake 0,14 | `DuckMusic(0.25, 0.55)` |
| **Fiende-träff** (vanlig) | `MeshHitFlash` / träffpool | oförändrat: `PlayHit` pool (`impactMetal_003`, eff −13,5) + punch `impactMetal_000` @0.55 (−19,4) | `MeshHitFlash` 90 ms (1,0.96,0.92) emission 1,15 behålls. **Ta bort helskärmsflash för icke-tunga träffar** (idag `threat hit` 0,1); shake 0,07 → 0,05 | – |
| **Pansar-träff** (Brute, boss, `agr_armor_hit`) | träff på Brute/Boss | lager `agr_armor_hit` @**0.6** ovanpå (eff −19,4); max 1 per 120 ms | `MeshHitFlash` i **Accent `#C8CED6`** 70 ms (stål = "pansar") i stället för varm vit | – |
| **Kill (lätt)** | `JuiceBurst` kill 0,32 s | oförändrat (`explosionCrunch_001` @0.9, −11,0). Swarm: `agr_swarm_death_0/1/2` @0.84 (−12,4) | burst-färg per fiende som idag | – |
| **Kill (tung: Brute/elite)** | `JuiceBurst` 0,4 s | oförändrat: death @1.04 + layer @1.12 (−4,2 → högsta) | burst (1,0.62,0.22) → **Primary `#D4A04A`**; helskärmsflash Primary α**0,12** 0,18 s; shake 0,22 | befintlig |
| **Boss-kill** | `BossRules` död | sekvens av befintliga klipp: Brute-death ×2 (t=0 / 0,18 s) + layer + efter 0,9 s `PlayWaveClear` (0,88) | 2 × tungt kill-burst, shake 0,22→0,36 (MaxShake), flash Primary α0,12 | `DuckMusic(0.9, 0.25)` |
| **Pickup – Shield** | `TryAddShield` ok | **ny** `agr_pickup_shield` @1.0 (−14,1) | pickup-pop `JuiceBurst` heart 0,42 s → färg **Secondary** | – |
| **Pickup – Health** | heal 1 | **ny** `agr_pickup_health` @1.0 (−14,0) | `JuiceBurst` **Primary** (inte röd: röd = liv/ExtraLife) | – |
| **Pickup – RapidFire** | boost 8 s | **ny** `agr_pickup_rapid` @1.0 (−14,0) | `JuiceBurst` **Focus `#E8C878`**; HUD-badge Focus | – |
| **Pickup – Score** | score | **ny** `agr_pickup_score` @1.0 (−21,8) | litet burst **Accent** | – |
| **Pickup – ExtraLife** | oförändrat | `powerUp7` @0.82 (−12,0) | flash 0,28 → α **0,20** (hjärt-röd (1,0.22,0.38)); flimmer fixas §6 | 0,22 s @0,5 (befintlig) |

**Att undvika:** `Time.timeScale = 0` vid hitstop – `HoldMusicForPause` pausar musiken när `timeScale ≤ 0.0001`. Hitstop finns inte idag; om det införs, använd `timeScale ≥ 0.05` och ≤ 60 ms.

## 3. Telegrafer (innan fienden slår)

| Hot | Idag | Ny tell – ljud | Ny tell – visuellt | Tid |
|---|---|---|---|---|
| **Brute** (laddar) | `TuneBrute` startar laddningen *direkt* (range 13, charge 0,95 s, speed 15, turn 210°/s, rest 2,4 s); `MonsterPresence.SetCharging` visar ringen samma frame; inget ljud | `agr_tell_brute` (pitchad ned, stigande) vid wind-up start, 1 röst, @1.0 (−11,9). Spelas ej vid charge-start | **Danger `#B85A28`** fylld ring α 0,25→0,8 + Accent-outline på kroppen; skala ändras inte (inga pulser). Ringens emission ska fadas med alpha (idag `colour×(1.2+alpha)` → "poppar" vid slutet) | **450 ms wind-up** (Brute står still/bromsar, vänder mot spelaren) innan charge. ⚠ **gameplay-beslut för SpelPM/GameBot.** Utan wind-up (alt. A): tell + ring spelas vid charge-start, ändå bättre än idag |
| **Swarm-nest** | `TelegraphNest` pulsar 0,7 s före spawn, anropas varje FixedUpdate, tyst; spawn-ring 0,55 s cyan (0.12,0.88,1); cykel 3,5 s, max 3 | `agr_tell_nest` **en gång/cykel** (flagga som nollas efter spawn) (−20,1) | pulsring i **Secondary `#6AA8C8`** + dubbelring (formcue för färgblinda); spawn-ring Secondary 550 ms | 700 ms |
| **Spik** (`ArenaHazard`, skada 2) | statisk, glöd ~0,86 Hz; `PlayHazardActivate` vid layout-bygge | `agr_tell_spike_near` som **närhets-tick**: spelare < **3,0 m** från närmaste skadande spik → tick var **0,45 s**, < 1,6 m var **0,25 s**; pitch 1,0→1,25 (lerp på avstånd) | spik-glöd Danger ×1,0→1,3 (lerp), inget blink | löpande; 1 röst; av under i-frames |
| **Elite** (våg 10/15/20…, = bossvågor) | `PlayEliteSting` (`jingles_NES00` @0.78, duck 1,5 s @0,4) vid vågstart; `MarkElite` tint (1,0.78,0.28) + `EliteOutline` + aura (1,0.72,0.22) | befintlig sting (−11,4). Ingen ny fil | outline/aura → **Focus `#E8C878`**, puls 1 Hz α 0,5↔0,9 (Reduce: statisk 0,7) | – |
| **Boss – aimed** (3 bultar) | ring gul (1,0.82,0.2) 2,4 m framför; `AimedWindupSeconds` 0,4; start *och* skott = `PlayEnemyShoot` | `agr_tell_boss_aimed` vid telegraf-start (−12,9); skotten behåller `PlayEnemyShoot` | ring **Focus `#E8C878`** + pilspets/kil mot spelaren (formcue) α 0,3→0,8 | 400 ms |
| **Boss – radial** (8 bultar) | ring orange (1,0.45,0.12) vid boss; `TelegraphSeconds` 0,8 | `agr_tell_boss_radial` vid telegraf-start (0,83 s ≈ hela tellen, svag stigning) (−10,9) | ring **Danger `#B85A28`** + 8 ekrar (formcue) α 0,3→0,8 | 800 ms |
| **Gunner / Sniper** | ingen tell (cooldown 1,35 s / 2,2 s, bolt 16 / 22 m/s) | **öppen punkt**: Sniper-bult är snabb (22) och osynlig före skott → föreslå 250 ms laser-linje (Danger α0,35) + `agr_tell_boss_aimed` @0.6. Ej levererat, kräver beslut | – | – |

Färg → mening: **Focus = riktat mot dig (aimed)**, **Danger = område/laddning (radial/Brute/spik)**, **Secondary = spawn/info (nest)**. Formcues (ring/kil/ekrar/dubbelring) i C-specen §colour-blind.

## 4. Pitch, röstgränser, cooldown

| Regel | Värde |
|---|---|
| Pitch-variation | träffar ±4 %; Swarm ±6 %; sköld ±4 %; **tells ingen jitter** (de är signaler, ska låta lika); spik-tick pitch följer avstånd |
| Samma klipp, min avstånd | **45 ms** (annars fasar flera träffar ihop) |
| Hit-klassen (hit/shield/armor/punch) | max **4** samtidiga röster, äldsta stjäls |
| Tells | max **2** samtidiga, 120 ms stagger; ny tell av *samma* typ inom cooldown släpps |
| Cooldowns per källa | Brute 1,2 s · nest 1 per 3,5 s-cykel · boss aimed 1,7 s · boss radial 2,6 s · spik-tick 0,45 s (0,25 s nära) |
| Pickups | max 2; samma pickup-klipp min 80 ms |
| Spik-tick | 1 röst globalt |
| Helskärmsflash | **max 3 Hz** (min 333 ms mellan flashar), α ≤ 0,20, aldrig blink-sekvens; ökande träffar förlänger inte flashen (idag `Max()` → håller flashen "tänd" vid ~6 träffar/s) |
| Unity | AudioManager 32 riktiga röster / 512 virtuella; stealing är på → håll hit-klass under 8 totalt |

⚠ **`_vary`-källan delas:** `PlayPitched` sätter `_vary.pitch` före `PlayOneShot`, vilket ändrar tonhöjd på *redan spelande* klipp i samma källa. För pitchade tells/spik-tick: skapa en separat `AudioSource _tell` (pitch=1 fix) eller baka varianter.

## 5. Föreslagna `AudioCues`-konstanter (skala/pitch) och metoder
```csharp
// Konstanter (scale; filerna har gain inbakat)
public const float ShieldHitScale        = 1.00f;  // + ShieldHitThudScale 0.50 (impactMetal_001)
public const float ShieldBreakScale      = 1.00f;  // + ShieldBreakHumScale 0.50 (forceField_001)
public const float ArmorHitScale         = 0.60f;  // min gap 0.12 s
public const float PickupShieldScale     = 1.00f;
public const float PickupHealthScale     = 1.00f;
public const float PickupRapidScale      = 1.00f;
public const float PickupScoreScale      = 1.00f;
public const float BruteTellScale        = 1.00f;
public const float NestTellScale         = 1.00f;
public const float BossAimedTellScale    = 1.00f;
public const float BossRadialTellScale   = 1.00f;
public const float SpikeTickScale        = 1.00f;  // pitch 1.00..1.25
public const float InvulnEndScale        = 0.80f;  // valfri
// ändrade: RicochetShotScale 0.88 (oförändrad, ny fil agr_ricochet), SwarmDeathScale 0.84 (oförändrad, nya filer)

// LoadClips()
_shieldHit    = Resources.Load<AudioClip>("Audio/Sfx/agr_shield_hit");
_shieldBreak  = Resources.Load<AudioClip>("Audio/Sfx/agr_shield_break");
_armorHit     = Resources.Load<AudioClip>("Audio/Sfx/agr_armor_hit");
_pickupShield = Resources.Load<AudioClip>("Audio/Sfx/agr_pickup_shield");
_pickupHealth = Resources.Load<AudioClip>("Audio/Sfx/agr_pickup_health");
_pickupRapid  = Resources.Load<AudioClip>("Audio/Sfx/agr_pickup_rapid");
_pickupScore  = Resources.Load<AudioClip>("Audio/Sfx/agr_pickup_score");
_tellBrute    = Resources.Load<AudioClip>("Audio/Sfx/agr_tell_brute");
_tellNest     = Resources.Load<AudioClip>("Audio/Sfx/agr_tell_nest");
_tellBossAimed  = Resources.Load<AudioClip>("Audio/Sfx/agr_tell_boss_aimed");
_tellBossRadial = Resources.Load<AudioClip>("Audio/Sfx/agr_tell_boss_radial");
_tellSpikeNear  = Resources.Load<AudioClip>("Audio/Sfx/agr_tell_spike_near");
_shootRicochet  = Resources.Load<AudioClip>("Audio/Sfx/agr_ricochet");          // ersätter zap1
_swarmDeaths    = LoadPool("Audio/Sfx/agr_swarm_death_0", "Audio/Sfx/agr_swarm_death_1", "Audio/Sfx/agr_swarm_death_2");

// Metoder (mönster som PlayEliteSting/PlayPlayerDamage)
public void PlayShieldHit()   { PlayPitched(_shieldHit, ShieldHitScale, 1f + Random.Range(-0.04f, 0.04f)); Play(_hitLight, 0.5f); }
public void PlayShieldBreak() { Play(_shieldBreak, ShieldBreakScale); Play(_hazardActivate, 0.5f); DuckMusic(0.25f, 0.55f); }
public void PlayArmorHit()    { if (RateOk(ref _armorT, 0.12f)) Play(_armorHit, ArmorHitScale); }
public void PlayPickup(Pickup.Kind k) { /* Shield/Health/RapidFire/Score -> respektive klipp, fallback PlayPickupMinor; ExtraLife oförändrad */ }
public void PlayBruteTell()      { if (RateOk(ref _bruteTellT, 1.2f))  PlayTell(_tellBrute, BruteTellScale); }
public void PlayNestTell()       { PlayTell(_tellNest, NestTellScale); }                // anroparen gate:ar en gång/cykel
public void PlayBossAimedTell()  { if (RateOk(ref _aimT, 1.7f)) PlayTell(_tellBossAimed, BossAimedTellScale); }
public void PlayBossRadialTell() { if (RateOk(ref _radT, 2.6f)) PlayTell(_tellBossRadial, BossRadialTellScale); }
public void PlaySpikeNearTick(float p01) { /* p01 = 1 vid kontakt, 0 vid 3 m; pitch = 1f + 0.25f*p01 på _tell-källan */ }
public void PlayInvulnEnd()      { Play(_invulnEnd, InvulnEndScale); }
// RateOk(ref float last, float gap) => Time.unscaledTime - last >= gap; PlayTell = separat AudioSource med pitch 1 (se §4 ⚠).
```
Kopplingspunkter: `ShipHealth.ApplyDamage` (shieldOnly / shieldBroke / hull), `Pickup.cs` ~l.208 (`else`-grenen som anropar `PlayPickupMinor`; switch på `Pickup.Kind`), `MonsterPresence.SetCharging`/Brute-tuning, `TelegraphNest`, boss-telegraf-start där `PlayEnemyShoot` anropas före skott, `ArenaHazard`-update (närhet).

## 6. Fixar jag hittade (flaggor)
1. **`MeshHitFlash.Apply(false)` → `SetPropertyBlock(null)`** raderar per-fiende-emission (`DressEnemyEmission`) efter första träffen. Monster återställs varje frame av `MonsterPresence`, men Mid01/Sniper/Bomber m.fl. blir släckta efter träff. Fix: spara blocket vid flash-start (`GetPropertyBlock(_saved)`) och återställ det i stället för `null`.
2. **`ShipVisuals.BlinkIntervalSeconds` 0,09 → 0,18** (idag växling var 0,09 s ≈ 5,6 Hz under 1,15 s i-frames; över 3 Hz-gränsen). Alternativ: stadig alpha-dim (0,55) – det är vad *Reduce effects* gör (C §3).
3. **`PulseBeacon`/`PingPong(t·6, 0.4)`** på brådskande ExtraLife → 7,5 Hz skalflimmer. Sänk till ≤ 2 Hz (`t·2`).
4. **`TelegraphRing` emission** `colour×(1.2+alpha)` fadas inte med alpha i Standard Transparent → ringen "poppar" i slutet. Verifiera i spel; fada emission med alpha.
5. **`FlashHit` + `Max()`** vid ~6 träffar/s ger flimmer > 3 Hz → införs min 333 ms-gap och α-tak 0,20 (§4).
6. **Boss-tells = skottljud** (se §0.3) – åtgärdas med tells ovan.

## 7. Nivåstege (effektiv nivå = loud100 + 20·log10(scale), före SFX-slider) – nya cues

| Bus / group | Cue | Clip | scale | clip loud100 | **effective** (dBFS, before SFX slider) | note |
|---|---|---|---|---|---|---|
| NEW hit | Shield hit | `agr_shield_hit` | 1 | -11.7 | **-11.7** | pitch ±4% |
| NEW hit | Shield hit thud layer | `impactMetal_001` | 0.5 | -12.1 | **-18.1** |  |
| NEW hit | Shield break | `agr_shield_break` | 1 | -10.2 | **-10.2** | duck 0.25s@0.55 |
| NEW hit | Shield break hum layer | `forceField_001` | 0.5 | -4.4 | **-10.4** |  |
| NEW hit | Armor/boss hit layer | `agr_armor_hit` | 0.6 | -15.0 | **-19.4** | rate-limited |
| NEW pickup | Pickup shield | `agr_pickup_shield` | 1 | -14.1 | **-14.1** |  |
| NEW pickup | Pickup health | `agr_pickup_health` | 1 | -14.0 | **-14.0** |  |
| NEW pickup | Pickup rapid | `agr_pickup_rapid` | 1 | -14.0 | **-14.0** |  |
| NEW pickup | Pickup score | `agr_pickup_score` | 1 | -21.8 | **-21.8** |  |
| NEW tell | Brute wind-up tell | `agr_tell_brute` | 1 | -11.9 | **-11.9** | 450 ms |
| NEW tell | Swarm nest tell | `agr_tell_nest` | 1 | -20.1 | **-20.1** | 700 ms before spawn |
| NEW tell | Boss aimed tell | `agr_tell_boss_aimed` | 1 | -12.9 | **-12.9** | 400 ms |
| NEW tell | Boss radial tell | `agr_tell_boss_radial` | 1 | -10.9 | **-10.9** | 800 ms |
| NEW tell | Spike proximity tick | `agr_tell_spike_near` | 1 | -25.7 | **-25.7** | 0.45/0.25 s |
| NEW mix fix | Ricochet shot (replaces zap1 @0.88) | `agr_ricochet` | 0.88 | -15.9 | **-17.0** | pitch ±4% |
| NEW mix fix | Swarm death pool (replaces zap1/spaceTrash) | `agr_swarm_death_0` | 0.84 | -10.9 | **-12.4** | pool of 3 |
| NEW ui | Invulnerability end | `agr_invuln_end` | 1 | -22.1 | **-22.1** | optional |

Jämförelse (befintliga): skrovträff −3,2 · Brute-död-lager −4,2 · rail −9,4 · Pierce −8,7 · wave clear −12,8 · doctrine −12,5 · träffar −13,5 · bultar −15,5. Nya cues ligger **−10…−14 (viktiga) och −20…−26 (bakgrund)**, under skrovträff/rail och i nivå med träffar, så rail 1.0, Pierce 1.12, wave clear 0.88, doctrine 0.68 och musik 0.28/arena 0.65 är **oförändrade**. Fullständig stege i C §4.

## 8. CREDITS
Se `CREDITS_0.47_fragment.md` (en rad per fil; samtliga är trimmade/nivå-/pitch-bakade derivat av Kenney CC0-klipp).

## 9. Caveats
- Ej avlyssnat; val bygger på mätning (längd, loud100, FFT-tonhöjdskurva). Lyssna särskilt på `agr_tell_brute` (pitch 0,72 kan låta dovt), `agr_tell_boss_radial` (stigningen är svag) och `agr_shield_break` + `forceField_001`-lagret.
- Brute-wind-up 450 ms och Sniper-tell är gameplay-beslut.
- Duck-värden och flash-α är startvärden att tuna i spel.
