namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Screen-fit checks for the resolution matrix. Rects are normalized
    /// anchors (0–1) or canvas pixels. On-screen font is fontSize times the
    /// canvas scaler. The 12 px floor is the Steam Deck 1280x800 size.
    /// </summary>
    public static class UiScreenFit
    {
        public const float DeckWidth = 1280f;
        public const float DeckHeight = 800f;
        public const float MinOnScreenFont = 12f;

        public static readonly float[] MatrixWidths = new float[]
        {
            1280f, 1366f, 1440f, 1920f, 1920f, 2560f, 2560f, 3440f,
        };

        public static readonly float[] MatrixHeights = new float[]
        {
            800f, 768f, 900f, 1080f, 1200f, 1080f, 1440f, 1440f,
        };

        public static int MatrixCount
        {
            get { return MatrixWidths.Length; }
        }

        public static bool InsideScreen(float minX, float minY, float maxX, float maxY, float screenW, float screenH)
        {
            return minX >= -0.5f
                && minY >= -0.5f
                && maxX <= screenW + 0.5f
                && maxY <= screenH + 0.5f
                && maxX > minX + 0.5f
                && maxY > minY + 0.5f;
        }

        public static bool InsideUnit(float minX, float minY, float maxX, float maxY)
        {
            return minX >= -0.0001f
                && minY >= -0.0001f
                && maxX <= 1.0001f
                && maxY <= 1.0001f
                && maxX > minX
                && maxY > minY;
        }

        public static float OnScreenFont(int fontSize, float screenW, float screenH)
        {
            if (fontSize <= 0)
            {
                return 0f;
            }

            return fontSize * SettingsMeasure.CanvasScale(screenW, screenH);
        }

        public static bool MeetsDeckFloor(int fontSize)
        {
            return OnScreenFont(fontSize, DeckWidth, DeckHeight) + 0.05f >= MinOnScreenFont;
        }

        public static int WrappedLines(string text, float boxWidth, int fontSize)
        {
            return SettingsMeasure.WrappedLineCount(text, boxWidth, fontSize);
        }

        public static bool TextFits(string text, float boxWidth, float boxHeight, int fontSize)
        {
            if (fontSize < 1 || boxWidth < 1f || boxHeight < 1f)
            {
                return false;
            }

            if (string.IsNullOrEmpty(text))
            {
                return true;
            }

            int lines = WrappedLines(text, boxWidth, fontSize);
            float lineHeight = fontSize * SettingsMeasure.LineHeightScale;
            return lines * lineHeight <= boxHeight + 0.5f;
        }
    }
}
