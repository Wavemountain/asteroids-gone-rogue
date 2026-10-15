namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Hangar preview target. Emission and studio lights sit above 1, so the
    /// bay uses a half-float target when the GPU can render one. Reduce effects
    /// stays on the old LDR target so that look does not change.
    /// </summary>
    public static class HangarPreviewHdr
    {
        public const int Ldr = 0;
        public const int ArgbHalf = 1;
        public const int DefaultHdr = 2;

        public static bool Use(bool reduceEffects, bool halfSupported, bool defaultHdrSupported)
        {
            if (reduceEffects)
            {
                return false;
            }

            return halfSupported || defaultHdrSupported;
        }

        public static int Format(bool reduceEffects, bool halfSupported, bool defaultHdrSupported)
        {
            if (!Use(reduceEffects, halfSupported, defaultHdrSupported))
            {
                return Ldr;
            }

            if (halfSupported)
            {
                return ArgbHalf;
            }

            return DefaultHdr;
        }

        public static bool AllowsHdr(int format)
        {
            return format == ArgbHalf || format == DefaultHdr;
        }
    }
}
