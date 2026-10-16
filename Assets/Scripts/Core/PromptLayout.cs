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

        /// <summary>
        /// Shrink a prompt row so it stays inside the hint box. 1 means the
        /// authored size already fits. The floor keeps a truncated row readable.
        /// </summary>
        public static float ScaleToFit(float rowWidth, float rowHeight, float boxWidth, float boxHeight)
        {
            float scaleX = 1f;
            float scaleY = 1f;
            if (boxWidth > 1f && rowWidth > boxWidth)
            {
                scaleX = boxWidth / rowWidth;
            }

            if (boxHeight > 1f && rowHeight > boxHeight)
            {
                scaleY = boxHeight / rowHeight;
            }

            float scale = scaleX < scaleY ? scaleX : scaleY;
            if (scale < 0.35f)
            {
                scale = 0.35f;
            }

            if (scale > 1f)
            {
                scale = 1f;
            }

            return scale;
        }

        public static bool SelfCheck()
        {
            float boxWidth = (0.72f - 0.04f) * 1280f;
            float boxHeight = (0.044f - 0.006f) * 800f;
            float abortMin = 0.74f * 1280f;
            float scale = ScaleToFit(1193f, 42.6f, boxWidth, boxHeight);
            float fitted = 1193f * scale;
            if (fitted > boxWidth + 0.5f || 42.6f * scale > boxHeight + 0.5f)
            {
                return false;
            }

            float center = ((0.04f + 0.72f) * 0.5f) * 1280f;
            float right = center + (fitted * 0.5f);
            if (right > abortMin || center - (fitted * 0.5f) < 0f)
            {
                return false;
            }

            return scale < 1f && ScaleToFit(100f, 10f, 400f, 40f) == 1f;
        }
    }
}
