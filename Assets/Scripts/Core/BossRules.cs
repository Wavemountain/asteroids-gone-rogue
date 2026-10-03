namespace AsteroidsGoneRogue
{
    /// <summary>
    /// World finales are waves 5, 10, 15, … The guardian is a scaled Brute
    /// built at runtime from the existing enemy prefab. HP is a pure function
    /// of the unwrapped world, then the difficulty grade.
    /// </summary>
    public static class BossRules
    {
        public const int FirstWave = 5;
        public const int HpFactor = 12;
        public const float VisualScale = 1.8f;
        public const float SpeedScale = 0.62f;
        public const int BoltDamage = 1;
        public const int AimedBurstCount = 3;
        public const int RadialCount = 8;
        public const float TelegraphSeconds = 0.8f;
        public const float AimedGapSeconds = 1.7f;
        public const float RadialGapSeconds = 2.6f;

        public static bool IsBossWave(int waveIndex)
        {
            int wave = waveIndex < 1 ? 1 : waveIndex;
            return wave >= FirstWave && (wave % WorldCatalog.WavesPerWorld) == 0;
        }

        /// <summary>
        /// Wave 5 already brings the guardian. The ladder Brute that used to
        /// own that wave stays in the roster table and does not also spawn.
        /// Later boss waves keep their roster Brutes.
        /// </summary>
        public static bool SuppressLadderBrute(int waveIndex)
        {
            return waveIndex == FirstWave && IsBossWave(waveIndex);
        }

        /// <summary>
        /// 12 × base Brute HP × world HP percent, then Easy/Normal/Hard.
        /// World 8 holds at the world-7 cap, so the sequence never drops.
        /// </summary>
        public static int HitPoints(int waveIndex, DifficultyGrade grade)
        {
            int world = WorldCatalog.NumberForWave(waveIndex);
            int worldPercent = WorldCatalog.HpMultiplierPercent(world);
            int raw = HpFactor * EnemyCatalog.HitPoints(EnemyKind.Brute) * worldPercent / 100;
            if (raw < 1)
            {
                raw = 1;
            }

            return DifficultySettings.ApplyGradeHp(raw, grade);
        }

        public static string Label()
        {
            return Loc.T("ui.boss", "WORLD GUARDIAN");
        }
    }
}
