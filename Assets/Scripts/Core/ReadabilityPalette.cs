namespace AsteroidsGoneRogue
{
    /// <summary>
    /// 0.47 Part C readability numbers. Worlds 3 and 4 drop floor brightness.
    /// Generic enemy bodies take a Danger emission, Mid01 keeps its rust and
    /// is boosted, the hull gets a steel rim, and asteroids are lighter stone
    /// with a weak Secondary rim. Unity-free.
    /// </summary>
    public static class ReadabilityPalette
    {
        public const int World3BrightnessMilli = 760;
        public const int World4BrightnessMilli = 680;
        public const float WatchMidMul = 1.5f;
        public const float WatchAsteroidMul = 1.15f;
        public const float EnemyEmissionScale = 0.9f;
        public const float MidEmissionScale = 1.35f;
        public const float HullRimScale = 0.35f;
        public const float AsteroidAlbedoScale = 1.5f;
        public const float AsteroidEmissionScale = 0.15f;
        public const float EnemyBodyR = 0.5f;
        public const float EnemyBodyG = 0.3f;
        public const float EnemyBodyB = 0.32f;
        public const float MidR = 0.82f;
        public const float MidG = 0.1f;
        public const float MidB = 0.12f;
        public const float HullR = 0.45f;
        public const float HullG = 0.52f;
        public const float HullB = 0.58f;
        public const float AsteroidR = 0.38f;
        public const float AsteroidG = 0.32f;
        public const float AsteroidB = 0.28f;
        public const float AsteroidVariantR = 0.46f;
        public const float AsteroidVariantG = 0.3f;
        public const float AsteroidVariantB = 0.22f;
        public const float BruteAuraR = 1f;
        public const float BruteAuraG = 0.32f;
        public const float BruteAuraB = 0.1f;
        public const float BrutePulseBase = 1f;
        public const float BruteIdleAmp = 0.2f;
        public const float BruteChargeAmp = 0.3f;

        public static void DangerRgb(out float red, out float green, out float blue)
        {
            red = 184f / 255f;
            green = 90f / 255f;
            blue = 40f / 255f;
        }

        public static void SecondaryRgb(out float red, out float green, out float blue)
        {
            red = 106f / 255f;
            green = 168f / 255f;
            blue = 200f / 255f;
        }

        public static bool WatchWorld(int world)
        {
            return world == 3 || world == 4;
        }

        /// <summary>
        /// Mid01 emission. Worlds 3 and 4 multiply the shared scale; others stay.
        /// </summary>
        public static float MidEmissionFor(int world)
        {
            if (WatchWorld(world))
            {
                return MidEmissionScale * WatchMidMul;
            }

            return MidEmissionScale;
        }

        /// <summary>
        /// Extra asteroid albedo on worlds 3 and 4. Other worlds stay at 1.
        /// </summary>
        public static float AsteroidAlbedoMul(int world)
        {
            if (WatchWorld(world))
            {
                return WatchAsteroidMul;
            }

            return 1f;
        }
    }
}
