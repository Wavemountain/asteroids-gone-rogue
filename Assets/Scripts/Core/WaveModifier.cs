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
    /// Elite waves are 8, 13, 18, 23, 28, 33… (FirstEliteWave, then every
    /// EliteStride). World finales stay on multiples of 5, so this cadence
    /// does not land on a boss wave. The modifier is a pure function of the
    /// wave number. Names go through Loc.
    /// </summary>
    public static class WaveModifier
    {
        public const int FirstEliteWave = 8;
        public const int EliteStride = 5;
        public const int EliteHpPercent = 140;
        public const int EliteCreditPercent = 25;
        public const int FasterPercent = 115;
        public const int ShieldedLargeBonus = 2;
        public const int ShieldedSmallBonus = 1;

        public static bool IsElite(int waveIndex)
        {
            int wave = waveIndex < 1 ? 1 : waveIndex;
            if (wave < FirstEliteWave)
            {
                return false;
            }

            return ((wave - FirstEliteWave) % EliteStride) == 0;
        }

        public static WaveModifierKind ForWave(int waveIndex)
        {
            if (!IsElite(waveIndex))
            {
                return WaveModifierKind.None;
            }

            int slot = ((waveIndex - FirstEliteWave) / EliteStride) % 3;
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
