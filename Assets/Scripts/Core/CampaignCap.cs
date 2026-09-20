namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Steam-slice content cap: one finished World 1 arena loop (waves 1–5, Brute),
    /// then a clear win. Worlds 2–7 stay in code for later slices; this run does
    /// not wrap endlessly. Pure C# so tests can lock the cap without Play Mode.
    /// </summary>
    public static class CampaignCap
    {
        public const int FinalWave = 5;
        public const int FinalWorld = 1;
        public const string WinTitle = "SECTOR CLEAR";

        public static bool IsFinalWave(int waveIndex)
        {
            return waveIndex == FinalWave;
        }

        public static bool IsWon(int lastResolvedWave, GamePhase phase)
        {
            return phase == GamePhase.CampaignClear && lastResolvedWave >= FinalWave;
        }

        public static string WinLine()
        {
            return Loc.Tf(
                "run.sector_clear",
                WinTitle + "  ·  WORLD {0}",
                FinalWorld);
        }

        public static string HangarWinHint()
        {
            return Loc.T(
                "run.sector_hangar",
                "World 1 complete  ·  New Run from the hangar.");
        }
    }
}
