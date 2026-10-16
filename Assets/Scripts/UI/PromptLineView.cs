using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Remembers the last painted token string so a scheme change can rebuild
    /// without the caller keeping a copy.
    /// </summary>
    public sealed class PromptLineMark : MonoBehaviour
    {
        public string Source;
        public string Stamp;
        public bool LockMetrics;
    }

    /// <summary>
    /// Inline prompt icons under a legacy Text. Missing sprites become the
    /// short label. No pulse, so Reduce effects stays quiet.
    /// </summary>
    public static class PromptLineView
    {
        private const string SegmentPrefix = "PSeg";
        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();
        private static readonly HashSet<string> Missing = new HashSet<string>();

        private struct PlayBox
        {
            public bool Ready;
            public float ScreenBox;
            public Font Font;
            public int FontSize;
            public FontStyle Style;
            public Vector2 AnchorMin;
            public Vector2 AnchorMax;
            public Vector2 Pivot;
            public Vector3 LocalScale;
        }

        private static PlayBox _play;
        private static bool _playLocked;

        public static void Paint(Text host, string source, InputScheme scheme, float canvasScale)
        {
            PromptLineMark existing = host != null ? host.GetComponent<PromptLineMark>() : null;
            bool locked = existing != null && existing.LockMetrics;
            Paint(host, source, scheme, canvasScale, locked);
        }

        /// <summary>
        /// lockMetrics keeps the host font, material, and scale. The wave-clear
        /// hint uses it so the row is not shrunk or scaled away from the wave-1 hint.
        /// </summary>
        public static void Paint(Text host, string source, InputScheme scheme, float canvasScale, bool lockMetrics)
        {
            if (host == null)
            {
                return;
            }

            PromptLineMark mark = host.GetComponent<PromptLineMark>();
            if (mark == null)
            {
                mark = host.gameObject.AddComponent<PromptLineMark>();
            }

            string safe = source ?? string.Empty;
            int fontSize = host.fontSize;
            float hostWidth = 0f;
            float hostHeight = 0f;
            if (host.rectTransform != null)
            {
                hostWidth = host.rectTransform.rect.width;
                hostHeight = host.rectTransform.rect.height;
            }

            string stamp = (lockMetrics ? "L" : "F")
                + "|"
                + ((int)scheme).ToString()
                + "|"
                + canvasScale.ToString("0.000")
                + "|"
                + fontSize.ToString()
                + "|"
                + hostWidth.ToString("0.0")
                + "|"
                + hostHeight.ToString("0.0")
                + "|"
                + BindingMap.Revision.ToString()
                + "|"
                + safe;
            if (mark.Stamp == stamp)
            {
                return;
            }

            mark.Source = safe;
            mark.Stamp = stamp;
            mark.LockMetrics = lockMetrics;
            host.text = string.Empty;
            host.raycastTarget = false;
            Clear(host.transform);
            if (safe.Length == 0)
            {
                return;
            }

            float iconPx = PromptLayout.IconCanvasPx(canvasScale);
            bool hi = PromptCatalog.UseHi(canvasScale);
            string[] lines = safe.Split('\n');
            int lineCount = lines.Length;
            // Kenney Future's preferred line is taller than 1.15em. A short box
            // wraps one segment onto a second line and the gate calls that overflow.
            // The wave-clear row is the same hint container as wave 1. Its box is
            // the font line (about 1.4em), not the 18px icon target.
            float lineHeight = host.fontSize * 1.4f;
            if (!lockMetrics && iconPx > lineHeight)
            {
                lineHeight = iconPx;
            }

            if (lineHeight < 1f)
            {
                lineHeight = iconPx > 1f ? iconPx : 1f;
            }

            bool centered = IsCenter(host.alignment);
            bool upper = IsUpper(host.alignment);
            float top = 0f;
            if (upper && hostHeight > 2f)
            {
                top = (hostHeight * 0.5f) - (lineHeight * 0.5f);
            }
            else
            {
                top = (lineCount - 1) * lineHeight * 0.5f;
            }

            for (int lineIndex = 0; lineIndex < lineCount; lineIndex++)
            {
                PromptPiece[] pieces = PromptText.Resolve(lines[lineIndex], scheme);
                float[] widths = MeasurePieces(host, pieces, scheme, hi, iconPx);
                float gap = host.fontSize * 0.12f;
                float rowWidth = PromptLayout.RowWidth(widths, gap);
                float fit = 1f;
                if (!lockMetrics && hostWidth > 2f && hostHeight > 2f)
                {
                    fit = PromptLayout.ScaleToFit(rowWidth, lineHeight, hostWidth, hostHeight);
                }

                float drawIcon = iconPx * fit;
                float drawGap = gap * fit;
                float drawLine = lineHeight * fit;
                int drawFont = host.fontSize;
                if (fit < 0.999f)
                {
                    drawFont = (int)(host.fontSize * fit);
                    if (drawFont < 1)
                    {
                        drawFont = 1;
                    }
                }

                float fittedRow = rowWidth * fit;
                float cursorX = centered ? -fittedRow * 0.5f : LeftOrigin(hostWidth);
                float cursorY = top - (lineIndex * drawLine);
                float leftLimit = hostWidth > 2f ? -hostWidth * 0.5f : -100000f;
                float rightLimit = hostWidth > 2f ? hostWidth * 0.5f : 100000f;
                int pieceCount = pieces.Length;
                for (int pieceIndex = 0; pieceIndex < pieceCount; pieceIndex++)
                {
                    float pieceWidth = widths[pieceIndex] * fit;
                    if (pieceWidth <= 0.5f)
                    {
                        cursorX += drawGap;
                        continue;
                    }

                    if (cursorX < leftLimit)
                    {
                        float overflow = leftLimit - cursorX;
                        if (overflow >= pieceWidth - 0.5f)
                        {
                            cursorX += pieceWidth + drawGap;
                            continue;
                        }

                        pieceWidth -= overflow;
                        cursorX = leftLimit;
                    }

                    if (cursorX >= rightLimit - 0.5f)
                    {
                        break;
                    }

                    if (cursorX + pieceWidth > rightLimit)
                    {
                        pieceWidth = rightLimit - cursorX;
                    }

                    Place(host, pieces[pieceIndex], scheme, hi, drawIcon, drawFont, cursorX, cursorY, pieceWidth, drawLine, lockMetrics);
                    cursorX += pieceWidth + drawGap;
                }
            }
        }

        /// <summary>
        /// Records the wave-1 PSegT the capture gate stores as HintBoxPx:
        /// rect height times |lossyScale|, plus the font, size, anchors, pivot,
        /// and scale that produced it.
        /// </summary>
        public static void CapturePlayBox(Text host)
        {
            if (_playLocked)
            {
                return;
            }

            RememberSegment(TallestSegment(host), false);
        }

        /// <summary>
        /// The capture gate's own wave-1 measurement. Later wave-1 layouts
        /// (death settle, the wave-clear row) must not replace it.
        /// </summary>
        public static void CapturePlaySegment(Text segment)
        {
            RememberSegment(segment, true);
        }

        /// <summary>
        /// Writes the captured wave-1 segment onto every wave-clear PSegT.
        /// Local height is the stored screen box divided by the current
        /// |lossyScale|, so HintBoxPx matches even when the canvas scale changed.
        /// </summary>
        public static void ApplyPlayBox(Text host)
        {
            if (!_play.Ready || host == null)
            {
                return;
            }

            Text[] parts = host.GetComponentsInChildren<Text>(true);
            if (parts == null)
            {
                return;
            }

            for (int index = 0; index < parts.Length; index++)
            {
                Text part = parts[index];
                if (part == null || part.gameObject == null || part.gameObject.name != SegmentPrefix + "T")
                {
                    continue;
                }

                RectTransform rect = part.rectTransform;
                if (rect == null)
                {
                    continue;
                }

                if (_play.Font != null)
                {
                    part.font = _play.Font;
                }

                if (_play.FontSize > 0)
                {
                    part.fontSize = _play.FontSize;
                }

                part.fontStyle = _play.Style;
                rect.anchorMin = _play.AnchorMin;
                rect.anchorMax = _play.AnchorMax;
                rect.pivot = _play.Pivot;
                rect.localScale = _play.LocalScale;
                float local = PromptLayout.LocalHeight(_play.ScreenBox, rect.lossyScale.y);
                Vector2 size = rect.sizeDelta;
                size.y = local;
                rect.sizeDelta = size;
            }
        }

        public static void Restyle(Text host, InputScheme scheme, float canvasScale)
        {
            if (host == null)
            {
                return;
            }

            PromptLineMark mark = host.GetComponent<PromptLineMark>();
            if (mark == null || mark.Source == null)
            {
                return;
            }

            bool locked = mark.LockMetrics;
            mark.Stamp = string.Empty;
            Paint(host, mark.Source, scheme, canvasScale, locked);
        }

        private static void Place(
            Text host,
            PromptPiece piece,
            InputScheme scheme,
            bool hi,
            float iconPx,
            int fontSize,
            float x,
            float y,
            float width,
            float lineHeight,
            bool lockMetrics)
        {
            if (piece.Kind == PromptPieceKind.Icon)
            {
                Sprite sprite = Load(scheme, piece.Action, hi);
                if (sprite != null)
                {
                    GameObject iconObject = new GameObject(SegmentPrefix + "I");
                    iconObject.transform.SetParent(host.transform, false);
                    RectTransform iconRect = iconObject.AddComponent<RectTransform>();
                    Image image = iconObject.AddComponent<Image>();
                    image.sprite = sprite;
                    image.preserveAspect = true;
                    image.raycastTarget = false;
                    image.color = piece.Important ? UiTheme.Primary : UiTheme.Accent;
                    iconRect.anchorMin = new Vector2(0.5f, 0.5f);
                    iconRect.anchorMax = new Vector2(0.5f, 0.5f);
                    iconRect.pivot = new Vector2(0f, 0.5f);
                    float iconW = iconPx;
                    float iconH = iconPx;
                    float iconX = x;
                    float iconY = y;
                    if (!ClampToHost(host, ref iconX, ref iconY, ref iconW, ref iconH))
                    {
                        UnityEngine.Object.DestroyImmediate(iconObject);
                        return;
                    }

                    iconRect.sizeDelta = new Vector2(iconW, iconH);
                    iconRect.anchoredPosition = new Vector2(iconX, iconY);
                    return;
                }
            }

            string label = piece.Kind == PromptPieceKind.Icon
                ? PromptText.Fallback(scheme, piece.Action)
                : piece.Text;
            if (string.IsNullOrEmpty(label))
            {
                return;
            }

            GameObject textObject = new GameObject(SegmentPrefix + "T");
            textObject.transform.SetParent(host.transform, false);
            RectTransform textRect = textObject.AddComponent<RectTransform>();
            Text text = textObject.AddComponent<Text>();
            text.font = host.font;
            text.fontSize = fontSize > 0 ? fontSize : host.fontSize;
            text.fontStyle = host.fontStyle;
            text.color = host.color;
            text.alignment = TextAnchor.MiddleLeft;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            text.supportRichText = false;
            text.text = label;
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.pivot = new Vector2(0f, 0.5f);
            float textWidth = width > 1f ? width : iconPx;
            float textHeight = lineHeight;
            float textX = x;
            float textY = y;
            if (!ClampToHost(host, ref textX, ref textY, ref textWidth, ref textHeight))
            {
                UnityEngine.Object.DestroyImmediate(textObject);
                return;
            }

            textRect.localScale = Vector3.one;
            if (lockMetrics)
            {
                float row = host.fontSize * 1.4f;
                if (row < 1f)
                {
                    row = lineHeight;
                }

                // The wave-clear row copies the wave-1 segment's screen box.
                // An icon-sized or clamped host (18px, or 18 * 0.489 = 8.8)
                // is not that box.
                float play = PlayLocalHeight(host);
                if (play > row)
                {
                    row = play;
                }

                if (textHeight + 0.5f < row)
                {
                    textHeight = row;
                }

                textX = Mathf.Round(textX);
                textY = Mathf.Round(textY);
                textWidth = Mathf.Round(textWidth);
                text.fontSize = host.fontSize > 0 ? host.fontSize : text.fontSize;
            }

            textRect.sizeDelta = new Vector2(textWidth, textHeight);
            textRect.anchoredPosition = new Vector2(textX, textY);
            if (!lockMetrics)
            {
                int size = text.fontSize;
                while (size > 1 && (text.preferredWidth > textWidth + 1f || text.preferredHeight > textHeight + 1f))
                {
                    size--;
                    text.fontSize = size;
                }
            }
        }

        /// <summary>
        /// Keep a segment inside the hint rect. Growing to preferredHeight
        /// pushed PSegT out of the parent and off the bottom of the screen.
        /// </summary>
        private static bool ClampToHost(Text host, ref float x, ref float y, ref float width, ref float height)
        {
            if (host == null || host.rectTransform == null)
            {
                return width > 0.5f && height > 0.5f;
            }

            float hostWidth = host.rectTransform.rect.width;
            float hostHeight = host.rectTransform.rect.height;
            if (hostWidth <= 2f || hostHeight <= 2f)
            {
                return width > 0.5f && height > 0.5f;
            }

            float halfW = hostWidth * 0.5f;
            float halfH = hostHeight * 0.5f;
            float left = x;
            float right = x + width;
            float bottom = y - (height * 0.5f);
            float top = y + (height * 0.5f);
            if (left < -halfW)
            {
                left = -halfW;
            }

            if (right > halfW)
            {
                right = halfW;
            }

            if (bottom < -halfH)
            {
                bottom = -halfH;
            }

            if (top > halfH)
            {
                top = halfH;
            }

            width = right - left;
            height = top - bottom;
            x = left;
            y = (bottom + top) * 0.5f;
            return width > 0.5f && height > 0.5f;
        }

        private static float[] MeasurePieces(Text host, PromptPiece[] pieces, InputScheme scheme, bool hi, float iconPx)
        {
            int count = pieces == null ? 0 : pieces.Length;
            float[] widths = new float[count];
            for (int index = 0; index < count; index++)
            {
                PromptPiece piece = pieces[index];
                if (piece.Kind == PromptPieceKind.Icon)
                {
                    Sprite sprite = Load(scheme, piece.Action, hi);
                    if (sprite != null)
                    {
                        widths[index] = iconPx;
                    }
                    else
                    {
                        widths[index] = MeasureText(host, PromptText.Fallback(scheme, piece.Action));
                    }
                }
                else
                {
                    widths[index] = MeasureText(host, piece.Text);
                }
            }

            return widths;
        }

        private static float MeasureText(Text host, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return 0f;
            }

            try
            {
                TextGenerator generator = new TextGenerator();
                TextGenerationSettings settings = host.GetGenerationSettings(new Vector2(4096f, 256f));
                settings.horizontalOverflow = HorizontalWrapMode.Overflow;
                settings.verticalOverflow = VerticalWrapMode.Overflow;
                float measured = generator.GetPreferredWidth(value, settings);
                if (settings.scaleFactor > 0.01f)
                {
                    measured /= settings.scaleFactor;
                }

                if (measured > 0.5f)
                {
                    // Preferred width on the live Text runs a few pixels past the generator.
                    return measured + 6f;
                }
            }
            catch (System.Exception)
            {
                // A text with no canvas yet still needs a width so the row can lay out.
            }

            return value.Length * host.fontSize * 0.56f;
        }

        private static Sprite Load(InputScheme scheme, string action, bool hi)
        {
            string path = PromptCatalog.ResourcePath(scheme, action, hi);
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            Sprite cached;
            if (Cache.TryGetValue(path, out cached))
            {
                return cached;
            }

            if (Missing.Contains(path))
            {
                return null;
            }

            Sprite loaded = Resources.Load<Sprite>(path);
            if (loaded == null)
            {
                Missing.Add(path);
                return null;
            }

            Cache[path] = loaded;
            return loaded;
        }

        private static void Clear(Transform host)
        {
            for (int index = host.childCount - 1; index >= 0; index--)
            {
                Transform child = host.GetChild(index);
                if (child != null && child.name.StartsWith(SegmentPrefix))
                {
                    UnityEngine.Object.DestroyImmediate(child.gameObject);
                }
            }
        }

        private static void RememberSegment(Text segment, bool lockSample)
        {
            if (segment == null || segment.rectTransform == null)
            {
                return;
            }

            RectTransform rect = segment.rectTransform;
            float box = PromptLayout.ScreenBox(rect.rect.height, rect.lossyScale.y);
            if (box <= 1f)
            {
                return;
            }

            _play.Ready = true;
            _play.ScreenBox = box;
            _play.Font = segment.font;
            _play.FontSize = segment.fontSize;
            _play.Style = segment.fontStyle;
            _play.AnchorMin = rect.anchorMin;
            _play.AnchorMax = rect.anchorMax;
            _play.Pivot = rect.pivot;
            _play.LocalScale = rect.localScale;
            if (lockSample)
            {
                _playLocked = true;
            }
        }

        private static float PlayLocalHeight(Text host)
        {
            if (!_play.Ready || host == null || host.rectTransform == null)
            {
                return 0f;
            }

            return PromptLayout.LocalHeight(_play.ScreenBox, host.rectTransform.lossyScale.y);
        }

        private static Text TallestSegment(Text host)
        {
            if (host == null)
            {
                return null;
            }

            Text[] parts = host.GetComponentsInChildren<Text>(true);
            if (parts == null)
            {
                return null;
            }

            Text best = null;
            float bestBox = -1f;
            for (int index = 0; index < parts.Length; index++)
            {
                Text part = parts[index];
                if (part == null || part.gameObject == null || part.gameObject.name != SegmentPrefix + "T")
                {
                    continue;
                }

                if (!part.gameObject.activeInHierarchy)
                {
                    continue;
                }

                RectTransform rect = part.rectTransform;
                if (rect == null)
                {
                    continue;
                }

                float box = PromptLayout.ScreenBox(rect.rect.height, rect.lossyScale.y);
                if (box > bestBox)
                {
                    bestBox = box;
                    best = part;
                }
            }

            return best;
        }

        private static float LeftOrigin(float hostWidth)
        {
            if (hostWidth < 2f)
            {
                return 0f;
            }

            return -hostWidth * 0.5f;
        }

        private static bool IsCenter(TextAnchor alignment)
        {
            return alignment == TextAnchor.MiddleCenter
                || alignment == TextAnchor.UpperCenter
                || alignment == TextAnchor.LowerCenter;
        }

        private static bool IsUpper(TextAnchor alignment)
        {
            return alignment == TextAnchor.UpperLeft
                || alignment == TextAnchor.UpperCenter
                || alignment == TextAnchor.UpperRight;
        }
    }
}
