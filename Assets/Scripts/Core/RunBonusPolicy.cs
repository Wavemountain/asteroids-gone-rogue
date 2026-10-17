namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Which run-start perks a seed receives. A positive daily seed is a daily
    /// run: legacy credits, hull, the first-shop discount, the legacy starting
    /// shield, and the sink start-shield stay off. A normal run keeps them.
    /// </summary>
    public static class RunBonusPolicy
    {
        public static bool AppliesLegacy(int dailySeed)
        {
            return dailySeed <= 0;
        }

        public static bool AppliesSinkShield(int dailySeed)
        {
            return dailySeed <= 0;
        }
    }
}
