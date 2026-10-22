using System;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Enemies spawn on the outside ring and fly in. The angle nudge is geometry
    /// only. Nest children do not use this.
    /// </summary>
    public static class EntryFlight
    {
        public const float RingClearance = 1.5f;
        public const float ShipEdgeMetres = 6f;
        public const float NudgeRadians = 0.78539816f;
        public const float GraceSeconds = 0.3f;
        public const float InsideSnap = 0.4f;

        public static float Seconds(EnemyKind kind, bool elite, bool boss)
        {
            if (boss)
            {
                return 1.5f;
            }

            if (elite || kind == EnemyKind.Brute || kind == EnemyKind.Swarm)
            {
                return 1.0f;
            }

            if (kind == EnemyKind.Gunner || kind == EnemyKind.Sniper || kind == EnemyKind.Bomber)
            {
                return 0.8f;
            }

            if (kind == EnemyKind.Mid01)
            {
                return 0.6f;
            }

            return 0.5f;
        }

        public static float NudgeAngle(
            ArenaShape shape,
            float angle,
            float playerX,
            float playerZ,
            float bodyRadius)
        {
            float nudged = angle;
            if (shape == null)
            {
                return nudged;
            }

            float shipEdge = shape.DistanceToEdge(playerX, playerZ);
            if (shipEdge <= ShipEdgeMetres)
            {
                float shipAngle = (float)Math.Atan2(playerZ, playerX);
                float delta = Wrap(nudged - shipAngle);
                float abs = delta < 0f ? -delta : delta;
                if (abs < NudgeRadians)
                {
                    if (delta >= 0f)
                    {
                        nudged = shipAngle + NudgeRadians;
                    }
                    else
                    {
                        nudged = shipAngle - NudgeRadians;
                    }
                }
            }

            float margin = bodyRadius + RingClearance;
            float minSqr = SpawnClearance.EntryMinSafeMetres * SpawnClearance.EntryMinSafeMetres;
            for (int step = 0; step < 8; step++)
            {
                float radius = shape.RadiusAt(nudged) + margin;
                float x = (float)Math.Cos(nudged) * radius;
                float z = (float)Math.Sin(nudged) * radius;
                float dx = x - playerX;
                float dz = z - playerZ;
                if ((dx * dx) + (dz * dz) >= minSqr)
                {
                    return nudged;
                }

                nudged += NudgeRadians;
            }

            return nudged;
        }

        public static void Resolve(
            ArenaShape shape,
            float angle,
            float bodyRadius,
            float playerX,
            float playerZ,
            out float outsideX,
            out float outsideZ,
            out float insideX,
            out float insideZ,
            out float crossX,
            out float crossZ)
        {
            float nudged = NudgeAngle(shape, angle, playerX, playerZ, bodyRadius);
            float margin = bodyRadius + RingClearance;
            shape.SpawnRing(nudged, margin, out outsideX, out outsideZ);
            shape.SpawnRing(nudged, -InsideSnap, out insideX, out insideZ);
            shape.SpawnRing(nudged, 0f, out crossX, out crossZ);
        }

        static float Wrap(float radians)
        {
            const float tau = 6.28318530718f;
            float wrapped = radians;
            while (wrapped > 3.14159265f)
            {
                wrapped -= tau;
            }

            while (wrapped < -3.14159265f)
            {
                wrapped += tau;
            }

            return wrapped;
        }
    }
}
