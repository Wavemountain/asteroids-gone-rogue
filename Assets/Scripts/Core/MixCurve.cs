namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Settings slider position p maps to gain p^2. Legacy PlayerPrefs stored
    /// the linear gain; SliderFromLinear recovers p and snaps to 0.05.
    /// Unity-free.
    /// </summary>
    public static class MixCurve
    {
        public const string CurveKey = "agr.audio.curve";
        public const int CurveVersion = 1;
        public const float DefaultMusicSlider = 0.5f;
        public const float DefaultSfxSlider = 0.9f;
        public const float MigrateStep = 0.05f;

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

        public static float Gain(float slider)
        {
            float p = Clamp01(slider);
            return p * p;
        }

        public static float RoundTo(float value, float step)
        {
            if (step <= 0.0001f)
            {
                return Clamp01(value);
            }

            float snapped = (float)System.Math.Round(value / step) * step;
            snapped = (float)System.Math.Round(snapped, 2);
            return Clamp01(snapped);
        }

        public static float SliderFromLinear(float linearGain)
        {
            float gain = Clamp01(linearGain);
            if (gain <= 0f)
            {
                return 0f;
            }

            float slider = (float)System.Math.Sqrt(gain);
            return RoundTo(slider, MigrateStep);
        }
    }
}
