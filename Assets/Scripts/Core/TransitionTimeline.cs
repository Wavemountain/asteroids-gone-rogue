namespace AsteroidsGoneRogue
{
    public enum TransitionKind
    {
        None = 0,
        Death = 1,
        WaveClear = 2,
    }

    /// <summary>
    /// Soft death is 0.6 s slow-mo, then a 0.22 s fail-panel slide.
    /// The 0.25 s hitstop and the explosion run before this clock starts.
    /// Wave clear is a 0.25 s slow-mo ease, then a fade and slide.
    /// Reduce effects skips the slow-mo and only fades. Input stays closed
    /// until the fade ends, and a button held across that edge is swallowed.
    /// Pause freezes the clock. Unity-free.
    /// </summary>
    public sealed class TransitionTimeline
    {
        public const float DeathSlowSeconds = 0.6f;
        public const float DeathFadeSeconds = 0.220f;
        public const float DeathSlowScale = 0.35f;
        public const float WaveBreathSeconds = 0.25f;
        public const float WaveFadeSeconds = 0.28f;
        public const float WaveSlowScale = 0.45f;
        public const float SlidePixels = 28f;

        public TransitionKind Kind;
        public float Elapsed;
        public bool ShatterDone;
        public bool Committed;
        public bool HeldAcross;
        public bool Swallow;

        public void Begin(TransitionKind kind)
        {
            Kind = kind;
            Elapsed = 0f;
            ShatterDone = false;
            Committed = false;
            HeldAcross = false;
            Swallow = false;
        }

        public void Clear()
        {
            Kind = TransitionKind.None;
            Elapsed = 0f;
            ShatterDone = false;
            Committed = false;
            HeldAcross = false;
            Swallow = false;
        }

        public void MarkShatter()
        {
            ShatterDone = true;
        }

        public void MarkCommitted()
        {
            Committed = true;
        }

        public float SlowSeconds(bool reduce)
        {
            if (reduce || !EffectScale.AllowsSlowMo(reduce))
            {
                return 0f;
            }

            if (Kind == TransitionKind.Death)
            {
                return DeathSlowSeconds;
            }

            if (Kind == TransitionKind.WaveClear)
            {
                return WaveBreathSeconds;
            }

            return 0f;
        }

        public float FadeSeconds()
        {
            if (Kind == TransitionKind.Death)
            {
                return DeathFadeSeconds;
            }

            if (Kind == TransitionKind.WaveClear)
            {
                return WaveFadeSeconds;
            }

            return 0f;
        }

        public void Advance(float unscaledDelta, bool held, bool reduce, bool paused)
        {
            if (Kind == TransitionKind.None || paused)
            {
                return;
            }

            float step = unscaledDelta;
            if (step < 0f)
            {
                step = 0f;
            }

            if (step > 0.05f)
            {
                step = 0.05f;
            }

            Elapsed += step;
            NoteHeld(held, reduce);
        }

        public float TimeScale(bool reduce)
        {
            if (Kind == TransitionKind.None || reduce || !EffectScale.AllowsSlowMo(reduce))
            {
                return 1f;
            }

            float slow = SlowSeconds(false);
            if (slow <= 0.0001f || Elapsed >= slow)
            {
                return 1f;
            }

            float u = Elapsed / slow;
            if (u < 0f)
            {
                u = 0f;
            }

            if (u > 1f)
            {
                u = 1f;
            }

            if (Kind == TransitionKind.WaveClear)
            {
                float dip = (float)System.Math.Sin(u * System.Math.PI);
                return 1f - ((1f - WaveSlowScale) * dip);
            }

            float ease = u / 0.2f;
            if (ease < 0f)
            {
                ease = 0f;
            }

            if (ease > 1f)
            {
                ease = 1f;
            }

            float smooth = ease * ease * (3f - (2f * ease));
            return 1f - ((1f - DeathSlowScale) * smooth);
        }

        public float CardAlpha(bool reduce)
        {
            if (Kind == TransitionKind.None)
            {
                return 1f;
            }

            if (!Committed)
            {
                return 0f;
            }

            float fade = FadeSeconds();
            if (fade <= 0.0001f)
            {
                return 1f;
            }

            float u = (Elapsed - SlowSeconds(reduce)) / fade;
            if (u < 0f)
            {
                u = 0f;
            }

            if (u > 1f)
            {
                u = 1f;
            }

            return u * u * (3f - (2f * u));
        }

        public float CardSlide(bool reduce)
        {
            float alpha = CardAlpha(reduce);
            float slide = (1f - alpha) * SlidePixels;
            if (slide < 0f)
            {
                slide = 0f;
            }

            return slide;
        }

        public bool ShatterDue(bool reduce)
        {
            if (Kind != TransitionKind.Death || ShatterDone)
            {
                return false;
            }

            return Elapsed >= SlowSeconds(reduce);
        }

        public bool ReadyToCommit(bool reduce)
        {
            if (Kind == TransitionKind.None || Committed)
            {
                return false;
            }

            return Elapsed >= SlowSeconds(reduce);
        }

        public bool InputOpen(bool reduce)
        {
            if (Kind == TransitionKind.None)
            {
                return true;
            }

            return Elapsed >= SlowSeconds(reduce) + FadeSeconds();
        }

        public bool BlocksInput(bool reduce)
        {
            if (Kind == TransitionKind.None)
            {
                return false;
            }

            if (!InputOpen(reduce))
            {
                return true;
            }

            return HeldAcross || Swallow;
        }

        private void NoteHeld(bool held, bool reduce)
        {
            if (!InputOpen(reduce))
            {
                Swallow = false;
                HeldAcross = held;
                return;
            }

            if (HeldAcross)
            {
                if (!held)
                {
                    Swallow = true;
                    HeldAcross = false;
                }

                return;
            }

            Swallow = false;
        }
    }

    /// <summary>
    /// Pause wins. Hitstop uses a small non-zero scale so physics steps stay
    /// valid and the music pause (scale at or under 0.0001) does not trip.
    /// </summary>
    public static class FeelClock
    {
        public const float HitstopTimeScale = 0.02f;
        public const float PauseEpsilon = 0.0001f;

        public static float Compose(float pauseScale, bool hitstopActive, float transitionScale)
        {
            if (pauseScale <= PauseEpsilon)
            {
                return 0f;
            }

            float scale = pauseScale;
            if (scale > 1f)
            {
                scale = 1f;
            }

            if (hitstopActive && HitstopTimeScale < scale)
            {
                scale = HitstopTimeScale;
            }

            if (transitionScale < scale)
            {
                scale = transitionScale;
            }

            if (scale < 0f)
            {
                scale = 0f;
            }

            if (scale > 1f)
            {
                scale = 1f;
            }

            return scale;
        }
    }
}
