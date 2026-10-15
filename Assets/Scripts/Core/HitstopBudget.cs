namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Grants 30–60 ms hitstops and refuses to chain more than 120 ms inside
    /// any one-second window. Reduce effects grants nothing. Unity-free.
    /// The driver applies the freeze with unscaled time and a non-zero scale
    /// so the music hold (timeScale == 0) and fixedDeltaTime stay untouched.
    /// </summary>
    public sealed class HitstopBudget
    {
        public const float MinSeconds = 0.03f;
        public const float MaxSeconds = 0.06f;
        public const float WindowSeconds = 1f;
        public const float CapSeconds = 0.12f;
        public const int SlotCount = 8;
        public const float HullSeconds = 0.04f;
        public const float HeavySeconds = 0.06f;
        public const float BossSeconds = 0.045f;

        private readonly float[] _stamp = new float[SlotCount];
        private readonly float[] _grant = new float[SlotCount];
        private float _until;

        public float ActiveUntil
        {
            get { return _until; }
        }

        public bool Active(float now)
        {
            return now < _until;
        }

        public float UsedInWindow(float now)
        {
            float used = 0f;
            for (int slot = 0; slot < SlotCount; slot++)
            {
                if (_grant[slot] <= 0f)
                {
                    continue;
                }

                float age = now - _stamp[slot];
                if (age < 0f)
                {
                    age = 0f;
                }

                if (age < WindowSeconds)
                {
                    used += _grant[slot];
                }
            }

            return used;
        }

        public float Request(float now, float requested, bool reduce)
        {
            if (reduce || !EffectScale.AllowsHitstop(reduce) || requested <= 0f)
            {
                return 0f;
            }

            float ask = requested;
            if (ask < MinSeconds)
            {
                ask = MinSeconds;
            }

            if (ask > MaxSeconds)
            {
                ask = MaxSeconds;
            }

            float used = 0f;
            for (int slot = 0; slot < SlotCount; slot++)
            {
                if (_grant[slot] <= 0f)
                {
                    continue;
                }

                float age = now - _stamp[slot];
                if (age < 0f)
                {
                    age = 0f;
                }

                if (age < WindowSeconds)
                {
                    used += _grant[slot];
                }
                else
                {
                    _grant[slot] = 0f;
                }
            }

            float room = CapSeconds - used;
            if (room <= 0.0005f)
            {
                return 0f;
            }

            if (ask > room)
            {
                ask = room;
            }

            int open = -1;
            for (int slot = 0; slot < SlotCount; slot++)
            {
                if (_grant[slot] <= 0f)
                {
                    open = slot;
                    break;
                }
            }

            if (open < 0)
            {
                open = 0;
                float oldest = _stamp[0];
                for (int slot = 1; slot < SlotCount; slot++)
                {
                    if (_stamp[slot] < oldest)
                    {
                        oldest = _stamp[slot];
                        open = slot;
                    }
                }
            }

            _stamp[open] = now;
            _grant[open] = ask;
            float start = now;
            if (_until > start)
            {
                start = _until;
            }

            _until = start + ask;
            return ask;
        }
    }
}
