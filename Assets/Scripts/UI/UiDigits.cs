namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Fixed-width digit cells. Kenney Future has no tabular figures:
    /// "1" is 468 units and the other digits are 852 of 1024.
    /// Narrow digits are 724 of 1024. Unity-free so the widths can be tested.
    /// </summary>
    public static class UiDigits
    {
        public const int FutureUnits = 852;
        public const int Em = 1024;
        public const int NarrowUnits = 724;
        public const int CreditCells = 5;
        public const int CreditSize = 40;
        public const int StripSize = 28;
        public const int StripSizeNarrow = 26;
        public const int ValueSize = 22;

        public static int CellWidth(int size)
        {
            if (size < 0)
            {
                size = 0;
            }

            return (size * FutureUnits + (Em - 1)) / Em;
        }

        public static int NarrowCell(int size)
        {
            if (size < 0)
            {
                size = 0;
            }

            return (size * NarrowUnits + (Em - 1)) / Em;
        }

        public const int LineUnits = 1152;

        public static int GlyphWidth(int size)
        {
            return CellWidth(size) + 4;
        }

        public static int GlyphHeight(int size)
        {
            if (size < 0)
            {
                size = 0;
            }

            return (size * LineUnits + (Em - 1)) / Em + 4;
        }

        public static bool GlyphFits(int size)
        {
            float width = size * (float)FutureUnits / Em;
            float height = size * 1.2f;
            return width <= GlyphWidth(size) + 2f && height <= GlyphHeight(size) + 2f;
        }

        public static int StripSizeFor(int glyphs, float contentWidth)
        {
            int wide = CellWidth(StripSize);
            if (glyphs >= 6 && contentWidth < wide * 6)
            {
                return StripSizeNarrow;
            }

            return StripSize;
        }
    }
}
