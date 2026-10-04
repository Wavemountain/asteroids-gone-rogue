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

        public static void Paint(Text host, string source, InputScheme scheme, float canvasScale)
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
            string stamp = ((int)scheme).ToString() + "|" + canvasScale.ToString("0.000") + "|" + fontSize.ToString() + "|" + safe;
            if (mark.Stamp == stamp)
            {
                return;
            }

            mark.Source = safe;
            mark.Stamp = stamp;
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
            float lineHeight = host.fontSize * 1.15f;
            if (iconPx > lineHeight)
            {
                lineHeight = iconPx;
            }

            float hostWidth = host.rectTransform.rect.width;
            float hostHeight = host.rectTransform.rect.height;
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
                float cursorX = centered ? -rowWidth * 0.5f : LeftOrigin(hostWidth);
                float cursorY = top - (lineIndex * lineHeight);
                int pieceCount = pieces.Length;
                for (int pieceIndex = 0; pieceIndex < pieceCount; pieceIndex++)
                {
                    float pieceWidth = widths[pieceIndex];
                    Place(host, pieces[pieceIndex], scheme, hi, iconPx, cursorX, cursorY, pieceWidth, lineHeight);
                    cursorX += pieceWidth + gap;
                }
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

            mark.Stamp = string.Empty;
            Paint(host, mark.Source, scheme, canvasScale);
        }

        private static void Place(
            Text host,
            PromptPiece piece,
            InputScheme scheme,
            bool hi,
            float iconPx,
            float x,
            float y,
            float width,
            float lineHeight)
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
                    iconRect.sizeDelta = new Vector2(iconPx, iconPx);
                    iconRect.anchoredPosition = new Vector2(x, y);
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
            text.fontSize = host.fontSize;
            text.fontStyle = host.fontStyle;
            text.color = host.color;
            text.alignment = TextAnchor.MiddleLeft;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            text.supportRichText = false;
            text.text = label;
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.pivot = new Vector2(0f, 0.5f);
            float textWidth = width > 1f ? width : iconPx;
            textRect.sizeDelta = new Vector2(textWidth, lineHeight);
            textRect.anchoredPosition = new Vector2(x, y);
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
                    return measured;
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
