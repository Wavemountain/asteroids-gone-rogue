namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Screen rects for the bay overlay and the hangar entry cell.
    /// The entry sits in the empty hull slot after the Mk II hull tiles.
    /// </summary>
    public static class SinkShopLayout
    {
        public const int Font = 18;
        public const float PanelMinX = 0.18f;
        public const float PanelMinY = 0.12f;
        public const float PanelMaxX = 0.82f;
        public const float PanelMaxY = 0.88f;
        public const int TileCount = 6;
        public const int Columns = 2;

        public static void EntryLocal(out float minX, out float minY, out float maxX, out float maxY)
        {
            float rowStep = ShopGridLayout.CellHeight + ShopGridLayout.CellGutter;
            int col = 3;
            int row = 2;
            float origin = ShopGridLayout.HullOriginX + (col * ShopGridLayout.HullStepX);
            float top = ShopGridLayout.GridTop - (row * rowStep);
            minX = origin;
            maxX = origin + ShopGridLayout.HullCellW;
            maxY = top;
            minY = top - ShopGridLayout.CellHeight;
        }

        public static void Tile(int index, out float minX, out float minY, out float maxX, out float maxY)
        {
            int slot = index;
            if (slot < 0)
            {
                slot = 0;
            }

            if (slot >= TileCount)
            {
                slot = TileCount - 1;
            }

            int col = slot % Columns;
            int row = slot / Columns;
            float gutter = 0.02f;
            float areaMinX = 0.20f;
            float areaMaxX = 0.80f;
            float areaMaxY = 0.76f;
            float areaMinY = 0.30f;
            float cellW = (areaMaxX - areaMinX - gutter) * 0.5f;
            float cellH = (areaMaxY - areaMinY - (gutter * 2f)) / 3f;
            float x0 = areaMinX + (col * (cellW + gutter));
            float y1 = areaMaxY - (row * (cellH + gutter));
            minX = x0;
            maxX = x0 + cellW;
            maxY = y1;
            minY = y1 - cellH;
        }

        public static void CloseButton(out float minX, out float minY, out float maxX, out float maxY)
        {
            minX = 0.36f;
            minY = 0.14f;
            maxX = 0.64f;
            maxY = 0.24f;
        }

        public static void Header(out float minX, out float minY, out float maxX, out float maxY)
        {
            minX = 0.20f;
            minY = 0.78f;
            maxX = 0.80f;
            maxY = 0.86f;
        }

        public static void Blurb(out float minX, out float minY, out float maxX, out float maxY)
        {
            minX = 0.20f;
            minY = 0.24f;
            maxX = 0.80f;
            maxY = 0.29f;
        }
    }
}
