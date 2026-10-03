# C — Paletter, kontrast, Reduce effects, mix (0.47)

Alla mått är **beräknade, ej testade i spel/avlyssnade**. Källa: `main` @ `8d86a3f`. Skript: `tools/contrast_worlds.py` → `tools/contrast_tables.generated.md` (full A–F, inbäddad som **Bilaga 1** sist) + `tools/contrast_min.json`. Ljudstege och bakning: `tools/ladder.py`, `tools/bake.py`, `tools/measure.py` (stegen står i §4.2).

Look-bible: fresh retro, ren HUD, anti-"epa". Token: Void `#070B12`, Surface `#0E1520`, Surface2 `#141C28`, Primary `#D4A04A`, Secondary `#6AA8C8`, Accent `#C8CED6`, Danger `#B85A28`, Focus `#E8C878`, Disabled `#3A4450`.

## 1. Världspaletter (golv, himmel/nebulosa, dimma, ambient) – 7 världar

Återanvänd `WorldRules.PaletteMilli` oförändrad utom **två** golv-justeringar (index 12 = brightness). Fog är **av** (`m_Fog: 0`), ingen skybox (`clearFlags=SolidColor`, bakgrund `(0.02,0.03,0.05)` = `#050813`), ambient platt `(0.12,0.14,0.18)` i strid (hangar `(0.20,0.17,0.13)`), lika för alla världar. "Himmel" = kamerabakgrund + nebulosa-quad (a 0,34 + inner 0,18; övre gräns).

| V | Namn | golv (albedo → upplyst) | himmel/nebulosa | stjärna | nebulosa | brightness (nu → förslag) | chroma | regel |
|---|---|---|---|---|---|---|---|---|
| 1 | Launch Belt | `#0A246B` → `#071E63` | `#1F2550` | `#759BD0` | `#3249A0` | 1.00 | 0.55 | more pickups |
| 2 | Deep Orbit | `#2C094B` → `#240745` | `#2A1539` | `#734A9E` | `#49256D` | 0.86 | 0.55 | faster asteroids |
| 3 | Far Drift | `#054C38` → `#044233` (**`#033A2D`**) | `#254641` | `#67E3C7` | `#3E8F7E` | 1.00 → **0.88** | 0.55 | faster enemy fire |
| 4 | Mine Fields | `#653505` → `#552D04` (**`#482603`**) | `#513323` | `#D2AA5E` | `#9E673D` | 0.94 → **0.80** | 0.55 | denser debris |
| 5 | Cross Gates | `#0F132D` → `#0B0F29` | `#171623` | `#3E4B61` | `#20263D` | 0.74 | 0.55 | fewer pickups |
| 6 | Debris Islands | `#300803` → `#270602` | `#2C0D0C` | `#732213` | `#4F130C` | 0.55 | 0.85 | dim visibility |
| 7 | Spoke Ring | `#570932` → `#49072E` | `#3D142B` | `#A63F76` | `#74234E` | 0.90 | 0.55 | heavy enemy fire |

Varför W3/W4: de är de ljusaste golven; Mid01 (2,5/2,6), Brute-aura (min 1,9/2,0) och asteroider (ΔL* −3/−2 = kamouflage) faller där. Med golvfixen: Mid01 2,8, Brute-snitt 3,1, asteroid ΔL* 0/+2 (behöver fortfarande objektfix, §2). Loop-steget (+0,06 brightness/loop, max 1,25) sänker kvoterna ~3–5 % per loop – kolla loop 2+ separat. **Golvtexturen (AstroFloor_v2) har inte inspekterats**; mätt som enfärgat tintat golv.

## 2. Kontrast (WCAG) – nyckelresultat (full tabell per värld: Bilaga 1 B/C)

Typisk skuggning, mot upplyst golv. Gränsvärden: spelkritiska ≥ 3:1 (mål), HUD-text ≥ 4,5:1.

**OK (≥ 3,3 överallt, golv *och* himmel):** spelarens skott 8,5–17,8 · fiendebult 4,1 (3,7 mot himmel W3) · Sniper 5,5 · Bomber 5,7 · Swarm 4,9 · Swarmling 5,8 · Elite-mark 10,4 · pickups 3,4 (ExtraLife, 3,0 mot himmel W3) / 4,4 (Shield) / 5,8 · spik 4,5 · telegraf-ringar 3,3–15.

**Under ~3:1 (flaggade) + förslag**

| Objekt | min nu | Problem | Förslag (omräknat) |
|---|---|---|---|
| Mid01 (röd) | 2,5 W3 / 2,6 W4 | röd på grön/brun | golvfix → 2,8; **emission (0.82,0.1,0.12) ×1,35 → 3,6** |
| Scout/Gunner/Drone/SwarmPod (mat default) | **1,0–1,7 alla världar** | kroppen försvinner; bara accentdelar syns | `Mat_Enemy` emission = **Danger ×0,9** → 2,8 sämst (W3), 4,1 (W5); ΔL* ≥ 28 |
| Brute/Boss-aura | puls-min 1,9 W3 / 2,0 W4 / 2,6 W1,W7 | puls går under 3 | puls-min 0,8 (idle: bas 1,0 amp 0,2) / 0,7 (charge) → 3,3 / 2,7; golvfix ger 3,1 snitt |
| Spelarskrov (stål) | 1,5–2,4 | ΔL* 11 i W3 (svagt) | **rim-emission Secondary ×0,35** → 2,1 (ΔL* ↑) |
| Asteroid | **1,1–1,5**, ΔL* −3 (W3), −2 (W4), 7 (W1,W7) | faktisk fara (asteroider skadar) som smälter in | **albedo ×1,5 + Secondary-emission ×0,15** → ΔL* ≥ 15 i alla världar (15 W3, 17 W4, 21 W1/7, 27 W2, 30 W5/6). Kräver art-beslut (ljusare sten) eller rim-ljus-shader. Tabell Bilaga 1 F |
| Fiendens HUD-ikon/`Danger` text | 3,5–4,3 på platta | under AA för liten text | Danger endast för stor text, staplar, rubriker, ikoner (3:1) – aldrig liten brödtext |

