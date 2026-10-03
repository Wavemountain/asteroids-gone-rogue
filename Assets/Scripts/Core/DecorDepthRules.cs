namespace AsteroidsGoneRogue
{
    /// <summary>
    /// How tall arena decor may stand before it crosses the fight camera.
    /// The play camera sits at (0, 35, -22) relative to the ship, FOV 54, and
    /// looks down the -Z side. Worlds 2–6 ship a single floor+wall mesh whose
    /// vertex max Y is 4.00–5.45 m; after the arena scale (30/22) that is
    /// 5.45–7.43 m and hides the ship from inside about 5 m. World 1 and world 7
    /// load the sunk Astro floor (top -0.08) instead of that wall mesh.
    /// Unity-free so the cap can be checked without the Editor.
    /// </summary>
    public static class DecorDepthRules
    {
        public const float MaxDecorTop = 1.0f;
        public const float MinCameraDistance = 33f;
        public const float CameraOffsetY = 35f;
        public const float CameraOffsetZ = -22f;
        public const float PlayY = 0.4f;
        public const float ArenaRadius = 30f;
        public const float ArenaDesignRadius = 22f;
        public const float FloorTop = -0.08f;
        public const float RimTop = 0.70f;
        public const float RimSouthZ = -29.2f;
        public const float SpikeMeshY = 1.85f;
        public const float PylonRadius = 14f;
        public const float PylonScale = 1.35f;
        public const float TrenchTop = 2.8f;
        public const float TrenchSouthZ = -11f;
        public const float MineRadius = 18.5f;
        public const float MineScale = 1.15f;
        public const float GateTop = 2.6f;
        public const float GateSouthZ = -16f;
        public const float IslandMeshY = 3.10f;
        public const float IslandFit = 1.15f;
        public const float IslandScale = 1.1f;
        public const float IslandSouthZ = -11.5f;
        public const float SpokeTop = 1.6f;
        public const float SpokeSouthZ = -16.75f;
        public const float SpokeSpikeScale = 0.85f;
        public const float BeltOuterScale = 1.68f;
        public const float BeltTop = 3.2f;
        public const float MeshYWorld2 = 4.80f;
        public const float MeshZWorld2 = 24f;
        public const float MeshYWorld3 = 4.55f;
        public const float MeshZWorld3 = 25f;
        public const float MeshYWorld4 = 4.50f;
        public const float MeshZWorld4 = 25f;
        public const float MeshYWorld5 = 4.00f;
        public const float MeshZWorld5 = 26f;
        public const float MeshYWorld6 = 5.45f;
        public const float MeshZWorld6 = 26f;
        public const int GameplayQueue = 2450;
        public const int DecorQueue = 2000;

        public static float ArenaScale()
        {
            return ArenaRadius / ArenaDesignRadius;
        }

        public static float ClampedTop(float unclampedTop)
        {
            if (unclampedTop <= MaxDecorTop)
            {
                return unclampedTop;
            }

            return MaxDecorTop;
        }

        public static float CameraDistance(float x, float y, float z)
        {
            float dx = x;
            float dy = y - CameraOffsetY;
            float dz = z - CameraOffsetZ;
            return (float)System.Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
        }

        /// <summary>
        /// Height of the ray from the camera to the ship at a gap (meters of
        /// -Z) in front of the ship. Decor above this ray sits in the lens.
        /// </summary>
        public static float RayHeight(float gapTowardCamera)
        {
            float gap = gapTowardCamera;
            if (gap < 0f)
            {
                gap = 0f;
            }

            float span = -CameraOffsetZ;
            if (span < 0.01f)
            {
                span = 0.01f;
            }

            return PlayY + ((CameraOffsetY - PlayY) * (gap / span));
        }

        public static bool Clears(float x, float unclampedTop, float z)
        {
            float top = ClampedTop(unclampedTop);
            if (top > MaxDecorTop)
            {
                return false;
            }

            if (CameraDistance(x, top, z) < MinCameraDistance)
            {
                return false;
            }

            float gap = z < 0f ? -z : 0f;
            return top <= RayHeight(gap);
        }
    }
}
