namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Hangar shop tile text, colours, and overlays. Unity-free so Roslyn can
    /// compile it. Already-upgraded tiles (bought, Mk II for sale, Mk II bought
    /// / maxed) used to share one owned plate: Secondary text on a Secondary
    /// fill, which is blank. Untouched tiles never took that path.
    /// </summary>
    public static class ShopTileView
    {
        public const float MinContrast = 4.5f;
        public const string UnboughtFillHex = "#141C28";
        public const string UnboughtTextHex = "#C8CED6";
        public const string PoorFillHex = "#3A4450";
        public const string PoorTextHex = "#F0C8A8";
        public const string LockedFillHex = "#3A4450";
        public const string LockedTextHex = "#A8B2BC";
        public const string BoughtFillHex = "#102028";
        public const string BoughtTextHex = "#C8CED6";
        public const string Mk2FillHex = "#141C28";
        public const string Mk2TextHex = "#C8CED6";
        public const string MaxedFillHex = "#0E1A22";
        public const string MaxedTextHex = "#C8CED6";
        public const string EquippedFillHex = "#141C28";
        public const string EquippedTextHex = "#D4A04A";
        public const string OffPathFillHex = "#3A241C";
        public const string OffPathTextHex = "#F0C8A8";
        public const string RunOverFillHex = "#3A4450";
        public const string RunOverTextHex = "#A8B2BC";
        public const string FocusOverlayName = "FocusFill";

        public static ShopTileModel Build(ShopTileInput input)
        {
            bool owned = input != null && input.Owned;
            bool mk2Owned = input != null && input.Mk2Owned;
            bool mk2Offer = input != null && input.Mk2Offer;
            bool canApply = input != null && input.CanApply;
            bool offPath = input != null && input.OffPath;
            bool weapon = input != null && input.Weapon;
            bool equipped = input != null && input.Equipped;
            bool runOver = input != null && input.RunOver;
            bool shopOpen = input != null && input.ShopOpen;
            bool focused = input != null && input.Focused;
            bool swedish = input != null && input.Swedish;
            int price = input != null ? input.Price : 0;
            int credits = input != null ? input.Credits : 0;
            if (price < 0)
            {
                price = 0;
            }

            if (credits < 0)
            {
                credits = 0;
            }

            string title = input != null ? input.Title : null;
            if (string.IsNullOrEmpty(title))
            {
                title = string.Empty;
            }

            bool tooPoor = !runOver && ((mk2Offer && credits < price) || (!owned && canApply && credits < price));
            bool locked = runOver || (!owned && !canApply);
            ShopUpgradeStage stage = ResolveStage(
                owned,
                mk2Owned,
                mk2Offer,
                canApply,
                offPath,
                weapon,
                equipped,
                runOver,
                credits,
                price);

            string status = StatusLine(owned, mk2Owned, mk2Offer, canApply, offPath, runOver, swedish, price, credits);
            ShopTileModel model = new ShopTileModel();
            model.Name = title;
            model.Status = status;
            model.Label = title + "\n" + status;
            model.Stage = stage;
            model.FillHex = FillHex(stage);
            model.TextHex = TextHex(stage);
            model.Focused = focused;
            bool interactable = !runOver
                && shopOpen
                && ((mk2Offer && !tooPoor) || (owned && weapon) || (!owned && !locked && !tooPoor));
            model.Interactable = interactable;
            model.Overlays = OverlaysFor(focused);
            return model;
        }

        public static ShopUpgradeStage ResolveStage(
            bool owned,
            bool mk2Owned,
            bool mk2Offer,
            bool canApply,
            bool offPath,
            bool weapon,
            bool equipped,
            bool runOver,
            int credits,
            int price)
        {
            if (runOver)
            {
                return ShopUpgradeStage.RunOver;
            }

            if (offPath)
            {
                return ShopUpgradeStage.OffPath;
            }

            if (equipped && weapon && owned)
            {
                return ShopUpgradeStage.Equipped;
            }

            if (owned && mk2Owned)
            {
                return ShopUpgradeStage.Maxed;
            }

            if (mk2Offer && credits < price)
            {
                return ShopUpgradeStage.Mk2Poor;
            }

            if (mk2Offer)
            {
                return ShopUpgradeStage.Mk2Available;
            }

            if (owned)
            {
                return ShopUpgradeStage.Bought;
            }

            if (!canApply)
            {
                return ShopUpgradeStage.Locked;
            }

            if (credits < price)
            {
                return ShopUpgradeStage.Poor;
            }

            return ShopUpgradeStage.Unbought;
        }

        public static string StatusLine(
            bool owned,
            bool mk2Owned,
            bool mk2Offer,
            bool canApply,
            bool offPath,
            bool runOver,
            bool swedish,
            int price,
            int credits)
        {
            string status;
            if (runOver)
            {
                status = swedish ? "Rundan är slut" : "Run is over";
            }
            else if (owned && mk2Owned)
            {
                status = "Mk II  MAX";
            }
            else if (mk2Offer)
            {
                status = "Mk II  " + CostText(price, swedish);
            }
            else if (owned)
            {
                status = (swedish ? "KÖPT" : "OWNED") + " +";
            }
            else if (!canApply)
            {
                status = swedish ? "LÅST" : "LOCKED";
            }
            else if (credits < price)
            {
                status = swedish ? "behöver " + price + " kr" : "need " + price + " cr";
            }
            else
            {
                status = CostText(price, swedish);
            }

            if (!runOver && offPath)
            {
                string prefix = swedish ? "av vägen" : "off-path";
                status = prefix + "  ·  " + status;
            }

            return status;
        }

        public static string FillHex(ShopUpgradeStage stage)
        {
            switch (stage)
            {
                case ShopUpgradeStage.Poor:
                    return PoorFillHex;
                case ShopUpgradeStage.Locked:
                    return LockedFillHex;
                case ShopUpgradeStage.Bought:
                    return BoughtFillHex;
                case ShopUpgradeStage.Mk2Available:
                    return Mk2FillHex;
                case ShopUpgradeStage.Mk2Poor:
                    return PoorFillHex;
                case ShopUpgradeStage.Maxed:
                    return MaxedFillHex;
                case ShopUpgradeStage.Equipped:
                    return EquippedFillHex;
                case ShopUpgradeStage.OffPath:
                    return OffPathFillHex;
                case ShopUpgradeStage.RunOver:
                    return RunOverFillHex;
                default:
                    return UnboughtFillHex;
            }
        }

        public static string TextHex(ShopUpgradeStage stage)
        {
            switch (stage)
            {
                case ShopUpgradeStage.Poor:
                    return PoorTextHex;
                case ShopUpgradeStage.Locked:
                    return LockedTextHex;
                case ShopUpgradeStage.Bought:
                    return BoughtTextHex;
                case ShopUpgradeStage.Mk2Available:
                    return Mk2TextHex;
                case ShopUpgradeStage.Mk2Poor:
                    return PoorTextHex;
                case ShopUpgradeStage.Maxed:
                    return MaxedTextHex;
                case ShopUpgradeStage.Equipped:
                    return EquippedTextHex;
                case ShopUpgradeStage.OffPath:
                    return OffPathTextHex;
                case ShopUpgradeStage.RunOver:
                    return RunOverTextHex;
                default:
                    return UnboughtTextHex;
            }
        }

        public static ShopTileOverlay[] OverlaysFor(bool focused)
        {
            if (!focused)
            {
                return new ShopTileOverlay[0];
            }

            ShopTileOverlay fill = new ShopTileOverlay(FocusOverlayName, 0, false);
            return new ShopTileOverlay[] { fill };
        }

        /// <summary>
        /// A refresh replaces the overlay list. It does not append another
        /// FocusFill or Mk II badge on top of the label.
        /// </summary>
        public static ShopTileOverlay[] MergeOverlays(ShopTileOverlay[] prior, ShopTileOverlay[] next)
        {
            if (next == null)
            {
                return new ShopTileOverlay[0];
            }

            return next;
        }

        public static bool AnyOverlayAboveLabel(ShopTileOverlay[] overlays)
        {
            if (overlays == null)
            {
                return false;
            }

            for (int i = 0; i < overlays.Length; i++)
            {
                if (overlays[i] != null && overlays[i].AboveLabel)
                {
                    return true;
                }
            }

            return false;
        }

        public static float Contrast(string textHex, string fillHex)
        {
            float textLum = RelativeLuminance(textHex);
            float fillLum = RelativeLuminance(fillHex);
            float lighter = textLum > fillLum ? textLum : fillLum;
            float darker = textLum > fillLum ? fillLum : textLum;
            return (lighter + 0.05f) / (darker + 0.05f);
        }

        public static bool Readable(string textHex, string fillHex)
        {
            return Contrast(textHex, fillHex) >= MinContrast;
        }

        private static string CostText(int price, bool swedish)
        {
            if (swedish)
            {
                return price + " kr";
            }

            return price + " cr";
        }

        private static float RelativeLuminance(string hex)
        {
            int red;
            int green;
            int blue;
            if (!TryParse(hex, out red, out green, out blue))
            {
                return 0f;
            }

            return (0.2126f * Channel(red))
                + (0.7152f * Channel(green))
                + (0.0722f * Channel(blue));
        }

        private static float Channel(int byteValue)
        {
            float channel = byteValue / 255f;
            if (channel <= 0.04045f)
            {
                return channel / 12.92f;
            }

            return (float)System.Math.Pow((channel + 0.055d) / 1.055d, 2.4d);
        }

        private static bool TryParse(string hex, out int red, out int green, out int blue)
        {
            red = 0;
            green = 0;
            blue = 0;
            if (string.IsNullOrEmpty(hex) || hex.Length < 7 || hex[0] != '#')
            {
                return false;
            }

            red = (Nibble(hex[1]) << 4) + Nibble(hex[2]);
            green = (Nibble(hex[3]) << 4) + Nibble(hex[4]);
            blue = (Nibble(hex[5]) << 4) + Nibble(hex[6]);
            return true;
        }

        private static int Nibble(char value)
        {
            if (value >= '0' && value <= '9')
            {
                return value - '0';
            }

            if (value >= 'a' && value <= 'f')
            {
                return value - 'a' + 10;
            }

            if (value >= 'A' && value <= 'F')
            {
                return value - 'A' + 10;
            }

            return 0;
        }
    }

    public enum ShopUpgradeStage
    {
        Unbought,
        Poor,
        Locked,
        Bought,
        Mk2Available,
        Mk2Poor,
        Maxed,
        Equipped,
        OffPath,
        RunOver
    }

    public sealed class ShopTileInput
    {
        public string Title;
        public int Price;
        public int Credits;
        public bool Owned;
        public bool Mk2Owned;
        public bool Mk2Offer;
        public bool CanApply;
        public bool OffPath;
        public bool Weapon;
        public bool Equipped;
        public bool RunOver;
        public bool ShopOpen;
        public bool Focused;
        public bool Swedish;
    }

    public sealed class ShopTileOverlay
    {
        public readonly string Name;
        public readonly int Order;
        public readonly bool AboveLabel;

        public ShopTileOverlay(string name, int order, bool aboveLabel)
        {
            Name = name;
            Order = order;
            AboveLabel = aboveLabel;
        }
    }

    public sealed class ShopTileModel
    {
        public string Name;
        public string Status;
        public string Label;
        public string FillHex;
        public string TextHex;
        public ShopUpgradeStage Stage;
        public bool Focused;
        public bool Interactable;
        public ShopTileOverlay[] Overlays;
    }
}
