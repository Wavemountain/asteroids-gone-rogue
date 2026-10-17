namespace AsteroidsGoneRogue
{
    public enum DoctrineId
    {
        None,
        Barrage,
        Lance,
        Hunter
    }

    /// <summary>
    /// 0.45 doctrine + Rail numbers. One doctrine per run. Mid purchase
    /// soft-locks the other paths. No Flak weapon, no mid-run swap.
    /// Barrage 55 + Spread 110 and Hunter 40 + Seeker 125 still land on a
    /// Normal wave-1 clear (165) and still miss Hard (140). Twin is 190, so
    /// Twin 190 + Lance 45 = 235 and misses that purse
    /// (Pierce + Lance is 200, still over). Off-path soft-lock stays ×1.35.
    /// </summary>
    public static class DoctrineRules
    {
        public const int UnlockWave = 2;
        public const int BarrageGateCost = 55;
        public const int LanceGateCost = 45;
        public const int HunterGateCost = 40;
        public const float OffPathMul = 1.35f;

        public const float FlakFeedCooldownMul = 0.85f;
        public const float FlakFeedHalfAngleBonus = 4f;
        public const int FlakFeedCost = 150;

        public const int StormPelletCount = 5;
        public const float StormCooldownMul = 1.3f;
        public const float StormSeconds = 0.1f;
        public const int StormCost = 240;

        public const float RailHoldSeconds = 0.55f;
        public const float RailDamageMul = 8f;
        public const float RailSpeedMul = 1.2f;
        public const float RailCooldownMul = 1.6f;
        public const float RailMissCancelCooldownMul = 0.5f;
        public const int RailCost = 160;

        public const float OverchargeTwinCooldownMul = 0.9f;
        public const int OverchargePierceBonusTargets = 1;
        public const int OverchargeLanceCost = 235;

        public const float SeekerCadenceCooldownMul = 0.75f;
        public const float SeekerCadenceTurnDegrees = 165f;
        public const int SeekerCadenceCost = 155;

        public const int TwinSeekCount = 2;
        public const float TwinSeekDamageScale = 0.70f;
        public const float TwinSeekCooldownMul = 1.2f;
        public const int TwinSeekCost = 225;

        public static bool HangarUnlocked(int waveIndex)
        {
            return waveIndex >= UnlockWave;
        }

        /// <summary>
        /// A doctrine card is an action only when it can actually be picked.
        /// "New Run to swap", gate text, owned, and too-poor are lock reasons, not buttons.
        /// </summary>
        public static bool CardIsAction(bool shopOpen, bool chosen, bool otherPath, bool gateMet, bool tooPoor)
        {
            if (!shopOpen || chosen || otherPath || !gateMet || tooPoor)
            {
                return false;
            }

            return true;
        }

        public static bool IsCapstone(UpgradeId id)
        {
            return id == UpgradeId.Storm
                || id == UpgradeId.OverchargeLance
                || id == UpgradeId.TwinSeek;
        }

        public static bool IsWildcard(UpgradeId id)
        {
            return id == UpgradeId.Ricochet;
        }

        public static DoctrineId PathOf(UpgradeId id)
        {
            switch (id)
            {
                case UpgradeId.SpreadBolt:
                case UpgradeId.FlakFeed:
                case UpgradeId.Storm:
                    return DoctrineId.Barrage;
                case UpgradeId.Pierce:
                case UpgradeId.TwinGuns:
                case UpgradeId.Rail:
                case UpgradeId.OverchargeLance:
                    return DoctrineId.Lance;
                case UpgradeId.Seeker:
                case UpgradeId.SeekerCadence:
                case UpgradeId.TwinSeek:
                    return DoctrineId.Hunter;
                default:
                    return DoctrineId.None;
            }
        }

        public static DoctrineId PathOf(FireMode mode)
        {
            switch (mode)
            {
                case FireMode.Spread:
                    return DoctrineId.Barrage;
                case FireMode.Pierce:
                case FireMode.Twin:
                case FireMode.Rail:
                    return DoctrineId.Lance;
                case FireMode.Seeker:
                    return DoctrineId.Hunter;
                default:
                    return DoctrineId.None;
            }
        }

        public static int PenalizedCost(int cost, bool offPath)
        {
            if (!offPath || cost <= 0)
            {
                return cost;
            }

            return (cost * 135 + 50) / 100;
        }

        public static string Key(DoctrineId id)
        {
            switch (id)
            {
                case DoctrineId.Barrage:
                    return "barrage";
                case DoctrineId.Lance:
                    return "lance";
                case DoctrineId.Hunter:
                    return "hunter";
                default:
                    return "none";
            }
        }
    }
}
