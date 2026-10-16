namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Arena parallax, nebula drift, near dust, and the grid pulse
    /// (Docs/0.48/06). Dust and the grid stay on Accent. They never use
    /// Primary or Danger. The daily roll is not read here.
    /// </summary>
    public static class ArenaDepth
    {
        public const float StarFarK = 0.0004f;
        public const float StarFarDriftX = 0.0010f;
        public const float StarFarDriftY = 0f;
        public const float StarMidK = 0.0010f;
        public const float StarMidDriftX = 0.0016f;
        public const float StarMidDriftY = 0.0004f;
        public const float StarNearK = 0.0022f;
        public const float StarNearDriftX = 0.0024f;
        public const float StarNearDriftY = 0.0008f;
        public const float NebulaDrift = 0.0020f;
        public const float NebulaInnerDrift = -0.0012f;
        public const float BreatheAmp = 0.03f;
        public const float BreatheSeconds = 24f;
        public const float ReduceMotion = 0.5f;

        public const int DustFull = 40;
        public const int DustDeck = 30;
        public const int DustReduce = 12;
        public const int DeckWidth = 1280;
        public const float DustSizeMin = 0.03f;
        public const float DustSizeMax = 0.06f;
        public const float DustLife = 6f;
        public const float DustParallax = -0.15f;
        public const float DustAlphaMin = 0.10f;
        public const float DustAlphaMax = 0.18f;
        public const float DustFadeRadius = 1.5f;
        public const float DustReduceAlpha = 0.6f;
        public const float AccentR = 200f / 255f;
        public const float AccentG = 206f / 255f;
        public const float AccentB = 214f / 255f;

        public const float GridBeat1 = 0.04f;
        public const float GridBeatOther = 0.02f;
        public const float GridPulseDecay = 8f;

        public static void Parallax(
            float camX,
            float camZ,
            float time,
            float k,
            float driftX,
            float driftY,
            bool reduce,
            out float offsetX,
            out float offsetY)
        {
            float scale = 1f;
            if (reduce)
            {
                scale = ReduceMotion;
            }

            float along = time;
            if (along < 0f)
            {
                along = 0f;
            }

            offsetX = (camX * k + driftX * along) * scale;
            offsetY = (camZ * k + driftY * along) * scale;
        }

        public static float Breathe(float time, bool reduce)
        {
            if (reduce)
            {
                return 0f;
            }

            float along = time;
            if (along < 0f)
            {
                along = 0f;
            }

            double turns = along / BreatheSeconds;
            double angle = turns * 6.283185307179586;
            return BreatheAmp * (float)System.Math.Sin(angle);
        }

        public static int DustCount(bool reduce, int screenWidth)
        {
            if (reduce)
            {
                return DustReduce;
            }

            if (screenWidth > 0 && screenWidth <= DeckWidth)
            {
                return DustDeck;
            }

            return DustFull;
        }

        public static float DustAlpha(float baseAlpha, float distance, bool reduce)
        {
            float cap = DustAlphaMax;
            if (reduce)
            {
                cap *= DustReduceAlpha;
            }

            float alpha = baseAlpha;
            if (alpha < 0f)
            {
                alpha = 0f;
            }

            if (alpha > cap)
            {
                alpha = cap;
            }

            float radius = DustFadeRadius;
            if (radius < 0.01f)
            {
                radius = 0.01f;
            }

            if (distance < radius)
            {
                float near = distance / radius;
                if (near < 0f)
                {
                    near = 0f;
                }

                alpha *= near;
            }

            if (alpha > cap)
            {
                alpha = cap;
            }

            return alpha;
        }

        /// <summary>
        /// Multiplier on the resting grid alpha. BPM 0 and Reduce both stay at 1
        /// (a static grid). pulse = exp(-8 * beat fraction). Beat 1 of 4 uses
        /// 0.04; the other beats use 0.02.
        /// </summary>
        public static float GridAlphaMul(float bpm, float phaseSeconds, bool reduce)
        {
            if (reduce || bpm <= 0f)
            {
                return 1f;
            }

            float beatLength = 60f / bpm;
            if (beatLength < 0.05f)
            {
                beatLength = 0.05f;
            }

            float phase = phaseSeconds;
            if (phase < 0f)
            {
                phase = 0f;
            }

            float beatIndex = phase / beatLength;
            int beat = (int)beatIndex;
            float fraction = beatIndex - beat;
            if (fraction < 0f)
            {
                fraction = 0f;
            }

            int inBar = beat % 4;
            if (inBar < 0)
            {
                inBar += 4;
            }

            float amp = GridBeatOther;
            if (inBar == 0)
            {
                amp = GridBeat1;
            }

            float pulse = (float)System.Math.Exp(-GridPulseDecay * fraction);
            return 1f + (amp * pulse);
        }

        public static bool SelfCheck()
        {
            if (Differ(StarFarK, 0.0004f) || Differ(StarMidK, 0.0010f) || Differ(StarNearK, 0.0022f))
            {
                return false;
            }

            if (Differ(NebulaDrift, 0.0020f) || Differ(NebulaInnerDrift, -0.0012f) || Differ(BreatheAmp, 0.03f))
            {
                return false;
            }

            if (Bad(DustFull != 40 || DustDeck != 30 || DustReduce != 12 || DustParallax >= 0f))
            {
                return false;
            }

            if (Bad(DustAlphaMax > 0.18f || DustSizeMax > 0.06f) || Differ(DustFadeRadius, 1.5f))
            {
                return false;
            }

            if (Bad(AccentB <= AccentR || AccentG < 0.75f || AccentR < 0.7f))
            {
                return false;
            }

            float ox;
            float oy;
            Parallax(1000f, 250f, 10f, StarFarK, StarFarDriftX, StarFarDriftY, false, out ox, out oy);
            if (!Near(ox, 0.41f) || !Near(oy, 0.1f))
            {
                return false;
            }

            Parallax(1000f, 250f, 10f, StarFarK, StarFarDriftX, StarFarDriftY, true, out ox, out oy);
            if (!Near(ox, 0.205f) || !Near(oy, 0.05f))
            {
                return false;
            }

            if (Breathe(0f, false) != 0f || Breathe(6f, true) != 0f || !Near(Breathe(6f, false), 0.03f))
            {
                return false;
            }

            if (!Near(Breathe(18f, false), -0.03f))
            {
                return false;
            }

            if (DustCount(false, 1920) != 40 || DustCount(false, 1280) != 30 || DustCount(false, 800) != 30)
            {
                return false;
            }

            if (DustCount(true, 3440) != 12 || DustCount(true, 800) != 12 || DustCount(false, 0) != 40)
            {
                return false;
            }

            if (DustAlpha(DustAlphaMax, 0f, false) != 0f || !Near(DustAlpha(DustAlphaMax, 1.5f, false), 0.18f))
            {
                return false;
            }

            if (!Near(DustAlpha(DustAlphaMax, 0.75f, false), 0.09f))
            {
                return false;
            }

            if (!Near(DustAlpha(DustAlphaMax, 10f, true), 0.108f) || DustAlpha(0.5f, 10f, false) > DustAlphaMax)
            {
                return false;
            }

            if (GridAlphaMul(0f, 0f, false) != 1f || GridAlphaMul(120f, 0f, true) != 1f)
            {
                return false;
            }

            if (!Near(GridAlphaMul(120f, 0f, false), 1.04f) || !Near(GridAlphaMul(120f, 0.5f, false), 1.02f))
            {
                return false;
            }

            return true;
        }

        private static bool Differ(float a, float b)
        {
            return a != b;
        }

        private static bool Bad(bool failed)
        {
            return failed;
        }

        private static bool Near(float a, float b)
        {
            float delta = a - b;
            if (delta < 0f)
            {
                delta = -delta;
            }

            return delta < 0.0008f;
        }
    }
}
