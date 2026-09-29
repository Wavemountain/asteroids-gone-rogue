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
        public static int DoctrineHintSlot { get { return HunterSlot + 1; } }

        public static int SlotCount
        {
            get { return DoctrineHintSlot + 1; }
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
            return Step(slot, dx, dy, null);
        }

        /// <summary>
        /// One grid step. When <paramref name="selectable"/> is set, hidden and
        /// non-interactable slots are not candidates, so a locked cell cannot
        /// steal the move from a farther live control.
        /// </summary>
        public static int Step(int slot, int dx, int dy, bool[] selectable)
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

            int exact = FindAt(x + dx, y + dy, selectable);
            if (exact >= 0)
            {
                return exact;
            }

            int along = FindAlong(from, x, y, dx, dy, false, selectable);
            if (along >= 0)
            {
                return along;
            }

            int wrap = FindAlong(from, x, y, dx, dy, true, selectable);
            return wrap >= 0 ? wrap : from;
        }

        /// <summary>
        /// Slot 0 is the Next Wave / Start Wave control. The nav list always contains it.
        /// </summary>
        public static bool NavIncludesPrimary()
        {
            return PrimarySlot >= 0 && PrimarySlot < SlotCount;
        }

        public static void ForcePrimarySelectable(bool[] selectable)
        {
            if (selectable == null || selectable.Length <= PrimarySlot)
            {
                return;
            }

            selectable[PrimarySlot] = true;
        }

        /// <summary>
        /// Invalid, hidden, owned, or locked selection falls back to Next Wave.
        /// A selectable slot is left alone.
        /// </summary>
        public static int ResolveFallback(int slot, bool[] selectable)
        {
            if (selectable == null || selectable.Length == 0)
            {
                return PrimarySlot;
            }

            if (slot >= 0 && slot < selectable.Length && selectable[slot])
            {
                return slot;
            }

            if (PrimarySlot < selectable.Length && selectable[PrimarySlot])
            {
                return PrimarySlot;
            }

            return PrimarySlot;
        }

        /// <summary>
        /// Walk one D-pad step, skipping slots that are not selectable.
        /// An invalid current slot snaps to Next Wave and does not consume the step,
        /// so a purchase that disables the focused row cannot leave the stick on a dead cell.
        /// If the direction has no selectable slot, the result is Next Wave.
        /// </summary>
        public static int StepSelectable(int slot, int dx, int dy, bool[] selectable)
        {
            ForcePrimarySelectable(selectable);
            bool currentOk = selectable != null
                && slot >= 0
                && slot < selectable.Length
                && selectable[slot];
            int origin = currentOk ? slot : PrimarySlot;
            if (!currentOk || (dx == 0 && dy == 0))
            {
                return origin;
            }

            int count = selectable.Length;
            int candidate = Step(origin, dx, dy, selectable);
            int guard = 0;
            while (guard < count)
            {
                if (candidate >= 0 && candidate < count && selectable[candidate])
                {
                    return candidate;
                }

                int stepped = Step(candidate, dx, dy, selectable);
                if (stepped == candidate)
                {
                    return PrimarySlot;
                }

                candidate = stepped;
                guard++;
            }

            return PrimarySlot;
        }

        public static bool Overlaps(
            float ax0,
            float ay0,
            float ax1,
            float ay1,
            float bx0,
            float by0,
            float bx1,
            float by1)
        {
            return ax0 < bx1 && ax1 > bx0 && ay0 < by1 && ay1 > by0;
        }

        public static void MapAnchors(
            float parentX0,
            float parentY0,
            float parentX1,
            float parentY1,
            float childX0,
            float childY0,
            float childX1,
            float childY1,
            out float screenX0,
            out float screenY0,
            out float screenX1,
            out float screenY1)
        {
            float width = parentX1 - parentX0;
            float height = parentY1 - parentY0;
            screenX0 = parentX0 + childX0 * width;
            screenY0 = parentY0 + childY0 * height;
            screenX1 = parentX0 + childX1 * width;
            screenY1 = parentY0 + childY1 * height;
        }

        /// <summary>
        /// Screen-space Next Wave rect against the wave-clear strip, shop headers,
        /// first-flight card, doctrine card, loadout preview, and top bar.
        /// </summary>
        public static bool NextWaveScreenClear()
        {
            float waveX0;
            float waveY0;
            float waveX1;
            float waveY1;
            MapAnchors(0.014f, 0.080f, 0.55f, 0.888f, 0.03f, 0.735f, 0.97f, 0.800f, out waveX0, out waveY0, out waveX1, out waveY1);

            float stripX0;
            float stripY0;
            float stripX1;
            float stripY1;
            MapAnchors(0.014f, 0.080f, 0.55f, 0.888f, 0.02f, 0.82f, 0.98f, 0.995f, out stripX0, out stripY0, out stripX1, out stripY1);

            float headX0;
            float headY0;
            float headX1;
            float headY1;
            MapAnchors(0.014f, 0.080f, 0.55f, 0.888f, 0.02f, 0.675f, 0.98f, 0.728f, out headX0, out headY0, out headX1, out headY1);

            if (Overlaps(waveX0, waveY0, waveX1, waveY1, stripX0, stripY0, stripX1, stripY1))
            {
                return false;
            }

            if (Overlaps(waveX0, waveY0, waveX1, waveY1, headX0, headY0, headX1, headY1))
            {
                return false;
            }

            if (Overlaps(waveX0, waveY0, waveX1, waveY1, 0.018f, 0.730f, 0.545f, 0.888f))
            {
                return false;
            }

            if (Overlaps(waveX0, waveY0, waveX1, waveY1, 0.562f, 0.608f, 0.986f, 0.898f))
            {
                return false;
            }

            if (Overlaps(waveX0, waveY0, waveX1, waveY1, 0.562f, 0.080f, 0.986f, 0.596f))
            {
                return false;
            }

            if (Overlaps(waveX0, waveY0, waveX1, waveY1, 0.012f, 0.905f, 0.988f, 0.995f))
            {
                return false;
            }

            return waveY1 <= 0.730f && waveY0 >= 0.080f;
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
                && DoctrineShopStripSharesNextWaveRow()
                && Step(PrimarySlot, 0, -1) == NormalSlot
                && Step(NormalSlot, 0, 1) == PrimarySlot
                && Step(EasySlot, 1, 0) == NormalSlot
                && Step(LangEnSlot, 1, 0) == LangSvSlot
                && Step(LangSvSlot, 1, 0) == MuteSlot
                && Step(CreditsSlot, 0, -1) != CreditsSlot
                && DominantStepY(0f, -1f, Flick) == 1
                && DominantStepY(0f, 1f, Flick) == -1
                && DominantStep(1f, 0f, Flick) == 1
                && NavIncludesPrimary()
                && NextWaveScreenClear();
        }

        public static bool LockedShopFallsBackToPrimary()
        {
            bool[] onlyPrimary = new bool[SlotCount];
            ForcePrimarySelectable(onlyPrimary);
            int lockedShop = ShopSlot(0);
            int cardUp = Step(BarrageSlot, 0, -1);
            int cardShopIndex;
            bool cardUpIsShop = TryShopIndex(cardUp, out cardShopIndex);
            return !cardUpIsShop
                && cardUp != BarrageSlot
                && ResolveFallback(lockedShop, onlyPrimary) == PrimarySlot
                && StepSelectable(lockedShop, 0, -1, onlyPrimary) == PrimarySlot
                && StepSelectable(BarrageSlot, 1, 0, onlyPrimary) == PrimarySlot
                && StepSelectable(PrimarySlot, 0, 0, onlyPrimary) == PrimarySlot;
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

            if (slot == DoctrineHintSlot)
            {
                x = 8;
                y = -1;
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
                    // Two columns inside the doctrine card strip, same screen band
                    // as Next Wave (see DoctrinePadCell). Not the far y=5 cells.
                    int doctrineX;
                    int doctrineY;
                    DoctrinePadCell(doctrine, out doctrineX, out doctrineY);
                    cx = doctrineX;
                    cy = doctrineY;
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

        private static int FindAt(int x, int y, bool[] selectable)
        {
            for (int slot = 0; slot < SlotCount; slot++)
            {
                if (!Accepts(slot, selectable))
                {
                    continue;
                }

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

        private static int FindAlong(int from, int x, int y, int dx, int dy, bool wrap, bool[] selectable)
        {
            int best = -1;
            int bestScore = int.MaxValue;
            for (int slot = 0; slot < SlotCount; slot++)
            {
                if (slot == from || !Accepts(slot, selectable))
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

        private static bool Accepts(int slot, bool[] selectable)
        {
            if (selectable == null)
            {
                return true;
            }

            return slot >= 0 && slot < selectable.Length && selectable[slot];
        }

        /// <summary>
        /// Doctrine shop buttons are parented to the card strip (GameUi doctrine
        /// shop row 0.190–0.397, two columns). That strip shares a screen row
        /// with Next Wave and sits to the right of the defense column (pad x = 5).
        /// </summary>
        private static void DoctrinePadCell(int doctrineIndex, out int padX, out int padY)
        {
            float shopMidY = DoctrineShopMidY();
            float nextWaveMidY = NextWaveMidY();
            float shopPitch = (0.094f + 0.016f) * (0.888f - 0.080f);
            float deltaY = shopMidY - nextWaveMidY;
            if (deltaY < 0f)
            {
                deltaY = -deltaY;
            }

            padX = 6 + (doctrineIndex % 2);
            padY = deltaY <= shopPitch * 0.5f ? -1 : 5;
        }

        private static float NextWaveMidY()
        {
            float waveX0;
            float waveY0;
            float waveX1;
            float waveY1;
            MapAnchors(0.014f, 0.080f, 0.55f, 0.888f, 0.03f, 0.735f, 0.97f, 0.800f, out waveX0, out waveY0, out waveX1, out waveY1);
            if (waveX1 <= waveX0)
            {
                return 0f;
            }

            return (waveY0 + waveY1) * 0.5f;
        }

        private static float DoctrineShopMidY()
        {
            float rowX0;
            float rowY0;
            float rowX1;
            float rowY1;
            MapAnchors(0.562f, 0.608f, 0.986f, 0.898f, 0.012f, 0.190f, 0.988f, 0.397f, out rowX0, out rowY0, out rowX1, out rowY1);
            float btnX0;
            float btnY0;
            float btnX1;
            float btnY1;
            MapAnchors(rowX0, rowY0, rowX1, rowY1, 0.04f, 0.08f, 0.48f, 0.92f, out btnX0, out btnY0, out btnX1, out btnY1);
            if (btnX1 <= btnX0 || rowX1 <= rowX0)
            {
                return 0f;
            }

            return (btnY0 + btnY1) * 0.5f;
        }

        /// <summary>
        /// Screen centers of the doctrine shop strip and Next Wave share one row,
        /// and the pad cells for the two visual columns sit on that row.
        /// </summary>
        public static bool DoctrineShopStripSharesNextWaveRow()
        {
            float waveX0;
            float waveY0;
            float waveX1;
            float waveY1;
            MapAnchors(0.014f, 0.080f, 0.55f, 0.888f, 0.03f, 0.735f, 0.97f, 0.800f, out waveX0, out waveY0, out waveX1, out waveY1);

            float stripX0;
            float stripY0;
            float stripX1;
            float stripY1;
            MapAnchors(0.562f, 0.608f, 0.986f, 0.898f, 0.012f, 0.190f, 0.988f, 0.397f, out stripX0, out stripY0, out stripX1, out stripY1);

            float leftX0;
            float leftY0;
            float leftX1;
            float leftY1;
            MapAnchors(stripX0, stripY0, stripX1, stripY1, 0.04f, 0.08f, 0.48f, 0.92f, out leftX0, out leftY0, out leftX1, out leftY1);

            float rightX0;
            float rightY0;
            float rightX1;
            float rightY1;
            MapAnchors(stripX0, stripY0, stripX1, stripY1, 0.52f, 0.08f, 0.96f, 0.92f, out rightX0, out rightY0, out rightX1, out rightY1);

            float waveMidY = (waveY0 + waveY1) * 0.5f;
            float leftMidY = (leftY0 + leftY1) * 0.5f;
            float rowPitch = (0.094f + 0.016f) * (0.888f - 0.080f);
            float bandDy = leftMidY - waveMidY;
            if (bandDy < 0f)
            {
                bandDy = -bandDy;
            }

            int padLeftX;
            int padLeftY;
            int padRightX;
            int padRightY;
            DoctrinePadCell(0, out padLeftX, out padLeftY);
            DoctrinePadCell(1, out padRightX, out padRightY);
            return bandDy <= rowPitch * 0.5f
                && leftX0 > waveX1
                && rightX0 > leftX1
                && padLeftY == -1
                && padRightY == -1
                && padRightX == padLeftX + 1
                && waveX0 < waveX1
                && stripY0 < stripY1
                && rightY0 < rightY1;
        }
    }
}
