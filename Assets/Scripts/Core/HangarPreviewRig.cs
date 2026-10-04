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
        public const float KeyIntensity = 2.05f;
        public const float FillIntensity = 0.48f;
        public const float RimIntensity = 1.7f;
        public const float KeyR = 1f;
        public const float KeyG = 0.84f;
        public const float KeyB = 0.62f;
        public const float FillR = 0.42f;
        public const float FillG = 0.62f;
        public const float FillB = 0.84f;
        public const float RimR = 0.62f;
        public const float RimG = 0.78f;
        public const float RimB = 1f;
        public const float CameraX = 0.2f;
        public const float CameraY = 4.55f;
        public const float CameraZ = -10f;
        public const float LookY = 0.08f;
        public const float FovDegrees = 40f;
        public const float ViewportWidth = 768f;
        public const float ViewportHeight = 960f;
        public const float FitRadius = 2.5f;

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
