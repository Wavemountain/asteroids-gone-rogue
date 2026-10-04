namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Equal-power music crossfade. Weights sit on the unit circle so
    /// out^2 + in^2 stays 1 (no -3 dB dip). An abort continues from the
    /// weights already playing instead of snapping the incoming stem to 1.
    /// Pitch and scale stay on each stem. Unity-free.
    /// </summary>
    public struct MusicRetarget
    {
        public float OutScale;
        public float OutPitch;
        public int OutHangar;
        public float InScale;
        public float InPitch;
        public int InHangar;
        public float StartAngle;
        public int SwapSources;
        public int ReplaceIncoming;
    }

    public static class MusicCrossfade
    {
        public const float HalfPi = 1.57079637f;

        public static float Clamp01(float value)
        {
            if (value < 0f)
            {
                return 0f;
            }

            if (value > 1f)
            {
                return 1f;
            }

            return value;
        }

        public static void Weights(float startAngle, float progress, out float outWeight, out float inWeight)
        {
            float angle = startAngle;
            if (angle < 0f)
            {
                angle = 0f;
            }

            if (angle > HalfPi)
            {
                angle = HalfPi;
            }

            float t = Clamp01(progress);
            angle = angle + ((HalfPi - angle) * t);
            outWeight = (float)System.Math.Cos(angle);
            inWeight = (float)System.Math.Sin(angle);
        }

        public static float AngleFromWeights(float outWeight, float inWeight)
        {
            if (outWeight < 0f)
            {
                outWeight = 0f;
            }

            if (inWeight < 0f)
            {
                inWeight = 0f;
            }

            if (outWeight <= 0.00001f && inWeight <= 0.00001f)
            {
                return 0f;
            }

            return (float)System.Math.Atan2(inWeight, outWeight);
        }

        public static float PowerSum(float outWeight, float inWeight)
        {
            return (outWeight * outWeight) + (inWeight * inWeight);
        }

        /// <summary>
        /// Same clip that is playing, or paused/muted, is already settled.
        /// A paused source reports isPlaying false; that must not restart it.
        /// </summary>
        public static bool KeepSameClip(bool sameClip, bool playing, bool pausedOrMuted)
        {
            if (!sameClip)
            {
                return false;
            }

            if (playing)
            {
                return true;
            }

            return pausedOrMuted;
        }

        public static bool SettledSame(bool notCrossfading, bool sameClip, bool playing, bool pausedOrMuted)
        {
            if (!notCrossfading)
            {
                return false;
            }

            return KeepSameClip(sameClip, playing, pausedOrMuted);
        }

        public static MusicRetarget Begin(
            float outScale,
            float outPitch,
            int outHangar,
            float inScale,
            float inPitch,
            int inHangar)
        {
            MusicRetarget plan = new MusicRetarget();
            plan.OutScale = outScale;
            plan.OutPitch = outPitch;
            plan.OutHangar = outHangar;
            plan.InScale = inScale;
            plan.InPitch = inPitch;
            plan.InHangar = inHangar;
            plan.StartAngle = 0f;
            plan.SwapSources = 0;
            plan.ReplaceIncoming = 1;
            return plan;
        }

        /// <summary>
        /// crossfading 0 starts a fresh fade from full outgoing weight.
        /// reverse 1 swaps the two stems and keeps both weights.
        /// A third clip keeps the louder stem and inherits the quieter weight
        /// so nothing jumps to full volume.
        /// </summary>
        public static MusicRetarget Retarget(
            float outWeight,
            float inWeight,
            float outScale,
            float outPitch,
            int outHangar,
            float inScale,
            float inPitch,
            int inHangar,
            int crossfading,
            int reverse,
            float newScale,
            float newPitch,
            int newHangar)
        {
            if (crossfading == 0)
            {
                return Begin(outScale, outPitch, outHangar, newScale, newPitch, newHangar);
            }

            MusicRetarget plan = new MusicRetarget();
            if (reverse != 0)
            {
                plan.OutScale = inScale;
                plan.OutPitch = inPitch;
                plan.OutHangar = inHangar;
                plan.InScale = newScale;
                plan.InPitch = newPitch;
                plan.InHangar = newHangar;
                plan.StartAngle = AngleFromWeights(inWeight, outWeight);
                plan.SwapSources = 1;
                plan.ReplaceIncoming = 0;
                return plan;
            }

            if (inWeight > outWeight)
            {
                plan.OutScale = inScale;
                plan.OutPitch = inPitch;
                plan.OutHangar = inHangar;
                plan.InScale = newScale;
                plan.InPitch = newPitch;
                plan.InHangar = newHangar;
                plan.StartAngle = AngleFromWeights(inWeight, outWeight);
                plan.SwapSources = 1;
                plan.ReplaceIncoming = 1;
                return plan;
            }

            plan.OutScale = outScale;
            plan.OutPitch = outPitch;
            plan.OutHangar = outHangar;
            plan.InScale = newScale;
            plan.InPitch = newPitch;
            plan.InHangar = newHangar;
            plan.StartAngle = AngleFromWeights(outWeight, inWeight);
            plan.SwapSources = 0;
            plan.ReplaceIncoming = 1;
            return plan;
        }
    }
}
