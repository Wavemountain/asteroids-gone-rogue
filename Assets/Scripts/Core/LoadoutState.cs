using System;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Upgrades bought in the hangar. Survives into the next wave.
    /// </summary>
    public sealed class LoadoutState
    {
        public const int MaxShieldCharges = 2;
        public const int HullHitPoints = 3;
        public const float BaseFireCooldown = 0.38f;
        public const float RapidFireCooldown = 0.16f;
        public const float EngineUpgrade02Cooldown = 0.12f;
        public const float BaseProjectileSpeed = 28f;
        public const float HardpointProjectileSpeed = 42f;
        public const int BaseProjectileDamage = 1;
        public const int HardpointProjectileDamage = 2;
        public const int NoseUpgrade02Damage = 3;
        public const float NoseUpgrade02Speed = 48f;

        public const int BodyUpgradeHullBonus = 1;

        public bool RapidFire { get; private set; }
        public int ShieldCharges { get; private set; }
        public bool NoseHardpoint { get; private set; }
        public bool BodyUpgrade01 { get; private set; }
        public bool NoseUpgrade02 { get; private set; }
        public bool EngineUpgrade02 { get; private set; }

        public int CurrentHullHitPoints
        {
            get { return HullHitPoints + (BodyUpgrade01 ? BodyUpgradeHullBonus : 0); }
        }

        public float FireCooldown
        {
            get
            {
                if (EngineUpgrade02)
                {
                    return EngineUpgrade02Cooldown;
                }

                return RapidFire ? RapidFireCooldown : BaseFireCooldown;
            }
        }

        public float ProjectileSpeed
        {
            get
            {
                if (NoseUpgrade02)
                {
                    return NoseUpgrade02Speed;
                }

                return NoseHardpoint ? HardpointProjectileSpeed : BaseProjectileSpeed;
            }
        }

        public int ProjectileDamage
        {
            get
            {
                if (NoseUpgrade02)
                {
                    return NoseUpgrade02Damage;
                }

                return NoseHardpoint ? HardpointProjectileDamage : BaseProjectileDamage;
            }
        }

        public bool Owns(UpgradeId id)
        {
            switch (id)
            {
                case UpgradeId.RapidFire:
                    return RapidFire;
                case UpgradeId.ShieldCell:
                    return ShieldCharges >= MaxShieldCharges;
                case UpgradeId.NoseHardpoint:
                    return NoseHardpoint;
                case UpgradeId.BodyUpgrade01:
                    return BodyUpgrade01;
                case UpgradeId.NoseUpgrade02:
                    return NoseUpgrade02;
                case UpgradeId.EngineUpgrade02:
                    return EngineUpgrade02;
                default:
                    return false;
            }
        }

        public bool CanApply(UpgradeId id)
        {
            switch (id)
            {
                case UpgradeId.NoseUpgrade02:
                    return NoseHardpoint && !NoseUpgrade02;
                case UpgradeId.EngineUpgrade02:
                    return RapidFire && !EngineUpgrade02;
                default:
                    return !Owns(id);
            }
        }

        public void Apply(UpgradeId id)
        {
            switch (id)
            {
                case UpgradeId.RapidFire:
                    RapidFire = true;
                    break;
                case UpgradeId.ShieldCell:
                    if (ShieldCharges < MaxShieldCharges)
                    {
                        ShieldCharges += 1;
                    }

                    break;
                case UpgradeId.NoseHardpoint:
                    NoseHardpoint = true;
                    break;
                case UpgradeId.BodyUpgrade01:
                    BodyUpgrade01 = true;
                    break;
                case UpgradeId.NoseUpgrade02:
                    NoseUpgrade02 = true;
                    break;
                case UpgradeId.EngineUpgrade02:
                    EngineUpgrade02 = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("id");
            }
        }
    }
}
