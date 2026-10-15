namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Key, rim, fill, and under-glow per world (Docs/0.48/02_lighting_rim_key.md §1).
    /// Worlds 4 and 6 rim in steel so amber and rust floors do not swallow silhouettes.
    /// Shadows stay off. Unity-free. The daily roll is not read here.
    /// </summary>
    public struct WorldLightRow
    {
        public int World;
        public float KeyIntensity;
        public float KeyR;
        public float KeyG;
        public float KeyB;
        public float RimIntensity;
        public float RimR;
        public float RimG;
        public float RimB;
        public int RimIsSteel;
        public float FillR;
        public float FillG;
        public float FillB;
        public float UnderGlow;
    }

    public static class WorldLight
    {
        public const float SteelR = 106f / 255f;
        public const float SteelG = 168f / 255f;
        public const float SteelB = 200f / 255f;

        public const float HangarKeyIntensity = 1.15f;
        public const float HangarKeyR = 0.92f;
        public const float HangarKeyG = 0.95f;
        public const float HangarKeyB = 1f;
        public const float LegacyRimIntensity = 0.32f;
        public const float LegacyUnderGlow = 2.1f;

        public static int Fold(int world)
        {
            if (world < 1)
            {
                return 1;
            }

            return WorldCatalog.LayoutNumber(world);
        }

        public static WorldLightRow Row(int world)
        {
            int layout = Fold(world);
            WorldLightRow row = new WorldLightRow();
            row.World = layout;
            row.RimIsSteel = 0;
            switch (layout)
            {
                case 2:
                    Paint(ref row, 1.10f, 244, 238, 255, 0.40f, 0.13f, 0.12f, 0.18f, 2.0f);
                    break;
                case 3:
                    Paint(ref row, 1.20f, 240, 250, 246, 0.45f, 0.12f, 0.15f, 0.16f, 1.8f);
                    break;
                case 4:
                    Paint(ref row, 1.20f, 255, 246, 234, 0.45f, 0.14f, 0.14f, 0.16f, 1.6f);
                    row.RimIsSteel = 1;
                    row.RimR = SteelR;
                    row.RimG = SteelG;
                    row.RimB = SteelB;
                    break;
                case 5:
                    Paint(ref row, 1.15f, 238, 242, 255, 0.40f, 0.11f, 0.12f, 0.16f, 2.2f);
                    break;
                case 6:
                    Paint(ref row, 1.05f, 255, 240, 234, 0.45f, 0.12f, 0.11f, 0.13f, 1.6f);
                    row.RimIsSteel = 1;
                    row.RimR = SteelR;
                    row.RimG = SteelG;
                    row.RimB = SteelB;
                    break;
                case 7:
                    Paint(ref row, 1.10f, 255, 240, 246, 0.40f, 0.13f, 0.11f, 0.15f, 2.0f);
                    break;
                default:
                    row.World = 1;
                    Paint(ref row, 1.15f, 242, 242, 255, 0.40f, 0.12f, 0.14f, 0.18f, 2.1f);
                    break;
            }

            return row;
        }

        public static bool SelfCheck()
        {
            WorldLightRow first = Row(1);
            if (!Near(first.KeyIntensity, 1.15f) || !Near(first.RimIntensity, 0.40f) || first.RimIsSteel != 0)
            {
                return false;
            }

            if (!Near(first.FillR, 0.12f) || !Near(first.FillG, 0.14f) || !Near(first.FillB, 0.18f))
            {
                return false;
            }

            if (!Near(first.UnderGlow, 2.1f) || !Near(first.KeyR, 242f / 255f))
            {
                return false;
            }

            WorldLightRow mines = Row(4);
            WorldLightRow islands = Row(6);
            if (mines.RimIsSteel != 1 || islands.RimIsSteel != 1)
            {
                return false;
            }

            if (!Near(mines.RimR, SteelR) || !Near(mines.RimG, SteelG) || !Near(islands.RimB, SteelB))
            {
                return false;
            }

            if (!Near(mines.KeyIntensity, 1.20f) || !Near(islands.KeyIntensity, 1.05f))
            {
                return false;
            }

            if (!Near(Row(2).UnderGlow, 2.0f) || !Near(Row(3).RimIntensity, 0.45f) || !Near(Row(5).UnderGlow, 2.2f))
            {
                return false;
            }

            if (!Near(Row(7).FillR, 0.13f) || !Near(Row(7).FillG, 0.11f) || !Near(Row(7).FillB, 0.15f))
            {
                return false;
            }

            if (Row(3).RimIsSteel != 0 || Row(8).World != 1 || !Near(Row(8).KeyIntensity, first.KeyIntensity))
            {
                return false;
            }

            if (!Near(HangarKeyIntensity, 1.15f) || !Near(LegacyRimIntensity, 0.32f))
            {
                return false;
            }

            return true;
        }

        private static void Paint(ref WorldLightRow row, float key, int keyR, int keyG, int keyB, float rim, float fillR, float fillG, float fillB, float under)
        {
            row.KeyIntensity = key;
            row.KeyR = keyR / 255f;
            row.KeyG = keyG / 255f;
            row.KeyB = keyB / 255f;
            row.RimIntensity = rim;
            row.FillR = fillR;
            row.FillG = fillG;
            row.FillB = fillB;
            row.UnderGlow = under;
        }

        private static bool Near(float a, float b)
        {
            float d = a - b;
            if (d < 0f)
            {
                d = -d;
            }

            return d < 0.002f;
        }
    }
}
