using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Shop tile fill and label colours. Every state keeps text/fill contrast
    /// at or above <see cref="MinContrast"/>. Owned used to paint Secondary
    /// text on a Secondary plate, which reads as light-blue on light-blue.
    /// </summary>
    public static class ShopTileChrome
    {
        public const float MinContrast = 4.5f;

        public const string NormalFillHex = "#141C28";
        public const string NormalTextHex = "#C8CED6";
        public const string HoverFillHex = "#1C2636";
        public const string HoverTextHex = "#C8CED6";
        public const string PressedFillHex = "#0C1218";
        public const string PressedTextHex = "#C8CED6";
        public const string FocusFillHex = "#1A2434";
        public const string FocusTextHex = "#E8C878";
        public const string LockedFillHex = "#3A4450";
        public const string LockedTextHex = "#A8B2BC";
        public const string PoorFillHex = "#3A4450";
        public const string PoorTextHex = "#F0C8A8";
        public const string OwnedFillHex = "#102028";
        public const string OwnedTextHex = "#6AA8C8";
        public const string EquippedFillHex = "#141C28";
        public const string EquippedTextHex = "#D4A04A";
        public const string OffPathFillHex = "#3A241C";
        public const string OffPathTextHex = "#F0C8A8";
        public const string ColumnFillHex = "#0E1520";

        public static Color Fill(ShopTileState state)
        {
            return Opaque(FillHex(state));
        }

        public static Color Text(ShopTileState state)
        {
            return Opaque(TextHex(state));
        }

        public static Color Column
        {
            get { return Opaque(ColumnFillHex); }
        }

        public static string FillHex(ShopTileState state)
        {
            switch (state)
            {
                case ShopTileState.Hovered:
                    return HoverFillHex;
                case ShopTileState.Pressed:
                    return PressedFillHex;
                case ShopTileState.Focused:
                    return FocusFillHex;
                case ShopTileState.Locked:
                    return LockedFillHex;
                case ShopTileState.Poor:
                    return PoorFillHex;
                case ShopTileState.Owned:
                    return OwnedFillHex;
                case ShopTileState.Equipped:
                    return EquippedFillHex;
                case ShopTileState.OffPath:
                    return OffPathFillHex;
                default:
                    return NormalFillHex;
            }
        }

        public static string TextHex(ShopTileState state)
        {
            switch (state)
            {
                case ShopTileState.Hovered:
                    return HoverTextHex;
                case ShopTileState.Pressed:
                    return PressedTextHex;
                case ShopTileState.Focused:
                    return FocusTextHex;
                case ShopTileState.Locked:
                    return LockedTextHex;
                case ShopTileState.Poor:
                    return PoorTextHex;
                case ShopTileState.Owned:
                    return OwnedTextHex;
                case ShopTileState.Equipped:
                    return EquippedTextHex;
                case ShopTileState.OffPath:
                    return OffPathTextHex;
                default:
                    return NormalTextHex;
            }
        }

        public static bool Readable(ShopTileState state)
        {
            return UiTheme.ContrastRatio(Text(state), Fill(state)) >= MinContrast;
        }

        public static ShopTileState FromFlags(bool owned, bool locked, bool tooPoor)
        {
            if (owned)
            {
                return ShopTileState.Owned;
            }

            if (locked)
            {
                return ShopTileState.Locked;
            }

            if (tooPoor)
            {
                return ShopTileState.Poor;
            }

            return ShopTileState.Normal;
        }

        private static Color Opaque(string hex)
        {
            Color color = UiTheme.Parse(hex);
            color.a = 1f;
            return color;
        }
    }

    public enum ShopTileState
    {
        Normal,
        Hovered,
        Pressed,
        Focused,
        Locked,
        Poor,
        Owned,
        Equipped,
        OffPath
    }
}
