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
        public const float UtilityMinY = 0.575f;
        public const float UtilityMaxY = 0.688f;
        public const float BossMinY = 0.492f;
        public const float BossMaxY = 0.560f;
        public const float HealthMinY = 0.250f;
        public const float HealthMaxY = 0.475f;
        // Play hint sits above the bottom health rack, boss bar, utility plate,
        // and doctrine badge. Hangar keeps the footer at y 0.008–0.072.
        public const float PlayHintMinX = 0.14f;
        public const float PlayHintMinY = 0.280f;
        public const float PlayHintMaxX = 0.86f;
        public const float PlayHintMaxY = 0.344f;
        public const float AbortWidthPx = 220f;
        public const float AbortHeightPx = 36f;
        public const float AbortMaxX = 0.985f;
        public const float AbortMaxY = 0.070f;

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
    }
}
