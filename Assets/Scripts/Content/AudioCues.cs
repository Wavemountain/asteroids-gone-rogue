using System.Collections;
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
        public const float ShieldHitScale = 1.00f;
        public const float ShieldHitThudScale = 0.50f;
        public const float ShieldBreakScale = 1.00f;
        public const float ShieldBreakHumScale = 0.50f;
        public const float ArmorHitScale = 0.60f;
        public const float PickupShieldScale = 1.00f;
        public const float PickupHealthScale = 1.00f;
        public const float PickupRapidScale = 1.00f;
        public const float PickupScoreScale = 1.00f;
        public const float BruteTellScale = 1.00f;
        public const float NestTellScale = 1.00f;
        public const float BossAimedTellScale = 1.00f;
        public const float BossRadialTellScale = 1.00f;
        public const float GunnerTellScale = 0.60f;
        public const float SpikeTickScale = 1.00f;
        public const float InvulnEndScale = 0.80f;
        public const float HullDuckSeconds = 0.15f;
        public const float HullDuckScale = 0.7f;

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
        private AudioSource _tell;
        private AudioSource _tick;
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
        private AudioClip _uiHover;
        private AudioClip _uiConfirm;
        private AudioClip _uiBack;
        private AudioClip _uiOpen;
        private AudioClip _uiClose;
        private float _uiHoverT;
        private float _uiPriorityUntil;
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
        private AudioClip _shieldHit;
        private AudioClip _shieldBreak;
        private AudioClip _armorHit;
        private AudioClip _pickupShield;
        private AudioClip _pickupHealth;
        private AudioClip _pickupRapid;
        private AudioClip _pickupScore;
        private AudioClip _tellBrute;
        private AudioClip _tellNest;
        private AudioClip _tellBossAimed;
        private AudioClip _tellBossRadial;
        private AudioClip _tellSpikeNear;
        private AudioClip _invulnEnd;
        private float _hitGapT;
        private float _armorT;
        private float _pickupT;
        private float _tellAnyT;
        private float _bruteTellT;
        private float _aimTellT;
        private float _radTellT;
        private bool _muted;
        private float _sfxVolume = MixCurve.DefaultSfxSlider;
        private float _musicVolume = MixCurve.DefaultMusicSlider;
        private AudioClip _currentMusic;
        private float _outScale = HangarMusicScale;
        private float _outPitch = HangarMusicPitch;
        private int _outHangar;
        private float _inScale = HangarMusicScale;
        private float _inPitch = HangarMusicPitch;
        private int _inHangar;
        private float _fadeStartAngle;
        private float _duckScale = 1f;
        private float _duckUntil;
        private float _duckSeconds = AbortDuckSeconds;
        private float _duckTarget = AbortDuckScale;
        private bool _creditsMusic;
        private float _lastSwarmPodSpawn;
        private bool _crossfading;
        private bool _armHangarReturn;
        private bool _hangarReturnClock;
        private float _fadeElapsed;
        private float _fadeSeconds;
        private AudioClip _incomingClip;
        private bool _musicHeld;
        private AudioClip _deathWhoosh;
        private AudioClip _waveWhoosh;
        private float _waveClearAt;
        private AudioClip _recordCue;
        public float StingerAt { get; private set; }
        private MusicLowPassState _lp = MusicLowPass.OpenState();
        private int _lpPhase = -1;
        private AudioLowPassFilter _musicLp;
        private AudioLowPassFilter _musicBLp;
        private AudioLowPassFilter _hangarLp;
        private readonly float[] _voiceStart = new float[VoiceRoster.SlotSpan];
        private readonly float[] _voiceUntil = new float[VoiceRoster.SlotSpan];
        private int _voiceCat = VoiceRoster.Uncapped;
        private bool _voiceSteal;

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
            _tell = CreateSource("TellSource", false);
            _tick = CreateSource("SpikeTickSource", false);
            if (_tell != null)
            {
                _tell.pitch = 1f;
            }
            _music = CreateSource("MusicSource", true);
            _musicB = CreateSource("MusicSourceB", true);
            _hangarLayer = CreateSource("HangarLayerSource", true);
            _railRise = CreateSource("RailRiseSource", false);
            _railHold = CreateSource("RailHoldSource", true);
            LoadClips();
            _muted = PlayerPrefs.GetInt(MuteKey, 0) == 1;
            LoadMixSliders();
            ApplyVolumes();
        }

        public void PlayShoot()
        {
            UseVoice(VoiceRoster.CatWeapon, false);
            PlayPooledPitched(_boltShots, 1f, _shoot, BoltPitchJitter);
        }

        public void PlayShootSpread()
        {
            UseVoice(VoiceRoster.CatWeapon, false);
            PlayPooled(_spreadShots, SpreadShotScale, _shootSpread != null ? _shootSpread : _shoot);
        }

        public void PlayShootPierce()
        {
            UseVoice(VoiceRoster.CatWeapon, false);
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
            _railRise.volume = SfxGain() * RailChargeScale;
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
            _railHold.volume = SfxGain() * RailHoldLoopScale;
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
            UseVoice(VoiceRoster.CatWeapon, true);
            PlayPitched(shot, RailShotScale, pitch);
            AudioClip thump = _railThump != null ? _railThump : _bruteDeathLayer;
            UseVoice(VoiceRoster.CatWeapon, true);
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
            UseVoice(VoiceRoster.CatWeapon, false);
            Play(_shootSeeker != null ? _shootSeeker : _shootPierce, SeekerShotScale);
        }

        public void PlayShootTwin()
        {
            UseVoice(VoiceRoster.CatWeapon, false);
            Play(_shootEnemy != null ? _shootEnemy : _shoot, 1f);
            UseVoice(VoiceRoster.CatWeapon, false);
            Play(_shootTwin != null ? _shootTwin : _shoot, TwinLayerScale);
        }

        public void PlayShootRicochet()
        {
            UseVoice(VoiceRoster.CatWeapon, false);
            PlayPitched(
                _shootRicochet != null ? _shootRicochet : _shootSpread,
                RicochetShotScale,
                1f + Random.Range(-RicochetPitchJitter, RicochetPitchJitter));
        }

        public void PlayEnemyShoot()
        {
            UseVoice(VoiceRoster.CatEnemyShot, false);
            Play(_shootEnemy != null ? _shootEnemy : _shoot, 0.7f);
        }

        public void PlayHit()
        {
            if (!ClaimHitGap())
            {
                return;
            }

            PlayHitNow();
        }

        private void PlayHitNow()
        {
            UseVoice(VoiceRoster.CatHit, false);
            PlayPooledPitched(_hits, 1f, _hit, HitPitchJitter);
            if (_hitPunch != null)
            {
                UseVoice(VoiceRoster.CatHit, false);
                Play(_hitPunch, HitPunchScale);
            }
        }

        private bool ClaimHitGap()
        {
            if (!CueVoiceBudget.GapReady(Time.unscaledTime, _hitGapT, CueVoiceBudget.SameClipGapSeconds))
            {
                return false;
            }

            _hitGapT = Time.unscaledTime;
            return true;
        }

        public void PlayHit(EnemyKind kind)
        {
            if (!ClaimHitGap())
            {
                return;
            }

            if (UsesLightThreatSfx(kind))
            {
                UseVoice(VoiceRoster.CatHit, false);
                Play(_hitLight != null ? _hitLight : _hit, 0.92f);
                return;
            }

            if (kind == EnemyKind.Brute)
            {
                UseVoice(VoiceRoster.CatHit, false);
                PlayPooled(_bruteHits, BruteHitScale, _bruteSpawn);
                return;
            }

            if (kind == EnemyKind.Swarm)
            {
                UseVoice(VoiceRoster.CatHit, false);
                PlayPooledPitched(_swarmHits, SwarmHitScale, _swarmSpawn, SwarmHitPitchJitter);
                return;
            }

            PlayHitNow();
        }

        public void PlayExplosion()
        {
            PlayAsteroidSplit();
        }

        public void PlayAsteroidSplit()
        {
            UseVoice(VoiceRoster.CatDeath, true);
            Play(_asteroidSplit);
        }

        public void PlayEnemyDeath()
        {
            UseVoice(VoiceRoster.CatDeath, true);
            Play(_enemyDeath);
            if (_enemyDeathPunch != null)
            {
                UseVoice(VoiceRoster.CatDeath, true);
                Play(_enemyDeathPunch, EnemyDeathPunchScale);
            }
        }

        public void PlayEnemyDeath(EnemyKind kind)
        {
            if (UsesLightThreatSfx(kind))
            {
                AudioClip light = _enemyDeathLight != null ? _enemyDeathLight : _enemyDeath;
                UseVoice(VoiceRoster.CatDeath, true);
                PlayPitched(light, LightKillScale, 1f + Random.Range(-LightKillPitchJitter, LightKillPitchJitter));
                return;
            }

            if (kind == EnemyKind.Brute)
            {
                UseVoice(VoiceRoster.CatDeath, true);
                Play(_bruteDeath != null ? _bruteDeath : _bruteSpawn, BruteDeathScale);
                if (_bruteDeathLayer != null)
                {
                    UseVoice(VoiceRoster.CatDeath, true);
                    Play(_bruteDeathLayer, BruteDeathLayerScale);
                }

                return;
            }

            if (kind == EnemyKind.Swarm)
            {
                UseVoice(VoiceRoster.CatDeath, true);
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
            UseVoice(VoiceRoster.CatHit, false);
            PlayPooled(_hazardHits, HazardHitScale, _shootSpread);
        }

        public void PlayPlayerDamage()
        {
            UseVoice(VoiceRoster.CatPlayer, false);
            Play(_playerDamage, 1.15f);
            UseVoice(VoiceRoster.CatPlayer, false);
            Play(_hit, 0.85f);
        }

        public void PlayShieldHit()
        {
            if (!CueVoiceBudget.GapReady(Time.unscaledTime, _hitGapT, CueVoiceBudget.SameClipGapSeconds))
            {
                return;
            }

            _hitGapT = Time.unscaledTime;
            UseVoice(VoiceRoster.CatPlayer, false);
            PlayPitched(_shieldHit, ShieldHitScale, 1f + Random.Range(-HitPitchJitter, HitPitchJitter));
            UseVoice(VoiceRoster.CatPlayer, false);
            Play(_hitLight, ShieldHitThudScale);
        }

        public void PlayShieldBreak()
        {
            UseVoice(VoiceRoster.CatPlayer, false);
            Play(_shieldBreak, ShieldBreakScale);
            UseVoice(VoiceRoster.CatPlayer, false);
            Play(_hazardActivate, ShieldBreakHumScale);
            DuckMusic(0.25f, 0.55f);
        }

        public void PlayArmorHit()
        {
            if (!CueVoiceBudget.GapReady(Time.unscaledTime, _armorT, 0.12f))
            {
                return;
            }

            _armorT = Time.unscaledTime;
            UseVoice(VoiceRoster.CatPlayer, false);
            Play(_armorHit, ArmorHitScale);
        }

        public void PlayPickup(Pickup.Kind kind)
        {
            if (kind == Pickup.Kind.ExtraLife)
            {
                PlayExtraLifePickup();
                return;
            }

            if (!CueVoiceBudget.GapReady(Time.unscaledTime, _pickupT, CueVoiceBudget.PickupGapSeconds))
            {
                return;
            }

            _pickupT = Time.unscaledTime;
            if (kind == Pickup.Kind.Shield)
            {
                UseVoice(VoiceRoster.CatPickup, false);
                Play(_pickupShield != null ? _pickupShield : _pickupMinor, PickupShieldScale);
                return;
            }

            if (kind == Pickup.Kind.Health)
            {
                UseVoice(VoiceRoster.CatPickup, false);
                Play(_pickupHealth != null ? _pickupHealth : _pickupMinor, PickupHealthScale);
                return;
            }

            if (kind == Pickup.Kind.RapidFire)
            {
                UseVoice(VoiceRoster.CatPickup, false);
                Play(_pickupRapid != null ? _pickupRapid : _pickupMinor, PickupRapidScale);
                return;
            }

            if (kind == Pickup.Kind.Score)
            {
                UseVoice(VoiceRoster.CatPickup, false);
                Play(_pickupScore != null ? _pickupScore : _pickupMinor, PickupScoreScale);
                return;
            }

            PlayPickupMinor();
        }

        public void PlayBruteTell()
        {
            if (!CueVoiceBudget.AllowTell(Time.unscaledTime, _tellAnyT, _bruteTellT, 1.2f))
            {
                return;
            }

            _bruteTellT = Time.unscaledTime;
            _tellAnyT = _bruteTellT;
            PlayTell(_tellBrute, BruteTellScale);
        }

        public void PlayNestTell()
        {
            if (!CueVoiceBudget.AllowTell(Time.unscaledTime, _tellAnyT, 0f, 0f))
            {
                return;
            }

            _tellAnyT = Time.unscaledTime;
            PlayTell(_tellNest, NestTellScale);
        }

        public void PlayBossAimedTell()
        {
            if (!CueVoiceBudget.AllowTell(Time.unscaledTime, _tellAnyT, _aimTellT, 1.7f))
            {
                return;
            }

            _aimTellT = Time.unscaledTime;
            _tellAnyT = _aimTellT;
            PlayTell(_tellBossAimed, BossAimedTellScale);
        }

        public void PlayGunnerTell()
        {
            if (!CueVoiceBudget.AllowTell(Time.unscaledTime, _tellAnyT, _aimTellT, 0.25f))
            {
                return;
            }

            _aimTellT = Time.unscaledTime;
            _tellAnyT = _aimTellT;
            PlayTell(_tellBossAimed, GunnerTellScale);
        }

        public void PlayBossRadialTell()
        {
            if (!CueVoiceBudget.AllowTell(Time.unscaledTime, _tellAnyT, _radTellT, 2.6f))
            {
                return;
            }

            _radTellT = Time.unscaledTime;
            _tellAnyT = _radTellT;
            PlayTell(_tellBossRadial, BossRadialTellScale);
        }

        public void PlaySpikeNearTick(float p01)
        {
            if (_tick == null || _tellSpikeNear == null || _muted)
            {
                return;
            }

            float near = p01;
            if (near < 0f)
            {
                near = 0f;
            }

            if (near > 1f)
            {
                near = 1f;
            }

            _tick.pitch = Mathf.Clamp(1f + 0.25f * near, 0.5f, 1.5f);
            UseVoice(VoiceRoster.CatTell, true);
            if (!TakeVoice(_tellSpikeNear, _tick.pitch))
            {
                return;
            }

            _tick.PlayOneShot(_tellSpikeNear, Mathf.Clamp(SpikeTickScale, 0f, 1.4f));
        }

        public void PlayInvulnEnd()
        {
            UseVoice(VoiceRoster.CatPlayer, false);
            Play(_invulnEnd, InvulnEndScale);
        }

        public void PlayBossFinale(Vector3 position)
        {
            StartCoroutine(BossFinaleRoutine(position));
        }

        private IEnumerator BossFinaleRoutine(Vector3 position)
        {
            CombatJuice.HeavyKill(position, CombatJuice.ExplosionShake);
            UseVoice(VoiceRoster.CatDeath, true);
            Play(_bruteDeath != null ? _bruteDeath : _enemyDeath, BruteDeathScale);
            if (_bruteDeathLayer != null)
            {
                UseVoice(VoiceRoster.CatDeath, true);
                Play(_bruteDeathLayer, BruteDeathLayerScale);
            }

            DuckMusic(0.9f, 0.25f);
            yield return new WaitForSeconds(0.18f);
            CombatJuice.HeavyKill(position, 0.36f);
            UseVoice(VoiceRoster.CatDeath, true);
            Play(_bruteDeath != null ? _bruteDeath : _enemyDeath, BruteDeathScale);
            yield return new WaitForSeconds(0.72f);
        }

        private void PlayTell(AudioClip clip, float scale)
        {
            if (_tell == null || clip == null || _muted)
            {
                ClearVoice();
                return;
            }

            UseVoice(VoiceRoster.CatTell, true);
            if (!TakeVoice(clip, 1f))
            {
                return;
            }

            _tell.pitch = 1f;
            _tell.PlayOneShot(clip, Mathf.Clamp(scale, 0f, 1.4f));
        }

        public void PlayHangarPurchase()
        {
            Play(_purchase);
        }

        public void PlayExtraLifePickup()
        {
            if (_extraLife != null)
            {
                UseVoice(VoiceRoster.CatPickup, false);
                Play(_extraLife, ExtraLifePickupScale);
            }
            else
            {
                UseVoice(VoiceRoster.CatPickup, false);
                Play(_extraLifeAlt != null ? _extraLifeAlt : _purchase, ExtraLifeAltScale);
            }

            DuckMusic(ExtraLifeDuckSeconds, ExtraLifeDuckScale);
        }

        public void PlayExtraLifeMiss()
        {
            UseVoice(VoiceRoster.CatPickup, false);
            Play(_extraLifeMiss != null ? _extraLifeMiss : _abort, ExtraLifeMissScale);
        }

        public void PlayPickupMinor()
        {
            UseVoice(VoiceRoster.CatPickup, false);
            Play(_pickupMinor != null ? _pickupMinor : _uiClick, PickupMinorScale);
        }

        public void PlayUiClick()
        {
            UseVoice(VoiceRoster.CatUi, false);
            Play(_uiClick != null ? _uiClick : _purchase, 0.88f);
        }

        public void PlayUiHover()
        {
            if (_muted)
            {
                return;
            }

            float now = Time.unscaledTime;
            bool priority = now < _uiPriorityUntil;
            if (!UiSfxGate.AllowHover(now, _uiHoverT, priority))
            {
                return;
            }

            _uiHoverT = now;
            UseVoice(VoiceRoster.CatUi, false);
            Play(_uiHover != null ? _uiHover : _uiClick, UiSfxGate.HoverScale);
        }

        public void PlayUiConfirm()
        {
            _uiPriorityUntil = Time.unscaledTime + 0.20f;
            UseVoice(VoiceRoster.CatUi, false);
            Play(_uiConfirm != null ? _uiConfirm : _uiClick, UiSfxGate.ConfirmScale);
        }

        public void PlayUiBack()
        {
            _uiPriorityUntil = Time.unscaledTime + 0.08f;
            UseVoice(VoiceRoster.CatUi, false);
            Play(_uiBack != null ? _uiBack : _uiClick, UiSfxGate.BackScale);
        }

        public void PlayUiOpen()
        {
            UseVoice(VoiceRoster.CatUi, false);
            Play(_uiOpen != null ? _uiOpen : _uiClick, UiSfxGate.OpenScale);
        }

        public void PlayUiClose()
        {
            UseVoice(VoiceRoster.CatUi, false);
            Play(_uiClose != null ? _uiClose : _uiClick, UiSfxGate.CloseScale);
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
            SetMusicLowPass(MusicLowPass.OpenHz, MusicLowPass.ReleaseSeconds);
        }

        public void PlayDeathWhoosh()
        {
            PlayPitched(_deathWhoosh, MusicLowPass.DeathWhooshScale, 1f);
            SetMusicLowPass(MusicLowPass.DeathHz, MusicLowPass.DeathSeconds);
        }

        public void PlayRetry()
        {
            Play(_retry != null ? _retry : _shootTwin, RetryScale);
        }

        public void PlayWaveClear()
        {
            PlayPitched(_waveWhoosh, MusicLowPass.WaveWhooshScale, 1f);
            float lead = MusicLowPass.WaveWhooshLeadSeconds;
            _waveClearAt = Time.unscaledTime + lead;
            if (_waveClearAt <= Time.unscaledTime)
            {
                _waveClearAt = 0f;
                StingerAt = Time.unscaledTime;
                Play(_waveClear, WaveClearScale);
            }
            else
            {
                StingerAt = _waveClearAt;
            }

            DuckMusic(MusicLowPass.WaveDuckSeconds, MusicLowPass.WaveDuckScale);
        }

        public void PlayFarDriftAward()
        {
            StingerAt = Time.unscaledTime;
            Play(_farDriftAward != null ? _farDriftAward : _waveClear, FarDriftAwardScale);
        }

        public void PlayRecord()
        {
            if (_recordCue == null)
            {
                _recordCue = Resources.Load<AudioClip>("Audio/Sfx/threeTone2");
            }

            AudioClip clip = _recordCue != null ? _recordCue : _doctrinePick;
            Play(clip, 0.55f);
            UseVoice(VoiceRoster.CatUi, false);
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
                PlayPitched(_waveWhoosh, MusicLowPass.WaveWhooshScale, 1f);
                return;
            }

            Play(_worldChange);
            PlayPitched(_waveWhoosh, MusicLowPass.WaveWhooshScale, 1f);
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

        public void ArmHangarReturn()
        {
            _armHangarReturn = true;
        }

        public void SyncMusicToPhase(GamePhase phase)
        {
            SyncMusicToPhase(phase, 1);
        }

        public void SyncMusicToPhase(GamePhase phase, int waveIndex)
        {
            ReleaseForPhase(phase);
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
            if (_sfx == null || clip == null || _muted)
            {
                ClearVoice();
                return;
            }

            if (!TakeVoice(clip, 1f))
            {
                return;
            }

            _sfx.PlayOneShot(clip, Mathf.Clamp(scale, 0f, 1.4f));
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
            if (_vary == null || clip == null || _muted)
            {
                ClearVoice();
                return;
            }

            float shaped = Mathf.Clamp(pitch, 0.5f, 1.5f);
            if (!TakeVoice(clip, shaped))
            {
                return;
            }

            _vary.pitch = shaped;
            _vary.PlayOneShot(clip, Mathf.Clamp(scale, 0f, 1.4f));
        }

        private void UseVoice(int category, bool stealOldest)
        {
            _voiceCat = category;
            _voiceSteal = stealOldest;
        }

        private void ClearVoice()
        {
            _voiceCat = VoiceRoster.Uncapped;
            _voiceSteal = false;
        }

        private bool TakeVoice(AudioClip clip, float pitch)
        {
            int category = _voiceCat;
            bool steal = _voiceSteal;
            ClearVoice();
            return VoiceRoster.Admit(category, Time.unscaledTime, CueSeconds(clip, pitch), steal, _voiceStart, _voiceUntil);
        }

        private static float CueSeconds(AudioClip clip, float pitch)
        {
            float length = 0.2f;
            if (clip != null && clip.length > 0.02f)
            {
                length = clip.length;
            }

            float rate = pitch;
            if (rate < 0.05f)
            {
                rate = 0.05f;
            }

            return length / rate;
        }

        public void SetMusicLowPass(float hz, float seconds)
        {
            _lp = MusicLowPass.Command(_lp, hz, seconds);
            if (seconds <= 0f)
            {
                ApplyMusicLowPass();
            }
        }

        /// <summary>
        /// Snap the music filter open. A second call while it is already open does nothing.
        /// </summary>
        public void ReleaseMusicLowPass()
        {
            MusicLowPassState next = MusicLowPass.ResetOpen(_lp);
            if (MusicLowPass.Same(_lp, next))
            {
                return;
            }

            _lp = next;
            ApplyMusicLowPass();
        }

        private void ReleaseForPhase(GamePhase phase)
        {
            bool hangarLike = phase == GamePhase.Hangar
                || phase == GamePhase.WaveClear
                || phase == GamePhase.CampaignClear;
            bool enteredPlay = phase == GamePhase.Playing && _lpPhase != (int)GamePhase.Playing;
            _lpPhase = (int)phase;
            if (hangarLike || enteredPlay)
            {
                ReleaseMusicLowPass();
            }
        }

        public float DominantMusicSeconds()
        {
            AudioSource lead = _music;
            if (_crossfading && _musicB != null)
            {
                float ramp = MusicPlan.CrossfadeRamp(_fadeElapsed, _fadeSeconds);
                if (ramp >= 0.5f)
                {
                    lead = _musicB;
                }
            }

            if (lead == null || lead.clip == null)
            {
                return 0f;
            }

            int rate = lead.clip.frequency;
            if (rate <= 0)
            {
                return 0f;
            }

            return lead.timeSamples / (float)rate;
        }

        private void TickMusicLowPass()
        {
            float before = _lp.Current;
            _lp = MusicLowPass.Advance(_lp, Time.unscaledDeltaTime);
            if (_lp.Current != before)
            {
                ApplyMusicLowPass();
            }
        }

        private void ApplyMusicLowPass()
        {
            ApplyOneLowPass(_musicLp);
            ApplyOneLowPass(_musicBLp);
            ApplyOneLowPass(_hangarLp);
        }

        private void ApplyOneLowPass(AudioLowPassFilter filter)
        {
            if (filter == null)
            {
                return;
            }

            filter.cutoffFrequency = _lp.Current;
            filter.lowpassResonanceQ = MusicLowPass.ResonanceQ;
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

        private void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                ReleaseMusicLowPass();
            }
        }

        private void OnApplicationQuit()
        {
            ReleaseMusicLowPass();
        }

        private void Update()
        {
            if (_waveClearAt > 0f && Time.unscaledTime >= _waveClearAt)
            {
                _waveClearAt = 0f;
                Play(_waveClear, WaveClearScale);
            }

            TickMusicLowPass();
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
            _fadeStartAngle = 0f;
            if (_musicB != null)
            {
                _musicB.Stop();
            }

            _outScale = scale;
            _outPitch = pitch;
            _outHangar = HangarFlag(clip);
            _inScale = scale;
            _inPitch = pitch;
            _inHangar = 0;
            _incomingClip = null;
            bool sameClip = _currentMusic == clip;
            bool pausedOrMuted = _muted || _musicHeld;
            if (MusicCrossfade.KeepSameClip(sameClip, _music.isPlaying, pausedOrMuted))
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

            bool settledSame = MusicCrossfade.SettledSame(
                !_crossfading,
                _currentMusic == clip,
                _music.isPlaying,
                _muted || _musicHeld);
            bool sameAsIncoming = _crossfading && _incomingClip == clip;
            if (!MusicPlan.ShouldStartCrossfade(settledSame, sameAsIncoming))
            {
                if (_crossfading)
                {
                    _inScale = scale;
                    _inPitch = pitch;
                    _inHangar = HangarFlag(clip);
                }
                else
                {
                    _outScale = scale;
                    _outPitch = pitch;
                    _outHangar = HangarFlag(clip);
                }

                ApplyVolumes();
                return;
            }

            if (_currentMusic == null || !_music.isPlaying || _musicB == null)
            {
                PlayLoop(clip, scale, pitch);
                return;
            }

            float outWeight;
            float inWeight;
            CurrentMusicWeights(out outWeight, out inWeight);
            int reverse = (_crossfading && _currentMusic == clip) ? 1 : 0;
            MusicRetarget plan = MusicCrossfade.Retarget(
                outWeight,
                inWeight,
                _outScale,
                _outPitch,
                _outHangar,
                _inScale,
                _inPitch,
                _inHangar,
                _crossfading ? 1 : 0,
                reverse,
                scale,
                pitch,
                HangarFlag(clip));
            if (plan.SwapSources != 0)
            {
                SwapMusicSources();
            }

            _outScale = plan.OutScale;
            _outPitch = plan.OutPitch;
            _outHangar = plan.OutHangar;
            _inScale = plan.InScale;
            _inPitch = plan.InPitch;
            _inHangar = plan.InHangar;
            _fadeStartAngle = plan.StartAngle;
            _fadeElapsed = 0f;
            if (_armHangarReturn)
            {
                _fadeSeconds = LookPolish.HangarMusicSeconds;
                _hangarReturnClock = true;
                _armHangarReturn = false;
            }
            else
            {
                _fadeSeconds = MusicPlan.MusicCrossfadeSeconds;
                _hangarReturnClock = false;
            }

            _crossfading = true;
            if (plan.ReplaceIncoming != 0)
            {
                _incomingClip = clip;
                _musicB.clip = clip;
                _musicB.loop = true;
                if (!_muted && !_musicHeld)
                {
                    _musicB.Play();
                }
            }

            ApplyVolumes();
        }

        private void FinishCrossfade()
        {
            _outScale = _inScale;
            _outPitch = _inPitch;
            _outHangar = _inHangar;
            SwapMusicSources();
            if (_musicB != null)
            {
                _musicB.Stop();
            }

            _incomingClip = null;
            _crossfading = false;
            _fadeElapsed = 0f;
            _fadeStartAngle = 0f;
        }

        private void SwapMusicSources()
        {
            AudioSource outgoing = _music;
            _music = _musicB;
            _musicB = outgoing;
            AudioClip settled = _currentMusic;
            _currentMusic = _incomingClip;
            _incomingClip = settled;
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
            float sfxGain = SfxGain();
            if (_sfx != null)
            {
                _sfx.volume = sfxGain;
            }

            if (_vary != null)
            {
                _vary.volume = sfxGain;
            }

            if (_tell != null)
            {
                _tell.volume = sfxGain;
                _tell.pitch = 1f;
            }

            if (_tick != null)
            {
                _tick.volume = sfxGain;
            }

            if (_railRise != null)
            {
                _railRise.volume = sfxGain * RailChargeScale;
            }

            ApplyRailHoldVolume();

            float outWeight;
            float inWeight;
            CurrentMusicWeights(out outWeight, out inWeight);
            float musicGain = MusicGain();
            if (_music != null)
            {
                _music.pitch = _outPitch;
                _music.volume = musicGain * _outScale * _duckScale * outWeight;
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
                _musicB.pitch = _inPitch;
                _musicB.volume = _crossfading ? musicGain * _inScale * _duckScale * inWeight : 0f;
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
                _railHold.volume = SfxGain() * RailHoldLoopScale;
            }
        }

        private void ApplyHangarLayer()
        {
            if (_hangarLayer == null || _hangarAmbience == null)
            {
                return;
            }

            float outWeight;
            float inWeight;
            CurrentMusicWeights(out outWeight, out inWeight);
            float hangarWeight = 0f;
            if (_outHangar != 0)
            {
                hangarWeight += outWeight;
            }

            if (_crossfading && _inHangar != 0)
            {
                hangarWeight += inWeight;
            }

            _hangarLayer.pitch = HangarLayerPitch;
            float level = MusicGain() * HangarLayerScale * _duckScale * hangarWeight;
            _hangarLayer.volume = level;
            if (level <= 0.0001f || _muted)
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

            if (!_hangarLayer.isPlaying && !_musicHeld)
            {
                _hangarLayer.Play();
            }
        }

        private float SfxGain()
        {
            if (_muted)
            {
                return 0f;
            }

            return MixCurve.Gain(_sfxVolume);
        }

        private float MusicGain()
        {
            if (_muted)
            {
                return 0f;
            }

            return MixCurve.Gain(_musicVolume);
        }

        private int HangarFlag(AudioClip clip)
        {
            if (clip != null && clip == _hangarAmbience)
            {
                return 1;
            }

            return 0;
        }

        private void CurrentMusicWeights(out float outWeight, out float inWeight)
        {
            if (!_crossfading)
            {
                outWeight = 1f;
                inWeight = 0f;
                return;
            }

            if (_hangarReturnClock)
            {
                LookPolish.HangarReturnWeights(_fadeElapsed, out outWeight, out inWeight);
                return;
            }

            float progress = MusicPlan.CrossfadeRamp(_fadeElapsed, _fadeSeconds);
            MusicCrossfade.Weights(_fadeStartAngle, progress, out outWeight, out inWeight);
        }

        private void LoadMixSliders()
        {
            int curve = PlayerPrefs.GetInt(MixCurve.CurveKey, 0);
            bool hasSfx = PlayerPrefs.HasKey(SfxKey);
            bool hasMusic = PlayerPrefs.HasKey(MusicKey);
            if (curve >= MixCurve.CurveVersion)
            {
                _sfxVolume = hasSfx ? PlayerPrefs.GetFloat(SfxKey, MixCurve.DefaultSfxSlider) : MixCurve.DefaultSfxSlider;
                _musicVolume = hasMusic ? PlayerPrefs.GetFloat(MusicKey, MixCurve.DefaultMusicSlider) : MixCurve.DefaultMusicSlider;
            }
            else if (hasSfx || hasMusic)
            {
                _sfxVolume = hasSfx
                    ? MixCurve.SliderFromLinear(PlayerPrefs.GetFloat(SfxKey, DefaultSfxVolume))
                    : MixCurve.DefaultSfxSlider;
                _musicVolume = hasMusic
                    ? MixCurve.SliderFromLinear(PlayerPrefs.GetFloat(MusicKey, DefaultMusicVolume))
                    : MixCurve.DefaultMusicSlider;
                PlayerPrefs.SetFloat(SfxKey, _sfxVolume);
                PlayerPrefs.SetFloat(MusicKey, _musicVolume);
                PlayerPrefs.SetInt(MixCurve.CurveKey, MixCurve.CurveVersion);
                PlayerPrefs.Save();
            }
            else
            {
                _sfxVolume = MixCurve.DefaultSfxSlider;
                _musicVolume = MixCurve.DefaultMusicSlider;
                PlayerPrefs.SetFloat(SfxKey, _sfxVolume);
                PlayerPrefs.SetFloat(MusicKey, _musicVolume);
                PlayerPrefs.SetInt(MixCurve.CurveKey, MixCurve.CurveVersion);
                PlayerPrefs.Save();
            }

            _sfxVolume = Mathf.Clamp01(_sfxVolume);
            _musicVolume = Mathf.Clamp01(_musicVolume);
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
            _shootRicochet = Resources.Load<AudioClip>("Audio/Sfx/agr_ricochet");
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
            _uiHover = Resources.Load<AudioClip>("Audio/Sfx/agr_ui_hover");
            _uiConfirm = Resources.Load<AudioClip>("Audio/Sfx/agr_ui_confirm");
            _uiBack = Resources.Load<AudioClip>("Audio/Sfx/agr_ui_back");
            _uiOpen = Resources.Load<AudioClip>("Audio/Sfx/agr_ui_open");
            _uiClose = Resources.Load<AudioClip>("Audio/Sfx/agr_ui_close");
            _deathWhoosh = Resources.Load<AudioClip>("Audio/Sfx/agr_death_whoosh");
            _waveWhoosh = Resources.Load<AudioClip>("Audio/Sfx/agr_wave_whoosh");
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
                "Audio/Sfx/agr_swarm_death_0",
                "Audio/Sfx/agr_swarm_death_1",
                "Audio/Sfx/agr_swarm_death_2");
            _shieldHit = Resources.Load<AudioClip>("Audio/Sfx/agr_shield_hit");
            _shieldBreak = Resources.Load<AudioClip>("Audio/Sfx/agr_shield_break");
            _armorHit = Resources.Load<AudioClip>("Audio/Sfx/agr_armor_hit");
            _pickupShield = Resources.Load<AudioClip>("Audio/Sfx/agr_pickup_shield");
            _pickupHealth = Resources.Load<AudioClip>("Audio/Sfx/agr_pickup_health");
            _pickupRapid = Resources.Load<AudioClip>("Audio/Sfx/agr_pickup_rapid");
            _pickupScore = Resources.Load<AudioClip>("Audio/Sfx/agr_pickup_score");
            _tellBrute = Resources.Load<AudioClip>("Audio/Sfx/agr_tell_brute");
            _tellNest = Resources.Load<AudioClip>("Audio/Sfx/agr_tell_nest");
            _tellBossAimed = Resources.Load<AudioClip>("Audio/Sfx/agr_tell_boss_aimed");
            _tellBossRadial = Resources.Load<AudioClip>("Audio/Sfx/agr_tell_boss_radial");
            _tellSpikeNear = Resources.Load<AudioClip>("Audio/Sfx/agr_tell_spike_near");
            _invulnEnd = Resources.Load<AudioClip>("Audio/Sfx/agr_invuln_end");
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
            if (sourceName == "MusicSource" || sourceName == "MusicSourceB" || sourceName == "HangarLayerSource")
            {
                AudioLowPassFilter filter = go.AddComponent<AudioLowPassFilter>();
                filter.cutoffFrequency = MusicLowPass.OpenHz;
                filter.lowpassResonanceQ = MusicLowPass.ResonanceQ;
                if (sourceName == "MusicSource")
                {
                    _musicLp = filter;
                }
                else if (sourceName == "MusicSourceB")
                {
                    _musicBLp = filter;
                }
                else
                {
                    _hangarLp = filter;
                }

                source.priority = VoiceRoster.Priority(VoiceRoster.CatMusic);
            }
            else if (sourceName == "TellSource" || sourceName == "SpikeTickSource")
            {
                source.priority = VoiceRoster.Priority(VoiceRoster.CatTell);
            }

            return source;
        }
    }
}
