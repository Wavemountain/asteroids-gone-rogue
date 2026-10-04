using System;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// UTC-date seed and the small LCG used when a daily run is active.
    /// A normal run keeps seed 0 and does not touch this stream.
    /// </summary>
    public static class DailySeed
    {
        public static int ForDate(int year, int month, int day)
        {
            if (!DateLooksValid(year, month, day))
            {
                return 0;
            }

            int mixed = Mix(Mix(Mix(216613626, year), month), day);
            mixed = mixed % 1000000000;
            if (mixed < 1)
            {
                mixed = 1;
            }

            return mixed;
        }

        public static int PackDate(int year, int month, int day)
        {
            if (!DateLooksValid(year, month, day))
            {
                return 0;
            }

            return (year * 10000) + (month * 100) + day;
        }

        public static bool DateLooksValid(int packed)
        {
            if (packed < 20000101 || packed > 21991231)
            {
                return false;
            }

            int year = packed / 10000;
            int month = (packed / 100) % 100;
            int day = packed % 100;
            return DateLooksValid(year, month, day);
        }

        public static bool DateLooksValid(int year, int month, int day)
        {
            if (year < 2000 || year > 2199 || month < 1 || month > 12 || day < 1)
            {
                return false;
            }

            return day <= DaysInMonth(year, month);
        }

        public static int Mix(int seed, int salt)
        {
            uint hash = (uint)seed;
            if (hash == 0u)
            {
                hash = 1u;
            }

            hash ^= (uint)salt;
            hash *= 16777619u;
            int mixed = (int)(hash & 0x7fffffffu);
            if (mixed == 0)
            {
                mixed = 1;
            }

            return mixed;
        }

        public static void UtcToday(out int year, out int month, out int day)
        {
            DateTime utc = DateTime.UtcNow;
            year = utc.Year;
            month = utc.Month;
            day = utc.Day;
        }

        public static string DateText(int packed)
        {
            if (!DateLooksValid(packed))
            {
                return string.Empty;
            }

            int year = packed / 10000;
            int month = (packed / 100) % 100;
            int day = packed % 100;
            return year.ToString("0000") + "-" + month.ToString("00") + "-" + day.ToString("00");
        }

        private static int DaysInMonth(int year, int month)
        {
            switch (month)
            {
                case 2:
                    return Leap(year) ? 29 : 28;
                case 4:
                case 6:
                case 9:
                case 11:
                    return 30;
                default:
                    return 31;
            }
        }

        private static bool Leap(int year)
        {
            if (year % 400 == 0)
            {
                return true;
            }

            if (year % 100 == 0)
            {
                return false;
            }

            return year % 4 == 0;
        }
    }

    /// <summary>
    /// Deterministic 15-bit draws. The same seed replays the same sequence.
    /// </summary>
    public sealed class SeedStream
    {
        private uint _state;

        public SeedStream(int seed)
        {
            uint bits = (uint)seed;
            _state = bits == 0u ? 1u : bits;
        }

        public int NextInt()
        {
            _state = (_state * 1103515245u) + 12345u;
            return (int)((_state >> 16) & 32767u);
        }

        public float Value()
        {
            return NextInt() / 32768f;
        }

        public int Range(int minInclusive, int maxExclusive)
        {
            int span = maxExclusive - minInclusive;
            if (span <= 1)
            {
                return minInclusive;
            }

            return minInclusive + (NextInt() % span);
        }
    }
}
