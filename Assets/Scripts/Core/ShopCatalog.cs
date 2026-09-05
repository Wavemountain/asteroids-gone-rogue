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
                90)
        };
    }
}
