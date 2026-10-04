namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Late-game credit sinks. Not part of <see cref="ShopCatalog"/>, so the
    /// wave-35 catalogue stays 9883. Unity-free.
    /// </summary>
    public static class ShopSinkCatalog
    {
        public const int PaintAmber = 0;
        public const int PaintSteel = 1;
        public const int TrailCyan = 2;
        public const int TrailAmber = 3;
        public const int StartShield = 4;
        public const int PickupReach = 5;
        public const int Count = 6;

        public const int PaintPrice = 420;
        public const int TrailPrice = 380;
        public const int ShieldPrice = 640;
        public const int ReachPrice0 = 520;
        public const int ReachPrice1 = 980;
        public const int ShieldCap = 1;
        public const int ReachCap = 2;
        public const int ReachPercentPerRank = 5;

        public static bool IsPaint(int sinkId)
        {
            return sinkId == PaintAmber || sinkId == PaintSteel;
        }

        public static bool IsTrail(int sinkId)
        {
            return sinkId == TrailCyan || sinkId == TrailAmber;
        }

        public static bool IsCosmetic(int sinkId)
        {
            return IsPaint(sinkId) || IsTrail(sinkId);
        }

        public static int Cap(int sinkId)
        {
            if (sinkId == StartShield)
            {
                return ShieldCap;
            }

            if (sinkId == PickupReach)
            {
                return ReachCap;
            }

            if (IsCosmetic(sinkId))
            {
                return 1;
            }

            return 0;
        }

        public static int Price(int sinkId, int ownedRank)
        {
            if (sinkId < 0 || sinkId >= Count)
            {
                return 0;
            }

            int rank = ownedRank < 0 ? 0 : ownedRank;
            if (sinkId == StartShield)
            {
                if (rank >= ShieldCap)
                {
                    return 0;
                }

                return ShieldPrice;
            }

            if (sinkId == PickupReach)
            {
                if (rank >= ReachCap)
                {
                    return 0;
                }

                if (rank <= 0)
                {
                    return ReachPrice0;
                }

                return ReachPrice1;
            }

            if (IsPaint(sinkId))
            {
                return PaintPrice;
            }

            if (IsTrail(sinkId))
            {
                return TrailPrice;
            }

            return 0;
        }

        public static bool PricesStayPositive()
        {
            if (PaintPrice < 1 || TrailPrice < 1 || ShieldPrice < 1)
            {
                return false;
            }

            if (ReachPrice0 < 1 || ReachPrice1 <= ReachPrice0)
            {
                return false;
            }

            for (int id = 0; id < Count; id++)
            {
                int first = Price(id, 0);
                if (first < 1)
                {
                    return false;
                }

                int cap = Cap(id);
                if (cap < 1)
                {
                    return false;
                }

                if (Price(id, cap) != 0)
                {
                    return false;
                }
            }

            return Price(PickupReach, 1) > Price(PickupReach, 0);
        }

        public static float ReachMultiplier(int rank)
        {
            int clamped = rank;
            if (clamped < 0)
            {
                clamped = 0;
            }

            if (clamped > ReachCap)
            {
                clamped = ReachCap;
            }

            return (100f + (clamped * ReachPercentPerRank)) / 100f;
        }

        public static void PaintRgb(int paintId, out float red, out float green, out float blue)
        {
            if (paintId == PaintSteel)
            {
                red = 0.416f;
                green = 0.659f;
                blue = 0.784f;
                return;
            }

            red = 0.831f;
            green = 0.627f;
            blue = 0.290f;
        }

        public static void TrailRgb(int trailId, out float red, out float green, out float blue)
        {
            if (trailId == TrailAmber)
            {
                red = 0.831f;
                green = 0.627f;
                blue = 0.290f;
                return;
            }

            red = 0.416f;
            green = 0.784f;
            blue = 0.784f;
        }
    }
}
