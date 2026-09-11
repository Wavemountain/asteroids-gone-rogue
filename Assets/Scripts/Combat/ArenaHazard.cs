using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Static arena prop. Damaging spikes hit the ship; blockers only reshape the floor.
    /// </summary>
    public sealed class ArenaHazard : MonoBehaviour
    {
        public int Damage = 1;
        public bool Damaging;

        private void OnCollisionEnter(Collision collision)
        {
            TryHit(collision != null ? collision.collider : null);
        }

        private void OnTriggerEnter(Collider other)
        {
            TryHit(other);
        }

        private void TryHit(Collider other)
        {
            if (!Damaging || other == null)
            {
                return;
            }

            ShipHealth health = other.GetComponentInParent<ShipHealth>();
            if (health != null)
            {
                health.ApplyDamage(Damage, DamageCause.HazardContact);
            }
        }
    }
}
