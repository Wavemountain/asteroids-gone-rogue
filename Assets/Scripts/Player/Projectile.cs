using System.Collections.Generic;
using UnityEngine;

namespace AsteroidsGoneRogue
{
    public sealed class Projectile : MonoBehaviour
    {
        public const float Lifetime = 2.4f;

        private Vector3 _velocity;
        private int _damage = 1;
        private float _dieAt;
        private bool _pierce;
        private bool _hostile;
        private EnemyKind _enemyKind = EnemyKind.Mid01;
        private readonly HashSet<int> _hitIds = new HashSet<int>();

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
            _velocity = direction.normalized * speed;
            _damage = damage;
            _pierce = pierce;
            _hostile = hostile;
            _enemyKind = enemyKind;
            _hitIds.Clear();
            _dieAt = Time.time + Lifetime;
            transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        private void Update()
        {
            transform.position += _velocity * Time.deltaTime;
            if (Time.time >= _dieAt)
            {
                Destroy(gameObject);
            }
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
            int id = target != null ? target.GetInstanceID() : other.GetInstanceID();
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