HUD-text på `HudPlate` (Surface α0,72) över golv/himmel: Primary 6,9–8,4 · Secondary 6,2–7,6 · Accent 10,2+ · Focus 10+ · shop_locked 7,5+ · shop_poor 10,4+ → **AA OK**. Danger 3,5–4,3 → *large only*. Disabled 1,6–2,0 → medvetet dämpat; **använd aldrig för levande info** (hull/sköld/cooldown).

### Färger per värld (HUD/fiende/projektil)
HUD-, fiende- och projektilfärger är **världsoberoende** (tokens/material), bara bakgrunden varierar. Rekommendation: ändra dem *inte* per värld (lär spelaren färgspråket) utan korrigera golvet (§1) + objekt (§2). Semantik:
- **Spelare = bärnsten/stål:** hull Primary, sköld Secondary, spelarskott gul→vit (Accent-vit kärna), pierce stål.
- **Fiende = rost (Danger):** kropp emission Danger, bult `#F27837`-ish → Danger-lik, telegraf Danger/Focus.
- **Info = stål (Secondary):** nest/spawn, sköld, pickup-Shield.
- **Belöning = Focus/Primary:** pickups, rapid-badge.
- **Röd endast för liv** (ExtraLife-hjärta `(1,0.22,0.38)`).
- Varma världar (W4 brun, W6 mörkröd, W7 plommon) har hue-närhet till rost-fiender men klarar luminans (bult ≥ 4,3; W6 6,8) → förlita dig på luminans + kontur, inte nyans.

### Anti-"epa": off-palette neon i koden → token
| Var | Nu | Ersätt med |
|---|---|---|
| Arena-läpp / grid | cyan `(0.42,0.88,1)` / `(0.2,0.85,1)` | Secondary `#6AA8C8` (lägre emission) |
| Layout-material Cyan/Magenta/Lime/Orange | rena neon | Secondary / Surface2-stål / Primary / Danger; ta bort Magenta/Lime |
| Swarm-aura | cyan `(0.12,0.88,1)` | Secondary |
| Swarmling | lime `(0.28,1,0.48)` | Secondary-grön-tonad ~`#7FB89A` (dämpad) eller Accent + form |
| Mid01 | `(0.82,0.1,0.12)` rent rött | Danger-familj (rost) – rött reserverat för liv |
| Telegraf-ringar | cyan / gul `(1,0.82,0.2)` / orange `(1,0.45,0.12)` | Secondary / Focus / Danger (B §3) |
| Heavy kill-burst | `(1,0.62,0.22)` | Primary |
| Mus-ikoner (E) | röd `(231,50,70)` | Primary (gjort) |

### Färgblindhet
Machado 2009 (protan/deutan/tritan, svårighet 1,0) på 12 par (spelar- vs fiendebult, Brute vs Swarmling, Mid01 vs Sniper, ExtraLife vs övriga pickups, telegraf-ringar, Primary vs Danger): **alla OK** på luminanskvot (CVD-invariant) – se Bilaga 1 E. Svagaste: ExtraLife vs Shield-pickup 1,31:1 i luminans (skiljs på ΔE 42–127 men ej luminans). Regler:
1. **Förlita dig aldrig på rött/grönt ensamt.** Varje hotklass har en *form*: aimed = **kil/pil** mot spelaren, radial = **8 ekrar**, nest = **dubbelring**, elite = **kontur**, Brute = **fylld ring**, spik = **taggad silhuett**.
2. ExtraLife: hjärt-silhuett + pulserande (≤2 Hz) kontur – inte bara röd.
3. Skillnad spelarskott/fiendeskott: spelare = ljus **vit/gul kärna, långsträckt**, fiende = **mörkare rost, rund**. Luminanskvot 2,1–2,6.
4. HUD: hull/sköld skiljs av *position + ikon*, inte endast Primary/Secondary (kvot 1,11).

## 3. "Reduce effects" (tillgänglighet)

**Namn:** *Reduce effects* (sv: *Minska effekter*). **Nyckel:** `agr.settings.reduceEffects` (PlayerPrefs int 0/1), **default 0 (av)**. Ny `SettingsRows`-rad direkt efter `ScreenShake` i `Order`; `SettingsState.CurrentVersion` 2 → 3 (migration: saknad nyckel = 0). Uppdatera `Loc` (sv/en).

Samspel med befintlig `ScreenShakeEnabled` (default true): är den **av** → shake 0 oavsett. Är Reduce **på** och ScreenShake på → shake ×0,35.

