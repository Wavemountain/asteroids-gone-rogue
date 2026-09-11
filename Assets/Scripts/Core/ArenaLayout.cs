namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Arena shape / hazard set that swaps with the world floor every 5 waves.
    /// Unity-free so tests can check titles and the wave→layout map.
    /// </summary>
    public enum ArenaLayoutId
    {
        Open = 0,
        PylonRing = 1,
        SplitTrench = 2,
        MineBelt = 3,
        CrossGates = 4,
        DebrisIslands = 5,
        SpokeRing = 6
    }

    public static class ArenaLayout
    {
        public const int WavesPerLayout = 5;
        public const int LayoutCount = 7;

        public static int WorldIndexForWave(int waveIndex)
        {
            int wave = waveIndex < 1 ? 1 : waveIndex;
            return ((wave - 1) / WavesPerLayout % LayoutCount) + 1;
        }

        public static ArenaLayoutId ForWave(int waveIndex)
        {
            return ForWorld(WorldIndexForWave(waveIndex));
        }

        public static ArenaLayoutId ForWorld(int world)
        {
            int index = world - 1;
            if (index < 0)
            {
                index = 0;
            }

            index %= LayoutCount;
            if (index < 0)
            {
                index += LayoutCount;
            }

            return (ArenaLayoutId)index;
        }

        public static string Title(ArenaLayoutId id)
        {
            switch (id)
            {
                case ArenaLayoutId.PylonRing:
                    return Loc.T("layout.pylon", "Pylon ring");
                case ArenaLayoutId.SplitTrench:
                    return Loc.T("layout.trench", "Split trench");
                case ArenaLayoutId.MineBelt:
                    return Loc.T("layout.mines", "Mine belt");
                case ArenaLayoutId.CrossGates:
                    return Loc.T("layout.cross", "Cross gates");
                case ArenaLayoutId.DebrisIslands:
                    return Loc.T("layout.islands", "Debris islands");
                case ArenaLayoutId.SpokeRing:
                    return Loc.T("layout.spokes", "Spoke ring");
                default:
                    return Loc.T("layout.open", "Open");
            }
        }

        public static string Badge(ArenaLayoutId id)
        {
            switch (id)
            {
                case ArenaLayoutId.PylonRing:
                    return Loc.T("badge.pylons", "PYLONS");
                case ArenaLayoutId.SplitTrench:
                    return Loc.T("badge.trench", "TRENCH");
                case ArenaLayoutId.MineBelt:
                    return Loc.T("badge.mines", "MINES");
                case ArenaLayoutId.CrossGates:
                    return Loc.T("badge.cross", "CROSS");
                case ArenaLayoutId.DebrisIslands:
                    return Loc.T("badge.islands", "ISLANDS");
                case ArenaLayoutId.SpokeRing:
                    return Loc.T("badge.spokes", "SPOKES");
                default:
                    return Loc.T("badge.open", "OPEN");
            }
        }

        public static bool UsesHazardSpikes(ArenaLayoutId id)
        {
            return id == ArenaLayoutId.PylonRing
                || id == ArenaLayoutId.MineBelt
                || id == ArenaLayoutId.DebrisIslands
                || id == ArenaLayoutId.CrossGates
                || id == ArenaLayoutId.SpokeRing;
        }
    }
}
