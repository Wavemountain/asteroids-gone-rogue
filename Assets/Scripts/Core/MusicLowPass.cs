namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Cutoff motion for the music low-pass. Open means 22 kHz. A second reset
    /// on an already-open state is a no-op.
    /// </summary>
    public struct MusicLowPassState
    {
        public float Current;
        public float From;
        public float To;
        public float Elapsed;
        public float Seconds;
    }

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

        public static MusicLowPassState OpenState()
        {
            MusicLowPassState state = new MusicLowPassState();
            state.Current = OpenHz;
            state.From = OpenHz;
            state.To = OpenHz;
            state.Elapsed = 0f;
            state.Seconds = 0f;
            return state;
        }

        public static bool IsOpen(MusicLowPassState state)
        {
            return state.Current >= OpenHz - 0.5f && state.To >= OpenHz - 0.5f;
        }

        public static bool Same(MusicLowPassState left, MusicLowPassState right)
        {
            return left.Current == right.Current
                && left.From == right.From
                && left.To == right.To
                && left.Elapsed == right.Elapsed
                && left.Seconds == right.Seconds;
        }

        /// <summary>
        /// Snap the filter open. Calling it again on that result changes nothing.
        /// </summary>
        public static MusicLowPassState ResetOpen(MusicLowPassState state)
        {
            if (IsOpen(state))
            {
                return state;
            }

            return OpenState();
        }

        public static MusicLowPassState Command(MusicLowPassState state, float hz, float seconds)
        {
            float target = hz;
            if (target < 10f)
            {
                target = 10f;
            }

            if (seconds < 0f)
            {
                seconds = 0f;
            }

            state.From = state.Current;
            state.To = target;
            state.Elapsed = 0f;
            state.Seconds = seconds;
            if (seconds <= 0f)
            {
                state.Current = target;
            }

            return state;
        }

        public static MusicLowPassState Advance(MusicLowPassState state, float unscaledDt)
        {
            if (state.Seconds <= 0f || state.Elapsed >= state.Seconds)
            {
                return state;
            }

            if (unscaledDt < 0f)
            {
                unscaledDt = 0f;
            }

            state.Elapsed += unscaledDt;
            float span = state.Seconds;
            if (span < 0.0001f)
            {
                span = 0.0001f;
            }

            float u = state.Elapsed / span;
            if (u < 0f)
            {
                u = 0f;
            }

            if (u > 1f)
            {
                u = 1f;
            }

            state.Current = LerpHz(state.From, state.To, u);
            return state;
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

            MusicLowPassState open = OpenState();
            open.Elapsed = 0.25f;
            MusicLowPassState again = ResetOpen(open);
            if (!Same(open, again) || !IsOpen(again))
            {
                return false;
            }

            MusicLowPassState dying = Command(OpenState(), DeathHz, DeathSeconds);
            dying = Advance(dying, 0.3f);
            if (IsOpen(dying) || dying.Current >= OpenHz)
            {
                return false;
            }

            MusicLowPassState snapped = ResetOpen(dying);
            if (snapped.Current != OpenHz || snapped.To != OpenHz || snapped.Seconds != 0f)
            {
                return false;
            }

            MusicLowPassState snappedAgain = ResetOpen(snapped);
            if (!Same(snapped, snappedAgain))
            {
                return false;
            }

            MusicLowPassState releasing = Command(dying, OpenHz, ReleaseSeconds);
            if (releasing.To != OpenHz || releasing.Seconds != ReleaseSeconds || IsOpen(releasing))
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
