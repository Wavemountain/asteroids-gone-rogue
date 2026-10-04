namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Full-screen camera drawn before the letterboxed play and decor cameras.
    /// Those cameras clear only their 16:9 rect, so the bars would otherwise
    /// keep uncleared color. This camera paints the bars black and draws nothing.
    /// </summary>
    public static class LetterboxClear
    {
        public const string CameraName = "LetterboxCamera";
        public const float Depth = -100f;
        public const int CullingMask = 0;

        public static void FullRect(out float x, out float y, out float width, out float height)
        {
            x = 0f;
            y = 0f;
            width = 1f;
            height = 1f;
        }
    }
}
