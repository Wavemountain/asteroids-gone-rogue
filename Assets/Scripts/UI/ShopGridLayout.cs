namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Screen rects for the hangar shop grid and the ship preview frame.
    /// The preview lives on its own overlay. These rects must not overlap, and
    /// the shop canvas sorts above that overlay so a bleed cannot cover a tile.
    /// </summary>
    public static class ShopGridLayout
    {
        public const int ShopSortOrder = 120;
        public const int PreviewSortOrder = 80;
        public const float HangarMinX = 0.014f;
        public const float HangarMinY = 0.080f;
        public const float HangarMaxX = 0.55f;
        public const float HangarMaxY = 0.888f;
        public const float PreviewMinX = 0.562f;
        public const float PreviewMinY = 0.080f;
        public const float PreviewMaxX = 0.986f;
        public const float PreviewMaxY = 0.596f;
        public const float GridTop = 0.665f;
        public const float CellHeight = 0.094f;
        public const float CellGutter = 0.016f;
        public const float HullOriginX = 0.02f;
        public const float HullStepX = 0.1175f;
        public const float HullCellW = 0.110f;
        public const int HullColumns = 4;
        public const int HullRows = 3;
        public const float WeaponsMinX = 0.51f;
        public const float WeaponsMaxX = 0.735f;
        public const float DefenseMinX = 0.755f;
        public const float DefenseMaxX = 0.98f;
        public const int DefenseRows = 5;
        public const float RefWidth = 1920f;
        public const float RefHeight = 1080f;
        public const float Match = 0.5f;
        public const int HullFont = 14;
        public const int NameFont = 18;
        public const float LineSpacing = 1.1f;

        public static void PreviewScreen(out float minX, out float minY, out float maxX, out float maxY)
        {
            minX = PreviewMinX;
            minY = PreviewMinY;
            maxX = PreviewMaxX;
            maxY = PreviewMaxY;
        }

        public static void GridScreen(out float minX, out float minY, out float maxX, out float maxY)
        {
            float rowStep = CellHeight + CellGutter;
            float localMinX = HullOriginX;
            float localMaxX = DefenseMaxX;
            float localMaxY = GridTop;
            float localMinY = GridTop - ((DefenseRows - 1) * rowStep) - CellHeight;
            ToScreen(localMinX, localMinY, localMaxX, localMaxY, out minX, out minY, out maxX, out maxY);
        }

        public static void HullCellScreen(int index, out float minX, out float minY, out float maxX, out float maxY)
        {
            int slot = index;
            if (slot < 0)
            {
                slot = 0;
            }

            int col = slot % HullColumns;
            int row = slot / HullColumns;
            float rowStep = CellHeight + CellGutter;
            float x0 = HullOriginX + (col * HullStepX);
            float top = GridTop - (row * rowStep);
            ToScreen(x0, top - CellHeight, x0 + HullCellW, top, out minX, out minY, out maxX, out maxY);
        }

        public static void ToScreen(
            float localMinX,
            float localMinY,
            float localMaxX,
            float localMaxY,
            out float minX,
            out float minY,
            out float maxX,
            out float maxY)
        {
            float panelW = HangarMaxX - HangarMinX;
            float panelH = HangarMaxY - HangarMinY;
            minX = HangarMinX + (localMinX * panelW);
            minY = HangarMinY + (localMinY * panelH);
            maxX = HangarMinX + (localMaxX * panelW);
            maxY = HangarMinY + (localMaxY * panelH);
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

        public static bool InsideScreen(float minX, float minY, float maxX, float maxY)
        {
            return minX >= 0f
                && minY >= 0f
                && maxX <= 1f
                && maxY <= 1f
                && maxX > minX
                && maxY > minY;
        }
    }
}
