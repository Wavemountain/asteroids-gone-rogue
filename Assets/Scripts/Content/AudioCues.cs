using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// CC0 clips loaded from Resources/Audio. Mute and volumes persist in PlayerPrefs.
    /// UI click is a distinct Kenney click — not the hangar purchase confirmation.
    /// 0.40 monster / spike banks follow the AtmosBot Kenney CC0 list
    /// (retro-modern chip/arcade, AAA mix polish). Distinct from UI clicks.
    /// 0.44 ExtraLife: powerUp7 @ 0.82 (threeTone2 alt), miss phaserDown3 @ 0.48,
    /// duck 0.22s @ 0.5 on pickup only. Hit pool impactMetal_000-003 ±4% + punch 0.55.
    /// Weapons stay on the 0.43 Kenney pools. Rail charge is phaserUp3,
    /// release is laserLarge_002 plus lowFrequency_explosion_001, doctrine pick is jingles_NES03.
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
        public const float HitPunchScale = 0.55f;
        public const float HitPitchJitter = 0.04f;
        public const float LightKillScale = 0.9f;
        public const float LightKillPitchJitter = 0.05f;
        public const float ExtraLifePickupScale = 0.82f;
        public const float ExtraLifeAltScale = 0.78f;
        public const float ExtraLifeMissScale = 0.48f;
        public const float ExtraLifeDuckSeconds = 0.22f;
        public const float ExtraLifeDuckScale = 0.5f;
        public const float PickupMinorScale = 0.38f;
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

        // Rail mix sits under Brute death (1.04 + 1.12 layer) and over a lone bolt.
        // Charge 0.6 and the hold loop 0.2 stay under Swarm hit (0.88).
        public const float RailChargeScale = 0.6f;
        public const float RailChargePitch = 0.95f;
        public const float RailHoldLoopScale = 0.2f;
        public const float RailHoldPitchMin = 0.9f;
        public const float RailHoldPitchMax = 1.15f;
        public const float RailHoldFadeSeconds = 0.08f;
        public const float RailShotScale = 1.0f;
        public const float RailShotPitch = 0.92f;
        public const float RailShotPitchJitter = 0.03f;
        public const float RailThumpLayerScale = 0.5f;
        public const float RailDuckSeconds = 0.18f;
        public const float RailDuckScale = 0.6f;
        public const float DoctrinePickScale = 0.68f;
        public const float DoctrinePickDuckSeconds = 0.3f;
        public const float DoctrinePickDuckScale = 0.4f;
        public const float WorldMusicScale = MusicPlan.WorldMusicScale;
        public const float BossMusicScale = MusicPlan.BossMusicScale;
        public const float MusicCrossfadeSeconds = MusicPlan.MusicCrossfadeSeconds;
        public const float EliteStingScale = MusicPlan.EliteStingScale;
        public const float EliteStingDuckSeconds = MusicPlan.EliteStingDuckSeconds;
        public const float EliteStingDuckScale = MusicPlan.EliteStingDuckScale;

        public static AudioCues Instance { get; private set; }

        private AudioSource _sfx;
        private AudioSource _vary;
        private AudioSource _music;
        private AudioSource _musicB;
        private AudioSource _hangarLayer;
        private AudioSource _railRise;
        private AudioSource _railHold;
        private AudioClip _shoot;
        private AudioClip[] _boltShots;
        private AudioClip _shootSpread;
        private AudioClip[] _spreadShots;
        private AudioClip _shootPierce;
        private AudioClip _shootSeeker;
        private AudioClip _shootTwin;
        private AudioClip _shootRicochet;
        private AudioClip _railCharge;
        private AudioClip _railHoldClip;
        private AudioClip _railShot;
        private AudioClip _railThump;
        private AudioClip _doctrinePick;
        private float _railHoldFadeUntil;
        private float _railHoldFadeStart;
        private float _railHoldFadeFrom;
        private AudioClip _shootEnemy;
        private AudioClip _hit;
        private AudioClip[] _hits;
        private AudioClip _hitPunch;
        private AudioClip _hitLight;
        private AudioClip _extraLife;
        private AudioClip _extraLifeAlt;
        private AudioClip _extraLifeMiss;
        private AudioClip _pickupMinor;
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
        private AudioClip[] _worldMusic;
        private AudioClip _bossMusic;
        private AudioClip _bossMusicFinal;
        private AudioClip _eliteSting;
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
        private bool _crossfading;
        private float _fadeElapsed;
        private float _fadeSeconds;
        private AudioClip _incomingClip;
        private bool _musicHeld;

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
            _musicB = CreateSource("MusicSourceB", true);
            _hangarLayer = CreateSource("HangarLayerSource", true);
            _railRise = CreateSource("RailRiseSource", false);
            _railHold = CreateSource("RailHoldSource", true);
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

        public void PlayShootRail()
        {
            PlayRailRelease();
        }

        /// <summary>
        /// phaserUp3 one-shot on its own source so release or cancel can stop it.
        /// </summary>
        public void PlayRailChargeRise()
        {
            _railHoldFadeUntil = 0f;
            if (_railHold != null && _railHold.isPlaying)
            {
                _railHold.Stop();
            }

            AudioClip clip = _railCharge != null ? _railCharge : _shootSeeker;
            if (_railRise == null || clip == null)
            {
                return;
            }

            _railRise.Stop();
            _railRise.clip = clip;
            _railRise.loop = false;
            _railRise.pitch = RailChargePitch;
            _railRise.volume = _muted ? 0f : _sfxVolume * RailChargeScale;
            if (!_muted)
            {
                _railRise.Play();
            }
        }

        /// <summary>
        /// Hold loop only after the charge is full. Pitch runs 0.9 to 1.15
        /// across one more hold-length (charge 1 to 2).
        /// </summary>
        public void TickRailHold(float charge01)
        {
            if (_railHold == null || _railHoldClip == null || charge01 < 1f)
            {
                return;
            }

            if (_muted)
            {
                _railHold.volume = 0f;
                return;
            }

            float along = Mathf.Clamp01(charge01 - 1f);
            _railHoldFadeUntil = 0f;
            _railHold.pitch = Mathf.Lerp(RailHoldPitchMin, RailHoldPitchMax, along);
            _railHold.volume = _sfxVolume * RailHoldLoopScale;
            if (_railHold.clip != _railHoldClip)
            {
                _railHold.clip = _railHoldClip;
                _railHold.loop = true;
            }

            if (!_railHold.isPlaying)
            {
                _railHold.Play();
            }
        }

        /// <summary>
        /// Stops the charge-rise source immediately. The hold loop fades out.
        /// </summary>
        public void StopRailCharge()
        {
            if (_railRise != null && _railRise.isPlaying)
            {
                _railRise.Stop();
            }

            if (_railHold == null || !_railHold.isPlaying)
            {
                return;
            }

            _railHoldFadeFrom = _railHold.volume;
            _railHoldFadeStart = Time.unscaledTime;
            _railHoldFadeUntil = _railHoldFadeStart + RailHoldFadeSeconds;
        }

        public void PlayRailRelease()
        {
            AudioClip shot = _railShot != null ? _railShot : _shootPierce;
            float pitch = RailShotPitch + Random.Range(-RailShotPitchJitter, RailShotPitchJitter);
            PlayPitched(shot, RailShotScale, pitch);
            AudioClip thump = _railThump != null ? _railThump : _bruteDeathLayer;
            Play(thump, RailThumpLayerScale);
            DuckMusic(RailDuckSeconds, RailDuckScale);
        }

        public void PlayDoctrinePick()
        {
            AudioClip clip = _doctrinePick != null ? _doctrinePick : _purchase;
            Play(clip, DoctrinePickScale);
            DuckMusic(DoctrinePickDuckSeconds, DoctrinePickDuckScale);
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
            PlayPooledPitched(_hits, 1f, _hit, HitPitchJitter);
            if (_hitPunch != null)
            {
                Play(_hitPunch, HitPunchScale);
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
                AudioClip light = _enemyDeathLight != null ? _enemyDeathLight : _enemyDeath;
                PlayPitched(light, LightKillScale, 1f + Random.Range(-LightKillPitchJitter, LightKillPitchJitter));
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

        public void PlayExtraLifePickup()
        {
            if (_extraLife != null)
            {
                Play(_extraLife, ExtraLifePickupScale);
            }
            else
            {
                Play(_extraLifeAlt != null ? _extraLifeAlt : _purchase, ExtraLifeAltScale);
            }

            DuckMusic(ExtraLifeDuckSeconds, ExtraLifeDuckScale);
        }

        public void PlayExtraLifeMiss()
        {
            Play(_extraLifeMiss != null ? _extraLifeMiss : _abort, ExtraLifeMissScale);
        }

        public void PlayPickupMinor()
        {
            Play(_pickupMinor != null ? _pickupMinor : _uiClick, PickupMinorScale);
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
            float duration = Mathf.Max(0.05f, seconds);
            float target = Mathf.Clamp01(scale);
            float now = Time.unscaledTime;
            if (now < _duckUntil)
            {
                _duckTarget = Mathf.Min(_duckTarget, target);
                _duckUntil = Mathf.Max(_duckUntil, now + duration);
                _duckSeconds = Mathf.Max(_duckSeconds, duration);
            }
            else
            {
                _duckSeconds = duration;
                _duckTarget = target;
                _duckUntil = now + _duckSeconds;
            }

            float remain = _duckUntil - now;
            _duckScale = Mathf.Lerp(1f, _duckTarget, Mathf.Clamp01(remain / _duckSeconds));
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
                if (MusicPlan.UsesBossTrack(waveIndex))
                {
                    PlayBossMusic(waveIndex);
                }
                else
                {
                    PlayWorldMusic(waveIndex);
                }
            }
            else
            {
                CrossfadeTo(_hangarAmbience, HangarMusicScale, HangarMusicPitch);
            }
        }

        public void PlayWorldMusic(int wave)
        {
            AudioClip worldClip = WorldClipFor(wave);
            CrossfadeTo(worldClip, MusicPlan.WorldMusicScale, ArenaMusicPitch);
        }

        public void PlayBossMusic(int wave)
        {
            AudioClip bossClip = BossClipFor(wave);
            if (bossClip == null)
            {
                bossClip = WorldClipFor(wave);
            }

            CrossfadeTo(bossClip, MusicPlan.BossMusicScale, ArenaMusicPitch);
        }

        public void PlayEliteSting()
        {
            AudioClip sting = _eliteSting != null ? _eliteSting : _waveClear;
            Play(sting, MusicPlan.EliteStingScale);
            DuckMusic(MusicPlan.EliteStingDuckSeconds, MusicPlan.EliteStingDuckScale);
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
            bool refresh = false;
            if (_duckUntil > 0f)
            {
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

                refresh = true;
            }

            if (_railHoldFadeUntil > 0f)
            {
                refresh = true;
            }

            HoldMusicForPause();
            if (_crossfading && !_musicHeld)
            {
                _fadeElapsed += Time.unscaledDeltaTime;
                float ramp = MusicPlan.CrossfadeRamp(_fadeElapsed, _fadeSeconds);
                if (ramp >= 1f)
                {
                    FinishCrossfade();
                }

                refresh = true;
            }

            if (refresh)
            {
                ApplyVolumes();
            }
        }

        private void PlayLoop(AudioClip clip, float scale, float pitch)
        {
            if (_music == null || clip == null)
            {
                return;
            }

            _crossfading = false;
            _fadeElapsed = 0f;
            if (_musicB != null)
            {
                _musicB.Stop();
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
            if (!_muted && !_musicHeld)
            {
                _music.Play();
            }
            else
            {
                _music.Stop();
            }
        }

        private void CrossfadeTo(AudioClip clip, float scale, float pitch)
        {
            if (clip == null || _music == null)
            {
                return;
            }

            bool sameAsCurrent = _currentMusic == clip && (_crossfading || _music.isPlaying);
            bool sameAsIncoming = _crossfading && _incomingClip == clip;
            if (!MusicPlan.ShouldStartCrossfade(sameAsCurrent, sameAsIncoming))
            {
                _musicScale = scale;
                _musicPitch = pitch;
                ApplyVolumes();
                return;
            }

            _musicScale = scale;
            _musicPitch = pitch;
            if (_currentMusic == null || !_music.isPlaying || _musicB == null)
            {
                PlayLoop(clip, scale, pitch);
                return;
            }

            if (_crossfading)
            {
                FinishCrossfade();
            }

            _incomingClip = clip;
            _musicB.clip = clip;
            _musicB.loop = true;
            _musicB.pitch = pitch;
            _musicB.volume = 0f;
            _fadeElapsed = 0f;
            _fadeSeconds = MusicPlan.MusicCrossfadeSeconds;
            _crossfading = true;
            if (!_muted && !_musicHeld)
            {
                _musicB.Play();
            }

            ApplyVolumes();
        }

        private void FinishCrossfade()
        {
            AudioSource outgoing = _music;
            _music = _musicB;
            _musicB = outgoing;
            if (_musicB != null)
            {
                _musicB.Stop();
            }

            _currentMusic = _incomingClip;
            _crossfading = false;
            _fadeElapsed = 0f;
        }

        private void HoldMusicForPause()
        {
            bool paused = Time.timeScale <= 0.0001f;
            if (paused == _musicHeld)
            {
                return;
            }

            _musicHeld = paused;
            if (paused)
            {
                if (_music != null && _music.isPlaying)
                {
                    _music.Pause();
                }

                if (_musicB != null && _musicB.isPlaying)
                {
                    _musicB.Pause();
                }

                return;
            }

            ApplyVolumes();
        }

        private AudioClip WorldClipFor(int wave)
        {
            string trackName = MusicPlan.WorldTrackName(wave);
            int trackIndex = 0;
            for (int nameIndex = 0; nameIndex < MusicPlan.WorldTrackNames.Length; nameIndex++)
            {
                if (MusicPlan.WorldTrackNames[nameIndex] == trackName)
                {
                    trackIndex = nameIndex;
                    break;
                }
            }

            AudioClip loaded = null;
            if (_worldMusic != null && trackIndex >= 0 && trackIndex < _worldMusic.Length)
            {
                loaded = _worldMusic[trackIndex];
            }

            if (loaded != null)
            {
                return loaded;
            }

            if (wave >= HighWaveMusicWave && _arenaHigh != null)
            {
                return _arenaHigh;
            }

            return _arenaLoop;
        }

        private AudioClip BossClipFor(int wave)
        {
            string bossName = MusicPlan.BossTrackName(wave);
            if (bossName == MusicPlan.BossFinalTrack)
            {
                return _bossMusicFinal != null ? _bossMusicFinal : _bossMusic;
            }

            return _bossMusic != null ? _bossMusic : _bossMusicFinal;
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

            if (_railRise != null)
            {
                _railRise.volume = _muted ? 0f : _sfxVolume * RailChargeScale;
            }

            ApplyRailHoldVolume();

            float musicLevel = _muted ? 0f : _musicVolume * _musicScale * _duckScale;
            float fadeRamp = _crossfading ? MusicPlan.CrossfadeRamp(_fadeElapsed, _fadeSeconds) : 1f;
            if (_music != null)
            {
                _music.pitch = _musicPitch;
                _music.volume = _crossfading ? musicLevel * (1f - fadeRamp) : musicLevel;
                if (_muted || _musicHeld)
                {
                    if (_music.isPlaying)
                    {
                        _music.Pause();
                    }
                }
                else if (_currentMusic != null && !_music.isPlaying)
                {
                    _music.UnPause();
                    if (!_music.isPlaying)
                    {
                        _music.Play();
                    }
                }
            }

            if (_musicB != null)
            {
                _musicB.pitch = _musicPitch;
                _musicB.volume = _crossfading ? musicLevel * fadeRamp : 0f;
                if (!_crossfading)
                {
                    if (_musicB.isPlaying)
                    {
                        _musicB.Stop();
                    }
                }
                else if (_muted || _musicHeld)
                {
                    if (_musicB.isPlaying)
                    {
                        _musicB.Pause();
                    }
                }
                else if (_incomingClip != null && !_musicB.isPlaying)
                {
                    _musicB.UnPause();
                    if (!_musicB.isPlaying)
                    {
                        _musicB.Play();
                    }
                }
            }

            ApplyHangarLayer();
        }

        private void ApplyRailHoldVolume()
        {
            if (_railHold == null)
            {
                return;
            }

            if (_railHoldFadeUntil > 0f)
            {
                float dur = RailHoldFadeSeconds;
                float u = dur <= 0.0001f ? 1f : (Time.unscaledTime - _railHoldFadeStart) / dur;
                if (u >= 1f)
                {
                    _railHold.volume = 0f;
                    if (_railHold.isPlaying)
                    {
                        _railHold.Stop();
                    }

                    _railHoldFadeUntil = 0f;
                    return;
                }

                _railHold.volume = Mathf.Lerp(_railHoldFadeFrom, 0f, Mathf.Clamp01(u));
                return;
            }

            if (_railHold.isPlaying)
            {
                _railHold.volume = _muted ? 0f : _sfxVolume * RailHoldLoopScale;
            }
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
            _railCharge = Resources.Load<AudioClip>("Audio/Sfx/phaserUp3");
            if (_railCharge == null)
            {
                _railCharge = Resources.Load<AudioClip>("Audio/Sfx/phaserUp5");
            }

            _railHoldClip = Resources.Load<AudioClip>("Audio/Sfx/engineCircular_001");
            _railShot = Resources.Load<AudioClip>("Audio/Sfx/laserLarge_002");
            if (_railShot == null)
            {
                _railShot = Resources.Load<AudioClip>("Audio/Sfx/laserLarge_000");
            }

            _railThump = Resources.Load<AudioClip>("Audio/Sfx/lowFrequency_explosion_001");
            if (_railThump == null)
            {
                _railThump = Resources.Load<AudioClip>("Audio/Sfx/lowFrequency_explosion_000");
            }

            _doctrinePick = Resources.Load<AudioClip>("Audio/Sfx/jingles_NES03");
            if (_doctrinePick == null)
            {
                _doctrinePick = Resources.Load<AudioClip>("Audio/Sfx/threeTone2");
            }

            _shootEnemy = Resources.Load<AudioClip>("Audio/Sfx/laserSmall_001");
            _hit = Resources.Load<AudioClip>("Audio/Sfx/impactMetal_003");
            _hits = LoadPool(
                "Audio/Sfx/impactMetal_000",
                "Audio/Sfx/impactMetal_001",
                "Audio/Sfx/impactMetal_002",
                "Audio/Sfx/impactMetal_003");
            _hitPunch = Resources.Load<AudioClip>("Audio/Sfx/impactMetal_000");
            _hitLight = Resources.Load<AudioClip>("Audio/Sfx/impactMetal_001");
            _extraLife = Resources.Load<AudioClip>("Audio/Sfx/powerUp7");
            _extraLifeAlt = Resources.Load<AudioClip>("Audio/Sfx/threeTone2");
            _extraLifeMiss = Resources.Load<AudioClip>("Audio/Sfx/phaserDown3");
            _pickupMinor = Resources.Load<AudioClip>("Audio/Sfx/pepSound1");
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
            _worldMusic = new AudioClip[MusicPlan.WorldTrackNames.Length];
            for (int worldSlot = 0; worldSlot < MusicPlan.WorldTrackNames.Length; worldSlot++)
            {
                _worldMusic[worldSlot] = Resources.Load<AudioClip>("Audio/Music/" + MusicPlan.WorldTrackNames[worldSlot]);
            }

            _bossMusic = Resources.Load<AudioClip>("Audio/Music/" + MusicPlan.BossTrack);
            _bossMusicFinal = Resources.Load<AudioClip>("Audio/Music/" + MusicPlan.BossFinalTrack);
            _eliteSting = Resources.Load<AudioClip>("Audio/Sfx/" + MusicPlan.EliteStingTrack);
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
