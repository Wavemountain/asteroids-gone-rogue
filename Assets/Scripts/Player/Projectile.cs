using System.Collections.Generic;
using UnityEngine;

namespace AsteroidsGoneRogue
{
    public sealed class Projectile : MonoBehaviour
    {
        public const float Lifetime = 2.4f;
        public const float SeekerLifetime = 3.1f;
        public const float RicochetLifetime = 3.4f;
        public const float SeekerTurnDegrees = 165f;

        private Vector3 _velocity;
        private int _damage = 1;
        private float _dieAt;
        private bool _pierce;
        private bool _hostile;
        private bool _seeker;
        private int _bounces;
        private EnemyKind _enemyKind = EnemyKind.Mid01;
        private float _seekerTurn = SeekerTurnDegrees;
        private int _pierceBonus;
        private bool _struck;
        private ShipShooter _railShooter;
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

            _seekerTurn = SeekerTurnDegrees;
            _pierceBonus = 0;
            _struck = false;
            _railShooter = null;
            _dieAt = Time.time + life;
            transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        public void SetSeekerTurn(float degrees)
        {
            if (degrees > 1f)
            {
                _seekerTurn = degrees;
            }
        }

        public void SetPierceBonusTargets(int count)
        {
            _pierceBonus = count < 0 ? 0 : count;
        }

        public void ArmRailMiss(ShipShooter shooter)
        {
            _railShooter = shooter;
        }

        private void OnDestroy()
        {
            if (_railShooter == null || _struck)
            {
                return;
            }

            ShipShooter shooter = _railShooter;
            _railShooter = null;
            if (shooter != null)
            {
                shooter.NotifyRailMiss();
            }
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
                _seekerTurn * Time.deltaTime);
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

            _struck = true;
            ShipHealth health = damageable as ShipHealth;
            if (health != null)
            {
                health.ApplyDamage(_damage, DamageCause.EnemyContact, _enemyKind);
            }
            else
            {
                damageable.ApplyDamage(_damage);
            }

            if (_pierce && _pierceBonus > 0)
            {
                _pierceBonus--;
                DamageExtraTarget(other.transform.position);
            }

            if (!_pierce)
            {
                Destroy(gameObject);
            }
        }

        private void DamageExtraTarget(Vector3 from)
        {
            Collider[] hits = Physics.OverlapSphere(from, 4.5f);
            float best = float.MaxValue;
            IDamageable chosen = null;
            for (int i = 0; i < hits.Length; i++)
            {
                Collider hit = hits[i];
                if (hit == null)
                {
                    continue;
                }

                IDamageable damageable = hit.GetComponentInParent<IDamageable>();
                if (damageable == null)
                {
                    continue;
                }

                if (_hostile)
                {
                    if (!hit.CompareTag(GameTags.Player) && damageable as ShipHealth == null)
                    {
                        continue;
                    }
                }
                else if (hit.CompareTag(GameTags.Player) || damageable as ShipHealth != null)
                {
                    continue;
                }

                MonoBehaviour target = damageable as MonoBehaviour;
                EntityId id = target != null ? target.GetEntityId() : hit.GetEntityId();
                if (_hitIds.Contains(id))
                {
                    continue;
                }

                Vector3 delta = hit.transform.position - from;
                delta.y = 0f;
                if (delta.sqrMagnitude >= best)
                {
                    continue;
                }

                best = delta.sqrMagnitude;
                chosen = damageable;
            }

            if (chosen == null)
            {
                return;
            }

            MonoBehaviour chosenBehaviour = chosen as MonoBehaviour;
            if (chosenBehaviour != null)
            {
                _hitIds.Add(chosenBehaviour.GetEntityId());
            }

            ShipHealth health = chosen as ShipHealth;
            if (health != null)
            {
                health.ApplyDamage(_damage, DamageCause.EnemyContact, _enemyKind);
            }
            else
            {
                chosen.ApplyDamage(_damage);
            }
        }
    }
}
