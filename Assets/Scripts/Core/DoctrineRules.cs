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
    /// </summary>
    public static class DoctrineRules
    {
        public const int UnlockWave = 2;
        public const int BarrageGateCost = 150;
        public const float OffPathMul = 1.35f;

        public const float FlakFeedCooldownMul = 0.85f;
        public const float FlakFeedHalfAngleBonus = 4f;
        public const int FlakFeedCost = 150;

        public const int StormPelletCount = 5;
        public const float StormCooldownMul = 1.8f;
        public const float StormSeconds = 1.5f;
        public const int StormCost = 240;

        public const float RailHoldSeconds = 0.55f;
        public const float RailDamageMul = 3f;
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
