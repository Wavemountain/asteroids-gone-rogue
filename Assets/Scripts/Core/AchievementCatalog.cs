namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Steam-slice achievements. Bitmask ids match hangar medals so new unlocks
    /// can land without a schema bump. Steam API names are stable for a later
    /// Steamworks drop-in; local persist + toast ship without the SDK.
    /// </summary>
    public enum AchievementId
    {
        FirstClear = 1,
        NoHitWave = 2,
        HardClear = 4,
        ExtraLifeStreak = 8,
        Doctrine = 16,
        RailCharge = 32
    }

    public static class AchievementCatalog
    {
        public const int ExtraLifeStreakNeed = 2;
        public const string PrefsKey = "agr.achievements.mask";
        public const string FirstClearTitle = "First Clear";
        public const string NoHitWaveTitle = "No-Hit Wave";
        public const string HardClearTitle = "Hard Clear";
        public const string ExtraLifeStreakTitle = "Extra-Life Streak";
        public const string DoctrineTitle = "Doctrine";
        public const string RailChargeTitle = "Rail Charge";

        public static readonly AchievementId[] All =
        {
            AchievementId.FirstClear,
            AchievementId.NoHitWave,
            AchievementId.HardClear,
            AchievementId.ExtraLifeStreak,
            AchievementId.Doctrine,
            AchievementId.RailCharge
        };

        public static string Title(AchievementId id)
        {
            switch (id)
            {
                case AchievementId.NoHitWave:
                    return Loc.T("ach.nohit", NoHitWaveTitle);
                case AchievementId.HardClear:
                    return Loc.T("ach.hard", HardClearTitle);
                case AchievementId.ExtraLifeStreak:
                    return Loc.T("ach.streak", ExtraLifeStreakTitle);
                case AchievementId.Doctrine:
                    return Loc.T("ach.doctrine", DoctrineTitle);
                case AchievementId.RailCharge:
                    return Loc.T("ach.rail", RailChargeTitle);
                default:
                    return Loc.T("ach.first", FirstClearTitle);
            }
        }

        public static string SteamApiName(AchievementId id)
        {
            switch (id)
            {
                case AchievementId.NoHitWave:
                    return "AGR_NO_HIT_WAVE";
                case AchievementId.HardClear:
                    return "AGR_HARD_CLEAR";
                case AchievementId.ExtraLifeStreak:
                    return "AGR_EXTRALIFE_STREAK";
                case AchievementId.Doctrine:
                    return "AGR_DOCTRINE";
                case AchievementId.RailCharge:
                    return "AGR_RAIL_CHARGE";
                default:
                    return "AGR_FIRST_CLEAR";
            }
        }

        public static string AwardLine(AchievementId id)
        {
            return "★ " + Title(id);
        }

        public static string LockedLine(AchievementId id)
        {
            return "○ " + Title(id);
        }

        public static string ToastLine(AchievementId id)
        {
            return Loc.Tf("ach.unlock", "ACHIEVEMENT  ·  {0}", Title(id));
        }

        public static bool Owns(int mask, AchievementId id)
        {
            return (mask & (int)id) != 0;
        }

        public static int WithAward(int mask, AchievementId id)
        {
            return mask | (int)id;
        }

        public static string LadderLine(int mask)
        {
            string line = string.Empty;
            for (int i = 0; i < All.Length; i++)
            {
                AchievementId id = All[i];
                if (line.Length > 0)
                {
                    line += "  ·  ";
                }

                line += Owns(mask, id) ? AwardLine(id) : LockedLine(id);
            }

            return line;
        }

        public static bool ShouldUnlockFirstClear(int clearedWave)
        {
            return clearedWave >= 1;
        }

        public static bool ShouldUnlockNoHit(bool waveTookHit)
        {
            return !waveTookHit;
        }

        public static bool ShouldUnlockHardClear(int clearedWave, DifficultyGrade grade)
        {
            return CampaignCap.IsFinalWave(clearedWave) && grade == DifficultyGrade.Hard;
        }

        public static bool ShouldUnlockExtraLifeStreak(int streak)
        {
            return streak >= ExtraLifeStreakNeed;
        }
    }
}
