namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Per-wave combat and credit curve. Unity-free so tests can walk waves 1–200
    /// without the Editor. Grades still multiply on top through
    /// <see cref="DifficultySettings"/>; this struct is the unwrapped wave layer.
    /// Every field is monotonic and capped. World 1 wave 1 matches today's baseline.
    /// </summary>
    public struct WaveDifficulty
    {
        public int Wave;
        public int World;
        public int HpPercent;
        public int DamagePercent;
        public int ExtraEnemies;
        public int FireRatePercent;
        public int AsteroidCount;
        public int CreditPercent;
    }

    public static class DifficultyCurve
    {
        public const int WithinWorldHpPercent = 3;
        public const int DamageBonusPerWorld = 4;
        public const int MaxDamageBonus = 60;
        public const int ExtraEnemyWorldSpan = 2;
        public const int MaxExtraEnemies = 4;
        public const int FireBonusPerWorld = 2;
        public const int MaxFireBonus = 30;
        public const int CreditBonusPerWorld = 8;
        public const int MaxCreditBonus = 50;
        public const int BaseLargeAsteroids = 5;
        public const int EarlyAsteroidCap = 7;
        public const int PlateauWave = 10;
        public const int PlateauAsteroidCap = 10;
        public const int MaxSpawnedEnemies = 15;

        public static WaveDifficulty ForWave(int waveIndex)
        {
            int wave = waveIndex < 1 ? 1 : waveIndex;
            int world = WorldCatalog.NumberForWave(wave);
            WaveDifficulty snapshot = new WaveDifficulty();
            snapshot.Wave = wave;
            snapshot.World = world;
            snapshot.HpPercent = HpPercentForWave(wave);
            snapshot.DamagePercent = DamagePercentForWorld(world);
            snapshot.ExtraEnemies = ExtraEnemiesForWorld(world);
            snapshot.FireRatePercent = FireRatePercentForWorld(world);
            snapshot.AsteroidCount = AsteroidCount(wave);
            snapshot.CreditPercent = CreditPercentForWorld(world);
            return snapshot;
        }

        /// <summary>
        /// Existing +15% per world step (capped), plus +3% for each wave inside the world.
        /// Wave 5 of a world is harder than wave 1. After world 7 the percent holds at the peak
        /// so later loops do not drop.
        /// </summary>
        public static int HpPercentForWave(int waveIndex)
        {
            int wave = waveIndex < 1 ? 1 : waveIndex;
            int world = WorldCatalog.NumberForWave(wave);
            int steps = WorldCatalog.HpSteps(world);
            int within = (wave - 1) % WorldCatalog.WavesPerWorld;
            if (steps >= DifficultySettings.MaxWorldHpSteps && world > WorldCatalog.Count)
            {
                within = WorldCatalog.WavesPerWorld - 1;
            }

            int percent = 100
                + (DifficultySettings.WorldHpPercent * steps)
                + (WithinWorldHpPercent * within);
            int peak = PeakHpPercent();
            if (percent > peak)
            {
                percent = peak;
            }

            return percent;
        }

        public static int PeakHpPercent()
        {
            return 100
                + (DifficultySettings.WorldHpPercent * DifficultySettings.MaxWorldHpSteps)
                + (WithinWorldHpPercent * (WorldCatalog.WavesPerWorld - 1));
        }

        public static int DamagePercentForWorld(int worldNumber)
        {
            int world = worldNumber < 1 ? 1 : worldNumber;
            int bonus = (world - 1) * DamageBonusPerWorld;
            if (bonus < 0)
            {
                bonus = 0;
            }

            if (bonus > MaxDamageBonus)
            {
                bonus = MaxDamageBonus;
            }

            return 100 + bonus;
        }

        public static int ExtraEnemiesForWorld(int worldNumber)
        {
            int world = worldNumber < 1 ? 1 : worldNumber;
            int extra = (world - 1) / ExtraEnemyWorldSpan;
            if (extra < 0)
            {
                extra = 0;
            }

            if (extra > MaxExtraEnemies)
            {
                extra = MaxExtraEnemies;
            }

            return extra;
        }

        public static int FireRatePercentForWorld(int worldNumber)
        {
            int world = worldNumber < 1 ? 1 : worldNumber;
            int bonus = (world - 1) * FireBonusPerWorld;
            if (bonus < 0)
            {
                bonus = 0;
            }

            if (bonus > MaxFireBonus)
            {
                bonus = MaxFireBonus;
            }

            return 100 + bonus;
        }

        public static int CreditPercentForWorld(int worldNumber)
        {
            int world = worldNumber < 1 ? 1 : worldNumber;
            int bonus = (world - 1) * CreditBonusPerWorld;
            if (bonus < 0)
            {
                bonus = 0;
            }

            if (bonus > MaxCreditBonus)
            {
                bonus = MaxCreditBonus;
            }

            return 100 + bonus;
        }

        /// <summary>
        /// Same plateau as <see cref="WaveManager.LargeAsteroidCount"/>.
        /// </summary>
        public static int AsteroidCount(int waveIndex)
        {
            int wave = waveIndex < 1 ? 1 : waveIndex;
            int count = BaseLargeAsteroids + (wave - 1);
            if (count < BaseLargeAsteroids)
            {
                count = BaseLargeAsteroids;
            }

            if (count > EarlyAsteroidCap)
            {
                count = EarlyAsteroidCap;
            }

            if (wave > PlateauWave)
            {
                count = count + (wave - PlateauWave);
                if (count > PlateauAsteroidCap)
                {
                    count = PlateauAsteroidCap;
                }
            }

            return count;
        }

        /// <summary>
        /// Grade first (Easy 4/5, Hard 5/4), then the wave HP percent. World 1 wave 1
        /// matches <see cref="DifficultySettings.ScaleEnemyHpForWorld"/> at world 1.
        /// </summary>
        public static int ScaleHp(int hp, int waveIndex, DifficultyGrade grade)
        {
            int graded = DifficultySettings.ApplyGradeHp(hp, grade);
            return ApplyPercent(graded, HpPercentForWave(waveIndex));
        }

        /// <summary>
        /// Slow outgoing-damage scale. Half-up so a base of 1 stays 1 until the
        /// bonus crosses +50%, then steps to 2. The percent itself caps at +60%.
        /// </summary>
        public static int ScaleOutgoingDamage(int amount, int waveIndex)
        {
            if (amount < 1)
            {
                return 0;
            }

            int percent = DamagePercentForWorld(WorldCatalog.NumberForWave(waveIndex));
            int scaled = (amount * percent + 50) / 100;
            if (scaled < 1)
            {
                scaled = 1;
            }

            return scaled;
        }

        /// <summary>
        /// World credit scale on the existing base, then +25% when the wave is elite.
        /// Base economy numbers are not changed.
        /// </summary>
        public static int ScaleCredits(int baseCredits, int waveIndex)
        {
            if (baseCredits < 0)
            {
                baseCredits = 0;
            }

            int percent = CreditPercentForWorld(WorldCatalog.NumberForWave(waveIndex));
            int scaled = baseCredits * percent / 100;
            if (WaveModifier.IsElite(waveIndex))
            {
                scaled = scaled * (100 + WaveModifier.EliteCreditPercent) / 100;
            }

            return scaled;
        }

        public static int ApplyPercent(int value, int percent)
        {
            if (value < 1)
            {
                value = 1;
            }

            if (percent < 1)
            {
                percent = 1;
            }

            int scaled = value * percent / 100;
            if (scaled < 1)
            {
                scaled = 1;
            }

            return scaled;
        }
    }
}
