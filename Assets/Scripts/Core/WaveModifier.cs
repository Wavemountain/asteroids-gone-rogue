namespace AsteroidsGoneRogue
{
    public enum WaveModifierKind
    {
        None,
        FasterEnemies,
        ShieldedAsteroids,
        DenseSwarm
    }

    /// <summary>
    /// Elite waves are 10, 15, 20… (every multiple of 5 from wave 10 on).
    /// The modifier is a pure function of the wave number. Names go through Loc.
    /// </summary>
    public static class WaveModifier
    {
        public const int FirstEliteWave = 10;
        public const int EliteStride = 5;
        public const int EliteHpPercent = 140;
        public const int EliteCreditPercent = 25;
        public const int FasterPercent = 115;
        public const int ShieldedLargeBonus = 2;
        public const int ShieldedSmallBonus = 1;

        public static bool IsElite(int waveIndex)
        {
            int wave = waveIndex < 1 ? 1 : waveIndex;
            return wave >= FirstEliteWave && (wave % EliteStride) == 0;
        }

        public static WaveModifierKind ForWave(int waveIndex)
        {
            if (!IsElite(waveIndex))
            {
                return WaveModifierKind.None;
            }

            int slot = ((waveIndex / EliteStride) - (FirstEliteWave / EliteStride)) % 3;
            if (slot < 0)
            {
                slot += 3;
            }

            switch (slot)
            {
                case 0:
                    return WaveModifierKind.FasterEnemies;
                case 1:
                    return WaveModifierKind.ShieldedAsteroids;
                default:
                    return WaveModifierKind.DenseSwarm;
            }
        }

        public static string Name(WaveModifierKind kind)
        {
            switch (kind)
            {
                case WaveModifierKind.FasterEnemies:
                    return Loc.T("wave.mod.faster", "Faster enemies");
                case WaveModifierKind.ShieldedAsteroids:
                    return Loc.T("wave.mod.shielded", "Shielded asteroids");
                case WaveModifierKind.DenseSwarm:
                    return Loc.T("wave.mod.dense", "Dense swarm");
                default:
                    return string.Empty;
            }
        }

        public static string NameForWave(int waveIndex)
        {
            return Name(ForWave(waveIndex));
        }

        public static string Banner(int waveIndex)
        {
            if (!IsElite(waveIndex))
            {
                return string.Empty;
            }

            return Loc.Tf("wave.elite.banner", "ELITE WAVE - {0}", NameForWave(waveIndex));
        }
    }
}
