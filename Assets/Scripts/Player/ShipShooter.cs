using UnityEngine;

namespace AsteroidsGoneRogue
{
    public sealed class ShipShooter : MonoBehaviour
    {
        private PlayerLoadout _loadout;
        private ContentFactory _factory;
        private float _nextFireTime;
        private float _boostUntil;
        private Transform _muzzle;

        public void Bind(PlayerLoadout loadout, ContentFactory factory, Transform muzzle)
        {
            _loadout = loadout;
            _factory = factory;
            _muzzle = muzzle;
        }

        public void GrantRapidBoost(float seconds)
        {
            _boostUntil = Time.time + Mathf.Max(0.1f, seconds);
        }

        public void TryFire()
        {
            if (Time.time < _nextFireTime || _loadout == null || _factory == null)
            {
                return;
            }

            LoadoutState loadout = _loadout.State;
            float cooldown = loadout.FireCooldown;
            if (Time.time < _boostUntil)
            {
                cooldown = Mathf.Min(cooldown, LoadoutState.RapidFireCooldown);
            }

            _nextFireTime = Time.time + cooldown;
            Vector3 origin = _muzzle != null ? _muzzle.position : transform.position + transform.forward * 1.6f;
            _factory.SpawnProjectile(origin, transform.forward, loadout.ProjectileSpeed, loadout.ProjectileDamage);
            _factory.SpawnVfx("Vfx_MuzzleFlash", origin, 0.12f);
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayShoot();
            }
        }
    }
}
