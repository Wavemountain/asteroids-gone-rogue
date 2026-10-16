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
        public const float W3GridDim = 0.34f;
        public const float GridEdgeStart = 0.78f;
        public const float FloorSmoothness = 0.2f;
        public const float FloorMetallic = 0.04f;
        public const float FloorTintMix = 0.18f;
        public const float W3ChromaKeep = 0.45f;
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
        public const float DeathFlashAlpha = 0.38f;
        public const float DeathFlashAlphaReduce = 0.08f;
        public const float DeathDesat = 0.70f;
        public const float DeathDesatReduce = 0.28f;
        public const float WaveBloomAdd = 0.15f;
        public const float WaveBloomSeconds = 0.40f;
        public const float FailPanelSeconds = 0.220f;
        public const float DomeRimAlpha = 0.30f;
        public const float EnemyCoreLift = 1.12f;
        public const float MinorEnemyLift = 1.30f;
        public const float PreviewStudioGain = 0.58f;
        public const float PreviewExposure = 0.85f;
        public const float CombatHullEmission = 1.0f;
        public const float PreviewEmissionCap = 0.65f;
        public const float PreviewAlbedoLift = 1.05f;
        // Art-director hangar grade. Blue keep stays 0.92. The chroma stretch
        // cap is 1.2 (was 1.8) so panel shading is not flattened. Warm hulls
        // then land on the 0.47 steel read. The peak is not raised.
        public const float PreviewBlueKeep = 0.92f;
        public const float PreviewSatFloor = 0.55f;
        public const float PreviewSatBoostCap = 1.2f;
        public const float PreviewNeutralR = 0.45f;
        public const float PreviewNeutralG = 0.52f;
        public const float PreviewNeutralB = 0.58f;
        public const float PreviewRimR = 106f / 255f;
        public const float PreviewRimG = 168f / 255f;
        public const float PreviewRimB = 200f / 255f;
        public const float PreviewNeutralEmit = 0.35f;
        public const float DeathEdgeFull = 0.82f;
        public const float DeathWashDimFull = 0.62f;
        public const float DeathWashDimReduce = 0.28f;
        public const float HangarBloomGain = 0.45f;
        public const float HangarExposureEv = -0.40f;
        public const float DeathVignetteTarget = 0.85f;
        public const float PinnedFlashU = 0.30f;
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

            float amount = DeathDesat;
            if (reduce)
            {
                amount = DeathDesat;
            }

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

        /// <summary>
        /// Extra edge multiply after the vignette. Zero in normal play. Reduce
        /// keeps a milder darkening that is still darker than the centre.
        /// </summary>
        public static float DeathEdge(float deathU, bool reduce)
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

            float amount = DeathEdgeFull;
            if (reduce)
            {
                amount *= EffectScale.PostVignetteMul;
            }

            return amount * u;
        }

        /// <summary>
        /// Black multiply drawn after the warm death wash. Reduce effects
        /// does not change this darkening: saturation and the wash stay put.
        /// </summary>
        public static float DeathWashDim(bool reduce)
        {
            if (reduce)
            {
                return DeathWashDimFull;
            }

            return DeathWashDimFull;
        }

        /// <summary>
        /// Optional glass chroma keep. The hull does not pass through here:
        /// hangar parts keep their own material colour. Warm colours are not
        /// swapped onto the steel neutral. Reduce does not enter.
        /// </summary>
        public static void PreviewSteel(float red, float green, float blue, out float outR, out float outG, out float outB)
        {
            float scaledR = red;
            float scaledG = green;
            float scaledB = blue;
            if (scaledR < 0f)
            {
                scaledR = 0f;
            }

            if (scaledG < 0f)
            {
                scaledG = 0f;
            }

            if (scaledB < 0f)
            {
                scaledB = 0f;
            }

            if (scaledR > 1f)
            {
                scaledR = 1f;
            }

            if (scaledG > 1f)
            {
                scaledG = 1f;
            }

            if (scaledB > 1f)
            {
                scaledB = 1f;
            }

            scaledB *= PreviewBlueKeep;
            if (scaledB < 0f)
            {
                scaledB = 0f;
            }

            float peak = scaledR;
            if (scaledG > peak)
            {
                peak = scaledG;
            }

            if (scaledB > peak)
            {
                peak = scaledB;
            }

            float floor = scaledR;
            if (scaledG < floor)
            {
                floor = scaledG;
            }

            if (scaledB < floor)
            {
                floor = scaledB;
            }

            float boost = 1f;
            if (peak > 0.0001f)
            {
                float sat = (peak - floor) / peak;
                if (sat < PreviewSatFloor && sat > 0.0001f)
                {
                    boost = PreviewSatFloor / sat;
                    if (boost > PreviewSatBoostCap)
                    {
                        boost = PreviewSatBoostCap;
                    }
                }
            }

            scaledR = peak - ((peak - scaledR) * boost);
            scaledG = peak - ((peak - scaledG) * boost);
            scaledB = peak - ((peak - scaledB) * boost);
            if (scaledR < 0f)
            {
                scaledR = 0f;
            }

            if (scaledG < 0f)
            {
                scaledG = 0f;
            }

            if (scaledB < 0f)
            {
                scaledB = 0f;
            }

            if (scaledR > 1f)
            {
                scaledR = 1f;
            }

            if (scaledG > 1f)
            {
                scaledG = 1f;
            }

            if (scaledB > 1f)
            {
                scaledB = 1f;
            }

            outR = scaledR;
            outG = scaledG;
            outB = scaledB;
        }

        /// <summary>
        /// 0.47 hull emission: steel rim times HullRimScale. Same for Reduce on and off.
        /// </summary>
        public static void PreviewNeutralEmission(out float red, out float green, out float blue)
        {
            red = PreviewRimR * PreviewNeutralEmit;
            green = PreviewRimG * PreviewNeutralEmit;
            blue = PreviewRimB * PreviewNeutralEmit;
        }

        /// <summary>
        /// Albedo ceiling before the viewport multiply. Exposure 0.46 keeps
        /// the limit at 1, so the graded hull cannot clip.
        /// </summary>
        public static float PreviewAlbedoLimit()
        {
            return AlbedoLimit(PreviewExposure);
        }

        private static float AlbedoLimit(float exposure)
        {
            if (exposure <= 0.0001f)
            {
                return 1f;
            }

            float limit = 0.995f / exposure;
            if (limit > 1f)
            {
                return 1f;
            }

            return limit;
        }

        public static float HsvSaturation(float red, float green, float blue)
        {
            float peak = red;
            if (green > peak)
            {
                peak = green;
            }

            if (blue > peak)
            {
                peak = blue;
            }

            float floor = red;
            if (green < floor)
            {
                floor = green;
            }

            if (blue < floor)
            {
                floor = blue;
            }

            if (peak <= 0.0001f)
            {
                return 0f;
            }

            return (peak - floor) / peak;
        }

        /// <summary>
        /// The hull keeps its own saturation. It is not pulled onto the steel
        /// neutral. Cool glass still keeps its blue. The grade does not raise
        /// a source peak.
        /// </summary>
        public static bool PreviewSaturationHolds()
        {
            float hullR;
            float hullG;
            float hullB;
            PreviewSteel(0.80f, 0.48f, 0.26f, out hullR, out hullG, out hullB);
            float sourceSat = HsvSaturation(0.80f, 0.48f, 0.26f);
            float hullSat = HsvSaturation(hullR, hullG, hullB);
            float steelSat = HsvSaturation(PreviewNeutralR, PreviewNeutralG, PreviewNeutralB);
            if (SatGap(hullSat, sourceSat) > 0.10f || SatGap(hullSat, steelSat) < 0.20f)
            {
                return false;
            }

            if (HueGap(0.80f, 0.48f, 0.26f, hullR, hullG, hullB) > 10f)
            {
                return false;
            }

            if (hullR <= hullG || hullG <= hullB)
            {
                return false;
            }

            float accentR;
            float accentG;
            float accentB;
            PreviewSteel(1f, 0.55f, 0.14f, out accentR, out accentG, out accentB);
            if (HueGap(1f, 0.55f, 0.14f, accentR, accentG, accentB) > 10f)
            {
                return false;
            }

            if (SatGap(HsvSaturation(accentR, accentG, accentB), HsvSaturation(1f, 0.55f, 0.14f)) > 0.10f)
            {
                return false;
            }

            float glassR;
            float glassG;
            float glassB;
            PreviewSteel(0.35f, 0.70f, 0.95f, out glassR, out glassG, out glassB);
            if (SatGap(glassB, 0.95f * PreviewBlueKeep) > 0.01f || glassR > 0.36f || glassG > 0.71f)
            {
                return false;
            }

            float panelR;
            float panelG;
            float panelB;
            PreviewSteel(0.50f, 0.52f, 0.56f, out panelR, out panelG, out panelB);
            float panelSat = HsvSaturation(panelR, panelG, panelB);
            if (panelSat > PreviewSatFloor || panelSat < 0.02f)
            {
                return false;
            }

            if (!AlbedoExposureHolds(0.80f, 0.48f, 0.26f) || !AlbedoExposureHolds(1f, 0.55f, 0.14f) || !AlbedoExposureHolds(0.35f, 0.70f, 0.95f))
            {
                return false;
            }

            if (!PeakHeld(0.80f, 0.48f, 0.26f) || !PeakHeld(1f, 0.55f, 0.14f) || !PeakHeld(0.35f, 0.70f, 0.95f) || !PeakHeld(1f, 0.55f, 0.15f))
            {
                return false;
            }

            return true;
        }

        private static float HueGap(float red, float green, float blue, float otherR, float otherG, float otherB)
        {
            float gap = HueDegrees(red, green, blue) - HueDegrees(otherR, otherG, otherB);
            if (gap < 0f)
            {
                gap = -gap;
            }

            if (gap > 180f)
            {
                gap = 360f - gap;
            }

            return gap;
        }

        private static bool AlbedoExposureHolds(float red, float green, float blue)
        {
            float gradedR;
            float gradedG;
            float gradedB;
            PreviewSteel(red, green, blue, out gradedR, out gradedG, out gradedB);
            float peak = gradedR;
            if (gradedG > peak)
            {
                peak = gradedG;
            }

            if (gradedB > peak)
            {
                peak = gradedB;
            }

            peak *= PreviewAlbedoLift;
            if (peak > PreviewAlbedoLimit())
            {
                peak = PreviewAlbedoLimit();
            }

            return peak * PreviewExposure < 0.995f;
        }

        private static float SatGap(float left, float right)
        {
            float gap = left - right;
            if (gap < 0f)
            {
                gap = -gap;
            }

            return gap;
        }

        private static float GradedHueGap(float red, float green, float blue)
        {
            float gradedR;
            float gradedG;
            float gradedB;
            PreviewSteel(red, green, blue, out gradedR, out gradedG, out gradedB);
            float gap = HueDegrees(PreviewNeutralR, PreviewNeutralG, PreviewNeutralB) - HueDegrees(gradedR, gradedG, gradedB);
            if (gap < 0f)
            {
                gap = -gap;
            }

            if (gap > 180f)
            {
                gap = 360f - gap;
            }

            return gap;
        }

        private static float HueDegrees(float red, float green, float blue)
        {
            float peak = red;
            if (green > peak)
            {
                peak = green;
            }

            if (blue > peak)
            {
                peak = blue;
            }

            float floor = red;
            if (green < floor)
            {
                floor = green;
            }

            if (blue < floor)
            {
                floor = blue;
            }

            float delta = peak - floor;
            if (delta <= 0.0001f)
            {
                return 0f;
            }

            float sector;
            if (peak == red)
            {
                sector = (green - blue) / delta;
                if (sector < 0f)
                {
                    sector += 6f;
                }
            }
            else if (peak == green)
            {
                sector = 2f + ((blue - red) / delta);
            }
            else
            {
                sector = 4f + ((red - green) / delta);
            }

            return sector * 60f;
        }

        private static float GradedSat(float red, float green, float blue)
        {
            float gradedR;
            float gradedG;
            float gradedB;
            PreviewSteel(red, green, blue, out gradedR, out gradedG, out gradedB);
            gradedR *= PreviewAlbedoLift;
            gradedG *= PreviewAlbedoLift;
            gradedB *= PreviewAlbedoLift;
            if (gradedR > 1f)
            {
                gradedR = 1f;
            }

            if (gradedG > 1f)
            {
                gradedG = 1f;
            }

            if (gradedB > 1f)
            {
                gradedB = 1f;
            }

            return HsvSaturation(gradedR, gradedG, gradedB);
        }

        private static bool PeakHeld(float red, float green, float blue)
        {
            float inPeak = red;
            if (green > inPeak)
            {
                inPeak = green;
            }

            if (blue > inPeak)
            {
                inPeak = blue;
            }

            float gradedR;
            float gradedG;
            float gradedB;
            PreviewSteel(red, green, blue, out gradedR, out gradedG, out gradedB);
            float outPeak = gradedR;
            if (gradedG > outPeak)
            {
                outPeak = gradedG;
            }

            if (gradedB > outPeak)
            {
                outPeak = gradedB;
            }

            return outPeak <= inPeak + 0.0001f;
        }

        public static void CapPreviewEmission(float red, float green, float blue, out float outR, out float outG, out float outB)
        {
            float max = red;
            if (green > max)
            {
                max = green;
            }

            if (blue > max)
            {
                max = blue;
            }

            if (max <= PreviewEmissionCap || max <= 0.0001f)
            {
                outR = red;
                outG = green;
                outB = blue;
                return;
            }

            float scale = PreviewEmissionCap / max;
            outR = red * scale;
            outG = green * scale;
            outB = blue * scale;
        }

        public static float PreviewAccentEmission(float meshEmission, float red, float green, float blue)
        {
            float max = red;
            if (green > max)
            {
                max = green;
            }

            if (blue > max)
            {
                max = blue;
            }

            if (max <= 0.0001f)
            {
                return 0f;
            }

            float capped = PreviewEmissionCap / max;
            if (meshEmission < capped)
            {
                return meshEmission;
            }

            return capped;
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

        /// <summary>
        /// 0.47 hull albedo versus the dimmed W3 grid line. Emission stays at
        /// <see cref="CombatHullEmission"/> and under the bloom threshold, so
        /// Reduce cannot recolour the body by changing bloom.
        /// </summary>
        public static bool CombatShipBeatsGrid()
        {
            float gridR = 0.08f * 0.76f * W3GridDim;
            float gridG = 0.86f * 0.76f * W3GridDim;
            float gridB = 0.68f * 0.76f * W3GridDim;
            float gridY = Luma(gridR, gridG, gridB);
            float shipR = 0.80f;
            float shipG = 0.48f;
            float shipB = 0.26f;
            if (shipR <= shipG || shipG <= shipB)
            {
                return false;
            }

            if (gridY <= 0.001f)
            {
                return false;
            }

            if (Luma(shipR, shipG, shipB) / gridY < 2.5f)
            {
                return false;
            }

            float sat = HsvSaturation(shipR, shipG, shipB);
            if (sat < 0.60f || sat > 0.95f)
            {
                return false;
            }

            return HullEmissionClearsBloom();
        }

        /// <summary>
        /// 0.47 steel rim times the fixed hull scale. Peak stays under the full
        /// bloom threshold and under the soft knee, in both Reduce modes.
        /// </summary>
        public static bool HullEmissionClearsBloom()
        {
            float scale = 0.35f * CombatHullEmission;
            float steelR = (106f / 255f) * scale;
            float steelG = (168f / 255f) * scale;
            float steelB = (200f / 255f) * scale;
            float peak = steelR;
            if (steelG > peak)
            {
                peak = steelG;
            }

            if (steelB > peak)
            {
                peak = steelB;
            }

            float fullThreshold = 1.10f;
            float reduceThreshold = fullThreshold + 0.15f;
            float kneeStart = fullThreshold - 0.35f;
            return peak < fullThreshold && peak < reduceThreshold && peak < kneeStart;
        }

        public static float Luma(float red, float green, float blue)
        {
            return (0.2126f * red) + (0.7152f * green) + (0.0722f * blue);
        }

        public static bool SelfCheck()
        {
            if (Bad(AmbientMul < 0.69f || AmbientMul > 0.71f))
            {
                return false;
            }

            if (Bad(AsteroidAlbedoBoost < 1.19f || AsteroidAlbedoKeep < 0.79f || AsteroidAlbedoKeep > 0.81f))
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

            if (!CombatShipBeatsGrid())
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

            if (Bad(BeamEmission > 1.5f || GridVisibleAlpha < 0.11f || DomeRimAlpha > 0.31f))
            {
                return false;
            }

            if (Bad(PreviewExposure < 0.84f || PreviewExposure > 0.86f))
            {
                return false;
            }

            if (Bad(PreviewBlueKeep < 0.91f || PreviewBlueKeep > 0.93f || PreviewSatBoostCap < 1.19f || PreviewSatBoostCap > 1.21f))
            {
                return false;
            }

            float glow = (212f / 255f) * PreviewEmissionCap * PreviewExposure;
            if (glow >= 1f || PreviewStudioGain > 0.7f || PreviewEmissionCap >= 1f || MinorEnemyLift < 1.25f)
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

            if (!PreviewSaturationHolds())
            {
                return false;
            }

            if (!EffectScale.SelfCheck())
            {
                return false;
            }

            return true;
        }

        private static bool Bad(bool failed)
        {
            return failed;
        }
    }
}
