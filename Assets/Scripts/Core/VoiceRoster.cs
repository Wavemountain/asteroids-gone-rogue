namespace AsteroidsGoneRogue
{
    /// <summary>
    /// One-shot voice caps from Docs/0.48/05 §4. Music is three dedicated
    /// sources. Steal drops the oldest start in that category only; it does
    /// not take a slot from another category. The daily roll is not read here.
    /// </summary>
    public static class VoiceRoster
    {
        public const int Uncapped = -1;
        public const int CatMusic = 0;
        public const int CatTell = 1;
        public const int CatPlayer = 2;
        public const int CatUi = 3;
        public const int CatWeapon = 4;
        public const int CatDeath = 5;
        public const int CatHit = 6;
        public const int CatPickup = 7;
        public const int CatEnemyShot = 8;
        public const int CategoryCount = 9;
        public const int RealVoiceLimit = 32;
        public const int SlotSpan = 30;

        public static int Cap(int category)
        {
            if (category == CatMusic)
            {
                return 3;
            }

            if (category == CatTell)
            {
                return 2;
            }

            if (category == CatPlayer)
            {
                return 2;
            }

            if (category == CatUi)
            {
                return 2;
            }

            if (category == CatWeapon)
            {
                return 6;
            }

            if (category == CatDeath)
            {
                return 6;
            }

            if (category == CatHit)
            {
                return 4;
            }

            if (category == CatPickup)
            {
                return 2;
            }

            if (category == CatEnemyShot)
            {
                return 3;
            }

            return 0;
        }

        public static int Priority(int category)
        {
            if (category == CatMusic)
            {
                return 0;
            }

            if (category == CatTell)
            {
                return 16;
            }

            if (category == CatPlayer)
            {
                return 24;
            }

            if (category == CatUi)
            {
                return 32;
            }

            if (category == CatWeapon)
            {
                return 64;
            }

            if (category == CatDeath)
            {
                return 80;
            }

            if (category == CatHit)
            {
                return 96;
            }

            if (category == CatPickup)
            {
                return 112;
            }

            if (category == CatEnemyShot)
            {
                return 128;
            }

            return 128;
        }

        public static int CategorySum()
        {
            int sum = 0;
            for (int category = 0; category < CategoryCount; category++)
            {
                sum += Cap(category);
            }

            return sum;
        }

        public static int Offset(int category)
        {
            int offset = 0;
            int last = category;
            if (last > CategoryCount)
            {
                last = CategoryCount;
            }

            for (int i = 0; i < last; i++)
            {
                offset += Cap(i);
            }

            return offset;
        }

        /// <summary>
        /// True when the cue may play. lengthSeconds is clip.length / pitch.
        /// A full category drops the new cue unless stealOldest releases the
        /// earliest start in that category.
        /// </summary>
        public static bool Admit(int category, float now, float lengthSeconds, bool stealOldest, float[] started, float[] until)
        {
            if (category < 0 || category >= CategoryCount)
            {
                return true;
            }

            int cap = Cap(category);
            if (cap <= 0)
            {
                return false;
            }

            if (started == null || until == null)
            {
                return true;
            }

            int offset = Offset(category);
            if (offset < 0 || offset + cap > started.Length || offset + cap > until.Length)
            {
                return true;
            }

            float length = lengthSeconds;
            if (length < 0.02f)
            {
                length = 0.02f;
            }

            int free = -1;
            int oldest = -1;
            float oldestStart = 0f;
            for (int i = 0; i < cap; i++)
            {
                int slot = offset + i;
                if (until[slot] <= now)
                {
                    free = slot;
                    break;
                }

                if (oldest < 0 || started[slot] < oldestStart)
                {
                    oldest = slot;
                    oldestStart = started[slot];
                }
            }

            int take = free;
            if (take < 0)
            {
                if (!stealOldest || oldest < 0)
                {
                    return false;
                }

                take = oldest;
            }

            started[take] = now;
            until[take] = now + length;
            return true;
        }

        public static bool SelfCheck()
        {
            if (CategorySum() != SlotSpan || SlotSpan > RealVoiceLimit || RealVoiceLimit != 32)
            {
                return false;
            }

            if (Cap(CatMusic) + Cap(CatTell) + Cap(CatPlayer) + Cap(CatUi) != 9)
            {
                return false;
            }

            if (Cap(CatWeapon) != 6 || Cap(CatDeath) != 6 || Cap(CatHit) != 4)
            {
                return false;
            }

            if (Cap(CatPickup) != 2 || Cap(CatEnemyShot) != 3 || Cap(CatTell) != 2 || Cap(CatPlayer) != 2)
            {
                return false;
            }

            if (Priority(CatMusic) != 0 || Priority(CatTell) != 16 || Priority(CatPlayer) != 24)
            {
                return false;
            }

            if (Priority(CatUi) != 32 || Priority(CatWeapon) != 64 || Priority(CatDeath) != 80)
            {
                return false;
            }

            if (Priority(CatHit) != 96 || Priority(CatPickup) != 112 || Priority(CatEnemyShot) != 128)
            {
                return false;
            }

            int previous = -1;
            for (int category = 0; category < CategoryCount; category++)
            {
                int priority = Priority(category);
                if (priority < previous)
                {
                    return false;
                }

                previous = priority;
            }

            if (Offset(0) != 0 || Offset(CategoryCount) != SlotSpan)
            {
                return false;
            }

            float[] started = new float[SlotSpan];
            float[] until = new float[SlotSpan];
            for (int i = 0; i < 4; i++)
            {
                if (!Admit(CatHit, 1f, 1f, false, started, until))
                {
                    return false;
                }
            }

            if (Admit(CatHit, 1f, 1f, false, started, until))
            {
                return false;
            }

            if (!Admit(CatHit, 2f, 1f, false, started, until))
            {
                return false;
            }

            for (int i = 0; i < 6; i++)
            {
                if (!Admit(CatDeath, 5f, 2f, true, started, until))
                {
                    return false;
                }
            }

            if (Admit(CatDeath, 5.2f, 2f, false, started, until))
            {
                return false;
            }

            if (!Admit(CatDeath, 5.4f, 2f, true, started, until))
            {
                return false;
            }

            int death = Offset(CatDeath);
            bool stole = false;
            for (int i = 0; i < Cap(CatDeath); i++)
            {
                if (started[death + i] == 5.4f)
                {
                    stole = true;
                }
            }

            if (!stole)
            {
                return false;
            }

            if (!Admit(CatPlayer, 0f, 3f, false, started, until))
            {
                return false;
            }

            if (!Admit(CatPlayer, 0f, 3f, false, started, until))
            {
                return false;
            }

            if (Admit(CatPlayer, 0.1f, 1f, false, started, until))
            {
                return false;
            }

            for (int i = 0; i < 6; i++)
            {
                if (!Admit(CatWeapon, 8f, 1f, false, started, until))
                {
                    return false;
                }
            }

            if (Admit(CatWeapon, 8.1f, 1f, false, started, until))
            {
                return false;
            }

            if (!Admit(CatWeapon, 8.2f, 1f, true, started, until))
            {
                return false;
            }

            if (!Admit(Uncapped, 0f, 1f, false, null, null))
            {
                return false;
            }

            return true;
        }
    }
}
