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
        public const float BaseProjectileSpeed = 28f;
        public const float HardpointProjectileSpeed = 42f;
        public const int BaseProjectileDamage = 1;
        public const int HardpointProjectileDamage = 2;

        public const int BodyUpgradeHullBonus = 1;

        public bool RapidFire { get; private set; }
        public int ShieldCharges { get; private set; }
        public bool NoseHardpoint { get; private set; }
        public bool BodyUpgrade01 { get; private set; }

        public int CurrentHullHitPoints
        {
            get { return HullHitPoints + (BodyUpgrade01 ? BodyUpgradeHullBonus : 0); }
        }

        public float FireCooldown
        {
            get { return RapidFire ? RapidFireCooldown : BaseFireCooldown; }
        }

        public float ProjectileSpeed
        {
            get { return NoseHardpoint ? HardpointProjectileSpeed : BaseProjectileSpeed; }
        }

        public int ProjectileDamage
        {
            get { return NoseHardpoint ? HardpointProjectileDamage : BaseProjectileDamage; }
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
                default:
                    return false;
            }
        }

        public bool CanApply(UpgradeId id)
        {
            return !Owns(id);
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
                default:
                    throw new ArgumentOutOfRangeException("id");
            }
        }
    }
}
