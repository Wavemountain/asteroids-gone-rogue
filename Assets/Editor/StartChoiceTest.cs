using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace AsteroidsGoneRogue.EditorTools
{
    /// <summary>
    /// Batch play-mode check for the boot Continue / Start new panel.
    /// Drives GameUi through MenuNavProbe (the same KeyDown / d-pad / confirm
    /// path the panel reads). Saves go to a temp profile, never the user's
    /// persistentDataPath. PlayerPrefs are snapshotted and restored.
    ///
    /// "C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe" -batchmode -projectPath . -executeMethod AsteroidsGoneRogue.EditorTools.StartChoiceTest.Run -logFile -
    /// </summary>
    [InitializeOnLoad]
    public static class StartChoiceTest
    {
        const string PlayScene = "Assets/Scenes/Play.unity";
        const double TotalSeconds = 300.0;
        const double CaseSeconds = 60.0;
        const double StepSeconds = 12.0;

        const int PhasePrepare = 0;
        const int PhaseEnter = 1;
        const int PhasePlay = 2;
        const int PhaseLeave = 3;
        const int PhaseNext = 4;
        const int PhaseRestore = 5;

        const int Wait = 0;
        const int Done = 1;
        const int Reboot = 2;
        const int Failed = -1;

        const int Credits = 180;
        const int Mask = 257;
        const int Doctrine = 1;
        const int Shield = 2;
        const int Wave = 3;
        const int OldRunId = 7;

        const string KeyRunning = "StartChoiceTest.Running";
        const string KeyPhase = "StartChoiceTest.Phase";
        const string KeyCase = "StartChoiceTest.Case";
        const string KeyStep = "StartChoiceTest.Step";
        const string KeyBoot = "StartChoiceTest.Boot";
        const string KeyProfile = "StartChoiceTest.Profile";
        const string KeyOut = "StartChoiceTest.Out";
        const string KeyStart = "StartChoiceTest.Start";
        const string KeyCaseStart = "StartChoiceTest.CaseStart";
        const string KeyStepStart = "StartChoiceTest.StepStart";
        const string KeyEx = "StartChoiceTest.Ex";
        const string KeyExCase = "StartChoiceTest.ExCase";
        const string KeyFailed = "StartChoiceTest.Failed";
        const string KeyReboot = "StartChoiceTest.Reboot";
        const string KeyEntering = "StartChoiceTest.Entering";
        const string KeyLeaving = "StartChoiceTest.Leaving";

        static readonly string[] CaseIds = new string[] { "2", "4", "5", "6a", "6b" };

        static readonly string[] SaveNames = new string[]
        {
            "run_save.json",
            "run_save.json.bak",
            "run_save.json.tmp",
            "meta_save.json",
            "meta_save.json.bak",
            "meta_save.json.tmp",
        };

        static long _deadlineTicks;
        static long _caseDeadlineTicks;
        static string _watchOut = string.Empty;
        static string _watchProfile = string.Empty;
        static string _watchPhase = "prepare";
        static string _watchCase = "run";
        static int _exitPosted;
        static int _bailed;
        static Timer _watchdog;
        static SynchronizationContext _main;
        static double _lastWaitLog = -1.0;
        static string _lastWaitWhat = string.Empty;
        static double _lastOverwriteLog = -1.0;
        static StackTraceLogType _savedWarningTrace;
        static bool _traceSaved;

        static StartChoiceTest()
        {
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            Application.logMessageReceived -= OnLog;
            Application.logMessageReceived += OnLog;
            if (SessionState.GetBool(KeyRunning, false))
            {
                RefreshWatch();
                ArmWatchdog();
                Note("phase resume " + PhaseName(SessionState.GetInt(KeyPhase, PhasePrepare)) + " elapsed=" + Since(KeyStart).ToString("0.0", CultureInfo.InvariantCulture));
            }
        }

        [InitializeOnEnterPlayMode]
        static void ForcePanelBeforeScene(EnterPlayModeOptions options)
        {
            if (!SessionState.GetBool(KeyRunning, false))
            {
                return;
            }

            string profile = SessionState.GetString(KeyProfile, string.Empty);
            if (profile.Length > 0)
            {
                RunSaveStore.ProfileRoot = profile;
            }

            StartChoice.ForcePanel = true;
            Environment.SetEnvironmentVariable(StartChoice.ForceEnv, "1");
            Note("force panel on batch override");
        }

        public static void Run()
        {
            if (SessionState.GetBool(KeyRunning, false))
            {
                Debug.LogError("StartChoiceTest FAIL already running");
                Quit(1);
                return;
            }

            string persistent = Path.GetFullPath(Application.persistentDataPath);
            string profile = NewProfile(persistent);
            if (profile == null)
            {
                WriteEarlyFail("FAIL isolation profile is inside persistentDataPath");
                Quit(1);
                return;
            }

            string stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
            string outDir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Captures", "StartChoiceTest", stamp));
            Directory.CreateDirectory(outDir);
            Directory.CreateDirectory(profile);
            File.WriteAllText(Path.Combine(outDir, "persistent-before.txt"), FingerprintPersistent(persistent));
            File.WriteAllText(Path.Combine(outDir, "prefs-before.txt"), ReadPrefsText());
            File.WriteAllText(Path.Combine(outDir, "cases.txt"), string.Empty);
            File.WriteAllText(
                Path.Combine(outDir, "isolation.txt"),
                "PASS profile " + profile + " outside " + persistent);

            Environment.SetEnvironmentVariable(RunSaveStore.ProfileEnv, profile);
            Environment.SetEnvironmentVariable(StartChoice.ForceEnv, "1");
            RunSaveStore.ProfileRoot = profile;
            StartChoice.ForcePanel = true;
            MenuNavProbe.Clear();

            SessionState.SetBool(KeyRunning, true);
            SessionState.SetInt(KeyCase, 0);
            SessionState.SetInt(KeyStep, 0);
            SessionState.SetInt(KeyBoot, 0);
            SessionState.SetString(KeyProfile, profile);
            SessionState.SetString(KeyOut, outDir);
            SessionState.SetString(KeyStart, Stamp());
            SessionState.SetString(KeyCaseStart, Stamp());
            SessionState.SetString(KeyStepStart, Stamp());
            SetPhase(PhasePrepare);
            SessionState.SetInt(KeyEx, 0);
            SessionState.SetInt(KeyExCase, 0);
            SessionState.SetInt(KeyFailed, 0);
            SessionState.SetBool(KeyReboot, false);
            SessionState.SetBool(KeyEntering, false);
            SessionState.SetBool(KeyLeaving, false);
            QuietWarnings();
            RefreshWatch();
            ArmWatchdog();
            Note("profile " + profile);
            Note("out " + outDir);
        }

        static void Tick()
        {
            if (!SessionState.GetBool(KeyRunning, false))
            {
                return;
            }

            try
            {
                if (EditorApplication.isPlaying)
                {
                    EditorApplication.QueuePlayerLoopUpdate();
                }

                RefreshWatch();
                int phase = SessionState.GetInt(KeyPhase, PhasePrepare);
                if (Since(KeyStart) > TotalSeconds)
                {
                    Timeout(PhaseName(phase), "timeout total");
                    return;
                }

                if ((phase == PhaseEnter || phase == PhasePlay) && Since(KeyCaseStart) > CaseSeconds)
                {
                    Timeout(PhaseName(phase), "timeout case");
                    return;
                }

                NoteWait(PhaseName(phase));
                if (phase == PhasePrepare)
                {
                    Prepare();
                }
                else if (phase == PhaseEnter)
                {
                    Enter();
                }
                else if (phase == PhasePlay)
                {
                    Play();
                }
                else if (phase == PhaseLeave)
                {
                    Leave();
                }
                else if (phase == PhaseNext)
                {
                    Next();
                }
                else if (phase == PhaseRestore)
                {
                    Restore();
                }
            }
            catch (Exception ex)
            {
                FailRun(ex.GetType().Name + " " + ex.Message);
            }
        }

        static void Prepare()
        {
            string profile = SessionState.GetString(KeyProfile, string.Empty);
            string persistent = Path.GetFullPath(Application.persistentDataPath);
            if (!ProfileOutsidePersistent(profile, persistent))
            {
                FailRun("FAIL isolation profile is inside persistentDataPath");
                return;
            }

            RunSaveStore.ProfileRoot = profile;
            int index = SessionState.GetInt(KeyCase, 0);
            int boot = SessionState.GetInt(KeyBoot, 0);
            string id = CaseIds[index];
            // The reboot must see the files Yes left. Clearing them would hide a fresh wave-1 save.
            if (!(id == "4" && boot == 1))
            {
                ClearProfileSaves(profile);
            }
            bool wrote = false;
            if (id == "2" || (id == "4" && boot == 0) || (id == "5" && boot == 0) || id == "6a")
            {
                if (!WriteKnownSave())
                {
                    Record(id, false, "TrySaveRun rejected the wave 3 save");
                    SessionState.SetInt(KeyFailed, 1);
                    SetPhase(PhaseNext);
                    return;
                }

                wrote = true;
            }

            if (id == "6a")
            {
                CorruptMain(profile);
            }
            else if (id == "6b")
            {
                WriteGarbage(profile);
            }

            if (wrote && id != "6b")
            {
                string main = Path.Combine(profile, RunSaveStore.RunFileName);
                if (!File.Exists(main) && id != "6a")
                {
                    Record(id, false, "run_save.json missing after TrySaveRun");
                    SessionState.SetInt(KeyFailed, 1);
                    SetPhase(PhaseNext);
                    return;
                }
            }

            SessionState.SetString(KeyCaseStart, Stamp());
            SessionState.SetString(KeyStepStart, Stamp());
            SessionState.SetInt(KeyStep, 0);
            SessionState.SetBool(KeyEntering, false);
            SetPhase(PhaseEnter);
        }

        static void Enter()
        {
            EditorApplication.QueuePlayerLoopUpdate();
            if (SessionState.GetBool(KeyEntering, false))
            {
                NoteWait("enter isPlaying=" + EditorApplication.isPlaying.ToString());
                if (EditorApplication.isPlaying)
                {
                    SessionState.SetBool(KeyEntering, false);
                    SessionState.SetInt(KeyExCase, SessionState.GetInt(KeyEx, 0));
                    SessionState.SetInt(KeyStep, 0);
                    SessionState.SetString(KeyStepStart, Stamp());
                    SetPhase(PhasePlay);
                }

                return;
            }

            if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                NoteWait("enter already changing");
                return;
            }

            SessionState.SetBool(KeyEntering, true);
            EditorSceneManager.OpenScene(PlayScene);
            Note("phase enter request isPlaying=true");
            EditorApplication.isPlaying = true;
            EditorApplication.QueuePlayerLoopUpdate();
        }

        static void Play()
        {
            if (!EditorApplication.isPlaying)
            {
                if (Since(KeyCaseStart) > CaseSeconds)
                {
                    FinishCase(false, "timed out entering play mode");
                }

                return;
            }

            if (Since(KeyCaseStart) > CaseSeconds)
            {
                FinishCase(false, "case timeout");
                return;
            }

            string id = CaseIds[SessionState.GetInt(KeyCase, 0)];
            int code = Wait;
            if (id == "2")
            {
                code = CaseContinue();
            }
            else if (id == "4")
            {
                code = CaseOverwrite();
            }
            else if (id == "5")
            {
                code = CaseNav();
            }
            else if (id == "6a")
            {
                code = CaseBak();
            }
            else if (id == "6b")
            {
                code = CaseNoBak();
            }

            if (code == Wait)
            {
                return;
            }

            if (code == Reboot)
            {
                SessionState.SetBool(KeyReboot, true);
                SessionState.SetInt(KeyBoot, SessionState.GetInt(KeyBoot, 0) + 1);
                SetPhase(PhaseLeave);
                return;
            }

            SetPhase(PhaseLeave);
        }

        static void Leave()
        {
            MenuNavProbe.Clear();
            if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (!SessionState.GetBool(KeyLeaving, false))
                {
                    SessionState.SetBool(KeyLeaving, true);
                    EditorApplication.isPlaying = false;
                }

                return;
            }

            SessionState.SetBool(KeyLeaving, false);
            SetPhase(PhaseNext);
        }

        static void Next()
        {
            if (SessionState.GetBool(KeyReboot, false))
            {
                SessionState.SetBool(KeyReboot, false);
                SessionState.SetInt(KeyStep, 0);
                SetPhase(PhasePrepare);
                return;
            }

            int index = SessionState.GetInt(KeyCase, 0) + 1;
            if (index >= CaseIds.Length)
            {
                SetPhase(PhaseRestore);
                return;
            }

            SessionState.SetInt(KeyCase, index);
            SessionState.SetInt(KeyBoot, 0);
            SessionState.SetInt(KeyStep, 0);
            SetPhase(PhasePrepare);
        }

        static int CaseContinue()
        {
            int step = SessionState.GetInt(KeyStep, 0);
            string fail;
            if (step == 0)
            {
                int ready = WaitPanel(out fail);
                if (ready == 0)
                {
                    return Wait;
                }

                if (ready < 0)
                {
                    FinishCase(false, fail);
                    return Failed;
                }

                if (!ContinueEnabled())
                {
                    FinishCase(false, "Continue disabled");
                    return Failed;
                }

                int world = WorldCatalog.NumberForWave(Wave);
                string expect = RunSummary.ContinueRunLabel(world, Wave);
                string label = ContinueLabel();
                if (label != expect)
                {
                    FinishCase(false, "label '" + label + "' expected '" + expect + "'");
                    return Failed;
                }

                if (Focus() != StartChoice.FocusContinue)
                {
                    FinishCase(false, "focus started on " + Focus().ToString(CultureInfo.InvariantCulture));
                    return Failed;
                }

                Advance();
                return Wait;
            }

            if (step == 1)
            {
                PulseKey(KeyCode.Return);
                Advance();
                return Wait;
            }

            GameUi ui = FindUi();
            if (ui == null || PanelOpen(ui) || WaveIndex() != Wave)
            {
                if (StepTimedOut())
                {
                    FinishCase(false, "Continue did not restore wave " + Wave.ToString(CultureInfo.InvariantCulture));
                    return Failed;
                }

                return Wait;
            }

            if (ExceptionsThisCase())
            {
                FinishCase(false, "exception during Continue");
                return Failed;
            }

            int credits = CreditsNow();
            int doctrine = DoctrineNow();
            int shield = ShieldNow();
            bool rapid = Owns(UpgradeId.RapidFire);
            bool spread = Owns(UpgradeId.SpreadBolt);
            if (credits != Credits || doctrine != Doctrine || shield != Shield || !rapid || !spread)
            {
                FinishCase(
                    false,
                    "restored credits " + credits.ToString(CultureInfo.InvariantCulture)
                    + " doctrine " + doctrine.ToString(CultureInfo.InvariantCulture)
                    + " shield " + shield.ToString(CultureInfo.InvariantCulture)
                    + " rapid " + rapid.ToString()
                    + " spread " + spread.ToString());
                return Failed;
            }

            FinishCase(true, "restored wave 3 credits 180 mask 257 doctrine 1 shield 2");
            return Done;
        }

        static int CaseOverwrite()
        {
            int boot = SessionState.GetInt(KeyBoot, 0);
            int step = SessionState.GetInt(KeyStep, 0);
            if (boot == 0)
            {
                string fail;
                if (step == 0)
                {
                    int ready = WaitPanel(out fail);
                    if (ready == 0)
                    {
                        return Wait;
                    }

                    if (ready < 0)
                    {
                        FinishCase(false, fail);
                        return Failed;
                    }

                    PulseKey(KeyCode.RightArrow);
                    Advance();
                    return Wait;
                }

                if (step == 1)
                {
                    if (Focus() != StartChoice.FocusNew || !StepQuiet())
                    {
                        if (StepTimedOut())
                        {
                            FinishCase(false, "Start new did not take focus");
                            return Failed;
                        }

                        return Wait;
                    }

                    PulseKey(KeyCode.Return);
                    Advance();
                    return Wait;
                }

                if (step == 2)
                {
                    if (!ConfirmOpen() || ConfirmFocus() != StartChoice.FocusNo || !StepQuiet())
                    {
                        if (StepTimedOut())
                        {
                            FinishCase(false, "overwrite confirm did not focus No");
                            return Failed;
                        }

                        return Wait;
                    }

                    PulseKey(KeyCode.LeftArrow);
                    Advance();
                    return Wait;
                }

                if (step == 3)
                {
                    if (ConfirmFocus() != StartChoice.FocusYes || !StepQuiet())
                    {
                        if (StepTimedOut())
                        {
                            FinishCase(false, "Yes did not take focus");
                            return Failed;
                        }

                        return Wait;
                    }

                    PulseKey(KeyCode.Return);
                    Advance();
                    return Wait;
                }

                LogOverwriteProbe();
                if (!OldRunReplaced())
                {
                    if (StepTimedOut())
                    {
                        _lastOverwriteLog = -1.0;
                        LogOverwriteProbe();
                        FinishCase(false, "Yes did not reach wave 1 with the old save gone or replaced");
                        return Failed;
                    }

                    return Wait;
                }

                _lastOverwriteLog = -1.0;
                LogOverwriteProbe();

                if (ExceptionsThisCase())
                {
                    FinishCase(false, "exception during overwrite Yes");
                    return Failed;
                }

                return Reboot;
            }

            string bootFail;
            int bootReady = WaitPanel(out bootFail);
            if (bootReady == 0)
            {
                return Wait;
            }

            if (bootReady < 0)
            {
                FinishCase(false, bootFail);
                return Failed;
            }

            _lastOverwriteLog = -1.0;
            LogOverwriteProbe();
            if (!FileIsFresh(MainPath()) || !FileIsFresh(BakPath()))
            {
                FinishCase(false, "old save still present after reboot");
                return Failed;
            }

            bool freshFile = File.Exists(MainPath()) || File.Exists(BakPath());
            if (!freshFile)
            {
                if (ContinueEnabled() || ContinueInteractable())
                {
                    FinishCase(false, "Continue still enabled after overwrite");
                    return Failed;
                }
            }
            else
            {
                GameManager game = UnityEngine.Object.FindAnyObjectByType<GameManager>();
                RunSaveData pending = game != null ? game.PendingContinue : null;
                if (!ContinueEnabled()
                    || pending == null
                    || pending.WaveIndex != 1
                    || pending.RunId == OldRunId)
                {
                    int offered = pending == null ? -1 : pending.WaveIndex;
                    int runId = pending == null ? -1 : pending.RunId;
                    FinishCase(
                        false,
                        "reboot did not offer the fresh wave 1 run wave "
                        + offered.ToString(CultureInfo.InvariantCulture)
                        + " run "
                        + runId.ToString(CultureInfo.InvariantCulture));
                    return Failed;
                }
            }

            if (ExceptionsThisCase())
            {
                FinishCase(false, "exception after overwrite reboot");
                return Failed;
            }

            if (!freshFile)
            {
                FinishCase(true, "wave 1 old save gone continue disabled after reboot");
            }
            else
            {
                GameManager game = UnityEngine.Object.FindAnyObjectByType<GameManager>();
                int runId = game != null && game.PendingContinue != null ? game.PendingContinue.RunId : -1;
                FinishCase(
                    true,
                    "wave 1 old run replaced by run "
                    + runId.ToString(CultureInfo.InvariantCulture)
                    + " continue offers wave 1");
            }

            return Done;
        }

        static int CaseNav()
        {
            int boot = SessionState.GetInt(KeyBoot, 0);
            int step = SessionState.GetInt(KeyStep, 0);
            if (boot == 0)
            {
                string fail;
                if (step == 0)
                {
                    int ready = WaitPanel(out fail);
                    if (ready == 0)
                    {
                        return Wait;
                    }

                    if (ready < 0)
                    {
                        FinishCase(false, fail);
                        return Failed;
                    }

                    if (Focus() != StartChoice.FocusContinue || !ContinueEnabled())
                    {
                        FinishCase(false, "saved run did not focus Continue");
                        return Failed;
                    }

                    PulseDpad(1f);
                    Advance();
                    return Wait;
                }

                if (step == 1)
                {
                    if (Focus() != StartChoice.FocusNew || !StepQuiet())
                    {
                        if (StepTimedOut())
                        {
                            FinishCase(false, "gamepad did not move to Start new");
                            return Failed;
                        }

                        return Wait;
                    }

                    PulseKey(KeyCode.LeftArrow);
                    Advance();
                    return Wait;
                }

                if (step == 2)
                {
                    if (Focus() != StartChoice.FocusContinue || !StepQuiet())
                    {
                        if (StepTimedOut())
                        {
                            FinishCase(false, "keyboard did not move back to Continue");
                            return Failed;
                        }

                        return Wait;
                    }

                    PulseKey(KeyCode.RightArrow);
                    Advance();
                    return Wait;
                }

                if (step == 3)
                {
                    if (Focus() != StartChoice.FocusNew || !StepQuiet())
                    {
                        if (StepTimedOut())
                        {
                            FinishCase(false, "keyboard did not move to Start new");
                            return Failed;
                        }

                        return Wait;
                    }

                    PulseKey(KeyCode.Return);
                    Advance();
                    return Wait;
                }

                if (step == 4)
                {
                    if (!ConfirmOpen() || ConfirmFocus() != StartChoice.FocusNo || !StepQuiet())
                    {
                        if (StepTimedOut())
                        {
                            FinishCase(false, "confirm did not open on No");
                            return Failed;
                        }

                        return Wait;
                    }

                    PulseKey(KeyCode.Escape);
                    Advance();
                    return Wait;
                }

                if (step == 5)
                {
                    if (ConfirmOpen() || !SaveExists() || !StepQuiet())
                    {
                        if (StepTimedOut())
                        {
                            FinishCase(false, "No did not return to the start panel with the save intact");
                            return Failed;
                        }

                        return Wait;
                    }

                    PulseKey(KeyCode.Return);
                    Advance();
                    return Wait;
                }

                if (step == 6)
                {
                    if (!ConfirmOpen() || ConfirmFocus() != StartChoice.FocusNo || !StepQuiet())
                    {
                        if (StepTimedOut())
                        {
                            FinishCase(false, "confirm did not reopen on No");
                            return Failed;
                        }

                        return Wait;
                    }

                    PulseDpad(-1f);
                    Advance();
                    return Wait;
                }

                if (step == 7)
                {
                    if (ConfirmFocus() != StartChoice.FocusYes || !StepQuiet())
                    {
                        if (StepTimedOut())
                        {
                            FinishCase(false, "gamepad did not move to Yes");
                            return Failed;
                        }

                        return Wait;
                    }

                    PulseSubmit();
                    Advance();
                    return Wait;
                }

                if (ConfirmOpen() || WaveIndex() != 1)
                {
                    if (StepTimedOut())
                    {
                        FinishCase(false, "Yes routing did not reach wave 1");
                        return Failed;
                    }

                    return Wait;
                }

                return Reboot;
            }

            string bootFail;
            if (step == 0)
            {
                int ready = WaitPanel(out bootFail);
                if (ready == 0)
                {
                    return Wait;
                }

                if (ready < 0)
                {
                    FinishCase(false, bootFail);
                    return Failed;
                }

                if (ContinueEnabled() || ContinueInteractable() || Focus() != StartChoice.FocusNew)
                {
                    FinishCase(false, "no-save focus did not start on Start new");
                    return Failed;
                }

                PulseDpad(-1f);
                Advance();
                return Wait;
            }

            if (step == 1)
            {
                if (!StepQuiet())
                {
                    return Wait;
                }

                if (Focus() != StartChoice.FocusNew)
                {
                    FinishCase(false, "gamepad moved onto grey Continue");
                    return Failed;
                }

                PulseKey(KeyCode.LeftArrow);
                Advance();
                return Wait;
            }

            if (!StepQuiet())
            {
                return Wait;
            }

            if (Focus() != StartChoice.FocusNew || ContinueEnabled())
            {
                FinishCase(false, "keyboard moved onto grey Continue");
                return Failed;
            }

            if (ExceptionsThisCase())
            {
                FinishCase(false, "exception during navigation");
                return Failed;
            }

            FinishCase(true, "pad and keyboard move, confirm focuses No, No keeps the save, Yes routes, grey Continue is skipped");
            return Done;
        }

        static int CaseBak()
        {
            string fail;
            int ready = WaitPanel(out fail);
            if (ready == 0)
            {
                return Wait;
            }

            if (ready < 0)
            {
                FinishCase(false, fail);
                return Failed;
            }

            if (!ContinueEnabled())
            {
                FinishCase(false, "bak was not used, Continue stayed disabled");
                return Failed;
            }

            string expect = RunSummary.ContinueRunLabel(WorldCatalog.NumberForWave(Wave), Wave);
            if (ContinueLabel() != expect)
            {
                FinishCase(false, "bak continue label '" + ContinueLabel() + "'");
                return Failed;
            }

            if (ExceptionsThisCase())
            {
                FinishCase(false, "exception while loading a corrupt save with bak");
                return Failed;
            }

            FinishCase(true, "corrupt main used bak, Continue enabled");
            return Done;
        }

        static int CaseNoBak()
        {
            string fail;
            int ready = WaitPanel(out fail);
            if (ready == 0)
            {
                return Wait;
            }

            if (ready < 0)
            {
                FinishCase(false, fail);
                return Failed;
            }

            if (ContinueEnabled() || ContinueInteractable())
            {
                FinishCase(false, "Continue enabled without a valid save");
                return Failed;
            }

            if (ExceptionsThisCase())
            {
                FinishCase(false, "exception while loading a corrupt save");
                return Failed;
            }

            FinishCase(true, "corrupt main without bak, Continue disabled");
            return Done;
        }

        static void FinishCase(bool pass, string reason)
        {
            string id = CaseIds[SessionState.GetInt(KeyCase, 0)];
            Record(id, pass, reason);
            if (!pass)
            {
                SessionState.SetInt(KeyFailed, 1);
            }
        }

        static void Restore()
        {
            if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.isPlaying = false;
                return;
            }

            string outDir = SessionState.GetString(KeyOut, string.Empty);
            string persistent = Path.GetFullPath(Application.persistentDataPath);
            RestoreWarnings();
            ClearEnv();
            RunSaveStore.ProfileRoot = null;
            StartChoice.ForcePanel = false;
            MenuNavProbe.Clear();

            string beforePrefs = File.Exists(Path.Combine(outDir, "prefs-before.txt"))
                ? File.ReadAllText(Path.Combine(outDir, "prefs-before.txt"))
                : string.Empty;
            WritePrefsText(beforePrefs);
            string afterPrefs = ReadPrefsText();
            bool prefsMatch = beforePrefs == afterPrefs;
            string beforeFiles = File.Exists(Path.Combine(outDir, "persistent-before.txt"))
                ? File.ReadAllText(Path.Combine(outDir, "persistent-before.txt"))
                : string.Empty;
            string afterFiles = FingerprintPersistent(persistent);
            bool filesSame = beforeFiles == afterFiles;
            string isolation = File.Exists(Path.Combine(outDir, "isolation.txt"))
                ? File.ReadAllText(Path.Combine(outDir, "isolation.txt")).Trim()
                : "FAIL isolation missing";
            if (!isolation.StartsWith("PASS", StringComparison.Ordinal))
            {
                SessionState.SetInt(KeyFailed, 1);
            }

            if (!prefsMatch || !filesSame)
            {
                SessionState.SetInt(KeyFailed, 1);
            }

            int exceptions = SessionState.GetInt(KeyEx, 0);
            if (exceptions > 0)
            {
                SessionState.SetInt(KeyFailed, 1);
            }

            bool pass = SessionState.GetInt(KeyFailed, 0) == 0;
            string cases = File.Exists(Path.Combine(outDir, "cases.txt"))
                ? File.ReadAllText(Path.Combine(outDir, "cases.txt"))
                : string.Empty;
            StringBuilder summary = new StringBuilder();
            summary.Append(cases);
            summary.Append(isolation);
            summary.Append('\n');
            summary.Append(prefsMatch ? "prefs MATCH" : "prefs FAIL");
            summary.Append('\n');
            summary.Append(filesSame ? "persistent UNCHANGED" : "persistent FAIL");
            summary.Append('\n');
            summary.Append("exceptions ");
            summary.Append(exceptions.ToString(CultureInfo.InvariantCulture));
            summary.Append('\n');
            summary.Append(pass ? "RESULT PASS" : "RESULT FAIL");
            summary.Append('\n');
            File.WriteAllText(Path.Combine(outDir, "summary.txt"), summary.ToString());
            File.WriteAllText(Path.Combine(outDir, "summary.json"), SummaryJson(cases, isolation, prefsMatch, filesSame, exceptions, pass));
            File.WriteAllText(Path.Combine(outDir, "persistent-after.txt"), afterFiles);
            File.WriteAllText(Path.Combine(outDir, "prefs-after.txt"), afterPrefs);
            Debug.Log("StartChoiceTest summary " + Path.Combine(outDir, "summary.txt"));
            Debug.Log(pass ? "StartChoiceTest PASS" : "StartChoiceTest FAIL");
            SessionState.SetBool(KeyRunning, false);
            Quit(pass ? 0 : 1);
        }

        static void FailRun(string reason)
        {
            string id = "run";
            int index = SessionState.GetInt(KeyCase, 0);
            if (index >= 0 && index < CaseIds.Length)
            {
                id = CaseIds[index];
            }

            Record(id, false, reason);
            SessionState.SetInt(KeyFailed, 1);
            SessionState.SetBool(KeyReboot, false);
            if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.isPlaying = false;
                SetPhase(PhaseLeave);
                SessionState.SetInt(KeyCase, CaseIds.Length);
                return;
            }

            SetPhase(PhaseRestore);
        }

        static bool WriteKnownSave()
        {
            RunSaveData data = new RunSaveData();
            data.Version = RunSaveCodec.CurrentVersion;
            data.WaveIndex = Wave;
            data.Score = 40;
            data.Credits = Credits;
            data.Lives = 3;
            data.Hull = 3;
            data.Shield = Shield;
            data.Difficulty = (int)DifficultyGrade.Normal;
            data.UpgradeMask = Mask;
            data.Doctrine = Doctrine;
            data.PrimaryMode = 0;
            data.UtilityMode = 0;
            data.HasUtility = 0;
            data.RunId = OldRunId;
            data.LastResolvedWave = 2;
            data.LastRunScore = 40;
            data.LastCreditsAwarded = 0;
            data.ExtraLifeStreak = 0;
            data.LegacyHull = 0;
            data.FirstDiscount = 0;
            data.FirstDiscountUsed = 0;
            data.Timestamp = "2026-11-18T12:00:00Z";
            data.BoonLevels = 0;
            data.BoonPending = 0;
            data.BoonOffer = 0;
            data.WaveInProgress = 0;
            data.LivesAtWaveStart = 0;
            data.Mk2Mask = 0;
            data.BankedLegacy = 0;
            data.ExtraLifeWorld = 0;
            data.HullNow = -1;
            data.ShieldNow = -1;
            data.LivesNow = -1;
            data.AssistUsed = 0;
            data.DailySeed = 0;
            data.DailyDate = 0;
            data.MutatorMask = 0;
            return RunSaveStore.TrySaveRun(data);
        }

        static void CorruptMain(string profile)
        {
            File.WriteAllText(Path.Combine(profile, RunSaveStore.RunFileName), "{");
        }

        static void WriteGarbage(string profile)
        {
            File.WriteAllText(Path.Combine(profile, RunSaveStore.RunFileName), "{");
            string bak = Path.Combine(profile, RunSaveStore.RunFileName + ".bak");
            if (File.Exists(bak))
            {
                File.Delete(bak);
            }
        }

        static void ClearProfileSaves(string profile)
        {
            string root = Path.GetFullPath(profile);
            for (int index = 0; index < SaveNames.Length; index++)
            {
                string path = Path.GetFullPath(Path.Combine(root, SaveNames[index]));
                if (!path.StartsWith(root, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException("refusing to delete outside the temp profile");
                }

                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        static string NewProfile(string persistent)
        {
            string stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
            string name = "agr-startchoice-" + stamp + "-" + System.Diagnostics.Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture);
            string first = Path.GetFullPath(Path.Combine(Path.GetTempPath(), name));
            if (ProfileOutsidePersistent(first, persistent))
            {
                return first;
            }

            string second = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "agr-startchoice-outside", name));
            if (ProfileOutsidePersistent(second, persistent))
            {
                return second;
            }

            return null;
        }

        static bool ProfileOutsidePersistent(string profile, string persistent)
        {
            if (string.IsNullOrEmpty(profile) || string.IsNullOrEmpty(persistent))
            {
                return false;
            }

            string fullProfile = Path.GetFullPath(profile).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string fullPersistent = Path.GetFullPath(persistent).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (string.Equals(fullProfile, fullPersistent, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string profilePrefix = fullProfile + Path.DirectorySeparatorChar;
            string persistentPrefix = fullPersistent + Path.DirectorySeparatorChar;
            if (profilePrefix.StartsWith(persistentPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (persistentPrefix.StartsWith(profilePrefix, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        static void PulseKey(KeyCode key)
        {
            MenuNavProbe.Pulse(new int[] { (int)key }, 1, 0f, false, false);
        }

        static void PulseDpad(float x)
        {
            MenuNavProbe.Pulse(null, 0, x, false, false);
        }

        static void PulseSubmit()
        {
            MenuNavProbe.Pulse(null, 0, 0f, true, false);
        }

        static void Advance()
        {
            SessionState.SetInt(KeyStep, SessionState.GetInt(KeyStep, 0) + 1);
            SessionState.SetString(KeyStepStart, Stamp());
        }

        static bool StepTimedOut()
        {
            return Since(KeyStepStart) > StepSeconds;
        }

        static bool StepQuiet()
        {
            return Since(KeyStepStart) > 0.4;
        }

        static int WaitPanel(out string fail)
        {
            fail = null;
            GameUi ui = FindUi();
            if (ui != null && PanelOpen(ui))
            {
                Note("panel visible case=" + _watchCase);
                return 1;
            }

            if (ui != null && PanelResolved(ui) && !PanelOpen(ui))
            {
                fail = "start panel not shown (skipped?)";
                return -1;
            }

            if (StepTimedOut())
            {
                fail = "start panel not shown (skipped?)";
                return -1;
            }

            return 0;
        }

        static GameUi FindUi()
        {
            return UnityEngine.Object.FindAnyObjectByType<GameUi>();
        }

        static bool PanelOpen(GameUi ui)
        {
            return FieldBool(ui, "_startOpen") && !FieldBool(ui, "_startResolved");
        }

        static bool PanelResolved(GameUi ui)
        {
            return FieldBool(ui, "_startResolved");
        }

        static bool ConfirmOpen()
        {
            GameUi ui = FindUi();
            return ui != null && FieldBool(ui, "_startConfirmOpen");
        }

        static bool ContinueEnabled()
        {
            GameUi ui = FindUi();
            if (ui == null)
            {
                return false;
            }

            object view = Field(ui, "_startView");
            if (view == null)
            {
                return false;
            }

            System.Reflection.FieldInfo flag = view.GetType().GetField("ContinueEnabled");
            return flag != null && flag.GetValue(view) is bool && (bool)flag.GetValue(view);
        }

        static bool ContinueInteractable()
        {
            GameUi ui = FindUi();
            if (ui == null)
            {
                return false;
            }

            object button = Field(ui, "_startContinue");
            Button widget = button as Button;
            return widget != null && widget.interactable && widget.gameObject.activeInHierarchy;
        }

        static string ContinueLabel()
        {
            GameUi ui = FindUi();
            if (ui == null)
            {
                return string.Empty;
            }

            object label = Field(ui, "_startContinueLabel");
            Text text = label as Text;
            return text != null ? text.text : string.Empty;
        }

        static int Focus()
        {
            GameUi ui = FindUi();
            return ui == null ? -1 : FieldInt(ui, "_startFocus");
        }

        static int ConfirmFocus()
        {
            GameUi ui = FindUi();
            return ui == null ? -1 : FieldInt(ui, "_startConfirmFocus");
        }

        static int WaveIndex()
        {
            GameManager game = UnityEngine.Object.FindAnyObjectByType<GameManager>();
            if (game == null || game.Session == null)
            {
                return -1;
            }

            return game.Session.WaveIndex;
        }

        static int CreditsNow()
        {
            GameManager game = UnityEngine.Object.FindAnyObjectByType<GameManager>();
            if (game == null || game.Session == null)
            {
                return -1;
            }

            return game.Session.Credits;
        }

        static LoadoutState Loadout()
        {
            PlayerLoadout loadout = UnityEngine.Object.FindAnyObjectByType<PlayerLoadout>();
            return loadout != null ? loadout.State : null;
        }

        static int DoctrineNow()
        {
            LoadoutState state = Loadout();
            return state == null ? -1 : (int)state.Doctrine;
        }

        static int ShieldNow()
        {
            LoadoutState state = Loadout();
            return state == null ? -1 : state.ShieldCharges;
        }

        static bool Owns(UpgradeId id)
        {
            LoadoutState state = Loadout();
            return state != null && state.Owns(id);
        }

        static string ProfilePath()
        {
            string profile = SessionState.GetString(KeyProfile, string.Empty);
            if (profile.Length == 0)
            {
                return string.Empty;
            }

            return Path.GetFullPath(profile);
        }

        static string MainPath()
        {
            return Path.Combine(ProfilePath(), RunSaveStore.RunFileName);
        }

        static string BakPath()
        {
            return MainPath() + ".bak";
        }

        static string SessionPhase()
        {
            GameManager game = UnityEngine.Object.FindAnyObjectByType<GameManager>();
            if (game == null || game.Session == null)
            {
                return "none";
            }

            return game.Session.Phase.ToString();
        }

        static bool TryReadSave(string path, out RunSaveData data)
        {
            data = null;
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return false;
            }

            try
            {
                return RunSaveCodec.TryParse(File.ReadAllText(path), out data) && data != null;
            }
            catch (Exception)
            {
                data = null;
                return false;
            }
        }

        static string DescribeSave(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return "runId=none fileWave=none";
            }

            RunSaveData data;
            if (!TryReadSave(path, out data))
            {
                return "runId=unreadable fileWave=unreadable";
            }

            return "runId=" + data.RunId.ToString(CultureInfo.InvariantCulture)
                + " fileWave=" + data.WaveIndex.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Missing, or a wave-1 file whose run id is not the fixture's.
        /// Yes deletes the old run. The hangar writer then saves the fresh
        /// run, and its .bak, when that run has credits or a purchase.
        /// A starting shield from the untouched prefs counts. Absence still passes.
        /// </summary>
        static bool FileIsFresh(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return true;
            }

            RunSaveData data;
            if (!TryReadSave(path, out data))
            {
                return false;
            }

            return data.WaveIndex == 1 && data.RunId != OldRunId;
        }

        static bool OldRunReplaced()
        {
            return WaveIndex() == 1 && FileIsFresh(MainPath()) && FileIsFresh(BakPath());
        }

        static void LogOverwriteProbe()
        {
            double elapsed = Since(KeyStart);
            if (elapsed - _lastOverwriteLog < 1.0)
            {
                return;
            }

            _lastOverwriteLog = elapsed;
            string main = MainPath();
            string bak = BakPath();
            Note(
                "case 4 wave 1 reached=" + (WaveIndex() == 1).ToString()
                + " wave=" + WaveIndex().ToString(CultureInfo.InvariantCulture)
                + " phase=" + SessionPhase()
                + " credits=" + CreditsNow().ToString(CultureInfo.InvariantCulture)
                + " shield=" + ShieldNow().ToString(CultureInfo.InvariantCulture));
            Note(
                "case 4 main save file gone=" + (!File.Exists(main)).ToString()
                + " path=" + main
                + " " + DescribeSave(main));
            Note(
                "case 4 bak save file gone=" + (!File.Exists(bak)).ToString()
                + " path=" + bak
                + " " + DescribeSave(bak));
        }

        static bool SaveExists()
        {
            return File.Exists(MainPath());
        }

        static bool ExceptionsThisCase()
        {
            return SessionState.GetInt(KeyEx, 0) > SessionState.GetInt(KeyExCase, 0);
        }

        static object Field(object target, string name)
        {
            if (target == null)
            {
                return null;
            }

            System.Reflection.FieldInfo field = target.GetType().GetField(
                name,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            return field != null ? field.GetValue(target) : null;
        }

        static bool FieldBool(object target, string name)
        {
            object value = Field(target, name);
            return value is bool && (bool)value;
        }

        static int FieldInt(object target, string name)
        {
            object value = Field(target, name);
            return value is int ? (int)value : -1;
        }

        static void Record(string id, bool pass, string reason)
        {
            string line = "case " + id + " " + (pass ? "PASS" : "FAIL") + " " + reason;
            Debug.Log("StartChoiceTest " + line);
            Console.Out.Flush();
            AppendRaw(CurrentOut(), line);
        }

        static void OnLog(string condition, string stack, LogType type)
        {
            if (!SessionState.GetBool(KeyRunning, false))
            {
                return;
            }

            if (type != LogType.Exception && type != LogType.Error)
            {
                return;
            }

            string outDir = SessionState.GetString(KeyOut, string.Empty);
            if (outDir.Length > 0)
            {
                File.AppendAllText(
                    Path.Combine(outDir, "unity-log.txt"),
                    type.ToString() + "\t" + condition + "\n" + stack + "\n");
            }

            if (type == LogType.Exception)
            {
                SessionState.SetInt(KeyEx, SessionState.GetInt(KeyEx, 0) + 1);
            }
        }

        static string FingerprintPersistent(string persistent)
        {
            StringBuilder builder = new StringBuilder();
            for (int index = 0; index < SaveNames.Length; index++)
            {
                string path = Path.Combine(persistent, SaveNames[index]);
                builder.Append(SaveNames[index]);
                builder.Append('\t');
                builder.Append(Fingerprint(path));
                builder.Append('\n');
            }

            return builder.ToString();
        }

        static string Fingerprint(string path)
        {
            if (!File.Exists(path))
            {
                return "absent";
            }

            using (FileStream stream = File.OpenRead(path))
            using (System.Security.Cryptography.SHA256 sha = System.Security.Cryptography.SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(stream);
                return "present " + BitConverter.ToString(hash).Replace("-", string.Empty);
            }
        }

        static string ReadPrefsText()
        {
            StringBuilder builder = new StringBuilder();
            string[] rows = PrefRows();
            for (int rowIndex = 0; rowIndex < rows.Length; rowIndex++)
            {
                string[] parts = rows[rowIndex].Split('\t');
                string kind = parts[0];
                string key = parts[1];
                if (!PlayerPrefs.HasKey(key))
                {
                    builder.Append("M\t");
                    builder.Append(key);
                    builder.Append('\n');
                    continue;
                }

                builder.Append(kind);
                builder.Append('\t');
                builder.Append(key);
                builder.Append('\t');
                if (kind == "I")
                {
                    builder.Append(PlayerPrefs.GetInt(key).ToString(CultureInfo.InvariantCulture));
                }
                else if (kind == "F")
                {
                    builder.Append(PlayerPrefs.GetFloat(key).ToString("R", CultureInfo.InvariantCulture));
                }
                else
                {
                    string raw = PlayerPrefs.GetString(key, string.Empty);
                    builder.Append(Convert.ToBase64String(Encoding.UTF8.GetBytes(raw)));
                }

                builder.Append('\n');
            }

            return builder.ToString();
        }

        static void WritePrefsText(string text)
        {
            string[] lines = text.Split('\n');
            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                string line = lines[lineIndex];
                if (line.Length == 0)
                {
                    continue;
                }

                string[] parts = line.Split('\t');
                if (parts.Length < 2)
                {
                    continue;
                }

                string kind = parts[0];
                string key = parts[1];
                if (kind == "M")
                {
                    if (PlayerPrefs.HasKey(key))
                    {
                        PlayerPrefs.DeleteKey(key);
                    }

                    continue;
                }

                if (parts.Length < 3)
                {
                    continue;
                }

                string payload = parts[2];
                if (kind == "I")
                {
                    int number;
                    if (int.TryParse(payload, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
                    {
                        PlayerPrefs.SetInt(key, number);
                    }
                }
                else if (kind == "F")
                {
                    float number;
                    if (float.TryParse(payload, NumberStyles.Float, CultureInfo.InvariantCulture, out number))
                    {
                        PlayerPrefs.SetFloat(key, number);
                    }
                }
                else if (kind == "S")
                {
                    byte[] bytes = Convert.FromBase64String(payload);
                    PlayerPrefs.SetString(key, Encoding.UTF8.GetString(bytes));
                }
            }

            PlayerPrefs.Save();
        }

        static string[] PrefRows()
        {
            return new string[]
            {
                "I\tagr.settings.version",
                "I\tagr.settings.screenShake",
                "I\tagr.settings.hintMode",
                "I\tagr.settings.hintSizeStep",
                "I\tagr.settings.confirmRestartInPlay",
                "I\tagr.settings.confirmRestartNewRun",
                "I\tagr.settings.padNavSource",
                "I\tagr.settings.assistMode",
                "I\tagr.settings.reduceEffects",
                "I\tagr.settings.windowMode",
                "I\tagr.settings.resolutionW",
                "I\tagr.settings.resolutionH",
                "I\tagr.settings.vSync",
                "I\tagr.settings.fpsCap",
                "I\tagr.settings.promptScheme",
                "I\tagr.settings.tutorialCards",
                "I\tagr.settings.filmGrain",
                "I\tagr.ui.firstHangarHint",
                "I\tagr.ui.doctrineHint",
                "I\tagr.audio.mute",
                "I\tagr.audio.curve",
                "I\tagr.daily.played",
                "I\tagr.achievements.mask",
                "I\tagr.hangar.medals",
                "I\tagr.best.score",
                "I\tagr.best.wave",
                "I\tagr.best.world",
                "I\tagr.wave.bestCombo",
                "F\tagr.audio.sfx",
                "F\tagr.audio.music",
                "F\tagr.settings.turnSpeed",
                "F\tagr.wave.bestSeconds",
                "S\tagr.settings.bindings",
                "S\tagr.ui.language",
                "S\tagr.sink.profile",
                "S\tagr.daily.board",
                "S\tagr.difficulty",
            };
        }

        static string SummaryJson(string cases, string isolation, bool prefsMatch, bool filesSame, int exceptions, bool pass)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("{\n  \"result\": \"");
            builder.Append(pass ? "PASS" : "FAIL");
            builder.Append("\",\n  \"isolation\": \"");
            builder.Append(EscapeJson(isolation));
            builder.Append("\",\n  \"prefs\": \"");
            builder.Append(prefsMatch ? "MATCH" : "FAIL");
            builder.Append("\",\n  \"persistent\": \"");
            builder.Append(filesSame ? "UNCHANGED" : "FAIL");
            builder.Append("\",\n  \"exceptions\": ");
            builder.Append(exceptions.ToString(CultureInfo.InvariantCulture));
            builder.Append(",\n  \"cases\": [\n");
            string[] lines = cases.Split('\n');
            bool first = true;
            for (int index = 0; index < lines.Length; index++)
            {
                string line = lines[index];
                if (line.Length == 0)
                {
                    continue;
                }

                if (!first)
                {
                    builder.Append(",\n");
                }

                first = false;
                builder.Append("    \"");
                builder.Append(EscapeJson(line));
                builder.Append('"');
            }

            builder.Append("\n  ]\n}\n");
            return builder.ToString();
        }

        static string EscapeJson(string text)
        {
            if (text == null)
            {
                return string.Empty;
            }

            return text.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", string.Empty);
        }

        static void WriteEarlyFail(string reason)
        {
            string stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
            string outDir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Captures", "StartChoiceTest", stamp));
            Directory.CreateDirectory(outDir);
            string body = "isolation FAIL " + reason + "\nprefs FAIL\npersistent UNCHANGED\nexceptions 0\nRESULT FAIL\n";
            File.WriteAllText(Path.Combine(outDir, "summary.txt"), body);
            File.WriteAllText(Path.Combine(outDir, "summary.json"), "{\"result\":\"FAIL\",\"isolation\":\"" + EscapeJson(reason) + "\"}\n");
            Debug.Log("StartChoiceTest " + reason);
        }

        static void ClearEnv()
        {
            Environment.SetEnvironmentVariable(RunSaveStore.ProfileEnv, null);
            Environment.SetEnvironmentVariable(StartChoice.ForceEnv, null);
        }

        static string Stamp()
        {
            return DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture);
        }

        static double Since(string key)
        {
            string startText = SessionState.GetString(key, string.Empty);
            long start;
            if (!long.TryParse(startText, NumberStyles.Integer, CultureInfo.InvariantCulture, out start) || start <= 0)
            {
                return 0.0;
            }

            return (DateTime.UtcNow.Ticks - start) / (double)TimeSpan.TicksPerSecond;
        }

        static void SetPhase(int phase)
        {
            int prior = SessionState.GetInt(KeyPhase, -1);
            SessionState.SetInt(KeyPhase, phase);
            if (prior != phase)
            {
                Note(
                    "phase " + PhaseName(prior) + " -> " + PhaseName(phase)
                    + " elapsed=" + Since(KeyStart).ToString("0.0", CultureInfo.InvariantCulture));
            }

            RefreshWatch();
        }

        static string PhaseName(int phase)
        {
            if (phase == PhasePrepare)
            {
                return "prepare";
            }

            if (phase == PhaseEnter)
            {
                return "enter";
            }

            if (phase == PhasePlay)
            {
                return "play";
            }

            if (phase == PhaseLeave)
            {
                return "leave";
            }

            if (phase == PhaseNext)
            {
                return "next";
            }

            if (phase == PhaseRestore)
            {
                return "restore";
            }

            return "unknown";
        }

        static void Note(string message)
        {
            string line = "StartChoiceTest " + message;
            Debug.Log(line);
            Console.Out.Flush();
            AppendRaw(CurrentOut(), line);
        }

        static void NoteWait(string what)
        {
            double elapsed = Since(KeyStart);
            if (what == _lastWaitWhat && elapsed - _lastWaitLog < 1.0)
            {
                return;
            }

            _lastWaitLog = elapsed;
            _lastWaitWhat = what;
            Note(
                "wait " + what
                + " phase=" + PhaseName(SessionState.GetInt(KeyPhase, PhasePrepare))
                + " case=" + _watchCase
                + " elapsed=" + elapsed.ToString("0.0", CultureInfo.InvariantCulture)
                + " realtime=" + Time.realtimeSinceStartup.ToString("0.0", CultureInfo.InvariantCulture));
        }

        static string CurrentOut()
        {
            if (!string.IsNullOrEmpty(_watchOut))
            {
                return _watchOut;
            }

            return SessionState.GetString(KeyOut, string.Empty);
        }

        static void AppendRaw(string outDir, string line)
        {
            if (string.IsNullOrEmpty(outDir) || string.IsNullOrEmpty(line))
            {
                return;
            }

            string path = Path.Combine(outDir, "cases.txt");
            using (FileStream stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.WriteLine(line);
                writer.Flush();
                stream.Flush(true);
            }
        }

        static void RefreshWatch()
        {
            _watchOut = SessionState.GetString(KeyOut, string.Empty);
            _watchProfile = SessionState.GetString(KeyProfile, string.Empty);
            _watchPhase = PhaseName(SessionState.GetInt(KeyPhase, PhasePrepare));
            int index = SessionState.GetInt(KeyCase, 0);
            _watchCase = index >= 0 && index < CaseIds.Length ? CaseIds[index] : "run";
            long start;
            long.TryParse(SessionState.GetString(KeyStart, string.Empty), NumberStyles.Integer, CultureInfo.InvariantCulture, out start);
            _deadlineTicks = start > 0 ? start + (long)(TotalSeconds * TimeSpan.TicksPerSecond) : 0;
            long caseStart;
            long.TryParse(SessionState.GetString(KeyCaseStart, string.Empty), NumberStyles.Integer, CultureInfo.InvariantCulture, out caseStart);
            _caseDeadlineTicks = caseStart > 0 ? caseStart + (long)(CaseSeconds * TimeSpan.TicksPerSecond) : _deadlineTicks;
        }

        static void ArmWatchdog()
        {
            _main = SynchronizationContext.Current;
            if (_watchdog != null)
            {
                return;
            }

            _watchdog = new Timer(OnWatchdog, null, 2000, 2000);
        }

        static void OnWatchdog(object state)
        {
            long now = DateTime.UtcNow.Ticks;
            bool total = _deadlineTicks > 0 && now >= _deadlineTicks;
            bool lateCase = _caseDeadlineTicks > 0 && now >= _caseDeadlineTicks;
            if (!total && !lateCase)
            {
                return;
            }

            string why = total ? "timeout total" : "timeout case";
            double elapsed = _deadlineTicks > 0
                ? (now - (_deadlineTicks - (long)(TotalSeconds * TimeSpan.TicksPerSecond))) / (double)TimeSpan.TicksPerSecond
                : 0.0;
            string line = "case " + _watchCase + " FAIL " + why + " phase=" + _watchPhase
                + " elapsed=" + elapsed.ToString("0.0", CultureInfo.InvariantCulture);
            try
            {
                AppendRaw(_watchOut, line);
                AppendRaw(_watchOut, "RESULT FAIL");
            }
            catch (Exception)
            {
            }

            if (Interlocked.Exchange(ref _exitPosted, 1) == 1)
            {
                Environment.Exit(1);
                return;
            }

            SynchronizationContext main = _main;
            if (main != null)
            {
                main.Post(delegate { MainThreadBail(why); }, null);
                return;
            }

            try
            {
                EditorApplication.delayCall += delegate { MainThreadBail(why); };
            }
            catch (Exception)
            {
                Environment.Exit(1);
            }
        }

        static void Timeout(string phase, string why)
        {
            Note("case " + _watchCase + " FAIL " + why + " phase=" + phase + " elapsed=" + Since(KeyStart).ToString("0.0", CultureInfo.InvariantCulture));
            MainThreadBail(why);
        }

        static void MainThreadBail(string why)
        {
            if (Interlocked.Exchange(ref _bailed, 1) == 1)
            {
                return;
            }

            try
            {
                RestoreWarnings();
                string outDir = CurrentOut();
                if (outDir.Length > 0 && File.Exists(Path.Combine(outDir, "prefs-before.txt")))
                {
                    WritePrefsText(File.ReadAllText(Path.Combine(outDir, "prefs-before.txt")));
                }

                ClearEnv();
                RunSaveStore.ProfileRoot = null;
                StartChoice.ForcePanel = false;
                MenuNavProbe.Clear();
                DeleteProfile(_watchProfile);
                if (EditorApplication.isPlaying)
                {
                    EditorApplication.isPlaying = false;
                }

                Note("RESULT FAIL " + why);
            }
            catch (Exception ex)
            {
                Debug.LogError("StartChoiceTest bail " + ex.Message);
            }

            SessionState.SetBool(KeyRunning, false);
            if (_watchdog != null)
            {
                _watchdog.Dispose();
                _watchdog = null;
            }

            EditorApplication.Exit(1);
        }

        static void DeleteProfile(string profile)
        {
            if (string.IsNullOrEmpty(profile) || !Directory.Exists(profile))
            {
                return;
            }

            string persistent = Path.GetFullPath(Application.persistentDataPath);
            if (!ProfileOutsidePersistent(profile, persistent))
            {
                return;
            }

            Directory.Delete(profile, true);
        }

        static void QuietWarnings()
        {
            _savedWarningTrace = Application.GetStackTraceLogType(LogType.Warning);
            _traceSaved = true;
            Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
        }

        static void RestoreWarnings()
        {
            if (!_traceSaved)
            {
                return;
            }

            Application.SetStackTraceLogType(LogType.Warning, _savedWarningTrace);
            _traceSaved = false;
        }

        static void Quit(int code)
        {
            if (Application.isBatchMode)
            {
                EditorApplication.Exit(code);
            }
        }
    }
}
