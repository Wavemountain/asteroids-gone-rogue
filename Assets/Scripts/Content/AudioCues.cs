using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Week 1 SFX hooks. Assign clips later — no audio pack is required to play.
    /// </summary>
    public sealed class AudioCues : MonoBehaviour
    {
        public static AudioCues Instance { get; private set; }

        public AudioSource Source;
        public AudioClip Shoot;
        public AudioClip Hit;
        public AudioClip Explosion;

        private void Awake()
        {
            Instance = this;
            if (Source == null)
            {
                Source = gameObject.AddComponent<AudioSource>();
                Source.playOnAwake = false;
                Source.spatialBlend = 0f;
            }
        }

        public void PlayShoot()
        {
            Play(Shoot);
        }

        public void PlayHit()
        {
            Play(Hit);
        }

        public void PlayExplosion()
        {
            Play(Explosion);
        }

        private void Play(AudioClip clip)
        {
            if (Source != null && clip != null)
            {
                Source.PlayOneShot(clip);
            }
        }
    }
}