| Effekt | Idag (kod) | Reduce effects = på |
|---|---|---|
| Skärmskak | `FollowCamera` `MaxShake` 0,36, `ShakeDecay` 11; hit 0,14/0,07, explosion 0,22, ExtraLife 0,12 | amplitud **×0,35**, `MaxShake` **0,12** |
| Helskärmsflash (`GameUi.FlashHit`, vit/Danger) | α upp till ~0,2–0,28, decay 0,12 s, `Max()` | **α-tak 0,08**, **min 0,5 s** mellan flashar (≤ 2 Hz), fade **0,22 s**; ingen flash på träff mot fiende; kill-flash av |
| Mesh-träffflash (`MeshHitFlash`) | 90 ms, emission 1,15 | **60 %** emission (0,7), **140 ms** (mjukare, inte snabbare) |
| i-frames blink (`ShipVisuals`) | hide/show var 0,09 s (5,6 Hz) | **ingen blink**: stadig alpha-dim **0,55** under 1,15 s |
| Burst (`JuiceBurst`) | hit 0,12 s, kill 0,32–0,4 s, heart 0,42 s | alpha **×0,4**, slutskala **×0,6**, längd oförändrad |
| Partiklar | inga ParticleSystems idag (primitiver + `TrailRenderer`) | `TrailRenderer` bredd ×0,6; **framtida** partiklar: antal ×0,4, storlek ×0,7 |
| Pulser/flimmer | ExtraLife `PingPong(t·6)` 7,5 Hz; elite-banner fontstorlek 22↔26; aura/ring-puls | **av**: ExtraLife statisk glöd; banner frusen; aura statisk 0,7; ring-skalning av |
| Hitstop | finns ej | n/a; om det införs: **av** (0 ms) – och aldrig `timeScale 0` |
| Kamera-kick | finns ej | n/a; framtida: ×0,3 |
| Bloom / postprocess | finns ej (ingen PP-stack) | n/a; framtida: intensity ×0,5, threshold +0,2 |
| Stroboskopi | – | **inget >3 Hz**; garantera min-gap-reglerna ovan |

**Telegrafer (måste förbli läsbara, bara mjukare):** ringens alpha-golv **≥ 0,45** (i stället för 0,25→0,8 ramp: 0,45→0,8), fade-in **120 ms**, ingen skalpuls, **formcues oförändrade** (kil/ekrar/dubbelring/kontur), elite-kontur statisk 0,7. **Ljud-tells orörda** (de är informationen) – Reduce påverkar inte ljud. Spik-glöd: ingen puls, konstant 1,15× (Danger).

Allt styrs via en `Fx.Reduce`-flagga + multiplikatorer i `CombatJuice`, `GameUi.FlashHit`, `MeshHitFlash`, `ShipVisuals`, `JuiceBurst`, `PulseBeacon` – inga nya assets.

## 4. Mix & ducking

### 4.1 Nuläge
Ingen `AudioMixer`; källor `_sfx`, `_vary` (pitchad), `_music`/`_musicB`, `_hangarLayer`, `_railRise`, `_railHold`. `ApplyVolumes`: SFX = `_sfxVolume` (default 0,8), musik = `_musicVolume` (default 0,28) × `_musicScale` × `_duckScale`. PlayerPrefs `agr.audio.sfx`, `agr.audio.music`, `agr.audio.mute` (linjärt). `PlayOneShot`-scale klamras 0–1,4.

### 4.2 Nivåstege – effektiv nivå = loud100 + 20·log10(scale), före SFX-slidern (loud100 = starkaste 100 ms-fönstret, dBFS)
Nya cues markerade **NEW**; "NEW mix fix" ersätter underdimensionerade befintliga.

| Bus / group | Cue | Clip | scale | clip loud100 | **effective** (dBFS, before SFX slider) | note |
|---|---|---|---|---|---|---|
| SFX/combat | Bolt shot (laserSmall pool) | `laserSmall_000` | 1 | -16.0 | **-16.0** | pitch ±3% |
| SFX/combat | Spread shot | `laserRetro_000` | 1.05 | -4.8 | **-4.4** |  |
| SFX/combat | Pierce shot | `laserLarge_000` | 1.12 | -9.7 | **-8.7** | loudest regular weapon |
| SFX/combat | Seeker shot | `phaserUp5` | 0.72 | -14.4 | **-17.3** |  |
| SFX/combat | Ricochet | `zap1` | 0.88 | -28.5 | **-29.6** | pitch ±4% |
| SFX/combat | Rail charge rise | `phaserUp3` | 0.6 | -12.0 | **-16.4** | pitch 0.95 |
| SFX/combat | Rail hold loop | `engineCircular_001` | 0.2 | -11.8 | **-25.8** |  |
| SFX/combat | Rail release | `laserLarge_002` | 1 | -9.4 | **-9.4** | pitch 0.92 |
| SFX/combat | Rail thump layer | `lowFrequency_explosion_001` | 0.5 | -4.8 | **-10.8** |  |
| SFX/combat | Enemy bolt | `laserSmall_001` | 0.7 | -15.3 | **-18.4** |  |
| SFX/hit | Enemy hit (pool) | `impactMetal_003` | 1 | -13.5 | **-13.5** | ±4% |
| SFX/hit | Hit punch layer | `impactMetal_000` | 0.55 | -14.2 | **-19.4** |  |
| SFX/hit | Brute hit | `impactMetal_001` | 1 | -12.1 | **-12.1** |  |
| SFX/hit | Swarm hit | `laserSmall_001` | 0.88 | -15.3 | **-16.4** | ±6% |
| SFX/hit | Player damage: hum | `forceField_000` | 1.15 | -4.4 | **-3.2** | existing hull hit |
| SFX/hit | Player damage: impact | `impactMetal_003` | 0.85 | -13.5 | **-14.9** |  |
| SFX/kill | Enemy death | `explosionCrunch_003` | 1 | -11.3 | **-11.3** |  |
| SFX/kill | Enemy death punch | `impactMetal_000` | 0.78 | -14.2 | **-16.4** |  |
| SFX/kill | Light kill (Mid/Pod/Swarmling) | `explosionCrunch_001` | 0.9 | -10.1 | **-11.0** | ±5% |
| SFX/kill | Brute death | `explosionCrunch_003` | 1.04 | -11.3 | **-11.0** |  |
| SFX/kill | Brute death layer | `lowFrequency_explosion_000` | 1.12 | -5.2 | **-4.2** |  |
| SFX/kill | Swarm death | `zap1` | 0.84 | -28.5 | **-30.0** |  |
| SFX/kill | Asteroid split | `explosionCrunch_000` | 1 | -10.1 | **-10.1** |  |
| SFX/spawn | Brute spawn | `lowThreeTone` | 0.98 | -17.3 | **-17.5** | duck 0.32s@0.4 |
| SFX/spawn | Swarm spawn | `phaseJump1` | 0.82 | -23.2 | **-24.9** |  |
| SFX/spawn | SwarmPod spawn | `phaserUp5` | 0.86 | -14.4 | **-15.7** | duck 0.36s@0.38 |
| SFX/spawn | Hazard activate | `forceField_001` | 0.94 | -4.4 | **-4.9** | duck 0.28s@0.42 |
| SFX/pickup | Pickup minor (all kinds today) | `pepSound1` | 0.38 | -27.4 | **-35.8** | too quiet |
| SFX/pickup | Extra life | `powerUp7` | 0.82 | -10.3 | **-12.0** | duck 0.22s@0.5 |
| SFX/stinger | Wave clear | `jingles_HIT07` | 0.88 | -11.7 | **-12.8** |  |
| SFX/stinger | Doctrine pick | `jingles_NES03` | 0.68 | -9.2 | **-12.5** | duck 0.3s@0.4 |
| SFX/stinger | Elite sting | `jingles_NES00` | 0.78 | -9.2 | **-11.4** | duck 1.5s@0.4 |
| SFX/stinger | Wave fail (GameOver is music file) | `lowDown` | 0.4 | -17.9 | **-25.9** | layer; main = GameOver 0.78 |
| UI | UI click | `click_002` | 0.88 | -25.5 | **-26.6** |  |
| UI | Hangar purchase | `confirmation_002` | 1 | -9.6 | **-9.6** |  |
| UI | Abort whoosh | `minimize_005` | 1 | -9.9 | **-9.9** | duck 0.55s@0.18 |
| UI | World change | `maximize_008` | 1 | -10.2 | **-10.2** | W3: 1.12 + duck |
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
**Utliggare i befintligt:** Pickup minor −35,8 (alla pickups) · Ricochet −29,6 · Swarm-död −30,0 (och *tystare än* Swarm-träff −16,4 – omvänd) · UI-klick −26,6 · Rail hold −25,8 (avsiktligt tyst loop). Fixade via B-filerna (pickups, `agr_ricochet`, `agr_swarm_death_*`). Median "viktigt" ≈ −10…−14, bakgrund ≈ −17…−26.

