namespace AsteroidsGoneRogue.EditorTools
{
    /// <summary>
    /// PASS only when the budget elapsed, the run actually progressed, and
    /// nothing failed. An early death or a stuck opening is INCOMPLETE.
    /// Mirrors Tools/longrun_policy.py.
    /// </summary>
    public static class LongRunVerdict
    {
        public const int MinWaveIndexForPass = 2;
        public const int DefaultMinutes = 30;
        public const float HitchSeconds = 0.05f;

        public static bool Progressed(int maxWaveIndex)
        {
            return maxWaveIndex >= MinWaveIndexForPass;
        }

        public static string Classify(
            bool saveFailed,
            bool resumeFailed,
            bool harnessFault,
            bool ranFullTime,
            bool progressed,
            bool diedEarly,
            bool resumeNotRun)
        {
            if (saveFailed || resumeFailed || harnessFault)
            {
                return "FAIL";
            }

            if (!ranFullTime || !progressed || diedEarly || resumeNotRun)
            {
                return "INCOMPLETE";
            }

            return "PASS";
        }
    }
}
