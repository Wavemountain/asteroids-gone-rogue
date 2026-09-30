namespace AsteroidsGoneRogue
{
    public enum SettingsRowId
    {
        Language = 0,
        Controls = 1,
        Close = 2,
    }

    public enum SettingsRowRole
    {
        Value = 0,
        Section = 1,
        Action = 2,
    }

    /// <summary>
    /// Ordered settings rows. Later options (music, sfx, shake, hint mode,
    /// text size, restart confirm, pad source) are added by appending this list
    /// and a matching builder case. Sections are drawn but skipped by the pad.
    /// </summary>
    public static class SettingsRows
    {
        public const float ContentTop = 0.86f;
        public const float ContentBottom = 0.08f;
        public const float RowGap = 0.018f;
        public const float SectionWeight = 2.6f;
        public const float RowWeight = 1f;

        public static readonly SettingsRowId[] Order = new SettingsRowId[]
        {
            SettingsRowId.Language,
            SettingsRowId.Controls,
            SettingsRowId.Close,
        };

        public static int Count
        {
            get { return Order.Length; }
        }

        public static SettingsRowRole Role(SettingsRowId id)
        {
            if (id == SettingsRowId.Language)
            {
                return SettingsRowRole.Value;
            }

            if (id == SettingsRowId.Close)
            {
                return SettingsRowRole.Action;
            }

            return SettingsRowRole.Section;
        }

        public static bool IsNavigable(SettingsRowId id)
        {
            return Role(id) != SettingsRowRole.Section;
        }

        public static bool IsValue(SettingsRowId id)
        {
            return Role(id) == SettingsRowRole.Value;
        }

        public static int ClampIndex(int index)
        {
            if (Order.Length == 0)
            {
                return 0;
            }

            if (index < 0)
            {
                return 0;
            }

            int last = Order.Length - 1;
            return index > last ? last : index;
        }

        public static SettingsRowId At(int index)
        {
            return Order[ClampIndex(index)];
        }

        public static int FirstNavigable()
        {
            for (int index = 0; index < Order.Length; index++)
            {
                if (IsNavigable(Order[index]))
                {
                    return index;
                }
            }

            return 0;
        }

        public static float Weight(SettingsRowId id)
        {
            return Role(id) == SettingsRowRole.Section ? SectionWeight : RowWeight;
        }

        /// <summary>
        /// Vertical band inside the settings panel, top to bottom in Order.
        /// Positive Move delta walks down the list and skips sections.
        /// </summary>
        public static void RowBand(int index, out float y0, out float y1)
        {
            int count = Order.Length;
            if (count <= 0)
            {
                y0 = ContentBottom;
                y1 = ContentTop;
                return;
            }

            int clamped = ClampIndex(index);
            float weightSum = 0f;
            for (int band = 0; band < count; band++)
            {
                weightSum += Weight(Order[band]);
            }

            int gaps = count - 1;
            float span = ContentTop - ContentBottom - (RowGap * gaps);
            if (span < 0f)
            {
                span = 0f;
            }

            float cursor = ContentTop;
            for (int row = 0; row < count; row++)
            {
                float share = weightSum <= 0f ? 0f : Weight(Order[row]) / weightSum;
                float height = span * share;
                float rowTop = cursor;
                float rowBottom = cursor - height;
                if (row == clamped)
                {
                    y0 = rowBottom;
                    y1 = rowTop;
                    return;
                }

                cursor = rowBottom - RowGap;
            }

            y0 = ContentBottom;
            y1 = ContentTop;
        }

        public static int Move(int index, int delta)
        {
            if (Order.Length == 0 || delta == 0)
            {
                return ClampIndex(index);
            }

            int dir = delta > 0 ? 1 : -1;
            int steps = delta > 0 ? delta : -delta;
            int current = ClampIndex(index);
            if (!IsNavigable(Order[current]))
            {
                current = NearestNavigable(current, dir);
            }

            for (int step = 0; step < steps; step++)
            {
                int next = NextNavigable(current, dir);
                if (next == current)
                {
                    break;
                }

                current = next;
            }

            return current;
        }

        private static int NearestNavigable(int index, int dir)
        {
            int forward = NextNavigable(index, dir);
            if (forward != index)
            {
                return forward;
            }

            return NextNavigable(index, -dir);
        }

        private static int NextNavigable(int index, int dir)
        {
            int count = Order.Length;
            for (int step = 1; step < count; step++)
            {
                int candidate = index + (dir * step);
                if (candidate < 0 || candidate >= count)
                {
                    return index;
                }

                if (IsNavigable(Order[candidate]))
                {
                    return candidate;
                }
            }

            return index;
        }
    }
}
