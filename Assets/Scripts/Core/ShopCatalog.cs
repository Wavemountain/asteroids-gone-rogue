namespace AsteroidsGoneRogue
{
    public sealed class ShopItem
    {
        public readonly UpgradeId Id;
        public readonly string Title;
        public readonly string Description;
        public readonly int Cost;

        public ShopItem(UpgradeId id, string title, string description, int cost)
        {
            Id = id;
            Title = title;
            Description = description;
            Cost = cost;
        }
    }

    public static class ShopCatalog
    {
        public static readonly ShopItem[] Items =
        {
            new ShopItem(
                UpgradeId.RapidFire,
                "Rapid Fire",
                "Cuts cannon cooldown nearly in half and swaps the engine slot.",
                100),
            new ShopItem(
                UpgradeId.ShieldCell,
                "Shield Cell",
                "Adds one visible shield hit before hull damage (max 2).",
                80),
            new ShopItem(
                UpgradeId.NoseHardpoint,
                "Nose Hardpoint",
                "Swaps the nose slot for faster, harder-hitting shots.",
                120),
            new ShopItem(
                UpgradeId.BodyUpgrade01,
                "Body Upgrade",
                "Swaps the hull to Ship_Body_Upgrade01 and adds 1 hull hit.",
                90),
            new ShopItem(
                UpgradeId.NoseUpgrade02,
                "Nose Upgrade 02",
                "Requires Nose Hardpoint. Swaps to Ship_Nose_Upgrade02 (3 damage).",
                150),
            new ShopItem(
                UpgradeId.EngineUpgrade02,
                "Engine Upgrade 02",
                "Requires Rapid Fire. Swaps to Ship_Engine_Upgrade02 (faster gun).",
                140),
            new ShopItem(
                UpgradeId.SpreadBolt,
                "Spread Bolt",
                "Second shot mode: 3 lower-damage pellets. Q / RMB to switch. Reuses bolt visuals.",
                110),
            new ShopItem(
                UpgradeId.Pierce,
                "Pierce",
                "Second shot mode: bolt goes through targets. Q / RMB to switch.",
                130)
        };
    }
}
