namespace AsteroidsGoneRogue
{
    /// <summary>
    /// 0.50 look constants. Unity-free so the logic gate can run SelfCheck
    /// without the editor. Gameplay code reads these values; it does not
    /// invent a second set.
    /// </summary>
    public static class LookPolish
    {
        public const int FilmGrainDefault = 0;
        public const float HangarGrainWhenOn = 0.04f;
        public const float GrainStepSeconds = 1f / 24f;
        public const float GridAlphaMax = 0.35f;
        public const float GridPulseMin = 0.90f;
        public const float GridPulseMax = 1.10f;
        public const float GridPulseAttackSeconds = 0.08f;
        public const float WaveStartBloomAdd = 0f;
        public const float Wave1SafeMetres = 10f;
        public const float LaterSafeMetres = 8f;
        public const float SpawnWarningSeconds = 0.6f;
        public const float SpawnQuietSeconds = 1.5f;
        public const int PlayerPool = 64;
        public const int EnemyPool = 48;
        public const float HudPanelAlpha = 0.55f;
        public const float TopPlateAlpha = 0.5f;
        public const float FadeR = 14f / 255f;
        public const float FadeG = 17f / 255f;
        public const float FadeB = 22f / 255f;
        public const float HangarFadeOut = 0.25f;
        public const float HangarFadeIn = 0.35f;
        public const float HangarMusicSeconds = 0.6f;
        public const float HangarMusicOut = 0.4f;
        public const float HangarMusicInDelay = 0.2f;
        public const float HangarStatusDelay = 0.15f;
        public const float HitAttack = 0.040f;
        public const float HitDecay = 0.220f;
        public const float HitDecayReduce = 0.160f;
        public const float HitPeak = 0.35f;
        public const float HitPeakReduce = 0.25f;
        public const float HitArcDegrees = 60f;
        public const float HitRimR = 194f / 255f;
        public const float HitRimG = 69f / 255f;
        public const float HitRimB = 58f / 255f;
        public const float CoreR = 255f / 255f;
        public const float CoreG = 241f / 255f;
        public const float CoreB = 214f / 255f;
        public const float HaloR = 212f / 255f;
        public const float HaloG = 160f / 255f;
        public const float HaloB = 74f / 255f;
        public const float HaloAlphaFull = 0.35f;
        public const float SteelR = 106f / 255f;
        public const float SteelG = 168f / 255f;
        public const float SteelB = 200f / 255f;
        public const float EnemyCoreR = 242f / 255f;
        public const float EnemyCoreG = 236f / 255f;
        public const float EnemyCoreB = 230f / 255f;
        public const float EnemyRimR = 194f / 255f;
        public const float EnemyRimG = 69f / 255f;
        public const float EnemyRimB = 58f / 255f;
        public const float EnemyDiameter = 0.34f;
        public const float EnemyBossDiameter = 0.50f;
        public const float EnemyRimThickness = 0.06f;
        public const float EnemyCoreEmission = 2.0f;
        public const float EnemyRimEmission = 1.5f;
        public const float EnemyPulseHz = 6f;
        public const float BoltLength = 0.70f;
        public const float BoltWidth = 0.12f;
        public const float BoltHaloLength = 0.90f;
        public const float BoltHaloWidth = 0.30f;
        public const float BoltEmission = 3.0f;
        public const float BoltTrail = 0.08f;
        public const float BoltTrailWidth = 0.10f;
        public const float SpreadLength = 0.45f;
        public const float SpreadWidth = 0.09f;
        public const float SpreadEmission = 2.5f;
        public const float SpreadTrail = 0.06f;
        public const float SpreadGapDegrees = 8f;
        public const float PierceLength = 1.40f;
        public const float PierceWidth = 0.06f;
        public const float PierceHaloLength = 1.60f;
        public const float PierceHaloWidth = 0.16f;
        public const float PierceEmission = 3.0f;
        public const float PierceTrail = 0.25f;
        public const float TwinLength = 0.55f;
        public const float TwinWidth = 0.08f;
        public const float TwinGapMetres = 0.30f;
        public const float TwinEmission = 2.5f;
        public const float TwinTrail = 0.06f;
        public const float SeekerLength = 0.50f;
        public const float SeekerWidth = 0.14f;
        public const float SeekerEmission = 2.8f;
        public const float SeekerTrail = 0.22f;
        public const float SeekerMinVertex = 0.06f;
        public const float RicochetLength = 0.50f;
        public const float RicochetWidth = 0.14f;
        public const float RicochetEmission = 3.0f;
        public const float RicochetTrail = 0.10f;
        public const float RailLength = 1.80f;
        public const float RailWidth = 0.05f;
        public const float RailEmission = 4.0f;
        public const float RailBeamFade = 0.12f;
        public const float ReduceHaloMul = 0.6f;
        public const float ReduceTrailMul = 0.5f;
        public const float ReduceMuzzleMul = 0.5f;
        public const float ReduceFlashMul = 0.6f;
        public const float ReduceChunkMul = 0.5f;
        public const int TierSmall = 0;
        public const int TierMid = 1;
        public const int TierLarge = 2;
        public const float LargeShake = 0.15f;
        public const float LargeHitstop = 0.04f;
        public const float BoltMuzzle = 0.06f;
        public const float RailMuzzle = 0.09f;

