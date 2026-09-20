using UnityEngine;
using UnityEngine.UI;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Locked Atmos UI tokens and shared chrome. Anti-epa, retro-modern AAA.
    /// Kenney Future headers 28–46, Kenney Future Narrow body 14–22 — never Arial.
    /// </summary>
    public static class UiTheme
    {
        public const string VoidHex = "#070B12";
        public const string SurfaceHex = "#0E1520";
        public const string Surface2Hex = "#141C28";
        public const string PrimaryHex = "#D4A04A";
        public const string SecondaryHex = "#6AA8C8";
        public const string AccentHex = "#C8CED6";
        public const string DangerHex = "#B85A28";
        public const string DisabledHex = "#3A4450";
        public const string FocusHex = "#E8C878";

        public const float SurfaceAlpha = 0.72f;
        public const float PanelAlpha = 0.96f;
        public const float DisabledAlpha = 0.45f;
        public const float FocusFillAlpha = 0.08f;
        public const float HoverBright = 0.10f;
        public const float PressedDark = 0.12f;
        public const float InactiveDesat = 0.40f;
        public const float FocusRingPx = 2f;
        public const int HeaderMin = 28;
        public const int HeaderMax = 46;
        public const int BodyMin = 14;
        public const int BodyMax = 22;
        public const int PanelPadMin = 16;
        public const int PanelPadMax = 24;
        public const int ButtonPadMin = 8;
        public const int ButtonPadMax = 12;
        public const string OwnedCheck = " ✓";

        public static readonly Color Void = Parse(VoidHex);
        public static readonly Color Surface = Parse(SurfaceHex);
        public static readonly Color Surface2 = Parse(Surface2Hex);
        public static readonly Color Primary = Parse(PrimaryHex);
        public static readonly Color Secondary = Parse(SecondaryHex);
        public static readonly Color Accent = Parse(AccentHex);
        public static readonly Color Danger = Parse(DangerHex);
        public static readonly Color DisabledRgb = Parse(DisabledHex);
        public static readonly Color Focus = Parse(FocusHex);

        public static Color Disabled
        {
            get { return WithAlpha(DisabledRgb, DisabledAlpha); }
        }

        public static Color HudPlate
        {
            get { return WithAlpha(Surface, SurfaceAlpha); }
        }

        public static Color PanelPlate
        {
            get { return WithAlpha(Surface, PanelAlpha); }
        }

        public static Color InnerWash
        {
            get { return WithAlpha(Surface2, 0.42f); }
        }

        public static Color HeaderWash
        {
            get { return WithAlpha(Primary, 0.22f); }
        }

        public static Color HeaderRule
        {
            get { return WithAlpha(Primary, 0.88f); }
        }

        public static Color DangerHeader
        {
            get { return WithAlpha(Danger, 0.42f); }
        }

        public static Color DangerTint
        {
            get { return WithAlpha(Danger, 0.82f); }
        }

        public static Color FocusFill
        {
            get { return WithAlpha(Primary, FocusFillAlpha); }
        }

        public static Color PrimaryCta
        {
            get { return Darken(Primary, 0.18f); }
        }

        public static Color ShopIdle
        {
            get { return WithAlpha(Surface2, 0.94f); }
        }

        public static Color ShopOwned
        {
            get { return WithAlpha(Secondary, 0.22f); }
        }

        public static Color ShopLocked
        {
            get { return WithAlpha(DisabledRgb, 0.55f); }
        }

        public static Color FooterHint
        {
            get { return WithAlpha(Accent, 0.92f); }
        }

        public static Color Parse(string hex)
        {
            Color color;
            if (!ColorUtility.TryParseHtmlString(hex, out color))
            {
                color = Color.magenta;
            }

            return color;
        }

        public static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        public static Color Brighten(Color color, float amount)
        {
            return Color.Lerp(color, Color.white, amount);
        }

        public static Color Darken(Color color, float amount)
        {
            return Color.Lerp(color, Color.black, amount);
        }

        public static Color Desaturate(Color color, float amount)
        {
            float grey = color.grayscale;
            return Color.Lerp(color, new Color(grey, grey, grey, color.a), amount);
        }

        public static ColorBlock MenuButtonColors()
        {
            ColorBlock colors = ColorBlock.defaultColorBlock;
            colors.normalColor = Color.white;
            colors.highlightedColor = Brighten(Color.white, HoverBright);
            colors.pressedColor = Darken(Color.white, PressedDark);
            colors.selectedColor = Color.Lerp(Color.white, Primary, FocusFillAlpha);
            colors.disabledColor = Disabled;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            return colors;
        }

        /// <summary>
        /// Shared panel template: surface plate, header wash, amber rule, optional
        /// title + footer hint. List content is parented by the caller.
        /// </summary>
        public static GameObject BuildPanel(
            string name,
            Transform parent,
            Vector2 min,
            Vector2 max,
            float headerMinY)
        {
            return BuildPanel(name, parent, min, max, headerMinY, HeaderWash, HeaderRule, PanelPlate);
        }

        public static GameObject BuildPanel(
            string name,
            Transform parent,
            Vector2 min,
            Vector2 max,
            float headerMinY,
            Color header,
            Color rule,
            Color plate)
        {
            GameObject go = Fill(name, parent, plate, min, max);
            Fill(name + "Header", go.transform, header, new Vector2(0f, headerMinY), Vector2.one);
            float ruleMin = headerMinY - 0.008f;
            if (ruleMin < 0f)
            {
                ruleMin = 0f;
            }

            Fill(name + "Rule", go.transform, rule, new Vector2(0.04f, ruleMin), new Vector2(0.96f, headerMinY));
            return go;
        }

        public static GameObject Fill(string name, Transform parent, Color color, Vector2 min, Vector2 max)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            Stretch(go.GetComponent<RectTransform>(), min, max);
            return go;
        }

        public static void Stretch(RectTransform rect, Vector2 min, Vector2 max)
        {
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public static void ApplyButton(Button button, bool primaryCta, bool danger, bool shop)
        {
            if (button == null)
            {
                return;
            }

            button.colors = MenuButtonColors();
            Navigation nav = button.navigation;
            nav.mode = Navigation.Mode.None;
            button.navigation = nav;
            Image plate = button.targetGraphic as Image;
            if (plate != null)
            {
                if (danger)
                {
                    plate.color = DangerTint;
                }
                else if (primaryCta)
                {
                    plate.color = PrimaryCta;
                }
                else
                {
                    plate.color = shop ? ShopIdle : Surface2;
                }
            }

            EnsureFocusFill(button.gameObject);
            Outline ring = EnsureRing(button.gameObject);
            ring.effectColor = shop ? WithAlpha(Secondary, 0.92f) : Color.clear;
            ring.enabled = shop;
        }

        public static Outline EnsureRing(GameObject go)
        {
            Outline ring = go.GetComponent<Outline>();
            if (ring == null)
            {
                ring = go.AddComponent<Outline>();
            }

            ring.effectDistance = new Vector2(FocusRingPx, -FocusRingPx);
            ring.useGraphicAlpha = false;
            return ring;
        }

        public static Image EnsureFocusFill(GameObject go)
        {
            Transform existing = go.transform.Find("FocusFill");
            if (existing != null)
            {
                return existing.GetComponent<Image>();
            }

            GameObject fill = new GameObject("FocusFill");
            fill.transform.SetParent(go.transform, false);
            fill.transform.SetAsFirstSibling();
            Image image = fill.AddComponent<Image>();
            image.color = FocusFill;
            image.raycastTarget = false;
            Stretch(fill.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
            fill.SetActive(false);
            return image;
        }

        public static void SetPadFocus(GameObject go, bool focused, bool shop)
        {
            if (go == null)
            {
                return;
            }

            Outline ring = EnsureRing(go);
            ring.effectDistance = new Vector2(FocusRingPx, -FocusRingPx);
            if (focused)
            {
                ring.effectColor = Focus;
                ring.enabled = true;
            }
            else if (shop)
            {
                ring.effectColor = WithAlpha(Secondary, 0.92f);
                ring.enabled = true;
            }
            else
            {
                ring.effectColor = Color.clear;
                ring.enabled = false;
            }

            Image fill = EnsureFocusFill(go);
            if (fill != null)
            {
                fill.color = FocusFill;
                fill.gameObject.SetActive(focused);
            }
        }

        public static void PaintShopPlate(Image plate, Text label, bool owned, bool locked, bool tooPoor)
        {
            if (plate == null)
            {
                return;
            }

            if (owned)
            {
                plate.color = ShopOwned;
            }
            else if (locked)
            {
                plate.color = ShopLocked;
            }
            else if (tooPoor)
            {
                plate.color = Disabled;
            }
            else
            {
                plate.color = ShopIdle;
            }

            if (label == null)
            {
                return;
            }

            if (owned)
            {
                label.color = Desaturate(Secondary, 0.25f);
            }
            else if (locked)
            {
                label.color = WithAlpha(Accent, 0.45f);
            }
            else if (tooPoor)
            {
                label.color = WithAlpha(Accent, DisabledAlpha);
            }
            else
            {
                label.color = Accent;
            }
        }

        public static void PaintLanguageChip(Image bezel, bool selected, bool focused)
        {
            if (bezel == null)
            {
                return;
            }

            Outline ring = EnsureRing(bezel.gameObject);
            if (focused)
            {
                ring.effectColor = Focus;
                ring.enabled = true;
            }
            else if (selected)
            {
                ring.effectColor = Secondary;
                ring.enabled = true;
            }
            else
            {
                ring.effectColor = Color.clear;
                ring.enabled = false;
            }

            bezel.color = selected
                ? WithAlpha(Secondary, 0.72f)
                : WithAlpha(Surface2, 0.55f);

            Image fill = EnsureFocusFill(bezel.gameObject);
            fill.gameObject.SetActive(focused);

            CanvasGroup group = bezel.GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = bezel.gameObject.AddComponent<CanvasGroup>();
                group.blocksRaycasts = true;
                group.interactable = true;
            }

            group.alpha = selected || focused ? 1f : (1f - InactiveDesat);
        }
    }
}
