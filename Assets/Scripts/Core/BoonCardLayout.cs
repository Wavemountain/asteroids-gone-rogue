namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Screen rects for the three boon cards. The hangar ship is a RenderTexture
    /// on ShipPreviewCanvas (overlay sort 80). These cards must sit on a higher
    /// overlay or the right card draws behind that ship. Not part of HangarPadNav.
    /// </summary>
    public static class BoonCardLayout
    {
        public const string CanvasName = "BoonModalCanvas";
        public const int ModalSortOrder = CanvasOrder.Boon;
        public const int PreviewSortOrder = CanvasOrder.ShipPreview;
        public const int Count = 3;
        public const float PanelMinX = 0.12f;
        public const float PanelMinY = 0.20f;
        public const float PanelMaxX = 0.88f;
        public const float PanelMaxY = 0.80f;
        public const float HeaderMinY = 0.78f;
        public const float CardInsetX = 0.03f;
        public const float CardWidth = 0.30f;
        public const float CardStep = 0.32f;
        public const float CardMinY = 0.18f;
        public const float CardMaxY = 0.74f;
        public const float TitleMinX = 0.06f;
        public const float TitleMinY = 0.80f;
        public const float TitleMaxX = 0.94f;
        public const float TitleMaxY = 0.96f;
        public const float HintMinX = 0.06f;
        public const float HintMinY = 0.03f;
        public const float HintMaxX = 0.94f;
        public const float HintMaxY = 0.15f;
        public const float LabelMinX = 0.04f;
        public const float LabelMinY = 0.06f;
        public const float LabelMaxX = 0.96f;
        public const float LabelMaxY = 0.94f;
        public const int CardFont = 16;
        public const int TitleFont = 22;
        public const int HintFont = 14;
        public const int MinFont = 10;
        public const float RefWidth = 1920f;
        public const float RefHeight = 1080f;
        public const float Match = 0.5f;

        public static void CardAnchors(int index, out float minX, out float minY, out float maxX, out float maxY)
        {
            int slot = index;
            if (slot < 0)
            {
                slot = 0;
            }

            if (slot >= Count)
            {
                slot = Count - 1;
            }

            float left = CardInsetX + (slot * CardStep);
            minX = left;
            minY = CardMinY;
            maxX = left + CardWidth;
            maxY = CardMaxY;
        }

        public static void ScreenRect(int index, out float minX, out float minY, out float maxX, out float maxY)
        {
            float localMinX;
            float localMinY;
            float localMaxX;
            float localMaxY;
            CardAnchors(index, out localMinX, out localMinY, out localMaxX, out localMaxY);
            float panelW = PanelMaxX - PanelMinX;
            float panelH = PanelMaxY - PanelMinY;
            minX = PanelMinX + (localMinX * panelW);
            minY = PanelMinY + (localMinY * panelH);
            maxX = PanelMinX + (localMaxX * panelW);
            maxY = PanelMinY + (localMaxY * panelH);
        }

        public static void PixelRect(
            int index,
            float screenW,
            float screenH,
            out float minX,
            out float minY,
            out float maxX,
            out float maxY)
        {
            float normMinX;
            float normMinY;
            float normMaxX;
            float normMaxY;
            ScreenRect(index, out normMinX, out normMinY, out normMaxX, out normMaxY);
            minX = normMinX * screenW;
            minY = normMinY * screenH;
            maxX = normMaxX * screenW;
            maxY = normMaxY * screenH;
        }

        public static void PanelChildScreen(
            float childMinX,
            float childMinY,
            float childMaxX,
            float childMaxY,
            out float minX,
            out float minY,
            out float maxX,
            out float maxY)
        {
            float panelW = PanelMaxX - PanelMinX;
            float panelH = PanelMaxY - PanelMinY;
            minX = PanelMinX + (childMinX * panelW);
            minY = PanelMinY + (childMinY * panelH);
            maxX = PanelMinX + (childMaxX * panelW);
            maxY = PanelMinY + (childMaxY * panelH);
        }

        public static bool InsideScreen(float minX, float minY, float maxX, float maxY, float screenW, float screenH)
        {
            return minX >= 0f
                && minY >= 0f
                && maxX <= screenW
                && maxY <= screenH
                && maxX > minX
                && maxY > minY;
        }

        public static bool Overlaps(
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

        public static float CanvasScale(float screenW, float screenH)
        {
            double wide = screenW;
            double high = screenH;
            if (wide < 1.0)
            {
                wide = 1.0;
            }

            if (high < 1.0)
            {
                high = 1.0;
            }

            double logW = System.Math.Log(wide / RefWidth, 2.0);
            double logH = System.Math.Log(high / RefHeight, 2.0);
            return (float)System.Math.Pow(2.0, logW + ((logH - logW) * Match));
        }

        public static float EstimateWidth(string text, int fontSize)
        {
            if (string.IsNullOrEmpty(text) || fontSize <= 0)
            {
                return 0f;
            }

            return text.Length * 10f * fontSize / 18f;
        }

        public static float CardInnerCanvasWidth(float screenW, float screenH)
        {
            float minX;
            float minY;
            float maxX;
            float maxY;
            ScreenRect(0, out minX, out minY, out maxX, out maxY);
            float scale = CanvasScale(screenW, screenH);
            if (scale < 0.0001f)
            {
                scale = 0.0001f;
            }

            float span = (maxX - minX) * (LabelMaxX - LabelMinX);
            return span * screenW / scale;
        }

        public static float CardInnerCanvasHeight(float screenW, float screenH)
        {
            float minX;
            float minY;
            float maxX;
            float maxY;
            ScreenRect(0, out minX, out minY, out maxX, out maxY);
            float scale = CanvasScale(screenW, screenH);
            if (scale < 0.0001f)
            {
                scale = 0.0001f;
            }

            float span = (maxY - minY) * (LabelMaxY - LabelMinY);
            return span * screenH / scale;
        }

        public static float PanelChildCanvasWidth(float childMinX, float childMaxX, float screenW, float screenH)
        {
            float scale = CanvasScale(screenW, screenH);
            if (scale < 0.0001f)
            {
                scale = 0.0001f;
            }

            float panelW = PanelMaxX - PanelMinX;
            float span = (childMaxX - childMinX) * panelW;
            return span * screenW / scale;
        }

        public static int WrappedLineCount(string text, float boxWidth, int fontSize)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            float charWidth = 10f * fontSize / 18f;
            if (charWidth < 0.01f)
            {
                charWidth = 0.01f;
            }

            int lines = 0;
            string[] paras = text.Split('\n');
            for (int paraIndex = 0; paraIndex < paras.Length; paraIndex++)
            {
                string para = paras[paraIndex];
                if (para.Length == 0)
                {
                    lines += 1;
                    continue;
                }

                float used = 0f;
                int count = 1;
                string[] words = para.Split(' ');
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

        public static bool TextFits(string text, float boxWidth, float boxHeight, int fontSize)
        {
            if (fontSize <= 0 || boxWidth <= 0f || boxHeight <= 0f)
            {
                return false;
            }

            if (string.IsNullOrEmpty(text))
            {
                return true;
            }

            string[] chunks = text.Split(new char[] { ' ', '\n' });
            for (int chunkIndex = 0; chunkIndex < chunks.Length; chunkIndex++)
            {
                if (EstimateWidth(chunks[chunkIndex], fontSize) > boxWidth)
                {
                    return false;
                }
            }

            int lines = WrappedLineCount(text, boxWidth, fontSize);
            float lineHeight = fontSize * 1.2f;
            return lines * lineHeight <= boxHeight;
        }

        public static int FitFont(float boxWidth, float boxHeight, string text, int baseSize)
        {
            int size = baseSize;
            if (size < MinFont)
            {
                size = MinFont;
            }

            while (size > MinFont && !TextFits(text, boxWidth, boxHeight, size))
            {
                size -= 1;
            }

            return size;
        }
    }
}