### 4.3 Musik (RMS/LUFS från repots Music-filer) och förhållande till SFX
| Music file | LUFS-I | mean RMS dBFS | scale | note |
|---|---|---|---|---|
| `GameOver` | -13.6 | -15.3 | 0.78 | fail sting (SFX path) |
| `MissionPlausible` | -15.0 | -15.8 | 0.65 | legacy fallback arena |
| `OutThere` | -26.5 | -28.1 | 0.65 | fallback |
| `SpaceCadet` | -13.5 | -14.8 | 0.55 | credits |
| `TimeDriving` | -17.8 | -18.4 | 0.65 | legacy fallback |
| `boss_guardian` | -14.2 | -16.1 | 0.65 | boss |
| `boss_guardian_final` | -14.1 | -15.5 | 0.65 | boss W7/loop2+ |
| `spacelifeNo14` | -19.2 | -20.8 | 0.48 | hangar (0.94 pitch) + 0.22 layer |
| `world1_launch_belt` | -15.0 | -14.6 | 0.65 | world track |
| `world2_deep_orbit` | -15.0 | -14.6 | 0.65 | world track |
| `world3_far_drift` | -15.2 | -16.1 | 0.65 | world track |
| `world4_mine_fields` | -15.0 | -15.6 | 0.65 | world track |
| `world5_cross_gates` | -15.1 | -17.5 | 0.65 | world track |
| `world6_debris_islands` | -15.0 | -16.3 | 0.65 | world track |
| `world7_spoke_ring` | -15.1 | -16.7 | 0.65 | world track |
Slutlig musiknivå vid default (arena 0,65 × slider 0,28 = −14,8 dB): världsspår ≈ **−30…−32 dBFS RMS**; hangar ≈ −38. SFX default 0,8 (−1,9 dB): viktiga SFX-toppar ≈ **−12…−16**, alltså ~15–18 dB över musiken – tillräckligt för klarhet, och ducking behövs mest för långa/täta stingers. Boss-spåren (−14,2/−14,1 LUFS) är ~1 dB hetare än världsspåren (−15,0) men med samma scale 0,65 → ok; sänk `BossMusicScale` till **0,62** om bossmusik + tells maskerar (valfritt, lyssna först).

