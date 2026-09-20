using UnityEngine;

namespace AsteroidsGoneRogue
{
    public sealed class ShipShooter : MonoBehaviour
    {
        public const int SpreadPelletCount = 3;
        public const float SpreadHalfAngleDegrees = 14f;

        private static readonly FireMode[] CycleOrder = WeaponSlots.PrimaryCycle;

        private PlayerLoadout _loadout;
        private ContentFactory _factory;
        private float _nextFireTime;
        private float _nextUtilityTime;
        private float _utilityCooldownDuration;
        private float _boostUntil;
        private Transform _muzzle;
        private FireMode _mode = FireMode.Bolt;

        public FireMode Mode
        {
            get { return ResolvedPrimary(); }
        }

        public FireMode UtilityMode
        {
            get
            {
                LoadoutState loadout = CurrentLoadout();
                if (loadout == null || !loadout.HasUtility)
                {
                    return FireMode.Bolt;
                }

                return loadout.UtilityMode;
            }
        }

        public bool HasUtility
        {
            get
            {
                LoadoutState loadout = CurrentLoadout();
                return loadout != null && loadout.HasUtility;
            }
        }

        public bool HasAltFire
        {
            get
            {
                LoadoutState loadout = CurrentLoadout();
                return loadout != null && loadout.HasAltFire;
            }
        }

        public float UtilityCooldown01
        {
            get
            {
                if (_utilityCooldownDuration <= 0.001f)
                {
                    return 1f;
                }

                float remaining = _nextUtilityTime - Time.time;
                if (remaining <= 0f)
                {
                    return 1f;
                }

                return 1f - Mathf.Clamp01(remaining / _utilityCooldownDuration);
            }
        }

        public void Bind(PlayerLoadout loadout, ContentFactory factory, Transform muzzle)
        {
            _loadout = loadout;
            _factory = factory;
            _muzzle = muzzle;
            SyncFromLoadout();
        }

        public void SyncFromLoadout()
        {
            LoadoutState loadout = CurrentLoadout();
            _mode = loadout != null ? loadout.ResolvedPrimary() : FireMode.Bolt;
        }

        public void GrantRapidBoost(float seconds)
        {
            _boostUntil = Time.time + Mathf.Max(0.1f, seconds);
        }

        public void CycleFireMode()
        {
            CycleFireMode(1);
        }

        public void CycleFireMode(int direction)
        {
            LoadoutState loadout = CurrentLoadout();
            if (loadout == null)
            {
                _mode = FireMode.Bolt;
                return;
            }

            if (CycleOrder.Length < 1)
            {
                _mode = FireMode.Bolt;
                return;
            }

            loadout.CyclePrimary(direction);
            _mode = loadout.ResolvedPrimary();
        }

        public void TryFire()
        {
            if (Time.time < _nextFireTime || _loadout == null || _factory == null)
            {
                return;
            }

            LoadoutState loadout = _loadout.State;
            FireMode mode = ResolvedPrimary();
            float cooldown = loadout.FireCooldown * WeaponSlots.PrimaryCooldownMul(mode);
            if (Time.time < _boostUntil)
            {
                cooldown = Mathf.Min(cooldown, LoadoutState.RapidFireCooldown);
            }

            _nextFireTime = Time.time + cooldown;
            FireModeShot(mode, loadout);
        }

        public void TryFireUtility()
        {
            LoadoutState loadout = CurrentLoadout();
            if (loadout == null || !loadout.HasUtility || _factory == null)
            {
                return;
            }

            if (Time.time < _nextUtilityTime)
            {
                return;
            }

            FireMode mode = loadout.UtilityMode;
            if (!WeaponSlots.IsUtility(mode) || !loadout.OwnsMode(mode))
            {
                return;
            }

            float cooldown = WeaponSlots.UtilityCooldown(mode);
            if (mode == FireMode.Seeker)
            {
                cooldown = LoadoutState.SeekerFireCooldown;
            }
            else if (mode == FireMode.Ricochet)
            {
                cooldown = LoadoutState.RicochetFireCooldown;
            }
            _utilityCooldownDuration = cooldown;
            _nextUtilityTime = Time.time + cooldown;
            FireModeShot(mode, loadout);
        }

        private void FireModeShot(FireMode mode, LoadoutState loadout)
        {
            Vector3 origin = _muzzle != null ? _muzzle.position : transform.position + transform.forward * 1.6f;
            if (mode == FireMode.Spread)
            {
                FireSpread(origin, loadout);
            }
            else if (mode == FireMode.Twin)
            {
                FireTwin(origin, loadout);
            }
            else
            {
                _factory.SpawnStyledShot(
                    origin,
                    transform.forward,
                    ShotSpeed(loadout, mode),
                    ShotDamage(loadout, mode),
                    mode);
            }

            _factory.SpawnVfx("Vfx_MuzzleFlash", origin, 0.12f);
            PlayShotCue(mode);
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
                _factory.SpawnProjectile(origin, dir, speed, damage, false, true);
            }
        }

        private void FireTwin(Vector3 origin, LoadoutState loadout)
        {
            Vector3 right = transform.right * LoadoutState.TwinOffsetMeters;
            Vector3 forward = transform.forward;
            float speed = loadout.ProjectileSpeed;
            int damage = loadout.ProjectileDamage;
            _factory.SpawnStyledShot(origin + right, forward, speed, damage, FireMode.Twin);
            _factory.SpawnStyledShot(origin - right, forward, speed, damage, FireMode.Twin);
        }

        private static float ShotSpeed(LoadoutState loadout, FireMode mode)
        {
            float speed = loadout.ProjectileSpeed;
            if (mode == FireMode.Seeker)
            {
                return speed * LoadoutState.SeekerSpeedScale;
            }

            return speed;
        }

        private static int ShotDamage(LoadoutState loadout, FireMode mode)
        {
            int damage = loadout.ProjectileDamage;
            if (mode == FireMode.Seeker)
            {
                return Mathf.Max(1, damage - LoadoutState.SeekerDamagePenalty);
            }

            return damage;
        }

        private void PlayShotCue(FireMode mode)
        {
            if (AudioCues.Instance == null)
            {
                return;
            }

            if (mode == FireMode.Spread)
            {
                AudioCues.Instance.PlayShootSpread();
            }
            else if (mode == FireMode.Pierce)
            {
                AudioCues.Instance.PlayShootPierce();
            }
            else if (mode == FireMode.Seeker)
            {
                AudioCues.Instance.PlayShootSeeker();
            }
            else if (mode == FireMode.Twin)
            {
                AudioCues.Instance.PlayShootTwin();
            }
            else if (mode == FireMode.Ricochet)
            {
                AudioCues.Instance.PlayShootRicochet();
            }
            else
            {
                AudioCues.Instance.PlayShoot();
            }
        }

        private LoadoutState CurrentLoadout()
        {
            return _loadout != null ? _loadout.State : null;
        }

        private FireMode ResolvedPrimary()
        {
            LoadoutState loadout = CurrentLoadout();
            if (loadout == null)
            {
                return FireMode.Bolt;
            }

            FireMode requested = WeaponSlots.IsPrimary(_mode) ? _mode : loadout.ResolvedPrimary();
            return loadout.OwnsMode(requested) && WeaponSlots.IsPrimary(requested)
                ? requested
                : FireMode.Bolt;
        }
    }
}
