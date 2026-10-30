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
        public const float BeltOuterScale = 1.6f;
        public const float BeltTop = 3.2f;

        // Contour decor. Clusters sit outside the play shape. Counts are the per-level budget.
        public const float EdgeLayerYMin = -1.5f;
        public const float EdgeLayerYMax = -0.6f;
        public const float FarLayerYMin = -20f;
        public const float FarLayerYMax = -6f;
        public const float EdgeOutsideMetres = 2f;
        public const float FarOutsideMetres = 6f;
        public const float DustBandMetres = 7f;
        public const float DustNearMetres = 2.2f;
        public const float DustFarMetres = 5f;
        public const float ClusterExtent = 0.65f;
        public const float DustCardExtent = 2f;
        public const float RockExtent = 0.2f;
        public const float RockGroupRadius = 0.35f;
        public const int EdgeClusterCount = 32;
        public const int FarClusterCount = 16;
        public const int DustCardCount = 48;
        public const int RockLodCount = 6;
        public const int MaxEdgeInstances = 1500;
        public const int MaxEdgeTris = 60000;
        public const int ClusterTris = 1120;
        public const int DustTris = 2;
        public const int RockLod0Tris = 294;
        public const int RockLod1Tris = 78;
        public const float MaxSpinDegrees = 4f;
        public const float AlbedoScale = 0.45f;
        public const float Desaturate = 0.70f;
        public const float DecorLumaCap = 0.17f;
        public const float ContrastMin = 3f;
        public const float EdgeFog = 0.35f;
        public const float FarFog = 0.70f;
        public const float RealRockR = 0.38f;
        public const float RealRockG = 0.32f;
        public const float RealRockB = 0.28f;
        public const float EdgeRockR = 0.16470589f;
        public const float EdgeRockG = 0.18039216f;
        public const float EdgeRockB = 0.20784314f;
        public const float RockRoughness = 0.95f;
        public const float DustLuma = 0.12f;
        public const float GlimmerLuma = 0.18f;
        public const float GlimmerLumaCap = 0.20f;
        public const float DustBaseR = 0.22745098f;
        public const float DustBaseG = 0.20392157f;
        public const float DustBaseB = 0.17254902f;

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
        public const int BlobQueue = 2990;
        public const string BlobMaterial = "Mat_ContactBlob";

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

        public static string Bare(string name)
        {
            if (name == null)
            {
                return string.Empty;
            }

            const string Suffix = " (Instance)";
            if (name.EndsWith(Suffix, System.StringComparison.Ordinal))
            {
                return name.Substring(0, name.Length - Suffix.Length);
            }

            return name;
        }

        public static bool IsBlobMaterial(string name)
        {
            return Bare(name) == BlobMaterial;
        }

        /// <summary>
        /// Gameplay meshes that share a material need their own queue.
        /// Blob shadows keep the authored queue (2990) on the shared material.
        /// Returns false when the shared material must stay untouched.
        /// </summary>
        public static bool InstanceQueue(int authored, bool blobShadow, out int instanceQueue)
        {
            instanceQueue = authored;
            if (blobShadow)
            {
                return false;
            }

            if (authored < 3000 && authored != GameplayQueue)
            {
                instanceQueue = GameplayQueue;
                return true;
            }

            return false;
        }

        public static bool SelfCheck()
        {
            int shared = BlobQueue;
            int inst;
            if (InstanceQueue(shared, true, out inst))
            {
                return false;
            }

            if (shared != BlobQueue || inst != BlobQueue)
            {
                return false;
            }

            if (!IsBlobMaterial(BlobMaterial) || !IsBlobMaterial(BlobMaterial + " (Instance)"))
            {
                return false;
            }

            if (IsBlobMaterial("Mat_Arena_Plate"))
            {
                return false;
            }

            int authored = DecorQueue;
            if (!InstanceQueue(authored, false, out inst) || inst != GameplayQueue)
            {
                return false;
            }

            if (authored != DecorQueue)
            {
                return false;
            }

            if (InstanceQueue(GameplayQueue, false, out inst))
            {
                return false;
            }

            if (InstanceQueue(3000, false, out inst))
            {
                return false;
            }

            if (!EdgeBudgetHolds())
            {
                return false;
            }

            if (!ClearsPlay(EdgeOutsideMetres, ClusterExtent))
            {
                return false;
            }

            if (!ClearsPlay(FarOutsideMetres, ClusterExtent))
            {
                return false;
            }

            if (!ClearsPlay(DustNearMetres, DustCardExtent))
            {
                return false;
            }

            if (!ClearsPlay(DustFarMetres, DustCardExtent))
            {
                return false;
            }

            if (!ClearsPlay(FarOutsideMetres - RockGroupRadius, RockExtent))
            {
                return false;
            }

            float gradedR;
            float gradedG;
            float gradedB;
            GradeDecor(0.12f, 0.28f, 0.9f, EdgeFog, out gradedR, out gradedG, out gradedB);
            float gradedLuma = Luma(gradedR, gradedG, gradedB);
            float realLuma = Luma(RealRockR, RealRockG, RealRockB);
            if (gradedLuma > DecorLumaCap || gradedLuma <= 0f || (realLuma / gradedLuma) < ContrastMin)
            {
                return false;
            }

            float farR;
            float farG;
            float farB;
            GradeDecor(0.12f, 0.28f, 0.9f, FarFog, out farR, out farG, out farB);
            float farLuma = Luma(farR, farG, farB);
            if (farLuma > DecorLumaCap || farLuma <= 0f || (realLuma / farLuma) < ContrastMin)
            {
                return false;
            }

            float dustR;
            float dustG;
            float dustB;
            DustRgb(DustLuma, out dustR, out dustG, out dustB);
            float dustLuma = Luma(dustR, dustG, dustB);
            if (dustLuma < 0.10f || dustLuma > 0.15f)
            {
                return false;
            }

            DustRgb(GlimmerLuma, out dustR, out dustG, out dustB);
            if (Luma(dustR, dustG, dustB) > GlimmerLumaCap)
            {
                return false;
            }

            for (int sample = 0; sample < 8; sample++)
            {
                float yEdge = EdgeDecorY(sample, EdgeLayerYMin, EdgeLayerYMax);
                if (yEdge < EdgeLayerYMin || yEdge > EdgeLayerYMax)
                {
                    return false;
                }

                float yFar = EdgeDecorY(sample, FarLayerYMin, FarLayerYMax);
                if (yFar < FarLayerYMin || yFar > FarLayerYMax)
                {
                    return false;
                }

                float spin = SpinDegrees(sample);
                if (spin < 0f || spin > MaxSpinDegrees)
                {
                    return false;
                }
            }

            return true;
        }

        public static int PlannedInstances()
        {
            return EdgeClusterCount + FarClusterCount + DustCardCount + RockLodCount;
        }

        public static int PlannedTris()
        {
            return (EdgeClusterCount * ClusterTris)
                + (FarClusterCount * ClusterTris)
                + (DustCardCount * DustTris)
                + (RockLodCount * RockLod0Tris);
        }

        public static bool EdgeBudgetHolds()
        {
            int instances = PlannedInstances();
            int tris = PlannedTris();
            if (instances > MaxEdgeInstances || instances < 1)
            {
                return false;
            }

            if (tris > MaxEdgeTris || tris < 1)
            {
                return false;
            }

            return true;
        }

        public static bool ClearsPlay(float outside, float extent)
        {
            return (outside - extent) > 0.05f;
        }

        public static float Luma(float red, float green, float blue)
        {
            return (0.2126f * red) + (0.7152f * green) + (0.0722f * blue);
        }

        public static float EdgeDecorY(int index, float yMin, float yMax)
        {
            float t = (index % 5) / 4f;
            return yMin + ((yMax - yMin) * t);
        }

        public static float SpinDegrees(int index)
        {
            float spin = 0.5f + ((index % 8) * 0.45f);
            if (spin > MaxSpinDegrees)
            {
                spin = MaxSpinDegrees;
            }

            if (spin < 0f)
            {
                spin = 0f;
            }

            return spin;
        }

        public static void DustRgb(float lumaTarget, out float red, out float green, out float blue)
        {
            float baseLuma = Luma(DustBaseR, DustBaseG, DustBaseB);
            float scale = lumaTarget / baseLuma;
            red = DustBaseR * scale;
            green = DustBaseG * scale;
            blue = DustBaseB * scale;
        }

        /// <summary>
        /// #2A2E35 times 0.45, 70% toward gray, then the world fog. Luma stays
        /// at or under 0.17 and at least 3:1 under a real asteroid.
        /// </summary>
        public static void GradeDecor(float fogR, float fogG, float fogB, float fogAmount, out float red, out float green, out float blue)
        {
            red = EdgeRockR * AlbedoScale;
            green = EdgeRockG * AlbedoScale;
            blue = EdgeRockB * AlbedoScale;
            float gray = Luma(red, green, blue);
            float keep = 1f - Desaturate;
            red = (red * keep) + (gray * Desaturate);
            green = (green * keep) + (gray * Desaturate);
            blue = (blue * keep) + (gray * Desaturate);
            float fog = fogAmount;
            if (fog < 0f)
            {
                fog = 0f;
            }

            if (fog > 1f)
            {
                fog = 1f;
            }

            float stay = 1f - fog;
            red = (red * stay) + (fogR * fog);
            green = (green * stay) + (fogG * fog);
            blue = (blue * stay) + (fogB * fog);
            float luma = Luma(red, green, blue);
            if (luma > DecorLumaCap && luma > 0.0001f)
            {
                float capScale = DecorLumaCap / luma;
                red *= capScale;
                green *= capScale;
                blue *= capScale;
            }

            float real = Luma(RealRockR, RealRockG, RealRockB);
            float limit = real / ContrastMin;
            luma = Luma(red, green, blue);
            if (luma > limit && luma > 0.0001f)
            {
                float contrastScale = limit / luma;
                red *= contrastScale;
                green *= contrastScale;
                blue *= contrastScale;
            }
        }
    }
}
