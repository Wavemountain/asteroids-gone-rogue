using System;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// 0.48 emission table (Docs/0.48/01_post_stack.md §7 and §9).
    /// FBX factors are a baseline only. Callers overwrite <c>_EmissionColor</c>
    /// as albedo-token times the multiplier. Unity-free.
    /// Rail's existing <c>Mat_Ship_Accent_Hot</c> is intentionally not a row.
    /// </summary>
    public static class EmissionTargets
    {
        public const int None = 0;
        public const int Primary = 1;
        public const int Steel = 2;
        public const int Danger = 3;

        public const float PrimaryR = 212f / 255f;
        public const float PrimaryG = 160f / 255f;
        public const float PrimaryB = 74f / 255f;
        public const float SteelR = 106f / 255f;
        public const float SteelG = 168f / 255f;
        public const float SteelB = 200f / 255f;
        public const float DangerR = 184f / 255f;
        public const float DangerG = 90f / 255f;
        public const float DangerB = 40f / 255f;

        public const float ShipAccentHotHero = 3.2f;
        public const float ShipGlow = 4.0f;
        public const float LaunchSignCap = 4.0f;
        public const float LayoutDangerAlbedoMul = 0.55f;
        public const float LayoutEmissionMul = 0.35f;
        public const float SniperDesat = 0.18f;
        public const float ShipCyan = 3.5f;
        public const float MagentaMul = 2.0f;
        public const float EnemyGlow = 7.0f;
        public const float EnemyBrute = 9.0f;
        public const float PlayerShot = 4.0f;
        public const float PlayerRail = 5.0f;
        public const float EnemyShot = 9.0f;
        public const float AsteroidHero = 0.3f;
        public const float FxRing = 4.5f;
        public const float FxPickupHeart = 1.5f;

        public const float HealR = 143f / 255f;
        public const float HealG = 209f / 255f;
        public const float HealB = 143f / 255f;

        public const int Heal = 4;

        public const float HullRimPower = 3.0f;
        public const float HullRimStrength = 0.6f;
        public const float GlowRimStrength = 0.25f;
        public const float EnemyRimPower = 2.5f;
        public const float EnemyRimStrength = 0.8f;
        public const float BruteRimPower = 2.0f;
        public const float BruteRimStrength = 1.0f;
        public const float BruteRimColorMul = 1.2f;
        public const float AsteroidRimPower = 4.0f;
        public const float AsteroidRimStrength = 0.25f;
        public const float AsteroidRimColorMul = 0.5f;

        public const int RimHull = 10;
        public const int RimEnemy = 11;
        public const int RimBrute = 12;
        public const int RimAsteroid = 13;

        public static bool TryMatch(string materialName, out int colorId, out float multiplier)
        {
            colorId = None;
            multiplier = 0f;
            if (string.IsNullOrEmpty(materialName))
            {
                return false;
            }

            string name = StripInstance(materialName);
            if (Has(name, "Mat_Ship_Accent_Hot"))
            {
                return false;
            }

            if (Has(name, "Mat_Ship_AccentHot_Hero"))
            {
                colorId = Primary;
                multiplier = ShipAccentHotHero;
                return true;
            }

            if (Has(name, "Mat_Ship_Glow"))
            {
                colorId = Primary;
                multiplier = ShipGlow;
                return true;
            }

            if (Has(name, "Magenta") && (Has(name, "Ship") || Has(name, "Weapon")))
            {
                colorId = Primary;
                multiplier = MagentaMul;
                return true;
            }

            if (Has(name, "Cyan") && (Has(name, "Ship") || Has(name, "Weapon")) && !Has(name, "Hangar"))
            {
                colorId = Steel;
                multiplier = ShipCyan;
                return true;
            }

            if ((Has(name, "Mat_Enemy_Brute") || Has(name, "Boss"))
                && (Has(name, "Glow") || Has(name, "Core") || Has(name, "Mat_Enemy")))
            {
                colorId = Danger;
                multiplier = EnemyBrute;
                return true;
            }

            if (Has(name, "Mat_Enemy") && (Has(name, "Glow") || Has(name, "Core")))
            {
                colorId = Danger;
                multiplier = EnemyGlow;
                return true;
            }

            if (Has(name, "Projectile") || Has(name, "Bolt"))
            {
                if (Has(name, "Enemy"))
                {
                    colorId = Danger;
                    multiplier = EnemyShot;
                    return true;
                }

                if (Has(name, "Rail"))
                {
                    colorId = Primary;
                    multiplier = PlayerRail;
                    return true;
                }

                colorId = Primary;
                multiplier = PlayerShot;
                return true;
            }

            if (Has(name, "Mat_FX_Pickup_Heart"))
            {
                colorId = Heal;
                multiplier = FxPickupHeart;
                return true;
            }

            if (Has(name, "Mat_Asteroid") && Has(name, "Hero"))
            {
                colorId = Steel;
                multiplier = AsteroidHero;
                return true;
            }

            if (Has(name, "Mat_FX") && (Has(name, "Ring") || Has(name, "Shock")))
            {
                colorId = Steel;
                multiplier = FxRing;
                return true;
            }

            return false;
        }

        public static void Rgb(int colorId, out float red, out float green, out float blue)
        {
            if (colorId == Steel)
            {
                red = SteelR;
                green = SteelG;
                blue = SteelB;
                return;
            }

            if (colorId == Danger)
            {
                red = DangerR;
                green = DangerG;
                blue = DangerB;
                return;
            }

            if (colorId == Heal)
            {
                red = HealR;
                green = HealG;
                blue = HealB;
                return;
            }

            red = PrimaryR;
            green = PrimaryG;
            blue = PrimaryB;
        }

        private static string StripInstance(string materialName)
        {
            const string suffix = " (Instance)";
            int cut = materialName.IndexOf(suffix, StringComparison.Ordinal);
            if (cut >= 0)
            {
                return materialName.Substring(0, cut);
            }

            return materialName;
        }

        /// <summary>
        /// Hangar launch-sign emission stays on Primary and never exceeds ×4.
        /// </summary>
        public static float CapLaunchSign(float multiplier)
        {
            if (multiplier <= 0f)
            {
                return 0f;
            }

            if (multiplier > LaunchSignCap)
            {
                return LaunchSignCap;
            }

            return multiplier;
        }

        public static void CapLaunchSignRgb(float red, float green, float blue, out float outR, out float outG, out float outB)
        {
            float capR = PrimaryR * LaunchSignCap;
            float capG = PrimaryG * LaunchSignCap;
            float capB = PrimaryB * LaunchSignCap;
            float scale = 1f;
            if (red > capR && capR > 0f)
            {
                float next = capR / red;
                if (next < scale)
                {
                    scale = next;
                }
            }

            if (green > capG && capG > 0f)
            {
                float next = capG / green;
                if (next < scale)
                {
                    scale = next;
                }
            }

            if (blue > capB && capB > 0f)
            {
                float next = capB / blue;
                if (next < scale)
                {
                    scale = next;
                }
            }

            outR = red * scale;
            outG = green * scale;
            outB = blue * scale;
        }

        /// <summary>
        /// Move a colour toward its luma. Sniper cyan stays cyan, only less neon.
        /// </summary>
        public static void Desaturate(float red, float green, float blue, float amount, out float outR, out float outG, out float outB)
        {
            float t = amount;
            if (t < 0f)
            {
                t = 0f;
            }

            if (t > 1f)
            {
                t = 1f;
            }

            float y = (0.2126f * red) + (0.7152f * green) + (0.0722f * blue);
            outR = red + ((y - red) * t);
            outG = green + ((y - green) * t);
            outB = blue + ((y - blue) * t);
        }

        public static float AsteroidEmission(float readabilityScale)
        {
            if (readabilityScale < 0f)
            {
                return 0f;
            }

            if (readabilityScale > AsteroidHero)
            {
                return AsteroidHero;
            }

            return readabilityScale;
        }

        /// <summary>
        /// Rim strength is readability. Reduce effects does not change it.
        /// </summary>
        public static float RimStrength(int target, bool reduce)
        {
            float strength = HullRimStrength;
            if (target == RimBrute)
            {
                strength = BruteRimStrength;
            }
            else if (target == RimAsteroid)
            {
                strength = AsteroidRimStrength;
            }
            else if (target == RimEnemy)
            {
                strength = EnemyRimStrength;
            }

            if (reduce)
            {
                return strength;
            }

            return strength;
        }

        public static bool SelfCheck()
        {
            int colorId;
            float multiplier;
            if (TryMatch("Mat_Ship_Accent_Hot", out colorId, out multiplier))
            {
                return false;
            }

            if (!TryMatch("Mat_Ship_AccentHot_Hero", out colorId, out multiplier) || colorId != Primary || multiplier != ShipAccentHotHero)
            {
                return false;
            }

            if (!TryMatch("Mat_Ship_Glow", out colorId, out multiplier) || multiplier != ShipGlow)
            {
                return false;
            }

            if (!TryMatch("Mat_Enemy_Brute_Glow", out colorId, out multiplier) || colorId != Danger || multiplier != EnemyBrute)
            {
                return false;
            }

            if (!TryMatch("Mat_Enemy_Core", out colorId, out multiplier) || multiplier != EnemyGlow)
            {
                return false;
            }

            if (!TryMatch("Mat_Projectile_Enemy", out colorId, out multiplier) || multiplier != EnemyShot)
            {
                return false;
            }

            if (!TryMatch("Mat_Projectile_Rail", out colorId, out multiplier) || multiplier != PlayerRail)
            {
                return false;
            }

            if (!TryMatch("Mat_Projectile", out colorId, out multiplier) || multiplier != PlayerShot)
            {
                return false;
            }

            if (!TryMatch("Mat_Asteroid_Hero", out colorId, out multiplier) || multiplier != AsteroidHero)
            {
                return false;
            }

            if (!TryMatch("Mat_FX_Ring", out colorId, out multiplier) || multiplier != FxRing)
            {
                return false;
            }

            if (!TryMatch("Mat_FX_Pickup_Heart", out colorId, out multiplier) || colorId != Heal || multiplier != FxPickupHeart)
            {
                return false;
            }

            if (AsteroidEmission(0.15f) != 0.15f || AsteroidEmission(0.9f) != AsteroidHero)
            {
                return false;
            }

            if (RimStrength(RimEnemy, false) != RimStrength(RimEnemy, true))
            {
                return false;
            }

            if (RimStrength(RimBrute, true) != BruteRimStrength || RimStrength(RimAsteroid, false) != AsteroidRimStrength)
            {
                return false;
            }

            if (CapLaunchSign(3.8f) != 3.8f || CapLaunchSign(4f) != LaunchSignCap)
            {
                return false;
            }

            if (CapLaunchSign(5.2f) != LaunchSignCap || CapLaunchSign(6.4f) != LaunchSignCap || CapLaunchSign(-1f) != 0f)
            {
                return false;
            }

            float hotR;
            float hotG;
            float hotB;
            CapLaunchSignRgb(PrimaryR * 6.4f, PrimaryG * 6.4f, PrimaryB * 6.4f, out hotR, out hotG, out hotB);
            float capR = PrimaryR * LaunchSignCap;
            if (hotR > capR + 0.001f || hotG > PrimaryG * LaunchSignCap + 0.001f)
            {
                return false;
            }

            float sniperR;
            float sniperG;
            float sniperB;
            Desaturate(0.18f, 0.72f, 1f, SniperDesat, out sniperR, out sniperG, out sniperB);
            if (!(sniperB > sniperG && sniperG > sniperR))
            {
                return false;
            }

            float before = (0.2126f * 0.18f) + (0.7152f * 0.72f) + (0.0722f * 1f);
            float after = (0.2126f * sniperR) + (0.7152f * sniperG) + (0.0722f * sniperB);
            float lumaDelta = after - before;
            if (lumaDelta < 0f)
            {
                lumaDelta = -lumaDelta;
            }

            if (lumaDelta > 0.02f || sniperB >= 1f)
            {
                return false;
            }

            float dangerR;
            float dangerG;
            float dangerB;
            Rgb(Danger, out dangerR, out dangerG, out dangerB);
            float wallY = (0.2126f * dangerR * LayoutDangerAlbedoMul)
                + (0.7152f * dangerG * LayoutDangerAlbedoMul)
                + (0.0722f * dangerB * LayoutDangerAlbedoMul);
            float enemyY = (0.2126f * 0.5f) + (0.7152f * 0.3f) + (0.0722f * 0.32f);
            if (wallY >= enemyY)
            {
                return false;
            }

            return true;
        }

        private static bool Has(string materialName, string token)
        {
            return materialName.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
