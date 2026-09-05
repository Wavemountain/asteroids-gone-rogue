using UnityEngine;

namespace AsteroidsGoneRogue
{
    public enum AsteroidSize
    {
        Large,
        Small
    }

    public sealed class Asteroid : MonoBehaviour, IDamageable, IThreat
    {
        public const int LargeHits = 2;
        public const int SmallHits = 1;
        public const int ShardsOnSplit = 3;

        private AsteroidSize _size;
        private int _hits;
        private WaveManager _waves;
        private ContentFactory _factory;
        private bool _dead;
        private Rigidbody _body;

        public AsteroidSize Size
        {
            get { return _size; }
        }

        public void Initialize(AsteroidSize size, WaveManager waves, ContentFactory factory, Vector3 drift)
        {
            _size = size;
            _waves = waves;
            _factory = factory;
            _hits = size == AsteroidSize.Large ? LargeHits : SmallHits;
            _dead = false;

            _body = GetComponent<Rigidbody>();
            if (_body != null)
            {
                _body.linearVelocity = drift;
                _body.angularVelocity = Random.insideUnitSphere * 1.6f;
            }
        }

        private void FixedUpdate()
        {
            if (_dead)
            {
                return;
            }

            WrapIfOutsideArena();
        }

        public void WrapIfOutsideArena()
        {
            Vector3 pos = _body != null ? _body.position : transform.position;
            if (!ArenaWrap.ShouldWrap(pos.x, pos.z, WaveManager.ArenaRadius)
                && !ArenaWrap.IsInvalidXz(pos.x, pos.z))
            {
                return;
            }

            float ox;
            float oz;
            ArenaWrap.WrapXz(pos.x, pos.z, WaveManager.ArenaRadius, out ox, out oz);
            Vector3 wrapped = new Vector3(ox, 0f, oz);
            if (_body != null)
            {
                _body.position = wrapped;
            }
            else
            {
                transform.position = wrapped;
            }
        }

        public void ApplyDamage(int amount)
        {
            if (_dead || amount <= 0)
            {
                return;
            }

            _hits -= amount;
            if (_hits > 0)
            {
                if (AudioCues.Instance != null)
                {
                    AudioCues.Instance.PlayHit();
                }

                return;
            }

            Die();
        }

        public void Despawn()
        {
            if (this != null)
            {
                Destroy(gameObject);
            }
        }

        private void Die()
        {
            _dead = true;
            int score = _size == AsteroidSize.Large ? ScoreValues.LargeAsteroid : ScoreValues.SmallAsteroid;

            if (_size == AsteroidSize.Large && _factory != null && _waves != null)
            {
                for (int i = 0; i < ShardsOnSplit; i++)
                {
                    float angle = (Mathf.PI * 2f * i) / ShardsOnSplit + Random.Range(-0.2f, 0.2f);
                    Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 1.1f;
                    Vector3 drift = offset.normalized * Random.Range(3.5f, 6.5f);
                    Asteroid shard = _factory.CreateSmallAsteroid(transform.position + offset, drift, _waves);
                    _waves.Register(shard);
                }
            }

            if (_factory != null)
            {
                _factory.SpawnVfx("Vfx_Explosion_Lowpoly", transform.position, 0.45f);
            }

            if (_waves != null)
            {
                _waves.NotifyDestroyed(this, score);
            }

            if (AudioCues.Instance != null)
            {
                if (_size == AsteroidSize.Large)
                {
                    AudioCues.Instance.PlayAsteroidSplit();
                }
                else
                {
                    AudioCues.Instance.PlayHit();
                }
            }

            Destroy(gameObject);
        }
    }
}
