# Credits — Asteroids gone rogue

All audio in this repository is **CC0** (Creative Commons Zero / public domain). No paywalled assets.

Support the authors if you can. Kenney asks for optional credit to `Kenney.nl`.

## Sound effects

Pack: **Kenney Sci-Fi Sounds** (CC0)  
Author: Kenney  
Source: https://kenney.nl/assets/sci-fi-sounds  
Mirror used: https://opengameart.org/content/sci-fi-sounds (`sci-fi_sounds.zip`)

| Cue | File in repo | Original pack file |
| --- | --- | --- |
| Shoot (bolt) | `Assets/Resources/Audio/Sfx/laserSmall_000.ogg` | `Audio/laserSmall_000.ogg` |
| Shoot (spread) | `Assets/Resources/Audio/Sfx/laserRetro_000.ogg` | `Audio/laserRetro_000.ogg` |
| Shoot (pierce) | `Assets/Resources/Audio/Sfx/laserLarge_000.ogg` | `Audio/laserLarge_000.ogg` |
| Enemy bolt | `Assets/Resources/Audio/Sfx/laserSmall_001.ogg` | `Audio/laserSmall_001.ogg` |
| Hit | `Assets/Resources/Audio/Sfx/impactMetal_003.ogg` | `Audio/impactMetal_003.ogg` |
| Hit punch layer | `Assets/Resources/Audio/Sfx/impactMetal_000.ogg` | `Audio/impactMetal_000.ogg` |
| SwarmPod / Mid hit | `Assets/Resources/Audio/Sfx/impactMetal_001.ogg` | `Audio/impactMetal_001.ogg` (no punch) |
| Brute hit | `Assets/Resources/Audio/Sfx/impactMetal_002.ogg` | `Audio/impactMetal_002.ogg` |
| Swarm hit | `Assets/Resources/Audio/Sfx/forceField_001.ogg` | `Audio/forceField_001.ogg` |
| Asteroid split | `Assets/Resources/Audio/Sfx/explosionCrunch_000.ogg` | `Audio/explosionCrunch_000.ogg` |
| Brute death | `Assets/Resources/Audio/Sfx/explosionCrunch_002.ogg` | `Audio/explosionCrunch_002.ogg` |
| Enemy death | `Assets/Resources/Audio/Sfx/explosionCrunch_003.ogg` | `Audio/explosionCrunch_003.ogg` |
| Brute spawn | `Assets/Resources/Audio/Sfx/lowFrequency_explosion_000.ogg` | `Audio/lowFrequency_explosion_000.ogg` |
| Enemy death punch | `Assets/Resources/Audio/Sfx/impactMetal_000.ogg` | `Audio/impactMetal_000.ogg` (layered; asteroid crunch stays `explosionCrunch_000` only) |
| SwarmPod / Mid death | `Assets/Resources/Audio/Sfx/explosionCrunch_001.ogg` | `Audio/explosionCrunch_001.ogg` (no punch) |
| Player damage | `Assets/Resources/Audio/Sfx/forceField_000.ogg` | `Audio/forceField_000.ogg` |

Pack: **Kenney Digital Audio** (CC0)  
Author: Kenney  
Source: https://kenney.nl/assets/digital-audio

| Cue | File in repo | Original pack file |
| --- | --- | --- |
| SwarmPod spawn | `Assets/Resources/Audio/Sfx/phaserUp5.ogg` | `Audio/phaserUp5.ogg` |
| Swarm / Swarmling spawn | `Assets/Resources/Audio/Sfx/phaserUp2.ogg` | `Audio/phaserUp2.ogg` |
| Swarm death | `Assets/Resources/Audio/Sfx/phaserDown3.ogg` | `Audio/phaserDown3.ogg` |

Pack: **Kenney Interface Sounds** (CC0)  
Author: Kenney  
Source: https://kenney.nl/assets/interface-sounds  
Mirror used: https://opengameart.org/content/interface-sounds (`kenney_interfaceSounds.zip`)

