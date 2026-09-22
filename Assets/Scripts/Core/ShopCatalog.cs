namespace AsteroidsGoneRogue
{
    public enum ShopGroup
    {
        Hull,
        Weapons,
        Defense,
        Doctrine
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
                "Primary slot: 3 amber pellets. LB / Q to cycle. Distinct from cyan pierce.",
                110,
                ShopGroup.Weapons),
            new ShopItem(
                UpgradeId.Pierce,
                "Pierce",
                "Primary slot: bolt goes through targets. LB / Q to cycle.",
                155,
                ShopGroup.Weapons),
            new ShopItem(
                UpgradeId.TwinGuns,
                "Twin Guns",
                "Primary slot: two parallel full-damage bolts. Not a spread fan.",
                140,
                ShopGroup.Weapons),
            new ShopItem(
                UpgradeId.Seeker,
                "Seeker",
                "Utility slot: magenta missile. Hold LT / E. Own cooldown. Weaker homing, slower cadence, −1 damage.",
                125,
                ShopGroup.Weapons),
            new ShopItem(
                UpgradeId.Ricochet,
                "Ricochet",
                "Utility slot: lime bolt, 2 rim bounces. Hold LT / E. Wildcard on every doctrine. No capstone.",
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
                ShopGroup.Defense),
            new ShopItem(
                UpgradeId.Rail,
                "Rail",
                "Lance mid. Primary: hold RT 0.55s, release. Damage ×3, speed ×1.2, no pierce. CD ×1.6. Miss or cancel pays half.",
                160,
                ShopGroup.Doctrine),
            new ShopItem(
                UpgradeId.FlakFeed,
                "Flak Feed",
                "Barrage mid. Spread cooldown ×0.85 and half-angle +4°. Soft-locks other paths.",
                150,
                ShopGroup.Doctrine),
            new ShopItem(
                UpgradeId.Storm,
                "Storm",
                "Barrage capstone. Spread fires 5 pellets. Cooldown ×1.8, at least 1.5s.",
                240,
                ShopGroup.Doctrine),
            new ShopItem(
                UpgradeId.OverchargeLance,
                "Overcharge Lance",
                "Lance capstone. Pierce hits +1 target. Twin cooldown ×0.9.",
                235,
                ShopGroup.Doctrine),
            new ShopItem(
                UpgradeId.SeekerCadence,
                "Seeker Cadence",
                "Hunter mid. Utility cooldown ×0.75. Seeker turn 165. Soft-locks other paths.",
                155,
                ShopGroup.Doctrine),
            new ShopItem(
                UpgradeId.TwinSeek,
                "Twin Seek",
                "Hunter capstone. Hold LT for 2 seekers at 70% damage. Utility cooldown ×1.2.",
                225,
                ShopGroup.Doctrine)
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
