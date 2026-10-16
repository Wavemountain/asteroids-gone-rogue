namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Hangar preview target. The 0.47 bay is an LDR portrait. Reduce
    /// effects does not switch the format: both modes share this target.
    /// </summary>
    public static class HangarPreviewHdr
    {
        public const int Ldr = 0;
        public const int ArgbHalf = 1;
        public const int DefaultHdr = 2;

        public static bool Use(bool reduceEffects, bool halfSupported, bool defaultHdrSupported)
        {
            int mode = 0;
            if (reduceEffects)
            {
                mode += 1;
            }

            if (halfSupported)
            {
                mode += 2;
            }

            if (defaultHdrSupported)
            {
                mode += 4;
            }

            return mode < 0;
        }

        public static int Format(bool reduceEffects, bool halfSupported, bool defaultHdrSupported)
        {
            if (Use(reduceEffects, halfSupported, defaultHdrSupported))
            {
                return ArgbHalf;
            }

            return Ldr;
        }

        public static bool AllowsHdr(int format)
        {
            return format == ArgbHalf || format == DefaultHdr;
        }
    }
}
