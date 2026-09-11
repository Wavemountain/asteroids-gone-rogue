using System.Collections.Generic;
using UnityEngine;

namespace AsteroidsGoneRogue
{
    public sealed class Projectile : MonoBehaviour
    {
        public const float Lifetime = 2.4f;
        public const float SeekerLifetime = 3.1f;
        public const float RicochetLifetime = 3.4f;
        public const float SeekerTurnDegrees = 240f;

        private Vector3 _velocity;
        private int _damage = 1;
        private float _dieAt;
        private bool _pierce;
        private bool _hostile;
        private bool _seeker;
        private int _bounces;
        private EnemyKind _enemyKind = EnemyKind.Mid01;
        private readonly HashSet<EntityId> _hitIds = new HashSet<EntityId>();

        public void Launch(Vector3 direction, float speed, int damage)
        {
            Launch(direction, speed, damage, false);
        }

        public void Launch(Vector3 direction, float speed, int damage, bool pierce)
        {
            Launch(direction, speed, damage, pierce, false, EnemyKind.Mid01);
        }

        public void Launch(
            Vector3 direction,
            float speed,
            int damage,
            bool pierce,
            bool hostile,
            EnemyKind enemyKind)
        {
            Launch(direction, speed, damage, pierce, hostile, enemyKind, false, 0);
        }

        public void Launch(
            Vector3 direction,
            float speed,
            int damage,
            bool pierce,
            bool hostile,
            EnemyKind enemyKind,
            bool seeker,
            int ricochetBounces)
        {
            _velocity = direction.normalized * speed;
            _damage = damage;
            _pierce = pierce;
            _hostile = hostile;
            _seeker = seeker;
            _bounces = ricochetBounces;
            _enemyKind = enemyKind;
            _hitIds.Clear();
            float life = Lifetime;
            if (seeker)
            {
                life = SeekerLifetime;
            }
            else if (ricochetBounces > 0)
            {
                life = RicochetLifetime;
            }

            _dieAt = Time.time + life;
            transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        private void Update()
        {
            if (_seeker)
            {
                SteerTowardThreat();
            }

            transform.position += _velocity * Time.deltaTime;
            if (_bounces > 0)
            {
                BounceOnRim();
            }

            if (Time.time >= _dieAt)
            {
                Destroy(gameObject);
            }
        }

        private void SteerTowardThreat()
        {
            Transform target = NearestThreat();
            if (target == null)
            {
                return;
            }

            Vector3 to = target.position - transform.position;
            to.y = 0f;
            if (to.sqrMagnitude < 0.04f)
            {
                return;
            }

            Vector3 desired = to.normalized * _velocity.magnitude;
            Quaternion look = Quaternion.LookRotation(desired, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                look,
                SeekerTurnDegrees * Time.deltaTime);
            _velocity = transform.forward * _velocity.magnitude;
        }

        private Transform NearestThreat()
        {
            Transform best = null;
            float bestDist = float.MaxValue;
            ConsiderTag(GameTags.Enemy, ref best, ref bestDist);
            ConsiderTag(GameTags.Asteroid, ref best, ref bestDist);
            return best;
        }

        private void ConsiderTag(string tag, ref Transform best, ref float bestDist)
        {
            GameObject[] tagged = GameObject.FindGameObjectsWithTag(tag);
            Vector3 here = transform.position;
            for (int i = 0; i < tagged.Length; i++)
            {
                GameObject go = tagged[i];
                if (go == null)
                {
                    continue;
                }

                Vector3 delta = go.transform.position - here;
                delta.y = 0f;
                float sqr = delta.sqrMagnitude;
                if (sqr < 0.16f || sqr >= bestDist)
                {
                    continue;
                }

                bestDist = sqr;
                best = go.transform;
            }
        }

        private void BounceOnRim()
        {
            Vector3 pos = transform.position;
            pos.y = 0f;
            float limit = WaveManager.ArenaRadius - 0.65f;
            if (pos.sqrMagnitude <= limit * limit)
            {
                return;
            }

            Vector3 normal = pos.normalized;
            _velocity = Vector3.Reflect(_velocity, normal);
            transform.position = normal * (limit - 0.15f);
            transform.rotation = Quaternion.LookRotation(_velocity.normalized, Vector3.up);
            _bounces--;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(GameTags.Projectile))
            {
                return;
            }

            if (_hostile)
            {
                if (!other.CompareTag(GameTags.Player))
                {
                    return;
                }
            }
            else if (other.CompareTag(GameTags.Player))
            {
                return;
            }

            IDamageable damageable = other.GetComponentInParent<IDamageable>();
            if (damageable == null)
            {
                return;
            }

            MonoBehaviour target = damageable as MonoBehaviour;
            EntityId id = target != null ? target.GetEntityId() : other.GetEntityId();
            if (!_hitIds.Add(id))
            {
                return;
            }

            ShipHealth health = damageable as ShipHealth;
            if (health != null)
            {
                health.ApplyDamage(_damage, DamageCause.EnemyContact, _enemyKind);
            }
            else
            {
                damageable.ApplyDamage(_damage);
            }

            if (!_pierce)
            {
                Destroy(gameObject);
            }
        }
    }
}
