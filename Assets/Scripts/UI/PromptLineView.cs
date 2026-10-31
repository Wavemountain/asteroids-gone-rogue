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

        /// <summary>
        /// Set only around the hangar footer paint. Wave 1 and wave clear share
        /// the Hint host; they must keep the play material that scores the reference.
        /// </summary>
        public static bool HangarInkPass;
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
            public float RasterScale;
            public Color Color;
            public TextAnchor Alignment;
            public bool AlignByGeometry;
            public float LineSpacing;
            public HorizontalWrapMode Horizontal;
            public VerticalWrapMode Vertical;
            public bool RichText;
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
        /// rect height times |lossyScale|, plus the font, size, anchors, and
        /// pivot. The canvas scale is stored so a later shot can keep the same
        /// pixel size. The segment scale is not stored: legacy dynamic text is
        /// already rasterized at fontSize times canvas.scaleFactor, and a
        /// second scale magnifies those atlas texels into smeared blocks.
        /// </summary>
        public static void CapturePlayBox(Text host)
        {
            if (_playLocked)
            {
                return;
            }

            SnapHintChildren(host);
            RememberSegment(TallestSegment(host), false);
        }

        /// <summary>
        /// The capture gate's own wave-1 measurement. Later wave-1 layouts
        /// (death settle, the wave-clear row) must not replace it.
        /// </summary>
        public static void CapturePlaySegment(Text segment)
        {
            if (segment != null)
            {
                SnapHintText(segment, null, Screen.width, Screen.height);
            }

            RememberSegment(segment, true);
        }

        /// <summary>
        /// Writes the captured wave-1 segment onto every wave-clear PSegT.
        /// The font is the wave-1 font. The material is the shared UI default,
        /// never a font.material instance. fontSize is the real canvas size
        /// that rasterizes at the wave-1 pixel size. localScale stays 1, and
        /// the rect is pixel-aligned. Local height is the stored screen box
        /// divided by |lossyScale| after that scale is 1, so HintBoxPx matches
        /// without scaling the glyphs a second time.
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

                UseDefaultUiMaterial(part);
                part.resizeTextForBestFit = false;
                part.fontStyle = _play.Style;
                part.color = _play.Color;
                part.alignment = _play.Alignment;
                part.alignByGeometry = _play.AlignByGeometry;
                part.lineSpacing = _play.LineSpacing;
                part.horizontalOverflow = _play.Horizontal;
                part.verticalOverflow = _play.Vertical;
                part.supportRichText = _play.RichText;
                float now = RasterScaleOf(part);
                int size = _play.FontSize;
                if (_play.RasterScale > 0.01f && now > 0.01f && Mathf.Abs(now - _play.RasterScale) > 0.02f)
                {
                    int pixels = Mathf.RoundToInt(_play.FontSize * _play.RasterScale);
                    if (pixels < 1)
                    {
                        pixels = 1;
                    }

                    size = Mathf.RoundToInt(pixels / now);
                }

                if (size < 1)
                {
                    size = 1;
                }

                part.fontSize = size;
                part.resizeTextMinSize = size;
                part.resizeTextMaxSize = size;

                rect.anchorMin = _play.AnchorMin;
                rect.anchorMax = _play.AnchorMax;
                rect.pivot = _play.Pivot;
                rect.localScale = Vector3.one;
                float scale = PromptLayout.ScreenBox(1f, rect.lossyScale.y);
                float local = PromptLayout.LocalHeight(_play.ScreenBox, scale);
                if (local < 1f)
                {
                    local = 1f;
                }

                Vector2 pos = rect.anchoredPosition;
                pos.x = Snap(pos.x, scale);
                pos.y = Snap(pos.y, scale);
                rect.anchoredPosition = pos;
                Vector2 sized = rect.sizeDelta;
                sized.x = Snap(sized.x, scale);
                sized.y = Snap(local, scale);
                if (sized.y < 1f)
                {
                    sized.y = 1f;
                }

                rect.sizeDelta = sized;
                RequestGlyphs(part, now);
                SnapHintText(part, null, Screen.width, Screen.height);
            }
        }

        /// <summary>
        /// Moves one hint glyph onto whole pixels. A null camera reads overlay
        /// world corners, which are already screen pixels. A capture camera
        /// maps the same corners through its viewport into the render target,
        /// so the snap matches the image and not the game-view size.
        /// </summary>
        public static void SnapHintText(Text part, Camera cam, float pixelWidth, float pixelHeight)
        {
            if (part == null || part.rectTransform == null || pixelWidth < 2f || pixelHeight < 2f)
            {
                return;
            }

            part.resizeTextForBestFit = false;
            RectTransform rect = part.rectTransform;
            rect.localScale = Vector3.one;
            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            float minX;
            float minY;
            float maxX;
            float maxY;
            if (!HintScreenRect(corners, cam, pixelWidth, pixelHeight, out minX, out minY, out maxX, out maxY))
            {
                return;
            }

            float left = Mathf.Round(minX);
            float bottom = Mathf.Round(minY);
            float right = Mathf.Round(maxX);
            float top = Mathf.Round(maxY);
            if (right < left + 1f)
            {
                right = left + 1f;
            }

            if (top < bottom + 1f)
            {
                top = bottom + 1f;
            }

            float width = maxX - minX;
            float height = maxY - minY;
            float pivotX = rect.pivot.x;
            float pivotY = rect.pivot.y;
            float dx = (left + ((right - left) * pivotX)) - (minX + (width * pivotX));
            float dy = (bottom + ((top - bottom) * pivotY)) - (minY + (height * pivotY));
            float dw = (right - left) - width;
            float dh = (top - bottom) - height;
            if (Mathf.Abs(dx) < 0.01f && Mathf.Abs(dy) < 0.01f && Mathf.Abs(dw) < 0.01f && Mathf.Abs(dh) < 0.01f)
            {
                return;
            }

            Vector3 move = PixelsToLocal(rect, cam, corners, pixelWidth, pixelHeight, dx, dy);
            Vector3 grown = PixelsToLocal(rect, cam, corners, pixelWidth, pixelHeight, dw, dh);
            if (Mathf.Abs(move.x) > 8f || Mathf.Abs(move.y) > 8f || Mathf.Abs(grown.x) > 8f || Mathf.Abs(grown.y) > 8f)
            {
                return;
            }

            Vector2 pos = rect.anchoredPosition;
            pos.x += move.x;
            pos.y += move.y;
            rect.anchoredPosition = pos;
            Vector2 sized = rect.sizeDelta;
            sized.x += grown.x;
            sized.y += grown.y;
            if (sized.x < 1f)
            {
                sized.x = 1f;
            }

            if (sized.y < 1f)
            {
                sized.y = 1f;
            }

            rect.sizeDelta = sized;
        }

        /// <summary>
        /// Asks the font for this segment at the size the canvas will draw.
        /// </summary>
        public static void WarmGlyphs(Text part)
        {
            RequestGlyphs(part, RasterScaleOf(part));
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
            if (HangarInkPass && host.gameObject.name == "Hint")
            {
                BindHintGlyph(text);
            }

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
            _play.RasterScale = RasterScaleOf(segment);
            _play.Color = segment.color;
            _play.Alignment = segment.alignment;
            _play.AlignByGeometry = segment.alignByGeometry;
            _play.LineSpacing = segment.lineSpacing;
            _play.Horizontal = segment.horizontalOverflow;
            _play.Vertical = segment.verticalOverflow;
            _play.RichText = segment.supportRichText;
            if (lockSample)
            {
                _playLocked = true;
            }
        }

        /// <summary>
        /// Shared UI material. font.material is a per-font instance; the
        /// dynamic atlas rebuild does not land on that copy, and the mesh
        /// then samples a stale texture as smeared blocks.
        /// </summary>
        private static void UseDefaultUiMaterial(Text part)
        {
            if (part == null)
            {
                return;
            }

            Material shared = Canvas.GetDefaultCanvasMaterial();
            if (shared == null)
            {
                return;
            }

            if (part.material != shared)
            {
                part.material = shared;
            }
        }

        private static float RasterScaleOf(Text part)
        {
            if (part != null)
            {
                Canvas canvas = part.canvas;
                if (canvas != null && canvas.scaleFactor > 0.01f)
                {
                    return canvas.scaleFactor;
                }
            }

            if (part != null && part.rectTransform != null)
            {
                return PromptLayout.ScreenBox(1f, part.rectTransform.lossyScale.y);
            }

            return 1f;
        }

        private static void RequestGlyphs(Text part, float rasterScale)
        {
            if (part == null || part.font == null || string.IsNullOrEmpty(part.text))
            {
                return;
            }

            float scale = rasterScale > 0.01f ? rasterScale : 1f;
            int pixel = Mathf.RoundToInt(part.fontSize * scale);
            if (pixel < 1)
            {
                pixel = 1;
            }

            part.font.RequestCharactersInTexture(part.text, pixel, part.fontStyle);
        }

        private static float Snap(float local, float scale)
        {
            if (scale < 0.01f)
            {
                return Mathf.Round(local);
            }

            return Mathf.Round(local * scale) / scale;
        }

        /// <summary>
        /// Pixel-align the hangar footer glyphs. Does not replace the wave-1
        /// box the capture gate stores. Ink is applied after the snap so a
        /// stamp skip cannot leave the previous child colour in place.
        /// </summary>
        public static void SnapHangarHint(Text host)
        {
            SnapHintChildren(host);
            ApplyHangarInk(host);
        }

        /// <summary>
        /// Product of CanvasGroup alphas from this node up to, but not past,
        /// <paramref name="stopBefore"/>. The canvas above the hint is not part
        /// of the hint ink.
        /// </summary>
        public static float HintGroupAlpha(Transform node, Transform stopBefore)
        {
            float alpha = 1f;
            Transform walk = node;
            while (walk != null && walk != stopBefore)
            {
                CanvasGroup group = walk.GetComponent<CanvasGroup>();
                if (group != null)
                {
                    alpha *= group.alpha;
                }

                walk = walk.parent;
            }

            return alpha;
        }

        /// <summary>
        /// The host colour is not what the camera draws. Paint copies it onto
        /// PSegT only when the layout stamp changes, and those children keep
        /// font.material: the Smooth atlas stores coverage in gray RGB, so a
        /// white vertex still peaks near 142 and the rise stays soft.
        /// This is the last writer for the hangar row: opaque white, group
        /// alpha 1 on the hint subtree, and AGR/HintInk so the stem is a hard pixel.
        /// </summary>
        public static void ApplyHangarInk(Text host)
        {
            if (host == null)
            {
                return;
            }

            host.color = UiTheme.FooterHint;
            ForceHintGroup(host.gameObject);
            Text[] parts = host.GetComponentsInChildren<Text>(true);
            if (parts == null)
            {
                return;
            }

            Transform stop = host.transform.parent;
            for (int index = 0; index < parts.Length; index++)
            {
                Text part = parts[index];
                if (part == null || part.gameObject == null || part.gameObject.name != SegmentPrefix + "T")
                {
                    continue;
                }

                BindHintGlyph(part);
                WarmGlyphs(part);
                float grouped = HintGroupAlpha(part.transform, stop);
                if (!UiTheme.EffectiveFooterHolds(part.color, grouped))
                {
                    part.color = UiTheme.FooterHint;
                    ForceHintGroup(part.gameObject);
                }
            }
        }

        /// <summary>
        /// Opaque white on the PSegT AgrCapture samples (parent named Hint).
        /// Sets the vertex colour, the material tint, and any CanvasGroup on
        /// the glyph and its Hint host. Called when the glyph is created and
        /// again after the capture canvas rebuild.
        /// </summary>
        public static void BindHintGlyph(Text part)
        {
            if (part == null)
            {
                return;
            }

            part.color = UiTheme.FooterHint;
            UseHintInk(part);
            Material ink = part.material;
            if (ink != null)
            {
                ink.color = Color.white;
                if (ink.HasProperty("_Color"))
                {
                    ink.SetColor("_Color", Color.white);
                }
            }

            ForceHintGroup(part.gameObject);
            if (part.transform.parent != null)
            {
                ForceHintGroup(part.transform.parent.gameObject);
            }
        }

        /// <summary>
        /// Hierarchy path for the capture log. The metric's object is this transform.
        /// </summary>
        public static string HintGlyphPath(Transform node)
        {
            if (node == null)
            {
                return string.Empty;
            }

            string path = node.name;
            Transform walk = node.parent;
            while (walk != null)
            {
                path = walk.name + "/" + path;
                walk = walk.parent;
            }

            return path;
        }

        /// <summary>
        /// Vertex alpha times every CanvasGroup from the glyph up to the canvas.
        /// </summary>
        public static float DrawnHintAlpha(Text part)
        {
            if (part == null)
            {
                return 0f;
            }

            float grouped = HintGroupAlpha(part.transform, null);
            float alpha = part.color.a * grouped;
            if (alpha < 0f)
            {
                return 0f;
            }

            if (alpha > 1f)
            {
                return 1f;
            }

            return alpha;
        }

        private static void ForceHintGroup(GameObject go)
        {
            if (go == null)
            {
                return;
            }

            CanvasGroup group = go.GetComponent<CanvasGroup>();
            if (group != null && group.alpha < 1f)
            {
                group.alpha = 1f;
            }
        }

        private static Material _hintInk;

        private static void UseHintInk(Text part)
        {
            if (part == null)
            {
                return;
            }

            if (_hintInk == null)
            {
                Shader shader = Shader.Find("AGR/HintInk");
                if (shader == null)
                {
                    shader = Resources.Load<Shader>("Shaders/AgrHintInk");
                }

                if (shader == null)
                {
                    UseDefaultUiMaterial(part);
                    return;
                }

                _hintInk = new Material(shader);
                _hintInk.hideFlags = HideFlags.HideAndDontSave;
                _hintInk.SetColor("_Color", Color.white);
                _hintInk.SetFloat("_Cutoff", 0.40f);
            }

            if (part.material != _hintInk)
            {
                part.material = _hintInk;
            }
        }

        private static void SnapHintChildren(Text host)
        {
            if (host == null)
            {
                return;
            }

            Text[] parts = host.GetComponentsInChildren<Text>(true);
            if (parts == null)
            {
                return;
            }

            float pixelW = Screen.width;
            float pixelH = Screen.height;
            for (int index = 0; index < parts.Length; index++)
            {
                Text part = parts[index];
                if (part == null || part.gameObject == null || part.gameObject.name != SegmentPrefix + "T")
                {
                    continue;
                }

                SnapHintText(part, null, pixelW, pixelH);
            }
        }

        private static bool HintScreenRect(Vector3[] corners, Camera cam, float pixelWidth, float pixelHeight, out float minX, out float minY, out float maxX, out float maxY)
        {
            minX = 0f;
            minY = 0f;
            maxX = 0f;
            maxY = 0f;
            if (corners == null || corners.Length == 0)
            {
                return false;
            }

            bool any = false;
            for (int index = 0; index < corners.Length; index++)
            {
                float x;
                float y;
                if (cam == null)
                {
                    x = corners[index].x;
                    y = corners[index].y;
                }
                else
                {
                    Vector3 vp = cam.WorldToViewportPoint(corners[index]);
                    if (vp.z <= 0.01f)
                    {
                        return false;
                    }

                    x = vp.x * pixelWidth;
                    y = vp.y * pixelHeight;
                }

                if (!any)
                {
                    minX = x;
                    minY = y;
                    maxX = x;
                    maxY = y;
                    any = true;
                    continue;
                }

                if (x < minX)
                {
                    minX = x;
                }

                if (y < minY)
                {
                    minY = y;
                }

                if (x > maxX)
                {
                    maxX = x;
                }

                if (y > maxY)
                {
                    maxY = y;
                }
            }

            return any && maxX - minX > 0.5f && maxY - minY > 0.5f;
        }

        private static Vector3 PixelsToLocal(RectTransform rect, Camera cam, Vector3[] corners, float pixelWidth, float pixelHeight, float dx, float dy)
        {
            if (rect == null)
            {
                return Vector3.zero;
            }

            if (cam == null || corners == null || corners.Length == 0)
            {
                return rect.InverseTransformVector(new Vector3(dx, dy, 0f));
            }

            Vector3 vp = cam.WorldToViewportPoint(corners[0]);
            Vector3 from = cam.ViewportToWorldPoint(new Vector3(vp.x, vp.y, vp.z));
            Vector3 to = cam.ViewportToWorldPoint(new Vector3(vp.x + (dx / pixelWidth), vp.y + (dy / pixelHeight), vp.z));
            return rect.InverseTransformVector(to - from);
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
