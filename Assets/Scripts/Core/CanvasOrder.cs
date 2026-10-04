namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Screen-space canvas sort scale. One list so a hangar panel cannot draw
    /// over a full-screen overlay. Higher values draw in front.
    /// </summary>
    public static class CanvasOrder
    {
        public const int Root = 0;
        public const int ShipPreview = 80;
        public const int HangarShop = 120;
        public const int Overlay = 150;
        public const int Rebind = 160;
        public const int Boon = 200;
        public const int Toast = 250;
    }
}
