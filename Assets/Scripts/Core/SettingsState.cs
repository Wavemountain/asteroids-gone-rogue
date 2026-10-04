namespace AsteroidsGoneRogue
{
    /// <summary>
    /// How much of the control hint is shown outside the settings panel.
    /// Off and SettingsOnly keep the full line in the panel only.
    /// HangarFooter is the short hangar footer (Hangar only).
    /// On also shows the in-wave play hint. Persisted ints stay stable.
    /// </summary>
    public enum HintMode
    {
        Off = 0,
        SettingsOnly = 1,
        HangarFooter = 2,
        On = 3,
    }

    /// <summary>
    /// Which pad axes move hangar highlights. Menus only; fly input ignores this.
    /// </summary>
    public enum PadNavSource
    {
        DPad = 0,
        Analog = 1,
        Both = 2,
    }

        /// <summary>
        /// Pure pad-nav filter. DPad keeps the d-pad and ignores the stick and the
        /// Horizontal/Vertical fallback. Analog keeps the left stick (and that
        /// fallback) and ignores the d-pad. Both prefers a live d-pad, then the
        /// stick, then the fallback so keyboard arrows still move.
        /// </summary>
    public static class PadNavSourceRules
    {
        public static bool Live(float x, float y, float dead)
        {
            float limit = dead < 0f ? 0f : dead;
            float mag = (x * x) + (y * y);
            return mag >= limit * limit;
        }

        public static void Select(
            PadNavSource source,
            float dpadX,
            float dpadY,
            float stickX,
            float stickY,
            float dead,
            out float outX,
            out float outY)
        {
            bool dpadLive = Live(dpadX, dpadY, dead);
            bool stickLive = Live(stickX, stickY, dead);
            if (source == AsteroidsGoneRogue.PadNavSource.DPad)
            {
                outX = dpadLive ? dpadX : 0f;
                outY = dpadLive ? dpadY : 0f;
                return;
            }

            if (source == AsteroidsGoneRogue.PadNavSource.Analog)
            {
                outX = stickLive ? stickX : 0f;
                outY = stickLive ? stickY : 0f;
                return;
            }

            if (dpadLive)
            {
                outX = dpadX;
                outY = dpadY;
                return;
            }

            outX = stickLive ? stickX : 0f;
            outY = stickLive ? stickY : 0f;
        }

        /// <summary>
        /// Menu highlight after the source filter. DPad never reads the left
        /// stick or the Horizontal/Vertical fallback (those axes include
        /// joystick 0/1). Analog and Both use that fallback only when their
        /// own source is quiet.
        /// </summary>
        public static void Select(
            PadNavSource source,
            float dpadX,
            float dpadY,
            float stickX,
            float stickY,
            float fallbackX,
            float fallbackY,
            float dead,
            out float outX,
            out float outY)
        {
            Select(source, dpadX, dpadY, stickX, stickY, dead, out outX, out outY);
            if (source == AsteroidsGoneRogue.PadNavSource.DPad)
            {
                return;
            }

            if (Live(outX, outY, dead))
            {
                return;
            }

            if (Live(fallbackX, fallbackY, dead))
            {
                outX = fallbackX;
                outY = fallbackY;
                return;
            }

            outX = 0f;
            outY = 0f;
        }
    }

    /// <summary>
    /// Versioned hangar settings. Language stays in <see cref="Loc"/>.
    /// Clamp and round-trip do not touch PlayerPrefs; Load/Save is the only Unity call.
    /// </summary>
    public sealed class SettingsState
    {
        public const int CurrentVersion = 5;
        public const int DefaultHintSizeStep = 1;
        public const int MaxHintSizeStep = 2;

        public const string VersionKey = "agr.settings.version";
        public const string ScreenShakeKey = "agr.settings.screenShake";
        public const string HintModeKey = "agr.settings.hintMode";
        public const string HintSizeStepKey = "agr.settings.hintSizeStep";
        public const string ConfirmRestartInPlayKey = "agr.settings.confirmRestartInPlay";
        public const string ConfirmRestartNewRunKey = "agr.settings.confirmRestartNewRun";
        public const string PadNavSourceKey = "agr.settings.padNavSource";
        public const string AssistModeKey = "agr.settings.assistMode";
        public const string ReduceEffectsKey = "agr.settings.reduceEffects";
        public const string WindowModeKey = "agr.settings.windowMode";
        public const string ResolutionWidthKey = "agr.settings.resolutionW";
        public const string ResolutionHeightKey = "agr.settings.resolutionH";
        public const string VSyncKey = "agr.settings.vSync";
        public const string FpsCapKey = "agr.settings.fpsCap";

        public bool ScreenShake;
        public bool AssistMode;
        public bool ReduceEffects;
        public HintMode HintMode;
        public int HintSizeStep;
        public bool ConfirmRestartInPlay;
        public bool ConfirmRestartNewRun;
        public PadNavSource PadNavSource;
        public int WindowMode;
        public int ResolutionWidth;
        public int ResolutionHeight;
        public bool VSync;
        public int FpsCap;

        public static SettingsState CreateDefault()
        {
            SettingsState state = new SettingsState();
            state.ScreenShake = true;
            state.HintMode = AsteroidsGoneRogue.HintMode.HangarFooter;
            state.HintSizeStep = DefaultHintSizeStep;
            state.ConfirmRestartInPlay = true;
            state.ConfirmRestartNewRun = true;
            state.PadNavSource = AsteroidsGoneRogue.PadNavSource.Both;
            state.AssistMode = false;
            state.ReduceEffects = false;
            state.WindowMode = DisplaySettings.DefaultWindowMode;
            state.ResolutionWidth = DisplaySettings.DefaultWidth;
            state.ResolutionHeight = DisplaySettings.DefaultHeight;
            state.VSync = DisplaySettings.DefaultVSync != 0;
            state.FpsCap = DisplaySettings.DefaultFpsCap;
            return state;
        }

        private static bool _screenShakeCached = true;
        private static bool _screenShakeReady;
        private static PadNavSource _menuPadNav = AsteroidsGoneRogue.PadNavSource.Both;
        private static bool _menuPadNavReady;
        private static bool _assistCached;
        private static bool _assistReady;
        private static bool _reduceCached;
        private static bool _reduceReady;

        public static bool AssistEnabled
        {
            get
            {
                if (!_assistReady)
                {
                    _assistCached = UnityEngine.PlayerPrefs.GetInt(AssistModeKey, 0) != 0;
                    _assistReady = true;
                }

                return _assistCached;
            }
        }

        /// <summary>
        /// Cached copy of <see cref="ReduceEffects"/>. Juice reads this and does
        /// not touch PlayerPrefs. Missing or pre-v4 saves stay off.
        /// </summary>
        public static bool ReduceEffectsEnabled
        {
            get
            {
                if (!_reduceReady)
                {
                    _reduceCached = UnityEngine.PlayerPrefs.GetInt(ReduceEffectsKey, 0) != 0;
                    _reduceReady = true;
                }

                return _reduceCached;
            }
        }

        /// <summary>
        /// Cached copy of <see cref="ScreenShake"/>. The camera reads this and
        /// does not touch PlayerPrefs. Publish refreshes it when the toggle changes.
        /// </summary>
        public static bool ScreenShakeEnabled
        {
            get
            {
                if (!_screenShakeReady)
                {
                    _screenShakeCached = UnityEngine.PlayerPrefs.GetInt(ScreenShakeKey, 1) != 0;
                    _screenShakeReady = true;
                }

                return _screenShakeCached;
            }
        }

        /// <summary>
        /// Cached <see cref="PadNavSource"/> for menu navigation. Publish refreshes
        /// it. Callers must not read PlayerPrefs on the nav path.
        /// </summary>
        public static PadNavSource MenuPadNav
        {
            get
            {
                if (!_menuPadNavReady)
                {
                    int stored = UnityEngine.PlayerPrefs.GetInt(
                        PadNavSourceKey,
                        (int)AsteroidsGoneRogue.PadNavSource.Both);
                    _menuPadNav = NormalizePadNavSource(stored);
                    _menuPadNavReady = true;
                }

                return _menuPadNav;
            }
        }

        public static void Publish(SettingsState state)
        {
            SettingsState source = state ?? CreateDefault();
            source.Normalize();
            _screenShakeCached = source.ScreenShake;
            _screenShakeReady = true;
            _menuPadNav = source.PadNavSource;
            _menuPadNavReady = true;
            _assistCached = source.AssistMode;
            _assistReady = true;
            _reduceCached = source.ReduceEffects;
            _reduceReady = true;
        }

        /// <summary>
        /// Shake applied this frame. Off, or a non-positive request, yields zero
        /// so a toggle cannot leave residual amplitude.
        /// </summary>
        public static float ShakeAmplitude(bool enabled, float amplitude)
        {
            if (!enabled)
            {
                return 0f;
            }

            if (amplitude <= 0f)
            {
                return 0f;
            }

            return amplitude;
        }

        public static int ClampHintSize(int step)
        {
            if (step < 0)
            {
                return 0;
            }

            if (step > MaxHintSizeStep)
            {
                return MaxHintSizeStep;
            }

            return step;
        }

        public static HintMode NormalizeHintMode(int value)
        {
            if (value == (int)AsteroidsGoneRogue.HintMode.Off)
            {
                return AsteroidsGoneRogue.HintMode.Off;
            }

            if (value == (int)AsteroidsGoneRogue.HintMode.SettingsOnly)
            {
                return AsteroidsGoneRogue.HintMode.SettingsOnly;
            }

            if (value == (int)AsteroidsGoneRogue.HintMode.On)
            {
                return AsteroidsGoneRogue.HintMode.On;
            }

            return AsteroidsGoneRogue.HintMode.HangarFooter;
        }

        public const int HintPxSmall = 14;
        public const int HintPxMedium = 18;
        public const int HintPxLarge = 22;

        public static int HintPx(int step)
        {
            int clamped = ClampHintSize(step);
            if (clamped <= 0)
            {
                return HintPxSmall;
            }

            if (clamped == 1)
            {
                return HintPxMedium;
            }

            return HintPxLarge;
        }

        /// <summary>
        /// Chosen 14/18/22, raised to the narrow-screen floor when the width is at most 1280.
        /// Wide screens keep the chosen size.
        /// </summary>
        public static int EffectiveHintSize(int step, int screenWidth, int narrowFloor)
        {
            int chosen = HintPx(step);
            if (screenWidth <= 1280 && narrowFloor > chosen)
            {
                return narrowFloor;
            }

            return chosen;
        }

        public static bool ShowsHangarFooter(HintMode mode)
        {
            return mode == AsteroidsGoneRogue.HintMode.HangarFooter
                || mode == AsteroidsGoneRogue.HintMode.On;
        }

        public static bool ShowsPlayHint(HintMode mode)
        {
            return mode == AsteroidsGoneRogue.HintMode.On;
        }

        /// <summary>
        /// Wave 1 always shows the Esc / Start coach, including when the play
        /// hint is hidden. Later waves never show it.
        /// </summary>
        public static bool ShowsFirstWaveCoach(bool firstWave, HintMode mode)
        {
            if (!firstWave)
            {
                return false;
            }

            if (mode == AsteroidsGoneRogue.HintMode.Off)
            {
                return true;
            }

            if (mode == AsteroidsGoneRogue.HintMode.SettingsOnly)
            {
                return true;
            }

            if (mode == AsteroidsGoneRogue.HintMode.HangarFooter)
            {
                return true;
            }

            return true;
        }

        /// <summary>
        /// Hint-size steps the settings row can land on. Widths at or below 1280
        /// hide 14 because the footer floor is 18, so the panel must not offer it.
        /// </summary>
        public static int[] HintSizeOptions(int screenWidth)
        {
            if (screenWidth <= 1280)
            {
                return new int[] { 1, 2 };
            }

            return new int[] { 0, 1, 2 };
        }

        /// <summary>
        /// Step actually shown for this width. A saved 14 becomes 18 on a narrow screen.
        /// </summary>
        public static int VisibleHintStep(int step, int screenWidth)
        {
            int[] options = HintSizeOptions(screenWidth);
            int clamped = ClampHintSize(step);
            int best = options[0];
            int count = options.Length;
            for (int i = 0; i < count; i++)
            {
                if (options[i] >= clamped)
                {
                    return options[i];
                }

                best = options[i];
            }

            return best;
        }

        /// <summary>
        /// Row cycle is Off, Hangar only (HangarFooter), On.
        /// SettingsOnly shares Off's slot because both hide the bottom line.
        /// </summary>
        public static HintMode StepHintMode(HintMode current, int direction)
        {
            int index = 0;
            if (current == AsteroidsGoneRogue.HintMode.HangarFooter)
            {
                index = 1;
            }
            else if (current == AsteroidsGoneRogue.HintMode.On)
            {
                index = 2;
            }

            if (direction > 0)
            {
                index += 1;
            }
            else if (direction < 0)
            {
                index -= 1;
            }

            if (index < 0)
            {
                index = 0;
            }

            if (index > 2)
            {
                index = 2;
            }

            if (index == 1)
            {
                return AsteroidsGoneRogue.HintMode.HangarFooter;
            }

            if (index == 2)
            {
                return AsteroidsGoneRogue.HintMode.On;
            }

            return AsteroidsGoneRogue.HintMode.Off;
        }

        public static PadNavSource StepPadNav(PadNavSource current, int direction)
        {
            int index = 0;
            if (current == AsteroidsGoneRogue.PadNavSource.Analog)
            {
                index = 1;
            }
            else if (current == AsteroidsGoneRogue.PadNavSource.Both)
            {
                index = 2;
            }

            if (direction > 0)
            {
                index += 1;
            }
            else if (direction < 0)
            {
                index -= 1;
            }

            if (index < 0)
            {
                index = 0;
            }

            if (index > 2)
            {
                index = 2;
            }

            if (index == 0)
            {
                return AsteroidsGoneRogue.PadNavSource.DPad;
            }

            if (index == 1)
            {
                return AsteroidsGoneRogue.PadNavSource.Analog;
            }

            return AsteroidsGoneRogue.PadNavSource.Both;
        }

        public static int StepHintSize(int step, int direction)
        {
            int next = ClampHintSize(step);
            if (direction > 0)
            {
                next += 1;
            }
            else if (direction < 0)
            {
                next -= 1;
            }

            return ClampHintSize(next);
        }

        public static int StepHintSize(int step, int direction, int screenWidth)
        {
            int[] options = HintSizeOptions(screenWidth);
            int visible = VisibleHintStep(step, screenWidth);
            int index = 0;
            int count = options.Length;
            for (int optionIndex = 0; optionIndex < count; optionIndex++)
            {
                if (options[optionIndex] == visible)
                {
                    index = optionIndex;
                    break;
                }
            }

            if (direction > 0)
            {
                index += 1;
            }
            else if (direction < 0)
            {
                index -= 1;
            }

            if (index < 0)
            {
                index = 0;
            }

            if (index >= count)
            {
                index = count - 1;
            }

            return options[index];
        }

        public static PadNavSource NormalizePadNavSource(int value)
        {
            if (value == (int)AsteroidsGoneRogue.PadNavSource.DPad)
            {
                return AsteroidsGoneRogue.PadNavSource.DPad;
            }

            if (value == (int)AsteroidsGoneRogue.PadNavSource.Analog)
            {
                return AsteroidsGoneRogue.PadNavSource.Analog;
            }

            return AsteroidsGoneRogue.PadNavSource.Both;
        }

        public void Normalize()
        {
            HintMode = NormalizeHintMode((int)HintMode);
            HintSizeStep = ClampHintSize(HintSizeStep);
            PadNavSource = NormalizePadNavSource((int)PadNavSource);
            WindowMode = DisplaySettings.NormalizeWindow(WindowMode);
            FpsCap = DisplaySettings.NormalizeFps(FpsCap);
            ResolutionWidth = DisplaySettings.ClampDimension(
                ResolutionWidth,
                DisplaySettings.MinWidth,
                DisplaySettings.DefaultWidth);
            ResolutionHeight = DisplaySettings.ClampDimension(
                ResolutionHeight,
                DisplaySettings.MinHeight,
                DisplaySettings.DefaultHeight);
        }

        public SettingsPrefs Capture()
        {
            Normalize();
            SettingsPrefs prefs = new SettingsPrefs();
            prefs.Version = CurrentVersion;
            prefs.ScreenShake = ScreenShake ? 1 : 0;
            prefs.HintMode = (int)HintMode;
            prefs.HintSizeStep = HintSizeStep;
            prefs.ConfirmRestartInPlay = ConfirmRestartInPlay ? 1 : 0;
            prefs.ConfirmRestartNewRun = ConfirmRestartNewRun ? 1 : 0;
            prefs.PadNavSource = (int)PadNavSource;
            prefs.AssistMode = AssistMode ? 1 : 0;
            prefs.ReduceEffects = ReduceEffects ? 1 : 0;
            prefs.WindowMode = WindowMode;
            prefs.ResolutionWidth = ResolutionWidth;
            prefs.ResolutionHeight = ResolutionHeight;
            prefs.VSync = VSync ? 1 : 0;
            prefs.FpsCap = FpsCap;
            return prefs;
        }

        public static SettingsState FromInts(
            int version,
            int screenShake,
            int hintMode,
            int hintSizeStep,
            int confirmRestartInPlay,
            int confirmRestartNewRun,
            int padNavSource)
        {
            return FromInts(
                version,
                screenShake,
                hintMode,
                hintSizeStep,
                confirmRestartInPlay,
                confirmRestartNewRun,
                padNavSource,
                0,
                0);
        }

        public static SettingsState FromInts(
            int version,
            int screenShake,
            int hintMode,
            int hintSizeStep,
            int confirmRestartInPlay,
            int confirmRestartNewRun,
            int padNavSource,
            int assistMode)
        {
            return FromInts(
                version,
                screenShake,
                hintMode,
                hintSizeStep,
                confirmRestartInPlay,
                confirmRestartNewRun,
                padNavSource,
                assistMode,
                0);
        }

        public static SettingsState FromInts(
            int version,
            int screenShake,
            int hintMode,
            int hintSizeStep,
            int confirmRestartInPlay,
            int confirmRestartNewRun,
            int padNavSource,
            int assistMode,
            int reduceEffects)
        {
            return FromInts(
                version,
                screenShake,
                hintMode,
                hintSizeStep,
                confirmRestartInPlay,
                confirmRestartNewRun,
                padNavSource,
                assistMode,
                reduceEffects,
                DisplaySettings.DefaultWindowMode,
                DisplaySettings.DefaultWidth,
                DisplaySettings.DefaultHeight,
                DisplaySettings.DefaultVSync,
                DisplaySettings.DefaultFpsCap);
        }

        public static SettingsState FromInts(
            int version,
            int screenShake,
            int hintMode,
            int hintSizeStep,
            int confirmRestartInPlay,
            int confirmRestartNewRun,
            int padNavSource,
            int assistMode,
            int reduceEffects,
            int windowMode,
            int resolutionWidth,
            int resolutionHeight,
            int vSync,
            int fpsCap)
        {
            if (version < 1 || version > CurrentVersion)
            {
                return CreateDefault();
            }

            SettingsState state = CreateDefault();
            state.ScreenShake = screenShake != 0;
            state.HintMode = NormalizeHintMode(hintMode);
            state.HintSizeStep = ClampHintSize(hintSizeStep);
            state.ConfirmRestartInPlay = confirmRestartInPlay != 0;
            // Version 1 stored the old default (off). Treat that saved value as unset.
            state.ConfirmRestartNewRun = version == 1 || confirmRestartNewRun != 0;
            state.PadNavSource = NormalizePadNavSource(padNavSource);
            state.AssistMode = version >= 3 && assistMode != 0;
            state.ReduceEffects = version >= 4 && reduceEffects != 0;
            // Versions before 5 have no display keys. Keep the launch defaults.
            if (version >= 5)
            {
                state.WindowMode = DisplaySettings.NormalizeWindow(windowMode);
                state.ResolutionWidth = DisplaySettings.ClampDimension(
                    resolutionWidth,
                    DisplaySettings.MinWidth,
                    DisplaySettings.DefaultWidth);
                state.ResolutionHeight = DisplaySettings.ClampDimension(
                    resolutionHeight,
                    DisplaySettings.MinHeight,
                    DisplaySettings.DefaultHeight);
                state.VSync = DisplaySettings.NormalizeVSync(vSync) != 0;
                state.FpsCap = DisplaySettings.NormalizeFps(fpsCap);
            }

            return state;
        }

        public static SettingsState FromCaptured(SettingsPrefs prefs)
        {
            return FromInts(
                prefs.Version,
                prefs.ScreenShake,
                prefs.HintMode,
                prefs.HintSizeStep,
                prefs.ConfirmRestartInPlay,
                prefs.ConfirmRestartNewRun,
                prefs.PadNavSource,
                prefs.AssistMode,
                prefs.ReduceEffects,
                prefs.WindowMode,
                prefs.ResolutionWidth,
                prefs.ResolutionHeight,
                prefs.VSync,
                prefs.FpsCap);
        }

        public static SettingsState Load()
        {
            int version = UnityEngine.PlayerPrefs.GetInt(VersionKey, 0);
            SettingsState state = FromInts(
                version,
                UnityEngine.PlayerPrefs.GetInt(ScreenShakeKey, 1),
                UnityEngine.PlayerPrefs.GetInt(HintModeKey, (int)AsteroidsGoneRogue.HintMode.HangarFooter),
                UnityEngine.PlayerPrefs.GetInt(HintSizeStepKey, DefaultHintSizeStep),
                UnityEngine.PlayerPrefs.GetInt(ConfirmRestartInPlayKey, 1),
                UnityEngine.PlayerPrefs.GetInt(ConfirmRestartNewRunKey, 1),
                UnityEngine.PlayerPrefs.GetInt(PadNavSourceKey, (int)AsteroidsGoneRogue.PadNavSource.Both),
                UnityEngine.PlayerPrefs.GetInt(AssistModeKey, 0),
                UnityEngine.PlayerPrefs.GetInt(ReduceEffectsKey, 0),
                UnityEngine.PlayerPrefs.GetInt(WindowModeKey, DisplaySettings.DefaultWindowMode),
                UnityEngine.PlayerPrefs.GetInt(ResolutionWidthKey, DisplaySettings.DefaultWidth),
                UnityEngine.PlayerPrefs.GetInt(ResolutionHeightKey, DisplaySettings.DefaultHeight),
                UnityEngine.PlayerPrefs.GetInt(VSyncKey, DisplaySettings.DefaultVSync),
                UnityEngine.PlayerPrefs.GetInt(FpsCapKey, DisplaySettings.DefaultFpsCap));
            Publish(state);
            if (version != CurrentVersion)
            {
                state.Save();
            }

            return state;
        }

        public void Save()
        {
            SettingsPrefs prefs = Capture();
            UnityEngine.PlayerPrefs.SetInt(VersionKey, prefs.Version);
            UnityEngine.PlayerPrefs.SetInt(ScreenShakeKey, prefs.ScreenShake);
            UnityEngine.PlayerPrefs.SetInt(HintModeKey, prefs.HintMode);
            UnityEngine.PlayerPrefs.SetInt(HintSizeStepKey, prefs.HintSizeStep);
            UnityEngine.PlayerPrefs.SetInt(ConfirmRestartInPlayKey, prefs.ConfirmRestartInPlay);
            UnityEngine.PlayerPrefs.SetInt(ConfirmRestartNewRunKey, prefs.ConfirmRestartNewRun);
            UnityEngine.PlayerPrefs.SetInt(PadNavSourceKey, prefs.PadNavSource);
            UnityEngine.PlayerPrefs.SetInt(AssistModeKey, prefs.AssistMode);
            UnityEngine.PlayerPrefs.SetInt(ReduceEffectsKey, prefs.ReduceEffects);
            UnityEngine.PlayerPrefs.SetInt(WindowModeKey, prefs.WindowMode);
            UnityEngine.PlayerPrefs.SetInt(ResolutionWidthKey, prefs.ResolutionWidth);
            UnityEngine.PlayerPrefs.SetInt(ResolutionHeightKey, prefs.ResolutionHeight);
            UnityEngine.PlayerPrefs.SetInt(VSyncKey, prefs.VSync);
            UnityEngine.PlayerPrefs.SetInt(FpsCapKey, prefs.FpsCap);
            UnityEngine.PlayerPrefs.Save();
            Publish(this);
        }
    }

    public struct SettingsPrefs
    {
        public int Version;
        public int ScreenShake;
        public int HintMode;
        public int HintSizeStep;
        public int ConfirmRestartInPlay;
        public int ConfirmRestartNewRun;
        public int PadNavSource;
        public int AssistMode;
        public int ReduceEffects;
        public int WindowMode;
        public int ResolutionWidth;
        public int ResolutionHeight;
        public int VSync;
        public int FpsCap;
    }
}
