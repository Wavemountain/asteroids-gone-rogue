namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Capture shot advance. A HUD failure is recorded and the next scene still
    /// runs. Only a fatal error (missing session, empty frame) stays on the
    /// current shot. Unity-free.
    /// </summary>
    public static class CaptureContinue
    {
        public static int NextShot(int shot, bool hudFailed, bool fatal)
        {
            if (fatal)
            {
                return shot;
            }

            if (shot < 0)
            {
                shot = 0;
            }

            return shot + 1;
        }

        public static int ExitCode(bool fatal, bool hudFailed)
        {
            return ExitCode(fatal, hudFailed, false);
        }

        public static int ExitCode(bool fatal, bool hudFailed, bool layerFailed)
        {
            if (fatal || hudFailed || layerFailed)
            {
                return 1;
            }

            return 0;
        }

        public static bool SelfCheck()
        {
            if (NextShot(2, true, false) != 3)
            {
                return false;
            }

            if (NextShot(2, true, true) != 2)
            {
                return false;
            }

            if (NextShot(0, false, false) != 1)
            {
                return false;
            }

            if (ExitCode(false, true) != 1 || ExitCode(true, false) != 1)
            {
                return false;
            }

            if (ExitCode(false, false, true) != 1)
            {
                return false;
            }

            return ExitCode(false, false) == 0 && ExitCode(false, false, false) == 0;
        }
    }
}
