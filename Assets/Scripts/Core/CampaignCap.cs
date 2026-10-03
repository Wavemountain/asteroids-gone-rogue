namespace AsteroidsGoneRogue
{
    /// <summary>
    /// World 1 still has a sector-clear beat at wave 5 (achievement, medal, headline).
    /// That beat does not end the run: every multiple of <see cref="ArenaLayout.WavesPerLayout"/>
    /// is a world boundary and play continues. Pure C# so tests can lock the boundary
    /// without Play Mode.
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

        /// <summary>
        /// Waves 5, 10, 15, … finish a world. The run continues on the next wave.
        /// </summary>
        public static bool IsWorldBoundary(int wave)
        {
            if (wave < ArenaLayout.WavesPerLayout)
            {
                return false;
            }

            return wave % ArenaLayout.WavesPerLayout == 0;
        }

        public static int NextWorldIndex(int clearedWave)
        {
            int upcoming = clearedWave < 1 ? 1 : clearedWave + 1;
            return WorldCatalog.NumberForWave(upcoming);
        }

        public static bool IsWon(int lastResolvedWave, GamePhase phase)
        {
            return phase == GamePhase.WaveClear
                && IsWorldBoundary(lastResolvedWave)
                && lastResolvedWave >= FinalWave;
        }

        public static string WinLine()
        {
            return Loc.Tf(
                "run.sector_clear",
                WinTitle + "  ·  WORLD {0}",
                FinalWorld);
        }

        public static string SectorClearTitle(int world)
        {
            int shown = world < 1 ? 1 : world;
            return Loc.Tf("run.sector_world", "SECTOR CLEAR - World {0} complete", shown);
        }
    }
}
