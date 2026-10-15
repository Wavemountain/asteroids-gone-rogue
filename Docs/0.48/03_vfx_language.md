# 0.48 · 03 VFX-språk

Det som finns idag: `JuiceBurst` (en skalande additiv quad per anrop, `new GameObject` varje gång, inga partiklar), `CombatJuice.Ripple/HitSpark/KillBloom/PickupBloom/FlashScreen/Shake` och `MeshHitFlash`. Inget script använder `ParticleSystem` än, men modulen `com.unity.modules.particlesystem` finns i manifestet.

**Beslut:** behåll `JuiceBurst` som "flash"-steg (steg 1). Lägg till en poolad `Assets/Scripts/Combat/VfxKit.cs` med 3 förbyggda `ParticleSystem` (Chunks, Smoke, Sparks) som skjuts med `Emit(EmitParams, n)`. Ingen Instantiate per effekt. Material: `Shader.Find("Particles/Standard Unlit")` (används redan), additiv för sparks och alpha för smoke. Pool-gräns: 600 partiklar totalt.

## 1. Färg-tokens (läggs i `UiTheme` som `Vfx*`, eller i ny `Core/VfxTokens.cs` Unity-fri)
| Token | Hex | Används |
|---|---|---|
| `VfxHot` | #FFF2D6 | flash-kärna (alla) |
| `VfxPrimary` | #D4A04A | spelarens skott, kill-flash heavy, pickups |
| `VfxSteel` | #6AA8C8 | sköld, Pierce, Seeker |
| `VfxDanger` | #B85A28 | fiende-skott, fiende-hit, tell |
| `VfxEmber` | #E07A3A | chunk-glöd (fade → `VfxSmoke`) |
| `VfxSmoke` | #2A2F38 α0.55 | rök (desat, aldrig färgad) |
| `VfxHeal` | #8FD18F | endast hälsopickup |
Inga rena RGB-primärer, ingen magenta och ingen neon-cyan (`#00FFFF` ersätts med VfxSteel).

## 2. Explosioner: flash → chunks → smoke
| Storlek | Används för | Flash (JuiceBurst) | Chunks | Smoke | Shake | Total tid |
|---|---|---|---|---|---|---|
| S | swarm, liten asteroid, Scout | VfxHot, 0.08 s, scale 0.2→1.2 | 6 st, 3–6 u/s, 0.35 s, Ember→Smoke, size 0.10 | 2 st, 0.6 s, size 0.5→1.0 | 0 | 0.6 s |
| M | Mid01/Gunner/Drone, mellan-asteroid | VfxHot→VfxPrimary, 0.12 s, 0.25→1.6 | 12 st, 4–8 u/s, 0.5 s, size 0.14 | 4 st, 0.9 s, 0.7→1.6 | 0.08 | 0.9 s |
| L | Brute, Boss, död spelare | VfxHot→VfxPrimary, 0.18 s, 0.4→2.45 (= `KillBloom heavy`) + Ripple VfxPrimary 0.4 s | 24 st, 5–11 u/s, 0.7 s, size 0.18 | 8 st, 1.4 s, 1.0→2.4 | via `CombatJuice.HeavyKill` | 1.4 s |

Koppla in via `CombatJuice.ThreatDamaged(target, exploded, heavy)` med storlek från `EnemyKind`. Asteroidstorlek från `Asteroid`.

## 3. Mynningsflammor per `FireMode`
Flamman sitter i en quad vid nosen (JuiceBurst-stil, poolad), alltid ≤0.06 s så att den inte döljer skotten.

| FireMode | Form | Färg | Tid | Skala | Extra |
|---|---|---|---|---|---|
| Bolt | punkt | VfxPrimary | 0.04 s | 0.25 | – |
| Spread | solfjäder 3 streck | VfxPrimary | 0.05 s | 0.35 | – |
| Pierce | lång spik | VfxSteel | 0.05 s | 0.45 långsmal | 2 sparks |
| Twin | 2 punkter (per pipa) | VfxPrimary | 0.04 s | 0.22 ×2 | – |
| Seeker | ring | VfxSteel | 0.06 s | 0.30 | – |
| Ricochet | punkt + 2 sparks | VfxPrimary | 0.04 s | 0.25 | 2 sparks |
| Rail | linje plus ringexplosion | VfxHot→VfxPrimary | 0.06 s | 0.6 | 6 sparks + projektil-ljus (02 §3). Laddglöden som finns idag (`ChargeGlowCompose`) är oförändrad |

## 4. Övrigt
| Effekt | Spec |
|---|---|
| Hit spark (`JuiceBurst.HitSpark`) | behåll quaden och lägg till 4 sparks, 0.15 s, 6–10 u/s, VfxHot→VfxDanger på fiende och VfxHot→VfxSteel på sköld |
| Sköld-rippel (`CombatJuice.PlayerShieldHit`) | Ripple VfxSteel α0.5, 0.25 s, radie 1.0→1.6. Break: 2 ringar (0, 0.08 s) + 10 sparks |
| Pickup sparkle (`CombatJuice.PickupBloom`) | idle: 1 sparkle per 0.4 s, 0.5 s, size 0.06, VfxPrimary (VfxHeal för hälsa). Uppplock: ring 0.2 s + 8 sparks uppåt |

## 5. Reduce effects-multiplikatorer (befintliga `EffectScale`-konstanter)
- Partikelantal: × `ParticleCountMul 0.4` (avrunda uppåt, min 1)
- Storlek: × `ParticleSizeMul 0.7`
- Flash-alfa: × `BurstAlphaMul 0.4`, slutskala × `BurstEndMul 0.6`
- Kill-flash (skärm): `AllowsKillFlash(reduce)` = av
- Smoke: × 0.5 tid. Mynningsflamma: alfa × 0.6
- Sparks på Pierce, Ricochet och Rail: max 2
- Ingenting här påverkar fiendens eller projektilernas egen färg och emission, det är läsbarhet.
