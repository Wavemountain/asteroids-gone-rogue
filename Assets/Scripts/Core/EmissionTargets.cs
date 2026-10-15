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
        /// 0.47 asteroid emission stays, and never climbs past the hero cap.
        /// </summary>
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

            return true;
        }

        private static bool Has(string materialName, string token)
        {
            return materialName.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
