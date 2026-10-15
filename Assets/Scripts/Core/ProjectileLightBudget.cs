namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Point-light budget for rail shots, heavy kills, and pierce shots
    /// (Docs/0.48/02_lighting_rim_key.md §3). Other shots stay unlit.
    /// Unity-free. The daily roll is not read here.
    /// </summary>
    public struct ProjectileLightSlot
    {
        public int Kind;
        public float Until;
        public int ColorId;
    }

    public static class ProjectileLightBudget
    {
        public const int PoolSize = 4;
        public const int MaxFull = 4;
        public const int KindNone = 0;
        public const int KindPierce = 1;
        public const int KindKill = 2;
        public const int KindRail = 3;

        public const float RailIntensity = 2.0f;
        public const float RailRange = 4f;
        public const float RailSeconds = 0.12f;
        public const float KillIntensity = 2.5f;
        public const float KillRange = 6f;
        public const float KillSeconds = 0.18f;
        public const float PierceIntensity = 1.2f;
        public const float PierceRange = 3f;
        public const float PierceSeconds = 2.4f;

        public static int ActiveCap(bool reduce, int pixelLightCount)
        {
            int cap = MaxFull;
            if (reduce)
            {
                cap = EffectScale.ProjectileLightMaxReduced;
            }

            if (pixelLightCount <= 0)
            {
                return 0;
            }

            if (pixelLightCount < cap)
            {
                return pixelLightCount;
            }

            return cap;
        }

        public static float IntensityFor(int kind, bool reduce)
        {
            float value = 0f;
            if (kind == KindRail)
            {
                value = RailIntensity;
            }
            else if (kind == KindKill)
            {
                value = KillIntensity;
            }
            else if (kind == KindPierce)
            {
                value = PierceIntensity;
            }

            if (reduce)
            {
                value *= EffectScale.ProjectileLightMul;
            }

            return value;
        }

        public static float RangeFor(int kind)
        {
            if (kind == KindRail)
            {
                return RailRange;
            }

            if (kind == KindKill)
            {
                return KillRange;
            }

            if (kind == KindPierce)
            {
                return PierceRange;
            }

            return 0f;
        }

        public static float SecondsFor(int kind)
        {
            if (kind == KindRail)
            {
                return RailSeconds;
            }

            if (kind == KindKill)
            {
                return KillSeconds;
            }

            if (kind == KindPierce)
            {
                return PierceSeconds;
            }

            return 0f;
        }

        public static int Priority(int kind)
        {
            if (kind == KindRail)
            {
                return 3;
            }

            if (kind == KindKill)
            {
                return 2;
            }

            if (kind == KindPierce)
            {
                return 1;
            }

            return 0;
        }

        /// <summary>
        /// Take a free slot under <paramref name="cap"/>, or steal the lowest
        /// priority slot when the new kind outranks it. Returns -1 when full
        /// of equal or higher lights. No random selection.
        /// </summary>
        public static int TryAcquire(ProjectileLightSlot[] slots, int cap, int kind, float now, float seconds, int colorId)
        {
            if (slots == null || kind == KindNone || cap <= 0)
            {
                return -1;
            }

            if (seconds < 0f)
            {
                seconds = 0f;
            }

            int limit = cap;
            if (limit > slots.Length)
            {
                limit = slots.Length;
            }

            if (limit > PoolSize)
            {
                limit = PoolSize;
            }

            int free = -1;
            int steal = -1;
            int stealPriority = 99;
            float stealUntil = 0f;
            int index = 0;
            while (index < limit)
            {
                bool expired = slots[index].Kind == KindNone || slots[index].Until <= now;
                if (expired)
                {
                    if (free < 0)
                    {
                        free = index;
                    }
                }
                else if (Priority(kind) > Priority(slots[index].Kind))
                {
                    int pri = Priority(slots[index].Kind);
                    if (pri < stealPriority || (pri == stealPriority && slots[index].Until < stealUntil))
                    {
                        steal = index;
                        stealPriority = pri;
                        stealUntil = slots[index].Until;
                    }
                }

                index++;
            }

            int chosen = free >= 0 ? free : steal;
            if (chosen < 0)
            {
                return -1;
            }

            ProjectileLightSlot slot = slots[chosen];
            slot.Kind = kind;
            slot.Until = now + seconds;
            slot.ColorId = colorId;
            slots[chosen] = slot;
            return chosen;
        }

        public static int LiveCount(ProjectileLightSlot[] slots, int cap, float now)
        {
            if (slots == null || cap <= 0)
            {
                return 0;
            }

            int limit = cap;
            if (limit > slots.Length)
            {
                limit = slots.Length;
            }

            int live = 0;
            int index = 0;
            while (index < limit)
            {
                if (slots[index].Kind != KindNone && slots[index].Until > now)
                {
                    live++;
                }

                index++;
            }

            return live;
        }

        public static bool SelfCheck()
        {
            if (Bad(PoolSize != 4 || MaxFull != 4 || EffectScale.ProjectileLightMaxReduced != 2))
            {
                return false;
            }

            if (Bad(EffectScale.ProjectileLightMul != 0.5f))
            {
                return false;
            }

            if (ActiveCap(false, 6) != 4 || ActiveCap(true, 6) != 2 || ActiveCap(false, 3) != 3)
            {
                return false;
            }

            if (ActiveCap(false, 0) != 0 || ActiveCap(true, 1) != 1 || ActiveCap(true, 8) != 2)
            {
                return false;
            }

            if (IntensityFor(KindRail, false) != RailIntensity || IntensityFor(KindRail, true) != 1f)
            {
                return false;
            }

            if (IntensityFor(KindKill, true) != KillIntensity * 0.5f || RangeFor(KindPierce) != PierceRange)
            {
                return false;
            }

            if (Priority(KindRail) <= Priority(KindKill) || Priority(KindKill) <= Priority(KindPierce))
            {
                return false;
            }

            ProjectileLightSlot[] slots = new ProjectileLightSlot[PoolSize];
            int guard = 0;
            while (guard < PoolSize)
            {
                int taken = TryAcquire(slots, 4, KindPierce, 0f, PierceSeconds, EmissionTargets.Primary);
                if (taken != guard)
                {
                    return false;
                }

                guard++;
            }

            if (TryAcquire(slots, 4, KindPierce, 0.1f, PierceSeconds, EmissionTargets.Primary) >= 0)
            {
                return false;
            }

            if (LiveCount(slots, 4, 0.1f) != 4)
            {
                return false;
            }

            int stolen = TryAcquire(slots, 4, KindRail, 0.2f, RailSeconds, EmissionTargets.Primary);
            if (stolen < 0 || slots[stolen].Kind != KindRail)
            {
                return false;
            }

            if (LiveCount(slots, 4, 0.2f) != 4)
            {
                return false;
            }

            ProjectileLightSlot[] reduced = new ProjectileLightSlot[PoolSize];
            int first = TryAcquire(reduced, ActiveCap(true, 6), KindKill, 1f, KillSeconds, EmissionTargets.Danger);
            int second = TryAcquire(reduced, ActiveCap(true, 6), KindPierce, 1f, PierceSeconds, EmissionTargets.Primary);
            int third = TryAcquire(reduced, ActiveCap(true, 6), KindRail, 1f, RailSeconds, EmissionTargets.Primary);
            if (first < 0 || second < 0 || third < 0 || reduced[third].Kind != KindRail)
            {
                return false;
            }

            if (third == first)
            {
                return false;
            }

            if (LiveCount(reduced, 2, 1f) != 2 || LiveCount(reduced, 0, 1f) != 0)
            {
                return false;
            }

            if (TryAcquire(null, 4, KindRail, 0f, RailSeconds, EmissionTargets.Primary) != -1)
            {
                return false;
            }

            return true;
        }

        private static bool Bad(bool failed)
        {
            return failed;
        }
    }
}