        public static readonly string[] CaptureSubjects = new string[]
        {
            "grid",
            "shot_bolt",
            "shot_spread",
            "shot_pierce",
            "shot_twin",
            "shot_seeker",
            "shot_ricochet",
            "shot_rail",
            "shot_enemy_mid01",
            "shot_enemy_scout",
            "shot_enemy_gunner",
            "shot_enemy_drone",
            "shot_enemy_bomber",
            "shot_enemy_sniper",
            "shot_enemy_swarmpod",
            "shot_enemy_brute",
            "shot_enemy_swarm",
            "shot_enemy_swarmling",
            "hit_vignette",
            "explosion_s",
            "explosion_m",
            "explosion_l",
        };

        public static int CaptureCount
        {
            get { return CaptureSubjects.Length * 4; }
        }

        public static string CaptureFile(int index)
        {
            int subjects = CaptureSubjects.Length;
            if (subjects < 1)
            {
                return string.Empty;
            }

            int span = subjects * 2;
            if (index < 0)
            {
                index = 0;
            }

            int phase = index / span;
            int rem = index % span;
            int mode = rem / subjects;
            int subject = rem % subjects;
            if (phase > 1)
            {
                phase = 1;
            }

            string phaseName = phase == 0 ? "before" : "after";
            string modeName = mode == 0 ? "full" : "reduce";
            return "look_" + phaseName + "_" + modeName + "_" + CaptureSubjects[subject] + ".png";
        }

        public static float SafeMetres(int wave)
        {
            if (wave <= 1)
            {
                return Wave1SafeMetres;
            }

            return LaterSafeMetres;
        }

        public static float SmoothGridPulse(float current, float target, float dt, bool reduce)
        {
            if (reduce)
            {
                return 1f;
            }

            if (target < GridPulseMin)
            {
                target = GridPulseMin;
            }

            if (target > GridPulseMax)
            {
                target = GridPulseMax;
            }

            if (current < GridPulseMin)
            {
                current = GridPulseMin;
            }

            if (current > GridPulseMax)
            {
                current = GridPulseMax;
            }

            float step = dt;
            if (step < 0f)
            {
                step = 0f;
            }

            float rate = 0.10f / GridPulseAttackSeconds;
            float maxStep = rate * step;
            float delta = target - current;
            if (delta > maxStep)
            {
                delta = maxStep;
            }

            if (delta < -maxStep)
            {
                delta = -maxStep;
            }

            return current + delta;
        }

        public static float PresentedGrain(float gradeGrain, bool enabled, bool reduce, bool hangar)
        {
            if (!enabled || reduce)
            {
                return 0f;
            }

            if (hangar)
            {
                return HangarGrainWhenOn;
            }

            return gradeGrain;
        }