| Cue | File in repo | Original pack file |
| --- | --- | --- |
| UI click | `Assets/Resources/Audio/Sfx/click_002.ogg` | `Audio/click_002.ogg` |
| Hangar purchase | `Assets/Resources/Audio/Sfx/confirmation_002.ogg` | `Audio/confirmation_002.ogg` |
| Abort whoosh | `Assets/Resources/Audio/Sfx/minimize_005.ogg` | `Audio/minimize_005.ogg` |
| Arena world swap | `Assets/Resources/Audio/Sfx/maximize_008.ogg` | `Audio/maximize_008.ogg` |
| World 3 entry | `Assets/Resources/Audio/Sfx/maximize_008.ogg` | same clip, hotter + short bed duck |

Pack: **Kenney Music Jingles** (CC0)  
Author: Kenney  
Source: https://kenney.nl/assets/music-jingles  
Mirror used: https://opengameart.org/content/85-short-music-jingles (`jingleSounds_Kenney.zip`)

| Cue | File in repo | Original pack file |
| --- | --- | --- |
| Wave clear | `Assets/Resources/Audio/Sfx/jingles_PIZZA07.ogg` | `OGG/jingles_PIZZA/jingles_PIZZA07.ogg` |
| Far Drift award | `Assets/Resources/Audio/Sfx/jingles_PIZZA16.ogg` | `Audio/Pizzicato jingles/jingles_PIZZI16.ogg` |

Kenney license text (from the Sci-Fi Sounds pack) is kept at `Assets/Audio/Kenney_License.txt`.

## Fonts

Pack: **Kenney Fonts** (CC0)  
Author: Kenney  
Source: https://kenney.nl/assets/kenney-fonts

| Use | File in repo | Original pack file |
| --- | --- | --- |
| Display (title / world / medals / headers) | `Assets/Resources/Fonts/KenneyFuture.ttf` | `Fonts/Kenney Future.ttf` |
| Body (HUD / shop / status) | `Assets/Resources/Fonts/KenneyFutureNarrow.ttf` | `Fonts/Kenney Future Narrow.ttf` |

License text from the pack is at `Assets/Resources/Fonts/Kenney_Fonts_License.txt`.

## Music

| Cue | File in repo | Track | Author | License | Source |
| --- | --- | --- | --- | --- | --- |
| Arena loop | `Assets/Resources/Audio/Music/OutThere.ogg` | Space Music: Out There | yd | CC0 | https://opengameart.org/content/space-music-out-there |
| Hangar ambience | `Assets/Resources/Audio/Music/spacelifeNo14.ogg` | Spacelife #14 | yd | CC0 | https://opengameart.org/content/spacelife-14 |

Hangar plays `spacelifeNo14` as a denser two-layer bed (pitch 0.94 + 1.02). Arena plays `OutThere` louder at concert pitch so the two beds stay distinct. Abort ducks the current bed under the whoosh. SwarmPod spawn ducks the bed briefly (0.62s gap / 0.86 scale) so `phaserUp5` cuts clutter. Brute uses a heavy bank (`lowFrequency_explosion_000` spawn / `impactMetal_002` hit / `explosionCrunch_002` death). Swarm uses a lighter buzz bank (`phaserUp2` spawn / `forceField_001` hit / `phaserDown3` death) — not UI clicks. Wave 10 Far Drift plays `jingles_PIZZA16` instead of the wave-clear sting. World 3 entry reuses `maximize_008` slightly hotter with a short bed duck (no new jingle).

## Engine wiring

`AudioCues` loads these clips from `Resources/Audio` at runtime (so Play Mode does not depend on Inspector references). Hangar UI has **Mute** plus **SFX** and **Music** sliders; Mute also plays the UI click. Values persist in PlayerPrefs. Local best score / wave / world persist under `agr.best.*`. Hangar medals persist under `agr.hangar.medals`. `UiFonts` loads Kenney Future / Future Narrow from `Resources/Fonts` (LegacyRuntime fallback; never Arial).
