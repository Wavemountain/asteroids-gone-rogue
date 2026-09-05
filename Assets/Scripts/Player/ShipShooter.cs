using UnityEngine;

namespace AsteroidsGoneRogue
{
    public sealed class ShipShooter : MonoBehaviour
    {
        public const int SpreadPelletCount = 3;
        public const float SpreadHalfAngleDegrees = 14f;

        private PlayerLoadout _loadout;
        private ContentFactory _factory;
        private float _nextFireTime;
        private float _boostUntil;
        private Transform _muzzle;
        private FireMode _mode = FireMode.Bolt;

        public FireMode Mode
        {
            get { return OwnedMode(_mode); }
        }

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

        public void CycleFireMode()
        {
            bool spread = Owns(UpgradeId.SpreadBolt);
            bool pierce = Owns(UpgradeId.Pierce);
            if (!spread && !pierce)
            {
                _mode = FireMode.Bolt;
                return;
            }

            FireMode current = OwnedMode(_mode);
            if (current == FireMode.Bolt)
            {
                _mode = spread ? FireMode.Spread : FireMode.Pierce;
            }
            else if (current == FireMode.Spread)
            {
                _mode = pierce ? FireMode.Pierce : FireMode.Bolt;
            }
            else
            {
                _mode = FireMode.Bolt;
            }
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
            FireMode mode = OwnedMode(_mode);
            if (mode == FireMode.Spread)
            {
                FireSpread(origin, loadout);
            }
            else
            {
                bool pierce = mode == FireMode.Pierce;
                _factory.SpawnProjectile(
                    origin,
                    transform.forward,
                    loadout.ProjectileSpeed,
                    loadout.ProjectileDamage,
                    pierce);
            }

            _factory.SpawnVfx("Vfx_MuzzleFlash", origin, 0.12f);
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayShoot();
            }
        }

        private void FireSpread(Vector3 origin, LoadoutState loadout)
        {
            int damage = loadout.SpreadPelletDamage;
            float speed = loadout.ProjectileSpeed;
            Vector3 forward = transform.forward;
            float[] yaw = { -SpreadHalfAngleDegrees, 0f, SpreadHalfAngleDegrees };
            for (int i = 0; i < SpreadPelletCount; i++)
            {
                Vector3 dir = Quaternion.AngleAxis(yaw[i], Vector3.up) * forward;
                _factory.SpawnProjectile(origin, dir, speed, damage, false);
            }
        }

        private bool Owns(UpgradeId id)
        {
            return _loadout != null && _loadout.State != null && _loadout.State.Owns(id);
        }

        private FireMode OwnedMode(FireMode requested)
        {
            if (requested == FireMode.Spread && Owns(UpgradeId.SpreadBolt))
            {
                return FireMode.Spread;
            }

            if (requested == FireMode.Pierce && Owns(UpgradeId.Pierce))
            {
                return FireMode.Pierce;
            }

            return FireMode.Bolt;
        }
    }
}
