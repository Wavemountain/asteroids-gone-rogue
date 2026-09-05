using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// CC0 clips loaded from Resources/Audio. Mute and volumes persist in PlayerPrefs.
    /// </summary>
    public sealed class AudioCues : MonoBehaviour
    {
        public const string MuteKey = "agr.audio.mute";
        public const string SfxKey = "agr.audio.sfx";
        public const string MusicKey = "agr.audio.music";
        public const float DefaultSfxVolume = 0.8f;
        public const float DefaultMusicVolume = 0.28f;
        private const float MusicOutputScale = 0.55f;

        public static AudioCues Instance { get; private set; }

        private AudioSource _sfx;
        private AudioSource _music;
        private AudioClip _shoot;
        private AudioClip _hit;
        private AudioClip _asteroidSplit;
        private AudioClip _enemyDeath;
        private AudioClip _playerDamage;
        private AudioClip _purchase;
        private AudioClip _worldChange;
        private AudioClip _waveClear;
        private AudioClip _arenaLoop;
        private AudioClip _hangarAmbience;
        private bool _muted;
        private float _sfxVolume = DefaultSfxVolume;
        private float _musicVolume = DefaultMusicVolume;
        private AudioClip _currentMusic;

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
            _music = CreateSource("MusicSource", true);
            LoadClips();
            _muted = PlayerPrefs.GetInt(MuteKey, 0) == 1;
            _sfxVolume = PlayerPrefs.GetFloat(SfxKey, DefaultSfxVolume);
            _musicVolume = PlayerPrefs.GetFloat(MusicKey, DefaultMusicVolume);
            ApplyVolumes();
        }

        public void PlayShoot()
        {
            Play(_shoot);
        }

        public void PlayHit()
        {
            Play(_hit);
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
        }

        public void PlayPlayerDamage()
        {
            Play(_playerDamage);
        }

        public void PlayHangarPurchase()
        {
            Play(_purchase);
        }

        public void PlayUiClick()
        {
            if (_sfx != null && _purchase != null && !_muted)
            {
                _sfx.PlayOneShot(_purchase, 0.42f);
            }
        }

        public void PlayWaveClear()
        {
            Play(_waveClear);
        }

        public void PlayWorldChange()
        {
            Play(_worldChange);
        }

        public void SyncMusicToPhase(GamePhase phase)
        {
            if (phase == GamePhase.Playing)
            {
                PlayLoop(_arenaLoop);
            }
            else
            {
                PlayLoop(_hangarAmbience);
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
            if (_sfx != null && clip != null && !_muted)
            {
                _sfx.PlayOneShot(clip);
            }
        }

        private void PlayLoop(AudioClip clip)
        {
            if (_music == null || clip == null)
            {
                return;
            }

            if (_currentMusic == clip && _music.isPlaying)
            {
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

            if (_music != null)
            {
                _music.volume = _muted ? 0f : _musicVolume * MusicOutputScale;
                if (_muted)
                {
                    _music.Pause();
                }
                else if (_currentMusic != null && !_music.isPlaying)
                {
                    _music.Play();
                }
            }
        }

        private void LoadClips()
        {
            _shoot = Resources.Load<AudioClip>("Audio/Sfx/laserSmall_000");
            _hit = Resources.Load<AudioClip>("Audio/Sfx/impactMetal_000");
            _asteroidSplit = Resources.Load<AudioClip>("Audio/Sfx/explosionCrunch_000");
            _enemyDeath = Resources.Load<AudioClip>("Audio/Sfx/explosionCrunch_003");
            _playerDamage = Resources.Load<AudioClip>("Audio/Sfx/forceField_000");
            _purchase = Resources.Load<AudioClip>("Audio/Sfx/confirmation_002");
            _worldChange = Resources.Load<AudioClip>("Audio/Sfx/maximize_008");
            _waveClear = Resources.Load<AudioClip>("Audio/Sfx/jingles_PIZZA07");
            _arenaLoop = Resources.Load<AudioClip>("Audio/Music/OutThere");
            _hangarAmbience = Resources.Load<AudioClip>("Audio/Music/spacelifeNo14");
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
