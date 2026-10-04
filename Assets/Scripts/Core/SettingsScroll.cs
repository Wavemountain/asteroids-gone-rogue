namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Settings list taller than the panel. Rows keep a deck-readable height
    /// and the viewport follows the focused row. Anchors are panel fractions
    /// unless a method says viewport space (0–1 of the masked area).
    /// </summary>
    public static class SettingsScroll
    {
        public const float ViewportTop = 0.86f;
        public const float ViewportBottom = 0.05f;
        public const float RowHeight = 0.076f;
        public const float SectionHeight = 0.26f;
        public const float RowGap = 0.008f;
        public const int RowFont = 18;
        public const float TextInset = 0.12f;

        public static float ViewportSpan
        {
            get { return ViewportTop - ViewportBottom; }
        }

        public static float Extent(SettingsRowId id)
        {
            if (SettingsRows.Role(id) == SettingsRowRole.Section)
            {
                return SectionHeight;
            }

            return RowHeight;
        }

        public static float ContentHeight()
        {
            int count = SettingsRows.Count;
            if (count <= 0)
            {
                return 0f;
            }

            float sum = 0f;
            for (int index = 0; index < count; index++)
            {
                sum += Extent(SettingsRows.At(index));
            }

            sum += RowGap * (count - 1);
            return sum;
        }

        /// <summary>
        /// Unscrolled band in panel anchors. Later rows sit below the viewport.
        /// </summary>
        public static void ContentBand(int index, out float bottom, out float top)
        {
            int count = SettingsRows.Count;
            if (count <= 0)
            {
                bottom = ViewportBottom;
                top = ViewportTop;
                return;
            }

            int clamped = index;
            if (clamped < 0)
            {
                clamped = 0;
            }

            if (clamped >= count)
            {
                clamped = count - 1;
            }

            float cursor = ViewportTop;
            for (int row = 0; row < count; row++)
            {
                float height = Extent(SettingsRows.At(row));
                float rowTop = cursor;
                float rowBottom = cursor - height;
                if (row == clamped)
                {
                    bottom = rowBottom;
                    top = rowTop;
                    return;
                }

                cursor = rowBottom - RowGap;
            }

            bottom = ViewportBottom;
            top = ViewportTop;
        }

        /// <summary>
        /// Positive shift moves content up so a low row enters the viewport.
        /// </summary>
        public static float ShiftForFocus(int focusIndex)
        {
            float bottom;
            float top;
            ContentBand(focusIndex, out bottom, out top);
            float shift = 0f;
            if (bottom < ViewportBottom)
            {
                shift = ViewportBottom - bottom;
            }

            if (top + shift > ViewportTop)
            {
                shift = ViewportTop - top;
            }

            if (shift < 0f)
            {
                shift = 0f;
            }

            return shift;
        }

        public static void VisibleBand(int index, int focusIndex, out float bottom, out float top)
        {
            ContentBand(index, out bottom, out top);
            float shift = ShiftForFocus(focusIndex);
            bottom += shift;
            top += shift;
        }

        public static void ViewportBand(int index, int focusIndex, out float bottom, out float top)
        {
            float panelBottom;
            float panelTop;
            VisibleBand(index, focusIndex, out panelBottom, out panelTop);
            float span = ViewportSpan;
            if (span < 0.0001f)
            {
                span = 0.0001f;
            }

            bottom = (panelBottom - ViewportBottom) / span;
            top = (panelTop - ViewportBottom) / span;
        }

        public static bool IntersectsViewport(float viewportBottom, float viewportTop)
        {
            return viewportTop > 0.001f && viewportBottom < 0.999f;
        }

        /// <summary>
        /// True when the viewport-space band is fully inside the mask.
        /// A focused row is shifted until this holds, so pad focus is never clipped.
        /// </summary>
        public static bool ContainsBand(float bottom, float top)
        {
            return bottom >= -0.001f && top <= 1.001f && top > bottom;
        }

        public static void RowX(SettingsRowId id, out float minX, out float maxX)
        {
            if (id == SettingsRowId.Close)
            {
                minX = 0.22f;
                maxX = 0.78f;
                return;
            }

            minX = SettingsMeasure.RowMinX;
            maxX = SettingsMeasure.RowMaxX;
        }
    }
}
