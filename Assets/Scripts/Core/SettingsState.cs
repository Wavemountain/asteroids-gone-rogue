namespace AsteroidsGoneRogue
{
    /// <summary>
    /// How much of the control hint is shown outside the settings panel.
    /// HangarFooter is the short bottom line. Later PRs wire the other modes.
    /// </summary>
    public enum HintMode
    {
        Off = 0,
        SettingsOnly = 1,
        HangarFooter = 2,
    }

    /// <summary>
    /// Which pad axes move the hangar highlight. Reserved for a later PR.
    /// </summary>
    public enum PadNavSource
    {
        DPad = 0,
        Analog = 1,
        Both = 2,
    }

    /// <summary>
    /// Versioned hangar settings. Language stays in <see cref="Loc"/>.
    /// Clamp and round-trip do not touch PlayerPrefs; Load/Save is the only Unity call.
    /// </summary>
    public sealed class SettingsState
    {
        public const int CurrentVersion = 1;
        public const int DefaultHintSizeStep = 1;
        public const int MaxHintSizeStep = 2;

        public const string VersionKey = "agr.settings.version";
        public const string ScreenShakeKey = "agr.settings.screenShake";
        public const string HintModeKey = "agr.settings.hintMode";
        public const string HintSizeStepKey = "agr.settings.hintSizeStep";
        public const string ConfirmRestartInPlayKey = "agr.settings.confirmRestartInPlay";
        public const string ConfirmRestartNewRunKey = "agr.settings.confirmRestartNewRun";
        public const string PadNavSourceKey = "agr.settings.padNavSource";

        public bool ScreenShake;
        public HintMode HintMode;
        public int HintSizeStep;
        public bool ConfirmRestartInPlay;
        public bool ConfirmRestartNewRun;
        public PadNavSource PadNavSource;

        public static SettingsState CreateDefault()
        {
            SettingsState state = new SettingsState();
            state.ScreenShake = true;
            state.HintMode = AsteroidsGoneRogue.HintMode.HangarFooter;
            state.HintSizeStep = DefaultHintSizeStep;
            state.ConfirmRestartInPlay = true;
            state.ConfirmRestartNewRun = false;
            state.PadNavSource = AsteroidsGoneRogue.PadNavSource.Both;
            return state;
        }

        private static bool _screenShakeCached = true;
        private static bool _screenShakeReady;

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

        public static void Publish(SettingsState state)
        {
            SettingsState source = state ?? CreateDefault();
            source.Normalize();
            _screenShakeCached = source.ScreenShake;
            _screenShakeReady = true;
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

            return AsteroidsGoneRogue.HintMode.HangarFooter;
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
            if (version != CurrentVersion)
            {
                return CreateDefault();
            }

            SettingsState state = CreateDefault();
            state.ScreenShake = screenShake != 0;
            state.HintMode = NormalizeHintMode(hintMode);
            state.HintSizeStep = ClampHintSize(hintSizeStep);
            state.ConfirmRestartInPlay = confirmRestartInPlay != 0;
            state.ConfirmRestartNewRun = confirmRestartNewRun != 0;
            state.PadNavSource = NormalizePadNavSource(padNavSource);
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
                prefs.PadNavSource);
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
                UnityEngine.PlayerPrefs.GetInt(ConfirmRestartNewRunKey, 0),
                UnityEngine.PlayerPrefs.GetInt(PadNavSourceKey, (int)AsteroidsGoneRogue.PadNavSource.Both));
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
    }
}