        public static int GrainFrame(int frame, float sinceStep, bool moving, float step)
        {
            if (moving)
            {
                return frame;
            }

            if (sinceStep >= step)
            {
                return frame + 1;
            }

            return frame;
        }

        public static int TakePoolSlot(int[] busy)
        {
            if (busy == null)
            {
                return -1;
            }

            int index = 0;
            while (index < busy.Length)
            {
                if (busy[index] == 0)
                {
                    busy[index] = 1;
                    return index;
                }

                index++;
            }

            return -1;
        }

        public static void ReleasePoolSlot(int[] busy, int index)
        {
            if (busy == null || index < 0 || index >= busy.Length)
            {
                return;
            }

            busy[index] = 0;
        }

        public static bool TrailHueAllowed(float red, float green, float blue)
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

            float min = red;
            if (green < min)
            {
                min = green;
            }

            if (blue < min)
            {
                min = blue;
            }

            float span = max - min;
            if (span < 0.08f)
            {
                return true;
            }

            float hue;
            if (max == red)
            {
                hue = (green - blue) / span;
            }
            else if (max == green)
            {
                hue = 2f + ((blue - red) / span);
            }
            else
            {
                hue = 4f + ((red - green) / span);
            }

            hue *= 60f;
            if (hue < 0f)
            {
                hue += 360f;
            }

            if (hue >= 330f || hue <= 20f)
            {
                return false;
            }

            return true;
        }

        public static bool EnemyIsDiamond(int kind, bool boss)
        {
            if (boss)
            {
                return true;
            }

            return kind == 5 || kind == 6 || kind == 7;
        }

        public static float EnemySize(bool boss)
        {
            if (boss)
            {
                return EnemyBossDiameter;
            }

            return EnemyDiameter;
        }

        public static int TierFor(int kind, bool boss)
        {
            if (boss || kind == 6 || kind == 7)
            {
                return TierLarge;
            }

            if (kind == 1 || kind == 9)
            {
                return TierSmall;
            }

            return TierMid;
        }

        public static int ClampTier(int tier)
        {
            if (tier < TierSmall)
            {
                return TierSmall;
            }

            if (tier > TierLarge)
            {
                return TierLarge;
            }

            return tier;
        }

        public static int ExplosionChunks(int tier, bool reduce)
        {
            int count = 0;
            int clamped = ClampTier(tier);
            if (clamped == TierMid)
            {
                count = 4;
            }
            else if (clamped == TierLarge)
            {
                count = 10;
            }

            if (!reduce)
            {
                return count;
            }

            int scaled = (int)(count * ReduceChunkMul);
            if (count > 0 && scaled < 1)
            {
                scaled = 1;
            }

            return scaled;
        }

        public static int ExplosionSmokes(int tier, bool reduce)
        {
            if (reduce || ClampTier(tier) == TierSmall)
            {
                return 0;
            }

            if (ClampTier(tier) == TierLarge)
            {
                return 8;
            }

            return 4;
        }

        public static float ExplosionChunkLife(int tier)
        {
            if (ClampTier(tier) == TierLarge)
            {
                return 0.90f;
            }

            if (ClampTier(tier) == TierMid)
            {
                return 0.60f;
            }

            return 0f;
        }

        public static float ExplosionSmokeLife(int tier)
        {
            if (ClampTier(tier) == TierLarge)
            {
                return 1.2f;
            }

            if (ClampTier(tier) == TierMid)
            {
                return 0.80f;
            }

            return 0f;
        }

        public static float ExplosionSparkLife(int tier)
        {
            if (ClampTier(tier) == TierSmall)
            {
                return 0.25f;
            }

            return 0.30f;
        }

        public static int ExplosionSparks(int tier, bool reduce)
        {
            if (reduce)
            {
                return 2;
            }

            if (ClampTier(tier) == TierSmall)
            {
                return 6;
            }

            if (ClampTier(tier) == TierLarge)
            {
                return 10;
            }

            return 4;
        }

