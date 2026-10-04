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

        public const int ClickIgnore = 0;
        public const int ClickFocus = 1;
        public const int ClickActivate = 2;
        public const float ThumbMin = 0.12f;

        public static float MaxShift()
        {
            float extra = ContentHeight() - ViewportSpan;
            if (extra < 0f)
            {
                return 0f;
            }

            return extra;
        }

        public static float ClampShift(float shift)
        {
            if (shift < 0f)
            {
                return 0f;
            }

            float max = MaxShift();
            if (shift > max)
            {
                return max;
            }

            return shift;
        }

        /// <summary>
        /// One wheel notch moves one row. Positive scrollY reveals earlier rows.
        /// </summary>
        public static float Wheel(float shift, float scrollY)
        {
            if (scrollY == 0f)
            {
                return ClampShift(shift);
            }

            float step = RowHeight + RowGap;
            float notches = scrollY;
            if (notches < 0f)
            {
                notches = -notches;
            }

            int count = (int)notches;
            if (count < 1)
            {
                count = 1;
            }

            float delta = step * count;
            if (scrollY > 0f)
            {
                return ClampShift(shift - delta);
            }

            return ClampShift(shift + delta);
        }

        public static void ViewportBandAt(int index, float shift, out float bottom, out float top)
        {
            float panelBottom;
            float panelTop;
            ContentBand(index, out panelBottom, out panelTop);
            float applied = ClampShift(shift);
            panelBottom += applied;
            panelTop += applied;
            float span = ViewportSpan;
            if (span < 0.0001f)
            {
                span = 0.0001f;
            }

            bottom = (panelBottom - ViewportBottom) / span;
            top = (panelTop - ViewportBottom) / span;
        }

        public static int ClickKind(float bottom, float top)
        {
            if (ContainsBand(bottom, top))
            {
                return ClickActivate;
            }

            if (IntersectsViewport(bottom, top))
            {
                return ClickFocus;
            }

            return ClickIgnore;
        }

        public static float ScrollbarSize()
        {
            float content = ContentHeight();
            float view = ViewportSpan;
            if (content <= view || content <= 0.0001f)
            {
                return 1f;
            }

            float size = view / content;
            if (size < ThumbMin)
            {
                size = ThumbMin;
            }

            if (size > 1f)
            {
                size = 1f;
            }

            return size;
        }

        /// <summary>
        /// Scrollbar value 1 is the top of the list (shift 0).
        /// </summary>
        public static float ScrollbarValue(float shift)
        {
            float max = MaxShift();
            if (max <= 0.0001f)
            {
                return 1f;
            }

            return 1f - (ClampShift(shift) / max);
        }

        public static float ShiftFromScrollbar(float value)
        {
            float clamped = value;
            if (clamped < 0f)
            {
                clamped = 0f;
            }

            if (clamped > 1f)
            {
                clamped = 1f;
            }

            return ClampShift((1f - clamped) * MaxShift());
        }
    }
}
