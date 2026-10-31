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
        public const int Scout = 5;
        public const int Gunner = 6;
        public const int Drone = 7;
        public const int Sniper = 8;
        public const int SwarmPod = 9;
        public const int Mid = 10;

        public const float LegacyBruteRadius = 1.625f;
        public const float LegacyBruteHeight = 3.250f;
        public const float LegacySwarmRadius = 0.910f;
        public const float LegacySwarmHeight = 1.820f;
        public const float LegacyBomberRadius = 4.635f;
        public const float LegacyBomberHeight = 9.270f;
        public const float SwarmlingScale = 0.45f;

        // 0.48 world max-axis, metres, after the importer unit scale and the
        // legacy capsule fit. v4 meshes are scaled onto these.
        public const float RefBruteExtent = 3.250f;
        public const float RefSwarmExtent = 1.820f;
        public const float RefBomberExtent = 9.270f;
        public const float RefSwarmlingExtent = 0.819f;
        public const float RefScoutExtent = 4.720f;
        public const float RefGunnerExtent = 5.795f;
        public const float RefDroneExtent = 2.390f;
        public const float RefSniperExtent = 7.565f;
        public const float RefSwarmPodExtent = 3.085f;
        public const float RefMidExtent = 5.207f;
        public const float ExtentBand = 0.15f;

        // Imported v4 hitbox max-axis before the fit. SelfCheck scales these.
        public const float V4BruteExtent = 2.435f;
        public const float V4SwarmExtent = 1.016f;
        public const float V4BomberExtent = 2.126f;
        public const float V4SwarmlingExtent = 0.600f;
        public const float V4ScoutExtent = 1.648f;
        public const float V4GunnerExtent = 1.877f;
        public const float V4DroneExtent = 2.496f;
        public const float V4SniperExtent = 2.202f;
        public const float V4SwarmPodExtent = 2.360f;
        public const float V4MidExtent = 2.332f;
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

        public static float ReferenceExtent(int id)
        {
            if (id == Brute)
            {
                return RefBruteExtent;
            }

            if (id == Swarm)
            {
                return RefSwarmExtent;
            }

            if (id == Bomber)
            {
                return RefBomberExtent;
            }

            if (id == Swarmling)
            {
                return RefSwarmlingExtent;
            }

            if (id == Scout)
            {
                return RefScoutExtent;
            }

            if (id == Gunner)
            {
                return RefGunnerExtent;
            }

            if (id == Drone)
            {
                return RefDroneExtent;
            }

            if (id == Sniper)
            {
                return RefSniperExtent;
            }

            if (id == SwarmPod)
            {
                return RefSwarmPodExtent;
            }

            if (id == Mid)
            {
                return RefMidExtent;
            }

            return 0f;
        }

        public static bool ExtentNearReference(float rawExtent, float reference)
        {
            float fitted = rawExtent * Uniform(rawExtent, reference);
            float low = reference * (1f - ExtentBand);
            float high = reference * (1f + ExtentBand);
            return fitted >= low && fitted <= high;
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

        public static bool IsEnemyDebrisName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            if (name.IndexOf("Chunk", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (name.IndexOf("Debris", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Combat-mesh hitbox. Chunks and debris stay out. Drone rotors,
        /// the sniper lance, and brute horns stay in even when the name
        /// also carries a telegraph or ring token.
        /// </summary>
        public static bool CountsForEnemyHitbox(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            if (IsEnemyDebrisName(name))
            {
                return false;
            }

            if (name.IndexOf("Rotor", System.StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("Lance", System.StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("Horn", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (name.IndexOf("Ring", System.StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("Well", System.StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("Aura", System.StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("Telegraph", System.StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("Presence", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return false;
            }

            return true;
        }

        public static bool SelfCheck()
        {
            if (Bad(SwarmLookScale < 0.75f || SwarmLookScale > 0.85f))
            {
                return false;
            }

            if (Bad(SwarmShrinksGameplay))
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

            if (!IsEnemyDebrisName("Enemy_Scout_v4_Chunk_01") || !IsEnemyDebrisName("Debris_Plate"))
            {
                return false;
            }

            if (CountsForEnemyHitbox("Enemy_Scout_v4_Chunk_01") || CountsForEnemyHitbox("Debris_Plate"))
            {
                return false;
            }

            if (!CountsForEnemyHitbox("Rotor_R") || !CountsForEnemyHitbox("RotorHub_L"))
            {
                return false;
            }

            if (!CountsForEnemyHitbox("Lance") || !CountsForEnemyHitbox("Horn_L") || !CountsForEnemyHitbox("Telegraph_Horn"))
            {
                return false;
            }

            if (CountsForEnemyHitbox("Telegraph_Muzzle"))
            {
                return false;
            }

            if (!CountsForEnemyHitbox("Body") || !CountsForEnemyHitbox("Thruster_Main"))
            {
                return false;
            }

            if (!ExtentNearReference(V4BruteExtent, RefBruteExtent))
            {
                return false;
            }

            if (!ExtentNearReference(V4SwarmExtent, RefSwarmExtent))
            {
                return false;
            }

            if (!ExtentNearReference(V4BomberExtent, RefBomberExtent))
            {
                return false;
            }

            if (!ExtentNearReference(V4SwarmlingExtent, RefSwarmlingExtent))
            {
                return false;
            }

            if (!ExtentNearReference(V4ScoutExtent, RefScoutExtent))
            {
                return false;
            }

            if (!ExtentNearReference(V4GunnerExtent, RefGunnerExtent))
            {
                return false;
            }

            if (!ExtentNearReference(V4DroneExtent, RefDroneExtent))
            {
                return false;
            }

            if (!ExtentNearReference(V4SniperExtent, RefSniperExtent))
            {
                return false;
            }

            if (!ExtentNearReference(V4SwarmPodExtent, RefSwarmPodExtent))
            {
                return false;
            }

            if (!ExtentNearReference(V4MidExtent, RefMidExtent))
            {
                return false;
            }

            if (ReferenceExtent(Scout) <= 0f || ReferenceExtent(None) != 0f)
            {
                return false;
            }

            return true;
        }

        private static bool Bad(bool failed)
        {
            return failed;
        }
    }
}
