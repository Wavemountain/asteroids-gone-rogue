namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Reduce-effects scales from 0.47 Part C. Callers pass the settings flag.
    /// Audio is intentionally untouched. Unity-free.
    /// </summary>
    public static class EffectScale
    {
        public const float ShakeMul = 0.35f;
        public const float MaxShakeReduced = 0.12f;
        public const float FlashAlphaCap = 0.08f;
        public const float FlashMinGap = 0.5f;
        public const float FlashDecay = 0.22f;
        public const float MeshEmissionReduced = 0.7f;
        public const float MeshEmissionFull = 1.15f;
        public const float MeshSecondsReduced = 0.14f;
        public const float InvulnDim = 0.55f;
        public const float BurstAlphaMul = 0.4f;
        public const float BurstEndMul = 0.6f;
        public const float TrailWidthMul = 0.6f;
        public const float ParticleCountMul = 0.4f;
        public const float ParticleSizeMul = 0.7f;
        public const float TelegraphFloor = 0.45f;
        public const float TelegraphPeak = 0.8f;
        public const float TelegraphFadeIn = 0.12f;
        public const float AuraReduced = 0.7f;
        public const float SpikeGlowReduced = 1.15f;
        public const float SpikeIdleReduced = 0.72f;

        public static float ShakeAdd(bool shakeEnabled, bool reduce, float amplitude)
        {
            if (!shakeEnabled || amplitude <= 0f)
            {
                return 0f;
            }

            if (reduce)
            {
                return amplitude * ShakeMul;
            }

            return amplitude;
        }

        public static float ShakeCap(bool reduce, float fullCap)
        {
            if (reduce)
            {
                return MaxShakeReduced;
            }

            if (fullCap < 0f)
            {
                return 0f;
            }

            return fullCap;
        }

        public static bool AllowsKillFlash(bool reduce)
        {
            return !reduce;
        }

        public static void FlashWindow(bool reduce, float requestedDecay, out float alphaCap, out float minGap, out float decay)
        {
            if (!reduce)
            {
                alphaCap = HitFlashLimiter.MaxAlpha;
                minGap = HitFlashLimiter.MinGapSeconds;
                decay = requestedDecay;
                return;
            }

            alphaCap = FlashAlphaCap;
            minGap = FlashMinGap;
            decay = FlashDecay;
        }

        public static float MeshEmission(bool reduce)
        {
            if (reduce)
            {
                return MeshEmissionReduced;
            }

            return MeshEmissionFull;
        }

        public static float MeshSeconds(bool reduce, float requested)
        {
            if (reduce)
            {
                return MeshSecondsReduced;
            }

            if (requested < 0.02f)
            {
                return 0.02f;
            }

            return requested;
        }

        public static bool SteadyInvulnDim(bool reduce)
        {
            return reduce;
        }

        public static float BurstAlpha(bool reduce, float alpha)
        {
            if (alpha < 0f)
            {
                alpha = 0f;
            }

            if (!reduce)
            {
                return alpha;
            }

            return alpha * BurstAlphaMul;
        }

        public static float BurstEnd(bool reduce, float endScale)
        {
            if (!reduce)
            {
                return endScale;
            }

            return endScale * BurstEndMul;
        }

        public static float TrailWidth(bool reduce, float width)
        {
            if (!reduce)
            {
                return width;
            }

            return width * TrailWidthMul;
        }

        /// <summary>
        /// Reduce keeps a readable floor and skips the scale pulse.
        /// pulseScale is 1 when the ring may grow, 0 when it stays put.
        /// </summary>
        public static void Telegraph(bool reduce, float t, float lifeSeconds, out float alpha, out int pulseScale)
        {
            if (t < 0f)
            {
                t = 0f;
            }

            if (t > 1f)
            {
                t = 1f;
            }

            if (!reduce)
            {
                alpha = 0.55f * (1f - t);
                if (alpha < 0f)
                {
                    alpha = 0f;
                }

                pulseScale = 1;
                return;
            }

            pulseScale = 0;
            float life = lifeSeconds;
            if (life < 0.12f)
            {
                life = 0.12f;
            }

            float elapsed = t * life;
            float fade = TelegraphFadeIn;
            if (fade < 0.02f)
            {
                fade = 0.02f;
            }

            float u = elapsed / fade;
            if (u < 0f)
            {
                u = 0f;
            }

            if (u > 1f)
            {
                u = 1f;
            }

            alpha = TelegraphFloor + ((TelegraphPeak - TelegraphFloor) * u);
            if (alpha < TelegraphFloor)
            {
                alpha = TelegraphFloor;
            }
        }

        public static float AuraMul(bool reduce, float pulseBase, float amplitude, float sine)
        {
            if (reduce)
            {
                return AuraReduced;
            }

            if (sine > 1f)
            {
                sine = 1f;
            }

            if (sine < -1f)
            {
                sine = -1f;
            }

            return pulseBase + (amplitude * sine);
        }

        public static float SpikeMul(bool reduce, bool damaging, float sine)
        {
            if (reduce)
            {
                return damaging ? SpikeGlowReduced : SpikeIdleReduced;
            }

            if (sine > 1f)
            {
                sine = 1f;
            }

            if (sine < -1f)
            {
                sine = -1f;
            }

            float amp = damaging ? 0.38f : 0.16f;
            return 0.72f + (sine * amp);
        }

        public static bool FreezePulse(bool reduce)
        {
            return reduce;
        }

        /// <summary>
        /// UI pulses (elite banner, recommended upgrade) freeze at the calm end.
        /// </summary>
        public static float UiPulse(bool reduce, float pingPong01)
        {
            if (reduce)
            {
                return 0f;
            }

            if (pingPong01 < 0f)
            {
                return 0f;
            }

            if (pingPong01 > 1f)
            {
                return 1f;
            }

            return pingPong01;
        }

        public static float ScalePulse(bool reduce, float pulsed)
        {
            if (reduce)
            {
                return 1f;
            }

            return pulsed;
        }
    }
}
