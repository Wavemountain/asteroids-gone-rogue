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

        public static string Wait(string remaining)
        {
            return Loc.Tf("daily.wait", "\u2022 Next daily in {0}.", remaining);
        }

        public static string LockedButton(string remaining)
        {
            return Loc.Tf("daily.wait.button", "Daily · {0}", remaining);
        }

        public static string ResetLoadout()
        {
            return Loc.T("daily.reset_loadout", "Daily resets this loadout.");
        }
    }

    /// <summary>
    /// Daily wipes the hangar loadout. Warn when that wipe would drop
    /// something the player already has. A fresh rank-0 hangar does not.
    /// </summary>
    public static class DailyResetWarning
    {
        public static bool Needed(
            bool continueOffer,
            bool leavingRun,
            int wave,
            int score,
            int credits,
            bool purchased,
            int shield,
            int doctrine)
        {
            if (continueOffer)
            {
                return true;
            }

            if (leavingRun)
            {
                return true;
            }

            if (wave > 1)
            {
                return true;
            }

            if (score > 0)
            {
                return true;
            }

            if (credits > 0)
            {
                return true;
            }

            if (purchased)
            {
                return true;
            }

            if (shield > 0)
            {
                return true;
            }

            if (doctrine != 0)
            {
                return true;
            }

            return false;
        }
    }
}
