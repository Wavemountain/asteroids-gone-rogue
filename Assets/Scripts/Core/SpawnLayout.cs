namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Extra ring phase for a daily seed. Seed 0 returns 0 so the unseeded
    /// spawn angles stay on the formulas in WaveManager.
    /// </summary>
    public static class SpawnLayout
    {
        public static double PhaseRadians(int seed, int wave)
        {
            if (seed == 0)
            {
                return 0d;
            }

            int waveIndex = wave < 1 ? 1 : wave;
            int mixed = DailySeed.Mix(seed, waveIndex);
            int bucket = mixed & 4095;
            return bucket * (6.283185307179586d / 4096d);
        }
    }
}
