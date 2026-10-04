namespace AsteroidsGoneRogue
{
    /// <summary>
    /// EN fallbacks for the daily-run chooser and the seed stamp.
    /// Swedish lives in <see cref="Loc"/>.
    /// </summary>
    public static class DailyCopy
    {
        public static string Title()
        {
            return Loc.T("daily.title", "Choose run");
        }

        public static string Normal()
        {
            return Loc.T("daily.normal", "Normal run");
        }

        public static string Daily()
        {
            return Loc.T("daily.daily", "Daily Run");
        }

        public static string Cancel()
        {
            return Loc.T("daily.cancel", "Back");
        }

        public static string Hint()
        {
            string line = Loc.T("daily.hint", "{confirm} start  ·  {cancel} back");
            return PromptText.Flatten(line, InputSchemeDriver.Current);
        }

        public static string Blurb(int todayScore, int todayWave)
        {
            string line = Loc.T("daily.blurb", "\u2022 One seed for this UTC day.");
            if (todayScore > 0 || todayWave > 0)
            {
                line += "\n" + Loc.Tf("daily.board", "Today {0}  ·  wave {1}", todayScore, todayWave);
            }

            return line;
        }

        public static string Stamp(int date, int seed)
        {
            if (date == 0 || seed == 0)
            {
                return string.Empty;
            }

            string day = DailySeed.DateText(date);
            if (day.Length == 0)
            {
                return string.Empty;
            }

            return Loc.Tf("daily.stamp", "Daily {0}  ·  seed {1}", day, seed);
        }
    }
}
