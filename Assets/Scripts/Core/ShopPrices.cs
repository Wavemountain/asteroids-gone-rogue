namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Hangar prices that are not baked into <see cref="ShopCatalog"/>.
    /// World scaling starts at world 3. Mk II is 1.6× the item's base price.
    /// Unity-free.
    /// </summary>
    public static class ShopPrices
    {
        public const int Mk2Percent = 160;
        public const int WorldScaleStart = 3;
        public const int WorldScaleStepPercent = 6;
        public const int WorldScaleCapPercent = 36;
        public const int HullRepairCost = 60;
        public const int ExtraLifeCost = 400;
        public const int ShieldRefillCost = 40;
        public const int BankCreditsPerPoint = 500;
        public const int BankMaxPerRun = 3;
        public const float Mk2CooldownMul = 0.92f;

        public static int WorldPercent(int worldNumber)
        {
            int world = worldNumber < 1 ? 1 : worldNumber;
            if (world < WorldScaleStart)
            {
                return 100;
            }

            int bonus = (world - (WorldScaleStart - 1)) * WorldScaleStepPercent;
            if (bonus > WorldScaleCapPercent)
            {
                bonus = WorldScaleCapPercent;
            }

            if (bonus < 0)
            {
                bonus = 0;
            }

            return 100 + bonus;
        }

        public static int ApplyWorld(int cost, int worldNumber)
        {
            if (cost <= 0)
            {
                return 0;
            }

            int scaled = cost * WorldPercent(worldNumber) / 100;
            if (scaled < 1)
            {
                scaled = 1;
            }

            return scaled;
        }

        public static int Mk2Cost(int baseCost)
        {
            if (baseCost <= 0)
            {
                return 0;
            }

            int cost = baseCost * Mk2Percent / 100;
            if (cost < 1)
            {
                cost = 1;
            }

            return cost;
        }
    }
}
