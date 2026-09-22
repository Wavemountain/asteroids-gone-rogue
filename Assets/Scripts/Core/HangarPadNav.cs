namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Hangar D-pad / stick shop grid. Primary sits above a 4-col hull tree,
    /// weapons column, defense column. Unity-free so tests can walk neighbors
    /// without EventSystem. D-pad is not aliased onto move axes.
    /// </summary>
    public static class HangarPadNav
    {
        public const int PrimarySlot = 0;
        public const int ShopSlot0 = 1;
        public const float RepeatFirstSeconds = 0.28f;
        public const float RepeatNextSeconds = 0.14f;
        public const float Flick = 0.55f;

        public static int CreditsSlot
        {
            get { return ShopSlot0 + ShopCatalog.Items.Length; }
        }

        public static int EasySlot { get { return CreditsSlot + 1; } }
        public static int NormalSlot { get { return CreditsSlot + 2; } }
        public static int HardSlot { get { return CreditsSlot + 3; } }
        public static int LangEnSlot { get { return CreditsSlot + 4; } }
        public static int LangSvSlot { get { return CreditsSlot + 5; } }
        public static int MuteSlot { get { return CreditsSlot + 6; } }
        public static int GotItSlot { get { return CreditsSlot + 7; } }
        public static int BarrageSlot { get { return GotItSlot + 1; } }
        public static int LanceSlot { get { return GotItSlot + 2; } }
        public static int HunterSlot { get { return GotItSlot + 3; } }

        public static int SlotCount
        {
            get { return HunterSlot + 1; }
        }

        public static int ShopSlot(int shopIndex)
        {
            return ShopSlot0 + shopIndex;
        }

        public static bool TryShopIndex(int slot, out int shopIndex)
        {
            shopIndex = slot - ShopSlot0;
            return shopIndex >= 0 && shopIndex < ShopCatalog.Items.Length;
        }

        public static int Step(int slot, int dx, int dy)
        {
            if (dx == 0 && dy == 0)
            {
                return ClampSlot(slot);
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

            int from = ClampSlot(slot);
            int x;
            int y;
            Coord(from, out x, out y);

            int exact = FindAt(x + dx, y + dy);
            if (exact >= 0)
            {
                return exact;
            }

            int along = FindAlong(from, x, y, dx, dy, false);
            if (along >= 0)
            {
                return along;
            }

            int wrap = FindAlong(from, x, y, dx, dy, true);
            return wrap >= 0 ? wrap : from;
        }

        public static int DominantStep(float x, float y, float flick)
        {
            float ax = x < 0f ? -x : x;
            float ay = y < 0f ? -y : y;
            if (ax < flick && ay < flick)
            {
                return 0;
            }

            if (ax >= ay)
            {
                return x > 0f ? 1 : -1;
            }

            return 0;
        }

        public static int DominantStepY(float x, float y, float flick)
        {
            float ax = x < 0f ? -x : x;
            float ay = y < 0f ? -y : y;
            if (ax < flick && ay < flick)
            {
                return 0;
            }

            if (ay > ax)
            {
                // Stick / D-pad up is +Y; hangar grid +Y walks down the shop, so invert.
                return y > 0f ? -1 : 1;
            }

            return 0;
        }

        public static bool LayoutSelfCheck()
        {
            int hullNose02 = ShopSlot(3);
            int spread = ShopSlot(10);
            int shield = ShopSlot(15);
            return Step(PrimarySlot, 0, 1) == ShopSlot(1)
                && Step(ShopSlot(1), 0, -1) == PrimarySlot
                && Step(hullNose02, 1, 0) == spread
                && Step(spread, -1, 0) == hullNose02
                && Step(spread, 1, 0) == shield
                && Step(shield, -1, 0) == spread
                && Step(PrimarySlot, 0, -1) == NormalSlot
                && Step(NormalSlot, 0, 1) == PrimarySlot
                && Step(EasySlot, 1, 0) == NormalSlot
                && Step(LangEnSlot, 1, 0) == LangSvSlot
                && Step(LangSvSlot, 1, 0) == MuteSlot
                && Step(CreditsSlot, 0, -1) != CreditsSlot
                && DominantStepY(0f, -1f, Flick) == 1
                && DominantStepY(0f, 1f, Flick) == -1
                && DominantStep(1f, 0f, Flick) == 1;
        }

        private static int ClampSlot(int slot)
        {
            if (slot < 0)
            {
                return PrimarySlot;
            }

            int last = SlotCount - 1;
            return slot > last ? last : slot;
        }

        private static void Coord(int slot, out int x, out int y)
        {
            if (slot <= PrimarySlot)
            {
                x = 1;
                y = -1;
                return;
            }

            if (slot == CreditsSlot)
            {
                x = 0;
                y = 8;
                return;
            }

            if (slot == EasySlot)
            {
                x = 0;
                y = -3;
                return;
            }

            if (slot == NormalSlot)
            {
                x = 1;
                y = -3;
                return;
            }

            if (slot == HardSlot)
            {
                x = 2;
                y = -3;
                return;
            }

            if (slot == LangEnSlot)
            {
                x = 3;
                y = -3;
                return;
            }

            if (slot == LangSvSlot)
            {
                x = 4;
                y = -3;
                return;
            }

            if (slot == MuteSlot)
            {
                x = 5;
                y = -3;
                return;
            }

            if (slot == GotItSlot)
            {
                x = 0;
                y = -4;
                return;
            }

            if (slot == BarrageSlot)
            {
                x = 6;
                y = -2;
                return;
            }

            if (slot == LanceSlot)
            {
                x = 7;
                y = -2;
                return;
            }

            if (slot == HunterSlot)
            {
                x = 8;
                y = -2;
                return;
            }

            int hull = 0;
            int weapon = 0;
            int defense = 0;
            int doctrine = 0;
            int shopIndex = slot - ShopSlot0;
            for (int i = 0; i < ShopCatalog.Items.Length; i++)
            {
                ShopGroup group = ShopCatalog.Items[i].Group;
                int cx;
                int cy;
                if (group == ShopGroup.Weapons)
                {
                    cx = 4;
                    cy = weapon;
                    weapon++;
                }
                else if (group == ShopGroup.Defense)
                {
                    cx = 5;
                    cy = defense;
                    defense++;
                }
                else if (group == ShopGroup.Doctrine)
                {
                    cx = 6 + (doctrine % 3);
                    cy = -5 - (doctrine / 3);
                    doctrine++;
                }
                else
                {
                    cx = hull % 4;
                    cy = hull / 4;
                    hull++;
                }

                if (i == shopIndex)
                {
                    x = cx;
                    y = cy;
                    return;
                }
            }

            x = 1;
            y = -1;
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

        private static int FindAlong(int from, int x, int y, int dx, int dy, bool wrap)
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
                    dirOk = wrap ? Sign(delx) == -Sign(dx) : Sign(delx) == Sign(dx);
                    int yDist = dely < 0 ? -dely : dely;
                    int xDist = wrap ? (delx < 0 ? -delx : delx) : (dx > 0 ? delx : -delx);
                    score = yDist * 20 + xDist;
                }
                else
                {
                    dirOk = wrap ? Sign(dely) == -Sign(dy) : Sign(dely) == Sign(dy);
                    int xDist = delx < 0 ? -delx : delx;
                    int yDist = wrap ? (dely < 0 ? -dely : dely) : (dy > 0 ? dely : -dely);
                    score = xDist * 20 + yDist;
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
