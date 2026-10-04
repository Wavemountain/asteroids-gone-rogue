namespace AsteroidsGoneRogue
{
    /// <summary>
    /// On-screen icon size for inline prompts. 18 px sits inside the 16–20 px
    /// band. Canvas units are that size divided by the scaler so a Deck and a
    /// 4K display land on the same physical height.
    /// </summary>
    public static class PromptLayout
    {
        public const float TargetScreenPx = 18f;
        public const float MinScreenPx = 16f;
        public const float MaxScreenPx = 20f;

        public static float IconCanvasPx(float canvasScale)
        {
            float scale = canvasScale < 0.05f ? 0.05f : canvasScale;
            float canvas = TargetScreenPx / scale;
            float minCanvas = MinScreenPx / scale;
            float maxCanvas = MaxScreenPx / scale;
            if (canvas < minCanvas)
            {
                canvas = minCanvas;
            }

            if (canvas > maxCanvas)
            {
                canvas = maxCanvas;
            }

            return canvas;
        }

        public static float RowWidth(float[] widths, float gap)
        {
            if (widths == null || widths.Length == 0)
            {
                return 0f;
            }

            float sum = 0f;
            int count = widths.Length;
            for (int index = 0; index < count; index++)
            {
                if (index > 0)
                {
                    sum += gap;
                }

                float piece = widths[index];
                if (piece > 0f)
                {
                    sum += piece;
                }
            }

            return sum;
        }

        public static bool Fits(float rowWidth, float available)
        {
            if (available <= 0f)
            {
                return false;
            }

            return rowWidth <= available + 0.5f;
        }
    }
}
