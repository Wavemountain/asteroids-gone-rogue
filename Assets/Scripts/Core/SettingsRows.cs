namespace AsteroidsGoneRogue
{
    public enum SettingsRowId
    {
        Language = 0,
        Controls = 1,
        Close = 2,
        Music = 3,
        Sfx = 4,
        Mute = 5,
        ScreenShake = 6,
        HintMode = 7,
        HintSize = 8,
        ConfirmAbort = 9,
        ConfirmNewRun = 10,
        PadNav = 11,
    }

    public enum SettingsRowRole
    {
        Value = 0,
        Section = 1,
        Action = 2,
    }

    /// <summary>
    /// Ordered settings rows. Sections are drawn but skipped by the pad.
    /// Music, SFX, and Mute are value rows. Later options append this list.
    /// </summary>
    public static class SettingsRows
    {
        public const float ContentTop = 0.86f;
        public const float ContentBottom = 0.05f;
        public const float RowGap = 0.012f;
        public const float SectionWeight = 7.2f;
        public const float RowWeight = 1f;
        public const float VolumeStep = 0.1f;

        public static readonly SettingsRowId[] Order = new SettingsRowId[]
        {
            SettingsRowId.Language,
            SettingsRowId.Music,
            SettingsRowId.Sfx,
            SettingsRowId.Mute,
            SettingsRowId.ScreenShake,
            SettingsRowId.HintMode,
            SettingsRowId.HintSize,
            SettingsRowId.ConfirmAbort,
            SettingsRowId.ConfirmNewRun,
            SettingsRowId.PadNav,
            SettingsRowId.Controls,
            SettingsRowId.Close,
        };

        public static int Count
        {
            get { return Order.Length; }
        }

        public static SettingsRowRole Role(SettingsRowId id)
        {
            if (id == SettingsRowId.Controls)
            {
                return SettingsRowRole.Section;
            }

            if (id == SettingsRowId.Close)
            {
                return SettingsRowRole.Action;
            }

            return SettingsRowRole.Value;
        }

        public static bool IsNavigable(SettingsRowId id)
        {
            return Role(id) != SettingsRowRole.Section;
        }

        public static bool IsValue(SettingsRowId id)
        {
            return Role(id) == SettingsRowRole.Value;
        }

        public static bool IsSlider(SettingsRowId id)
        {
            return id == SettingsRowId.Music || id == SettingsRowId.Sfx;
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

        public static int IndexOf(SettingsRowId id)
        {
            for (int index = 0; index < Order.Length; index++)
            {
                if (Order[index] == id)
                {
                    return index;
                }
            }

            return 0;
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

        public static float ClampVolume(float volume)
        {
            if (volume < 0f)
            {
                return 0f;
            }

            if (volume > 1f)
            {
                return 1f;
            }

            return volume;
        }

        /// <summary>
        /// Pad left/right steps a volume slider by 10 percent and clamps to 0–1.
        /// </summary>
        public static float StepVolume(float current, int direction)
        {
            float delta = 0f;
            if (direction > 0)
            {
                delta = VolumeStep;
            }
            else if (direction < 0)
            {
                delta = -VolumeStep;
            }

            float next = ClampVolume(current + delta);
            return UnityEngine.Mathf.Round(next * 1000f) / 1000f;
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

    /// <summary>
    /// Canvas-unit layout for the settings panel and the achievement ladder.
    /// Matches the 1920x1080 scaler at match 0.5. Char width is the 10px-at-18px
    /// estimate the layout tests already use.
    /// </summary>
    public static class SettingsMeasure
    {
        public const float RefWidth = 1920f;
        public const float RefHeight = 1080f;
        public const float Match = 0.5f;
        public const float CharPxAt18 = 10f;
        public const float LineHeightScale = 1.15f;
        public const float PanelMinX = 0.30f;
        public const float PanelMaxX = 0.70f;
        public const float PanelMinY = 0.12f;
        public const float PanelMaxY = 0.88f;
        public const float RowMinX = 0.06f;
        public const float RowMaxX = 0.94f;
        public const float SliderMinX = 0.40f;
        public const float SliderMaxX = 0.96f;
        public const float BodyMinX = 0.08f;
        public const float BodyMaxX = 0.92f;
        public const float ControlsHeaderInset = 0.05f;
        public const float OldSliderMinUnits = 101f;
        public const float LadderMinX = 0.355f;
        public const float LadderMaxX = 0.478f;
        public const float LadderMinY = 0.905f;
        public const float LadderMaxY = 0.950f;
        public const int LadderFont = 12;
        public const float LadderLineSpacing = 0.85f;

        public static float CanvasScale(float screenWidth, float screenHeight)
        {
            float safeW = screenWidth < 1f ? 1f : screenWidth;
            float safeH = screenHeight < 1f ? 1f : screenHeight;
            float logW = UnityEngine.Mathf.Log(safeW / RefWidth, 2f);
            float logH = UnityEngine.Mathf.Log(safeH / RefHeight, 2f);
            float logAvg = logW + ((logH - logW) * Match);
            return UnityEngine.Mathf.Pow(2f, logAvg);
        }

        public static float CanvasWidth(float screenWidth, float screenHeight)
        {
            return screenWidth / CanvasScale(screenWidth, screenHeight);
        }

        public static float CanvasHeight(float screenWidth, float screenHeight)
        {
            return screenHeight / CanvasScale(screenWidth, screenHeight);
        }

        public static float EstimateWidth(string text, int fontSize)
        {
            if (string.IsNullOrEmpty(text) || fontSize <= 0)
            {
                return 0f;
            }

            return text.Length * CharPxAt18 * fontSize / 18f;
        }

        public static int WrappedLineCount(string text, float boxWidth, int fontSize)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            float charWidth = CharPxAt18 * fontSize / 18f;
            if (charWidth < 0.01f)
            {
                charWidth = 0.01f;
            }

            string[] paragraphs = text.Split('\n');
            int lines = 0;
            for (int paragraph = 0; paragraph < paragraphs.Length; paragraph++)
            {
                string para = paragraphs[paragraph];
                if (para.Length == 0)
                {
                    lines += 1;
                    continue;
                }

                string[] words = para.Split(' ');
                float used = 0f;
                int count = 1;
                for (int wordIndex = 0; wordIndex < words.Length; wordIndex++)
                {
                    float wordWidth = words[wordIndex].Length * charWidth;
                    float space = used > 0f ? charWidth : 0f;
                    if (used + space + wordWidth > boxWidth && used > 0f)
                    {
                        count += 1;
                        used = wordWidth;
                    }
                    else
                    {
                        used += space + wordWidth;
                    }
                }

                lines += count;
            }

            return lines;
        }

        public static float SliderWidth(float screenWidth, float screenHeight)
        {
            float span = (SliderMaxX - SliderMinX) * (RowMaxX - RowMinX) * (PanelMaxX - PanelMinX);
            return span * CanvasWidth(screenWidth, screenHeight);
        }

        public static float ControlsBodyWidth(float screenWidth, float screenHeight)
        {
            float span = (BodyMaxX - BodyMinX) * (PanelMaxX - PanelMinX);
            return span * CanvasWidth(screenWidth, screenHeight);
        }

        public static float ControlsBodyHeight(float screenWidth, float screenHeight)
        {
            float bandTop;
            float bandBottom;
            SettingsRows.RowBand(SettingsRows.IndexOf(SettingsRowId.Controls), out bandBottom, out bandTop);
            float bodySpan = bandTop - ControlsHeaderInset - bandBottom;
            if (bodySpan < 0f)
            {
                bodySpan = 0f;
            }

            float panelSpan = PanelMaxY - PanelMinY;
            return bodySpan * panelSpan * CanvasHeight(screenWidth, screenHeight);
        }

        public static bool ControlsTextFits(string text, float screenWidth, float screenHeight, int fontSize)
        {
            float boxWidth = ControlsBodyWidth(screenWidth, screenHeight);
            float boxHeight = ControlsBodyHeight(screenWidth, screenHeight);
            int lines = WrappedLineCount(text, boxWidth, fontSize);
            return lines * fontSize * LineHeightScale <= boxHeight;
        }

        public static float LadderBoxWidth(float screenWidth, float screenHeight)
        {
            return (LadderMaxX - LadderMinX) * CanvasWidth(screenWidth, screenHeight);
        }

        public static bool LadderLinesFit(string header, string count, float screenWidth, float screenHeight)
        {
            float box = LadderBoxWidth(screenWidth, screenHeight);
            return EstimateWidth(header, LadderFont) <= box && EstimateWidth(count, LadderFont) <= box;
        }

        public static float LadderBoxHeight(float screenWidth, float screenHeight)
        {
            return (LadderMaxY - LadderMinY) * CanvasHeight(screenWidth, screenHeight);
        }

        /// <summary>
        /// Three short lines at <see cref="LadderFont"/> with tightened spacing stay inside the top-bar box.
        /// </summary>
        public static bool LadderBlockFits(string text, float screenWidth, float screenHeight)
        {
            float boxWidth = LadderBoxWidth(screenWidth, screenHeight);
            float boxHeight = LadderBoxHeight(screenWidth, screenHeight);
            if (boxWidth <= 1f || boxHeight <= 1f)
            {
                return false;
            }

            int lines = WrappedLineCount(text, boxWidth, LadderFont);
            float lineHeight = LadderFont * 1.2f * LadderLineSpacing;
            return lines * lineHeight <= boxHeight;
        }
    }
}