        public static float ExplosionFlash(int tier)
        {
            if (ClampTier(tier) == TierLarge)
            {
                return 0.09f;
            }

            if (ClampTier(tier) == TierMid)
            {
                return 0.07f;
            }

            return 0.05f;
        }

        public static float FlashAlpha(bool reduce)
        {
            if (reduce)
            {
                return ReduceFlashMul;
            }

            return 1f;
        }

        public static float HitVignetteAlpha(float elapsed, bool reduce)
        {
            float attack = HitAttack;
            float decay = reduce ? HitDecayReduce : HitDecay;
            float peak = reduce ? HitPeakReduce : HitPeak;
            if (elapsed < 0f || attack <= 0f)
            {
                return 0f;
            }

            if (elapsed < attack)
            {
                return peak * (elapsed / attack);
            }

            float down = elapsed - attack;
            if (down >= decay || decay <= 0f)
            {
                return 0f;
            }

            return peak * (1f - (down / decay));
        }

        public static float HangarVeilAlpha(float elapsed)
        {
            if (elapsed < 0f)
            {
                return 0f;
            }

            if (elapsed < HangarFadeOut)
            {
                return elapsed / HangarFadeOut;
            }

            float fadeIn = elapsed - HangarFadeOut;
            if (fadeIn >= HangarFadeIn)
            {
                return 0f;
            }

            return 1f - (fadeIn / HangarFadeIn);
        }

        public static bool HangarStatusHeld(float elapsed)
        {
            if (elapsed < 0f)
            {
                return false;
            }

            return elapsed < HangarFadeOut + HangarStatusDelay;
        }

        public static void HangarReturnWeights(float elapsed, out float outWeight, out float inWeight)
        {
            float outU = 0f;
            if (HangarMusicOut > 0f)
            {
                outU = elapsed / HangarMusicOut;
            }

            if (outU < 0f)
            {
                outU = 0f;
            }

            if (outU > 1f)
            {
                outU = 1f;
            }

            outWeight = 1f - outU;
            float inU = 0f;
            float rise = HangarMusicSeconds - HangarMusicInDelay;
            if (rise > 0f)
            {
                inU = (elapsed - HangarMusicInDelay) / rise;
            }

            if (inU < 0f)
            {
                inU = 0f;
            }

            if (inU > 1f)
            {
                inU = 1f;
            }

            inWeight = inU;
        }

        public static float HaloAlpha(bool reduce)
        {
            float alpha = HaloAlphaFull;
            if (reduce)
            {
                alpha *= ReduceHaloMul;
            }

            return alpha;
        }

        public static float ShotTrailTime(bool reduce, bool hostile, float seconds)
        {
            if (hostile)
            {
                return 0f;
            }

            float time = seconds;
            if (reduce)
            {
                time *= ReduceTrailMul;
            }

            return time;
        }

        public static float ShotTrailWidth(bool reduce, float width)
        {
            if (!reduce)
            {
                return width;
            }

            return width * ReduceTrailMul;
        }

        public static float TwinGap(float legacyMetres)
        {
            if (legacyMetres < 0f)
            {
                return TwinGapMetres;
            }

            return TwinGapMetres;
        }

        public static float MuzzleLife(int mode, bool reduce)
        {
            float life = BoltMuzzle;
            if (mode == 6)
            {
                life = RailMuzzle;
            }

            if (reduce)
            {
                life *= ReduceMuzzleMul;
            }

            return life;
        }

        public static bool EnemyRimPulses(bool reduce)
        {
            return !reduce;
        }

