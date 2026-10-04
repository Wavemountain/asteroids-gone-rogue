namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Which run-start perks a seed receives. A positive daily seed is a daily
    /// run: the legacy starting shield and the sink start-shield stay off.
    /// Credit, hull, and discount bonuses are not these gates.
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
