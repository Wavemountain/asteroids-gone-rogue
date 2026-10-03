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
        FarDrift = 4,
        MineFields = 8,
        CrossGates = 16,
        DebrisIslands = 32,
        SpokeRing = 64
    }

    public static class MedalCatalog
    {
        public const string ScoutWingTitle = "Scout Wing";
        public const string DeepOrbitTitle = "Deep Orbit";
        public const string FarDriftTitle = "Far Drift";
        public const string World3EntryTitle = "New sector";
        public const int World2EntryWorld = 2;
        public const int World3EntryWorld = 3;
        public const int ScoutWingClearsAtWave = 3;
        public const int FarDriftClearsAtWave = 10;
        public const int MineFieldsClearsAtWave = 20;
        public const int CrossGatesClearsAtWave = 25;
        public const int DebrisIslandsClearsAtWave = 30;
        public const int SpokeRingClearsAtWave = 35;
        public const int BadgeCapacity = 3;
        public const float World2FlashSeconds = 1.55f;
        public const float World3FlashSeconds = 2.15f;

        public static readonly MedalId[] All =
        {
            MedalId.ScoutWing,
            MedalId.DeepOrbit,
            MedalId.FarDrift,
            MedalId.MineFields,
            MedalId.CrossGates,
            MedalId.DebrisIslands,
            MedalId.SpokeRing
        };

        public static string Title(MedalId id)
        {
            switch (id)
            {
                case MedalId.DeepOrbit:
                    return Loc.T("medal.deep", DeepOrbitTitle);
                case MedalId.FarDrift:
                    return Loc.T("medal.far", FarDriftTitle);
                case MedalId.MineFields:
                    return WorldCatalog.Name(4);
                case MedalId.CrossGates:
                    return WorldCatalog.Name(5);
                case MedalId.DebrisIslands:
                    return WorldCatalog.Name(6);
                case MedalId.SpokeRing:
                    return WorldCatalog.Name(7);
                default:
                    return Loc.T("medal.scout", ScoutWingTitle);
            }
        }

        public static string AwardLine(MedalId id)
        {
            return UiGlyph.Medal + Title(id);
        }

        public static string LockedLine(MedalId id)
        {
            return UiGlyph.Locked + Title(id);
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

            if (clearedWave == MineFieldsClearsAtWave)
            {
                medal = MedalId.MineFields;
                return true;
            }

            if (clearedWave == CrossGatesClearsAtWave)
            {
                medal = MedalId.CrossGates;
                return true;
            }

            if (clearedWave == DebrisIslandsClearsAtWave)
            {
                medal = MedalId.DebrisIslands;
                return true;
            }

            if (clearedWave == SpokeRingClearsAtWave)
            {
                medal = MedalId.SpokeRing;
                return true;
            }

            medal = MedalId.ScoutWing;
            return false;
        }

        /// <summary>
        /// World 2 entry (wave 6) awards Deep Orbit. Wave 10 clear awards Far Drift.
        /// Wave 11 is the World 3 entry beat and is not a medal.
        /// </summary>
        public static bool AwardsOnWave(int wave, out MedalId medal)
        {
            if (wave == 6)
            {
                return TryForWorldEntry(World2EntryWorld, out medal);
            }

            return TryForClearedWave(wave, out medal);
        }

        public static string WorldClearBoard(int mask)
        {
            string lineA = BoardMark(mask, MedalId.MineFields)
                + "  ·  "
                + BoardMark(mask, MedalId.CrossGates);
            string lineB = BoardMark(mask, MedalId.DebrisIslands)
                + "  ·  "
                + BoardMark(mask, MedalId.SpokeRing);
            return lineA + "\n" + lineB;
        }

        private static string BoardMark(int mask, MedalId id)
        {
            return Owns(mask, id) ? AwardLine(id) : LockedLine(id);
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
            if (TryForWorldEntry(world, out medal))
            {
                return AwardLine(medal);
            }

            if (world == World3EntryWorld)
            {
                return Loc.T("medal.world3", World3EntryTitle);
            }

            return string.Empty;
        }

        public static string World3HangarLine()
        {
            return Loc.Tf("run.world3_online", "World 3 online  ·  {0}", Loc.T("medal.world3", World3EntryTitle));
        }

        public static float WorldEntryFlashSeconds(int world)
        {
            return world == World3EntryWorld ? World3FlashSeconds : World2FlashSeconds;
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

        /// <summary>
        /// Always-visible three-rung ladder (earned ★ / locked ○) so the next medal is obvious.
        /// World 3 entry is a New sector beat with no medal — Far Drift awards on wave 10 clear.
        /// </summary>
        public static string LadderLine(int mask)
        {
            string line = string.Empty;
            int shown = 0;
            for (int i = 0; i < All.Length && shown < BadgeCapacity; i++)
            {
                MedalId id = All[i];
                if (line.Length > 0)
                {
                    line += "  ·  ";
                }

                line += Owns(mask, id) ? AwardLine(id) : LockedLine(id);
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
