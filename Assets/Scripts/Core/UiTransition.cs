namespace AsteroidsGoneRogue
{
    public enum UiMotionKind
    {
        None = 0,
        Menu = 1,
        Screen = 2,
        Cards = 3,
    }

    public enum UiMotionDir
    {
        In = 0,
        Out = 1,
        Swap = 2,
    }

    /// <summary>
    /// Motion tokens from the 0.48 type brief. Cubic-bezier curves are evaluated
    /// in place so menus do not need an AnimationCurve. Unity-free.
    /// </summary>
    public static class UiMotion
    {
        public const float Fast = 0.150f;
        public const float Base = 0.200f;
        public const float Slow = 0.250f;
        public const float Exit = 0.150f;
        public const int Space1 = 4;
        public const int Space2 = 8;
        public const int Space3 = 12;
        public const int Space4 = 16;
        public const int Space5 = 24;
        public const int Space6 = 32;
        public const int Space7 = 48;
        public const float SlidePixels = 16f;
        public const float ScaleFrom = 0.98f;
        public const float StepCap = 0.05f;

        public static float EaseOut(float t)
        {
            return Cubic(t, 0.2f, 0f, 0f, 1f);
        }

        public static float EaseIn(float t)
        {
            return Cubic(t, 0.4f, 0f, 1f, 1f);
        }

        public static float EaseInOut(float t)
        {
            return Cubic(t, 0.4f, 0f, 0.2f, 1f);
        }

        public static float Cubic(float x, float x1, float y1, float x2, float y2)
        {
            if (x <= 0f)
            {
                return 0f;
            }

            if (x >= 1f)
            {
                return 1f;
            }

            float t = x;
            for (int step = 0; step < 8; step++)
            {
                float xt = Bezier(t, x1, x2);
                float dx = BezierSlope(t, x1, x2);
                if (dx < 0.0001f && dx > -0.0001f)
                {
                    break;
                }

                t -= (xt - x) / dx;
                if (t < 0f)
                {
                    t = 0f;
                }

                if (t > 1f)
                {
                    t = 1f;
                }
            }

            return Bezier(t, y1, y2);
        }

        private static float Bezier(float t, float c1, float c2)
        {
            float u = 1f - t;
            return (3f * u * u * t * c1) + (3f * u * t * t * c2) + (t * t * t);
        }

        private static float BezierSlope(float t, float c1, float c2)
        {
            float u = 1f - t;
            return (3f * u * u * c1) + (6f * u * t * (c2 - c1)) + (3f * t * t * (1f - c2));
        }
    }

    /// <summary>
    /// Menu and panel open/close. Unscaled time, pause freezes the clock,
    /// and a button held across the end cannot skip the next action.
    /// Reduce effects keeps the fade and drops slide, scale, and stagger.
    /// </summary>
    public sealed class UiTransition
    {
        public const float MenuOutSeconds = UiMotion.Exit;
        public const float MenuInSeconds = UiMotion.Base;
        public const float MenuOverlapSeconds = 0.050f;
        public const float ScreenSeconds = UiMotion.Slow;
        public const float ScreenHoldSeconds = 0.060f;
        public const float CardSeconds = UiMotion.Base;
        public const float CardStaggerSeconds = 0.060f;

        public UiMotionKind Kind;
        public UiMotionDir Dir;
        public float Elapsed;
        public int CardCount;
        public bool Active;
        public bool HeldAcross;
        public bool Swallow;

        private bool _closeReady;
        private bool _openReady;
        private float _openAt;

        public void Begin(UiMotionKind kind, UiMotionDir dir, int cardCount)
        {
            Kind = kind;
            Dir = dir;
            CardCount = cardCount < 1 ? 1 : cardCount;
            Elapsed = 0f;
            Active = kind != UiMotionKind.None;
            HeldAcross = false;
            Swallow = false;
            _closeReady = false;
            _openReady = false;
            _openAt = -1f;
            if (!Active)
            {
                return;
            }

            if (kind == UiMotionKind.Screen)
            {
                _closeReady = true;
                _openAt = ScreenSeconds + ScreenHoldSeconds;
                return;
            }

            if (kind == UiMotionKind.Cards || dir == UiMotionDir.In)
            {
                _openReady = true;
                return;
            }

            if (dir == UiMotionDir.Out)
            {
                _closeReady = true;
                return;
            }

            _closeReady = true;
            _openAt = MenuOutSeconds - MenuOverlapSeconds;
        }

        public void Clear()
        {
            Kind = UiMotionKind.None;
            Dir = UiMotionDir.In;
            Elapsed = 0f;
            CardCount = 1;
            Active = false;
            HeldAcross = false;
            Swallow = false;
            _closeReady = false;
            _openReady = false;
            _openAt = -1f;
        }

        public float Span(bool reduce)
        {
            if (Kind == UiMotionKind.Menu)
            {
                if (Dir == UiMotionDir.In)
                {
                    return MenuInSeconds;
                }

                if (Dir == UiMotionDir.Out)
                {
                    return MenuOutSeconds;
                }

                return MenuOutSeconds + MenuInSeconds - MenuOverlapSeconds;
            }

            if (Kind == UiMotionKind.Screen)
            {
                return ScreenSeconds + ScreenHoldSeconds + ScreenSeconds;
            }

            if (Kind == UiMotionKind.Cards)
            {
                int count = CardCount < 1 ? 1 : CardCount;
                float stagger = reduce ? 0f : CardStaggerSeconds;
                return ((count - 1) * stagger) + CardSeconds;
            }

            return 0f;
        }

        public bool Running(bool reduce)
        {
            if (!Active)
            {
                return false;
            }

            return Elapsed + 0.0001f < Span(reduce);
        }

        public void Advance(float unscaledDelta, bool held, bool reduce, bool paused)
        {
            if (!Active || paused)
            {
                return;
            }

            float step = unscaledDelta;
            if (step < 0f)
            {
                step = 0f;
            }

            if (step > UiMotion.StepCap)
            {
                step = UiMotion.StepCap;
            }

            Elapsed += step;
            if (_openAt >= 0f && !_openReady && Elapsed + 0.0001f >= _openAt)
            {
                _openReady = true;
            }

            NoteHeld(held, reduce);
        }

        public bool TakeOpenCue()
        {
            if (!_openReady)
            {
                return false;
            }

            _openReady = false;
            return true;
        }

        public bool TakeCloseCue()
        {
            if (!_closeReady)
            {
                return false;
            }

            _closeReady = false;
            return true;
        }

        public void SampleIn(bool reduce, out float alpha, out float slide, out float scale)
        {
            Sample(true, reduce, out alpha, out slide, out scale);
        }

        public void SampleOut(bool reduce, out float alpha, out float slide, out float scale)
        {
            Sample(false, reduce, out alpha, out slide, out scale);
        }

        public float Veil(bool reduce)
        {
            if (Kind != UiMotionKind.Screen || !Active)
            {
                return 0f;
            }

            float fade = ScreenSeconds;
            if (fade < 0.0001f)
            {
                fade = 0.0001f;
            }

            if (Elapsed <= fade)
            {
                return UiMotion.EaseInOut(Elapsed / fade);
            }

            if (Elapsed <= fade + ScreenHoldSeconds)
            {
                return 1f;
            }

            float u = (Elapsed - fade - ScreenHoldSeconds) / fade;
            if (u < 0f)
            {
                u = 0f;
            }

            if (u > 1f)
            {
                u = 1f;
            }

            return 1f - UiMotion.EaseInOut(u);
        }

        public float CardAlpha(int index, bool reduce)
        {
            float local = CardLocal(index, reduce);
            if (local <= 0f)
            {
                return 0f;
            }

            float u = local / CardSeconds;
            if (u > 1f)
            {
                u = 1f;
            }

            return UiMotion.EaseOut(u);
        }

        public float CardScale(int index, bool reduce)
        {
            if (reduce)
            {
                return 1f;
            }

            float alpha = CardAlpha(index, false);
            return UiMotion.ScaleFrom + ((1f - UiMotion.ScaleFrom) * alpha);
        }

        public float CardLocal(int index, bool reduce)
        {
            if (index < 0)
            {
                index = 0;
            }

            float start = reduce ? 0f : index * CardStaggerSeconds;
            return Elapsed - start;
        }

        public bool BlocksInput(bool reduce)
        {
            if (!Active)
            {
                return false;
            }

            if (Running(reduce))
            {
                return true;
            }

            return HeldAcross || Swallow;
        }

        private void Sample(bool incoming, bool reduce, out float alpha, out float slide, out float scale)
        {
            alpha = incoming ? 1f : 0f;
            slide = 0f;
            scale = 1f;
            if (!Active || Kind == UiMotionKind.None)
            {
                alpha = 1f;
                return;
            }

            if (Kind != UiMotionKind.Menu)
            {
                alpha = incoming ? 1f : 0f;
                return;
            }

            float start;
            float duration;
            bool easeOut;
            if (Dir == UiMotionDir.In)
            {
                if (!incoming)
                {
                    alpha = 0f;
                    return;
                }

                start = 0f;
                duration = MenuInSeconds;
                easeOut = true;
            }
            else if (Dir == UiMotionDir.Out)
            {
                if (incoming)
                {
                    alpha = 0f;
                    return;
                }

                start = 0f;
                duration = MenuOutSeconds;
                easeOut = false;
            }
            else if (incoming)
            {
                start = MenuOutSeconds - MenuOverlapSeconds;
                duration = MenuInSeconds;
                easeOut = true;
            }
            else
            {
                start = 0f;
                duration = MenuOutSeconds;
                easeOut = false;
            }

            if (duration < 0.0001f)
            {
                duration = 0.0001f;
            }

            float u = (Elapsed - start) / duration;
            if (u < 0f)
            {
                alpha = incoming ? 0f : 1f;
                if (!reduce && incoming)
                {
                    slide = UiMotion.SlidePixels;
                    scale = UiMotion.ScaleFrom;
                }

                if (!reduce && !incoming)
                {
                    slide = 0f;
                    scale = 1f;
                }

                return;
            }

            if (u > 1f)
            {
                u = 1f;
            }

            float shaped = easeOut ? UiMotion.EaseOut(u) : UiMotion.EaseIn(u);
            if (easeOut)
            {
                alpha = shaped;
                if (!reduce)
                {
                    slide = (1f - shaped) * UiMotion.SlidePixels;
                    scale = UiMotion.ScaleFrom + ((1f - UiMotion.ScaleFrom) * shaped);
                }
            }
            else
            {
                alpha = 1f - shaped;
                if (!reduce)
                {
                    slide = shaped * UiMotion.SlidePixels;
                    scale = 1f - ((1f - UiMotion.ScaleFrom) * shaped);
                }
            }
        }

        private void NoteHeld(bool held, bool reduce)
        {
            if (Running(reduce))
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
    /// Score and credit count-up. Combat HUD snaps small gains.
    /// Reduce effects snaps immediately and skips the end pop.
    /// </summary>
    public sealed class UiCountUp
    {
        public const float SnapBelow = 50f;
        public const float MinSeconds = 0.25f;
        public const float MaxSeconds = 0.6f;
        public const float LogScale = 0.05f;
        public const float HudSeconds = 0.25f;
        public const float PopPeak = 1.08f;

        public float From;
        public float To;
        public float Elapsed;
        public float Seconds;

        public static float Duration(float delta)
        {
            if (delta < 0f)
            {
                delta = -delta;
            }

            float log = (float)System.Math.Log10(delta + 1f);
            float seconds = MinSeconds + (LogScale * log);
            if (seconds < MinSeconds)
            {
                seconds = MinSeconds;
            }

            if (seconds > MaxSeconds)
            {
                seconds = MaxSeconds;
            }

            return seconds;
        }

        public static float PopScale(float elapsed, float duration, bool reduce)
        {
            if (reduce || elapsed < 0f || duration <= 0.0001f || elapsed >= duration)
            {
                return 1f;
            }

            float u = elapsed / duration;
            float tri = u < 0.5f ? u * 2f : (1f - u) * 2f;
            return 1f + ((PopPeak - 1f) * tri);
        }

        public void Snap(float value)
        {
            From = value;
            To = value;
            Elapsed = 0f;
            Seconds = 0f;
        }

        public void Retarget(float value, bool combatHud, bool reduce)
        {
            if (System.Math.Abs(value - To) < 0.001f && (Seconds <= 0f || Elapsed >= Seconds))
            {
                To = value;
                From = value;
                return;
            }

            if (System.Math.Abs(value - To) < 0.001f)
            {
                return;
            }

            if (reduce)
            {
                Snap(value);
                return;
            }

            float shown = Display();
            float delta = value - shown;
            if (delta < 0f)
            {
                delta = -delta;
            }

            if (combatHud && delta < SnapBelow)
            {
                Snap(value);
                return;
            }

            From = shown;
            To = value;
            Elapsed = 0f;
            Seconds = combatHud ? HudSeconds : Duration(delta);
        }

        public void Advance(float unscaledDelta, bool paused)
        {
            if (paused || Seconds <= 0f)
            {
                return;
            }

            float step = unscaledDelta;
            if (step < 0f)
            {
                step = 0f;
            }

            if (step > UiMotion.StepCap)
            {
                step = UiMotion.StepCap;
            }

            Elapsed += step;
        }

        public float Display()
        {
            if (Seconds <= 0.0001f)
            {
                return To;
            }

            float u = Elapsed / Seconds;
            if (u < 0f)
            {
                u = 0f;
            }

            if (u > 1f)
            {
                u = 1f;
            }

            return From + ((To - From) * UiMotion.EaseOut(u));
        }

        public int Rounded()
        {
            float value = Display();
            if (value >= 0f)
            {
                return (int)(value + 0.5f);
            }

            return (int)(value - 0.5f);
        }
    }

    /// <summary>
    /// Menu one-shot scales and the hover gap. Hover is dropped while a
    /// confirm or back is still the priority voice.
    /// </summary>
    public static class UiSfxGate
    {
        public const float HoverGapSeconds = 0.06f;
        public const float HoverScale = 0.45f;
        public const float ConfirmScale = 0.55f;
        public const float BackScale = 0.85f;
        public const float OpenScale = 0.70f;
        public const float CloseScale = 0.70f;
        public const int UiVoiceCap = 2;

        public static bool AllowHover(float now, float lastHover, bool priorityBusy)
        {
            if (priorityBusy)
            {
                return false;
            }

            if (HoverGapSeconds <= 0f || lastHover <= 0f)
            {
                return true;
            }

            return now - lastHover >= HoverGapSeconds;
        }
    }
}
