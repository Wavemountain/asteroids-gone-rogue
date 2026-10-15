namespace AsteroidsGoneRogue
{
    /// <summary>
    /// 0.48 premium visual pass. Unity-free. Locked tables (WorldLight,
    /// PostGrade, PlayHudLayout, EmissionTargets) stay the source of truth.
    /// This file only adds the deltas the art pass asked for on top of them.
    /// The daily roll is not read here.
    /// </summary>
    public static class PremiumLook
    {
        public const float AmbientMul = 0.70f;
        public const float AsteroidAlbedoBoost = 1.2f;
        public const float AsteroidAlbedoKeep = 0.8f;
        public const float CraterDark = 0.78f;
        public const float KeyElevation = 35f;
        public const float KeyYaw = -35f;
        public const float KeyWarmMix = 0.45f;
        public const float KeyWarmR = 1f;
        public const float KeyWarmG = 0.86f;
        public const float KeyWarmB = 0.62f;
        public const float ShadowStrength = 0.65f;

        public const float GridVisibleAlpha = 0.12f;
        public const float GridEdgeStart = 0.78f;
        public const float FloorSmoothness = 0.2f;
        public const float FloorMetallic = 0.04f;
        public const float FloorTintMix = 0.18f;
        public const float W3ChromaKeep = 0.60f;
        public const float BeamEmission = 1.5f;
        public const float BeamR = 0.20f;
        public const float BeamG = 0.95f;
        public const float BeamB = 0.55f;

        public const float SurfaceR = 14f / 255f;
        public const float SurfaceG = 21f / 255f;
        public const float SurfaceB = 32f / 255f;

        public const float PlateAlpha = 0.35f;
        public const float BarMaxPx = 280f;
        public const float CornerMaxPx = 320f;
        public const float HudNumber = 28f;
        public const float HudLabel = 14f;

        public const float DeathHitstopSeconds = 0.25f;
        public const float DeathFlashAlpha = 0.20f;
        public const float DeathFlashAlphaReduce = 0.08f;
        public const float DeathDesat = 0.70f;
        public const float DeathDesatReduce = 0.28f;
        public const float WaveBloomAdd = 0.15f;
        public const float WaveBloomSeconds = 0.40f;
        public const float FailPanelSeconds = 0.220f;
        public const float DomeRimAlpha = 0.30f;
        public const float EnemyCoreLift = 1.12f;
        public const float MinorEnemyLift = 1.30f;
        public const float PreviewStudioGain = 0.62f;
        public const float PreviewExposure = 0.28f;
        public const float HangarBloomGain = 0.45f;
        public const float HangarExposureEv = -0.40f;
        public const float DeathVignetteTarget = 0.78f;
        public const float PinnedFlashU = 0.55f;
        public const float OutsideR = 0.015f;
        public const float OutsideG = 0.022f;
        public const float OutsideB = 0.045f;

        public static void WarmKey(float red, float green, float blue, out float outR, out float outG, out float outB)
        {
            float keep = 1f - KeyWarmMix;
            outR = (red * keep) + (KeyWarmR * KeyWarmMix);
            outG = (green * keep) + (KeyWarmG * KeyWarmMix);
            outB = (blue * keep) + (KeyWarmB * KeyWarmMix);
        }

        public static float AmbientChannel(float channel)
        {
            return channel * AmbientMul;
        }

        public static void FloorRgb(int world, float tintR, float tintG, float tintB, out float red, out float green, out float blue)
        {
            float mix = FloorTintMix;
            red = SurfaceR + ((tintR - SurfaceR) * mix);
            green = SurfaceG + ((tintG - SurfaceG) * mix);
            blue = SurfaceB + ((tintB - SurfaceB) * mix);
            if (world == 3)
            {
                Desaturate(red, green, blue, W3ChromaKeep, out red, out green, out blue);
            }
        }

        public static void BeamRgb(out float red, out float green, out float blue)
        {
            Desaturate(BeamR, BeamG, BeamB, W3ChromaKeep, out red, out green, out blue);
        }

        public static void Desaturate(float red, float green, float blue, float keep, out float outR, out float outG, out float outB)
        {
            if (keep < 0f)
            {
                keep = 0f;
            }

            if (keep > 1f)
            {
                keep = 1f;
            }

            float y = (0.2126f * red) + (0.7152f * green) + (0.0722f * blue);
            outR = y + ((red - y) * keep);
            outG = y + ((green - y) * keep);
            outB = y + ((blue - y) * keep);
        }

        public static float DeathSaturation(float gradeSat, float deathU, bool reduce)
        {
            float u = deathU;
            if (u < 0f)
            {
                u = 0f;
            }

            if (u > 1f)
            {
                u = 1f;
            }

            float amount = reduce ? DeathDesatReduce : DeathDesat;
            return gradeSat + ((-100f - gradeSat) * amount * u);
        }

        /// <summary>
        /// Death stills need a darker edge than the locked 0.45 grade. The locked
        /// clock stays 0.40 s; this only lifts the vignette that is drawn.
        /// </summary>
        public static float PresentedVignette(float graded, float deathU, bool reduce)
        {
            float u = deathU;
            if (u < 0f)
            {
                u = 0f;
            }

            if (u > 1f)
            {
                u = 1f;
            }

            if (u <= 0f)
            {
                return graded;
            }

            float target = DeathVignetteTarget;
            if (reduce)
            {
                target *= EffectScale.PostVignetteMul;
            }

            return graded + ((target - graded) * u);
        }

        public static float PinnedFlashAlpha(bool reduce)
        {
            if (reduce)
            {
                return DeathFlashAlphaReduce;
            }

            return DeathFlashAlpha;
        }

        public static float FlashAlpha(float armU, bool reduce)
        {
            float u = armU;
            if (u < 0f)
            {
                u = 0f;
            }

            if (u > 1f)
            {
                u = 1f;
            }

            float peak = reduce ? DeathFlashAlphaReduce : DeathFlashAlpha;
            float env = 1f - u;
            if (u < 0.35f)
            {
                env = u / 0.35f;
            }

            return peak * env;
        }

        public static void FlashRgb(float armU, out float red, out float green, out float blue)
        {
            float u = armU;
            if (u < 0f)
            {
                u = 0f;
            }

            if (u > 1f)
            {
                u = 1f;
            }

            float dangerR = 184f / 255f;
            float dangerG = 90f / 255f;
            float dangerB = 40f / 255f;
            red = 1f + ((dangerR - 1f) * u);
            green = 1f + ((dangerG - 1f) * u);
            blue = 1f + ((dangerB - 1f) * u);
        }

        public static float CornerMaxX(float screenWidth, float columnMaxX)
        {
            float width = screenWidth;
            if (width < 1f)
            {
                width = 1920f;
            }

            float corner = 0.012f + (CornerMaxPx / width);
            if (corner > columnMaxX)
            {
                corner = columnMaxX;
            }

            return corner;
        }

        public static void BarSpan(float screenWidth, out float minX, out float maxX)
        {
            float width = screenWidth;
            if (width < 1f)
            {
                width = 1920f;
            }

            float span = BarMaxPx / width;
            if (span > 0.46f)
            {
                span = 0.46f;
            }

            minX = 0.5f - (span * 0.5f);
            maxX = 0.5f + (span * 0.5f);
        }

        public static float Luma(float red, float green, float blue)
        {
            return (0.2126f * red) + (0.7152f * green) + (0.0722f * blue);
        }

        public static bool SelfCheck()
        {
            if (AmbientMul < 0.69f || AmbientMul > 0.71f)
            {
                return false;
            }

            if (AsteroidAlbedoBoost < 1.19f || AsteroidAlbedoKeep < 0.79f || AsteroidAlbedoKeep > 0.81f)
            {
                return false;
            }

            float warmR;
            float warmG;
            float warmB;
            WarmKey(0.94f, 0.98f, 0.96f, out warmR, out warmG, out warmB);
            if (warmB >= 0.96f || warmR < 0.9f)
            {
                return false;
            }

            float floorR;
            float floorG;
            float floorB;
            FloorRgb(3, 0.015f, 0.228f, 0.167f, out floorR, out floorG, out floorB);
            float floorY = Luma(floorR, floorG, floorB);
            float rockR = 0.38f * AsteroidAlbedoBoost;
            float rockG = 0.32f * AsteroidAlbedoBoost;
            float rockB = 0.28f * AsteroidAlbedoBoost;
            float rockY = Luma(rockR, rockG, rockB);
            if (floorY <= 0.01f || rockY / floorY < 2f)
            {
                return false;
            }

            float hullY = Luma(0.50f, 0.58f, 0.66f);
            if (hullY / floorY < 2.5f)
            {
                return false;
            }

            float sat = DeathSaturation(-12f, 1f, false);
            if (sat > -70f || sat < -80f)
            {
                return false;
            }

            if (DeathSaturation(-12f, 0f, false) != -12f)
            {
                return false;
            }

            if (FlashAlpha(0.2f, false) <= FlashAlpha(0.2f, true))
            {
                return false;
            }

            float barMin;
            float barMax;
            BarSpan(1920f, out barMin, out barMax);
            if ((barMax - barMin) * 1920f > BarMaxPx + 0.1f)
            {
                return false;
            }

            if (BeamEmission > 1.5f || GridVisibleAlpha < 0.11f || DomeRimAlpha > 0.31f)
            {
                return false;
            }

            float glow = (212f / 255f) * 4f * PreviewExposure;
            if (glow >= 1f || PreviewStudioGain > 0.7f || MinorEnemyLift < 1.25f)
            {
                return false;
            }

            if (PresentedVignette(0.45f, 1f, false) < 0.75f)
            {
                return false;
            }

            float enemyY = Luma(0.5f * MinorEnemyLift, 0.3f * MinorEnemyLift, 0.32f * MinorEnemyLift);
            float w4R;
            float w4G;
            float w4B;
            FloorRgb(4, 0.2856f, 0.1496f, 0.0136f, out w4R, out w4G, out w4B);
            float w4Y = Luma(w4R, w4G, w4B);
            if (w4Y <= 0.01f || enemyY / w4Y < 3.5f)
            {
                return false;
            }

            if (hullY / floorY < 3.5f)
            {
                return false;
            }

            return true;
        }
    }
}
