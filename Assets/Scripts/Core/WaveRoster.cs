using System.Collections.Generic;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Waves 1–10 stay on today's ladder. Later waves add world spice and a
    /// deterministic rotation so wave 11 is not a copy of wave 10. Counts stay
    /// at or under <see cref="MaxCount"/> and the list is never empty.
    /// </summary>
    public static class WaveRoster
    {
        public const int MaxCount = 8;

        public static EnemyKind[] Extend(EnemyKind[] baseRoster, int waveIndex)
        {
            int wave = waveIndex < 1 ? 1 : waveIndex;
            EnemyKind[] source = CopyOrSeed(baseRoster);
            if (wave <= 10)
            {
                return source;
            }

            int layoutWorld = WorldCatalog.LayoutForWave(wave);
            var built = new List<EnemyKind>();
            AppendSpice(built, layoutWorld, wave);
            if (WaveModifier.ForWave(wave) == WaveModifierKind.DenseSwarm)
            {
                built.Insert(0, EnemyKind.Swarm);
            }

            int shift = (wave - 1) % source.Length;
            for (int step = 0; step < source.Length; step++)
            {
                built.Add(source[(step + shift) % source.Length]);
            }

            return Trim(built);
        }

        private static void AppendSpice(List<EnemyKind> built, int layoutWorld, int wave)
        {
            EnemyKind[] pool = SpicePool(layoutWorld);
            if (pool.Length == 0)
            {
                return;
            }

            int spin = (wave - 1) % pool.Length;
            built.Add(pool[spin]);
            if (pool.Length > 1)
            {
                built.Add(pool[(spin + 1) % pool.Length]);
            }
        }

        /// <summary>
        /// World 3 adds Bomber, 4 Sniper, 5 SwarmPod, 6 Brute, 7 the full set.
        /// Earlier layouts add nothing, so waves 11–15 are the first change.
        /// </summary>
        private static EnemyKind[] SpicePool(int layoutWorld)
        {
            switch (layoutWorld)
            {
                case 3:
                    return new[] { EnemyKind.Bomber };
                case 4:
                    return new[] { EnemyKind.Bomber, EnemyKind.Sniper };
                case 5:
                    return new[] { EnemyKind.Bomber, EnemyKind.Sniper, EnemyKind.SwarmPod };
                case 6:
                    return new[] { EnemyKind.Bomber, EnemyKind.Sniper, EnemyKind.SwarmPod, EnemyKind.Brute };
                case 7:
                    return new[]
                    {
                        EnemyKind.Bomber, EnemyKind.Sniper, EnemyKind.SwarmPod,
                        EnemyKind.Brute, EnemyKind.Swarm, EnemyKind.Gunner
                    };
                default:
                    return new EnemyKind[0];
            }
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
