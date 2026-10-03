namespace AsteroidsGoneRogue
{
    public enum SettingsRoute
    {
        None = 0,
        Open = 1,
        CloseSave = 2,
        Activate = 3,
        Nudge = 4,
        Move = 5,
        CreditsClose = 6,
        PlayAbort = 7,
        HangarEscape = 8,
        HangarStart = 9,
        HangarBack = 10,
    }

    public struct SettingsInputFlags
    {
        public bool Open;
        public bool Playing;
        public bool CreditsVisible;
        public bool Escape;
        public bool Start;
        public bool Cancel;
        public bool Submit;
        public bool F1;
        public bool Select;
        public bool Gear;
        public bool Scrim;
        public int NavX;
        public int NavY;
    }

    /// <summary>
    /// Pure hangar shortcut router. Settings consumes Esc, Start, B, and Submit
    /// before credits, play-abort, and hangar handlers. A wave never opens it.
    /// </summary>
    public static class SettingsInputRouter
    {
        public static int ScreenStepY(float x, float y, float flick)
        {
            float ax = x < 0f ? -x : x;
            float ay = y < 0f ? -y : y;
            if (ax < flick && ay < flick)
            {
                return 0;
            }

            if (ay > ax)
            {
                return y > 0f ? 1 : -1;
            }

            return 0;
        }

        public static bool BlocksHangarPad(SettingsInputFlags flags)
        {
            return flags.Open && !flags.Playing;
        }

        public static SettingsRoute Route(SettingsInputFlags flags)
        {
            if (flags.Open)
            {
                if (flags.Escape || flags.Start || flags.Cancel || flags.Select || flags.F1 || flags.Scrim)
                {
                    return SettingsRoute.CloseSave;
                }

                if (flags.Playing)
                {
                    return SettingsRoute.None;
                }

                if (flags.Submit || flags.Gear)
                {
                    return SettingsRoute.Activate;
                }

                if (flags.NavY != 0)
                {
                    return SettingsRoute.Move;
                }

                if (flags.NavX != 0)
                {
                    return SettingsRoute.Nudge;
                }

                return SettingsRoute.None;
            }

            if (flags.CreditsVisible)
            {
                if (flags.Escape || flags.Cancel)
                {
                    return SettingsRoute.CreditsClose;
                }

                return SettingsRoute.None;
            }

            if (flags.Playing && (flags.Escape || flags.Start))
            {
                return SettingsRoute.PlayAbort;
            }

            if (!flags.Playing && !flags.CreditsVisible)
            {
                if (flags.F1 || flags.Select || flags.Gear)
                {
                    return SettingsRoute.Open;
                }

                if (flags.Escape)
                {
                    return SettingsRoute.HangarEscape;
                }

                if (flags.Start)
                {
                    return SettingsRoute.HangarStart;
                }

                if (flags.Cancel)
                {
                    return SettingsRoute.HangarBack;
                }
            }

            return SettingsRoute.None;
        }
    }
}
