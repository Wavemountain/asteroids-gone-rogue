namespace AsteroidsGoneRogue
{
    /// <summary>
    /// One hazard per layout world. Unity-free. Loops reuse the layout world's rule.
    /// </summary>
    public enum WorldRuleKind
    {
        MorePickups = 1,
        FasterAsteroids = 2,
        FasterEnemyFire = 3,
        DebrisDensity = 4,
        FewerPickups = 5,
        DimVisibility = 6,
        HeavyEnemyFire = 7
    }

    /// <summary>
    /// Floor and sky colours for one layout. Channels are 0–1 before brightness.
    /// </summary>
    public struct WorldTint
    {
        public float StarR;
        public float StarG;
        public float StarB;
        public float NebulaR;
        public float NebulaG;
        public float NebulaB;
        public float GridR;
        public float GridG;
        public float GridB;
        public float FloorR;
        public float FloorG;
        public float FloorB;
        public float Brightness;
        public float Chroma;
        public int PurpleNebula;
    }

    /// <summary>
    /// Per-world enemy emphasis, one hazard, and a floor/sky palette.
    /// Worlds 8+ use the layout of worlds 1–7 with a small loop brightness step.
    /// No Unity types.
    /// </summary>
    public static class WorldRules
    {
        public const int FasterAsteroidPercent = 130;
        public const int EnemyFirePercent = 112;
        public const int HeavyFirePercent = 120;
        public const int DebrisExtraAsteroids = 2;
        public const float LoopBrightnessStep = 0.06f;
        public const float MaxBrightness = 1.25f;
        public const int PaletteStride = 15;

        // starR starG starB nebulaR nebulaG nebulaB gridR gridG gridB floorR floorG floorB brightness purple chroma
        // Values are milli (0–1000). World 6 chroma is higher (dim, stronger tint).
        public static readonly int[] PaletteMilli =
        {
            350, 620, 1000, 120, 280, 900, 200, 550, 1000, 40, 140, 420, 1000, 0, 550,
            620, 280, 980, 420, 120, 720, 520, 220, 860, 200, 40, 340, 860, 1, 550,
            100, 980, 780, 40, 620, 500, 80, 860, 680, 20, 300, 220, 1000, 0, 550,
            1000, 700, 120, 820, 400, 80, 900, 560, 140, 420, 220, 20, 940, 1, 550,
            280, 400, 620, 140, 200, 420, 180, 280, 520, 80, 100, 240, 740, 0, 550,
            900, 220, 100, 620, 120, 60, 780, 200, 100, 340, 60, 20, 550, 1, 850,
            1000, 180, 620, 720, 80, 420, 880, 160, 520, 380, 40, 220, 900, 1, 550
        };

        private static readonly EnemyKind[] Emphasis1 =
        {
            EnemyKind.Scout, EnemyKind.Mid01
        };

        private static readonly EnemyKind[] Emphasis2 =
        {
            EnemyKind.Scout, EnemyKind.Drone, EnemyKind.Swarm
        };

        private static readonly EnemyKind[] Emphasis3 =
        {
            EnemyKind.Gunner, EnemyKind.Gunner
        };

        private static readonly EnemyKind[] Emphasis4 =
        {
            EnemyKind.Bomber, EnemyKind.Bomber
        };

        private static readonly EnemyKind[] Emphasis5 =
        {
            EnemyKind.Sniper, EnemyKind.Sniper
        };

        private static readonly EnemyKind[] Emphasis6 =
        {
            EnemyKind.SwarmPod, EnemyKind.Swarm, EnemyKind.SwarmPod
        };

        private static readonly EnemyKind[] Emphasis7 =
        {
            EnemyKind.Gunner, EnemyKind.Bomber, EnemyKind.Sniper, EnemyKind.SwarmPod,
            EnemyKind.Scout, EnemyKind.Brute, EnemyKind.Swarm
        };

        public static int LayoutCount
        {
            get { return WorldCatalog.Count; }
        }

        public static WorldRuleKind KindForLayout(int layoutWorld)
        {
            switch (WorldCatalog.LayoutNumber(layoutWorld))
            {
                case 2:
                    return WorldRuleKind.FasterAsteroids;
                case 3:
                    return WorldRuleKind.FasterEnemyFire;
                case 4:
                    return WorldRuleKind.DebrisDensity;
                case 5:
                    return WorldRuleKind.FewerPickups;
                case 6:
                    return WorldRuleKind.DimVisibility;
                case 7:
                    return WorldRuleKind.HeavyEnemyFire;
                default:
                    return WorldRuleKind.MorePickups;
            }
        }

        public static WorldRuleKind KindForWave(int waveIndex)
        {
            return KindForLayout(WorldCatalog.LayoutForWave(waveIndex));
        }

        public static WorldRuleKind KindForWorld(int worldNumber)
        {
            return KindForLayout(worldNumber);
        }

        /// <summary>
        /// Kinds weighted to the front of the wave roster. World 2 is scouts,
        /// drones, and swarm. World 7 is the full mix. Mines and gates stay
        /// on the existing layout, not as extra enemy kinds.
        /// </summary>
        public static EnemyKind[] Emphasis(int layoutWorld)
        {
            switch (WorldCatalog.LayoutNumber(layoutWorld))
            {
                case 2:
                    return Emphasis2;
                case 3:
                    return Emphasis3;
                case 4:
                    return Emphasis4;
                case 5:
                    return Emphasis5;
                case 6:
                    return Emphasis6;
                case 7:
                    return Emphasis7;
                default:
                    return Emphasis1;
            }
        }

        public static string ShortLine(int worldNumber)
        {
            switch (KindForWorld(worldNumber))
            {
                case WorldRuleKind.FasterAsteroids:
                    return Loc.T("world.rule.2", "Faster asteroids");
                case WorldRuleKind.FasterEnemyFire:
                    return Loc.T("world.rule.3", "Faster enemy fire");
                case WorldRuleKind.DebrisDensity:
                    return Loc.T("world.rule.4", "Denser debris");
                case WorldRuleKind.FewerPickups:
                    return Loc.T("world.rule.5", "Fewer pickups");
                case WorldRuleKind.DimVisibility:
                    return Loc.T("world.rule.6", "Dim visibility");
                case WorldRuleKind.HeavyEnemyFire:
                    return Loc.T("world.rule.7", "Heavy enemy fire");
                default:
                    return Loc.T("world.rule.1", "Extra pickup");
            }
        }

        public static string BannerLine(int worldNumber)
        {
            string rule = ShortLine(worldNumber);
            if (string.IsNullOrEmpty(rule))
            {
                return string.Empty;
            }

            return Loc.Tf("world.rule", "World rule: {0}", rule);
        }

        public static string ShortLineForWave(int waveIndex)
        {
            return ShortLine(WorldCatalog.NumberForWave(waveIndex));
        }

        public static string BannerLineForWave(int waveIndex)
        {
            return BannerLine(WorldCatalog.NumberForWave(waveIndex));
        }

        public static int AsteroidSpeedPercent(int waveIndex)
        {
            if (KindForWave(waveIndex) == WorldRuleKind.FasterAsteroids)
            {
                return FasterAsteroidPercent;
            }

            return 100;
        }

        public static int ExtraAsteroids(int waveIndex)
        {
            if (KindForWave(waveIndex) == WorldRuleKind.DebrisDensity)
            {
                return DebrisExtraAsteroids;
            }

            return 0;
        }

        /// <summary>+1 extra pickup, -1 skip the wave pickup, 0 leave the ladder.</summary>
        public static int PickupDelta(int waveIndex)
        {
            WorldRuleKind kind = KindForWave(waveIndex);
            if (kind == WorldRuleKind.MorePickups)
            {
                return 1;
            }

            if (kind == WorldRuleKind.FewerPickups)
            {
                return -1;
            }

            return 0;
        }

        public static int EnemyFirePercentFor(int waveIndex)
        {
            WorldRuleKind kind = KindForWave(waveIndex);
            if (kind == WorldRuleKind.HeavyEnemyFire)
            {
                return HeavyFirePercent;
            }

            if (kind == WorldRuleKind.FasterEnemyFire)
            {
                return EnemyFirePercent;
            }

            return 100;
        }

        public static WorldTint TintForLayout(int layoutWorld)
        {
            int layout = WorldCatalog.LayoutNumber(layoutWorld);
            int row = (layout - 1) * PaletteStride;
            WorldTint tint = new WorldTint();
            tint.StarR = Milli(row + 0);
            tint.StarG = Milli(row + 1);
            tint.StarB = Milli(row + 2);
            tint.NebulaR = Milli(row + 3);
            tint.NebulaG = Milli(row + 4);
            tint.NebulaB = Milli(row + 5);
            tint.GridR = Milli(row + 6);
            tint.GridG = Milli(row + 7);
            tint.GridB = Milli(row + 8);
            tint.FloorR = Milli(row + 9);
            tint.FloorG = Milli(row + 10);
            tint.FloorB = Milli(row + 11);
            tint.Brightness = Milli(row + 12);
            tint.PurpleNebula = PaletteMilli[row + 13] > 0 ? 1 : 0;
            tint.Chroma = Milli(row + 14);
            return tint;
        }

        public static WorldTint TintForWorld(int worldNumber)
        {
            WorldTint tint = TintForLayout(worldNumber);
            int loop = WorldCatalog.LoopForNumber(worldNumber);
            if (loop > WorldCatalog.FirstLoop)
            {
                tint.Brightness += (loop - WorldCatalog.FirstLoop) * LoopBrightnessStep;
            }

            if (tint.Brightness < 0.05f)
            {
                tint.Brightness = 0.05f;
            }

            if (tint.Brightness > MaxBrightness)
            {
                tint.Brightness = MaxBrightness;
            }

            return tint;
        }

        public static WorldTint TintForWave(int waveIndex)
        {
            return TintForWorld(WorldCatalog.NumberForWave(waveIndex));
        }

        /// <summary>
        /// Soften toward gray the same way ArenaEnv does, then compare.
        /// </summary>
        public static void Soften(float red, float green, float blue, float chroma, out float outR, out float outG, out float outB)
        {
            float mix = chroma;
            if (mix < 0f)
            {
                mix = 0f;
            }

            if (mix > 1f)
            {
                mix = 1f;
            }

            float y = (0.2126f * red) + (0.7152f * green) + (0.0722f * blue);
            outR = y + ((red - y) * mix);
            outG = y + ((green - y) * mix);
            outB = y + ((blue - y) * mix);
        }

        public static float ChannelDistance(
            float ar,
            float ag,
            float ab,
            float br,
            float bg,
            float bb)
        {
            float dr = ar - br;
            float dg = ag - bg;
            float db = ab - bb;
            return Sqrt(dr * dr + dg * dg + db * db);
        }

        /// <summary>
        /// Layouts 1–7 stay apart after soften. Neighbours, including the
        /// world 7 to world 8 loop step, stay apart as well.
        /// </summary>
        public static bool PaletteDistinct(float minStar, float minFloor, float minNeighbour)
        {
            if (PaletteMilli == null || PaletteMilli.Length != LayoutCount * PaletteStride)
            {
                return false;
            }

            for (int left = 1; left <= LayoutCount; left++)
            {
                for (int right = left + 1; right <= LayoutCount; right++)
                {
                    if (StarDistance(left, right) < minStar)
                    {
                        return false;
                    }

                    if (FloorDistance(left, right) < minFloor)
                    {
                        return false;
                    }
                }
            }

            for (int world = 1; world <= LayoutCount + 1; world++)
            {
                if (StarDistance(world, world + 1) < minNeighbour)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool RulesCoverLayouts()
        {
            bool[] seen = new bool[LayoutCount + 1];
            for (int layout = 1; layout <= LayoutCount; layout++)
            {
                int kind = (int)KindForLayout(layout);
                if (kind < 1 || kind > LayoutCount || seen[kind])
                {
                    return false;
                }

                seen[kind] = true;
                if (Emphasis(layout) == null || Emphasis(layout).Length < 1)
                {
                    return false;
                }
            }

            for (int world = 1; world <= LayoutCount + 8; world++)
            {
                if (KindForWorld(world) != KindForLayout(world))
                {
                    return false;
                }

                if (string.IsNullOrEmpty(ShortLine(world)))
                {
                    return false;
                }
            }

            return true;
        }

        private static float StarDistance(int worldA, int worldB)
        {
            WorldTint left = TintForWorld(worldA);
            WorldTint right = TintForWorld(worldB);
            float lr;
            float lg;
            float lb;
            float rr;
            float rg;
            float rb;
            Soften(Clamp01(left.StarR * left.Brightness), Clamp01(left.StarG * left.Brightness), Clamp01(left.StarB * left.Brightness), left.Chroma, out lr, out lg, out lb);
            Soften(Clamp01(right.StarR * right.Brightness), Clamp01(right.StarG * right.Brightness), Clamp01(right.StarB * right.Brightness), right.Chroma, out rr, out rg, out rb);
            return ChannelDistance(lr, lg, lb, rr, rg, rb);
        }

        private static float FloorDistance(int worldA, int worldB)
        {
            WorldTint left = TintForWorld(worldA);
            WorldTint right = TintForWorld(worldB);
            return ChannelDistance(left.FloorR, left.FloorG, left.FloorB, right.FloorR, right.FloorG, right.FloorB);
        }

        private static float Milli(int index)
        {
            return PaletteMilli[index] / 1000f;
        }

        private static float Clamp01(float value)
        {
            if (value < 0f)
            {
                return 0f;
            }

            if (value > 1f)
            {
                return 1f;
            }

            return value;
        }

        private static float Sqrt(float value)
        {
            if (value <= 0f)
            {
                return 0f;
            }

            float guess = value;
            for (int step = 0; step < 8; step++)
            {
                guess = (guess + (value / guess)) * 0.5f;
            }

            return guess;
        }
    }
}
