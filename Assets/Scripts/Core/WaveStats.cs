namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Per-wave kills, credits, time, damage, and combo. Unity-free so the
    /// summary numbers can be checked without the editor. Reset at wave start.
    /// </summary>
    public sealed class WaveStats
    {
        public int Kills { get; private set; }
        public int Credits { get; private set; }
        public float Seconds { get; private set; }
        public int HullDamage { get; private set; }
        public int ShieldDamage { get; private set; }
        public int Combo { get; private set; }
        public int BestCombo { get; private set; }
        public bool Frozen { get; private set; }

        public void Reset()
        {
            Kills = 0;
            Credits = 0;
            Seconds = 0f;
            HullDamage = 0;
            ShieldDamage = 0;
            Combo = 0;
            BestCombo = 0;
            Frozen = false;
        }

        public void NoteKill()
        {
            if (Frozen)
            {
                return;
            }

            if (Kills < 999999)
            {
                Kills += 1;
            }

            if (Combo < 999999)
            {
                Combo += 1;
            }

            if (Combo > BestCombo)
            {
                BestCombo = Combo;
            }
        }

        public void BreakCombo()
        {
            Combo = 0;
        }

        public void AddTime(float seconds)
        {
            if (Frozen || seconds <= 0f)
            {
                return;
            }

            Seconds += seconds;
            if (Seconds > 5999f)
            {
                Seconds = 5999f;
            }
        }

        public void NoteDamage(int shieldLost, int hullLost)
        {
            if (Frozen)
            {
                return;
            }

            if (shieldLost > 0)
            {
                ShieldDamage += shieldLost;
            }

            if (hullLost > 0)
            {
                HullDamage += hullLost;
            }
        }

        public void SetCredits(int credits)
        {
            if (credits < 0)
            {
                credits = 0;
            }

            Credits = credits;
        }

        public void Freeze()
        {
            Frozen = true;
        }

        public void Present(int kills, int credits, float seconds, int hullDamage, int shieldDamage, int bestCombo)
        {
            Reset();
            if (kills < 0)
            {
                kills = 0;
            }

            Kills = kills;
            SetCredits(credits);
            if (seconds < 0f)
            {
                seconds = 0f;
            }

            Seconds = seconds > 5999f ? 5999f : seconds;
            HullDamage = hullDamage < 0 ? 0 : hullDamage;
            ShieldDamage = shieldDamage < 0 ? 0 : shieldDamage;
            BestCombo = bestCombo < 0 ? 0 : bestCombo;
            Combo = BestCombo;
            Freeze();
        }

        public static string FormatTime(float seconds)
        {
            if (seconds < 0f)
            {
                seconds = 0f;
            }

            int total = (int)seconds;
            if (total > 5999)
            {
                total = 5999;
            }

            int mins = total / 60;
            int secs = total % 60;
            string secText = secs < 10 ? "0" + secs.ToString() : secs.ToString();
            return mins.ToString() + ":" + secText;
        }

        public static int VisualCredits(int value)
        {
            if (value < 0)
            {
                return 0;
            }

            if (value > 99999)
            {
                return 99999;
            }

            return value;
        }

        public static string SignedCredits(int value)
        {
            return "+" + VisualCredits(value).ToString();
        }

        public static int ClampDamage(int value)
        {
            if (value < 0)
            {
                return 0;
            }

            return value;
        }

        /// <summary>
        /// Caption suffix when a legacy or mutator credit multiplier is not 1.
        /// Daily drops the legacy factor. 120 → " ×1,2". 100 → empty.
        /// </summary>
        public static string CreditSuffix(bool daily, int legacyPercent, int mutatorPercent)
        {
            int legacy = legacyPercent;
            if (daily || legacy < 1)
            {
                legacy = 100;
            }

            int mutator = mutatorPercent < 1 ? 100 : mutatorPercent;
            int combined = (int)(((long)legacy * mutator + 50L) / 100L);
            if (combined == 100)
            {
                return string.Empty;
            }

            return " \u00d7" + FormatFactor(combined);
        }

        public static string FormatFactor(int percent)
        {
            if (percent < 0)
            {
                percent = 0;
            }

            int whole = percent / 100;
            int frac = percent % 100;
            if (frac == 0)
            {
                return whole.ToString();
            }

            if (frac % 10 == 0)
            {
                return whole.ToString() + "," + (frac / 10).ToString();
            }

            string fracText = frac < 10 ? "0" + frac.ToString() : frac.ToString();
            return whole.ToString() + "," + fracText;
        }

        public static bool HullDanger(int hull, int maxHull)
        {
            if (maxHull < 1)
            {
                maxHull = 1;
            }

            if (hull < 0)
            {
                hull = 0;
            }

            int quarter = (int)System.Math.Floor(0.25 * maxHull);
            int limit = quarter < 1 ? 1 : quarter;
            return hull <= limit;
        }

        public static WaveMark Judge(float seconds, int combo, float bestSeconds, int bestCombo)
        {
            WaveMark mark = new WaveMark();
            if (seconds > 0f && (bestSeconds <= 0f || seconds < bestSeconds))
            {
                mark.TimeBest = true;
            }

            if (combo > 0 && combo > bestCombo)
            {
                mark.ComboBest = true;
            }

            return mark;
        }

        /// <summary>
        /// Repair fills the hull and spends the cost. A short purse or a full
        /// bar leaves the vitals unchanged, matching the hangar shop.
        /// </summary>
        public static HangarVitals AfterRepair(HangarVitals before, int cost)
        {
            HangarVitals next = before;
            if (cost < 0)
            {
                cost = 0;
            }

            if (before.Credits < cost || before.Hull <= 0 || before.Hull >= before.MaxHull)
            {
                return next;
            }

            next.Credits = before.Credits - cost;
            next.Hull = before.MaxHull;
            return next;
        }

        public static HangarVitals AfterRefill(HangarVitals before, int cost)
        {
            HangarVitals next = before;
            if (cost < 0)
            {
                cost = 0;
            }

            if (before.Credits < cost || before.MaxShield <= 0 || before.Shield >= before.MaxShield)
            {
                return next;
            }

            next.Credits = before.Credits - cost;
            next.Shield = before.MaxShield;
            return next;
        }

        public static HangarVitals AfterLife(HangarVitals before, int cost, int cap)
        {
            HangarVitals next = before;
            if (cost < 0)
            {
                cost = 0;
            }

            if (cap < before.MaxLives)
            {
                cap = before.MaxLives;
            }

            if (before.Credits < cost || before.Lives >= cap)
            {
                return next;
            }

            next.Credits = before.Credits - cost;
            next.MaxLives = cap;
            next.Lives = before.Lives + 1;
            if (next.Lives > next.MaxLives)
            {
                next.Lives = next.MaxLives;
            }

            return next;
        }
    }

    public struct WaveMark
    {
        public bool TimeBest;
        public bool ComboBest;

        public bool Badge
        {
            get { return TimeBest || ComboBest; }
        }
    }

    public struct HangarVitals
    {
        public int Credits;
        public int Lives;
        public int MaxLives;
        public int Hull;
        public int MaxHull;
        public int Shield;
        public int MaxShield;
    }
}
