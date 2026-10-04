namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Optional New Run mutators. Mask 0 is the shipped game: every scale
    /// function returns its input unchanged.
    /// </summary>
    public static class MutatorCatalog
    {
        public const int Glass = 0;
        public const int Swarm = 1;
        public const int Heavy = 2;
        public const int Quiet = 3;
        public const int Overclock = 4;
        public const int LongHaul = 5;
        public const int Count = 6;
        public const int MaxPicks = 2;

        public static int Bit(int id)
        {
            if (id < 0 || id >= Count)
            {
                return 0;
            }

            return 1 << id;
        }
    }

    public static class MutatorRuntime
    {
        public static int Mask { get; private set; }

        public static void Set(int mask)
        {
            Mask = MutatorRules.Sanitize(mask);
        }

        public static void Clear()
        {
            Mask = 0;
        }
    }

    public static class MutatorRules
    {
        public static bool Has(int mask, int id)
        {
            int bit = MutatorCatalog.Bit(id);
            return bit != 0 && (mask & bit) != 0;
        }

        public static bool Incompatible(int left, int right)
        {
            if (left == right)
            {
                return false;
            }

            bool swarmHeavy = (left == MutatorCatalog.Swarm && right == MutatorCatalog.Heavy)
                || (left == MutatorCatalog.Heavy && right == MutatorCatalog.Swarm);
            bool glassQuiet = (left == MutatorCatalog.Glass && right == MutatorCatalog.Quiet)
                || (left == MutatorCatalog.Quiet && right == MutatorCatalog.Glass);
            return swarmHeavy || glassQuiet;
        }

        public static int CountBits(int mask)
        {
            int count = 0;
            for (int id = 0; id < MutatorCatalog.Count; id++)
            {
                if (Has(mask, id))
                {
                    count += 1;
                }
            }

            return count;
        }

        /// <summary>
        /// More than two bits, an unknown bit, or a forbidden pair becomes none.
        /// </summary>
        public static int Sanitize(int mask)
        {
            if (mask <= 0)
            {
                return 0;
            }

            int allowed = (1 << MutatorCatalog.Count) - 1;
            if ((mask & ~allowed) != 0)
            {
                return 0;
            }

            if (CountBits(mask) > MutatorCatalog.MaxPicks)
            {
                return 0;
            }

            for (int left = 0; left < MutatorCatalog.Count; left++)
            {
                if (!Has(mask, left))
                {
                    continue;
                }

                for (int right = left + 1; right < MutatorCatalog.Count; right++)
                {
                    if (Has(mask, right) && Incompatible(left, right))
                    {
                        return 0;
                    }
                }
            }

            return mask;
        }

        public static bool TryToggle(int mask, int id, out int next)
        {
            next = Sanitize(mask);
            int bit = MutatorCatalog.Bit(id);
            if (bit == 0)
            {
                return false;
            }

            if ((next & bit) != 0)
            {
                next = next & ~bit;
                return true;
            }

            if (CountBits(next) >= MutatorCatalog.MaxPicks)
            {
                return false;
            }

            for (int other = 0; other < MutatorCatalog.Count; other++)
            {
                if (Has(next, other) && Incompatible(other, id))
                {
                    return false;
                }
            }

            next = next | bit;
            return true;
        }

        public static int ScorePercent(int mask)
        {
            int clean = Sanitize(mask);
            int percent = 100;
            if (Has(clean, MutatorCatalog.Glass))
            {
                percent = percent * 115 / 100;
            }

            if (Has(clean, MutatorCatalog.Swarm))
            {
                percent = percent * 110 / 100;
            }

            if (Has(clean, MutatorCatalog.Heavy))
            {
                percent = percent * 110 / 100;
            }

            if (Has(clean, MutatorCatalog.Overclock))
            {
                percent = percent * 120 / 100;
            }

            if (Has(clean, MutatorCatalog.LongHaul))
            {
                percent = percent * 110 / 100;
            }

            return Clamp(percent, 100, 160);
        }

        public static int CreditPercent(int mask)
        {
            int clean = Sanitize(mask);
            int percent = 100;
            if (Has(clean, MutatorCatalog.Quiet))
            {
                percent = percent * 125 / 100;
            }

            return Clamp(percent, 100, 140);
        }

        public static int ClampScorePercent(int percent)
        {
            return Clamp(percent, 100, 160);
        }

        public static int ClampCreditPercent(int percent)
        {
            return Clamp(percent, 100, 140);
        }

        public static int ScaleScore(int amount, int mask)
        {
            if (amount <= 0 || Sanitize(mask) == 0)
            {
                return amount;
            }

            int percent = ScorePercent(mask);
            if (percent == 100)
            {
                return amount;
            }

            long scaled = ((long)amount * percent) + 50L;
            return (int)(scaled / 100L);
        }

        public static int ScaleCredits(int amount, int mask)
        {
            if (amount <= 0 || Sanitize(mask) == 0)
            {
                return amount;
            }

            int percent = CreditPercent(mask);
            if (percent == 100)
            {
                return amount;
            }

            long scaled = ((long)amount * percent) + 50L;
            return (int)(scaled / 100L);
        }

        public static int ScaleDamage(int amount, int mask)
        {
            if (amount <= 0 || !Has(Sanitize(mask), MutatorCatalog.Glass))
            {
                return amount;
            }

            return (amount * 3 + 1) / 2;
        }

        public static int ScaleRockHits(int hits, int mask)
        {
            if (hits <= 0 || !Has(Sanitize(mask), MutatorCatalog.Heavy))
            {
                return hits;
            }

            return (hits * 3 + 1) / 2;
        }

        public static int ExtraSplits(int mask)
        {
            return Has(Sanitize(mask), MutatorCatalog.Heavy) ? 1 : 0;
        }

        public static float DropCeiling(int mask)
        {
            if (!Has(Sanitize(mask), MutatorCatalog.Quiet))
            {
                return 0.22f;
            }

            return 0.11f;
        }

        public static float ScaleFireCooldown(float cooldown, int mask)
        {
            if (!Has(Sanitize(mask), MutatorCatalog.Overclock))
            {
                return cooldown;
            }

            return cooldown * 0.80f;
        }

        public static int NextRung(int rung, int mask)
        {
            if (!Has(Sanitize(mask), MutatorCatalog.LongHaul))
            {
                return rung;
            }

            int next = rung + 1;
            if (next > 10)
            {
                next = 10;
            }

            return next;
        }

        public static EnemyKind[] AdjustRoster(EnemyKind[] roster, int mask)
        {
            if (roster == null || !Has(Sanitize(mask), MutatorCatalog.Swarm))
            {
                return roster;
            }

            EnemyKind[] swapped = new EnemyKind[roster.Length + 1];
            for (int index = 0; index < roster.Length; index++)
            {
                EnemyKind kind = roster[index];
                if (kind == EnemyKind.Brute)
                {
                    kind = EnemyKind.Swarmling;
                }

                swapped[index] = kind;
            }

            swapped[roster.Length] = EnemyKind.Swarmling;
            return swapped;
        }

        public static string Factor(int percent)
        {
            if (percent < 0)
            {
                percent = 0;
            }

            int whole = percent / 100;
            int frac = percent % 100;
            string tail = frac < 10 ? "0" + frac.ToString() : frac.ToString();
            return whole.ToString() + "." + tail;
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }

            if (value > max)
            {
                return max;
            }

            return value;
        }
    }
}
