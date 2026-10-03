namespace AsteroidsGoneRogue
{
    public enum ConfirmKind
    {
        None = 0,
        AbortWave = 1,
        NewRun = 2,
    }

    public enum ConfirmAction
    {
        None = 0,
        Open = 1,
        Yes = 2,
        No = 3,
        MoveFocus = 4,
        Blocked = 5,
    }

    public struct ConfirmRequest
    {
        public bool DialogOpen;
        public int Focus;
        public bool Playing;
        public bool RestartScreen;
        public bool ConfirmInPlay;
        public bool ConfirmNewRun;
        public bool SettingsOpen;
        public bool CreditsVisible;
        public bool Escape;
        public bool Start;
        public bool Cancel;
        public bool Submit;
        public bool AbortClick;
        public bool NewRunClick;
        public bool Scrim;
        public int FocusDelta;
    }

    /// <summary>
    /// Pure confirm router. An open dialog consumes every input first so Esc,
    /// Start, B, and A cannot reach abort, New Run, or hangar navigation.
    /// Settings and credits cover the screen next: they return None so the
    /// panel or credits overlay can consume Esc, Start, B, and A. No New Run,
    /// confirm dialog, or abort starts underneath them.
    /// Default focus is No. B, Esc, Start, and the scrim are No.
    /// A confirms only when Yes is focused; A on No cancels.
    /// </summary>
    public static class ConfirmDialogRouter
    {
        public const int FocusNo = 0;
        public const int FocusYes = 1;

        public static int DefaultFocus()
        {
            return FocusNo;
        }

        /// <summary>
        /// Yes sits on the left, No on the right. Left selects Yes, right selects No.
        /// </summary>
        public static int MoveFocus(int focus, int delta)
        {
            if (delta < 0)
            {
                return FocusYes;
            }

            if (delta > 0)
            {
                return FocusNo;
            }

            if (focus == FocusYes)
            {
                return FocusYes;
            }

            return FocusNo;
        }

        public static ConfirmAction Route(ConfirmRequest request)
        {
            if (request.DialogOpen)
            {
                if (request.Escape || request.Start || request.Cancel || request.Scrim)
                {
                    return ConfirmAction.No;
                }

                if (request.Submit)
                {
                    if (request.Focus == FocusYes)
                    {
                        return ConfirmAction.Yes;
                    }

                    return ConfirmAction.No;
                }

                if (request.FocusDelta != 0)
                {
                    return ConfirmAction.MoveFocus;
                }

                return ConfirmAction.Blocked;
            }

            if (request.SettingsOpen || request.CreditsVisible)
            {
                return ConfirmAction.None;
            }

            bool abort = request.Playing && (request.Escape || request.Start || request.AbortClick);
            if (abort)
            {
                if (request.ConfirmInPlay)
                {
                    return ConfirmAction.Open;
                }

                return ConfirmAction.Yes;
            }

            bool newRun = request.RestartScreen && (request.NewRunClick || request.Start);
            if (newRun)
            {
                if (request.ConfirmNewRun)
                {
                    return ConfirmAction.Open;
                }

                return ConfirmAction.Yes;
            }

            return ConfirmAction.None;
        }
    }

    /// <summary>
    /// Pause clock for the in-wave abort confirm. Scale is 0 only while that
    /// dialog is open on a live wave. No, Yes, or the wave ending underneath
    /// all restore 1 so a pause cannot leak.
    /// </summary>
    public static class ConfirmPause
    {
        public const float LiveScale = 1f;
        public const float HeldScale = 0f;

        public static float TimeScale(bool abortDialogOpen, bool waveLive)
        {
            if (abortDialogOpen && waveLive)
            {
                return HeldScale;
            }

            return LiveScale;
        }

        public static bool DismissAbort(bool abortDialogOpen, bool waveLive)
        {
            return abortDialogOpen && !waveLive;
        }

        public static bool SilenceShip(bool abortDialogOpen, bool waveLive)
        {
            return abortDialogOpen && waveLive;
        }

        public static bool RestoreShipInput(bool wasSilenced, bool silenceNow, bool waveLive)
        {
            return wasSilenced && !silenceNow && waveLive;
        }
    }

    /// <summary>
    /// Screen anchors for the confirm card. Yes is left, No is right, and the
    /// card sits inside the view from 1280x800 through 3440x1440.
    /// </summary>
    public static class ConfirmDialogLayout
    {
        public const float PanelMinX = 0.32f;
        public const float PanelMinY = 0.36f;
        public const float PanelMaxX = 0.68f;
        public const float PanelMaxY = 0.64f;
        public const float YesMinX = 0.08f;
        public const float YesMinY = 0.12f;
        public const float YesMaxX = 0.46f;
        public const float YesMaxY = 0.36f;
        public const float NoMinX = 0.54f;
        public const float NoMinY = 0.12f;
        public const float NoMaxX = 0.92f;
        public const float NoMaxY = 0.36f;
    }
}
