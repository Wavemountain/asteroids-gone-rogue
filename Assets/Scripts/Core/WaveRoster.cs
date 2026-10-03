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
