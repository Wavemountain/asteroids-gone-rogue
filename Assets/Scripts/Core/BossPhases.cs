namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Boss phase thresholds and the stagger HP gate. Integer math only.
    /// Unity-free.
    /// </summary>
    public enum BossPhase
    {
        One = 0,
        Two = 1,
        Three = 2,
    }

    public static class BossPhases
    {
        public const int StaggerMs = 1000;

        public static int ThresholdHp(int maxHp, BossPhase phase)
        {
            int hp = maxHp < 1 ? 1 : maxHp;
            if (phase == BossPhase.Two)
            {
                return CeilPercent(hp, 50);
            }

            if (phase == BossPhase.Three)
            {
                return CeilPercent(hp, 20);
            }

            return hp;
        }

        public static BossPhase PhaseForHp(int hp, int maxHp)
        {
            int two = ThresholdHp(maxHp, BossPhase.Two);
            int three = ThresholdHp(maxHp, BossPhase.Three);
            if (hp > two)
            {
                return BossPhase.One;
            }

            if (hp > three)
            {
                return BossPhase.Two;
            }

            return BossPhase.Three;
        }

        /// <summary>
        /// While a stagger gate is active, HP cannot fall through the floor.
        /// Damage is still applied down to that floor.
        /// </summary>
        public static int GateHp(int hp, int floorHp, bool gateActive)
        {
            int next = hp;
            if (next < 0)
            {
                next = 0;
            }

            if (!gateActive)
            {
                return next;
            }

            int floor = floorHp;
            if (floor < 1)
            {
                floor = 1;
            }

            if (next < floor)
            {
                return floor;
            }

            return next;
        }

        public static bool GateActive(float now, float until)
        {
            return now < until;
        }

        public static int ApplyDamage(int hp, int damage, int floorHp, bool gateActive)
        {
            int next = hp - damage;
            if (next < 0)
            {
                next = 0;
            }

            return GateHp(next, floorHp, gateActive);
        }

        public static int CeilPercent(int maxHp, int percent)
        {
            int hp = maxHp < 1 ? 1 : maxHp;
            int part = percent;
            if (part < 0)
            {
                part = 0;
            }

            int value = (hp * part + 99) / 100;
            if (value < 1)
            {
                value = 1;
            }

            if (value > hp)
            {
                value = hp;
            }

            return value;
        }

        public static bool SelfCheck()
        {
            int[] samples = new int[] { 1, 5, 96, 180, 601 };
            for (int index = 0; index < samples.Length; index++)
            {
                int max = samples[index];
                int two = ThresholdHp(max, BossPhase.Two);
                int three = ThresholdHp(max, BossPhase.Three);
                if (two < three || two > max || three < 1)
                {
                    return false;
                }

                if (PhaseForHp(two + 1, max) != BossPhase.One && two < max)
                {
                    return false;
                }

                if (two > three)
                {
                    if (PhaseForHp(two, max) != BossPhase.Two)
                    {
                        return false;
                    }

                    if (PhaseForHp(three + 1, max) != BossPhase.Two)
                    {
                        return false;
                    }
                }

                if (PhaseForHp(three, max) != BossPhase.Three)
                {
                    return false;
                }

                if (three > 1 && PhaseForHp(three - 1, max) != BossPhase.Three)
                {
                    return false;
                }
            }

            int maxHp = 180;
            int atHalf = ThresholdHp(maxHp, BossPhase.Two);
            int atFifth = ThresholdHp(maxHp, BossPhase.Three);
            int from = CeilPercent(maxHp, 55);
            int gated = ApplyDamage(from, from, atFifth, true);
            if (gated != atFifth || gated == atHalf || gated == 0)
            {
                return false;
            }

            if (ApplyDamage(10, 100, 1, true) != 1)
            {
                return false;
            }

            if (ApplyDamage(from, from, atFifth, false) != 0)
            {
                return false;
            }

            if (!GateActive(0f, 1f) || GateActive(1f, 1f) || GateActive(1.01f, 1f))
            {
                return false;
            }

            return ThresholdHp(5, BossPhase.Two) == 3 && ThresholdHp(5, BossPhase.Three) == 1;
        }
    }
}
