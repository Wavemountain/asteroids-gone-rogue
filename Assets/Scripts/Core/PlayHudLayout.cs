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
        // Health and boss keep the 62ee6bf heights (0.082 and 0.040) and sit
        // just above the bottom hint row so the two rows do not overlap.
        public const float HealthMinY = 0.050f;
        public const float HealthMaxY = 0.132f;
        public const float BossMinY = 0.140f;
        public const float BossMaxY = 0.180f;
        // Tools panel and doctrine badge are the 62ee6bf rects.
        public const float UtilityMinX = 0.74f;
        public const float UtilityMinY = 0.085f;
        public const float UtilityMaxX = 0.985f;
        public const float UtilityMaxY = 0.205f;
        public const float DoctrineMinX = 0.74f;
        public const float DoctrineMinY = 0.21f;
        public const float DoctrineMaxX = 0.985f;
        public const float DoctrineMaxY = 0.255f;
        // Combat hint is its own bottom row, left of the abort slot.
        // 0.034 leaves 0.016 under HealthMinY: 12.8 px at 800, 17.3 at 1080, 23.0 at 1440.
        // Hangar footer is GameUi.HangarHintMinY–HangarHintMaxY, under the ship preview.
        public const float PlayHintMinX = 0.04f;
        public const float PlayHintMinY = 0.006f;
        public const float PlayHintMaxX = 0.72f;
        public const float PlayHintMaxY = 0.034f;
        // Abort/Hangar ghost is the 62ee6bf slot: under the tools panel, right of the hint.
        public const float AbortMinX = 0.74f;
        public const float AbortMinY = 0.012f;
        public const float AbortMaxX = 0.985f;
        public const float AbortMaxY = 0.072f;
        // Hud is the amber score. HudCaption is the wave line under it.
        // 0.012 of height is 9.6 px at 800, 13.0 px at 1080, 17.3 px at 1440.
        public const float CaptionMinY = 0.946f;
        public const float CaptionMaxY = 0.964f;
        public const float ScoreMinY = 0.970f;
        public const float ScoreMaxY = 0.998f;

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
            // Normalised anchors, so 1280x800 and 3440x1440 share this slot.
            minX = AbortMinX;
            minY = AbortMinY;
            maxX = AbortMaxX;
            maxY = AbortMaxY;
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

            if (!ScoreClearsCaption())
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

        private static bool ScoreClearsCaption()
        {
            if (!Stacked(CaptionMinY, CaptionMaxY) || !Stacked(CaptionMaxY, ScoreMinY) || !Stacked(ScoreMinY, ScoreMaxY))
            {
                return false;
            }

            float[] heights = { 800f, 1080f, 1440f };
            for (int heightIndex = 0; heightIndex < heights.Length; heightIndex++)
            {
                float gap = (ScoreMinY - CaptionMaxY) * heights[heightIndex];
                if (gap < 4f)
                {
                    return false;
                }
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
