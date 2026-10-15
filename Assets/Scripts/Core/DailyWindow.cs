using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// UTC daily window. The seed is the UTC date. Once that date is played,
    /// the next daily waits until the following UTC midnight. Rules are pure;
    /// <see cref="DailyPlayedStore"/> is the only PlayerPrefs edge.
    /// </summary>
    public static class DailyWindow
    {
        public const int OffsetMin = -840;
        public const int OffsetMax = 840;
        public const int DaySeconds = 86400;

        public static bool Allows(int playedDate, int today)
        {
            if (!DailySeed.DateLooksValid(today))
            {
                return false;
            }

            if (!DailySeed.DateLooksValid(playedDate))
            {
                return true;
            }

            return playedDate != today;
        }

        /// <summary>
        /// Seconds until the next UTC midnight. 00:00:00 is the start of the
        /// day, so a full day remains. 23:59:59 leaves one second.
        /// </summary>
        public static int SecondsUntilNextMidnight(int hour, int minute, int second)
        {
            if (hour < 0 || hour > 23 || minute < 0 || minute > 59 || second < 0 || second > 59)
            {
                return 0;
            }

            int elapsed = (hour * 3600) + (minute * 60) + second;
            int left = DaySeconds - elapsed;
            if (left < 1)
            {
                return DaySeconds;
            }

            return left;
        }

        public static string RemainingText(int seconds)
        {
            if (seconds < 0)
            {
                seconds = 0;
            }

            if (seconds > DaySeconds)
            {
                seconds = DaySeconds;
            }

            int hours = seconds / 3600;
            int minutes = (seconds % 3600) / 60;
            int secs = seconds % 60;
            if (hours > 0)
            {
                return hours.ToString() + "h " + minutes.ToString("00") + "m";
            }

            if (minutes > 0)
            {
                return minutes.ToString() + "m " + secs.ToString("00") + "s";
            }

            return secs.ToString() + "s";
        }

        /// <summary>
        /// Convert a civil clock to UTC. <paramref name="offsetMinutes"/> is
        /// minutes east of UTC (Sweden winter +60, summer +120). The machine
        /// timezone is not consulted.
        /// </summary>
        public static bool TryUtc(
            int year,
            int month,
            int day,
            int hour,
            int minute,
            int second,
            int offsetMinutes,
            out int utcYear,
            out int utcMonth,
            out int utcDay,
            out int utcHour,
            out int utcMinute,
            out int utcSecond)
        {
            utcYear = 0;
            utcMonth = 0;
            utcDay = 0;
            utcHour = 0;
            utcMinute = 0;
            utcSecond = 0;
            if (offsetMinutes < OffsetMin || offsetMinutes > OffsetMax)
            {
                return false;
            }

            return TryShift(
                year,
                month,
                day,
                hour,
                minute,
                second,
                -offsetMinutes,
                out utcYear,
                out utcMonth,
                out utcDay,
                out utcHour,
                out utcMinute,
                out utcSecond);
        }

        public static bool TryShift(
            int year,
            int month,
            int day,
            int hour,
            int minute,
            int second,
            int deltaMinutes,
            out int outYear,
            out int outMonth,
            out int outDay,
            out int outHour,
            out int outMinute,
            out int outSecond)
        {
            outYear = 0;
            outMonth = 0;
            outDay = 0;
            outHour = 0;
            outMinute = 0;
            outSecond = 0;
            if (!DatePartsValid(year, month, day))
            {
                return false;
            }

            if (hour < 0 || hour > 23 || minute < 0 || minute > 59 || second < 0 || second > 59)
            {
                return false;
            }

            long total = ((long)DayNumber(year, month, day) * 1440L) + (hour * 60L) + minute;
            total += deltaMinutes;
            int newDay = (int)(total / 1440L);
            int minuteOfDay = (int)(total % 1440L);
            if (minuteOfDay < 0)
            {
                minuteOfDay += 1440;
                newDay -= 1;
            }

            int shiftedYear;
            int shiftedMonth;
            int shiftedDay;
            if (!FromDayNumber(newDay, out shiftedYear, out shiftedMonth, out shiftedDay))
            {
                return false;
            }

            outYear = shiftedYear;
            outMonth = shiftedMonth;
            outDay = shiftedDay;
            outHour = minuteOfDay / 60;
            outMinute = minuteOfDay % 60;
            outSecond = second;
            return true;
        }

        private static bool DatePartsValid(int year, int month, int day)
        {
            if (year < 1 || year > 9999 || month < 1 || month > 12 || day < 1)
            {
                return false;
            }

            return day <= DaysInMonth(year, month);
        }

        private static int DayNumber(int year, int month, int day)
        {
            int days = 0;
            if (year >= 2000)
            {
                for (int cursor = 2000; cursor < year; cursor++)
                {
                    days += Leap(cursor) ? 366 : 365;
                }
            }
            else
            {
                for (int cursor = year; cursor < 2000; cursor++)
                {
                    days -= Leap(cursor) ? 366 : 365;
                }
            }

            for (int cursor = 1; cursor < month; cursor++)
            {
                days += DaysInMonth(year, cursor);
            }

            days += day - 1;
            return days;
        }

        private static bool FromDayNumber(int dayNumber, out int year, out int month, out int day)
        {
            year = 2000;
            month = 1;
            day = 1;
            int remaining = dayNumber;
            if (remaining >= 0)
            {
                while (true)
                {
                    int yearDays = Leap(year) ? 366 : 365;
                    if (remaining < yearDays)
                    {
                        break;
                    }

                    remaining -= yearDays;
                    year += 1;
                    if (year > 9999)
                    {
                        return false;
                    }
                }
            }
            else
            {
                while (remaining < 0)
                {
                    year -= 1;
                    if (year < 1)
                    {
                        return false;
                    }

                    remaining += Leap(year) ? 366 : 365;
                }
            }

            month = 1;
            while (month <= 12)
            {
                int dim = DaysInMonth(year, month);
                if (remaining < dim)
                {
                    break;
                }

                remaining -= dim;
                month += 1;
            }

            if (month > 12)
            {
                return false;
            }

            day = remaining + 1;
            return DatePartsValid(year, month, day);
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
    /// Packed UTC date of the daily that was actually started. Missing key
    /// means none played, so older profiles keep today's daily available.
    /// </summary>
    public static class DailyPlayedStore
    {
        public const string Key = "agr.daily.played";

        public static int Read()
        {
            int packed = PlayerPrefs.GetInt(Key, 0);
            if (!DailySeed.DateLooksValid(packed))
            {
                return 0;
            }

            return packed;
        }

        public static void Commit(int packedDate)
        {
            if (!DailySeed.DateLooksValid(packedDate))
            {
                return;
            }

            if (Read() == packedDate)
            {
                return;
            }

            PlayerPrefs.SetInt(Key, packedDate);
            PlayerPrefs.Save();
        }
    }
}