### 4.4 Ducking-tabell (`DuckMusic(seconds, scale)`; scale = musikens mål, 0,4 = −8 dB)
Semantik att behålla (PR #43 *Min/Max*): målet appliceras **direkt**, rampar linjärt tillbaka till 1; vid överlapp **min(target)**, **max(until)**, **max(duration)**. Förslag: 20–40 ms attack-ramp i `ApplyVolumes` mot klick (valfritt).

| Cue | sek | mål | status |
|---|---|---|---|
| Abort/pausvarning | 0,55 | 0,18 | befintlig |
| Fail | 0,55 | 0,30 | befintlig |
| Doctrine pick | 0,30 | 0,40 | befintlig |
| Brute spawn | 0,32 | 0,40 | befintlig |
| SwarmPod spawn | 0,36 | 0,38 | befintlig |
| Credits open | 0,35 | 0,40 | befintlig |
| World 3 byte | 0,42 | 0,40 | befintlig |
| Hazard activate | 0,28 | 0,42 | befintlig |
| ExtraLife | 0,22 | 0,50 | befintlig |
| Rail | 0,18 | 0,60 | befintlig |
| Elite sting | 1,50 | 0,40 | befintlig |
| **Skrovträff** | 0,15 | 0,70 | NEW (mild) |
| **Sköld bryts** | 0,25 | 0,55 | NEW |
| **Boss-kill** | 0,90 | 0,25 | NEW |
| **Boss-start (musikbyte)** | – | – | befintlig mixning; tells ducka *inte* |
| Tells (alla) | – | – | **ingen duck** – ljudnivån avgör, musiken ska bära rytmen |
| Pickups, sköldträff, armor | – | – | ingen duck |

Max samtidig duck-djup: aldrig under **0,18** (befintligt golv); stapling av elite 0,4 + träff 0,7 → 0,4 (min) – ok.

### 4.5 Kanaler/trim (logiska bussar; ingen mixer idag)
| Buss | Innehåll | Trim (rel.) | Slider |
|---|---|---|---|
| Master | `AudioListener.volume` | 0 dB | Master (ny) |
| Music | `_music`/`_musicB`/`_hangarLayer` | scale: arena 0,65, hangar 0,48, boss 0,65 | Music |
| SFX-combat | vapen, träffar, kills, spawns, tells, pickups | cue-scale i §4.2 | SFX |
| Stingers | wave clear, doctrine, elite, fail | scale i §4.2 | SFX |
| UI | click, purchase, abort, world change | SFX × **0,88** (const `UiTrim`) | följer SFX (valfri egen UI-slider) |

Prioritet (Unity `AudioSource.priority`, lågt = viktigare): **`_music` = 0**, stingers/tells 64, träffar 128, loops/ambient 200 – så musiken aldrig stjäls när 32 röster är fulla.

### 4.6 Slider → dB (förslag)
Dagens linjära slider (0–1, steg 0,1) ger dålig upplösning i det tysta området. Använd **gain = p²** (≈ −40 dB vid p = 0,1; 0 vid p = 0 = mute): `dB = 40·log10(p)`.

| p (slider) | gain | dB |
|---|---|---|
| 1,0 | 1,000 | 0,0 |
| 0,9 | 0,810 | −1,8 |
| 0,8 | 0,640 | −3,9 |
| 0,7 | 0,490 | −6,2 |
| 0,6 | 0,360 | −8,9 |
| 0,5 | 0,250 | −12,0 |
| 0,4 | 0,160 | −15,9 |
| 0,3 | 0,090 | −20,9 |
| 0,2 | 0,040 | −28,0 |
| 0,1 | 0,010 | −40,0 |
| 0,0 | 0 | −∞ |

**Defaults som bevarar nuvarande ljudbild:** Music **p = 0,5** (gain 0,25 ≈ dagens 0,28), SFX **p = 0,9** (0,81 ≈ dagens 0,80), Master **1,0**, (UI följer SFX). **Migration:** vid `CurrentVersion` 2→3 sätt `p = sqrt(gammal_gain)` (0,28 → 0,53; 0,8 → 0,89), avrunda till 0,05. Nyckel `agr.audio.master` (nytt). Om `AudioMixer` införs senare: exponera `MasterVol/MusicVol/SfxVol/UiVol` i dB med samma kurva (`dB = 40·log10(p)`, golv −80).

## 5. Caveats
- Kontrastmodellen är approximativ (linjär färgrymd; sol + ambient + rim från `Play.unity`; golvet är enfärgat). WCAG är pessimistiskt för mörkt-på-mörkt – därför är ΔL* med som kompletterande mått.
- Golvtexturen och verklig nebulosa-textur är **inte** inspekterade; ändra W3/W4 först efter synlig kontroll.
- Reduce-multiplikatorerna är startvärden; inget är testat i spel.
- Allt ljud **oavlyssnat**; loudness = ffmpeg/numpy-mätning.

---
# Bilaga 1 – genererade kontrasttabeller (`tools/contrast_tables.generated.md`)

### A. World palette (reused from `WorldRules.PaletteMilli`, loop 1; hex = what ArenaEnv.Retint/TintFloor produce)

| W | Name | floor albedo | floor lit* | sky (nebula over bg)** | star | nebula | brightness | chroma | purple neb | rule |
|---|---|---|---|---|---|---|---|---|---|---|
| 1 | Launch Belt | `#0A246B` | `#071E63` | `#1F2550` | `#759BD0` | `#3249A0` | 1.00 | 0.55 | 0 | more pickups |
| 2 | Deep Orbit | `#2C094B` | `#240745` | `#2A1539` | `#734A9E` | `#49256D` | 0.86 | 0.55 | 1 | faster asteroids |
| 3 | Far Drift | `#054C38` | `#044233` | `#254641` | `#67E3C7` | `#3E8F7E` | 1.00 | 0.55 | 0 | faster enemy fire |
| 4 | Mine Fields | `#653505` | `#552D04` | `#513323` | `#D2AA5E` | `#9E673D` | 0.94 | 0.55 | 1 | denser debris |
| 5 | Cross Gates | `#0F132D` | `#0B0F29` | `#171623` | `#3E4B61` | `#20263D` | 0.74 | 0.55 | 0 | fewer pickups |
| 6 | Debris Islands | `#300803` | `#270602` | `#2C0D0C` | `#732213` | `#4F130C` | 0.55 | 0.85 | 1 | dim visibility |
| 7 | Spoke Ring | `#570932` | `#49072E` | `#3D142B` | `#A63F76` | `#74234E` | 0.90 | 0.55 | 1 | heavy enemy fire |

\* lit = albedo x (ambient `(0.12,0.14,0.18)` + sun 1.15 x `(0.92,0.95,1)` x N.L 0.707 + rim 0.32 x N.L 0.276 x floor tint), linear space, then sRGB. \*\* sky = camera bg `#050813` under nebula a=0.34 + inner a=0.18 (uniform-cover upper bound, real texture is patchier/darker). Fog: **off** (`m_Fog: 0`), no skybox, `clearFlags=SolidColor`. Ambient: Flat `#1F242E`-ish `(0.12,0.14,0.18)` in combat, `(0.20,0.17,0.13)` in hangar, same for all 7 worlds.

### B. Contrast ratios (WCAG, higher = easier) object vs LIT FLOOR, typical lighting (shade factor 0.5)

Cell = ratio. `!` = below 3:1 (gameplay-critical). Shade-side minimum (emission+ambient only) is in section C.

| Item | W1 | W2 | W3 | W4 | W5 | W6 | W7 | min | apparent colour (W1) |
|---|---|---|---|---|---|---|---|---|---|
| Player bolt (primary) | 14.4 | 16.7 | 10.8 | 11.3 | 17.8 | 17.8 | 14.7 | **10.8** | `#FFFF7F` |
| Player spread | 11.3 | 13.1 | 8.5 | 8.8 | 13.9 | 14.0 | 11.5 | **8.5** | `#FFDB5F` |
| Player pierce (steel) | 13.4 | 15.5 | 10.0 | 10.5 | 16.5 | 16.6 | 13.6 | **10.0** | `#AAFFFF` |
| Player seeker | 11.6 | 13.4 | 8.7 | 9.0 | 14.3 | 14.3 | 11.8 | **8.7** | `#91EEFF` |
| Player ricochet | 13.0 | 15.1 | 9.7 | 10.2 | 16.0 | 16.1 | 13.2 | **9.7** | `#FFEF74` |
| Enemy bolt | 5.5 | 6.4 | 4.1 | 4.3 | 6.8 | 6.8 | 5.6 | **4.1** | `#F27837` |
| Enemy Mid01 (red) | 3.4 | 3.9 | 2.5! | 2.6! | 4.2 | 4.2 | 3.4 | **2.5** | `#DC373F` |
| Enemy Scout/Gunner/Drone/SwarmPod (mat default) | 1.4! | 1.6! | 1.0! | 1.1! | 1.7! | 1.7! | 1.4! | **1.0** | `#5A3138` |
| Enemy Sniper (cyan) | 7.3 | 8.5 | 5.5 | 5.7 | 9.1 | 9.1 | 7.5 | **5.5** | `#5ABCFF` |
| Enemy Bomber (orange) | 7.6 | 8.8 | 5.7 | 5.9 | 9.4 | 9.4 | 7.7 | **5.7** | `#FFA13E` |
| Monster Brute / Boss (aura, pulse avg) | 3.8 | 4.4 | 2.8! | 2.9! | 4.6 | 4.7 | 3.8 | **2.8** | `#D65341` |
| Monster Brute / Boss (pulse min) | 2.6! | 3.0 | 1.9! | 2.0! | 3.2 | 3.2 | 2.6! | **1.9** | `#A54740` |
| Monster Swarm (aura) | 6.6 | 7.7 | 4.9 | 5.2 | 8.2 | 8.2 | 6.7 | **4.9** | `#32BAD3` |
| Monster Swarmling (aura) | 7.8 | 9.0 | 5.8 | 6.1 | 9.6 | 9.6 | 7.9 | **5.8** | `#4ED174` |
| Elite mark (MarkElite) | 13.9 | 16.1 | 10.4 | 10.9 | 17.2 | 17.2 | 14.2 | **10.4** | `#FFFB44` |
| Pickup ExtraLife heart | 4.5 | 5.2 | 3.4 | 3.5 | 5.5 | 5.5 | 4.5 | **3.4** | `#FF3E6B` |
| Pickup generic (Mat_Ship_Accent) | 7.7 | 8.9 | 5.8 | 6.0 | 9.5 | 9.5 | 7.9 | **5.8** | `#FFA41F` |
| Pickup Shield (Mat_Shield, a=.22) | 5.9 | 6.8 | 4.4 | 4.6 | 7.2 | 7.3 | 6.0 | **4.4** | `#31ABE1` |
| Spike (damaging, emission pulse avg) | 6.0 | 6.9 | 4.5 | 4.7 | 7.4 | 7.4 | 6.1 | **4.5** | `#FF7D22` |
| Player hull (steel) | 2.0 | 2.3 | 1.5 | 1.5 | 2.4 | 2.4 | 2.0 | **1.5** | `#455464` |
| Player accent (amber) | 7.7 | 8.9 | 5.8 | 6.0 | 9.5 | 9.5 | 7.9 | **5.8** | `#FFA41F` |
| Asteroid | 1.2 | 1.4 | 1.1 | 1.1 | 1.5 | 1.5 | 1.2 | **1.1** | `#3A322E` |
| Ring: Swarm drop (cyan) (alpha 0 end state) | 12.2 | 14.2 | 9.2 | 9.6 | 15.1 | 15.1 | 12.4 | **9.2** | `#25FFFF` |
| Ring: Boss aimed (yellow) (alpha 0 end state) | 13.9 | 16.1 | 10.4 | 10.9 | 17.1 | 17.2 | 14.1 | **10.4** | `#FFFB3D` |
| Ring: Boss radial (orange) (alpha 0 end state) | 6.5 | 7.5 | 4.8 | 5.1 | 8.0 | 8.0 | 6.6 | **4.8** | `#FF8A25` |
| Ring: ExtraLife (red) (alpha 0 end state) | 4.3 | 5.0 | 3.3 | 3.4 | 5.4 | 5.4 | 4.4 | **3.3** | `#FF3762` |

### C. Same, object vs SKY (edge of arena, nebula behind), and shade-side minimum vs floor

| Item | W1 sky | W2 sky | W3 sky | W4 sky | W5 sky | W6 sky | W7 sky | min sky | min floor, shade-side |
|---|---|---|---|---|---|---|---|---|---|
| Player bolt (primary) | 13.7 | 15.7 | 9.8 | 10.7 | 17.0 | 17.0 | 14.9 | **9.8** | 10.8 |
| Player spread | 10.7 | 12.3 | 7.7 | 8.4 | 13.3 | 13.3 | 11.6 | **7.7** | 7.7 |
| Player pierce (steel) | 12.8 | 14.6 | 9.1 | 10.0 | 15.7 | 15.8 | 13.8 | **9.1** | 10.0 |
| Player seeker | 11.0 | 12.6 | 7.9 | 8.6 | 13.6 | 13.6 | 11.9 | **7.9** | 7.7 |
| Player ricochet | 12.4 | 14.1 | 8.8 | 9.6 | 15.3 | 15.3 | 13.4 | **8.8** | 8.8 |
| Enemy bolt | 5.2 | 6.0 | 3.7 | 4.1 | 6.4 | 6.4 | 5.6 | **3.7** | 3.4 |
| Enemy Mid01 (red) | 3.2 | 3.7 | 2.3! | 2.5! | 4.0 | 4.0 | 3.5 | **2.3** | 2.1! |
| Enemy Scout/Gunner/Drone/SwarmPod (mat default) | 1.3! | 1.5! | 1.1! | 1.0! | 1.6! | 1.6! | 1.4! | **1.0** | 1.0! |
| Enemy Sniper (cyan) | 7.0 | 8.0 | 5.0 | 5.4 | 8.6 | 8.6 | 7.6 | **5.0** | 5.1 |
| Enemy Bomber (orange) | 7.2 | 8.2 | 5.2 | 5.6 | 8.9 | 8.9 | 7.8 | **5.2** | 5.4 |
| Monster Brute / Boss (aura, pulse avg) | 3.6 | 4.1 | 2.6! | 2.8! | 4.4 | 4.4 | 3.9 | **2.6** | 2.3! |
| Monster Brute / Boss (pulse min) | 2.5! | 2.8! | 1.8! | 1.9! | 3.1 | 3.1 | 2.7! | **1.8** | 1.4! |
| Monster Swarm (aura) | 6.3 | 7.2 | 4.5 | 4.9 | 7.8 | 7.8 | 6.8 | **4.5** | 4.4 |
| Monster Swarmling (aura) | 7.4 | 8.5 | 5.3 | 5.8 | 9.1 | 9.1 | 8.0 | **5.3** | 5.2 |
| Elite mark (MarkElite) | 13.2 | 15.1 | 9.5 | 10.3 | 16.3 | 16.3 | 14.3 | **9.5** | 8.8 |
| Pickup ExtraLife heart | 4.3 | 4.9 | 3.0 | 3.3 | 5.2 | 5.3 | 4.6 | **3.0** | 3.3 |
| Pickup generic (Mat_Ship_Accent) | 7.3 | 8.4 | 5.3 | 5.7 | 9.1 | 9.1 | 7.9 | **5.3** | 5.0 |
| Pickup Shield (Mat_Shield, a=.22) | 5.6 | 6.4 | 4.0 | 4.4 | 6.9 | 6.9 | 6.0 | **4.0** | 2.1! |
| Spike (damaging, emission pulse avg) | 5.7 | 6.5 | 4.1 | 4.4 | 7.0 | 7.0 | 6.1 | **4.1** | 4.2 |
| Player hull (steel) | 1.9 | 2.1 | 1.3 | 1.5 | 2.3 | 2.3 | 2.0 | **1.3** | 1.0 |
| Player accent (amber) | 7.3 | 8.4 | 5.3 | 5.7 | 9.1 | 9.1 | 7.9 | **5.3** | 5.0 |
| Asteroid | 1.2 | 1.3 | 1.2 | 1.1 | 1.4 | 1.4 | 1.3 | **1.1** | 1.1 |

### D. HUD tokens: text on HudPlate (Surface @ 0.72) composited over each world's lit floor / sky, and on solid panels

| Token | hex | on Void | on Surface | on Surface2 | min over W1-7 plate-over-floor | min plate-over-sky | AA text (4.5) |
|---|---|---|---|---|---|---|---|
| primary | `#D4A04A` | 8.4 | 7.8 | 7.3 | 7.0 | 6.9 | pass |
| secondary | `#6AA8C8` | 7.6 | 7.0 | 6.6 | 6.4 | 6.2 | pass |
| accent | `#C8CED6` | 12.4 | 11.6 | 10.8 | 10.5 | 10.2 | pass |
| danger | `#B85A28` | 4.3 | 4.0 | 3.7 | 3.6 | 3.5 | large only |
| focus | `#E8C878` | 12.2 | 11.3 | 10.6 | 10.2 | 10.0 | pass |
| disabled | `#3A4450` | 2.0 | 1.9 | 1.7 | 1.7 | 1.6 | FAIL |
| shop_locked_text | `#A8B2BC` | 9.2 | 8.5 | 8.0 | 7.7 | 7.5 | pass |
| shop_poor_text | `#F0C8A8` | 12.7 | 11.8 | 11.0 | 10.7 | 10.4 | pass |

Disabled `#3A4450` is by design a de-emphasised state (WCAG exempts inactive controls); never use it for live information (hull/shield/cooldown).

### E. Colour-blind check (Machado 2009, severity 1.0, linear RGB). dE = CIE76 between the two apparent colours; luminance ratio is CVD-invariant enough to rely on

| Pair | normal dE | protan dE | deutan dE | tritan dE | luminance ratio | verdict |
|---|---|---|---|---|---|---|
| Player bolt vs Enemy bolt | 69 | 41 | 30 | 71 | 2.63 | OK |
| Player spread vs Enemy bolt | 51 | 34 | 23 | 53 | 2.06 | OK |
| Player pierce vs Enemy bolt | 98 | 67 | 73 | 107 | 2.45 | OK |
| Brute aura vs Swarmling aura (orange vs green) | 109 | 36 | 19 | 120 | 2.07 | OK |
| Brute aura vs Swarm aura (orange vs cyan) | 100 | 53 | 69 | 117 | 1.75 | OK |
| Mid01 red vs Sniper cyan | 108 | 68 | 88 | 131 | 2.17 | OK |
| ExtraLife heart vs generic pickup | 74 | 73 | 47 | 45 | 1.73 | OK |
| ExtraLife heart vs Shield pickup | 106 | 42 | 69 | 127 | 1.31 | OK |
| Ring boss aimed (yellow) vs radial (orange) | 54 | 33 | 22 | 49 | 1.90 | OK |
| Ring ExtraLife (red) vs Swarm drop (cyan) | 128 | 48 | 50 | 142 | 2.70 | OK |
| Token Primary vs Danger (hull vs low hull) | 32 | 25 | 19 | 31 | 1.97 | OK |
| Token Primary vs Secondary (hull vs shield bar) | 77 | 71 | 77 | 67 | 1.11 | OK |

### F. Proposed readability fixes and re-measured contrast (typical shading, min over the listed worlds)

Floor proposal: `Brightness` W3 1.00 -> 0.88, W4 0.94 -> 0.80 (PaletteMilli index 12 of rows 3 and 4 only). Hue, chroma and all other worlds unchanged. Lit floor becomes W3 `#033A2D`, W4 `#482603` (was `#044233` / `#552D04`).

| Item | change | before min | after min (W1-7, with floor change) | after at W5 (darkest) | after worst world |
|---|---|---|---|---|---|
| Floor only, no object change: Enemy Mid01 red | W3/W4 brightness only | 2.5 | 2.8 | 4.2 | W3 |
| Floor only: Brute/Boss pulse avg | W3/W4 brightness only | 2.8 | 3.1 | 4.6 | W3 |
| Floor only: Brute/Boss pulse min | W3/W4 brightness only | 1.9 | 2.2 | 3.2 | W3 |
| Scout/Gunner/Drone/SwarmPod body | Mat_Enemy emission = Danger x 0.5 | 1.0 | 1.6 | 2.3 | W3 |
| Scout/Gunner/Drone/SwarmPod body | Mat_Enemy emission = Danger x 0.7 | 1.0 | 2.1 | 3.1 | W3 |
| Scout/Gunner/Drone/SwarmPod body | Mat_Enemy emission = Danger x 0.9 | 1.0 | 2.8 | 4.1 | W3 |
| Mid01 body | emission (0.82,0.1,0.12) x1.35 | 2.5 | 3.6 | 5.4 | W3 |
| Brute/Boss pulse min | pulse = 1.0 + 0.2 sin (idle), 0.9 + 0.35 sin (charge): min 0.8 / 0.55 -> use base 1.0, amp .2 / .3: min 0.8 / 0.7 | 1.9 | 3.3 | 4.8 | W3 |
| Brute/Boss pulse min (charging) | min pulse 0.7 | 1.9 | 2.7 | 4.1 | W3 |
| Player hull (steel) | + rim emission Secondary x 0.15 | 1.5 | 1.8 | 2.6 | W3 |
| Player hull (steel) | + rim emission Secondary x 0.25 | 1.5 | 1.9 | 2.8 | W3 |
| Player hull (steel) | + rim emission Secondary x 0.35 | 1.5 | 2.1 | 3.1 | W3 |
| Asteroid | albedo x1.4 + emission Secondary x 0.1 | 1.1 | 1.5 | 2.2 | W3 |
| Asteroid | albedo x1.4 + emission Secondary x 0.16 | 1.1 | 1.6 | 2.3 | W3 |

Emission needed (Secondary `#6AA8C8` steel, lit floor with W3/W4 fix, typical shading) for neutral hazards that cannot rely on an accent colour:

| Object | albedo | k for 2:1 | k for 3:1 |
|---|---|---|---|
| Asteroid (as is) | `#615247` | 0.51 | 0.71 |
| Asteroid albedo x1.5 | `#917A6B` | 0.34 | 0.61 |
| Player hull | `#738594` | 0.31 | 0.59 |

Supplementary: CIELAB lightness difference dL* (object minus lit floor) for the low-ratio items. WCAG ratio is pessimistic for dark-on-dark; dL* >= 15 reads clearly, 8-15 is visible but weak, < 8 is camouflage.

| Item | W1 | W2 | W3 | W4 | W5 | W6 | W7 |
|---|---|---|---|---|---|---|---|
| Asteroid | 7 | 13 | -3 | -2 | 16 | 16 | 7 |
| Player hull (steel) | 20 | 27 | 11 | 12 | 30 | 30 | 21 |
| Enemy Scout/Gunner/Drone/SwarmPod (mat default) | 11 | 17 | 1 | 3 | 20 | 20 | 11 |
| Enemy Mid01 (red) | 35 | 41 | 26 | 27 | 45 | 45 | 36 |
| Monster Brute / Boss (pulse min) | 28 | 34 | 18 | 20 | 37 | 37 | 28 |

dL* after proposed fixes (floor W3/W4 brightness change in all rows; object change as named):

| Item / change | W1 | W2 | W3 | W4 | W5 | W6 | W7 | WCAG min |
|---|---|---|---|---|---|---|---|---|
| Asteroid, floor fix only | 7 | 13 | 0 | 2 | 16 | 16 | 7 | 1.0 |
| Asteroid, floor fix + albedo x1.5 | 19 | 25 | 13 | 15 | 29 | 29 | 20 | 1.6 |
| Asteroid, floor fix + albedo x1.5 + Secondary emission x0.15 | 21 | 27 | 15 | 17 | 30 | 30 | 21 | 1.7 |
| Asteroid, floor fix + albedo x1.5 + Secondary emission x0.30 | 24 | 31 | 18 | 20 | 34 | 34 | 25 | 1.9 |
| Generic enemy body, floor fix + emission Danger x0.9 | 35 | 41 | 28 | 30 | 44 | 44 | 35 | 2.8 |
