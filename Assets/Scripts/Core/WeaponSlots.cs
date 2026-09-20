namespace AsteroidsGoneRogue
{
    public enum WeaponSlot
    {
        Primary,
        Utility
    }

    /// <summary>
    /// Dual-fire v1 slot rules. Primary cycles Bolt + owned Spread/Twin/Pierce.
    /// Utility is Seeker / Ricochet (Hook later). No new FireModes this pass.
    /// </summary>
    public static class WeaponSlots
    {
        public const float BoltCooldownMul = 1f;
        public const float SpreadCooldownMul = 1.35f;
        public const float TwinCooldownMul = 1.1f;
        public const float PierceCooldownMul = 1f;
        public const float SeekerCooldownMul = 2.4f;
        public const float RicochetCooldownMul = 1.5f;

        public static readonly FireMode[] PrimaryCycle =
        {
            FireMode.Bolt,
            FireMode.Spread,
            FireMode.Twin,
            FireMode.Pierce
        };

        public static bool IsPrimary(FireMode mode)
        {
            return mode == FireMode.Bolt
                || mode == FireMode.Spread
                || mode == FireMode.Twin
                || mode == FireMode.Pierce;
        }

        public static bool IsUtility(FireMode mode)
        {
            return mode == FireMode.Seeker || mode == FireMode.Ricochet;
        }

        public static bool TrySlot(UpgradeId id, out WeaponSlot slot)
        {
            FireMode mode;
            return TryMode(id, out mode, out slot);
        }

        public static bool TryMode(UpgradeId id, out FireMode mode, out WeaponSlot slot)
        {
            switch (id)
            {
                case UpgradeId.SpreadBolt:
                    mode = FireMode.Spread;
                    slot = WeaponSlot.Primary;
                    return true;
                case UpgradeId.TwinGuns:
                    mode = FireMode.Twin;
                    slot = WeaponSlot.Primary;
                    return true;
                case UpgradeId.Pierce:
                    mode = FireMode.Pierce;
                    slot = WeaponSlot.Primary;
                    return true;
                case UpgradeId.Seeker:
                    mode = FireMode.Seeker;
                    slot = WeaponSlot.Utility;
                    return true;
                case UpgradeId.Ricochet:
                    mode = FireMode.Ricochet;
                    slot = WeaponSlot.Utility;
                    return true;
                default:
                    mode = FireMode.Bolt;
                    slot = WeaponSlot.Primary;
                    return false;
            }
        }

        public static bool IsWeapon(UpgradeId id)
        {
            WeaponSlot slot;
            return TrySlot(id, out slot);
        }

        public static float PrimaryCooldownMul(FireMode mode)
        {
            if (mode == FireMode.Spread)
            {
                return SpreadCooldownMul;
            }

            if (mode == FireMode.Twin)
            {
                return TwinCooldownMul;
            }

            if (mode == FireMode.Pierce)
            {
                return PierceCooldownMul;
            }

            return BoltCooldownMul;
        }

        public static float UtilityCooldownMul(FireMode mode)
        {
            if (mode == FireMode.Ricochet)
            {
                return RicochetCooldownMul;
            }

            return SeekerCooldownMul;
        }

        public static float UtilityCooldown(FireMode mode)
        {
            return LoadoutState.BaseFireCooldown * UtilityCooldownMul(mode);
        }
    }
}
