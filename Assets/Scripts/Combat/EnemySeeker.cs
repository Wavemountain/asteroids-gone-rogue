using System.Collections.Generic;
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
        private ContentFactory _factory;
        private float _nextShot;
        private float _nextSpawn;
        private float _chargeUntil;
        private float _restUntil;
        private readonly List<EnemySeeker> _minions = new List<EnemySeeker>();

        public EnemyKind Kind
        {
            get { return _kind; }
        }

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
            _factory = Object.FindAnyObjectByType<ContentFactory>();
            _nextShot = Time.time + 0.85f;
            _nextSpawn = Time.time + 1.6f;
            _chargeUntil = 0f;
            _restUntil = 0f;
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
                CombatJuice.ThreatDamaged(transform, false);
                if (AudioCues.Instance != null)
                {
                    AudioCues.Instance.PlayHit(_kind);
                }

                return;
            }

            _dead = true;
            CombatJuice.ThreatDamaged(transform, true);
            if (_factory != null)
            {
                _factory.SpawnVfx("Vfx_Explosion_Lowpoly", transform.position, 0.45f);
                _factory.MaybeDropPickup(transform.position);
            }

            if (_waves != null)
            {
                _waves.NotifyDestroyed(this, EnemyCatalog.Score(_kind));
            }

            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayEnemyDeath(_kind);
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
                if (_kind == EnemyKind.Swarm)
                {
                    TrySpawnMinion();
                }

                return;
            }

            Vector3 dir = toPlayer.normalized;
            float speed = _speed;
            float turn = _turn;
            if (_kind == EnemyKind.Brute)
            {
                TuneBrute(toPlayer.magnitude, ref speed, ref turn);
            }

            Quaternion look = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                look,
                turn * Time.fixedDeltaTime);
            _body.linearVelocity = dir * speed;
            if (_kind == EnemyKind.Swarm)
            {
                TrySpawnMinion();
            }
            else
            {
                TryFireBolt(dir);
            }

            Vector3 pos = transform.position;
            pos.y = 0f;
            float limit = WaveManager.ArenaRadius - 1.2f;
            if (pos.sqrMagnitude > limit * limit)
            {
                pos = pos.normalized * limit;
                transform.position = pos;
            }
        }

        private void TuneBrute(float distance, ref float speed, ref float turn)
        {
            float now = Time.time;
            if (now < _chargeUntil)
            {
                speed = EnemyCatalog.BruteChargeSpeed;
                turn = EnemyCatalog.BruteChargeTurn;
                return;
            }

            if (now < _restUntil)
            {
                speed = _speed * 0.45f;
                return;
            }

            if (distance < EnemyCatalog.BruteChargeRange && distance > 2.2f)
            {
                _chargeUntil = now + EnemyCatalog.BruteChargeSeconds;
                _restUntil = _chargeUntil + EnemyCatalog.BruteRestSeconds;
                speed = EnemyCatalog.BruteChargeSpeed;
                turn = EnemyCatalog.BruteChargeTurn;
            }
        }

        private void TrySpawnMinion()
        {
            if (_kind != EnemyKind.Swarm || _factory == null || _waves == null)
            {
                return;
            }

            PruneMinions();
            if (_minions.Count >= EnemyCatalog.NestMaxMinions || Time.time < _nextSpawn)
            {
                return;
            }

            _nextSpawn = Time.time + EnemyCatalog.NestSpawnSeconds;
            float angle = Random.Range(0f, Mathf.PI * 2f);
            Vector3 pos = transform.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 1.6f;
            pos.y = 0f;
            EnemySeeker minion = _factory.CreateEnemy(pos, _target, _waves, EnemyCatalog.VisualName(EnemyKind.Swarmling));
            if (minion != null)
            {
                _minions.Add(minion);
                _waves.Register(minion);
            }
        }

        private void PruneMinions()
        {
            for (int i = _minions.Count - 1; i >= 0; i--)
            {
                if (_minions[i] == null)
                {
                    _minions.RemoveAt(i);
                }
            }
        }

        private void TryFireBolt(Vector3 toPlayerDir)
        {
            if (!EnemyCatalog.FiresBolts(_kind) || _factory == null || Time.time < _nextShot)
            {
                return;
            }

            if (Vector3.Dot(transform.forward, toPlayerDir) < 0.62f)
            {
                return;
            }

            _nextShot = Time.time + EnemyCatalog.FireCooldown(_kind);
            Vector3 origin = transform.position + transform.forward * 1.15f;
            _factory.SpawnEnemyProjectile(origin, transform.forward, EnemyCatalog.BoltSpeed(_kind), 1, _kind);
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayEnemyShoot();
            }
        }
    }
}
