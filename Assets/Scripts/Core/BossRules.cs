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
        public const int HpFactor = 18;
        public const int HardNormalHpWorlds = 1;
        public const float VisualScale = 1.8f;
        public const float SpeedScale = 0.62f;
        public const int BoltDamage = 1;
        public const int AimedBurstCount = 3;
        public const int RadialCount = 8;
        public const float TelegraphSeconds = 0.8f;
        public const float AimedWindupSeconds = 0.4f;
        public const float AimedWindupMin = 0.3f;
        public const float AimedWindupMax = 0.5f;
        public const float AimedGapSeconds = 1.7f;
        public const float RadialGapSeconds = 2.6f;
        public const float StaggerSeconds = 1f;
        public const float Phase3RadialGapSeconds = 2.85f;
        public const float Phase3SpeedMul = 1.10f;
        public const float RadialPairSeconds = 0.4f;
        public const float AimedPairSeconds = 0.35f;

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
        /// 18 × base Brute HP × world HP percent, then Easy/Normal/Hard.
        /// The world percent uses the unwrapped world and holds at
        /// <see cref="DifficultySettings.MaxWorldHpSteps"/>, so the sequence never drops.
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

            if (grade == DifficultyGrade.Hard && world <= HardNormalHpWorlds)
            {
                grade = DifficultyGrade.Normal;
            }

            return DifficultySettings.ApplyGradeHp(raw, grade);
        }

        public static bool WindupInBand()
        {
            return AimedWindupSeconds >= AimedWindupMin && AimedWindupSeconds <= AimedWindupMax;
        }

        public static float AimedWindupFor(int layoutWorld)
        {
            return BossWorldTwist.AimedWindupSeconds(layoutWorld);
        }

        public static float TelegraphFor(int layoutWorld)
        {
            return BossWorldTwist.RadialTelegraphSeconds(layoutWorld);
        }

        public static int GapCount(int layoutWorld, int phase, int grade)
        {
            return BossWorldTwist.GapCount(layoutWorld, phase, grade);
        }

        public static int PatternStep(int phase, int grade, int index)
        {
            return BossWorldTwist.PatternStep(phase, grade, index);
        }

        public static bool PhaseTelegraphsFair()
        {
            return WindupInBand() && BossWorldTwist.PhaseTelegraphsFair();
        }

        public static string Label()
        {
            return Loc.T("ui.boss", "WORLD GUARDIAN");
        }
    }
}
