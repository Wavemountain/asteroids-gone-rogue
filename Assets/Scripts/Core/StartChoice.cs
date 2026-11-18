namespace AsteroidsGoneRogue
{
    public enum StartFocus
    {
        Continue = 0,
        NewRun = 1,
    }

    public enum StartSkip
    {
        /// <summary>No save. Enter today's new-run boot. Do not delete anything.</summary>
        NewRun = 0,

        /// <summary>A save exists. Keep today's hangar boot. Never delete the save.</summary>
        KeepSave = 1,
    }

    public enum StartAction
    {
        None = 0,
        MoveFocus = 1,
        Blocked = 2,
        Continue = 3,
        AskOverwrite = 4,
        BeginNewRun = 5,
        OverwriteYes = 6,
        OverwriteNo = 7,
    }

    public struct StartChoiceView
    {
        public bool ContinueEnabled;
        public bool ConfirmOverwrite;
        public int Focus;
        public int WorldNumber;
        public int WaveIndex;
    }

    public struct StartRequest
    {
        public bool PanelOpen;
        public bool ConfirmOpen;
        public int Focus;
        public int ConfirmFocus;
        public bool ContinueEnabled;
        public bool HasSave;
        public bool Escape;
        public bool Start;
        public bool Cancel;
        public bool Submit;
        public bool ContinueClick;
        public bool NewClick;
        public bool YesClick;
        public bool NoClick;
        public bool Scrim;
        public int FocusDelta;
    }

    /// <summary>
    /// Boot choice before the hangar. Unity-free.
    /// Continue is on only when a save exists. With no save it stays visible
    /// and unselectable, and focus starts on Start new. Start new always asks
    /// before overwriting that save. The hangar New Run confirm setting does
    /// not apply here.
    /// </summary>
    public static class StartChoice
    {
        public const int FocusContinue = 0;
        public const int FocusNew = 1;
        public const int FocusNo = 0;
        public const int FocusYes = 1;

        /// <summary>Command-line skip. LongRun can pass this later.</summary>
        public const string SkipArg = "-skipStartPanel";

        /// <summary>
        /// Editor test only. When the environment variable is "1", the batch
        /// start-choice test shows the panel instead of auto-skipping.
        /// </summary>
        internal const string ForceEnv = "AGR_STARTCHOICE_FORCE";

        internal static bool ForcePanel;

        /// <summary>
        /// Explicit skip. LongRun will set this (or pass <see cref="SkipArg"/>).
        /// When the panel is skipped: no save enters the normal new run; a save
        /// keeps today's boot and is never deleted from this path.
        /// </summary>
        public static bool SkipPanel;

        /// <summary>
        /// Set by AgrCapture while a capture is in play mode. Batch mode skips
        /// on its own; this covers an editor capture that is already running.
        /// </summary>
        public static bool CaptureRunning;

        public static StartChoiceView Decide(bool hasSave, int worldNumber, int waveIndex)
        {
            StartChoiceView view = new StartChoiceView();
            int world = worldNumber < 1 ? 1 : worldNumber;
            int wave = waveIndex < 1 ? 1 : waveIndex;
            view.ContinueEnabled = hasSave;
            view.ConfirmOverwrite = hasSave;
            view.Focus = hasSave ? FocusContinue : FocusNew;
            view.WorldNumber = world;
            view.WaveIndex = wave;
            return view;
        }

        public static bool ShouldSkip(bool batchMode, bool captureRunning)
        {
            if (ForcePanel)
            {
                return false;
            }

            return batchMode || captureRunning || SkipPanel;
        }

        public static StartSkip WhenSkipped(bool hasSave)
        {
            if (hasSave)
            {
                return StartSkip.KeepSave;
            }

            return StartSkip.NewRun;
        }

        public static bool ReadsSkipArg(string[] args)
        {
            if (args == null)
            {
                return false;
            }

            for (int index = 0; index < args.Length; index++)
            {
                if (args[index] == SkipArg)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// AgrCapture's existing -executeMethod launch. Not a new flag.
        /// </summary>
        public static bool CaptureCommand(string[] args)
        {
            if (args == null)
            {
                return false;
            }

            for (int index = 0; index < args.Length; index++)
            {
                if (args[index] != "-executeMethod" || index + 1 >= args.Length)
                {
                    continue;
                }

                string method = args[index + 1];
                if (method != null && method.IndexOf("AgrCapture", System.StringComparison.Ordinal) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static int MoveChoice(int focus, int delta, bool continueEnabled)
        {
            if (!continueEnabled)
            {
                return FocusNew;
            }

            if (delta < 0)
            {
                return FocusContinue;
            }

            if (delta > 0)
            {
                return FocusNew;
            }

            if (focus == FocusContinue)
            {
                return FocusContinue;
            }

            return FocusNew;
        }

        /// <summary>
        /// Yes is left, No is right. Default focus is No.
        /// </summary>
        public static int MoveOverwrite(int focus, int delta)
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

        public static StartAction Route(StartRequest request)
        {
            if (!request.PanelOpen)
            {
                return StartAction.None;
            }

            if (request.ConfirmOpen)
            {
                if (request.Escape || request.Start || request.Cancel || request.Scrim || request.NoClick)
                {
                    return StartAction.OverwriteNo;
                }

                if (request.YesClick)
                {
                    return StartAction.OverwriteYes;
                }

                if (request.Submit)
                {
                    if (request.ConfirmFocus == FocusYes)
                    {
                        return StartAction.OverwriteYes;
                    }

                    return StartAction.OverwriteNo;
                }

                if (request.FocusDelta != 0)
                {
                    return StartAction.MoveFocus;
                }

                return StartAction.Blocked;
            }

            if (request.ContinueClick)
            {
                if (!request.ContinueEnabled)
                {
                    return StartAction.Blocked;
                }

                return StartAction.Continue;
            }

            if (request.NewClick || (request.Submit && (request.Focus == FocusNew || !request.ContinueEnabled)))
            {
                if (request.HasSave)
                {
                    return StartAction.AskOverwrite;
                }

                return StartAction.BeginNewRun;
            }

            if (request.Submit && request.Focus == FocusContinue && request.ContinueEnabled)
            {
                return StartAction.Continue;
            }

            if (request.FocusDelta != 0)
            {
                return StartAction.MoveFocus;
            }

            return StartAction.Blocked;
        }

        public static bool SelfCheck()
        {
            StartChoiceView empty = Decide(false, 0, 0);
            if (empty.ContinueEnabled || empty.ConfirmOverwrite || empty.Focus != FocusNew)
            {
                return false;
            }

            if (empty.WorldNumber != 1 || empty.WaveIndex != 1)
            {
                return false;
            }

            StartChoiceView saved = Decide(true, 2, 8);
            if (!saved.ContinueEnabled || !saved.ConfirmOverwrite || saved.Focus != FocusContinue)
            {
                return false;
            }

            if (saved.WorldNumber != 2 || saved.WaveIndex != 8)
            {
                return false;
            }

            if (MoveChoice(FocusContinue, 1, false) != FocusNew)
            {
                return false;
            }

            if (MoveChoice(FocusNew, -1, true) != FocusContinue || MoveChoice(FocusContinue, 1, true) != FocusNew)
            {
                return false;
            }

            if (MoveOverwrite(FocusNo, 0) != FocusNo || MoveOverwrite(FocusNo, -1) != FocusYes)
            {
                return false;
            }

            if (MoveOverwrite(FocusYes, 1) != FocusNo)
            {
                return false;
            }

            StartRequest choice = new StartRequest();
            choice.PanelOpen = true;
            choice.ContinueEnabled = false;
            choice.HasSave = false;
            choice.Focus = FocusNew;
            choice.ContinueClick = true;
            if (Route(choice) != StartAction.Blocked)
            {
                return false;
            }

            choice.ContinueClick = false;
            choice.NewClick = true;
            if (Route(choice) != StartAction.BeginNewRun)
            {
                return false;
            }

            choice.NewClick = false;
            choice.HasSave = true;
            choice.ContinueEnabled = true;
            choice.Focus = FocusContinue;
            choice.Submit = true;
            if (Route(choice) != StartAction.Continue)
            {
                return false;
            }

            choice.Submit = false;
            choice.Focus = FocusNew;
            choice.NewClick = true;
            if (Route(choice) != StartAction.AskOverwrite)
            {
                return false;
            }

            StartRequest confirm = new StartRequest();
            confirm.PanelOpen = true;
            confirm.ConfirmOpen = true;
            confirm.HasSave = true;
            confirm.ConfirmFocus = FocusNo;
            confirm.Submit = true;
            if (Route(confirm) != StartAction.OverwriteNo)
            {
                return false;
            }

            confirm.Submit = false;
            confirm.Escape = true;
            if (Route(confirm) != StartAction.OverwriteNo)
            {
                return false;
            }

            confirm.Escape = false;
            confirm.Scrim = true;
            if (Route(confirm) != StartAction.OverwriteNo)
            {
                return false;
            }

            confirm.Scrim = false;
            confirm.NoClick = true;
            if (Route(confirm) != StartAction.OverwriteNo)
            {
                return false;
            }

            confirm.NoClick = false;
            confirm.ConfirmFocus = FocusYes;
            confirm.Submit = true;
            if (Route(confirm) != StartAction.OverwriteYes)
            {
                return false;
            }

            confirm.Submit = false;
            confirm.YesClick = true;
            if (Route(confirm) != StartAction.OverwriteYes)
            {
                return false;
            }

            if (!ShouldSkip(true, false) || !ShouldSkip(false, true))
            {
                return false;
            }

            bool prior = SkipPanel;
            SkipPanel = false;
            bool clear = ShouldSkip(false, false);
            SkipPanel = true;
            bool flagged = ShouldSkip(false, false);
            SkipPanel = prior;
            if (clear || !flagged)
            {
                return false;
            }

            if (WhenSkipped(false) != StartSkip.NewRun || WhenSkipped(true) != StartSkip.KeepSave)
            {
                return false;
            }

            if (!ReadsSkipArg(new string[] { "-batchmode", SkipArg }))
            {
                return false;
            }

            if (ReadsSkipArg(new string[] { "-captureOut" }) || ReadsSkipArg(null))
            {
                return false;
            }

            if (!CaptureCommand(new string[] { "-executeMethod", "AsteroidsGoneRogue.EditorTools.AgrCapture.Run" }))
            {
                return false;
            }

            if (CaptureCommand(new string[] { "-executeMethod", "Other.Run" }))
            {
                return false;
            }

            if (RectsOverlap(
                StartChoiceLayout.ContinueMinX,
                StartChoiceLayout.ContinueMinY,
                StartChoiceLayout.ContinueMaxX,
                StartChoiceLayout.ContinueMaxY,
                StartChoiceLayout.NewMinX,
                StartChoiceLayout.NewMinY,
                StartChoiceLayout.NewMaxX,
                StartChoiceLayout.NewMaxY))
            {
                return false;
            }

            if (RectsOverlap(
                StartChoiceLayout.TitleMinX,
                StartChoiceLayout.TitleMinY,
                StartChoiceLayout.TitleMaxX,
                StartChoiceLayout.TitleMaxY,
                StartChoiceLayout.ContinueMinX,
                StartChoiceLayout.ContinueMinY,
                StartChoiceLayout.ContinueMaxX,
                StartChoiceLayout.ContinueMaxY))
            {
                return false;
            }

            if (RectsOverlap(
                StartChoiceLayout.YesMinX,
                StartChoiceLayout.YesMinY,
                StartChoiceLayout.YesMaxX,
                StartChoiceLayout.YesMaxY,
                StartChoiceLayout.NoMinX,
                StartChoiceLayout.NoMinY,
                StartChoiceLayout.NoMaxX,
                StartChoiceLayout.NoMaxY))
            {
                return false;
            }

            if (RectsOverlap(
                StartChoiceLayout.QuestionMinX,
                StartChoiceLayout.QuestionMinY,
                StartChoiceLayout.QuestionMaxX,
                StartChoiceLayout.QuestionMaxY,
                StartChoiceLayout.YesMinX,
                StartChoiceLayout.YesMinY,
                StartChoiceLayout.YesMaxX,
                StartChoiceLayout.YesMaxY))
            {
                return false;
            }

            return true;
        }

        public static bool RectsOverlap(
            float aMinX,
            float aMinY,
            float aMaxX,
            float aMaxY,
            float bMinX,
            float bMinY,
            float bMaxX,
            float bMaxY)
        {
            return aMinX < bMaxX && bMinX < aMaxX && aMinY < bMaxY && bMinY < aMaxY;
        }
    }

    /// <summary>
    /// Screen anchors for the boot card. Continue is left, Start new is right.
    /// The overwrite confirm uses the same row: Yes left, No right.
    /// </summary>
    public static class StartChoiceLayout
    {
        public const float PlateMinX = 0.16f;
        public const float PlateMinY = 0.28f;
        public const float PlateMaxX = 0.84f;
        public const float PlateMaxY = 0.80f;
        public const float TitleMinX = 0.22f;
        public const float TitleMinY = 0.60f;
        public const float TitleMaxX = 0.78f;
        public const float TitleMaxY = 0.70f;
        public const float ContinueMinX = 0.22f;
        public const float ContinueMinY = 0.36f;
        public const float ContinueMaxX = 0.48f;
        public const float ContinueMaxY = 0.52f;
        public const float NewMinX = 0.52f;
        public const float NewMinY = 0.36f;
        public const float NewMaxX = 0.78f;
        public const float NewMaxY = 0.52f;
        public const float QuestionMinX = 0.22f;
        public const float QuestionMinY = 0.58f;
        public const float QuestionMaxX = 0.78f;
        public const float QuestionMaxY = 0.70f;
        public const float YesMinX = 0.22f;
        public const float YesMinY = 0.36f;
        public const float YesMaxX = 0.48f;
        public const float YesMaxY = 0.52f;
        public const float NoMinX = 0.52f;
        public const float NoMinY = 0.36f;
        public const float NoMaxX = 0.78f;
        public const float NoMaxY = 0.52f;
    }
}
