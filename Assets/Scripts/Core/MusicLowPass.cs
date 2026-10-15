namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Death low-pass on the music sources (Docs/0.48/05 §3). The curve is
    /// exponential in frequency. SFX are not filtered. The daily roll is not read here.
    /// </summary>
    public static class MusicLowPass
    {
        public const float OpenHz = 22000f;
        public const float DeathHz = 600f;
        public const float DeathSeconds = 0.6f;
        public const float ReleaseSeconds = 0.4f;
        public const float ResonanceQ = 1f;
        public const float DeathWhooshScale = 0.60f;
        public const float WaveWhooshScale = 0.75f;
        public const float WaveWhooshLeadSeconds = 0.15f;
        public const float WaveDuckSeconds = 0.8f;
        public const float WaveDuckScale = 0.6f;

        public static float LerpHz(float fromHz, float toHz, float u)
        {
            float from = fromHz;
            float to = toHz;
            if (from < 10f)
            {
                from = 10f;
            }

            if (to < 10f)
            {
                to = 10f;
            }

            if (u <= 0f)
            {
                return from;
            }

            if (u >= 1f)
            {
                return to;
            }

            double start = System.Math.Log(from);
            double end = System.Math.Log(to);
            return (float)System.Math.Exp(start + ((end - start) * u));
        }

        public static bool SelfCheck()
        {
            if (Differ(OpenHz, 22000f) || Differ(DeathHz, 600f) || Differ(ResonanceQ, 1f))
            {
                return false;
            }

            if (Differ(DeathSeconds, 0.6f) || Differ(ReleaseSeconds, 0.4f))
            {
                return false;
            }

            if (Differ(DeathWhooshScale, 0.60f) || Differ(WaveWhooshScale, 0.75f) || Differ(WaveWhooshLeadSeconds, 0.15f))
            {
                return false;
            }

            if (Differ(WaveDuckSeconds, 0.8f) || Differ(WaveDuckScale, 0.6f))
            {
                return false;
            }

            if (LerpHz(OpenHz, DeathHz, 0f) != OpenHz || LerpHz(OpenHz, DeathHz, 1f) != DeathHz)
            {
                return false;
            }

            float mid = LerpHz(OpenHz, DeathHz, 0.5f);
            float geometric = (float)System.Math.Sqrt(OpenHz * DeathHz);
            float delta = mid - geometric;
            if (delta < 0f)
            {
                delta = -delta;
            }

            if (delta > 0.5f)
            {
                return false;
            }

            if (mid <= DeathHz || mid >= OpenHz)
            {
                return false;
            }

            if (LerpHz(5f, DeathHz, 0f) != 10f)
            {
                return false;
            }

            return true;
        }

        private static bool Differ(float a, float b)
        {
            return a != b;
        }
    }
}
