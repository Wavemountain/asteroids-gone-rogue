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
| Shoot | `Assets/Resources/Audio/Sfx/laserSmall_000.ogg` | `Audio/laserSmall_000.ogg` |
| Hit | `Assets/Resources/Audio/Sfx/impactMetal_000.ogg` | `Audio/impactMetal_000.ogg` |
| Asteroid split | `Assets/Resources/Audio/Sfx/explosionCrunch_000.ogg` | `Audio/explosionCrunch_000.ogg` |
| Enemy death | `Assets/Resources/Audio/Sfx/explosionCrunch_003.ogg` | `Audio/explosionCrunch_003.ogg` |
| Player damage | `Assets/Resources/Audio/Sfx/forceField_000.ogg` | `Audio/forceField_000.ogg` |

Pack: **Kenney Interface Sounds** (CC0)  
Author: Kenney  
Source: https://kenney.nl/assets/interface-sounds  
Mirror used: https://opengameart.org/content/interface-sounds (`kenney_interfaceSounds.zip`)

| Cue | File in repo | Original pack file |
| --- | --- | --- |
| Hangar purchase | `Assets/Resources/Audio/Sfx/confirmation_002.ogg` | `Audio/confirmation_002.ogg` |
| Arena world swap | `Assets/Resources/Audio/Sfx/maximize_008.ogg` | `Audio/maximize_008.ogg` |

Pack: **Kenney Music Jingles** (CC0)  
Author: Kenney  
Source: https://kenney.nl/assets/music-jingles  
Mirror used: https://opengameart.org/content/85-short-music-jingles (`jingleSounds_Kenney.zip`)

| Cue | File in repo | Original pack file |
| --- | --- | --- |
| Wave clear | `Assets/Resources/Audio/Sfx/jingles_PIZZA07.ogg` | `OGG/jingles_PIZZA/jingles_PIZZA07.ogg` |

Kenney license text (from the Sci-Fi Sounds pack) is kept at `Assets/Audio/Kenney_License.txt`.

## Music

| Cue | File in repo | Track | Author | License | Source |
| --- | --- | --- | --- | --- | --- |
| Arena loop | `Assets/Resources/Audio/Music/OutThere.ogg` | Space Music: Out There | yd | CC0 | https://opengameart.org/content/space-music-out-there |
| Hangar ambience | `Assets/Resources/Audio/Music/spacelifeNo14.ogg` | Spacelife #14 | yd | CC0 | https://opengameart.org/content/spacelife-14 |

## Engine wiring

`AudioCues` loads these clips from `Resources/Audio` at runtime (so Play Mode does not depend on Inspector references). Hangar UI has **Mute** plus **SFX** and **Music** sliders; values persist in PlayerPrefs.
