using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// CC0 clips loaded from Resources/Audio. Mute and volumes persist in PlayerPrefs.
    /// UI click is a distinct Kenney click — not the hangar purchase confirmation.
    /// 0.40 monster / spike banks follow the AtmosBot Kenney CC0 list
    /// (retro-modern chip/arcade, AAA mix polish). Distinct from UI clicks.
    /// </summary>
    public sealed class AudioCues : MonoBehaviour
    {
        public const string MuteKey = "agr.audio.mute";
        public const string SfxKey = "agr.audio.sfx";
        public const string MusicKey = "agr.audio.music";
        public const float DefaultSfxVolume = 0.8f;
        public const float DefaultMusicVolume = 0.28f;
        public const float HangarMusicScale = 0.48f;
        public const float ArenaMusicScale = 0.65f;
        public const int HighWaveMusicWave = 8;
        public const float HangarMusicPitch = 0.94f;
        public const float ArenaMusicPitch = 1f;
        public const float HangarLayerScale = 0.22f;
        public const float HangarLayerPitch = 1.02f;
        public const float AbortDuckScale = 0.18f;
        public const float AbortDuckSeconds = 0.55f;
        public const float HitPunchScale = 1.22f;
        public const float EnemyDeathPunchScale = 0.78f;
        public const float SwarmPodSpawnScale = 0.86f;
        public const float SwarmPodDuckSeconds = 0.36f;
        public const float SwarmPodDuckScale = 0.38f;
        public const float SwarmPodSpawnGapSeconds = 0.62f;
        public const float FarDriftAwardScale = 0.94f;
        public const float World3ChangeScale = 1.12f;
        public const float World3DuckSeconds = 0.42f;
        public const float World3DuckScale = 0.4f;
        public const float BruteSpawnScale = 0.98f;
        public const float BruteSpawnDuckSeconds = 0.32f;
        public const float BruteSpawnDuckScale = 0.4f;
        public const float SwarmSpawnScale = 0.82f;
        public const float BruteHitScale = 1f;
        public const float SwarmHitScale = 0.88f;
        public const float BruteDeathScale = 1.04f;
        public const float BruteDeathLayerScale = 1.12f;
        public const float SwarmDeathScale = 0.84f;
        public const float SwarmSpawnLayerScale = 0.36f;
        public const float SwarmHitPitchJitter = 0.06f;
        public const float HazardActivateScale = 0.94f;
        public const float HazardActivateDuckSeconds = 0.28f;
        public const float HazardActivateDuckScale = 0.42f;
        public const float HazardHitScale = 0.72f;
        public const float CreditsLoopScale = 0.55f;
        public const float CreditsOpenDuckSeconds = 0.35f;
        public const float CreditsOpenDuckScale = 0.4f;
        public const float WaveClearScale = 0.88f;
        public const float FailScale = 0.78f;
        public const float FailLayerScale = 0.4f;
        public const float FailDuckSeconds = 0.55f;
        public const float FailDuckScale = 0.3f;
        public const float RetryScale = 0.75f;
        public const float BoltPitchJitter = 0.03f;
        public const float SpreadShotScale = 1.05f;
        public const float PierceShotScale = 1.12f;
        public const float TwinLayerScale = 0.45f;
        public const float SeekerShotScale = 0.72f;
        public const float RicochetShotScale = 0.88f;
        public const float RicochetPitchJitter = 0.04f;

        public static AudioCues Instance { get; private set; }

        private AudioSource _sfx;
        private AudioSource _vary;
        private AudioSource _music;
        private AudioSource _hangarLayer;
        private AudioClip _shoot;
        private AudioClip[] _boltShots;
        private AudioClip _shootSpread;
        private AudioClip[] _spreadShots;
        private AudioClip _shootPierce;
        private AudioClip _shootSeeker;
        private AudioClip _shootTwin;
        private AudioClip _shootRicochet;
        private AudioClip _shootEnemy;
        private AudioClip _hit;
        private AudioClip _hitPunch;
        private AudioClip _hitLight;
        private AudioClip _asteroidSplit;
        private AudioClip _enemyDeath;
        private AudioClip _enemyDeathPunch;
        private AudioClip _enemyDeathLight;
        private AudioClip _playerDamage;
        private AudioClip _purchase;
        private AudioClip _uiClick;
        private AudioClip _abort;
        private AudioClip _worldChange;
        private AudioClip _waveClear;
        private AudioClip _farDriftAward;
        private AudioClip _swarmPodSpawn;
        private AudioClip _bruteSpawn;
        private AudioClip[] _bruteHits;
        private AudioClip _bruteDeath;
        private AudioClip _bruteDeathLayer;
        private AudioClip _swarmSpawn;
        private AudioClip _swarmSpawnLayer;
        private AudioClip[] _swarmHits;
        private AudioClip[] _swarmDeaths;
        private AudioClip _hazardActivate;
        private AudioClip[] _hazardHits;
        private AudioClip _arenaLoop;
        private AudioClip _arenaHigh;
        private AudioClip _hangarAmbience;
        private AudioClip _creditsLoop;
        private AudioClip _creditsOpen;
        private AudioClip _creditsClose;
        private AudioClip _fail;
        private AudioClip _failLayer;
        private AudioClip _retry;
        private bool _muted;
        private float _sfxVolume = DefaultSfxVolume;
        private float _musicVolume = DefaultMusicVolume;
        private AudioClip _currentMusic;
        private float _musicScale = HangarMusicScale;
        private float _musicPitch = HangarMusicPitch;
        private float _duckScale = 1f;
        private float _duckUntil;
        private float _duckSeconds = AbortDuckSeconds;
        private float _duckTarget = AbortDuckScale;
        private bool _creditsMusic;
        private float _lastSwarmPodSpawn;

        public bool Muted
        {
            get { return _muted; }
        }

        public float SfxVolume
        {
            get { return _sfxVolume; }
        }

        public float MusicVolume
        {
            get { return _musicVolume; }
        }

        private void Awake()
        {
            Instance = this;
            _sfx = CreateSource("SfxSource", false);
            _vary = CreateSource("VarySfxSource", false);
            _music = CreateSource("MusicSource", true);
            _hangarLayer = CreateSource("HangarLayerSource", true);
            LoadClips();
            _muted = PlayerPrefs.GetInt(MuteKey, 0) == 1;
            _sfxVolume = PlayerPrefs.GetFloat(SfxKey, DefaultSfxVolume);
            _musicVolume = PlayerPrefs.GetFloat(MusicKey, DefaultMusicVolume);
            ApplyVolumes();
        }

        public void PlayShoot()
        {
            PlayPooledPitched(_boltShots, 1f, _shoot, BoltPitchJitter);
        }

        public void PlayShootSpread()
        {
            PlayPooled(_spreadShots, SpreadShotScale, _shootSpread != null ? _shootSpread : _shoot);
        }

        public void PlayShootPierce()
        {
            Play(_shootPierce != null ? _shootPierce : _shoot, PierceShotScale);
        }

        public void PlayShootSeeker()
        {
            Play(_shootSeeker != null ? _shootSeeker : _shootPierce, SeekerShotScale);
        }

        public void PlayShootTwin()
        {
            Play(_shootEnemy != null ? _shootEnemy : _shoot, 1f);
            Play(_shootTwin != null ? _shootTwin : _shoot, TwinLayerScale);
        }

        public void PlayShootRicochet()
        {
            PlayPitched(
                _shootRicochet != null ? _shootRicochet : _shootSpread,
                RicochetShotScale,
                1f + Random.Range(-RicochetPitchJitter, RicochetPitchJitter));
        }

        public void PlayEnemyShoot()
        {
            Play(_shootEnemy != null ? _shootEnemy : _shoot, 0.7f);
        }

        public void PlayHit()
        {
            Play(_hit, HitPunchScale);
            if (_hitPunch != null)
            {
                Play(_hitPunch, 0.58f);
            }
        }

        public void PlayHit(EnemyKind kind)
        {
            if (UsesLightThreatSfx(kind))
            {
                Play(_hitLight != null ? _hitLight : _hit, 0.92f);
                return;
            }

            if (kind == EnemyKind.Brute)
            {
                PlayPooled(_bruteHits, BruteHitScale, _bruteSpawn);
                return;
            }

            if (kind == EnemyKind.Swarm)
            {
                PlayPooledPitched(_swarmHits, SwarmHitScale, _swarmSpawn, SwarmHitPitchJitter);
                return;
            }

            PlayHit();
        }

        public void PlayExplosion()
        {
            PlayAsteroidSplit();
        }

        public void PlayAsteroidSplit()
        {
            Play(_asteroidSplit);
        }

        public void PlayEnemyDeath()
        {
            Play(_enemyDeath);
            if (_enemyDeathPunch != null)
            {
                Play(_enemyDeathPunch, EnemyDeathPunchScale);
            }
        }

        public void PlayEnemyDeath(EnemyKind kind)
        {
            if (UsesLightThreatSfx(kind))
            {
                Play(_enemyDeathLight != null ? _enemyDeathLight : _enemyDeath, 0.9f);
                return;
            }

            if (kind == EnemyKind.Brute)
            {
                Play(_bruteDeath != null ? _bruteDeath : _bruteSpawn, BruteDeathScale);
                if (_bruteDeathLayer != null)
                {
                    Play(_bruteDeathLayer, BruteDeathLayerScale);
                }

                return;
            }

            if (kind == EnemyKind.Swarm)
            {
                PlayPooled(_swarmDeaths, SwarmDeathScale, _swarmSpawn);
                return;
            }

            PlayEnemyDeath();
        }

        public static bool UsesLightThreatSfx(EnemyKind kind)
        {
            return kind == EnemyKind.Mid01 || kind == EnemyKind.SwarmPod || kind == EnemyKind.Swarmling;
        }

        public static bool UsesMonsterThreatSfx(EnemyKind kind)
        {
            return kind == EnemyKind.Brute || kind == EnemyKind.Swarm;
        }

        public void PlayMonsterSpawn(EnemyKind kind)
        {
            if (kind == EnemyKind.Brute)
            {
                Play(_bruteSpawn != null ? _bruteSpawn : _bruteDeath, BruteSpawnScale);
                DuckMusic(BruteSpawnDuckSeconds, BruteSpawnDuckScale);
                return;
            }

            if (kind == EnemyKind.Swarm)
            {
                Play(_swarmSpawn != null ? _swarmSpawn : _swarmPodSpawn, SwarmSpawnScale);
                if (_swarmSpawnLayer != null)
                {
                    Play(_swarmSpawnLayer, SwarmSpawnLayerScale);
                }

                return;
            }

            if (kind == EnemyKind.Swarmling)
            {
                Play(_swarmSpawn != null ? _swarmSpawn : _swarmPodSpawn, 0.42f);
            }
        }

        public void PlayHazardActivate()
        {
            Play(_hazardActivate != null ? _hazardActivate : _playerDamage, HazardActivateScale);
            DuckMusic(HazardActivateDuckSeconds, HazardActivateDuckScale);
        }

        public void PlayHazardHit()
        {
            PlayPooled(_hazardHits, HazardHitScale, _shootSpread);
        }

        public void PlayPlayerDamage()
        {
            Play(_playerDamage, 1.15f);
            Play(_hit, 0.85f);
        }

        public void PlayHangarPurchase()
        {
            Play(_purchase);
        }

        public void PlayUiClick()
        {
            Play(_uiClick != null ? _uiClick : _purchase, 0.88f);
        }

        public void PlayAbortWhoosh()
        {
            Play(_abort != null ? _abort : _worldChange);
            DuckMusic(AbortDuckSeconds, AbortDuckScale);
        }

        public void DuckMusic(float seconds, float scale)
        {
            _duckSeconds = Mathf.Max(0.05f, seconds);
            _duckTarget = Mathf.Clamp01(scale);
            _duckUntil = Time.unscaledTime + _duckSeconds;
            _duckScale = _duckTarget;
            ApplyVolumes();
        }

        public void PlayCreditsOpen()
        {
            Play(_creditsOpen != null ? _creditsOpen : _waveClear);
            DuckMusic(CreditsOpenDuckSeconds, CreditsOpenDuckScale);
        }

        public void PlayCreditsLoop()
        {
            _creditsMusic = true;
            PlayLoop(_creditsLoop != null ? _creditsLoop : _hangarAmbience, CreditsLoopScale, 1f);
        }

        public void PlayCreditsClose()
        {
            Play(_creditsClose != null ? _creditsClose : _farDriftAward);
        }

        public void StopCreditsMusic()
        {
            _creditsMusic = false;
            SyncMusicToPhase(GamePhase.Hangar);
        }

        public void PlayWaveFail()
        {
            Play(_fail != null ? _fail : _playerDamage, FailScale);
            if (_failLayer != null)
            {
                Play(_failLayer, FailLayerScale);
            }

            DuckMusic(FailDuckSeconds, FailDuckScale);
        }

        public void PlayRetry()
        {
            Play(_retry != null ? _retry : _shootTwin, RetryScale);
        }

        public void PlayWaveClear()
        {
            Play(_waveClear, WaveClearScale);
        }

        public void PlayFarDriftAward()
        {
            Play(_farDriftAward != null ? _farDriftAward : _waveClear, FarDriftAwardScale);
        }

        public void PlayWorldChange()
        {
            PlayWorldChange(0);
        }

        public void PlayWorldChange(int world)
        {
            if (world == MedalCatalog.World3EntryWorld)
            {
                Play(_worldChange, World3ChangeScale);
                DuckMusic(World3DuckSeconds, World3DuckScale);
                return;
            }

            Play(_worldChange);
        }

        public void PlaySwarmPodSpawn()
        {
            if (Time.unscaledTime - _lastSwarmPodSpawn < SwarmPodSpawnGapSeconds)
            {
                return;
            }

            _lastSwarmPodSpawn = Time.unscaledTime;
            Play(_swarmPodSpawn != null ? _swarmPodSpawn : _worldChange, SwarmPodSpawnScale);
            DuckMusic(SwarmPodDuckSeconds, SwarmPodDuckScale);
        }

        public void SyncMusicToPhase(GamePhase phase)
        {
            SyncMusicToPhase(phase, 1);
        }

        public void SyncMusicToPhase(GamePhase phase, int waveIndex)
        {
            if (_creditsMusic && phase != GamePhase.Playing)
            {
                return;
            }

            _creditsMusic = false;
            if (phase == GamePhase.Playing)
            {
                AudioClip clip = _arenaLoop;
                if (waveIndex >= HighWaveMusicWave && _arenaHigh != null)
                {
                    clip = _arenaHigh;
                }

                PlayLoop(clip, ArenaMusicScale, ArenaMusicPitch);
            }
            else
            {
                PlayLoop(_hangarAmbience, HangarMusicScale, HangarMusicPitch);
            }
        }

        public void SetMuted(bool muted)
        {
            _muted = muted;
            PlayerPrefs.SetInt(MuteKey, muted ? 1 : 0);
            PlayerPrefs.Save();
            ApplyVolumes();
        }

        public void ToggleMute()
        {
            SetMuted(!_muted);
        }

        public void SetSfxVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(SfxKey, _sfxVolume);
            PlayerPrefs.Save();
            ApplyVolumes();
        }

        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(MusicKey, _musicVolume);
            PlayerPrefs.Save();
            ApplyVolumes();
        }

        private void Play(AudioClip clip)
        {
            Play(clip, 1f);
        }

        private void Play(AudioClip clip, float scale)
        {
            if (_sfx != null && clip != null && !_muted)
            {
                _sfx.PlayOneShot(clip, Mathf.Clamp(scale, 0f, 1.4f));
            }
        }

        private void PlayPooled(AudioClip[] clips, float scale, AudioClip fallback)
        {
            AudioClip clip = PickClip(clips);
            Play(clip != null ? clip : fallback, scale);
        }

        private void PlayPooledPitched(AudioClip[] clips, float scale, AudioClip fallback, float pitchJitter)
        {
            AudioClip clip = PickClip(clips);
            PlayPitched(clip != null ? clip : fallback, scale, 1f + Random.Range(-pitchJitter, pitchJitter));
        }

        private void PlayPitched(AudioClip clip, float scale, float pitch)
        {
            if (_vary != null && clip != null && !_muted)
            {
                _vary.pitch = Mathf.Clamp(pitch, 0.5f, 1.5f);
                _vary.PlayOneShot(clip, Mathf.Clamp(scale, 0f, 1.4f));
            }
        }

        private static AudioClip[] LoadPool(params string[] keys)
        {
            AudioClip[] clips = new AudioClip[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                clips[i] = Resources.Load<AudioClip>(keys[i]);
            }

            return clips;
        }

        private static AudioClip PickClip(AudioClip[] clips)
        {
            if (clips == null || clips.Length == 0)
            {
                return null;
            }

            int live = 0;
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i] != null)
                {
                    live++;
                }
            }

            if (live == 0)
            {
                return null;
            }

            int pick = Random.Range(0, live);
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i] == null)
                {
                    continue;
                }

                if (pick == 0)
                {
                    return clips[i];
                }

                pick--;
            }

            return clips[0];
        }

        private void Update()
        {
            if (_duckUntil <= 0f)
            {
                return;
            }

            if (Time.unscaledTime >= _duckUntil)
            {
                _duckScale = 1f;
                _duckUntil = 0f;
            }
            else
            {
                float remain = _duckUntil - Time.unscaledTime;
                _duckScale = Mathf.Lerp(1f, _duckTarget, Mathf.Clamp01(remain / _duckSeconds));
            }

            ApplyVolumes();
        }

        private void PlayLoop(AudioClip clip, float scale, float pitch)
        {
            if (_music == null || clip == null)
            {
                return;
            }

            _musicScale = scale;
            _musicPitch = pitch;
            if (_currentMusic == clip && _music.isPlaying)
            {
                ApplyVolumes();
                return;
            }

            _currentMusic = clip;
            _music.clip = clip;
            _music.loop = true;
            ApplyVolumes();
            if (!_muted)
            {
                _music.Play();
            }
            else
            {
                _music.Stop();
            }
        }

        private void ApplyVolumes()
        {
            if (_sfx != null)
            {
                _sfx.volume = _muted ? 0f : _sfxVolume;
            }

            if (_vary != null)
            {
                _vary.volume = _muted ? 0f : _sfxVolume;
            }

            if (_music != null)
            {
                _music.pitch = _musicPitch;
                _music.volume = _muted ? 0f : _musicVolume * _musicScale * _duckScale;
                if (_muted)
                {
                    _music.Pause();
                }
                else if (_currentMusic != null && !_music.isPlaying)
                {
                    _music.Play();
                }
            }

            ApplyHangarLayer();
        }

        private void ApplyHangarLayer()
        {
            if (_hangarLayer == null || _hangarAmbience == null)
            {
                return;
            }

            bool hangar = _currentMusic == _hangarAmbience && !_muted;
            _hangarLayer.pitch = HangarLayerPitch;
            _hangarLayer.volume = hangar ? _musicVolume * HangarLayerScale * _duckScale : 0f;
            if (!hangar)
            {
                if (_hangarLayer.isPlaying)
                {
                    _hangarLayer.Stop();
                }

                return;
            }

            if (_hangarLayer.clip != _hangarAmbience)
            {
                _hangarLayer.clip = _hangarAmbience;
                _hangarLayer.loop = true;
            }

            if (!_hangarLayer.isPlaying)
            {
                _hangarLayer.Play();
            }
        }

        private void LoadClips()
        {
            _shoot = Resources.Load<AudioClip>("Audio/Sfx/laserSmall_000");
            _boltShots = LoadPool(
                "Audio/Sfx/laserSmall_000",
                "Audio/Sfx/laserSmall_001",
                "Audio/Sfx/laserSmall_002");
            _shootSpread = Resources.Load<AudioClip>("Audio/Sfx/laserRetro_000");
            _spreadShots = LoadPool(
                "Audio/Sfx/laserRetro_000",
                "Audio/Sfx/laserRetro_001",
                "Audio/Sfx/laserRetro_002");
            _shootPierce = Resources.Load<AudioClip>("Audio/Sfx/laserLarge_000");
            _shootSeeker = Resources.Load<AudioClip>("Audio/Sfx/phaserUp5");
            _shootTwin = Resources.Load<AudioClip>("Audio/Sfx/twoTone1");
            _shootRicochet = Resources.Load<AudioClip>("Audio/Sfx/zap1");
            _shootEnemy = Resources.Load<AudioClip>("Audio/Sfx/laserSmall_001");
            _hit = Resources.Load<AudioClip>("Audio/Sfx/impactMetal_003");
            _hitPunch = Resources.Load<AudioClip>("Audio/Sfx/impactMetal_000");
            _hitLight = Resources.Load<AudioClip>("Audio/Sfx/impactMetal_001");
            _asteroidSplit = Resources.Load<AudioClip>("Audio/Sfx/explosionCrunch_000");
            _enemyDeath = Resources.Load<AudioClip>("Audio/Sfx/explosionCrunch_003");
            _enemyDeathPunch = Resources.Load<AudioClip>("Audio/Sfx/impactMetal_000");
            _enemyDeathLight = Resources.Load<AudioClip>("Audio/Sfx/explosionCrunch_001");
            _playerDamage = Resources.Load<AudioClip>("Audio/Sfx/forceField_000");
            _purchase = Resources.Load<AudioClip>("Audio/Sfx/confirmation_002");
            _uiClick = Resources.Load<AudioClip>("Audio/Sfx/click_002");
            _abort = Resources.Load<AudioClip>("Audio/Sfx/minimize_005");
            _worldChange = Resources.Load<AudioClip>("Audio/Sfx/maximize_008");
            _waveClear = Resources.Load<AudioClip>("Audio/Sfx/jingles_HIT07");
            if (_waveClear == null)
            {
                _waveClear = Resources.Load<AudioClip>("Audio/Sfx/jingles_HIT04");
            }

            if (_waveClear == null)
            {
                _waveClear = Resources.Load<AudioClip>("Audio/Sfx/jingles_PIZZA07");
            }

            _farDriftAward = Resources.Load<AudioClip>("Audio/Sfx/jingles_PIZZA16");
            if (_farDriftAward == null)
            {
                _farDriftAward = Resources.Load<AudioClip>("Audio/Sfx/jingles_HIT12");
            }
            _swarmPodSpawn = Resources.Load<AudioClip>("Audio/Sfx/phaserUp5");
            // AtmosBot 0.40 list — Kenney CC0, distinct from UI clicks.
            _bruteSpawn = Resources.Load<AudioClip>("Audio/Sfx/lowThreeTone");
            _bruteHits = LoadPool(
                "Audio/Sfx/impactMetal_000",
                "Audio/Sfx/impactMetal_001",
                "Audio/Sfx/impactMetal_002");
            _bruteDeath = Resources.Load<AudioClip>("Audio/Sfx/explosionCrunch_003");
            _bruteDeathLayer = Resources.Load<AudioClip>("Audio/Sfx/lowFrequency_explosion_000");
            _swarmSpawn = Resources.Load<AudioClip>("Audio/Sfx/phaseJump1");
            _swarmSpawnLayer = Resources.Load<AudioClip>("Audio/Sfx/slime_000");
            _swarmHits = LoadPool(
                "Audio/Sfx/laserSmall_000",
                "Audio/Sfx/laserSmall_001",
                "Audio/Sfx/laserSmall_002",
                "Audio/Sfx/laserSmall_003",
                "Audio/Sfx/laserSmall_004");
            _swarmDeaths = LoadPool(
                "Audio/Sfx/zap1",
                "Audio/Sfx/spaceTrash1",
                "Audio/Sfx/spaceTrash2",
                "Audio/Sfx/spaceTrash3");
            _hazardActivate = Resources.Load<AudioClip>("Audio/Sfx/forceField_001");
            _hazardHits = LoadPool(
                "Audio/Sfx/laserRetro_000",
                "Audio/Sfx/laserRetro_001",
                "Audio/Sfx/laserRetro_002");
            _arenaLoop = Resources.Load<AudioClip>("Audio/Music/MissionPlausible");
            if (_arenaLoop == null)
            {
                _arenaLoop = Resources.Load<AudioClip>("Audio/Music/OutThere");
            }

            _arenaHigh = Resources.Load<AudioClip>("Audio/Music/TimeDriving");
            _hangarAmbience = Resources.Load<AudioClip>("Audio/Music/spacelifeNo14");
            _creditsLoop = Resources.Load<AudioClip>("Audio/Music/SpaceCadet");
            _creditsOpen = Resources.Load<AudioClip>("Audio/Sfx/jingles_NES07");
            _creditsClose = Resources.Load<AudioClip>("Audio/Sfx/jingles_NES12");
            // Atmos fail — Kenney Music Loops Game Over one-shot; phaserDown3 fallback only.
            _fail = Resources.Load<AudioClip>("Audio/Music/GameOver");
            if (_fail == null)
            {
                _fail = Resources.Load<AudioClip>("Audio/Sfx/GameOver");
            }

            if (_fail == null)
            {
                _fail = Resources.Load<AudioClip>("Audio/Sfx/phaserDown3");
            }

            _failLayer = Resources.Load<AudioClip>("Audio/Sfx/lowDown");
            _retry = Resources.Load<AudioClip>("Audio/Sfx/twoTone1");
            if (_creditsClose == null)
            {
                _creditsClose = _farDriftAward;
            }
        }

        private AudioSource CreateSource(string sourceName, bool loop)
        {
            GameObject go = new GameObject(sourceName);
            go.transform.SetParent(transform, false);
            AudioSource source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = loop;
            source.spatialBlend = 0f;
            source.dopplerLevel = 0f;
            return source;
        }
    }
}
