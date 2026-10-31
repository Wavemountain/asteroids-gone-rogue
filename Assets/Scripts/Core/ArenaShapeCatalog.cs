using System;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// One fixed star-like contour per layout world. Low harmonics stay inside
    /// an 8% radial bulge so the polygon cannot fold a corner that traps a ship.
    /// Area matches the density factors against the old 30 m disc. The contour is fixed.
    /// </summary>
    public static class ArenaShapeCatalog
    {
        public const float LegacyRadius = 30f;
        public const float Bulge = 0.08f;
        public const float FloorMargin = 20f;
        public const int WorldCount = 7;

        public static readonly float[] AreaFactors =
        {
            1.30f, 1.35f, 1.40f, 1.45f, 1.45f, 1.35f, 1.50f
        };

        static readonly ArenaShape[] Cache = new ArenaShape[WorldCount];
        static int _active = 1;

        public static void SetActive(int layoutWorld)
        {
            int world = layoutWorld;
            if (world < 1)
            {
                world = 1;
            }

            if (world > WorldCount)
            {
                world = ((world - 1) % WorldCount) + 1;
            }

            _active = world;
        }

        public static int ActiveWorld
        {
            get { return _active; }
        }

        public static ArenaShape Active
        {
            get { return ForWorld(_active); }
        }

        public static ArenaShape ForWorld(int layoutWorld)
        {
            int world = layoutWorld;
            if (world < 1)
            {
                world = 1;
            }

            if (world > WorldCount)
            {
                world = ((world - 1) % WorldCount) + 1;
            }

            int index = world - 1;
            if (Cache[index] == null)
            {
                Cache[index] = Build(world);
            }

            return Cache[index];
        }

        public static float FloorReach(ArenaShape shape)
        {
            float bound = LegacyRadius;
            if (shape != null)
            {
                bound = shape.BoundingRadius;
            }

            return bound + FloorMargin;
        }

        /// <summary>
        /// 0.48 framed world 1 at the 30 m disc. The contour floor reaches
        /// farther, so the same camera offset fills the shot with plates.
        /// </summary>
        public static float World1CameraPull
        {
            get
            {
                float reach = FloorReach(ForWorld(1));
                if (reach < 1f)
                {
                    return 1f;
                }

                return reach / LegacyRadius;
            }
        }

        public static bool SelfCheck()
        {
            float circle = 3.14159265f * LegacyRadius * LegacyRadius;
            for (int world = 1; world <= WorldCount; world++)
            {
                ArenaShape shape = ForWorld(world);
                if (shape.PointCount < 32 || shape.PointCount > 64)
                {
                    return false;
                }

                if (shape.MinRadius < 31f)
                {
                    return false;
                }

                if (!shape.IsConvex() || !shape.Contains(0f, 0f))
                {
                    return false;
                }

                float bulge = (shape.BoundingRadius - shape.MinRadius)
                    / (shape.BoundingRadius + shape.MinRadius);
                if (bulge > 0.081f)
                {
                    return false;
                }

                float target = AreaFactors[world - 1] * circle;
                float ratio = shape.Area / target;
                if (ratio < 0.98f || ratio > 1.02f)
                {
                    return false;
                }

                float radius = shape.RadiusAt(0f);
                if (!shape.Contains(radius * 0.5f, 0f) || shape.Contains(radius + 1.5f, 0f))
                {
                    return false;
                }

                float nx;
                float nz;
                shape.EdgeNormal(radius - 0.2f, 0f, out nx, out nz);
                if (nx < 0.35f)
                {
                    return false;
                }

                shape.EdgeNormal(radius + 2f, 0f, out nx, out nz);
                if (nx < 0.35f)
                {
                    return false;
                }
            }

            return ArenaEdge.NoTrap();
        }

        static ArenaShape Build(int world)
        {
            float bulgeScale = 1f;
            if (world == 7)
            {
                bulgeScale = 0.55f;
            }

            float[] raw = new float[ArenaShape.SampleCount];
            float peak = 0f;
            float step = 6.28318530718f / ArenaShape.SampleCount;
            for (int i = 0; i < ArenaShape.SampleCount; i++)
            {
                float theta = i * step;
                float harmonic = Harmonic(world, theta);
                raw[i] = harmonic;
                float abs = harmonic < 0f ? -harmonic : harmonic;
                if (abs > peak)
                {
                    peak = abs;
                }
            }

            if (peak < 0.000001f)
            {
                peak = 1f;
            }

            float[] radii = new float[ArenaShape.SampleCount];
            for (int i = 0; i < ArenaShape.SampleCount; i++)
            {
                radii[i] = 1f + ((raw[i] / peak) * Bulge * bulgeScale);
            }

            ArenaShape unit = ArenaShape.FromSamples(radii);
            float target = AreaFactors[world - 1] * 3.14159265f * LegacyRadius * LegacyRadius;
            float area = unit.Area;
            if (area < 0.0001f)
            {
                area = 0.0001f;
            }

            float scale = (float)Math.Sqrt(target / area);
            for (int i = 0; i < ArenaShape.SampleCount; i++)
            {
                radii[i] *= scale;
            }

            return ArenaShape.FromSamples(radii);
        }

        static float Harmonic(int world, float theta)
        {
            if (world == 2)
            {
                return (0.72f * (float)Math.Cos(theta))
                    + (0.38f * (float)Math.Cos((2f * theta) + 1.2f));
            }

            if (world == 3)
            {
                return (float)Math.Cos((2f * theta) + 0.55f);
            }

            if (world == 4)
            {
                return (float)Math.Cos((2f * theta) + 1.15f);
            }

            if (world == 5)
            {
                return (0.85f * (float)Math.Cos((2f * theta) + 2.4f))
                    + (0.25f * (float)Math.Cos((3f * theta) + 0.4f));
            }

            if (world == 6)
            {
                return (0.75f * (float)Math.Cos(theta + 1.57f))
                    + (0.35f * (float)Math.Cos((3f * theta) + 0.2f));
            }

            if (world == 7)
            {
                return (float)Math.Cos(4f * theta);
            }

            return (float)Math.Cos(2f * theta);
        }
    }
}
