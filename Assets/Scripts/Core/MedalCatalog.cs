namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Hangar medals. Bitmask ids so new awards can land without a schema bump.
    /// Pure C# so tests can check award copy and badge-row order without the Editor.
    /// Badge row capacity is three medals (Scout Wing, Deep Orbit, Far Drift).
    /// </summary>
    public enum MedalId
    {
        ScoutWing = 1,
        DeepOrbit = 2,
        FarDrift = 4
    }

    public static class MedalCatalog
    {
        public const string ScoutWingTitle = "Scout Wing";
        public const string DeepOrbitTitle = "Deep Orbit";
        public const string FarDriftTitle = "Far Drift";
        public const int World2EntryWorld = 2;
        public const int World3EntryWorld = 3;
        public const int ScoutWingClearsAtWave = 3;
        public const int FarDriftClearsAtWave = 10;
        public const int BadgeCapacity = 3;

        public static readonly MedalId[] All =
        {
            MedalId.ScoutWing,
            MedalId.DeepOrbit,
            MedalId.FarDrift
        };

        public static string Title(MedalId id)
        {
            switch (id)
            {
                case MedalId.DeepOrbit:
                    return DeepOrbitTitle;
                case MedalId.FarDrift:
                    return FarDriftTitle;
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

            if (clearedWave == FarDriftClearsAtWave)
            {
                medal = MedalId.FarDrift;
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

            if (world == World3EntryWorld)
            {
                medal = MedalId.FarDrift;
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
            int shown = 0;
            for (int i = 0; i < All.Length && shown < BadgeCapacity; i++)
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
                shown++;
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
