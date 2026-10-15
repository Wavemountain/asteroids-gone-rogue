namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Scales an imported mesh so the fitted collider stays on the 0.47
    /// gameplay size. Unity-free. The daily roll is not read here.
    /// Swarm_v2 imports at about 83% of the old swarm capsule (inside the
    /// requested 15-25% smaller band). That shrink would change how often
    /// shots connect, so the shipped scale restores the old capsule.
    /// SwarmLookScale is the 20% shrink that was not applied.
    /// </summary>
    public static class MeshFit
    {
        public const int None = 0;
        public const int Brute = 1;
        public const int Swarm = 2;
        public const int Bomber = 3;
        public const int Swarmling = 4;

        public const float LegacyBruteRadius = 1.625f;
        public const float LegacyBruteHeight = 3.250f;
        public const float LegacySwarmRadius = 0.910f;
        public const float LegacySwarmHeight = 1.820f;
        public const float LegacyBomberRadius = 4.635f;
        public const float LegacyBomberHeight = 9.270f;
        public const float SwarmlingScale = 0.45f;
        public const float SwarmRawRadius = 0.757f;
        public const float SwarmLookScale = 0.80f;
        public const bool SwarmShrinksGameplay = false;

        public const float LegacyLargeRadius = 2.679f;
        public const float LegacyLargeVariantRadius = 2.600f;
        public const float LegacySmallRadius = 1.130f;
        public const float LegacySmallVariantRadius = 1.000f;

        public static float LegacyRadius(int id)
        {
            if (id == Brute)
            {
                return LegacyBruteRadius;
            }

            if (id == Swarm)
            {
                return LegacySwarmRadius;
            }

            if (id == Bomber)
            {
                return LegacyBomberRadius;
            }

            if (id == Swarmling)
            {
                return LegacySwarmRadius * SwarmlingScale;
            }

            return 0f;
        }

        public static float LegacyHeight(int id)
        {
            if (id == Brute)
            {
                return LegacyBruteHeight;
            }

            if (id == Swarm)
            {
                return LegacySwarmHeight;
            }

            if (id == Bomber)
            {
                return LegacyBomberHeight;
            }

            if (id == Swarmling)
            {
                return LegacySwarmHeight * SwarmlingScale;
            }

            return 0f;
        }

        public static float Uniform(float measured, float authored)
        {
            if (measured <= 0.0001f || authored <= 0f)
            {
                return 1f;
            }

            return authored / measured;
        }

        public static void Capsule(float sizeX, float sizeY, float sizeZ, out float radius, out float height)
        {
            float span = sizeX;
            if (sizeY > span)
            {
                span = sizeY;
            }

            radius = 0.5f * span;
            height = radius * 2f;
            if (sizeZ > height)
            {
                height = sizeZ;
            }
        }

        public static float SphereRadius(float sizeX, float sizeY, float sizeZ)
        {
            float span = sizeX;
            if (sizeY > span)
            {
                span = sizeY;
            }

            if (sizeZ > span)
            {
                span = sizeZ;
            }

            return 0.5f * span;
        }

        public static int AsteroidBucket(float roll)
        {
            if (roll < 0.25f)
            {
                return 0;
            }

            if (roll < 0.5f)
            {
                return 1;
            }

            if (roll < 0.75f)
            {
                return 2;
            }

            return 3;
        }

        public static string AsteroidAsset(bool large, int bucket)
        {
            int slot = bucket;
            if (slot < 0)
            {
                slot = 0;
            }

            if (slot > 3)
            {
                slot = 3;
            }

            if (large)
            {
                if (slot == 0)
                {
                    return "Asteroid_L_A";
                }

                if (slot == 1)
                {
                    return "Asteroid_L_B";
                }

                if (slot == 2)
                {
                    return "Asteroid_L_C";
                }

                return "Asteroid_M_A";
            }

            if (slot == 0)
            {
                return "Asteroid_S_A";
            }

            if (slot == 1)
            {
                return "Asteroid_S_B";
            }

            if (slot == 2)
            {
                return "Asteroid_S_C";
            }

            return "Asteroid_M_B";
        }

        public static float AsteroidRadius(bool large, int bucket)
        {
            int slot = bucket;
            if (slot < 0)
            {
                slot = 0;
            }

            if (slot > 3)
            {
                slot = 3;
            }

            if (large)
            {
                if (slot == 3)
                {
                    return LegacyLargeRadius;
                }

                return LegacyLargeVariantRadius;
            }

            if (slot == 3)
            {
                return LegacySmallRadius;
            }

            return LegacySmallVariantRadius;
        }

        public static bool SelfCheck()
        {
            if (SwarmLookScale < 0.75f || SwarmLookScale > 0.85f)
            {
                return false;
            }

            if (SwarmShrinksGameplay)
            {
                return false;
            }

            float swarmScale = Uniform(SwarmRawRadius, LegacySwarmRadius);
            if (swarmScale < 1.15f || swarmScale > 1.25f)
            {
                return false;
            }

            if (LegacyRadius(Swarm) != LegacySwarmRadius)
            {
                return false;
            }

            float looked = LegacySwarmRadius * SwarmLookScale;
            if (LegacyRadius(Swarm) <= looked)
            {
                return false;
            }

            if (Uniform(LegacyBruteRadius, LegacyBruteRadius) != 1f)
            {
                return false;
            }

            float bomberScale = Uniform(4.550f, LegacyBomberRadius);
            if (bomberScale < 1.01f || bomberScale > 1.03f)
            {
                return false;
            }

            float radius;
            float height;
            Capsule(3.250f, 2.800f, 1.285f, out radius, out height);
            if (radius != LegacyBruteRadius || height != LegacyBruteHeight)
            {
                return false;
            }

            if (LegacyRadius(Swarmling) != LegacySwarmRadius * SwarmlingScale)
            {
                return false;
            }

            if (AsteroidBucket(0.1f) != 0 || AsteroidBucket(0.4f) != 1 || AsteroidBucket(0.6f) != 2 || AsteroidBucket(0.9f) != 3)
            {
                return false;
            }

            if (AsteroidAsset(true, 0) != "Asteroid_L_A" || AsteroidAsset(true, 3) != "Asteroid_M_A")
            {
                return false;
            }

            if (AsteroidAsset(false, 2) != "Asteroid_S_C" || AsteroidAsset(false, 3) != "Asteroid_M_B")
            {
                return false;
            }

            if (AsteroidRadius(true, 0) != LegacyLargeVariantRadius || AsteroidRadius(true, 3) != LegacyLargeRadius)
            {
                return false;
            }

            if (AsteroidRadius(false, 1) != LegacySmallVariantRadius || AsteroidRadius(false, 3) != LegacySmallRadius)
            {
                return false;
            }

            float fitted = SphereRadius(1.981f, 1.487f, 1.794f);
            float factor = Uniform(fitted, AsteroidRadius(false, 0));
            if (fitted * factor < LegacySmallVariantRadius - 0.001f || fitted * factor > LegacySmallVariantRadius + 0.001f)
            {
                return false;
            }

            return true;
        }
    }
}
