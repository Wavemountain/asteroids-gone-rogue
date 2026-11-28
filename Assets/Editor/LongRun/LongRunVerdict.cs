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
        public const int WatchdogSeconds = 60;
        public const int CombatWatchdogSeconds = 90;
        public const int LogCapBytes = 20 * 1024 * 1024;
        public const int StepTimeoutSeconds = 20;
        public const int TargetMinutesWhenUnspecified = 90;

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

        /// <summary>
        /// Target-wave runs can PASS before the clock ends. A diagnostic flag
        /// is never PASS. Save, resume, and harness faults still win.
        /// </summary>
        public static string ClassifyCampaign(
            bool saveFailed,
            bool resumeFailed,
            bool harnessFault,
            bool diagnostic,
            int targetWave,
            int clearedWaves,
            bool ranFullTime,
            bool progressed,
            bool diedEarly,
            bool resumeNotRun)
        {
            if (saveFailed || resumeFailed || harnessFault)
            {
                return "FAIL";
            }

            if (diagnostic)
            {
                return "DIAGNOSTIC";
            }

            if (targetWave > 0)
            {
                if (clearedWaves >= targetWave)
                {
                    return "PASS";
                }

                return "INCOMPLETE";
            }

            return Classify(saveFailed, resumeFailed, harnessFault, ranFullTime, progressed, diedEarly, resumeNotRun);
        }
    }
}
