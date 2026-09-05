namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Hangar medals. Bitmask ids so new awards can land without a schema bump.
    /// Pure C# so tests can check award copy and badge-row order without the Editor.
    /// </summary>
    public enum MedalId
    {
        ScoutWing = 1,
        DeepOrbit = 2
    }

    public static class MedalCatalog
    {
        public const string ScoutWingTitle = "Scout Wing";
        public const string DeepOrbitTitle = "Deep Orbit";
        public const int World2EntryWorld = 2;
        public const int ScoutWingClearsAtWave = 3;

        public static readonly MedalId[] All =
        {
            MedalId.ScoutWing,
            MedalId.DeepOrbit
        };

        public static string Title(MedalId id)
        {
            switch (id)
            {
                case MedalId.DeepOrbit:
                    return DeepOrbitTitle;
                default:
                    return ScoutWingTitle;
            }
        }

        public static string AwardLine(MedalId id)
        {
            return "★ " + Title(id);
        }

        public static bool TryForClearedWave(int clearedWave, out MedalId medal)
        {
            if (clearedWave == ScoutWingClearsAtWave)
            {
                medal = MedalId.ScoutWing;
                return true;
            }

            medal = MedalId.ScoutWing;
            return false;
        }

        public static bool TryForWorldEntry(int world, out MedalId medal)
        {
            if (world == World2EntryWorld)
            {
                medal = MedalId.DeepOrbit;
                return true;
            }

            medal = MedalId.ScoutWing;
            return false;
        }

        public static string WorldEntryBeat(int world)
        {
            MedalId medal;
            if (!TryForWorldEntry(world, out medal))
            {
                return string.Empty;
            }

            return AwardLine(medal);
        }

        public static string BadgeRow(int mask)
        {
            string line = string.Empty;
            for (int i = 0; i < All.Length; i++)
            {
                MedalId id = All[i];
                if ((mask & (int)id) == 0)
                {
                    continue;
                }

                if (line.Length > 0)
                {
                    line += "  ·  ";
                }

                line += AwardLine(id);
            }

            return line;
        }

        public static bool Owns(int mask, MedalId id)
        {
            return (mask & (int)id) != 0;
        }

        public static int WithAward(int mask, MedalId id)
        {
            return mask | (int)id;
        }
    }
}
