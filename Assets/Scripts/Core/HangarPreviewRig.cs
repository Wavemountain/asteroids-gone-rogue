using System;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Hangar studio motion, lights, and upgrade readouts. Unity-free.
    /// The camera pose stays the one locked in <c>HangarShipPreview</c>.
    /// </summary>
    public static class HangarPreviewRig
    {
        public const float LegacySpinDegrees = 18f;
        public const float TurntableScale = 0.5f;
        public const float KeyIntensity = 0.85f;
        public const float FillIntensity = 0.16f;
        public const float RimIntensity = 0.70f;
        public const float RimReduceScale = 0.45f;
        public const float BounceIntensity = 0.10f;
        public const float KeyR = 1f;
        public const float KeyG = 0.84f;
        public const float KeyB = 0.62f;
        public const float FillR = 0.42f;
        public const float FillG = 0.62f;
        public const float FillB = 0.84f;
        public const float RimR = 0.62f;
        public const float RimG = 0.78f;
        public const float RimB = 1f;
        public const float BounceR = 0.16f;
        public const float BounceG = 0.17f;
        public const float BounceB = 0.18f;
        public const float KeyLocalX = -3.0f;
        public const float KeyLocalY = 4.0f;
        public const float KeyLocalZ = -3.5f;
        public const float FillLocalX = 3.5f;
        public const float FillLocalY = 1.5f;
        public const float FillLocalZ = -2.5f;
        public const float RimLocalX = 0.5f;
        public const float RimLocalY = 3.0f;
        public const float RimLocalZ = 4.0f;
        public const float BounceLocalX = 0f;
        public const float BounceLocalY = -1.2f;
        public const float BounceLocalZ = 0f;
        public const float KeySpotAngle = 40f;
        public const float RimSpotAngle = 30f;
        public const float SpotInnerFraction = 0.55f;
        public const float KeyRange = 16f;
        public const float FillRange = 10f;
        public const float RimRange = 14f;
        public const float BounceRange = 4f;
        public const float BackdropCenterR = 34f / 255f;
        public const float BackdropCenterG = 26f / 255f;
        public const float BackdropCenterB = 20f / 255f;
        public const float BackdropEdgeR = 10f / 255f;
        public const float BackdropEdgeG = 12f / 255f;
        public const float BackdropEdgeB = 16f / 255f;
        public const float SurfaceR = 14f / 255f;
        public const float SurfaceG = 21f / 255f;
        public const float SurfaceB = 32f / 255f;
        public const float Surface2R = 20f / 255f;
        public const float Surface2G = 28f / 255f;
        public const float Surface2B = 40f / 255f;
        public const float BackdropPastLook = 6f;
        public const float BackdropMargin = 1.35f;
        public const float CameraX = 0.2f;
        public const float CameraY = 4.55f;
        public const float CameraZ = -10f;
        public const float LookY = 0.08f;
        public const float FovDegrees = 40f;
        public const float ViewportWidth = 768f;
        public const float ViewportHeight = 960f;
        public const float FitRadius = 2.5f;
        public const float WellMinX = 0.048f;
        public const float WellMinY = 0.048f;
        public const float WellMaxX = 0.952f;
        public const float WellMaxY = 0.860f;

        public const int BitRapid = 0;
        public const int BitNose = 1;
        public const int BitNose02 = 4;
        public const int BitNose03 = 5;
        public const int BitEngine02 = 6;
        public const int BitEngine03 = 7;
        public const int BitOvercharger = 14;
        public const int BitAfterburner = 15;

        public struct Accent
        {
            public bool Hardpoint;
            public bool Engine;
            public bool Mk2;
            public int PaintId;
            public int TrailId;
        }

        public static float SpinDegreesPerSecond(bool interacting, bool reduceEffects)
        {
            if (interacting || reduceEffects)
            {
                return 0f;
            }

            return LegacySpinDegrees * TurntableScale;
        }

        public static bool GlowPulses(bool reduceEffects)
        {
            return false;
        }

        /// <summary>
        /// Full studio rim, or a calmer rim when Reduce effects is on.
        /// The turntable already stops for that flag; this only dims the rim light.
        /// </summary>
        public static float StudioRim(bool reduceEffects)
        {
            if (reduceEffects)
            {
                return RimIntensity * RimReduceScale;
            }

            return RimIntensity;
        }

        /// <summary>
        /// Radial backdrop: warm centre, cool edge. Never pure black or green.
        /// </summary>
        public static void BackdropRgb(float u, float v, out float red, out float green, out float blue)
        {
            float du = u - 0.5f;
            float dv = v - 0.5f;
            float dist = (float)Math.Sqrt((du * du) + (dv * dv)) / 0.70710678f;
            if (dist < 0f)
            {
                dist = 0f;
            }

            if (dist > 1f)
            {
                dist = 1f;
            }

            red = BackdropCenterR + ((BackdropEdgeR - BackdropCenterR) * dist);
            green = BackdropCenterG + ((BackdropEdgeG - BackdropCenterG) * dist);
            blue = BackdropCenterB + ((BackdropEdgeB - BackdropCenterB) * dist);
        }

        public static Accent FromMasks(int upgradeMask, int mk2Mask, int paintId, int trailId)
        {
            Accent accent = new Accent();
            int mask = upgradeMask < 0 ? 0 : upgradeMask;
            accent.Hardpoint = Bit(mask, BitNose) || Bit(mask, BitNose02) || Bit(mask, BitNose03) || Bit(mask, BitOvercharger);
            accent.Engine = Bit(mask, BitRapid) || Bit(mask, BitEngine02) || Bit(mask, BitEngine03) || Bit(mask, BitAfterburner);
            accent.Mk2 = mk2Mask != 0;
            accent.PaintId = paintId;
            accent.TrailId = trailId;
            return accent;
        }

        public static bool ShipFitsFrame(float radius)
        {
            if (radius <= 0f)
            {
                return false;
            }

            float dx = CameraX;
            float dy = CameraY - LookY;
            float dz = CameraZ;
            double distSq = (dx * dx) + (dy * dy) + (dz * dz);
            if (distSq < 0.01)
            {
                return false;
            }

            double dist = Math.Sqrt(distSq);
            double halfRad = (FovDegrees * 0.5) * (Math.PI / 180.0);
            double verticalHalf = dist * Math.Tan(halfRad);
            double aspect = ViewportWidth / ViewportHeight;
            if (aspect <= 0.0)
            {
                return false;
            }

            double horizontalHalf = verticalHalf * aspect;
            double need = radius;
            return need < horizontalHalf && need < verticalHalf;
        }

        /// <summary>
        /// Contain the portrait preview inside a landscape well. uvRect stays the
        /// full texture. The normalized rect is the unstretched image inside the well.
        /// </summary>
        public static void Contain(
            float frameWidth,
            float frameHeight,
            float textureWidth,
            float textureHeight,
            out float uvX,
            out float uvY,
            out float uvW,
            out float uvH,
            out float minX,
            out float minY,
            out float maxX,
            out float maxY)
        {
            uvX = 0f;
            uvY = 0f;
            uvW = 1f;
            uvH = 1f;
            minX = 0f;
            minY = 0f;
            maxX = 1f;
            maxY = 1f;
            if (frameWidth <= 0f || frameHeight <= 0f || textureWidth <= 0f || textureHeight <= 0f)
            {
                return;
            }

            float frameAspect = frameWidth / frameHeight;
            float textureAspect = textureWidth / textureHeight;
            if (frameAspect > textureAspect)
            {
                float used = textureAspect / frameAspect;
                float pad = (1f - used) * 0.5f;
                minX = pad;
                maxX = 1f - pad;
                return;
            }

            if (textureAspect > frameAspect)
            {
                float used = frameAspect / textureAspect;
                float pad = (1f - used) * 0.5f;
                minY = pad;
                maxY = 1f - pad;
            }
        }

        private static bool Bit(int mask, int index)
        {
            if (index < 0 || index > 30)
            {
                return false;
            }

            return (mask & (1 << index)) != 0;
        }
    }
}
