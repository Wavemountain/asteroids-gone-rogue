namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Hangar preview target. Reduce effects does not pick the format:
    /// both modes use HDR when the GPU can build one.
    /// </summary>
    public static class HangarPreviewHdr
    {
        public const int Ldr = 0;
        public const int ArgbHalf = 1;
        public const int DefaultHdr = 2;

        public static bool Use(bool reduceEffects, bool halfSupported, bool defaultHdrSupported)
        {
            bool supported = halfSupported || defaultHdrSupported;
            if (reduceEffects)
            {
                return supported;
            }

            return supported;
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
