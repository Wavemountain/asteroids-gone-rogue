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
        public readonly int Cost;
        public readonly ShopGroup Group;
        private readonly string _title;
        private readonly string _description;

        public string Title
        {
            get { return Loc.T("shop.title." + Id, _title); }
        }

        public string Description
        {
            get { return Loc.T("shop.desc." + Id, _description); }
        }

        public ShopItem(UpgradeId id, string title, string description, int cost, ShopGroup group)
        {
            Id = id;
            _title = title;
            _description = description;
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
                UpgradeId.BodyUpgrade02,
                "Hull Plate 02",
                "Requires Body Upgrade. Extra hull plate (5 hits). Reuses the Upgrade01 mesh.",
                175,
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
                UpgradeId.NoseUpgrade03,
                "Nose Upgrade 03",
                "Requires Nose 02. 4-damage shots. Reuses the Nose 02 mesh.",
                200,
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
                UpgradeId.EngineUpgrade03,
                "Engine Upgrade 03",
                "Requires Engine 02. Faster cannon. Reuses the Engine 02 mesh.",
                190,
                ShopGroup.Hull),
            new ShopItem(
                UpgradeId.Overcharger,
                "Overcharger",
                "Nose branch. +1 damage, slightly slower gun. Locks Afterburner.",
                230,
                ShopGroup.Hull),
            new ShopItem(
                UpgradeId.Afterburner,
                "Afterburner",
                "Engine branch. Fastest cannon. Locks Overcharger.",
                230,
                ShopGroup.Hull),
            new ShopItem(
                UpgradeId.SpreadBolt,
                "Spread Bolt",
                "Shot mode: 3 amber pellets. Q / RMB to switch. Distinct from cyan pierce.",
                110,
                ShopGroup.Weapons),
            new ShopItem(
                UpgradeId.Pierce,
                "Pierce",
                "Shot mode: bolt goes through targets. Q / RMB to switch.",
                155,
                ShopGroup.Weapons),
            new ShopItem(
                UpgradeId.TwinGuns,
                "Twin Guns",
                "Shot mode: two parallel full-damage bolts. Not a spread fan.",
                140,
                ShopGroup.Weapons),
            new ShopItem(
                UpgradeId.Seeker,
                "Seeker",
                "Shot mode: magenta missile. Weaker homing, slower cadence, lower damage than bolt.",
                125,
                ShopGroup.Weapons),
            new ShopItem(
                UpgradeId.Ricochet,
                "Ricochet",
                "Shot mode: lime bolt that bounces off the arena rim (not pierce).",
                170,
                ShopGroup.Weapons),
            new ShopItem(
                UpgradeId.ShieldCell,
                "Shield Cell",
                "Adds one visible shield hit before hull damage (max 2, or 3 with Matrix).",
                80,
                ShopGroup.Defense),
            new ShopItem(
                UpgradeId.ShieldMatrix,
                "Shield Matrix",
                "Requires two Shield Cells. Raises shield cap to 3.",
                185,
                ShopGroup.Defense)
        };

        public static string HeaderFor(ShopGroup group)
        {
            switch (group)
            {
                case ShopGroup.Weapons:
                    return Loc.T("shop.header.weapons", WeaponsHeader);
                case ShopGroup.Defense:
                    return Loc.T("shop.header.defense", DefenseHeader);
                default:
                    return Loc.T("shop.header.hull", HullHeader);
            }
        }
    }
}
