using UnityEngine;

namespace AsteroidsGoneRogue
{
    public sealed class Projectile : MonoBehaviour
    {
        public const float Lifetime = 2.4f;

        private Vector3 _velocity;
        private int _damage = 1;
        private float _dieAt;

        public void Launch(Vector3 direction, float speed, int damage)
        {
            _velocity = direction.normalized * speed;
            _damage = damage;
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

            damageable.ApplyDamage(_damage);
            Destroy(gameObject);
        }
    }
}
