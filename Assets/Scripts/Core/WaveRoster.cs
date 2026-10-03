using System.Collections.Generic;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Each layout world weights its own kinds in front of the ladder.
    /// Wave 5 stays on the ladder so the guardian is not stacked with extra spice.
    /// Later waves also rotate the base list. Counts stay at or under <see cref="MaxCount"/>.
    /// </summary>
    public static class WaveRoster
    {
        public const int MaxCount = 8;

        /// <summary>
        /// Elite waves that are also world finales (10, 15, 20, …).
        /// Hostile actors exclude rocks. Overflow is cut from the roster tail.
        /// </summary>
        public const int EliteBossHostileCap = 14;

        public static EnemyKind[] Extend(EnemyKind[] baseRoster, int waveIndex)
        {
            int wave = waveIndex < 1 ? 1 : waveIndex;
            EnemyKind[] source = CopyOrSeed(baseRoster);
            // Wave 5 keeps the ladder. The guardian replaces the old Brute at spawn.
            if (wave == BossRules.FirstWave)
            {
                return source;
            }

            int layoutWorld = WorldCatalog.LayoutForWave(wave);
            var built = new List<EnemyKind>();
            EnemyKind[] emphasis = WorldRules.Emphasis(layoutWorld);
            for (int spice = 0; spice < emphasis.Length; spice++)
            {
                built.Add(emphasis[spice]);
            }

            if (wave > 10 && WaveModifier.ForWave(wave) == WaveModifierKind.DenseSwarm)
            {
                built.Insert(0, EnemyKind.Swarm);
            }

            if (wave <= 10)
            {
                for (int step = 0; step < source.Length; step++)
                {
                    built.Add(source[step]);
                }

                return Trim(built);
            }

            int shift = (wave - 1) % source.Length;
            for (int step = 0; step < source.Length; step++)
            {
                built.Add(source[(step + shift) % source.Length]);
            }

            return Trim(built);
        }

        /// <summary>
        /// Roster the spawner walks. On an elite boss wave, drop from the tail
        /// until roster slots, curve extras, the elite brute, and the guardian
        /// stay at or under <see cref="EliteBossHostileCap"/>. Other waves
        /// keep the ladder. Deterministic: the same wave and extra count
        /// always drop the same tail.
        /// </summary>
        public static EnemyKind[] FitEliteBossRoster(EnemyKind[] roster, int waveIndex, int extraCount)
        {
            if (roster == null || roster.Length == 0)
            {
                return new EnemyKind[0];
            }

            if (!BossRules.IsBossWave(waveIndex) || !WaveModifier.IsElite(waveIndex))
            {
                return CopyRoster(roster);
            }

            int extraActors = extraCount < 0 ? 0 : extraCount;
            int included = 0;
            for (int scanIndex = 0; scanIndex < roster.Length; scanIndex++)
            {
                if (IncludeInSpawn(roster[scanIndex], waveIndex))
                {
                    included++;
                }
            }

            int reserved = 2;
            int budget = DifficultyCurve.MaxSpawnedEnemies - reserved;
            if (budget < 1)
            {
                budget = 1;
            }

            int rosterSlots = included;
            if (rosterSlots > budget)
            {
                rosterSlots = budget;
            }

            int room = budget - rosterSlots;
            if (room < 0)
            {
                room = 0;
            }

            int extraSlots = extraActors;
            if (extraSlots > room)
            {
                extraSlots = room;
            }

            int totalActors = rosterSlots + extraSlots + reserved;
            int overflow = totalActors - EliteBossHostileCap;
            if (overflow < 1)
            {
                return CopyRoster(roster);
            }

            int keep = rosterSlots - overflow;
            if (keep < 0)
            {
                keep = 0;
            }

            EnemyKind[] fitted = new EnemyKind[keep];
            int written = 0;
            for (int keepIndex = 0; keepIndex < roster.Length && written < keep; keepIndex++)
            {
                if (!IncludeInSpawn(roster[keepIndex], waveIndex))
                {
                    continue;
                }

                fitted[written] = roster[keepIndex];
                written++;
            }

            return fitted;
        }

        /// <summary>
        /// Wave 5's ladder Brute does not spawn beside the world guardian.
        /// </summary>
        public static bool IncludeInSpawn(EnemyKind kind, int waveIndex)
        {
            if (kind == EnemyKind.Brute && BossRules.SuppressLadderBrute(waveIndex))
            {
                return false;
            }

            return true;
        }

        private static EnemyKind[] CopyRoster(EnemyKind[] roster)
        {
            EnemyKind[] copy = new EnemyKind[roster.Length];
            for (int copyIndex = 0; copyIndex < roster.Length; copyIndex++)
            {
                copy[copyIndex] = roster[copyIndex];
            }

            return copy;
        }

        private static EnemyKind[] CopyOrSeed(EnemyKind[] baseRoster)
        {
            if (baseRoster == null || baseRoster.Length == 0)
            {
                return new[] { EnemyKind.Mid01 };
            }

            EnemyKind[] copy = new EnemyKind[baseRoster.Length];
            for (int index = 0; index < baseRoster.Length; index++)
            {
                copy[index] = baseRoster[index];
            }

            return copy;
        }

        private static EnemyKind[] Trim(List<EnemyKind> built)
        {
            if (built == null || built.Count == 0)
            {
                return new[] { EnemyKind.Mid01 };
            }

            int count = built.Count;
            if (count > MaxCount)
            {
                count = MaxCount;
            }

            EnemyKind[] trimmed = new EnemyKind[count];
            for (int index = 0; index < count; index++)
            {
                trimmed[index] = built[index];
            }

            return trimmed;
        }
    }
}
