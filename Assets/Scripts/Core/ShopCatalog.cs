namespace AsteroidsGoneRogue
{
    public enum ShopGroup
    {
        Hull,
        Weapons,
        Defense
    }

    public sealed class ShopItem
    {
        public readonly UpgradeId Id;
        public readonly string Title;
        public readonly string Description;
        public readonly int Cost;
        public readonly ShopGroup Group;

        public ShopItem(UpgradeId id, string title, string description, int cost, ShopGroup group)
        {
            Id = id;
            Title = title;
            Description = description;
            Cost = cost;
            Group = group;
        }
    }

    public static class ShopCatalog
    {
        public const string HullHeader = "HULL / NOSE / ENGINE";
        public const string WeaponsHeader = "WEAPONS";
        public const string DefenseHeader = "DEFENSE";

        public static readonly ShopItem[] Items =
        {
            new ShopItem(
                UpgradeId.BodyUpgrade01,
                "Body Upgrade",
                "Swaps the hull to Ship_Body_Upgrade01 and adds 1 hull hit.",
                90,
                ShopGroup.Hull),
            new ShopItem(
                UpgradeId.NoseHardpoint,
                "Nose Hardpoint",
                "Swaps the nose slot for faster, harder-hitting shots.",
                120,
                ShopGroup.Hull),
            new ShopItem(
                UpgradeId.NoseUpgrade02,
                "Nose Upgrade 02",
                "Requires Nose Hardpoint. Swaps to Ship_Nose_Upgrade02 (3 damage).",
                150,
                ShopGroup.Hull),
            new ShopItem(
                UpgradeId.RapidFire,
                "Rapid Fire",
                "Cuts cannon cooldown nearly in half and swaps the engine slot.",
                100,
                ShopGroup.Hull),
            new ShopItem(
                UpgradeId.EngineUpgrade02,
                "Engine Upgrade 02",
                "Requires Rapid Fire. Swaps to Ship_Engine_Upgrade02 (faster gun).",
                140,
                ShopGroup.Hull),
            new ShopItem(
                UpgradeId.SpreadBolt,
                "Spread Bolt",
                "Second shot mode: 3 amber pellets. Q / RMB to switch. Distinct from cyan pierce.",
                110,
                ShopGroup.Weapons),
            new ShopItem(
                UpgradeId.Pierce,
                "Pierce",
                "Second shot mode: bolt goes through targets. Q / RMB to switch.",
                130,
                ShopGroup.Weapons),
            new ShopItem(
                UpgradeId.ShieldCell,
                "Shield Cell",
                "Adds one visible shield hit before hull damage (max 2).",
                80,
                ShopGroup.Defense)
        };

        public static string HeaderFor(ShopGroup group)
        {
            switch (group)
            {
                case ShopGroup.Weapons:
                    return WeaponsHeader;
                case ShopGroup.Defense:
                    return DefenseHeader;
                default:
                    return HullHeader;
            }
        }
    }
}
