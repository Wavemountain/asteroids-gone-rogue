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
        private readonly HashSet<int> _hitIds = new HashSet<int>();

        public void Launch(Vector3 direction, float speed, int damage)
        {
            Launch(direction, speed, damage, false);
        }

        public void Launch(Vector3 direction, float speed, int damage, bool pierce)
        {
            _velocity = direction.normalized * speed;
            _damage = damage;
            _pierce = pierce;
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
            if (other.CompareTag(GameTags.Player) || other.CompareTag(GameTags.Projectile))
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

            damageable.ApplyDamage(_damage);
            if (!_pierce)
            {
                Destroy(gameObject);
            }
        }
    }
}
