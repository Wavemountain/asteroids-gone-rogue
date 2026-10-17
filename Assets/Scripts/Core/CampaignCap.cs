namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Campaign final is world 7, wave 35 (Hard Clear). That beat does not end
    /// the run: every multiple of <see cref="ArenaLayout.WavesPerLayout"/> is a
    /// world boundary and play continues into the loop. World 1's medal chip and
    /// the "World 2 after wave 5" hook stay on <see cref="WorldCatalog.WavesPerWorld"/>.
    /// Pure C# so tests can lock the boundary without Play Mode.
    /// </summary>
    public static class CampaignCap
    {
        public const int FinalWave = 35;
        public const int FinalWorld = 7;
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

        /// <summary>
        /// World-boundary clear at or past the campaign final. Later loop
        /// boundaries stay won. Callers must still advance the wave: this
        /// flag does not end the run.
        /// </summary>
        public static bool IsWon(int lastResolvedWave, GamePhase phase)
        {
            return phase == GamePhase.WaveClear
                && IsWorldBoundary(lastResolvedWave)
                && lastResolvedWave >= FinalWave
                && WorldCatalog.NumberForWave(lastResolvedWave) >= FinalWorld;
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
            return Loc.Tf("run.sector_world", "SECTOR CLEAR - World {0}", shown);
        }
    }
}
