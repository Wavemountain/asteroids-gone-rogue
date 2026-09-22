namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Atmos store-pass camera poses. F12 / editor menu / scripted hold all
    /// read this table. Unity-free numbers so tests can lock paths without Play.
    /// </summary>
    public static class StoreCapturePoses
    {
        public const string OutFolder = "Docs/StoreCaptures/out";
        public const string PlaceholderFolder = "Docs/StoreCaptures/placeholders";
        public const string CapsuleHex = "#D4A04A";
        public const float CapsuleR = 0.831f;
        public const float CapsuleG = 0.627f;
        public const float CapsuleB = 0.29f;

        public const string HangarShop = "01_hangar_shop_health_launchsign";
        public const string PlayVoid = "02_play_void_astrofloor";
        public const string CombatJuice = "03_combat_juice_bolt_spread";
        public const string BruteSwarm = "04_brute_swarm_beat";
        public const string FailOrWin = "05_fail_or_win";
        public const string RailCharge = "06_rail_charge";
        public const string Capsule = "capsule_ship_complete_34";

        public static readonly string[] ShotIds =
        {
            HangarShop,
            PlayVoid,
            CombatJuice,
            BruteSwarm,
            FailOrWin,
            RailCharge
        };

        public static string FileName(string id)
        {
            return id + ".png";
        }

        public static string PlaceholderName(string id)
        {
            return id + ".txt";
        }

        public static void Pose(string id, out float px, out float py, out float pz, out float lx, out float ly, out float lz, out float fov)
        {
            fov = 54f;
            if (id == PlayVoid)
            {
                px = 0f;
                py = 38f;
                pz = -26f;
                lx = 0f;
                ly = 0f;
                lz = 0f;
                return;
            }

            if (id == CombatJuice)
            {
                px = 4.2f;
                py = 18f;
                pz = -16f;
                lx = 0.4f;
                ly = 0.2f;
                lz = 1.2f;
                fov = 48f;
                return;
            }

            if (id == BruteSwarm)
            {
                px = -6f;
                py = 22f;
                pz = -18f;
                lx = 2f;
                ly = 0.4f;
                lz = 4f;
                fov = 50f;
                return;
            }

            if (id == FailOrWin)
            {
                px = 0f;
                py = 31f;
                pz = -19f;
                lx = 0.25f;
                ly = 0.4f;
                lz = 0f;
                return;
            }

            if (id == RailCharge)
            {
                px = 1.6f;
                py = 6.4f;
                pz = -8.2f;
                lx = 0.15f;
                ly = 0.45f;
                lz = 1.35f;
                fov = 36f;
                return;
            }

            if (id == Capsule)
            {
                px = -6.4f;
                py = 3.1f;
                pz = -4.8f;
                lx = -8.6f;
                ly = 0.55f;
                lz = 8.2f;
                fov = 32f;
                return;
            }

            px = 2.4f;
            py = 12.5f;
            pz = -11.5f;
            lx = 1.95f;
            ly = 0.8f;
            lz = -2.55f;
            fov = 46f;
        }
    }
}
