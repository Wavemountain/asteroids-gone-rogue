namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Combat HUD column. Width is capped in pixels so 1280..3440 leave the
    /// arena readable. Menu plates stay on <c>UiTheme.SurfaceAlpha</c>.
    /// </summary>
    public static class PlayHudLayout
    {
        public const float PlayHudAlpha = 0.40f;
        public const float MaxWidthPx = 420f;
        public const float Left = 0.012f;
        public const float NarrowCap = 0.28f;
        public const float TextInset = 0.012f;
        public const float TextTail = 0.008f;

        public const float StatusMinY = 0.705f;
        public const float StatusMaxY = 0.888f;
        public const float HealthMinY = 0.056f;
        public const float HealthMaxY = 0.118f;
        public const float BossMinY = 0.132f;
        public const float BossMaxY = 0.172f;
        public const float UtilityMinX = 0.74f;
        public const float UtilityMinY = 0.196f;
        public const float UtilityMaxX = 0.985f;
        public const float UtilityMaxY = 0.300f;
        public const float DoctrineMinX = 0.74f;
        public const float DoctrineMinY = 0.312f;
        public const float DoctrineMaxX = 0.985f;
        public const float DoctrineMaxY = 0.356f;
        // Combat hint is its own row under the health rack and doctrine badge.
        // Hangar keeps the footer at y 0.008–0.072.
        public const float PlayHintMinX = 0.04f;
        public const float PlayHintMinY = 0.006f;
        public const float PlayHintMaxX = 0.72f;
        public const float PlayHintMaxY = 0.044f;
        public const float AbortWidthPx = 220f;
        public const float AbortHeightPx = 36f;
        public const float AbortMaxX = 0.985f;
        public const float AbortMaxY = 0.048f;
        public const float AbortSpanCapX = 0.22f;
        public const float AbortSpanCapY = 0.055f;

        public static float ColumnMaxX(float screenWidth)
        {
            float width = screenWidth;
            if (width < 1f)
            {
                width = 1920f;
            }

            float span = MaxWidthPx / width;
            if (span > NarrowCap)
            {
                span = NarrowCap;
            }

            float maxX = Left + span;
            if (maxX > 0.96f)
            {
                maxX = 0.96f;
            }

            return maxX;
        }

        public static float TextMaxX(float screenWidth)
        {
            float maxX = ColumnMaxX(screenWidth) - TextTail;
            float minText = Left + TextInset;
            if (maxX < minText)
            {
                return minText;
            }

            return maxX;
        }

        public static void AbortRect(float screenWidth, float screenHeight, out float minX, out float minY, out float maxX, out float maxY)
        {
            float width = screenWidth;
            float height = screenHeight;
            if (width < 1f)
            {
                width = 1920f;
            }

            if (height < 1f)
            {
                height = 1080f;
            }

            float spanX = AbortWidthPx / width;
            float spanY = AbortHeightPx / height;
            if (spanX > AbortSpanCapX)
            {
                spanX = AbortSpanCapX;
            }

            if (spanY > AbortSpanCapY)
            {
                spanY = AbortSpanCapY;
            }

            maxX = AbortMaxX;
            maxY = AbortMaxY;
            minX = maxX - spanX;
            minY = maxY - spanY;
        }

        public static bool SelfCheck()
        {
            if (!Stacked(PlayHintMaxY, HealthMinY) || !Stacked(PlayHintMaxY, DoctrineMinY))
            {
                return false;
            }

            if (!Stacked(AbortMaxY, UtilityMinY) || !Stacked(UtilityMaxY, DoctrineMinY) || !Stacked(HealthMaxY, BossMinY))
            {
                return false;
            }

            float[] widths = { 1280f, 1920f, 2560f, 3440f };
            float[] heights = { 800f, 1080f, 1440f };
            for (int widthIndex = 0; widthIndex < widths.Length; widthIndex++)
            {
                for (int heightIndex = 0; heightIndex < heights.Length; heightIndex++)
                {
                    if (!RowClear(widths[widthIndex], heights[heightIndex]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool RowClear(float screenWidth, float screenHeight)
        {
            float hintMinX = PlayHintMinX;
            float hintMinY = PlayHintMinY;
            float hintMaxX = PlayHintMaxX;
            float hintMaxY = PlayHintMaxY;
            float abortMinX;
            float abortMinY;
            float abortMaxX;
            float abortMaxY;
            AbortRect(screenWidth, screenHeight, out abortMinX, out abortMinY, out abortMaxX, out abortMaxY);
            float barMin;
            float barMax;
            BarX(screenWidth, out barMin, out barMax);
            if (Overlaps(hintMinX, hintMinY, hintMaxX, hintMaxY, barMin, HealthMinY, barMax, HealthMaxY))
            {
                return false;
            }

            if (Overlaps(hintMinX, hintMinY, hintMaxX, hintMaxY, barMin, BossMinY, barMax, BossMaxY))
            {
                return false;
            }

            if (Overlaps(hintMinX, hintMinY, hintMaxX, hintMaxY, UtilityMinX, UtilityMinY, UtilityMaxX, UtilityMaxY))
            {
                return false;
            }

            if (Overlaps(hintMinX, hintMinY, hintMaxX, hintMaxY, DoctrineMinX, DoctrineMinY, DoctrineMaxX, DoctrineMaxY))
            {
                return false;
            }

            if (Overlaps(hintMinX, hintMinY, hintMaxX, hintMaxY, abortMinX, abortMinY, abortMaxX, abortMaxY))
            {
                return false;
            }

            if (Overlaps(abortMinX, abortMinY, abortMaxX, abortMaxY, UtilityMinX, UtilityMinY, UtilityMaxX, UtilityMaxY))
            {
                return false;
            }

            if (Overlaps(abortMinX, abortMinY, abortMaxX, abortMaxY, DoctrineMinX, DoctrineMinY, DoctrineMaxX, DoctrineMaxY))
            {
                return false;
            }

            if (Overlaps(abortMinX, abortMinY, abortMaxX, abortMaxY, barMin, HealthMinY, barMax, HealthMaxY))
            {
                return false;
            }

            if (Overlaps(barMin, HealthMinY, barMax, HealthMaxY, barMin, BossMinY, barMax, BossMaxY))
            {
                return false;
            }

            if (Overlaps(UtilityMinX, UtilityMinY, UtilityMaxX, UtilityMaxY, DoctrineMinX, DoctrineMinY, DoctrineMaxX, DoctrineMaxY))
            {
                return false;
            }

            return true;
        }

        private static void BarX(float screenWidth, out float minX, out float maxX)
        {
            float span = 280f / screenWidth;
            if (span > 0.46f)
            {
                span = 0.46f;
            }

            minX = 0.5f - (span * 0.5f);
            maxX = 0.5f + (span * 0.5f);
        }

        private static bool Stacked(float lowerMax, float upperMin)
        {
            return lowerMax < upperMin;
        }

        private static bool Overlaps(
            float aMinX,
            float aMinY,
            float aMaxX,
            float aMaxY,
            float bMinX,
            float bMinY,
            float bMaxX,
            float bMaxY)
        {
            return aMinX < bMaxX && aMaxX > bMinX && aMinY < bMaxY && aMaxY > bMinY;
        }
    }
}
