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
        private bool _charging;
        private float _chargeStart;
        private float _railFiredAt;
        private float _railFullCd;
        private GameObject _chargeGlow;

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

        public bool ChargesPrimary
        {
            get { return ResolvedPrimary() == FireMode.Rail; }
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
            EnsureChargeGlow();
            SyncFromLoadout();
        }

        public void SyncFromLoadout()
        {
            LoadoutState loadout = CurrentLoadout();
            _mode = loadout != null ? loadout.ResolvedPrimary() : FireMode.Bolt;
            HideCharge();
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
            HideCharge();
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

        public void TickRailCharge(bool held)
        {
            if (!ChargesPrimary)
            {
                HideCharge();
                return;
            }

            if (Time.time < _nextFireTime)
            {
                _charging = false;
                SetChargeGlow(0f);
                return;
            }

            if (held)
            {
                if (!_charging)
                {
                    _charging = true;
                    _chargeStart = Time.time;
                }

                float charge = (Time.time - _chargeStart) / DoctrineRules.RailHoldSeconds;
                SetChargeGlow(charge);
                return;
            }

            if (!_charging)
            {
                return;
            }

            float heldFor = Time.time - _chargeStart;
            _charging = false;
            SetChargeGlow(0f);
            LoadoutState loadout = CurrentLoadout();
            if (loadout == null)
            {
                return;
            }

            float full = loadout.RailCooldown;
            if (heldFor + 0.0001f >= DoctrineRules.RailHoldSeconds)
            {
                _railFiredAt = Time.time;
                _railFullCd = full;
                _nextFireTime = Time.time + full;
                FireRail(loadout);
                return;
            }

            _nextFireTime = Time.time + full * DoctrineRules.RailMissCancelCooldownMul;
        }

        public void NotifyRailMiss()
        {
            if (_railFullCd <= 0f)
            {
                return;
            }

            float halfEnd = _railFiredAt + _railFullCd * DoctrineRules.RailMissCancelCooldownMul;
            if (halfEnd < _nextFireTime)
            {
                _nextFireTime = halfEnd;
            }
        }

        public void TryFire()
        {
            if (Time.time < _nextFireTime || _loadout == null || _factory == null)
            {
                return;
            }

            LoadoutState loadout = _loadout.State;
            FireMode mode = ResolvedPrimary();
            if (mode == FireMode.Rail)
            {
                return;
            }

            float cooldown = loadout.FireCooldown * WeaponSlots.PrimaryCooldownMul(mode);
            cooldown *= loadout.PrimaryExtraMul(mode);
            if (mode == FireMode.Spread && loadout.Storm)
            {
                cooldown = Mathf.Max(cooldown, DoctrineRules.StormSeconds);
            }

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
                cooldown *= loadout.SeekerUtilityMul();
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
            Vector3 origin = MuzzleOrigin();
            if (mode == FireMode.Spread)
            {
                FireSpread(origin, loadout);
            }
            else if (mode == FireMode.Twin)
            {
                FireTwin(origin, loadout);
            }
            else if (mode == FireMode.Seeker)
            {
                FireSeekers(origin, loadout);
            }
            else
            {
                Projectile shot = _factory.SpawnStyledShot(
                    origin,
                    transform.forward,
                    ShotSpeed(loadout, mode),
                    ShotDamage(loadout, mode),
                    mode);
                if (mode == FireMode.Pierce && loadout.OverchargeLance && shot != null)
                {
                    shot.SetPierceBonusTargets(DoctrineRules.OverchargePierceBonusTargets);
                }
            }

            _factory.SpawnVfx("Vfx_MuzzleFlash", origin, 0.12f);
            PlayShotCue(mode);
        }

        private void FireRail(LoadoutState loadout)
        {
            Vector3 origin = MuzzleOrigin();
            int damage = Mathf.Max(1, Mathf.RoundToInt(loadout.ProjectileDamage * DoctrineRules.RailDamageMul));
            float speed = loadout.ProjectileSpeed * DoctrineRules.RailSpeedMul;
            Projectile shot = _factory.SpawnStyledShot(origin, transform.forward, speed, damage, FireMode.Rail);
            if (shot != null)
            {
                shot.ArmRailMiss(this);
            }

            _factory.SpawnVfx("Vfx_MuzzleFlash", origin, 0.2f);
            SetChargeGlow(1.15f);
            PlayShotCue(FireMode.Rail);
        }

        private void FireSpread(Vector3 origin, LoadoutState loadout)
        {
            int damage = loadout.SpreadPelletDamage;
            float speed = loadout.ProjectileSpeed;
            Vector3 forward = transform.forward;
            int count = loadout.Storm ? DoctrineRules.StormPelletCount : SpreadPelletCount;
            float half = SpreadHalfAngleDegrees;
            if (loadout.FlakFeed)
            {
                half += DoctrineRules.FlakFeedHalfAngleBonus;
            }

            for (int i = 0; i < count; i++)
            {
                float yaw = count <= 1 ? 0f : Mathf.Lerp(-half, half, i / (float)(count - 1));
                Vector3 dir = Quaternion.AngleAxis(yaw, Vector3.up) * forward;
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

        private void FireSeekers(Vector3 origin, LoadoutState loadout)
        {
            int count = loadout.TwinSeek ? DoctrineRules.TwinSeekCount : 1;
            float scale = loadout.TwinSeek ? DoctrineRules.TwinSeekDamageScale : 1f;
            int damage = Mathf.Max(1, Mathf.RoundToInt(ShotDamage(loadout, FireMode.Seeker) * scale));
            float speed = ShotSpeed(loadout, FireMode.Seeker);
            float turn = loadout.SeekerCadence
                ? DoctrineRules.SeekerCadenceTurnDegrees
                : Projectile.SeekerTurnDegrees;
            float offset = LoadoutState.TwinOffsetMeters;
            for (int i = 0; i < count; i++)
            {
                float side = count == 1 ? 0f : (i == 0 ? -offset : offset);
                Projectile shot = _factory.SpawnStyledShot(
                    origin + transform.right * side,
                    transform.forward,
                    speed,
                    damage,
                    FireMode.Seeker);
                if (shot != null)
                {
                    shot.SetSeekerTurn(turn);
                }
            }
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

        private Vector3 MuzzleOrigin()
        {
            return _muzzle != null ? _muzzle.position : transform.position + transform.forward * 1.6f;
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
            else if (mode == FireMode.Rail)
            {
                AudioCues.Instance.PlayShootRail();
            }
            else
            {
                AudioCues.Instance.PlayShoot();
            }
        }

        private void EnsureChargeGlow()
        {
            if (_chargeGlow != null || _factory == null)
            {
                return;
            }

            Transform parent = _muzzle != null ? _muzzle : transform;
            _chargeGlow = _factory.CreateRailChargeVfx(parent);
        }

        private void HideCharge()
        {
            _charging = false;
            SetChargeGlow(0f);
        }

        private void SetChargeGlow(float charge01)
        {
            if (_chargeGlow == null)
            {
                return;
            }

            bool on = charge01 > 0.001f;
            if (_chargeGlow.activeSelf != on)
            {
                _chargeGlow.SetActive(on);
            }

            if (!on)
            {
                return;
            }

            float t = Mathf.Clamp01(charge01);
            _chargeGlow.transform.localScale = Vector3.one * Mathf.Lerp(0.72f, 1.4f, t);
            Renderer[] renderers = _chargeGlow.GetComponentsInChildren<Renderer>(true);
            Color emission = Color.Lerp(new Color(0.35f, 0.72f, 0.9f), new Color(0.92f, 0.97f, 1f), t)
                * Mathf.Lerp(1.4f, 4.8f, t);
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetColor("_EmissionColor", emission);
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].SetPropertyBlock(block);
                }
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
