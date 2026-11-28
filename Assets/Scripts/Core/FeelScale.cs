namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Cosmetic feel composition. Reduce effects stays above the new
    /// accessibility scales. Unity-free so the logic tests can run it.
    /// </summary>
    public static class FeelScale
    {
        public const float IntensityMin = 0f;
        public const float IntensityMax = 1f;
        public const float IntensityStep = 0.1f;
        public const float IntensityDefault = 1f;
        public const float FalloffNear = 1f;
        public const float FalloffFar = 0.35f;
        public const float FalloffDistance = 24f;
        public const float HitShakeGateSeconds = 0.12f;

        public const int FlashFull = 0;
        public const int FlashHalf = 1;
        public const int FlashOff = 2;

        public const float SmallKillShake = 0.10f;
        public const float MidKillShake = 0.20f;
        public const float LargeKillShake = 0.28f;
        public const float ThreatHitShake = 0.04f;
        public const float ArmorHitShake = 0.05f;
        public const float ShieldBreakShake = 0.16f;
        public const float BossPhaseShake = 0.20f;
        public const float BossKillShake = 0.30f;
        public const float SecondaryKillShake = 0.12f;
        public const float BossPhaseFlash = 0.14f;
        public const float BossKillFlash = 0.20f;
        public const int BossKillBursts = 5;

        public static float ClampIntensity(float value)
        {
            float clamped = value;
            if (clamped < IntensityMin)
            {
                clamped = IntensityMin;
            }

            if (clamped > IntensityMax)
            {
                clamped = IntensityMax;
            }

            float steps = (float)System.Math.Round((clamped - IntensityMin) / IntensityStep);
            float snapped = IntensityMin + (steps * IntensityStep);
            if (snapped < IntensityMin)
            {
                snapped = IntensityMin;
            }

            if (snapped > IntensityMax)
            {
                snapped = IntensityMax;
            }

            return snapped;
        }

        public static float StepIntensity(float current, int direction)
        {
            float delta = 0f;
            if (direction > 0)
            {
                delta = IntensityStep;
            }
            else if (direction < 0)
            {
                delta = -IntensityStep;
            }

            return ClampIntensity(current + delta);
        }

        public static int ClampFlashLevel(int level)
        {
            if (level < FlashFull)
            {
                return FlashFull;
            }

            if (level > FlashOff)
            {
                return FlashOff;
            }

            return level;
        }

        public static int StepFlashLevel(int current, int direction)
        {
            int delta = 0;
            if (direction > 0)
            {
                delta = 1;
            }
            else if (direction < 0)
            {
                delta = -1;
            }

            return ClampFlashLevel(current + delta);
        }

        /// <summary>
        /// amplitude × intensity × (reduce ? ShakeMul : 1). The camera cap
        /// stays on <see cref="EffectScale.ShakeCap"/>.
        /// </summary>
        public static float FinalShake(float amplitude, float intensity, bool reduce)
        {
            float scaled = amplitude;
            if (scaled < 0f)
            {
                scaled = 0f;
            }

            scaled *= ClampIntensity(intensity);
            if (reduce)
            {
                scaled *= EffectScale.ShakeMul;
            }

            return scaled;
        }

        public static bool HitstopAllowed(bool hitstopOn, bool reduce)
        {
            return hitstopOn && !reduce;
        }

        public static float FlashScale(int level)
        {
            int clamped = ClampFlashLevel(level);
            if (clamped == FlashOff)
            {
                return 0f;
            }

            if (clamped == FlashHalf)
            {
                return 0.5f;
            }

            return 1f;
        }

        public static float FlashAlpha(float strength, int level, float limiterMax)
        {
            float alpha = strength * FlashScale(level);
            if (alpha < 0f)
            {
                alpha = 0f;
            }

            float cap = limiterMax;
            if (cap < 0f)
            {
                cap = 0f;
            }

            if (alpha > cap)
            {
                alpha = cap;
            }

            return alpha;
        }

        public static float DistanceFalloff(float distance)
        {
            float span = FalloffDistance;
            if (span < 0.001f)
            {
                span = 0.001f;
            }

            float t = distance / span;
            if (t < 0f)
            {
                t = 0f;
            }

            if (t > 1f)
            {
                t = 1f;
            }

            return FalloffNear + ((FalloffFar - FalloffNear) * t);
        }

        public static bool SelfCheck()
        {
            if (FinalShake(0.22f, 0f, false) != 0f)
            {
                return false;
            }

            if (FinalShake(0.22f, 1f, false) != 0.22f)
            {
                return false;
            }

            float reduced = FinalShake(0.22f, 1f, true);
            float expect = 0.22f * EffectScale.ShakeMul;
            if (reduced < expect - 0.0001f || reduced > expect + 0.0001f)
            {
                return false;
            }

            if (FinalShake(0.20f, 0f, true) != 0f)
            {
                return false;
            }

            if (!HitstopAllowed(true, false) || HitstopAllowed(false, false) || HitstopAllowed(true, true))
            {
                return false;
            }

            if (FlashAlpha(0.20f, FlashOff, 0.20f) != 0f)
            {
                return false;
            }

            if (FlashAlpha(0.20f, FlashHalf, 0.20f) != 0.10f)
            {
                return false;
            }

            if (FlashAlpha(0.50f, FlashFull, 0.20f) != 0.20f)
            {
                return false;
            }

            if (!Near(DistanceFalloff(0f), FalloffNear) || !Near(DistanceFalloff(FalloffDistance), FalloffFar))
            {
                return false;
            }

            float large = LargeKillShake;
            float mid = MidKillShake;
            float small = SmallKillShake;
            if (large < mid || mid < small)
            {
                return false;
            }

            if (!Near(ClampIntensity(0.94f), 0.9f) || !Near(ClampIntensity(1.5f), 1f) || !Near(ClampIntensity(-1f), 0f))
            {
                return false;
            }

            if (ClampFlashLevel(9) != FlashOff || StepFlashLevel(FlashOff, 1) != FlashOff)
            {
                return false;
            }

            return BossKillShake <= 0.36f && HitShakeGateSeconds >= 0.12f;
        }

        private static bool Near(float a, float b)
        {
            float delta = a - b;
            if (delta < 0f)
            {
                delta = -delta;
            }

            return delta < 0.0001f;
        }
    }
}
