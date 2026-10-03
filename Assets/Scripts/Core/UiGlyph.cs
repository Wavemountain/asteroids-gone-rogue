namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Marks that exist in both Kenney Future and Kenney Future Narrow.
    /// U+2605 and U+25CB are not in those fonts.
    /// </summary>
    public static class UiGlyph
    {
        public const string Medal = "\u2022 ";
        public const string Locked = "+ ";
        public const char LifeFull = '\u2022';
        public const char LifeEmpty = '\u00B7';
        public const string OwnedMark = " +";
    }
}
