# 0.48 · 05 Ljud: död, vinst, meny + voice-limit + crossfade

## 0. Läget vid `7a64598` (0.46-QA-resterna)
Koden är läst, inget är lyssnat på:
- Equal-power-crossfade: **fixad**. `MusicCrossfade.Weights` använder cos/sin. `PowerSum` finns.
- Avbruten crossfade och sameAsCurrent: **fixad**. `MusicCrossfade.Retarget`/`KeepSameClip`/`SettledSame` och `MusicPlan.ShouldStartCrossfade(sameAsCurrent, sameAsIncoming)`. `AudioCues.FinishCrossfade` finns.
- Pitch och skala per källa under fade: **fixad**. `_outPitch`/`_inPitch` per `_music`/`_musicB` och `MusicRetarget.OutScale/InScale`.
- Hangar-layer-pop: **fixad**. `ApplyHangarLayer` viktar `HangarLayerScale` med de aktuella crossfade-vikterna (`_outHangar`/`_inHangar`).
→ Ingen crossfade-kod behöver göras om i 0.48. Kvar att göra är bara death low-pass (§3) och voice-limit (§4). SpeltestBot bör lyssna på W3→boss och hangar→run en gång.

## 1. Nya SFX (staged, äkta OggS, CC0 Kenney)
Sökväg: `/workspace/agr-0.48/Assets/Resources/Audio/Sfx/`. Ladda med `Resources.Load<AudioClip>("Audio/Sfx/<namn>")`.
Mätt med ffmpeg volumedetect. Referens: `click_002` mean −16.2 dB vid skala 1.0.

| Fil | Källa (Kenney) | Bearbetning | Längd | mean/max dB | Skala | Används |
|---|---|---|---|---|---|---|
| `agr_ui_hover` | Interface Sounds `select_002` | – | 0.04 s | −15.9/−1.0 | 0.45 | fokus/hover (gap 0.06 s) |
| `agr_ui_confirm` | Interface `confirmation_001` | – | 0.29 s | −11.3/−0.9 | 0.55 | bekräfta, run-summary-total |
| `agr_ui_back` | Interface `back_002` | – | 0.07 s | −17.0/−1.0 | 0.85 | tillbaka/avbryt |
| `agr_ui_open` | Interface `open_002` | – | 0.31 s | −16.4/−0.9 | 0.70 | `UiTransition` in |
| `agr_ui_close` | Interface `close_002` | – | 0.31 s | −16.4/−1.0 | 0.70 | `UiTransition` ut |
| `agr_death_whoosh` | Sci-fi Sounds `forceField_004` | rate ×0.7, lowpass 2.2 kHz, fade 0.6–1.1 s | 1.36 s | −11.5/−0.8 | 0.60 | slow-mo-whoosh vid död |
| `agr_wave_whoosh` | Sci-fi `doorOpen_000` | highpass 120 Hz, fade 0.4–0.7 s | 0.53 s | −17.9/−1.2 | 0.75 | våg klar → boon, värld-banner |
Familj: alla fem menyljud kommer från samma Interface Sounds-pack med samma glas/klick-timbre. `click_002`, `minimize_005` och `maximize_008` behålls för sina nuvarande användningar.

## 2. Sekvenser
**Död** (spelare, sista liv). All tid är unscaled:
- t0: `Time.timeScale` 1→0.3 över 0.1 s, håll till 0.6 s, sedan 0.3→1 på 0.25 s (GameBots slow-mo). Med Reduce: samma, eftersom ljud och timescale inte är effekter.
- t0: `PlayPlayerDamage` som idag, plus `agr_death_whoosh` ×0.60 på `_vary` (pitch 1.0) och L-explosion (03).
- t0: low-pass på musik 22 000 → 600 Hz över 0.6 s (§3). SFX filtreras inte.
- t0+0.6 s: stinger = **återanvänd `lowDown`** (finns, −21.4 dB mean, skala 1.0), samtidigt som `GameOver`-musiken crossfadas in via befintlig `CrossfadeTo`. Low-pass släpps 600 → 22 000 Hz över 0.4 s när GameOver-spåret tar över.
**Våg klar / värld klar**: `agr_wave_whoosh` ×0.75 vid banner-in, sedan befintlig `_waveClear` (`jingles_HIT07`, `WaveClearScale 0.88`) 0.15 s senare. Ducka musik 0.6× i 0.8 s (befintlig `_duckScale`-väg). Värld klar: `_worldChange` (`maximize_008`) som idag och därefter whoosh.
**Meny**: hover = `agr_ui_hover`, confirm = `agr_ui_confirm`, back = `agr_ui_back`, open och close kopplas till `UiTransition`. `PlayUiClick` (`click_002`) finns kvar för toggles och sliders. `PlayHangarPurchase` (`confirmation_002`) behålls, köp ska låta annorlunda än confirm.

## 3. Musik-low-pass vid död
Det finns ingen `AudioMixer` i repot, och det ska inte införas nu. Lägg `AudioLowPassFilter` på `MusicSource`, `MusicSourceB` och `HangarLayerSource` i `AudioCues` (`AddComponent<AudioLowPassFilter>()` i samma kod som `CreateSource`). `cutoffFrequency = 22000`, `lowpassResonanceQ = 1`. Ny `public void SetMusicLowPass(float hz, float seconds)` som lerpar exponentiellt (log-frekvens) på unscaled tid. Filtret står på 22 kHz när det inte används, vilket är i praktiken bypass. CPU-kostnaden är försumbar.

## 4. Voice-limit och prioritet
AudioManager har `m_RealVoiceCount 32` och `m_VirtualVoiceCount 512`. One-shots går via `PlayOneShot` på `_sfx`/`_vary`, och den befintliga `CueVoiceBudget` sköter gap och tells. Lägg till en räknare per kategori (aktiva one-shots registreras med sluttid = clip.length/pitch) i `CueVoiceBudget`:

| Kategori | Max samtidiga | `AudioSource.priority` | Steal-regel |
|---|---|---|---|
| Musik (A/B + hangar layer) | 3 | 0 | aldrig |
| Tells/telegraf (`_tell`, spike tick) | 2 | 16 | nyare tell ersätter äldre av samma typ, Brute/Boss stjäl alltid |
| Spelare skada/död/sköld | 2 | 24 | aldrig stjäls |
| UI | 2 | 32 | hover droppas om confirm/back spelar |
| Spelarvapen | 6 | 64 | droppa nytt om fullt (Rail undantaget, stjäl äldsta vapen) |
| Fiendedöd/explosion | 6 | 80 | stjäl äldsta, L-explosion stjäl S |
| Hits (`impactMetal_*`) | 4 | 96 | droppa nytt (gap `SameClipGapSeconds` gäller redan) |
| Pickups | 2 | 112 | droppa nytt |
| Fiendeskott | 3 | 128 | droppa nytt |
Totalt ≤ 30, under 32 real voices. Eftersom `PlayOneShot` delar källa per kategori kan källan inte "stjälas" på riktigt. Steal betyder att det nya skottet prioriteras och att räknaren för det äldsta släpps. För riktig steal kan GameBot senare ge varje kategori en liten pool `AudioSource[]`. Det är valfritt.
