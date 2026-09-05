using UnityEngine;

namespace AsteroidsGoneRogue
{
    public sealed class ShipShooter : MonoBehaviour
    {
        private PlayerLoadout _loadout;
        private ContentFactory _factory;
        private float _nextFireTime;
        private Transform _muzzle;

        public void Bind(PlayerLoadout loadout, ContentFactory factory, Transform muzzle)
        {
            _loadout = loadout;
            _factory = factory;
            _muzzle = muzzle;
        }

        public void TryFire()
        {
            if (Time.time < _nextFireTime || _loadout == null)
            {
                return;
            }

            LoadoutState loadout = _loadout.State;
            _nextFireTime = Time.time + loadout.FireCooldown;
            Vector3 origin = _muzzle != null ? _muzzle.position : transform.position + transform.forward * 1.6f;
            _factory.SpawnProjectile(origin, transform.forward, loadout.ProjectileSpeed, loadout.ProjectileDamage);
        }
    }
}
