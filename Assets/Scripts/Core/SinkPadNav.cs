namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Pad grid for the bay overlay. Six tiles, then Close. Unity-free.
    /// </summary>
    public static class SinkPadNav
    {
        public const int TileCount = 6;
        public const int CloseSlot = 6;
        public const int SlotCount = 7;

        public static int Step(int slot, int dx, int dy)
        {
            if (dx == 0 && dy == 0)
            {
                return Clamp(slot);
            }

            if (dx != 0 && dy != 0)
            {
                if (dx * dx >= dy * dy)
                {
                    dy = 0;
                }
                else
                {
                    dx = 0;
                }
            }

            int from = Clamp(slot);
            int x;
            int y;
            Coord(from, out x, out y);
            int exact = FindAt(x + dx, y + dy);
            if (exact >= 0)
            {
                return exact;
            }

            int along = FindAlong(from, x, y, dx, dy);
            if (along >= 0)
            {
                return along;
            }

            return from;
        }

        public static bool SelfCheck()
        {
            return Step(0, 1, 0) == 1
                && Step(1, -1, 0) == 0
                && Step(0, 0, 1) == 2
                && Step(4, 0, 1) == CloseSlot
                && Step(5, 0, 1) == CloseSlot
                && Step(CloseSlot, 0, -1) != CloseSlot
                && Step(-1, 0, 0) == 0
                && ShopSinkCatalog.PricesStayPositive();
        }

        private static int Clamp(int slot)
        {
            if (slot < 0)
            {
                return 0;
            }

            if (slot >= SlotCount)
            {
                return SlotCount - 1;
            }

            return slot;
        }

        public static void Coord(int slot, out int x, out int y)
        {
            if (slot == CloseSlot)
            {
                x = 0;
                y = 3;
                return;
            }

            int tile = slot;
            if (tile < 0)
            {
                tile = 0;
            }

            if (tile >= TileCount)
            {
                tile = TileCount - 1;
            }

            x = tile % 2;
            y = tile / 2;
        }

        private static int FindAt(int x, int y)
        {
            for (int slot = 0; slot < SlotCount; slot++)
            {
                int sx;
                int sy;
                Coord(slot, out sx, out sy);
                if (sx == x && sy == y)
                {
                    return slot;
                }
            }

            return -1;
        }

        private static int FindAlong(int from, int x, int y, int dx, int dy)
        {
            int best = -1;
            int bestScore = int.MaxValue;
            for (int slot = 0; slot < SlotCount; slot++)
            {
                if (slot == from)
                {
                    continue;
                }

                int sx;
                int sy;
                Coord(slot, out sx, out sy);
                int delx = sx - x;
                int dely = sy - y;
                bool dirOk;
                int score;
                if (dx != 0)
                {
                    dirOk = Sign(delx) == Sign(dx);
                    int yDist = dely < 0 ? -dely : dely;
                    int xDist = dx > 0 ? delx : -delx;
                    score = (yDist * 20) + xDist;
                }
                else
                {
                    dirOk = Sign(dely) == Sign(dy);
                    int xDist = delx < 0 ? -delx : delx;
                    int yDist = dy > 0 ? dely : -dely;
                    score = (xDist * 20) + yDist;
                }

                if (!dirOk || score >= bestScore)
                {
                    continue;
                }

                bestScore = score;
                best = slot;
            }

            return best;
        }

        private static int Sign(int value)
        {
            if (value > 0)
            {
                return 1;
            }

            if (value < 0)
            {
                return -1;
            }

            return 0;
        }
    }
}
