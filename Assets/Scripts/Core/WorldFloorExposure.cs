namespace AsteroidsGoneRogue
{
    /// <summary>
    /// World 2 floor against the 0.48 disc at 76c3a5d. The contour reach scale
    /// makes the plate about twice as wide, and the glow slots on that mesh
    /// wash the shot white. W2 is capped at the legacy disc. Emission is
    /// cleared, which is what the 0.48 floor shader wrote. Other worlds keep
    /// their contour scale and their emission. Unity-free.
    /// </summary>
    public static class WorldFloorExposure
    {
        public const int World = 2;

        public static float LegacyDiscScale(float arenaRadius, float designRadius)
        {
            if (designRadius < 0.001f)
            {
                return arenaRadius;
            }

            return arenaRadius / designRadius;
        }

        /// <summary>
        /// W2 never grows past the 0.48 disc. Other worlds keep the contour scale.
        /// </summary>
        public static float CapMeshScale(int world, float scale, float legacyScale)
        {
            if (world != World || scale <= legacyScale)
            {
                return scale;
            }

            return legacyScale;
        }

        public static void ReferenceAlbedo(out float red, out float green, out float blue)
        {
            // 76c3a5d world 2 palette: floor milli 200, 40, 340, brightness 860.
            PremiumLook.FloorRgb(World, 0.2f * 0.86f, 0.04f * 0.86f, 0.34f * 0.86f, out red, out green, out blue);
        }

        /// <summary>
        /// W2 emission is forced to the 0.48 value (none). Albedo cannot rise
        /// above that paint. Other worlds pass through.
        /// </summary>
        public static void Cap(
            int world,
            float albedoR,
            float albedoG,
            float albedoB,
            float emitR,
            float emitG,
            float emitB,
            out float outAlbedoR,
            out float outAlbedoG,
            out float outAlbedoB,
            out float outEmitR,
            out float outEmitG,
            out float outEmitB)
        {
            outAlbedoR = albedoR;
            outAlbedoG = albedoG;
            outAlbedoB = albedoB;
            outEmitR = emitR;
            outEmitG = emitG;
            outEmitB = emitB;
            if (world != World)
            {
                return;
            }

            float refR;
            float refG;
            float refB;
            ReferenceAlbedo(out refR, out refG, out refB);
            if (outAlbedoR > refR)
            {
                outAlbedoR = refR;
            }

            if (outAlbedoG > refG)
            {
                outAlbedoG = refG;
            }

            if (outAlbedoB > refB)
            {
                outAlbedoB = refB;
            }

            outEmitR = 0f;
            outEmitG = 0f;
            outEmitB = 0f;
        }

        public static bool ProductWithinReference(float albedoR, float albedoG, float albedoB, float emitR, float emitG, float emitB)
        {
            float refR;
            float refG;
            float refB;
            ReferenceAlbedo(out refR, out refG, out refB);
            if (albedoR * emitR > refR * 0f + 0.0001f)
            {
                return false;
            }

            if (albedoG * emitG > refG * 0f + 0.0001f)
            {
                return false;
            }

            if (albedoB * emitB > refB * 0f + 0.0001f)
            {
                return false;
            }

            if (albedoR > refR + 0.0001f || albedoG > refG + 0.0001f || albedoB > refB + 0.0001f)
            {
                return false;
            }

            return true;
        }

        public static bool SelfCheck()
        {
            float legacy = LegacyDiscScale(30f, 22f);
            float reach = ArenaShapeCatalog.FloorReach(ArenaShapeCatalog.ForWorld(World));
            float contour = reach / 22f;
            float capped = CapMeshScale(World, contour, legacy);
            if (capped > legacy + 0.0001f || !(contour > capped))
            {
                return false;
            }

            if (CapMeshScale(1, contour, legacy) != contour)
            {
                return false;
            }

            float refR;
            float refG;
            float refB;
            ReferenceAlbedo(out refR, out refG, out refB);
            float albedoR;
            float albedoG;
            float albedoB;
            float emitR;
            float emitG;
            float emitB;
            Cap(World, 0.9f, 0.8f, 0.7f, 4f, 2f, 0.8f, out albedoR, out albedoG, out albedoB, out emitR, out emitG, out emitB);
            if (emitR != 0f || emitG != 0f || emitB != 0f)
            {
                return false;
            }

            if (!ProductWithinReference(albedoR, albedoG, albedoB, emitR, emitG, emitB))
            {
                return false;
            }

            if (albedoR > refR + 0.0001f)
            {
                return false;
            }

            Cap(3, refR, refG, refB, 0.25f, 0.1f, 0.05f, out albedoR, out albedoG, out albedoB, out emitR, out emitG, out emitB);
            if (emitR != 0.25f || albedoR != refR)
            {
                return false;
            }

            return true;
        }
    }
}
