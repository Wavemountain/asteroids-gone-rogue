using System;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Soft push, hard inside limit, asteroid reflect, and the steel lip alpha.
    /// Unity-free. Gameplay calls this instead of wrapping around the old disc.
    /// </summary>
    public static class ArenaEdge
    {
        public const float SoftBand = 2f;
        public const float MaxAccel = 32f;
        public const float EnemyAccel = 24f;
        public const float HardInside = 0.3f;
        public const float AsteroidDamping = 0.8f;
        public const float AsteroidInset = 0.05f;
        public const float WarningBand = 3f;
        public const float RestAlpha = 0.12f;
        public const float NearAlpha = 0.75f;
        public const float NearMetres = 6f;
        public const float FullMetres = 2f;
        public const float FadeSeconds = 0.3f;
        public const float ReduceAlpha = 0.45f;
        public const float BounceInside = 0.8f;
        public const float BounceBand = 0.65f;

        public static bool Push(
            ArenaShape shape,
            float x,
            float z,
            float vx,
            float vz,
            float accelCap,
            float dt,
            out float ox,
            out float oz,
            out float ovx,
            out float ovz)
        {
            ox = x;
            oz = z;
            ovx = vx;
            ovz = vz;
            if (shape == null)
            {
                return false;
            }

            if (dt < 0f)
            {
                dt = 0f;
            }

            float distance = shape.DistanceToEdge(x, z);
            float nx;
            float nz;
            shape.EdgeNormal(x, z, out nx, out nz);
            if (distance < HardInside)
            {
                float cx;
                float cz;
                shape.ClosestPoint(x, z, out cx, out cz);
                ox = cx - (nx * HardInside);
                oz = cz - (nz * HardInside);
                float outward = (vx * nx) + (vz * nz);
                if (outward > 0f)
                {
                    ovx = vx - (outward * nx);
                    ovz = vz - (outward * nz);
                }

                return true;
            }

            if (distance < SoftBand)
            {
                float span = SoftBand;
                if (span < 0.001f)
                {
                    span = 0.001f;
                }

                float along = 1f - (distance / span);
                if (along < 0f)
                {
                    along = 0f;
                }

                if (along > 1f)
                {
                    along = 1f;
                }

                float accel = accelCap * along;
                ovx = vx - (nx * accel * dt);
                ovz = vz - (nz * accel * dt);
                float outward = (ovx * nx) + (ovz * nz);
                if (outward > 0f)
                {
                    ovx -= outward * nx;
                    ovz -= outward * nz;
                }
            }

            return false;
        }

        public static bool TryReflect(
            ArenaShape shape,
            float x,
            float z,
            float vx,
            float vz,
            out float ox,
            out float oz,
            out float ovx,
            out float ovz)
        {
            ox = x;
            oz = z;
            ovx = vx;
            ovz = vz;
            if (shape == null)
            {
                return false;
            }

            float distance = shape.DistanceToEdge(x, z);
            if (distance >= AsteroidInset)
            {
                return false;
            }

            float nx;
            float nz;
            shape.EdgeNormal(x, z, out nx, out nz);
            float outward = (vx * nx) + (vz * nz);
            if (outward > 0f)
            {
                ovx = (vx - (2f * outward * nx)) * AsteroidDamping;
                ovz = (vz - (2f * outward * nz)) * AsteroidDamping;
            }

            float cx;
            float cz;
            shape.ClosestPoint(x, z, out cx, out cz);
            ox = cx - (nx * AsteroidInset);
            oz = cz - (nz * AsteroidInset);
            return true;
        }

        public static float LineAlpha(float distanceToEdge, float previous, float dt, bool reduce)
        {
            if (reduce)
            {
                return ReduceAlpha;
            }

            float target = RestAlpha;
            if (distanceToEdge <= FullMetres)
            {
                target = NearAlpha;
            }
            else if (distanceToEdge < NearMetres)
            {
                float span = NearMetres - FullMetres;
                float along = (NearMetres - distanceToEdge) / span;
                target = RestAlpha + ((NearAlpha - RestAlpha) * along);
            }

            if (dt < 0f)
            {
                dt = 0f;
            }

            float maxDelta = 0f;
            if (FadeSeconds > 0.001f)
            {
                maxDelta = (NearAlpha - RestAlpha) * (dt / FadeSeconds);
            }

            float delta = target - previous;
            if (delta > maxDelta)
            {
                delta = maxDelta;
            }

            if (delta < -maxDelta)
            {
                delta = -maxDelta;
            }

            return previous + delta;
        }

        public static bool NoTrap()
        {
            const float step = 0.05f;
            for (int world = 1; world <= ArenaShapeCatalog.WorldCount; world++)
            {
                ArenaShape shape = ArenaShapeCatalog.ForWorld(world);
                if (!shape.IsConvex())
                {
                    return false;
                }

                for (int ray = 0; ray < 8; ray++)
                {
                    float angle = ray * (6.28318530718f / 8f);
                    float x;
                    float z;
                    shape.SpawnRing(angle, 1.2f, out x, out z);
                    float vx = -(float)Math.Cos(angle) * 6f;
                    float vz = -(float)Math.Sin(angle) * 6f;
                    for (int tick = 0; tick < 40; tick++)
                    {
                        float ox;
                        float oz;
                        float ovx;
                        float ovz;
                        bool snapped = Push(shape, x, z, vx, vz, MaxAccel, step, out ox, out oz, out ovx, out ovz);
                        vx = ovx;
                        vz = ovz;
                        if (snapped)
                        {
                            x = ox;
                            z = oz;
                        }
                        else
                        {
                            x = ox + (vx * step);
                            z = oz + (vz * step);
                        }
                    }

                    if (shape.DistanceToEdge(x, z) < HardInside - 0.05f)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
