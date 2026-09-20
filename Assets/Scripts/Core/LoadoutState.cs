using System;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Upgrades bought in the hangar. Survives into the next wave.
    /// </summary>
    public sealed class LoadoutState
    {
        public const int MaxShieldCharges = 2;
        public const int MatrixMaxShieldCharges = 3;
        public const int HullHitPoints = 3;
        public const float BaseFireCooldown = 0.38f;
        public const float SeekerCooldownMul = WeaponSlots.SeekerCooldownMul;
        public const float RicochetCooldownMul = WeaponSlots.RicochetCooldownMul;
        public const float SeekerFireCooldown = BaseFireCooldown * SeekerCooldownMul;
        public const float RicochetFireCooldown = BaseFireCooldown * RicochetCooldownMul;
        public const float SeekerSpeedScale = 0.58f;
        public const int SeekerDamagePenalty = 1;
        public const float RapidFireCooldown = 0.16f;
        public const float EngineUpgrade02Cooldown = 0.12f;
        public const float EngineUpgrade03Cooldown = 0.09f;
        public const float AfterburnerCooldown = 0.075f;
        public const float OverchargerCooldownPenalty = 0.03f;
        public const float BaseProjectileSpeed = 28f;
        public const float HardpointProjectileSpeed = 42f;
        public const int BaseProjectileDamage = 1;
        public const int HardpointProjectileDamage = 2;
        public const int NoseUpgrade02Damage = 3;
        public const float NoseUpgrade02Speed = 48f;
        public const int NoseUpgrade03Damage = 4;
        public const float NoseUpgrade03Speed = 52f;
        public const int OverchargerDamageBonus = 1;
        public const int BodyUpgradeHullBonus = 1;
        public const float TwinOffsetMeters = 0.45f;
        public const int RicochetBounces = 2;

        public bool RapidFire { get; private set; }
        public int ShieldCharges { get; private set; }
        public bool NoseHardpoint { get; private set; }
        public bool BodyUpgrade01 { get; private set; }
        public bool BodyUpgrade02 { get; private set; }
        public bool NoseUpgrade02 { get; private set; }
        public bool NoseUpgrade03 { get; private set; }
        public bool EngineUpgrade02 { get; private set; }
        public bool EngineUpgrade03 { get; private set; }
        public bool SpreadBolt { get; private set; }
        public bool Pierce { get; private set; }
        public bool TwinGuns { get; private set; }
        public bool Seeker { get; private set; }
        public bool Ricochet { get; private set; }
        public bool ShieldMatrix { get; private set; }
        public bool Overcharger { get; private set; }
        public bool Afterburner { get; private set; }

        /// <summary>Equipped primary. Default Bolt. Cycle LB/RB among owned primaries.</summary>
        public FireMode PrimaryMode { get; private set; }

        /// <summary>Equipped utility when <see cref="HasUtility"/> is set. Start empty.</summary>
        public FireMode UtilityMode { get; private set; }

        public bool HasUtility { get; private set; }

        public void Reset()
        {
            RapidFire = false;
            ShieldCharges = 0;
            NoseHardpoint = false;
            BodyUpgrade01 = false;
            BodyUpgrade02 = false;
            NoseUpgrade02 = false;
            NoseUpgrade03 = false;
            EngineUpgrade02 = false;
            EngineUpgrade03 = false;
            SpreadBolt = false;
            Pierce = false;
            TwinGuns = false;
            Seeker = false;
            Ricochet = false;
            ShieldMatrix = false;
            Overcharger = false;
            Afterburner = false;
            PrimaryMode = FireMode.Bolt;
            UtilityMode = FireMode.Bolt;
            HasUtility = false;
        }

        public int CurrentMaxShield
        {
            get { return ShieldMatrix ? MatrixMaxShieldCharges : MaxShieldCharges; }
        }

        public int CurrentHullHitPoints
        {
            get
            {
                int hull = HullHitPoints;
                if (BodyUpgrade01)
                {
                    hull += BodyUpgradeHullBonus;
                }

                if (BodyUpgrade02)
                {
                    hull += BodyUpgradeHullBonus;
                }

                return hull;
            }
        }

        public float FireCooldown
        {
            get
            {
                if (Afterburner)
                {
                    return AfterburnerCooldown;
                }

                float cooldown = BaseFireCooldown;
                if (EngineUpgrade03)
                {
                    cooldown = EngineUpgrade03Cooldown;
                }
                else if (EngineUpgrade02)
                {
                    cooldown = EngineUpgrade02Cooldown;
                }
                else if (RapidFire)
                {
                    cooldown = RapidFireCooldown;
                }

                if (Overcharger)
                {
                    cooldown += OverchargerCooldownPenalty;
                }

                return cooldown;
            }
        }

        public float ProjectileSpeed
        {
            get
            {
                if (NoseUpgrade03)
                {
                    return NoseUpgrade03Speed;
                }

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
                int damage = BaseProjectileDamage;
                if (NoseUpgrade03)
                {
                    damage = NoseUpgrade03Damage;
                }
                else if (NoseUpgrade02)
                {
                    damage = NoseUpgrade02Damage;
                }
                else if (NoseHardpoint)
                {
                    damage = HardpointProjectileDamage;
                }

                if (Overcharger)
                {
                    damage += OverchargerDamageBonus;
                }

                return damage;
            }
        }

        public int SpreadPelletDamage
        {
            get { return Math.Max(1, ProjectileDamage / 3); }
        }

        public bool HasPrimaryAlt
        {
            get { return SpreadBolt || Pierce || TwinGuns; }
        }

        public bool HasAltFire
        {
            get { return HasPrimaryAlt || Seeker || Ricochet; }
        }

        public float PrimaryCooldown
        {
            get { return FireCooldown * WeaponSlots.PrimaryCooldownMul(ResolvedPrimary()); }
        }

        public float UtilityCooldown
        {
            get
            {
                if (!HasUtility)
                {
                    return 0f;
                }

                return WeaponSlots.UtilityCooldown(UtilityMode);
            }
        }

        public FireMode ResolvedPrimary()
        {
            return OwnsMode(PrimaryMode) && WeaponSlots.IsPrimary(PrimaryMode)
                ? PrimaryMode
                : FireMode.Bolt;
        }

        public bool OwnsMode(FireMode mode)
        {
            if (mode == FireMode.Bolt)
            {
                return true;
            }

            if (mode == FireMode.Spread)
            {
                return SpreadBolt;
            }

            if (mode == FireMode.Pierce)
            {
                return Pierce;
            }

            if (mode == FireMode.Twin)
            {
                return TwinGuns;
            }

            if (mode == FireMode.Seeker)
            {
                return Seeker;
            }

            if (mode == FireMode.Ricochet)
            {
                return Ricochet;
            }

            return false;
        }

        public bool IsEquipped(UpgradeId id)
        {
            FireMode mode;
            WeaponSlot slot;
            if (!WeaponSlots.TryMode(id, out mode, out slot))
            {
                return false;
            }

            if (slot == WeaponSlot.Utility)
            {
                return HasUtility && UtilityMode == mode;
            }

            return ResolvedPrimary() == mode;
        }

        public bool TryEquip(UpgradeId id)
        {
            FireMode mode;
            WeaponSlot slot;
            if (!WeaponSlots.TryMode(id, out mode, out slot) || !Owns(id))
            {
                return false;
            }

            if (slot == WeaponSlot.Primary)
            {
                if (PrimaryMode == mode)
                {
                    PrimaryMode = FireMode.Bolt;
                }
                else
                {
                    PrimaryMode = mode;
                }

                return true;
            }

            if (HasUtility && UtilityMode == mode)
            {
                HasUtility = false;
                UtilityMode = FireMode.Bolt;
                return true;
            }

            UtilityMode = mode;
            HasUtility = true;
            return true;
        }

        public void AutoEquipAfterPurchase(UpgradeId id)
        {
            FireMode mode;
            WeaponSlot slot;
            if (!WeaponSlots.TryMode(id, out mode, out slot) || !Owns(id))
            {
                return;
            }

            if (slot == WeaponSlot.Primary)
            {
                PrimaryMode = mode;
                return;
            }

            if (!HasUtility)
            {
                UtilityMode = mode;
                HasUtility = true;
            }
        }

        public void CyclePrimary(int direction)
        {
            if (!HasPrimaryAlt)
            {
                PrimaryMode = FireMode.Bolt;
                return;
            }

            int dir = direction < 0 ? -1 : 1;
            FireMode current = ResolvedPrimary();
            int start = 0;
            for (int i = 0; i < WeaponSlots.PrimaryCycle.Length; i++)
            {
                if (WeaponSlots.PrimaryCycle[i] == current)
                {
                    start = i;
                    break;
                }
            }

            int length = WeaponSlots.PrimaryCycle.Length;
            for (int step = 1; step <= length; step++)
            {
                int index = (start + dir * step) % length;
                if (index < 0)
                {
                    index += length;
                }

                FireMode next = WeaponSlots.PrimaryCycle[index];
                if (OwnsMode(next))
                {
                    PrimaryMode = next;
                    return;
                }
            }

            PrimaryMode = FireMode.Bolt;
        }

        public LoadoutState Clone()
        {
            LoadoutState copy = new LoadoutState();
            copy.RapidFire = RapidFire;
            copy.ShieldCharges = ShieldCharges;
            copy.NoseHardpoint = NoseHardpoint;
            copy.BodyUpgrade01 = BodyUpgrade01;
            copy.BodyUpgrade02 = BodyUpgrade02;
            copy.NoseUpgrade02 = NoseUpgrade02;
            copy.NoseUpgrade03 = NoseUpgrade03;
            copy.EngineUpgrade02 = EngineUpgrade02;
            copy.EngineUpgrade03 = EngineUpgrade03;
            copy.SpreadBolt = SpreadBolt;
            copy.Pierce = Pierce;
            copy.TwinGuns = TwinGuns;
            copy.Seeker = Seeker;
            copy.Ricochet = Ricochet;
            copy.ShieldMatrix = ShieldMatrix;
            copy.Overcharger = Overcharger;
            copy.Afterburner = Afterburner;
            copy.PrimaryMode = PrimaryMode;
            copy.UtilityMode = UtilityMode;
            copy.HasUtility = HasUtility;
            return copy;
        }

        public LoadoutState WithPreview(UpgradeId id)
        {
            LoadoutState copy = Clone();
            if (copy.CanApply(id))
            {
                copy.Apply(id);
            }

            return copy;
        }

        public bool Owns(UpgradeId id)
        {
            switch (id)
            {
                case UpgradeId.RapidFire:
                    return RapidFire;
                case UpgradeId.ShieldCell:
                    return ShieldCharges >= CurrentMaxShield;
                case UpgradeId.NoseHardpoint:
                    return NoseHardpoint;
                case UpgradeId.BodyUpgrade01:
                    return BodyUpgrade01;
                case UpgradeId.BodyUpgrade02:
                    return BodyUpgrade02;
                case UpgradeId.NoseUpgrade02:
                    return NoseUpgrade02;
                case UpgradeId.NoseUpgrade03:
                    return NoseUpgrade03;
                case UpgradeId.EngineUpgrade02:
                    return EngineUpgrade02;
                case UpgradeId.EngineUpgrade03:
                    return EngineUpgrade03;
                case UpgradeId.SpreadBolt:
                    return SpreadBolt;
                case UpgradeId.Pierce:
                    return Pierce;
                case UpgradeId.TwinGuns:
                    return TwinGuns;
                case UpgradeId.Seeker:
                    return Seeker;
                case UpgradeId.Ricochet:
                    return Ricochet;
                case UpgradeId.ShieldMatrix:
                    return ShieldMatrix;
                case UpgradeId.Overcharger:
                    return Overcharger;
                case UpgradeId.Afterburner:
                    return Afterburner;
                default:
                    return false;
            }
        }

        public bool CanApply(UpgradeId id)
        {
            switch (id)
            {
                case UpgradeId.BodyUpgrade02:
                    return BodyUpgrade01 && !BodyUpgrade02;
                case UpgradeId.NoseUpgrade02:
                    return NoseHardpoint && !NoseUpgrade02;
                case UpgradeId.NoseUpgrade03:
                    return NoseUpgrade02 && !NoseUpgrade03;
                case UpgradeId.EngineUpgrade02:
                    return RapidFire && !EngineUpgrade02;
                case UpgradeId.EngineUpgrade03:
                    return EngineUpgrade02 && !EngineUpgrade03;
                case UpgradeId.Overcharger:
                    return NoseUpgrade03 && !Overcharger && !Afterburner;
                case UpgradeId.Afterburner:
                    return EngineUpgrade03 && !Afterburner && !Overcharger;
                case UpgradeId.ShieldMatrix:
                    return ShieldCharges >= MaxShieldCharges && !ShieldMatrix;
                case UpgradeId.ShieldCell:
                    return ShieldCharges < CurrentMaxShield;
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
                    if (ShieldCharges < CurrentMaxShield)
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
                case UpgradeId.BodyUpgrade02:
                    BodyUpgrade02 = true;
                    break;
                case UpgradeId.NoseUpgrade02:
                    NoseUpgrade02 = true;
                    break;
                case UpgradeId.NoseUpgrade03:
                    NoseUpgrade03 = true;
                    break;
                case UpgradeId.EngineUpgrade02:
                    EngineUpgrade02 = true;
                    break;
                case UpgradeId.EngineUpgrade03:
                    EngineUpgrade03 = true;
                    break;
                case UpgradeId.SpreadBolt:
                    SpreadBolt = true;
                    break;
                case UpgradeId.Pierce:
                    Pierce = true;
                    break;
                case UpgradeId.TwinGuns:
                    TwinGuns = true;
                    break;
                case UpgradeId.Seeker:
                    Seeker = true;
                    break;
                case UpgradeId.Ricochet:
                    Ricochet = true;
                    break;
                case UpgradeId.ShieldMatrix:
                    ShieldMatrix = true;
                    break;
                case UpgradeId.Overcharger:
                    Overcharger = true;
                    break;
                case UpgradeId.Afterburner:
                    Afterburner = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("id");
            }
        }
    }
}
