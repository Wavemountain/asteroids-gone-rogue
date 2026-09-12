using UnityEngine;

namespace AsteroidsGoneRogue
{
    public sealed class ShipShooter : MonoBehaviour
    {
        public const int SpreadPelletCount = 3;
        public const float SpreadHalfAngleDegrees = 14f;

        private static readonly FireMode[] CycleOrder =
        {
            FireMode.Bolt,
            FireMode.Spread,
            FireMode.Twin,
            FireMode.Pierce,
            FireMode.Seeker,
            FireMode.Ricochet
        };

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
            if (!OwnsAnyAltFire())
            {
                _mode = FireMode.Bolt;
                return;
            }

            FireMode current = OwnedMode(_mode);
            int start = ModeIndex(current);
            for (int step = 1; step <= CycleOrder.Length; step++)
            {
                FireMode next = CycleOrder[(start + step) % CycleOrder.Length];
                if (ModeOwned(next))
                {
                    _mode = next;
                    return;
                }
            }

            _mode = FireMode.Bolt;
        }

        public void TryFire()
        {
            if (Time.time < _nextFireTime || _loadout == null || _factory == null)
            {
                return;
            }

            LoadoutState loadout = _loadout.State;
            FireMode mode = OwnedMode(_mode);
            float cooldown = loadout.FireCooldown;
            if (mode == FireMode.Seeker)
            {
                cooldown = LoadoutState.SeekerFireCooldown;
            }
            else if (Time.time < _boostUntil)
            {
                cooldown = Mathf.Min(cooldown, LoadoutState.RapidFireCooldown);
            }

            _nextFireTime = Time.time + cooldown;
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

        private bool Owns(UpgradeId id)
        {
            return _loadout != null && _loadout.State != null && _loadout.State.Owns(id);
        }

        private bool OwnsAnyAltFire()
        {
            return _loadout != null && _loadout.State != null && _loadout.State.HasAltFire;
        }

        private bool ModeOwned(FireMode mode)
        {
            if (mode == FireMode.Bolt)
            {
                return true;
            }

            if (mode == FireMode.Spread)
            {
                return Owns(UpgradeId.SpreadBolt);
            }

            if (mode == FireMode.Pierce)
            {
                return Owns(UpgradeId.Pierce);
            }

            if (mode == FireMode.Twin)
            {
                return Owns(UpgradeId.TwinGuns);
            }

            if (mode == FireMode.Seeker)
            {
                return Owns(UpgradeId.Seeker);
            }

            if (mode == FireMode.Ricochet)
            {
                return Owns(UpgradeId.Ricochet);
            }

            return false;
        }

        private FireMode OwnedMode(FireMode requested)
        {
            return ModeOwned(requested) ? requested : FireMode.Bolt;
        }

        private static int ModeIndex(FireMode mode)
        {
            for (int i = 0; i < CycleOrder.Length; i++)
            {
                if (CycleOrder[i] == mode)
                {
                    return i;
                }
            }

            return 0;
        }
    }
}
