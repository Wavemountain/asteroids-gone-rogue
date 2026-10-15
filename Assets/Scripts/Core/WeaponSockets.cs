namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Hero weapon mounts. Each FBX origin sits on the socket and the barrel
    /// runs local +Z. Doctrine does not hide an owned gun and does not invent
    /// one; old saves with an out-of-range doctrine keep the equipped bits.
    /// The capsule hitbox stays the pre-hero footprint.
    /// </summary>
    public static class WeaponSockets
    {
        public const int Rail = 0;
        public const int Twin = 1;
        public const int Pierce = 2;
        public const int Seeker = 3;
        public const int Count = 4;

        public const float MuzzleZ = 1.65f;
        public const float HeroSpanMeters = 3.3f;

        public static string Name(int socket)
        {
            switch (socket)
            {
                case Rail:
                    return "Rail";
                case Twin:
                    return "Twin";
                case Pierce:
                    return "Pierce";
                case Seeker:
                    return "Seeker";
                default:
                    return string.Empty;
            }
        }

        public static string AssetName(int socket)
        {
            switch (socket)
            {
                case Rail:
                    return "Weapon_Rail";
                case Twin:
                    return "Weapon_Twin";
                case Pierce:
                    return "Weapon_Pierce";
                case Seeker:
                    return "Weapon_Seeker";
                default:
                    return string.Empty;
            }
        }

        public static string VisualName(int socket)
        {
            switch (socket)
            {
                case Rail:
                    return "RailHardpoint";
                case Twin:
                    return "TwinBarrels";
                case Pierce:
                    return "PierceNeedle";
                case Seeker:
                    return "SeekerRail";
                default:
                    return string.Empty;
            }
        }

        public static float AuthoredLength(int socket)
        {
            switch (socket)
            {
                case Rail:
                    return 0.78f;
                case Twin:
                    return 0.50f;
                case Pierce:
                    return 0.60f;
                case Seeker:
                    return 0.33f;
                default:
                    return 0f;
            }
        }

        public static void Mount(int socket, out float x, out float y, out float z)
        {
            x = 0f;
            y = 0f;
            z = 1.05f;
            switch (socket)
            {
                case Rail:
                    y = 0.16f;
                    z = 0.87f;
                    break;
                case Twin:
                    y = 0.02f;
                    z = 1.15f;
                    break;
                case Pierce:
                    y = 0.10f;
                    z = 1.05f;
                    break;
                case Seeker:
                    y = 0.22f;
                    z = 1.28f;
                    break;
                default:
                    x = 0f;
                    y = 0f;
                    z = 0f;
                    break;
            }
        }

        /// <summary>
        /// Blender FBX sometimes lands 100x off. Snap only the classic unit
        /// factors so a correctly sized mesh keeps scale 1.
        /// </summary>
        public static float SnapFactor(float measuredExtent, float authoredExtent)
        {
            if (measuredExtent <= 0.00001f || authoredExtent <= 0f)
            {
                return 1f;
            }

            float ratio = authoredExtent / measuredExtent;
            if (ratio >= 50f && ratio <= 200f)
            {
                return 100f;
            }

            if (ratio >= 0.005f && ratio <= 0.02f)
            {
                return 0.01f;
            }

            return 1f;
        }

        public static int NormalizeDoctrine(int doctrine)
        {
            if (doctrine < (int)DoctrineId.None || doctrine > (int)DoctrineId.Hunter)
            {
                return (int)DoctrineId.None;
            }

            return doctrine;
        }

        /// <summary>
        /// Owned sockets stay visible for every doctrine, including Barrage.
        /// An unknown doctrine value from an old save is treated as None and
        /// still shows the equipped guns. Nothing is shown that is not owned.
        /// </summary>
        public static bool Shown(int socket, bool rail, bool twin, bool pierce, bool seeker, int doctrine)
        {
            int path = NormalizeDoctrine(doctrine);
            bool owned = false;
            switch (socket)
            {
                case Rail:
                    owned = rail;
                    break;
                case Twin:
                    owned = twin;
                    break;
                case Pierce:
                    owned = pierce;
                    break;
                case Seeker:
                    owned = seeker;
                    break;
                default:
                    return false;
            }

            if (!owned)
            {
                return false;
            }

            if (path == (int)DoctrineId.None || path == (int)DoctrineId.Barrage
                || path == (int)DoctrineId.Lance || path == (int)DoctrineId.Hunter)
            {
                return true;
            }

            return false;
        }
    }
}