        public static bool SelfCheck()
        {
            if (!IsZero(FilmGrainDefault) || !IsZeroF(WaveStartBloomAdd))
            {
                return false;
            }

            if (SafeMetres(1) != Wave1SafeMetres || SafeMetres(2) != LaterSafeMetres)
            {
                return false;
            }

            if (SafeMetres(1) < 10f)
            {
                return false;
            }

            int[] busy = new int[4];
            int first = TakePoolSlot(busy);
            ReleasePoolSlot(busy, first);
            int again = TakePoolSlot(busy);
            if (first != 0 || again != first)
            {
                return false;
            }

            float peak = HitVignetteAlpha(HitAttack, false);
            if (peak < HitPeak - 0.001f || peak > HitPeak + 0.001f)
            {
                return false;
            }

            if (HitVignetteAlpha(HitAttack + HitDecay, false) > 0.001f)
            {
                return false;
            }

            if (HitVignetteAlpha(HitAttack, true) < HitPeakReduce - 0.001f)
            {
                return false;
            }

            if (!TrailHueAllowed(CoreR, CoreG, CoreB))
            {
                return false;
            }

            if (TrailHueAllowed(EnemyRimR, EnemyRimG, EnemyRimB))
            {
                return false;
            }

            if (!EnemyIsDiamond(5, false) || EnemyIsDiamond(0, false) || !EnemyIsDiamond(0, true))
            {
                return false;
            }

            if (TierFor(1, false) != TierSmall || TierFor(9, false) != TierSmall)
            {
                return false;
            }

            if (TierFor(6, false) != TierLarge || TierFor(7, true) != TierLarge)
            {
                return false;
            }

            if (TierFor(2, false) != TierMid)
            {
                return false;
            }

            int midChunks = ExplosionChunks(TierMid, false);
            int largeChunks = ExplosionChunks(TierLarge, false);
            if (midChunks < 3 || midChunks > 5 || largeChunks < 8 || largeChunks > 12)
            {
                return false;
            }

            if (ExplosionSmokes(TierSmall, false) != 0 || ExplosionSmokes(TierMid, true) != 0)
            {
                return false;
            }

            if (ExplosionSparks(TierSmall, false) != 6)
            {
                return false;
            }

            if (CaptureFile(0) != "look_before_full_grid.png")
            {
                return false;
            }

            if (CaptureCount != CaptureSubjects.Length * 4)
            {
                return false;
            }

            if (SmoothGridPulse(1f, 1.2f, 0.01f, false) > GridPulseMax)
            {
                return false;
            }

            if (SmoothGridPulse(1.04f, 1.2f, 1f, true) != 1f)
            {
                return false;
            }

            if (PresentedGrain(0.12f, false, false, false) != 0f)
            {
                return false;
            }

            if (PresentedGrain(0.12f, true, true, false) != 0f)
            {
                return false;
            }

            float hangarGrain = PresentedGrain(0.12f, true, false, true);
            if (hangarGrain < 0.039f || hangarGrain > 0.041f)
            {
                return false;
            }

            if (GrainFrame(3, 1f, true, GrainStepSeconds) != 3)
            {
                return false;
            }

            if (GrainFrame(3, 1f, false, GrainStepSeconds) != 4)
            {
                return false;
            }

            if (!PanelFits(HudPanelAlpha))
            {
                return false;
            }

            if (!ShotLong(BoltLength, BoltWidth) || !IsPositive(EnemyDiameter))
            {
                return false;
            }

            float outW;
            float inW;
            HangarReturnWeights(0f, out outW, out inW);
            if (outW < 0.99f || inW > 0.001f)
            {
                return false;
            }

            HangarReturnWeights(HangarMusicSeconds, out outW, out inW);
            if (outW > 0.001f || inW < 0.99f)
            {
                return false;
            }

            if (HangarVeilAlpha(0f) > 0.001f || HangarVeilAlpha(HangarFadeOut) < 0.99f)
            {
                return false;
            }

            return true;
        }

        private static bool IsZero(int value)
        {
            return value == 0;
        }

        private static bool IsZeroF(float value)
        {
            return value == 0f;
        }

        private static bool IsPositive(float value)
        {
            return value > 0f;
        }

        private static bool PanelFits(float alpha)
        {
            return alpha <= 0.55f;
        }

        private static bool ShotLong(float length, float width)
        {
            return length > width;
        }
    }
}
