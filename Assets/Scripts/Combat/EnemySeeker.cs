using UnityEngine;

namespace AsteroidsGoneRogue
{
    public sealed class EnemySeeker : MonoBehaviour, IDamageable, IThreat
    {
        public const int HitPoints = 3;
        public const float Speed = 6.5f;
        public const float TurnDegreesPerSecond = 220f;

        private Transform _target;
        private WaveManager _waves;
        private EnemyKind _kind;
        private int _hp;
        private float _speed;
        private float _turn;
        private bool _dead;
        private Rigidbody _body;

        public void Initialize(Transform target, WaveManager waves)
        {
            Initialize(target, waves, EnemyKind.Mid01);
        }

        public void Initialize(Transform target, WaveManager waves, EnemyKind kind)
        {
            _target = target;
            _waves = waves;
            _kind = kind;
            _hp = EnemyCatalog.HitPoints(kind);
            _speed = EnemyCatalog.Speed(kind);
            _turn = EnemyCatalog.TurnDegreesPerSecond(kind);
            _dead = false;
            _body = GetComponent<Rigidbody>();
        }

        public void ApplyDamage(int amount)
        {
            if (_dead || amount <= 0)
            {
                return;
            }

            _hp -= amount;
            if (_hp > 0)
            {
                if (AudioCues.Instance != null)
                {
                    AudioCues.Instance.PlayHit();
                }

                return;
            }

            _dead = true;
            if (_waves != null)
            {
                _waves.NotifyDestroyed(this, EnemyCatalog.Score(_kind));
            }

            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayEnemyDeath();
            }

            Destroy(gameObject);
        }

        public void Despawn()
        {
            if (this != null)
            {
                Destroy(gameObject);
            }
        }

        private void FixedUpdate()
        {
            if (_dead || _target == null || _body == null)
            {
                return;
            }

            Vector3 toPlayer = _target.position - transform.position;
            toPlayer.y = 0f;
            if (toPlayer.sqrMagnitude < 0.01f)
            {
                return;
            }

            Vector3 dir = toPlayer.normalized;
            Quaternion look = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                look,
                _turn * Time.fixedDeltaTime);
            _body.velocity = dir * _speed;

            Vector3 pos = transform.position;
            pos.y = 0f;
            float limit = WaveManager.ArenaRadius - 1.2f;
            if (pos.sqrMagnitude > limit * limit)
            {
                pos = pos.normalized * limit;
                transform.position = pos;
            }
        }
    }
}
