using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using AsteroidsGoneRogue;

namespace AsteroidsGoneRogue.EditorTools
{
    /// <summary>
    /// Editor batch play of a real run. Entry point:
    /// AsteroidsGoneRogue.EditorTools.LongRun.Run
    /// Does not skip waves, grant credits, or change rules. Not in player builds.
    /// </summary>
    [InitializeOnLoad]
    public static class LongRun
    {
        const string PlayScene = "Assets/Scenes/Play.unity";
        const string KeyRunning = "LongRun.Running";
        const string KeyOut = "LongRun.Out";
        const int ShotCap = 200;
        const int Width = 1920;
        const int Height = 1080;
        const double BootLimitSeconds = 45.0;

        static readonly LongRunPilot.Threat[] Threats = new LongRunPilot.Threat[LongRunPilot.ThreatCap];
        static readonly LongRunPilot.Threat[] Latched = new LongRunPilot.Threat[LongRunPilot.ThreatCap];
        static readonly UnityEngine.EntityId[] SeenIds = new UnityEngine.EntityId[LongRunPilot.ThreatCap];
        static readonly UnityEngine.EntityId[] LatchIds = new UnityEngine.EntityId[LongRunPilot.ThreatCap];
        static readonly float[] PrevX = new float[LongRunPilot.ThreatCap];
        static readonly float[] PrevZ = new float[LongRunPilot.ThreatCap];
        static readonly UnityEngine.EntityId[] PrevIds = new UnityEngine.EntityId[LongRunPilot.ThreatCap];
        static readonly string[] ThreatNames = new string[LongRunPilot.ThreatCap];
        static readonly string[] LatchNames = new string[LongRunPilot.ThreatCap];
        static readonly List<string> PendingLogs = new List<string>();
        static bool _logs;
        static bool _finishing;
        static bool _shooting;

        static LongRun()
        {
            Hook();
        }

        public static void RestorePrefs()
        {
            string backup;
            if (!TryArg("-backup", out backup) || backup.Length == 0)
            {
                Debug.LogError("LongRun FAIL: -backup <path> is required");
                EditorApplication.Exit(1);
                return;
            }

            string report;
            bool matched = LongRunPrefs.RestoreAndVerify(backup, out report);
            Debug.Log("LongRun prefs verify=" + (matched ? "MATCH" : "MISMATCH") + " report=" + report);
            if (!matched)
            {
                Debug.LogError("LongRun FAIL: PlayerPrefs restore mismatch. Report " + report);
            }

            EditorApplication.Exit(matched ? 0 : 1);
        }

        public static void Run()
        {
            string pending = LongRunPrefs.UnrestoredBackup();
            if (pending.Length > 0)
            {
                Debug.LogError("LongRun FAIL: unrestored PlayerPrefs marker " + LongRunPrefs.MarkerPath());
                Debug.LogError(
                    "-executeMethod AsteroidsGoneRogue.EditorTools.LongRun.RestorePrefs -backup "
                    + pending);
                EditorApplication.Exit(1);
                return;
            }

            string error;
            LongRunState state = Parse(out error);
            if (state == null)
            {
                Debug.LogError("LongRun FAIL: " + error);
                EditorApplication.Exit(1);
                return;
            }

            if (SessionState.GetBool(KeyRunning, false))
            {
                Debug.LogError("LongRun FAIL: a long run is already active");
                EditorApplication.Exit(1);
                return;
            }

            try
            {
                Prepare(state);
            }
            catch (Exception bootEx)
            {
                Debug.LogError("LongRun FAIL: " + bootEx.Message);
                SafeRelease(state);
                EditorApplication.Exit(1);
                return;
            }

            _probeFailed = false;
            _probeLogged = false;
            _enterFrame = -1;
            _probe = null;
            _probeHost = null;
            _hostKnown = false;
            _gen = 0;
            _sceneName = string.Empty;
            _lastDiag = string.Empty;
            _diagCount = 0;
            SessionState.SetInt("LongRun.ProbeTries", 0);
            SessionState.SetBool("LongRun.ProbeArmed", false);
            SessionState.SetInt("LongRun.ProbeGen", 0);
            SessionState.SetBool(KeyRunning, true);
            SessionState.SetString(KeyOut, state.OutDir);
            Save(state);
            Hook();
            Application.runInBackground = true;
            EditorSceneManager.OpenScene(PlayScene);
            Debug.Log(
                "LongRun start out=" + state.OutDir
                + " minutes=" + state.Minutes.ToString(CultureInfo.InvariantCulture)
                + " seed=" + state.Seed.ToString(CultureInfo.InvariantCulture)
                + " resumeWorld=" + state.ResumeWorld.ToString(CultureInfo.InvariantCulture)
                + " resumeMinutes=" + state.ResumeMinutes.ToString(CultureInfo.InvariantCulture)
                + " targetWave=" + state.TargetWave.ToString(CultureInfo.InvariantCulture)
                + " progression=" + (state.Skipped != 0 ? "skipped-start" : "natural")
                + " profile=" + state.Profile);
            if (state.Skipped != 0)
            {
                Debug.Log("LongRun DIAGNOSTIC skipped start requested world=" + state.StartWorld.ToString(CultureInfo.InvariantCulture)
                    + " wave=" + state.StartWaveArg.ToString(CultureInfo.InvariantCulture)
                    + " loadout=" + state.Loadout
                    + " not applied; this branch has no start-at-wave control. PR #163 is not included.");
            }

            if (state.Invulnerable != 0)
            {
                Debug.Log("LongRun DIAGNOSTIC invulnerable requested. The harness does not write hull, shield, or invulnerability. This run cannot PASS.");
            }
            EditorApplication.EnterPlaymode();
        }

        static void Hook()
        {
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            if (_logs)
            {
                return;
            }

            Application.logMessageReceived += OnLog;
            Application.logMessageReceivedThreaded += OnNativeLog;
            _logs = true;
        }

        const int UniqueCap = 16;
        static int _nativeErrors;
        static int _nativeExceptions;
        static int _ourErrors;
        static readonly System.Collections.Generic.List<string> _uniqueLogs = new System.Collections.Generic.List<string>();
        static readonly System.Collections.Generic.HashSet<string> _seenLogs = new System.Collections.Generic.HashSet<string>();
        static readonly object _logLock = new object();
        static bool _probeFailed;
        static bool _probeLogged;
        static LongRunDeathLog _probe;
        static GameObject _probeHost;
        static UnityEngine.EntityId _hostId;
        static bool _hostKnown;
        static int _enterFrame = -1;
        static int _gen;
        static string _sceneName = string.Empty;
        static string _lastDiag = string.Empty;
        static int _diagCount;
        static int _seenCount;
        static int _latchedCount;
        static int _prevCount;
        static float _latchAt = -1f;
        static float _prevAt = -1f;
        static float _holdAt = -1f;
        static float _senseAt = -1f;
        static LongRunPilot.Stick _held;
        static string _lastError = string.Empty;
        static long _lastErrorUtc;
        static string _lastNearest = string.Empty;
        static float _lastNearestDist;
        static bool _lastNearestPerceived;
        static string _lastProjectile = "none";
        static float _lastProjectileSpeed;
        static float _menuPulseAt = -1f;
        static int _buyBefore;
        static int _menuWave = -1;
        static int _shopSteps;
        static int _returnSteps;
        static int _hangarRepeats;
        static bool _awaitingBuy;
        static bool _hangarRecovery;
        static int _startExits;
        static int _startHold;
        static string _focusPath = string.Empty;
        static string _lastFocus = string.Empty;
        static string _lastHangarNote = string.Empty;
        static int _creditLast;
        static bool _creditSeen;
        static float _lastNearestSpeed;
        static float _lastShipSpeed;
        static float _playLogAt = -1f;
        static int _combatSig = int.MinValue;
        static long _combatUtc;
        static int _seenEnemies;
        static string _nearKind = "none";
        static float _nearDist = -1f;
        static string _targetId = "none";
        static string _targetKind = "none";
        static float _aimError;
        static string _fireReason = "none";
        static int _waveRemaining = -1;
        static int _pendingSpawns;
        static int _enemiesAll;
        static string _search = "engage";
        static int _shotsSeen;
        static string _evadeMode = "hold";
        static float _standoffDist = -1f;
        static string _prio = "none";
        static string _threatLine = "none";
        static readonly float[] _evadeAt = new float[12];
        static readonly string[] _evadeModeRing = new string[12];
        static readonly float[] _evadeStandoff = new float[12];
        static int _evadeWrite;
        static int _boonOffer = -1;
        static string _lastBoonNote = string.Empty;
        static string _lastUiNote = string.Empty;
        static float _uiLogAt = -1f;

        static void OnNativeLog(string condition, string stackTrace, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert)
            {
                return;
            }

            string message = condition ?? string.Empty;
            string stack = stackTrace ?? string.Empty;
            string key = type.ToString() + "\t" + message;
            bool ours = message.IndexOf("LongRun", System.StringComparison.Ordinal) >= 0
                || stack.IndexOf("LongRunDeathLog", System.StringComparison.Ordinal) >= 0
                || stack.IndexOf("EnsureProbe", System.StringComparison.Ordinal) >= 0;
            lock (_logLock)
            {
                if (type == LogType.Exception)
                {
                    _nativeExceptions += 1;
                }
                else
                {
                    _nativeErrors += 1;
                }

                if (ours)
                {
                    _ourErrors += 1;
                }

                if (_uniqueLogs.Count < UniqueCap && _seenLogs.Add(key))
                {
                    _uniqueLogs.Add(key);
                }
            }
        }

        static void OnLog(string condition, string stackTrace, LogType type)
        {
            if (_shooting || _finishing)
            {
                return;
            }

            if (!SessionState.GetBool(KeyRunning, false))
            {
                return;
            }

            if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert)
            {
                return;
            }

            string message = condition == null ? string.Empty : condition;
            string stack = stackTrace == null ? string.Empty : stackTrace;
            long nowTicks = DateTime.UtcNow.Ticks;
            if (message == _lastError && (nowTicks - _lastErrorUtc) < 2L * TimeSpan.TicksPerSecond)
            {
                return;
            }

            _lastError = message;
            _lastErrorUtc = nowTicks;
            PendingLogs.Add(type.ToString() + "\n" + message + "\n" + stack);
        }

        static void Tick()
        {
            if (_finishing || !SessionState.GetBool(KeyRunning, false))
            {
                return;
            }

            LongRunState state = Load();
            if (state == null)
            {
                _finishing = true;
                SessionState.SetBool(KeyRunning, false);
                Debug.LogError("LongRun FAIL: state file missing");
                EditorApplication.Exit(1);
                return;
            }

            try
            {
                TickBody(state);
            }
            catch (Exception tickEx)
            {
                state.HarnessFault = 1;
                state.Fault = tickEx.ToString();
                state.Phase = "stop";
                state.ExitAsked = 0;
                Save(state);
                Debug.LogError("LongRun FAIL: " + tickEx.Message);
            }
        }

        static void TickBody(LongRunState state)
        {
            FlushLogs(state);
            GamepadInput.SetEditorMenu(0, 0, false, false, false);
            GamepadInput.SetEditorUtility(false);
            if (_startHold > 0)
            {
                GamePhase heldPhase = GamePhase.Hangar;
                GameManager heldGame = GameManager.Live;
                if (heldGame != null && heldGame.Session != null)
                {
                    heldPhase = heldGame.Session.Phase;
                }

                if (heldPhase == GamePhase.Playing || heldPhase == GamePhase.Failed)
                {
                    _startHold = 0;
                }
                else
                {
                    _startHold -= 1;
                    GamepadInput.SetEditorPump(true, 0f, 0f, 0f, 0f, false);
                    GamepadInput.SetEditorMenu(0, 0, false, false, true);
                }
            }
            if (state.ProgressUtc <= 0)
            {
                state.ProgressUtc = DateTime.UtcNow.Ticks;
            }

            GameSession earlySession = GameManager.Live != null ? GameManager.Live.Session : null;
            bool waveLive = EditorApplication.isPlaying && earlySession != null && earlySession.Phase == GamePhase.Playing;
            if (waveLive)
            {
                NoteCombat(state, earlySession);
            }
            else
            {
                _combatSig = int.MinValue;
                _combatUtc = 0L;
            }

            if (!waveLive && state.Watchdog == 0 && Since(state.ProgressUtc) > LongRunVerdict.WatchdogSeconds)
            {
                state.Watchdog = 1;
                state.HarnessFault = 1;
                state.Fault = "watchdog 60s";
                WriteLogTail(state);
                state.Phase = "stop";
                state.ExitAsked = 0;
                Save(state);
                Debug.LogError("LongRun FAIL: watchdog 60s");
            }

            if (state.Phase == "stop")
            {
                Stop(state);
                return;
            }

            if (state.Phase == "leave")
            {
                Leave(state);
                return;
            }

            if (!EditorApplication.isPlaying)
            {
                if (Since(state.WaitUtc) > BootLimitSeconds)
                {
                    state.HarnessFault = 1;
                    state.Fault = "play mode did not start";
                    state.Phase = "stop";
                    Save(state);
                    Debug.LogError("LongRun FAIL: play mode did not start");
                }

                return;
            }

            string probe = EnsureProbe();
            if (probe == "wait")
            {
                if (Since(state.WaitUtc) > BootLimitSeconds)
                {
                    state.HarnessFault = 1;
                    state.Fault = "play not ready";
                    state.Phase = "stop";
                    state.ExitAsked = 0;
                    Save(state);
                    Debug.LogError("LongRun FAIL: play not ready");
                }

                return;
            }

            if (probe.Length > 0)
            {
                _probeFailed = true;
                state.HarnessFault = 1;
                state.Fault = probe;
                state.Phase = "stop";
                state.ExitAsked = 0;
                Save(state);
                Debug.LogError("LongRun FAIL: " + probe);
                return;
            }

            EditorApplication.QueuePlayerLoopUpdate();
            GameManager game = GameManager.Live;
            GameSession session = game != null ? game.Session : null;
            if (game == null || session == null)
            {
                if (Since(state.WaitUtc) > BootLimitSeconds)
                {
                    state.HarnessFault = 1;
                    state.Fault = "GameSession missing";
                    state.Phase = "stop";
                    Save(state);
                    Debug.LogError("LongRun FAIL: GameSession missing");
                }

                return;
            }

            if (state.Phase == "waitResume")
            {
                Resume(state, game, session);
                return;
            }

            if (state.Phase == "overContinue")
            {
                OverContinue(state, game, session);
                return;
            }

            if (state.Phase == "newRunNo")
            {
                NewRunAnswer(state, session, false);
                return;
            }

            if (state.Phase == "newRunYes")
            {
                NewRunAnswer(state, session, true);
                return;
            }

            if (state.Phase == "wait")
            {
                state.Phase = "play";
                state.Settle = 0;
                Save(state);
            }

            if (Elapsed(state) >= state.Minutes * 60.0)
            {
                state.Phase = "stop";
                state.ExitAsked = 0;
                Save(state);
                return;
            }

            Observe(state, session);
            NoteUnknownUi(state, game, session);
            if (state.Phase == "play")
            {
                Menu(state, game, session);
            }

            if (state.Phase == "play" && session.Phase == GamePhase.Playing)
            {
                Steer(state);
            }
            else if (state.Phase == "play")
            {
                GamepadInput.SetEditorPump(true, 0f, 0f, 0f, 0f, false);
            }
            else
            {
                GamepadInput.SetEditorPump(false, 0f, 0f, 0f, 0f, false);
            }

            Save(state);
        }

        static void Leave(LongRunState state)
        {
            GamepadInput.SetEditorPump(false, 0f, 0f, 0f, 0f, false);
            if (EditorApplication.isPlaying)
            {
                if (state.ExitAsked == 0)
                {
                    state.ExitAsked = 1;
                    Save(state);
                    EditorApplication.ExitPlaymode();
                }

                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            state.Phase = "waitResume";
            state.ExitAsked = 0;
            state.WaitUtc = DateTime.UtcNow.Ticks;
            state.Settle = 0;
            Save(state);
            EditorSceneManager.OpenScene(PlayScene);
            EditorApplication.EnterPlaymode();
        }

        static void Stop(LongRunState state)
        {
            GamepadInput.SetEditorPump(false, 0f, 0f, 0f, 0f, false);
            if (EditorApplication.isPlaying)
            {
                if (state.ExitAsked == 0)
                {
                    state.ExitAsked = 1;
                    Save(state);
                    EditorApplication.ExitPlaymode();
                }

                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            RestoreAndReport(state);
        }

        static void Resume(LongRunState state, GameManager game, GameSession session)
        {
            state.Settle += 1;
            if (state.Settle < 8)
            {
                Save(state);
                return;
            }

            if (!game.HasContinueOffer && state.Settle < 9)
            {
                state.ResumeFailed = 1;
                state.Fault = "continue offer missing";
                LogEvent(state, "resume", "resume", WorldCatalog.NumberForWave(session.WaveIndex), session.WaveIndex, session.Credits, session.Lives, ",\"result\":\"FAIL\",\"mismatches\":\"continue offer missing\"");
                state.Phase = "stop";
                state.ExitAsked = 0;
                Save(state);
                return;
            }

            if (game.HasContinueOffer)
            {
                if (state.Settle == 8)
                {
                    PulseMenu(0, 0, true, false, false, true);
                    state.Settle = 9;
                    state.StepUtc = DateTime.UtcNow.Ticks;
                    Save(state);
                    return;
                }

                if (Since(state.StepUtc) <= LongRunVerdict.StepTimeoutSeconds)
                {
                    return;
                }

                state.ResumeFailed = 1;
                state.Fault = "continue confirm timed out";
                LogEvent(state, "resume", "resume", WorldOf(session), WaveOf(session), CreditsOf(session), LivesOf(session), ",\"result\":\"FAIL\",\"mismatches\":\"continue confirm timed out\"");
                state.Phase = "stop";
                state.ExitAsked = 0;
                Save(state);
                return;
            }

            session = game.Session;
            string mismatches = CompareCheckpoint(state, game, session);
            state.ResumeDone = 1;
            if (mismatches.Length > 0)
            {
                state.ResumeFailed = 1;
                state.Fault = mismatches;
                LogEvent(state, "resume", "resume", WorldOf(session), WaveOf(session), CreditsOf(session), LivesOf(session), ",\"result\":\"FAIL\",\"mismatches\":\"" + Json(mismatches) + "\"");
                state.Phase = "stop";
                state.ExitAsked = 0;
                Save(state);
                return;
            }

            LogEvent(state, "resume", "resume", WorldOf(session), WaveOf(session), CreditsOf(session), LivesOf(session), ",\"result\":\"PASS\",\"mismatches\":\"\"");
            state.ProgressUtc = DateTime.UtcNow.Ticks;
            state.Phase = "play";
            state.Settle = 0;
            state.LastPhase = string.Empty;
            Save(state);
        }

        static void OverContinue(LongRunState state, GameManager game, GameSession session)
        {
            if (state.NewRunYes != 0)
            {
                state.Phase = "stop";
                state.ExitAsked = 0;
                Save(state);
                return;
            }

            if (game.HasContinueOffer)
            {
                state.Phase = "waitResume";
                state.Settle = 7;
                state.StepUtc = DateTime.UtcNow.Ticks;
                Save(state);
                return;
            }

            state.OverContinue = 3;
            LogEvent(state, "resume", "game-over-continue", WorldOf(session), WaveOf(session), CreditsOf(session), LivesOf(session), ",\"result\":\"INCOMPLETE\",\"mismatches\":\"FailRun sets PendingContinue null and deletes the run; Continue is not offered after game over\"");
            state.Phase = "newRunNo";
            state.Settle = 0;
            state.StepUtc = DateTime.UtcNow.Ticks;
            state.ProgressUtc = state.StepUtc;
            Save(state);
        }

        static void NewRunAnswer(LongRunState state, GameSession session, bool yes)
        {
            int world = WorldOf(session);
            int wave = WaveOf(session);
            float now = Time.realtimeSinceStartup;
            if (now - _menuPulseAt < 0.35f)
            {
                return;
            }

            _menuPulseAt = now;
            if (!yes)
            {
                PulseMenu(0, 0, false, true, false, false);
                state.NewRunNo = 1;
                LogEvent(state, "transition", "new-run", world, wave, CreditsOf(session), LivesOf(session), ",\"choice\":\"No\",\"input\":\"cancel\"");
                state.Phase = "newRunYes";
                state.StepUtc = DateTime.UtcNow.Ticks;
                state.ProgressUtc = state.StepUtc;
                Save(state);
                return;
            }

            PulseMenu(0, 0, true, false, false, true);
            state.NewRunYes = 1;
            LogEvent(state, "transition", "new-run", world, wave, CreditsOf(session), LivesOf(session), ",\"choice\":\"Yes\",\"input\":\"confirm\"");
            state.Phase = "play";
            state.Settle = 0;
            state.ProgressUtc = DateTime.UtcNow.Ticks;
            Save(state);
        }

        static void NoteLife(LongRunState state, GameSession session, string source)
        {
            state.LifeLogs += 1;
            string maneuver = _held.Maneuver;
            if (string.IsNullOrEmpty(maneuver))
            {
                maneuver = "hold";
            }

            string line = "source=" + source
                + " nearest=" + _lastNearest
                + " distance=" + _lastNearestDist.ToString("0.00", CultureInfo.InvariantCulture)
                + " perceived=" + (_lastNearestPerceived ? "yes" : "no")
                + " maneuver=" + maneuver
                + " projectile=" + _lastProjectile
                + " speed=" + _lastNearestSpeed.ToString("0.00", CultureInfo.InvariantCulture)
                + " shipSpeed=" + _lastShipSpeed.ToString("0.00", CultureInfo.InvariantCulture)
                + " wave=" + WaveOf(session).ToString(CultureInfo.InvariantCulture)
                + " killer=" + _lastNearest
                + " evade=" + maneuver
                + " standoff=" + _standoffDist.ToString("0.0", CultureInfo.InvariantCulture);
            File.AppendAllText(Path.Combine(state.OutDir, "life-loss.txt"), line + "\n");
            float lifeNow = Time.realtimeSinceStartup;
            for (int evadeIndex = 0; evadeIndex < _evadeAt.Length; evadeIndex++)
            {
                if (_evadeAt[evadeIndex] <= 0f || lifeNow - _evadeAt[evadeIndex] > 1f)
                {
                    continue;
                }

                File.AppendAllText(
                    Path.Combine(state.OutDir, "life-loss.txt"),
                    " recent t=" + _evadeAt[evadeIndex].ToString("0.00", CultureInfo.InvariantCulture)
                    + " evade=" + _evadeModeRing[evadeIndex]
                    + " standoff=" + _evadeStandoff[evadeIndex].ToString("0.0", CultureInfo.InvariantCulture)
                    + "\n");
            }
            LogEvent(
                state,
                "life",
                source,
                WorldOf(session),
                WaveOf(session),
                CreditsOf(session),
                LivesOf(session),
                ",\"nearest\":\"" + Json(_lastNearest) + "\",\"distance\":" + _lastNearestDist.ToString("0.00", CultureInfo.InvariantCulture)
                + ",\"perceived\":" + (_lastNearestPerceived ? "true" : "false")
                + ",\"maneuver\":\"" + Json(maneuver) + "\",\"projectile\":\"" + Json(_lastProjectile)
                + "\",\"speed\":" + _lastNearestSpeed.ToString("0.00", CultureInfo.InvariantCulture)
                + ",\"shipSpeed\":" + _lastShipSpeed.ToString("0.00", CultureInfo.InvariantCulture));
        }

        static void Observe(LongRunState state, GameSession session)
        {
            int wave = session.WaveIndex;
            int world = WorldCatalog.NumberForWave(wave);
            int lives = session.Lives;
            string phase = PhaseName(session.Phase);
            if (wave > state.MaxWave)
            {
                state.MaxWave = wave;
            }

            if (world > state.MaxWorld)
            {
                state.MaxWorld = world;
            }

            bool first = state.LastPhase.Length == 0;
            bool phaseChanged = phase != state.LastPhase;
            bool waveChanged = wave != state.LastWave;
            bool worldChanged = world != state.LastWorld;
            bool livesChanged = state.LastLives >= 0 && lives < state.LastLives;
            if (first || phaseChanged || waveChanged || worldChanged || livesChanged)
            {
                string name = phase;
                if (!first && session.Phase == GamePhase.Failed && phaseChanged)
                {
                    name = "game over";
                    state.Deaths += 1;
                    if (Elapsed(state) < state.Minutes * 60.0 && state.TargetWave <= 0)
                    {
                        state.DiedEarly = 1;
                    }

                    if (state.TargetWave > 0 && state.ClearedWaves < state.TargetWave)
                    {
                        state.DiedEarly = 1;
                    }

                    NoteLife(state, session, "game over");
                    state.Phase = "overContinue";
                    state.StepUtc = DateTime.UtcNow.Ticks;
                    state.ExitAsked = 0;
                }
                else if (!first && worldChanged)
                {
                    name = "world";
                }
                else if (!first && waveChanged)
                {
                    name = "wave";
                }
                else if (!first && livesChanged && session.Phase == GamePhase.Playing)
                {
                    name = "continue";
                    state.Continues += 1;
                    NoteLife(state, session, "continue");
                }

                if (!first && wave < state.LastWave)
                {
                    state.ClearedWaves = 0;
                }

                if (!first && wave > state.LastWave && state.LastWave >= 1)
                {
                    state.ClearedWaves = wave - 1;
                }

                LogEvent(state, "transition", name, world, wave, session.Credits, lives, "");
                state.ProgressUtc = DateTime.UtcNow.Ticks;
                if (phaseChanged || waveChanged)
                {
                    state.Settle = 0;
                }
            }

            state.LastPhase = phase;
            state.LastWave = wave;
            state.LastWorld = world;
            state.LastLives = lives;
            NoteCredits(state, session, "session");
            if (state.TargetWave > 0 && state.ClearedWaves >= state.TargetWave && state.Diagnostic == 0)
            {
                state.Phase = "stop";
                state.ExitAsked = 0;
                LogEvent(state, "transition", "target-wave", world, wave, session.Credits, lives, "");
            }
        }

        static void Menu(LongRunState state, GameManager game, GameSession session)
        {
            if (session.Phase == GamePhase.Playing || session.Phase == GamePhase.Failed)
            {
                return;
            }

            state.Settle += 1;
            if (game.ShowDifficultyChooser)
            {
                if (state.Settle < 5)
                {
                    return;
                }

                if (Time.realtimeSinceStartup - _menuPulseAt < 0.35f)
                {
                    return;
                }

                Seed(state);
                PulseMenu(0, 0, false, false, true, false);
                LogEvent(state, "transition", "hangar", WorldOf(session), WaveOf(session), session.Credits, session.Lives, ",\"choice\":\"normal\",\"input\":\"start\"");
                state.Settle = 0;
                return;
            }

            if (game.TutorialActive)
            {
                return;
            }

            if (game.BoonChoiceOpen)
            {
                if (state.Settle < 8)
                {
                    return;
                }

                if (Time.realtimeSinceStartup - _menuPulseAt < 0.35f)
                {
                    return;
                }

                if (_boonOffer < 0 && game.Boons != null)
                {
                    _boonOffer = game.Boons.OfferAt(0);
                }

                string boonFocus = FocusName();
                bool boonHit = PulseMenu(0, 0, true, false, false, false);
                NoteBoon(state, session, "confirm-left", boonFocus, OpenPanels(game), boonHit, _boonOffer);
                return;
            }

            if (_boonOffer >= 0)
            {
                NoteBoon(state, session, "chosen", "BoonCard0", "none", true, _boonOffer);
                LogEvent(
                    state,
                    "transition",
                    "boon",
                    WorldOf(session),
                    session.WaveIndex,
                    session.Credits,
                    session.Lives,
                    ",\"offer\":" + _boonOffer.ToString(CultureInfo.InvariantCulture) + ",\"pick\":0");
                state.BoonWave = session.WaveIndex;
                _boonOffer = -1;
            }

            if (state.Settle < 12)
            {
                return;
            }

            if (_menuWave != session.WaveIndex)
            {
                _menuWave = session.WaveIndex;
                _shopSteps = 0;
                _returnSteps = 0;
                _hangarRepeats = 0;
                _awaitingBuy = false;
                _buyBefore = 0;
                _hangarRecovery = false;
                _startExits = 0;
                _focusPath = string.Empty;
                _lastFocus = string.Empty;
            }

            if (Time.realtimeSinceStartup - _menuPulseAt < 0.35f)
            {
                return;
            }

            if (_awaitingBuy && _buyBefore > 0 && session.Credits < _buyBefore)
            {
                NotePurchase(state, session, "focused-slot", _buyBefore);
                _awaitingBuy = false;
                _buyBefore = 0;
                state.BoughtWave = session.WaveIndex;
            }

            string focusName = FocusName();
            string control = HangarControl(focusName);
            string panels = OpenPanels(game);
            if (state.BoughtWave != session.WaveIndex && session.Credits > 0 && _shopSteps < 8)
            {
                _shopSteps += 1;
                if (focusName.StartsWith("Buy_", System.StringComparison.Ordinal) && !_awaitingBuy)
                {
                    _buyBefore = session.Credits;
                    _awaitingBuy = true;
                    bool bought = PulseMenu(0, 0, true, false, false, true);
                    NoteHangar(state, session, "confirm-buy", focusName, control, panels, bought);
                    return;
                }

                _awaitingBuy = false;
                int navX = (_shopSteps % 2) == 0 ? 1 : 0;
                int navY = navX == 0 ? -1 : 0;
                PulseMenu(navX, navY, false, false, false, false);
                NoteHangar(state, session, navX == 0 ? "nav-down" : "nav-right", focusName, control, panels, false);
                return;
            }

            if (state.BoughtWave != session.WaveIndex)
            {
                state.BoughtWave = session.WaveIndex;
            }

            if (ReadyToResume(state, session))
            {
                File.WriteAllText(Path.Combine(state.OutDir, "checkpoint.txt"), Snapshot(game, session));
                state.Phase = "leave";
                state.ExitAsked = 0;
                state.ResumeReached = 1;
                LogEvent(state, "transition", "save", WorldOf(session), session.WaveIndex, session.Credits, session.Lives, "");
                return;
            }

            if (focusName != "Primary")
            {
                _startExits += 1;
                int towardPrimary = SideCycle(focusName) ? -1 : 1;
                PulseMenu(0, towardPrimary, false, false, true, false);
                _startHold = 8;
                NoteHangar(state, session, "start-exit", focusName, control, panels, false);
                if (_startExits >= 6)
                {
                    NoteHangar(state, session, "fail", focusName, control, panels, false);
                    state.HarnessFault = 1;
                    state.Fault = "hangar input repeated without a phase change";
                    state.Phase = "stop";
                    state.ExitAsked = 0;
                    Save(state);
                    Debug.LogError("LongRun FAIL: hangar input repeated without a phase change");
                }

                return;
            }

            Seed(state);
            bool hit = PulseMenu(0, 0, true, false, false, true);
            _hangarRepeats += 1;
            state.ContinueChecks += 1;
            NoteHangar(state, session, "continue-reachable", focusName, control, panels, hit);
            NoteCredits(state, session, focusName);
            if (session.Credits == 0 && state.ContinueChecks == 1)
            {
                LogEvent(state, "transition", "zero-credits", WorldOf(session), session.WaveIndex, 0, session.Lives, ",\"reachable\":true,\"input\":\"confirm\"");
            }
        }

        static bool PulseMenu(int navX, int navY, bool confirm, bool cancel, bool start, bool submit)
        {
            _menuPulseAt = Time.realtimeSinceStartup;
            GamepadInput.SetEditorPump(true, 0f, 0f, 0f, 0f, false);
            GamepadInput.SetEditorMenu(navX, navY, confirm, cancel, start);
            if (start)
            {
                _startHold = 8;
            }
            if (!submit || !confirm)
            {
                return false;
            }

            UnityEngine.EventSystems.EventSystem events = UnityEngine.EventSystems.EventSystem.current;
            if (events == null || events.currentSelectedGameObject == null)
            {
                return false;
            }

            UnityEngine.EventSystems.BaseEventData data = new UnityEngine.EventSystems.BaseEventData(events);
            return UnityEngine.EventSystems.ExecuteEvents.Execute(
                events.currentSelectedGameObject,
                data,
                UnityEngine.EventSystems.ExecuteEvents.submitHandler);
        }

        static void NoteCombat(LongRunState state, GameSession session)
        {
            if (state.Watchdog != 0)
            {
                return;
            }

            int enemies = 0;
            EnemySeeker[] seekers = UnityEngine.Object.FindObjectsByType<EnemySeeker>();
            if (seekers != null)
            {
                enemies = seekers.Length;
            }

            int rocks = 0;
            Asteroid[] stones = UnityEngine.Object.FindObjectsByType<Asteroid>();
            if (stones != null)
            {
                rocks = stones.Length;
            }

            ShipHealth health = UnityEngine.Object.FindAnyObjectByType<ShipHealth>();
            int hull = health != null ? health.Hull : 0;
            int shield = health != null ? health.Shield : 0;
            _enemiesAll = enemies;
            WaveManager waves = UnityEngine.Object.FindAnyObjectByType<WaveManager>();
            _waveRemaining = waves != null ? waves.RemainingThreats : -1;
            int entering = 0;
            if (seekers != null)
            {
                for (int seekerIndex = 0; seekerIndex < seekers.Length; seekerIndex++)
                {
                    if (seekers[seekerIndex] != null && seekers[seekerIndex].IsEntering)
                    {
                        entering += 1;
                    }
                }
            }

            _pendingSpawns = entering;
            int sig = session.WaveIndex * 100000 + _waveRemaining * 1000 + enemies * 50 + rocks + hull * 13 + shield * 17 + entering;
            long now = DateTime.UtcNow.Ticks;
            if (sig != _combatSig || _combatUtc <= 0L)
            {
                _combatSig = sig;
                _combatUtc = now;
            }

            if (Since(_combatUtc) <= LongRunVerdict.CombatWatchdogSeconds)
            {
                return;
            }

            state.Watchdog = 1;
            state.HarnessFault = 1;
            state.Fault = "watchdog no-combat-progress";
            WriteLogTail(state);
            state.Phase = "stop";
            state.ExitAsked = 0;
            Save(state);
            Debug.LogError("LongRun FAIL: watchdog no-combat-progress");
        }

        static void RememberAim(ShipController ship, LongRunPilot.Stick stick)
        {
            _fireReason = string.IsNullOrEmpty(stick.FireReason) ? "none" : stick.FireReason;
            _search = "engage";
            _seenEnemies = 0;
            _nearKind = "none";
            _nearDist = -1f;
            _targetId = "none";
            _targetKind = "none";
            Vector3 pos = ship.transform.position;
            float best = 1000000f;
            for (int index = 0; index < _seenCount; index++)
            {
                if (Threats[index].Kind == LongRunPilot.KindShot)
                {
                    continue;
                }

                string name = ThreatNames[index] != null ? ThreatNames[index] : string.Empty;
                if (name.StartsWith("Enemy", System.StringComparison.Ordinal) || name.StartsWith("Brute", System.StringComparison.Ordinal))
                {
                    _seenEnemies += 1;
                }

                float dx = Threats[index].X - pos.x;
                float dz = Threats[index].Z - pos.z;
                float dist = (float)System.Math.Sqrt((dx * dx) + (dz * dz));
                if (dist >= best)
                {
                    continue;
                }

                best = dist;
                _nearDist = dist;
                _nearKind = name.Length == 0 ? "body" : name;
                _targetId = SeenIds[index].ToString();
                _targetKind = Threats[index].Kind == LongRunPilot.KindLarge ? "large" : "body";
            }

            Vector3 fwd = ship.transform.forward;
            float forwardLen = (float)System.Math.Sqrt((fwd.x * fwd.x) + (fwd.z * fwd.z));
            float dot = 1f;
            if (forwardLen > 0.01f)
            {
                dot = ((fwd.x * stick.AimX) + (fwd.z * stick.AimY)) / forwardLen;
            }

            if (dot > 1f)
            {
                dot = 1f;
            }

            if (dot < -1f)
            {
                dot = -1f;
            }

            _aimError = (float)System.Math.Acos(dot) * 57.2958f;
            float bestShot = 1000000f;
            _threatLine = "none";
            for (int shotIndex = 0; shotIndex < _seenCount; shotIndex++)
            {
                if (Threats[shotIndex].Kind != LongRunPilot.KindShot)
                {
                    continue;
                }

                float sdx = Threats[shotIndex].X - pos.x;
                float sdz = Threats[shotIndex].Z - pos.z;
                float sdist = (float)System.Math.Sqrt((sdx * sdx) + (sdz * sdz));
                if (sdist >= bestShot)
                {
                    continue;
                }

                bestShot = sdist;
                float speedSq = (Threats[shotIndex].Vx * Threats[shotIndex].Vx) + (Threats[shotIndex].Vz * Threats[shotIndex].Vz);
                float tti = speedSq > 0.25f ? sdist / (float)System.Math.Sqrt(speedSq) : -1f;
                _threatLine = SeenIds[shotIndex].ToString()
                    + "/" + sdist.ToString("0.0", CultureInfo.InvariantCulture)
                    + "/" + tti.ToString("0.00", CultureInfo.InvariantCulture);
            }

            _search = stick.Maneuver == "patrol" ? "patrol" : "engage";
            int shotCount = 0;
            for (int countShot = 0; countShot < _seenCount; countShot++)
            {
                if (Threats[countShot].Kind == LongRunPilot.KindShot)
                {
                    shotCount += 1;
                }
            }

            _shotsSeen = shotCount;
            _evadeMode = string.IsNullOrEmpty(stick.Maneuver) ? "hold" : stick.Maneuver;
            _standoffDist = stick.StandoffDist;
            _prio = string.IsNullOrEmpty(stick.Prio) ? "none" : stick.Prio;
            _evadeAt[_evadeWrite] = Time.realtimeSinceStartup;
            _evadeModeRing[_evadeWrite] = _evadeMode;
            _evadeStandoff[_evadeWrite] = _standoffDist;
            _evadeWrite += 1;
            if (_evadeWrite >= _evadeAt.Length)
            {
                _evadeWrite = 0;
            }
        }

        static void NotePlay(LongRunState state, ShipController ship, float moveX, float moveY, bool fire)
        {
            float now = Time.realtimeSinceStartup;
            if (now - _playLogAt < 1f)
            {
                return;
            }

            _playLogAt = now;
            string id = "null";
            string pos = "?,?";
            if (ship != null)
            {
                id = ship.GetEntityId().ToString();
                Vector3 at = ship.transform.position;
                pos = at.x.ToString("0.0", CultureInfo.InvariantCulture) + "," + at.z.ToString("0.0", CultureInfo.InvariantCulture);
            }

            string line = "LongRun play phase=Playing ship=" + id
                + " input=" + moveX.ToString("0.00", CultureInfo.InvariantCulture)
                + "," + moveY.ToString("0.00", CultureInfo.InvariantCulture)
                + " fire=" + (fire ? "1" : "0")
                + " timeScale=" + Time.timeScale.ToString("0.00", CultureInfo.InvariantCulture)
                + " pos=" + pos
                + " enemies=" + _seenEnemies.ToString(CultureInfo.InvariantCulture)
                + " enemiesAll=" + _enemiesAll.ToString(CultureInfo.InvariantCulture)
                + " waveRemaining=" + _waveRemaining.ToString(CultureInfo.InvariantCulture)
                + " pendingSpawns=" + _pendingSpawns.ToString(CultureInfo.InvariantCulture)
                + " search=" + _search
                + " shots=" + _shotsSeen.ToString(CultureInfo.InvariantCulture)
                + " near=" + _nearKind + ":" + _nearDist.ToString("0.0", CultureInfo.InvariantCulture)
                + " target=" + _targetId + ":" + _targetKind
                + " aimErr=" + _aimError.ToString("0", CultureInfo.InvariantCulture)
                + " fireReason=" + _fireReason
                + " threat=" + _threatLine
                + " evade=" + _evadeMode
                + " standoff=" + _standoffDist.ToString("0.0", CultureInfo.InvariantCulture)
                + " prio=" + _prio;
            Debug.Log(line);
            if (state != null && state.OutDir.Length > 0)
            {
                File.AppendAllText(Path.Combine(state.OutDir, "play-input.txt"), line + "\n");
            }
        }

        static string FocusName()
        {
            UnityEngine.EventSystems.EventSystem events = UnityEngine.EventSystems.EventSystem.current;
            if (events == null || events.currentSelectedGameObject == null)
            {
                return "null";
            }

            return events.currentSelectedGameObject.name;
        }

        static string HangarControl(string focusName)
        {
            if (focusName == "null")
            {
                return "none";
            }

            if (focusName == "Primary")
            {
                return "next-wave";
            }

            if (focusName.StartsWith("Buy_", System.StringComparison.Ordinal))
            {
                return "shop";
            }

            if (focusName == "ExtraLife" || focusName == "HullRepair" || focusName == "ShieldRefill" || focusName == "Bank")
            {
                return "service";
            }

            if (focusName == "NewRun")
            {
                return "new-run";
            }

            return "other";
        }

        static string OpenPanels(GameManager game)
        {
            StringBuilder panels = new StringBuilder();
            if (game != null && game.BoonChoiceOpen)
            {
                panels.Append("boon ");
            }

            if (game != null && game.ShowDifficultyChooser)
            {
                panels.Append("difficulty ");
            }

            if (game != null && game.TutorialActive)
            {
                panels.Append("tutorial ");
            }

            if (PanelActive("ConfirmRoot"))
            {
                panels.Append("confirm ");
            }

            if (PanelActive("SettingsRoot"))
            {
                panels.Append("settings ");
            }

            if (PanelActive("RunSetupCanvas"))
            {
                panels.Append("setup ");
            }

            if (PanelActive("BoonRoot"))
            {
                panels.Append("boon-root ");
            }

            if (PanelActive("DoctrineIntro"))
            {
                panels.Append("doctrine-intro ");
            }

            if (PanelActive("FirstHangarHint"))
            {
                panels.Append("hangar-hint ");
            }

            if (panels.Length == 0)
            {
                return "none";
            }

            return panels.ToString().Trim();
        }

        static bool PanelActive(string name)
        {
            GameObject panel = GameObject.Find(name);
            return panel != null && panel.activeInHierarchy;
        }

        static void NoteBoon(LongRunState state, GameSession session, string action, string focusName, string panels, bool handlerHit, int offer)
        {
            string line = "action=" + action
                + " focus=" + focusName
                + " control=boon"
                + " panels=" + panels
                + " phase=" + PhaseName(session.Phase)
                + " handler=" + (handlerHit ? "hit" : "miss")
                + " credits=" + session.Credits.ToString(CultureInfo.InvariantCulture)
                + " wave=" + session.WaveIndex.ToString(CultureInfo.InvariantCulture)
                + " world=" + WorldOf(session).ToString(CultureInfo.InvariantCulture)
                + " offer=" + offer.ToString(CultureInfo.InvariantCulture)
                + " pick=0";
            if (line == _lastBoonNote)
            {
                return;
            }

            _lastBoonNote = line;
            Debug.Log("LongRun boon " + line);
            if (state.OutDir.Length > 0)
            {
                File.AppendAllText(Path.Combine(state.OutDir, "boon-input.txt"), line + "\n");
            }
        }

        static void NoteUnknownUi(LongRunState state, GameManager game, GameSession session)
        {
            if (session == null || session.Phase == GamePhase.Playing)
            {
                return;
            }

            float now = Time.realtimeSinceStartup;
            if (now - _uiLogAt < 2f)
            {
                return;
            }

            _uiLogAt = now;
            string focusName = FocusName();
            string panels = game != null ? OpenPanels(game) : "none";
            string control = HangarControl(focusName);
            bool named = panels != "none" || control == "next-wave" || control == "shop" || control == "service";
            string action = named ? "ui" : "unknown-panel";
            string line = "action=" + action
                + " focus=" + focusName
                + " control=" + (named ? control : "unknown")
                + " panels=" + panels
                + " phase=" + PhaseName(session.Phase)
                + " handler=miss"
                + " credits=" + session.Credits.ToString(CultureInfo.InvariantCulture)
                + " wave=" + session.WaveIndex.ToString(CultureInfo.InvariantCulture)
                + " world=" + WorldOf(session).ToString(CultureInfo.InvariantCulture);
            if (line == _lastUiNote)
            {
                return;
            }

            _lastUiNote = line;
            Debug.Log("LongRun ui " + line);
            if (state.OutDir.Length > 0)
            {
                File.AppendAllText(Path.Combine(state.OutDir, "ui-state.txt"), line + "\n");
            }
        }

        static void NoteHangar(LongRunState state, GameSession session, string action, string focusName, string control, string panels, bool handlerHit)
        {
            if (_focusPath.Length == 0)
            {
                _focusPath = focusName;
            }
            else if (focusName != _lastFocus && _focusPath.Length < 180)
            {
                _focusPath = _focusPath + ">" + focusName;
            }

            _lastFocus = focusName;

            string line = "action=" + action
                + " focus=" + focusName
                + " control=" + control
                + " path=" + _focusPath
                + " panels=" + panels
                + " phase=" + PhaseName(session.Phase)
                + " handler=" + (handlerHit ? "hit" : "miss")
                + " credits=" + session.Credits.ToString(CultureInfo.InvariantCulture)
                + " wave=" + session.WaveIndex.ToString(CultureInfo.InvariantCulture);
            if (line == _lastHangarNote)
            {
                return;
            }

            _lastHangarNote = line;
            Debug.Log("LongRun hangar " + line);
            File.AppendAllText(Path.Combine(state.OutDir, "hangar-input.txt"), line + "\n");
            NoteCredits(state, session, focusName);
        }

        static bool SideCycle(string focusName)
        {
            return focusName == "OpenCredits"
                || focusName == "DiffEasy"
                || focusName == "DiffNormal"
                || focusName == "DiffHard";
        }

        static void NoteCredits(LongRunState state, GameSession session, string source)
        {
            if (session == null)
            {
                return;
            }

            int now = session.Credits;
            if (_creditSeen && now != _creditLast)
            {
                LogEvent(
                    state,
                    "credits",
                    source,
                    WorldOf(session),
                    session.WaveIndex,
                    now,
                    session.Lives,
                    ",\"before\":" + _creditLast.ToString(CultureInfo.InvariantCulture)
                    + ",\"after\":" + now.ToString(CultureInfo.InvariantCulture)
                    + ",\"delta\":" + (now - _creditLast).ToString(CultureInfo.InvariantCulture));
            }

            _creditSeen = true;
            _creditLast = now;
        }

        static void NotePurchase(LongRunState state, GameSession session, string item, int before)
        {
            int after = session.Credits;
            state.Purchases += 1;
            LogEvent(
                state,
                "purchase",
                "shop",
                WorldOf(session),
                session.WaveIndex,
                after,
                session.Lives,
                ",\"item\":\"" + Json(item) + "\",\"cost\":" + (before - after).ToString(CultureInfo.InvariantCulture)
                + ",\"price\":" + (before - after).ToString(CultureInfo.InvariantCulture)
                + ",\"creditsBefore\":" + before.ToString(CultureInfo.InvariantCulture)
                + ",\"creditsAfter\":" + after.ToString(CultureInfo.InvariantCulture));
            state.ProgressUtc = DateTime.UtcNow.Ticks;
        }

        static bool ReadyToResume(LongRunState state, GameSession session)
        {
            if (state.ResumeDone != 0 || state.ResumeReached != 0)
            {
                return false;
            }

            if (session.Phase != GamePhase.WaveClear && session.Phase != GamePhase.Hangar)
            {
                return false;
            }

            bool worldHit = state.ResumeWorld > 0
                && WorldCatalog.NumberForWave(session.WaveIndex) > state.ResumeWorld;
            bool timeHit = state.ResumeMinutes > 0
                && session.WaveIndex > 1
                && Elapsed(state) >= state.ResumeMinutes * 60.0;
            return worldHit || timeHit;
        }

        static void Steer(LongRunState state)
        {
            ShipController ship = UnityEngine.Object.FindAnyObjectByType<ShipController>();
            if (ship == null)
            {
                GamepadInput.SetEditorPump(true, 0f, 0f, 0f, 0f, false);
                NotePlay(state, null, 0f, 0f, false);
                return;
            }

            Vector3 pos = ship.transform.position;
            float now = Time.realtimeSinceStartup;
            if (state != null)
            {
                float movedX = pos.x - state.LastShipX;
                float movedZ = pos.z - state.LastShipZ;
                if ((movedX * movedX) + (movedZ * movedZ) > 0.25f || state.LastShipX > 100000f)
                {
                    state.LastShipX = pos.x;
                    state.LastShipZ = pos.z;
                    state.ProgressUtc = DateTime.UtcNow.Ticks;
                }
            }

            Vector3 fwd = ship.transform.forward;
            Camera camera = PlayCamera();
            _seenCount = 0;
            _seenCount = CollectAsteroids(camera, pos, _seenCount, now);
            _seenCount = CollectEnemies(camera, pos, _seenCount, now);
            _seenCount = CollectBolts(camera, pos, _seenCount, now);
            RememberMotion(now);
            if (_senseAt < 0f)
            {
                _senseAt = now;
            }

            if (_latchAt < 0f || now - _latchAt >= LongRunPilot.ReactionSeconds)
            {
                if (now - _senseAt >= LongRunPilot.ReactionSeconds)
                {
                    _latchedCount = _seenCount;
                    for (int index = 0; index < _seenCount; index++)
                    {
                        Latched[index] = Threats[index];
                        LatchIds[index] = SeenIds[index];
                    }

                    _latchAt = now;
                }
            }

            float edge = VisibleEdge(camera, pos);
            bool hold = _latchAt < 0f || now - _holdAt < LongRunPilot.InputHoldSeconds;
            LongRunPilot.Stick stick = _held;
            if (!hold)
            {
                stick = LongRunPilot.Decide(pos.x, pos.z, fwd.x, fwd.z, Latched, _latchedCount, edge);
                _held = stick;
                _holdAt = now;
            }

            if (string.IsNullOrEmpty(stick.Maneuver))
            {
                stick.Maneuver = "hold";
            }

            _held.Maneuver = stick.Maneuver;
            GamepadInput.SetEditorPump(true, stick.MoveX, stick.MoveY, stick.AimX, stick.AimY, stick.Fire);
            GamepadInput.SetEditorUtility(stick.Utility);
            RememberAim(ship, stick);
            NotePlay(state, ship, stick.MoveX, stick.MoveY, stick.Fire);
            Rigidbody hull = ship.GetComponent<Rigidbody>();
            if (hull != null)
            {
                float shipSpeed = hull.linearVelocity.magnitude;
                if (shipSpeed > 0.05f)
                {
                    _lastShipSpeed = shipSpeed;
                }
            }
            LongRunDeathLog.NoteChosen(stick.MoveX, stick.MoveY, stick.AimX, stick.AimY, stick.Fire);
            NoteNearest(pos);
        }

        static int CollectAsteroids(Camera camera, Vector3 pos, int count, float now)
        {
            Asteroid[] found = UnityEngine.Object.FindObjectsByType<Asteroid>();
            if (found == null)
            {
                return count;
            }

            for (int index = 0; index < found.Length; index++)
            {
                Asteroid rock = found[index];
                if (rock == null || !OnScreen(camera, rock.transform.position))
                {
                    continue;
                }

                Vector3 at = rock.transform.position;
                float radius = VisibleRadius(rock);
                int kind = radius >= 2.2f ? LongRunPilot.KindLarge : LongRunPilot.KindBody;
                count = Keep(pos, at.x, at.z, 0f, 0f, radius, kind, rock.GetEntityId(), rock.name, count, now);
            }

            return count;
        }

        static int CollectEnemies(Camera camera, Vector3 pos, int count, float now)
        {
            EnemySeeker[] found = UnityEngine.Object.FindObjectsByType<EnemySeeker>();
            if (found == null)
            {
                return count;
            }

            for (int index = 0; index < found.Length; index++)
            {
                EnemySeeker enemy = found[index];
                if (enemy == null || !OnScreen(camera, enemy.transform.position))
                {
                    continue;
                }

                Vector3 at = enemy.transform.position;
                float radius = VisibleRadius(enemy);
                int kind = radius >= 1.6f ? LongRunPilot.KindLarge : LongRunPilot.KindBody;
                count = Keep(pos, at.x, at.z, 0f, 0f, radius, kind, enemy.GetEntityId(), enemy.name, count, now);
            }

            return count;
        }

        static int CollectBolts(Camera camera, Vector3 pos, int count, float now)
        {
            Projectile[] found = UnityEngine.Object.FindObjectsByType<Projectile>();
            if (found == null)
            {
                return count;
            }

            for (int index = 0; index < found.Length; index++)
            {
                Projectile bolt = found[index];
                if (bolt == null || !bolt.Hostile || !OnScreen(camera, bolt.transform.position))
                {
                    continue;
                }

                Vector3 at = bolt.transform.position;
                count = Keep(pos, at.x, at.z, 0f, 0f, 0.4f, LongRunPilot.KindShot, bolt.GetEntityId(), bolt.name, count, now);
            }

            return count;
        }

        static int Keep(Vector3 pos, float x, float z, float vx, float vz, float radius, int kind, UnityEngine.EntityId id, string name, int count, float now)
        {
            if (count < LongRunPilot.ThreatCap)
            {
                Threats[count].X = x;
                Threats[count].Z = z;
                Threats[count].Vx = vx;
                Threats[count].Vz = vz;
                Threats[count].Radius = radius;
                Threats[count].Kind = kind;
                Threats[count].Role = LongRunPilot.RoleOf(name, kind);
                SeenIds[count] = id;
                ThreatNames[count] = name ?? string.Empty;
                return count + 1;
            }

            int farthest = 0;
            float farSq = -1f;
            for (int index = 0; index < count; index++)
            {
                float dx = Threats[index].X - pos.x;
                float dz = Threats[index].Z - pos.z;
                float distSq = (dx * dx) + (dz * dz);
                if (distSq > farSq)
                {
                    farSq = distSq;
                    farthest = index;
                }
            }

            float nextX = x - pos.x;
            float nextZ = z - pos.z;
            float nextSq = (nextX * nextX) + (nextZ * nextZ);
            if (nextSq < farSq)
            {
                Threats[farthest].X = x;
                Threats[farthest].Z = z;
                Threats[farthest].Vx = vx;
                Threats[farthest].Vz = vz;
                Threats[farthest].Radius = radius;
                Threats[farthest].Kind = kind;
                Threats[farthest].Role = LongRunPilot.RoleOf(name, kind);
                SeenIds[farthest] = id;
                ThreatNames[farthest] = name ?? string.Empty;
            }

            return count;
        }

        static void RememberMotion(float now)
        {
            float dt = now - _prevAt;
            if (dt < 0.001f)
            {
                dt = 0.001f;
            }

            if (_prevAt >= 0f)
            {
                for (int index = 0; index < _seenCount; index++)
                {
                    for (int prev = 0; prev < _prevCount; prev++)
                    {
                        if (!SeenIds[index].Equals(PrevIds[prev]))
                        {
                            continue;
                        }

                        Threats[index].Vx = (Threats[index].X - PrevX[prev]) / dt;
                        Threats[index].Vz = (Threats[index].Z - PrevZ[prev]) / dt;
                        break;
                    }
                }
            }

            _prevCount = _seenCount;
            for (int copy = 0; copy < _seenCount; copy++)
            {
                PrevIds[copy] = SeenIds[copy];
                PrevX[copy] = Threats[copy].X;
                PrevZ[copy] = Threats[copy].Z;
            }

            _prevAt = now;
        }

        static void NoteNearest(Vector3 pos)
        {
            _lastNearest = "none";
            _lastNearestDist = -1f;
            _lastNearestPerceived = false;
            _lastProjectile = "none";
            _lastProjectileSpeed = 0f;
            _lastNearestSpeed = 0f;
            float best = 1000000f;
            float bestShot = 1000000f;
            for (int index = 0; index < _seenCount; index++)
            {
                float dx = Threats[index].X - pos.x;
                float dz = Threats[index].Z - pos.z;
                float dist = (float)System.Math.Sqrt((dx * dx) + (dz * dz));
                if (dist < best)
                {
                    best = dist;
                    _lastNearest = ThreatNames[index];
                    _lastNearestDist = dist;
                    _lastNearestPerceived = WasLatched(SeenIds[index]);
                    float svx = Threats[index].Vx;
                    float svz = Threats[index].Vz;
                    _lastNearestSpeed = (float)System.Math.Sqrt((svx * svx) + (svz * svz));
                }

                if (Threats[index].Kind == LongRunPilot.KindShot && dist < bestShot)
                {
                    bestShot = dist;
                    _lastProjectile = ThreatNames[index];
                    _lastProjectileSpeed = (float)System.Math.Sqrt((Threats[index].Vx * Threats[index].Vx) + (Threats[index].Vz * Threats[index].Vz));
                }
            }
        }

        static bool WasLatched(UnityEngine.EntityId id)
        {
            for (int index = 0; index < _latchedCount; index++)
            {
                if (LatchIds[index].Equals(id))
                {
                    return true;
                }
            }

            return false;
        }

        static Camera PlayCamera()
        {
            Camera main = Camera.main;
            if (main != null)
            {
                return main;
            }

            Camera[] cameras = Camera.allCameras;
            if (cameras == null)
            {
                return null;
            }

            for (int index = 0; index < cameras.Length; index++)
            {
                Camera camera = cameras[index];
                if (camera != null && camera.enabled && camera.name != "DecorCamera")
                {
                    return camera;
                }
            }

            return null;
        }

        static bool OnScreen(Camera camera, Vector3 world)
        {
            if (camera == null)
            {
                return false;
            }

            Vector3 view = camera.WorldToViewportPoint(world);
            return view.z > 0f && view.x >= 0f && view.x <= 1f && view.y >= 0f && view.y <= 1f;
        }

        static float VisibleRadius(Component body)
        {
            Collider collider = body.GetComponent<Collider>();
            if (collider == null)
            {
                return 0.6f;
            }

            Vector3 extents = collider.bounds.extents;
            float radius = extents.x > extents.z ? extents.x : extents.z;
            if (radius < 0.2f)
            {
                radius = 0.2f;
            }

            return radius;
        }

        static float VisibleEdge(Camera camera, Vector3 pos)
        {
            if (camera == null)
            {
                return 30f;
            }

            Vector3 view = camera.WorldToViewportPoint(pos);
            float left = view.x;
            float right = 1f - view.x;
            float down = view.y;
            float up = 1f - view.y;
            float margin = left;
            if (right < margin)
            {
                margin = right;
            }

            if (down < margin)
            {
                margin = down;
            }

            if (up < margin)
            {
                margin = up;
            }

            if (margin < 0f)
            {
                margin = 0f;
            }

            return margin * 20f;
        }

        static void Seed(LongRunState state)
        {
            if (state.Seed < 0)
            {
                return;
            }

            UnityEngine.Random.InitState(state.Seed);
        }

        static string EnsureProbe()
        {
            if (!ProbeReady())
            {
                return "wait";
            }

            string why = InspectProbe();
            if (why.Length == 0)
            {
                ArmProbe(SessionState.GetInt("LongRun.ProbeTries", 0) + 1);
                return string.Empty;
            }

            bool armed = SessionState.GetBool("LongRun.ProbeArmed", false);
            if (armed || why != "component null")
            {
                Diag(why);
            }

            bool free = armed
                || why == "domain reload occurred"
                || why == "active scene changed"
                || why == "host object changed"
                || why == "destroyed (Unity-null but C# ref alive)";
            return AttachProbe(!free);
        }

        static bool ProbeReady()
        {
            if (!EditorApplication.isPlaying)
            {
                return false;
            }

            if (_enterFrame < 0)
            {
                _enterFrame = Time.frameCount;
            }

            if (Time.frameCount < _enterFrame + 10)
            {
                return false;
            }

            UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return false;
            }

            GameManager game = GameManager.Live;
            if (game == null || game.Session == null)
            {
                return false;
            }

            GamePhase phase = game.Session.Phase;
            return phase == GamePhase.Hangar
                || phase == GamePhase.Playing
                || phase == GamePhase.WaveClear
                || phase == GamePhase.Failed
                || phase == GamePhase.CampaignClear;
        }

        static string InspectProbe()
        {
            int savedGen = SessionState.GetInt("LongRun.ProbeGen", 0);
            if (savedGen > 0 && _gen == 0)
            {
                _gen = savedGen;
                _probe = null;
                _probeHost = null;
                _hostKnown = false;
                return "domain reload occurred";
            }

            if (ReferenceEquals(_probe, null))
            {
                return "component null";
            }

            if (_probe == null)
            {
                return "destroyed (Unity-null but C# ref alive)";
            }

            UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (_sceneName.Length > 0 && scene.name != _sceneName)
            {
                _sceneName = scene.name;
                return "active scene changed";
            }

            if (_hostKnown && !_probe.gameObject.GetEntityId().Equals(_hostId))
            {
                return "host object changed";
            }

            if (!_probe.Recording)
            {
                _probe.enabled = false;
                _probe.enabled = true;
                if (_probe.Recording)
                {
                    return string.Empty;
                }

                return "Recording=false";
            }

            return string.Empty;
        }

        static string AttachProbe(bool countFailure)
        {
            int fails = SessionState.GetInt("LongRun.ProbeTries", 0);
            if (countFailure && fails >= 2)
            {
                return ProbeReason("budget-exhausted", fails);
            }

            int attempt = fails + 1;
            try
            {
                if (ReferenceEquals(_probeHost, null) || _probeHost == null)
                {
                    _probeHost = new GameObject("LongRunFrameProbe");
                    _probeHost.hideFlags = HideFlags.HideInHierarchy;
                    UnityEngine.Object.DontDestroyOnLoad(_probeHost);
                }

                if (_probeHost == null)
                {
                    if (countFailure)
                    {
                        SessionState.SetInt("LongRun.ProbeTries", attempt);
                    }

                    return ProbeReason("found=null", attempt);
                }

                if (_probeHost.GetComponent<LongRunFrameProbe>() == null)
                {
                    _probeHost.AddComponent<LongRunFrameProbe>();
                }

                _probe = _probeHost.GetComponent<LongRunDeathLog>();
                if (_probe == null)
                {
                    _probe = _probeHost.AddComponent<LongRunDeathLog>();
                }

                if (_probe == null)
                {
                    if (countFailure)
                    {
                        SessionState.SetInt("LongRun.ProbeTries", attempt);
                    }

                    return ProbeReason("addcomponent-null", attempt);
                }

                if (!_probe.Recording)
                {
                    _probe.enabled = false;
                    _probe.enabled = true;
                }

                if (!_probe.Recording)
                {
                    if (countFailure)
                    {
                        SessionState.SetInt("LongRun.ProbeTries", attempt);
                    }

                    return ProbeReason("found-not-recording", attempt);
                }

                _sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                _hostId = _probeHost.GetEntityId();
                _hostKnown = true;
                _gen = 1;
                SessionState.SetInt("LongRun.ProbeGen", 1);
                SessionState.SetBool("LongRun.ProbeArmed", true);
                ArmProbe(attempt);
                return string.Empty;
            }
            catch (System.Exception attachEx)
            {
                if (countFailure)
                {
                    SessionState.SetInt("LongRun.ProbeTries", attempt);
                }

                return ProbeReason("addcomponent-exception:" + attachEx.Message, attempt);
            }
        }

        static void Diag(string why)
        {
            if (why == _lastDiag || _diagCount >= 8)
            {
                return;
            }

            _lastDiag = why;
            _diagCount += 1;
            Debug.Log("LongRun recorder diag " + why);
        }

        static void ArmProbe(int attempt)
        {
            LongRunTrace.Armed = true;
            LongRunTrace.Write = LongRunDeathLog.Append;
            if (_probeLogged)
            {
                return;
            }

            _probeLogged = true;
            Debug.Log("LongRun death recorder recording " + ProbeReason("ok", attempt));
        }

        static string ProbeReason(string code, int attempt)
        {
            UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            bool sceneLoaded = scene.IsValid() && scene.isLoaded;
            return code
                + " attempt=" + attempt.ToString(CultureInfo.InvariantCulture)
                + " isPlaying=" + EditorApplication.isPlaying.ToString()
                + " frameCount=" + Time.frameCount.ToString(CultureInfo.InvariantCulture)
                + " sceneLoaded=" + sceneLoaded.ToString();
        }

        static void Prepare(LongRunState state)
        {
            Directory.CreateDirectory(state.OutDir);
            Directory.CreateDirectory(state.Profile);
            Directory.CreateDirectory(state.Backup);
            Directory.CreateDirectory(Path.Combine(state.OutDir, "shots"));
            if (!LongRunSaveGuard.ProfileIsIsolated(state.Live, state.Profile))
            {
                throw new IOException("test profile is not isolated from " + state.Live);
            }

            string[] hashes = new string[LongRunSaveGuard.SaveNames.Length];
            for (int index = 0; index < LongRunSaveGuard.SaveNames.Length; index++)
            {
                string name = LongRunSaveGuard.SaveNames[index];
                hashes[index] = LongRunSaveGuard.CopyAndVerify(
                    Path.Combine(state.Live, name),
                    Path.Combine(state.Backup, name));
            }

            LongRunSaveGuard.WriteManifest(Path.Combine(state.Backup, "manifest.txt"), LongRunSaveGuard.SaveNames, hashes);
            LongRunPrefs.Capture(Path.Combine(state.Backup, "prefs.json"));
            Environment.SetEnvironmentVariable(LongRunSaveGuard.EnvProfile, state.Profile);
        }

        static void SafeRelease(LongRunState state)
        {
            Environment.SetEnvironmentVariable(LongRunSaveGuard.EnvProfile, null);
            if (state == null)
            {
                return;
            }

            string prefsPath = Path.Combine(state.Backup, "prefs.json");
            if (File.Exists(prefsPath))
            {
                try
                {
                    string bootReport;
                    if (!LongRunPrefs.RestoreAndVerify(prefsPath, out bootReport))
                    {
                        Debug.LogError("LongRun prefs restore mismatch " + bootReport);
                    }
                }
                catch (Exception restoreEx)
                {
                    Debug.LogError("LongRun prefs restore failed: " + restoreEx.Message);
                }
            }
        }

        static void RestoreAndReport(LongRunState state)
        {
            if (_finishing)
            {
                return;
            }

            _finishing = true;
            LongRunTrace.Armed = false;
            LongRunTrace.Write = null;
            GamepadInput.SetEditorPump(false, 0f, 0f, 0f, 0f, false);
            Environment.SetEnvironmentVariable(LongRunSaveGuard.EnvProfile, null);
            string saveDetail;
            bool savesOk = LongRunSaveGuard.ManifestMatchesLive(
                Path.Combine(state.Backup, "manifest.txt"),
                state.Live,
                out saveDetail);
            bool prefsOk = true;
            string prefsDetail = string.Empty;
            try
            {
                string prefsReport;
                prefsOk = LongRunPrefs.RestoreAndVerify(Path.Combine(state.Backup, "prefs.json"), out prefsReport);
                prefsDetail = prefsOk ? prefsReport : "MISMATCH " + prefsReport;
            }
            catch (Exception restoreEx)
            {
                prefsOk = false;
                prefsDetail = restoreEx.Message;
            }

            if (!savesOk || !prefsOk)
            {
                state.SaveFailed = 1;
                state.Fault = saveDetail + " " + prefsDetail;
            }

            NoteDiagnostics(state);
            LongRunFrames.Flush();
            FlushLogs(state);
            string verdict = VerdictOf(state);
            string summaryPath = Path.Combine(state.OutDir, "summary.md");
            File.WriteAllText(summaryPath, Summary(state, verdict, savesOk, saveDetail, prefsOk, prefsDetail));
            WriteMachineSummary(state, verdict, savesOk, prefsOk);
            SessionState.SetBool(KeyRunning, false);
            Debug.Log("LongRun verdict=" + verdict + " summary=" + summaryPath + " elapsed=" + Elapsed(state).ToString("0.0", CultureInfo.InvariantCulture));
            if (verdict != "PASS")
            {
                Debug.LogError("LongRun verdict=" + verdict);
            }

            int code = 0;
            if (verdict == "FAIL")
            {
                code = 1;
            }
            else if (verdict == "INCOMPLETE")
            {
                code = 2;
            }
            else if (verdict == "DIAGNOSTIC")
            {
                code = 3;
            }

            EditorApplication.Exit(code);
        }

        static void NoteDiagnostics(LongRunState state)
        {
            int losses = state.Continues + state.Deaths;
            int files = 0;
            if (Directory.Exists(state.OutDir))
            {
                files = Directory.GetFiles(state.OutDir, "death-*.txt").Length;
            }

            if (_probeFailed || _ourErrors > 0 || (losses > 0 && files == 0))
            {
                state.HarnessFault = 1;
                if (state.Fault.Length == 0)
                {
                    state.Fault = "diagnostics failed";
                }
            }
        }

        static string VerdictOf(LongRunState state)
        {
            bool resumeConfigured = state.ResumeWorld > 0 || state.ResumeMinutes > 0;
            bool resumeNotRun = resumeConfigured && state.ResumeReached == 0;
            return LongRunVerdict.ClassifyCampaign(
                state.SaveFailed != 0,
                state.ResumeFailed != 0,
                state.HarnessFault != 0,
                state.Diagnostic != 0,
                state.TargetWave,
                state.ClearedWaves,
                Elapsed(state) + 0.05 >= state.Minutes * 60.0,
                LongRunVerdict.Progressed(state.MaxWave),
                state.DiedEarly != 0,
                resumeNotRun);
        }

        static string Summary(LongRunState state, string verdict, bool savesOk, string saveDetail, bool prefsOk, string prefsDetail)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("# Long run\n\n");
            builder.Append("- Verdict: ");
            builder.Append(verdict);
            builder.Append("\n- Elapsed seconds (harness wall clock): ");
            builder.Append(Elapsed(state).ToString("0.0", CultureInfo.InvariantCulture));
            builder.Append("\n- Real elapsed seconds (realtimeSinceStartup): ");
            builder.Append(LongRunFrames.RealElapsedSeconds.ToString("0.00", CultureInfo.InvariantCulture));
            builder.Append("\n- Playing seconds (realtimeSinceStartup): ");
            builder.Append(LongRunFrames.PlayingSeconds.ToString("0.00", CultureInfo.InvariantCulture));
            builder.Append("\n- Frame sum seconds: ");
            builder.Append(LongRunFrames.SumSeconds.ToString("0.00", CultureInfo.InvariantCulture));
            builder.Append("\n- Progression: ");
            builder.Append(state.Skipped != 0 ? "skipped start (diagnostic, not applied)" : "natural");
            builder.Append("\n- Target wave: ");
            builder.Append(state.TargetWave.ToString(CultureInfo.InvariantCulture));
            builder.Append("\n- Cleared waves: ");
            builder.Append(state.ClearedWaves.ToString(CultureInfo.InvariantCulture));
            builder.Append("\n- Budget seconds: ");
            builder.Append((state.Minutes * 60).ToString(CultureInfo.InvariantCulture));
            builder.Append("\n- World: ");
            builder.Append(state.MaxWorld.ToString(CultureInfo.InvariantCulture));
            builder.Append("\n- Wave: ");
            builder.Append(state.MaxWave.ToString(CultureInfo.InvariantCulture));
            builder.Append("\n- Purchases: ");
            builder.Append(state.Purchases.ToString(CultureInfo.InvariantCulture));
            builder.Append("\n- Deaths: ");
            builder.Append((state.Deaths + state.Continues).ToString(CultureInfo.InvariantCulture));
            builder.Append("\n- Lives lost and continued: ");
            builder.Append(state.Continues.ToString(CultureInfo.InvariantCulture));
            builder.Append("\n- Out of lives: ");
            builder.Append(state.Deaths.ToString(CultureInfo.InvariantCulture));
            builder.Append("\n- Seed: ");
            builder.Append(state.Seed.ToString(CultureInfo.InvariantCulture));
            builder.Append("\n- Resume: ");
            if (state.ResumeReached == 0)
            {
                builder.Append("not reached");
            }
            else if (state.ResumeFailed != 0)
            {
                builder.Append("FAIL ");
                builder.Append(state.Fault);
            }
            else
            {
                builder.Append("PASS");
            }

            builder.Append("\n- Save files: ");
            builder.Append(savesOk ? "untouched" : "CHANGED " + saveDetail);
            builder.Append("\n- PlayerPrefs restore: ");
            builder.Append(prefsOk ? "MATCH " + prefsDetail : "FAIL " + prefsDetail);
            builder.Append("\n- Live save dir: ");
            builder.Append(state.Live);
            builder.Append("\n- Test profile: ");
            builder.Append(state.Profile);
            builder.Append("\n- Backup: ");
            builder.Append(state.Backup);
            builder.Append("\n- Frametime avg ms: ");
            builder.Append((LongRunFrames.Average() * 1000f).ToString("0.00", CultureInfo.InvariantCulture));
            builder.Append("\n- Frametime p95 ms: ");
            builder.Append((LongRunFrames.Percentile(0.95f) * 1000f).ToString("0.00", CultureInfo.InvariantCulture));
            builder.Append("\n- Frametime p99 ms: ");
            builder.Append((LongRunFrames.Percentile(0.99f) * 1000f).ToString("0.00", CultureInfo.InvariantCulture));
            builder.Append("\n- Frametime max ms: ");
            builder.Append((LongRunFrames.Max * 1000f).ToString("0.00", CultureInfo.InvariantCulture));
            builder.Append("\n- Hitches over 50 ms: ");
            builder.Append(LongRunFrames.Hitches.ToString(CultureInfo.InvariantCulture));
            builder.Append("\n- Frames: ");
            builder.Append(LongRunFrames.Count.ToString(CultureInfo.InvariantCulture));
            if (LongRunFrames.PlayingSeconds > 0.05 && LongRunFrames.Count > 0)
            {
                double rate = LongRunFrames.Count / LongRunFrames.PlayingSeconds;
                builder.Append("\n- Batchmode player-loop rate: ");
                builder.Append(rate.ToString("0.0", CultureInfo.InvariantCulture));
                builder.Append(" frames/s (frames / Playing seconds; not display FPS)");
            }
            builder.Append("\n- Exceptions and error logs: ");
            builder.Append((state.Exceptions + state.Errors).ToString(CultureInfo.InvariantCulture));
            builder.Append("\n- Native errors: ");
            builder.Append(_nativeErrors.ToString(CultureInfo.InvariantCulture));
            builder.Append("\n- Native exceptions: ");
            builder.Append(_nativeExceptions.ToString(CultureInfo.InvariantCulture));
            builder.Append("\n- Diagnostics errors: ");
            builder.Append(_ourErrors.ToString(CultureInfo.InvariantCulture));
            for (int nativeIndex = 0; nativeIndex < _uniqueLogs.Count; nativeIndex++)
            {
                builder.Append("\n- Native log: ");
                builder.Append(_uniqueLogs[nativeIndex].Replace("\r", " ").Replace("\n", " "));
            }
            builder.Append("\n- Watchdog: ");
            if (state.Watchdog != 0 && state.Fault.IndexOf("no-combat-progress", System.StringComparison.Ordinal) >= 0)
            {
                builder.Append("watchdog no-combat-progress 90s");
            }
            else if (state.Watchdog != 0)
            {
                builder.Append(state.Fault.Length == 0 ? "watchdog 60s" : state.Fault);
            }
            else
            {
                builder.Append("menus within 60s and combat within 90s");
            }
            builder.Append("\n- Log: ");
            builder.Append(Path.Combine(state.OutDir, "events.jsonl"));
            if (state.Fault.Length > 0 && verdict == "FAIL")
            {
                builder.Append("\n- Fault: ");
                builder.Append(state.Fault.Replace("\r", " ").Replace("\n", " "));
            }

            builder.Append("\n\nAutopilot does not test human fun.\n");
            return builder.ToString();
        }

        static void WriteMachineSummary(LongRunState state, string verdict, bool savesOk, bool prefsOk)
        {
            StringBuilder json = new StringBuilder();
            StringBuilder text = new StringBuilder();
            json.Append("{\n  \"verdict\": \"");
            json.Append(verdict);
            json.Append("\",\n  \"progression\": \"");
            json.Append(state.Skipped != 0 ? "skipped start" : "natural");
            json.Append("\",\n  \"goals\": [\n");
            text.Append("verdict ");
            text.Append(verdict);
            text.Append("\nprogression ");
            text.Append(state.Skipped != 0 ? "skipped start" : "natural");
            text.Append('\n');
            int losses = state.Continues + state.Deaths;
            string lifeResult = "INCOMPLETE";
            string lifeReason = "no life loss";
            if (losses > 0 && state.LifeLogs >= losses)
            {
                lifeResult = "PASS";
                lifeReason = "nearest threat, perception, maneuver, projectile, and speed logged";
            }
            else if (losses > 0)
            {
                lifeResult = "FAIL";
                lifeReason = "a life loss was not logged";
            }

            string levelResult = "INCOMPLETE";
            string levelReason = "no -targetWave";
            if (state.TargetWave > 0 && state.Diagnostic != 0)
            {
                levelResult = "DIAGNOSTIC";
                levelReason = "diagnostic flags cannot PASS";
            }
            else if (state.TargetWave > 0 && state.ClearedWaves >= state.TargetWave)
            {
                levelResult = "PASS";
                levelReason = "cleared " + state.ClearedWaves.ToString(CultureInfo.InvariantCulture) + " waves";
            }
            else if (state.TargetWave > 0)
            {
                levelReason = "cleared " + state.ClearedWaves.ToString(CultureInfo.InvariantCulture) + " of " + state.TargetWave.ToString(CultureInfo.InvariantCulture);
            }

            string overResult = "INCOMPLETE";
            string overReason = "game over not reached";
            if (state.OverContinue == 1)
            {
                overResult = "PASS";
                overReason = "continue after game over matched";
            }
            else if (state.OverContinue == 2)
            {
                overResult = "FAIL";
                overReason = "continue after game over mismatched";
            }
            else if (state.OverContinue == 3)
            {
                overReason = "continue offer missing after game over";
            }

            string newRunResult = state.NewRunNo != 0 && state.NewRunYes != 0 ? "PASS" : "INCOMPLETE";
            string newRunReason = state.NewRunNo != 0 && state.NewRunYes != 0
                ? "No is Cancel, Yes is Confirm on the focused control"
                : "new run Yes/No not reached";
            bool first = true;
            string watchReason = "menus within 60s and combat within 90s";
            if (state.Watchdog != 0)
            {
                watchReason = state.Fault;
                if (watchReason.IndexOf("no-combat-progress", System.StringComparison.Ordinal) >= 0)
                {
                    watchReason = "watchdog no-combat-progress 90s";
                }
                else if (watchReason.Length == 0)
                {
                    watchReason = "watchdog 60s";
                }
            }

            Goal(json, text, ref first, "watchdog", state.Watchdog != 0 ? "FAIL" : "PASS", watchReason);
            Goal(json, text, ref first, "level-10", levelResult, levelReason);
            Goal(json, text, ref first, "life-loss", lifeResult, lifeReason);
            Goal(json, text, ref first, "invulnerable", state.Invulnerable != 0 ? "DIAGNOSTIC" : "PASS", state.Invulnerable != 0 ? "flag set; hull and shield were not written" : "not used");
            Goal(json, text, ref first, "skipped-start", state.Skipped != 0 ? "DIAGNOSTIC" : "PASS", state.Skipped != 0 ? "requested and not applied; PR #163 is not on this branch" : "natural progression");
            Goal(json, text, ref first, "resume", state.ResumeFailed != 0 ? "FAIL" : (state.ResumeDone != 0 ? "PASS" : "INCOMPLETE"), state.ResumeDone != 0 ? "world, wave, lives, credits, upgrades, hull, shield, seed compared" : "resume not run");
            Goal(json, text, ref first, "game-over-continue", overResult, overReason);
            Goal(json, text, ref first, "new-run", newRunResult, newRunReason);
            Goal(json, text, ref first, "purchases", state.ContinueChecks > 0 ? "PASS" : "INCOMPLETE", state.ContinueChecks > 0 ? "price and credits logged; continue reachable at 0 credits" : "no hangar continue");
            Goal(json, text, ref first, "prefs", savesOk && prefsOk ? "PASS" : "FAIL", savesOk && prefsOk ? "MATCH" : "save or prefs mismatch");
            json.Append("\n  ]\n}\n");
            text.Append("logCapBytes ");
            text.Append(LongRunVerdict.LogCapBytes.ToString(CultureInfo.InvariantCulture));
            text.Append("\nperceptionSeconds ");
            text.Append(LongRunPilot.ReactionSeconds.ToString("0.00", CultureInfo.InvariantCulture));
            text.Append("\ninputHoldSeconds ");
            text.Append(LongRunPilot.InputHoldSeconds.ToString("0.00", CultureInfo.InvariantCulture));
            text.Append('\n');
            File.WriteAllText(Path.Combine(state.OutDir, "summary.json"), json.ToString());
            File.WriteAllText(Path.Combine(state.OutDir, "summary.txt"), text.ToString());
        }

        static void Goal(StringBuilder json, StringBuilder text, ref bool first, string id, string result, string reason)
        {
            if (!first)
            {
                json.Append(",\n");
            }

            first = false;
            json.Append("    {\"id\": \"");
            json.Append(id);
            json.Append("\", \"result\": \"");
            json.Append(result);
            json.Append("\", \"reason\": \"");
            json.Append(Json(reason));
            json.Append("\"}");
            text.Append(id);
            text.Append(' ');
            text.Append(result);
            text.Append(' ');
            text.Append(reason);
            text.Append('\n');
        }

        static void FlushLogs(LongRunState state)
        {
            if (PendingLogs.Count == 0 || state == null || state.OutDir.Length == 0)
            {
                return;
            }

            int take = PendingLogs.Count;
            if (take > 8)
            {
                take = 8;
            }

            for (int index = 0; index < take; index++)
            {
                string raw = PendingLogs[0];
                PendingLogs.RemoveAt(0);
                string[] parts = raw.Split(new char[] { '\n' }, 3);
                string kind = parts.Length > 0 ? parts[0] : "Error";
                string message = parts.Length > 1 ? parts[1] : string.Empty;
                string stack = parts.Length > 2 ? parts[2] : string.Empty;
                if (kind == "Exception")
                {
                    state.Exceptions += 1;
                }
                else
                {
                    state.Errors += 1;
                }

                if (message.Length > 400)
                {
                    message = message.Substring(0, 400);
                }

                if (stack.Length > 800)
                {
                    stack = stack.Substring(0, 800);
                }

                LogEvent(state, "error", kind, state.MaxWorld, state.MaxWave, 0, 0, ",\"message\":\"" + Json(message) + "\",\"stack\":\"" + Json(stack) + "\"");
            }

            Save(state);
        }

        static void LogEvent(LongRunState state, string kind, string name, int world, int wave, int credits, int lives, string extra)
        {
            string shot = TakeShot(state);
            StringBuilder builder = new StringBuilder();
            builder.Append("{\"t\":");
            builder.Append(Elapsed(state).ToString("0.000", CultureInfo.InvariantCulture));
            builder.Append(",\"kind\":\"");
            builder.Append(Json(kind));
            builder.Append("\",\"name\":\"");
            builder.Append(Json(name));
            builder.Append("\",\"world\":");
            builder.Append(world.ToString(CultureInfo.InvariantCulture));
            builder.Append(",\"wave\":");
            builder.Append(wave.ToString(CultureInfo.InvariantCulture));
            builder.Append(",\"credits\":");
            builder.Append(credits.ToString(CultureInfo.InvariantCulture));
            builder.Append(",\"lives\":");
            builder.Append(lives.ToString(CultureInfo.InvariantCulture));
            builder.Append(",\"shot\":\"");
            builder.Append(Json(shot));
            builder.Append('"');
            if (!string.IsNullOrEmpty(extra))
            {
                builder.Append(extra);
            }

            builder.Append("}\n");
            string line = builder.ToString();
            int bytes = Encoding.UTF8.GetByteCount(line);
            if (state.LogBytes > LongRunVerdict.LogCapBytes - bytes)
            {
                if (state.LogCapped == 0)
                {
                    state.LogCapped = 1;
                    Debug.Log("LongRun log cap " + LongRunVerdict.LogCapBytes.ToString(CultureInfo.InvariantCulture));
                }

                return;
            }

            state.LogBytes += bytes;
            File.AppendAllText(Path.Combine(state.OutDir, "events.jsonl"), line);
        }

        static void WriteLogTail(LongRunState state)
        {
            string source = Path.Combine(state.OutDir, "events.jsonl");
            string tail = string.Empty;
            if (File.Exists(source))
            {
                using (FileStream stream = File.Open(source, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    int keep = 65536;
                    if (stream.Length < keep)
                    {
                        keep = (int)stream.Length;
                    }

                    if (keep > 0)
                    {
                        stream.Seek(-keep, SeekOrigin.End);
                        byte[] buffer = new byte[keep];
                        int read = stream.Read(buffer, 0, keep);
                        tail = Encoding.UTF8.GetString(buffer, 0, read);
                    }
                }
            }

            File.WriteAllText(Path.Combine(state.OutDir, "log-tail.txt"), tail);
        }

        static string TakeShot(LongRunState state)
        {
            if (state.Shot >= ShotCap || !EditorApplication.isPlaying)
            {
                return string.Empty;
            }

            state.Shot += 1;
            string relative = "shots/" + state.Shot.ToString("0000", CultureInfo.InvariantCulture) + ".png";
            string path = Path.Combine(state.OutDir, relative.Replace('/', Path.DirectorySeparatorChar));
            _shooting = true;
            try
            {
                if (!WritePng(path))
                {
                    return string.Empty;
                }
            }
            catch (Exception shotEx)
            {
                Debug.LogWarning("LongRun shot failed: " + shotEx.Message);
                return string.Empty;
            }
            finally
            {
                _shooting = false;
            }

            return relative;
        }

        static bool WritePng(string path)
        {
            Camera[] cameras = UnityEngine.Object.FindObjectsByType<Camera>();
            int usable = 0;
            if (cameras != null)
            {
                for (int index = 0; index < cameras.Length; index++)
                {
                    Camera cam = cameras[index];
                    if (cam != null && cam.enabled && cam.targetTexture == null)
                    {
                        usable += 1;
                    }
                }
            }

            if (usable == 0)
            {
                return false;
            }

            Camera[] draw = new Camera[usable];
            int slot = 0;
            for (int index = 0; index < cameras.Length; index++)
            {
                Camera cam = cameras[index];
                if (cam != null && cam.enabled && cam.targetTexture == null)
                {
                    draw[slot] = cam;
                    slot += 1;
                }
            }

            for (int outer = 0; outer < draw.Length; outer++)
            {
                for (int inner = outer + 1; inner < draw.Length; inner++)
                {
                    if (draw[inner].depth < draw[outer].depth)
                    {
                        Camera swap = draw[outer];
                        draw[outer] = draw[inner];
                        draw[inner] = swap;
                    }
                }
            }

            RenderTexture target = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);
            target.Create();
            RenderTexture previous = RenderTexture.active;
            RenderTexture[] oldTargets = new RenderTexture[draw.Length];
            try
            {
                for (int index = 0; index < draw.Length; index++)
                {
                    oldTargets[index] = draw[index].targetTexture;
                    draw[index].targetTexture = target;
                    draw[index].Render();
                }

                RenderTexture.active = target;
                Texture2D image = new Texture2D(Width, Height, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0f, 0f, Width, Height), 0, 0);
                image.Apply();
                File.WriteAllBytes(path, image.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(image);
            }
            finally
            {
                for (int index = 0; index < draw.Length; index++)
                {
                    if (draw[index] != null)
                    {
                        draw[index].targetTexture = oldTargets[index];
                    }
                }

                RenderTexture.active = previous;
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
            }

            return true;
        }

        static string Snapshot(GameManager game, GameSession session)
        {
            int upgrades = 0;
            int mk2 = 0;
            int doctrine = 0;
            PlayerLoadout loadout = UnityEngine.Object.FindAnyObjectByType<PlayerLoadout>();
            if (loadout != null && loadout.State != null)
            {
                upgrades = RunSaveCodec.PackUpgrades(loadout.State);
                mk2 = loadout.State.Mk2Mask;
                doctrine = (int)loadout.State.Doctrine;
            }

            StringBuilder builder = new StringBuilder();
            AppendField(builder, "world", WorldOf(session).ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "wave", WaveOf(session).ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "credits", session != null ? session.Credits.ToString(CultureInfo.InvariantCulture) : "0");
            AppendField(builder, "lives", LivesOf(session).ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "score", session != null ? session.Score.ToString(CultureInfo.InvariantCulture) : "0");
            AppendField(builder, "upgrades", upgrades.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "mk2", mk2.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "doctrine", doctrine.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "boons", BoonText(game));
            AppendField(builder, "runId", game != null ? game.ActiveRunId.ToString(CultureInfo.InvariantCulture) : "0");
            AppendField(builder, "dailySeed", game != null ? game.ActiveDailySeed.ToString(CultureInfo.InvariantCulture) : "0");
            AppendField(builder, "difficulty", ((int)DifficultySettings.Current).ToString(CultureInfo.InvariantCulture));
            ShipHealth health = UnityEngine.Object.FindAnyObjectByType<ShipHealth>();
            AppendField(builder, "hull", health != null ? health.Hull.ToString(CultureInfo.InvariantCulture) : "-1");
            AppendField(builder, "shield", health != null ? health.Shield.ToString(CultureInfo.InvariantCulture) : "-1");
            return builder.ToString();
        }

        static string CompareCheckpoint(LongRunState state, GameManager game, GameSession session)
        {
            string path = Path.Combine(state.OutDir, "checkpoint.txt");
            if (!File.Exists(path))
            {
                return "checkpoint missing";
            }

            string live = Snapshot(game, session);
            string[] saved = File.ReadAllLines(path);
            string[] now = live.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder mismatches = new StringBuilder();
            int lines = saved.Length;
            if (now.Length > lines)
            {
                lines = now.Length;
            }

            for (int index = 0; index < lines; index++)
            {
                string left = index < saved.Length ? saved[index] : string.Empty;
                string right = index < now.Length ? now[index] : string.Empty;
                if (!string.Equals(left, right, StringComparison.Ordinal))
                {
                    if (mismatches.Length > 0)
                    {
                        mismatches.Append("; ");
                    }

                    mismatches.Append(left);
                    mismatches.Append(" != ");
                    mismatches.Append(right);
                }
            }

            return mismatches.ToString();
        }

        static string BoonText(GameManager game)
        {
            if (game == null || game.Boons == null || game.Boons.Levels == null)
            {
                return string.Empty;
            }

            int[] levels = game.Boons.Levels;
            StringBuilder builder = new StringBuilder();
            for (int index = 0; index < levels.Length; index++)
            {
                if (index > 0)
                {
                    builder.Append(',');
                }

                builder.Append(levels[index].ToString(CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }

        static void AppendField(StringBuilder builder, string key, string value)
        {
            builder.Append(key);
            builder.Append('=');
            builder.Append(value);
            builder.Append('\n');
        }

        static int WorldOf(GameSession session)
        {
            if (session == null)
            {
                return 0;
            }

            return WorldCatalog.NumberForWave(session.WaveIndex);
        }

        static int WaveOf(GameSession session)
        {
            if (session == null)
            {
                return 0;
            }

            return session.WaveIndex;
        }

        static int CreditsOf(GameSession session)
        {
            if (session == null)
            {
                return 0;
            }

            return session.Credits;
        }

        static int LivesOf(GameSession session)
        {
            if (session == null)
            {
                return 0;
            }

            return session.Lives;
        }

        static string PhaseName(GamePhase phase)
        {
            if (phase == GamePhase.Playing)
            {
                return "playing";
            }

            if (phase == GamePhase.WaveClear)
            {
                return "wave-clear";
            }

            if (phase == GamePhase.Failed)
            {
                return "failed";
            }

            if (phase == GamePhase.CampaignClear)
            {
                return "campaign-clear";
            }

            return "hangar";
        }

        static LongRunState Parse(out string error)
        {
            error = string.Empty;
            LongRunState state = new LongRunState();
            state.Minutes = LongRunVerdict.DefaultMinutes;
            state.Seed = -1;
            state.ResumeWorld = 1;
            state.ResumeMinutes = 0;
            state.LastShipX = 100001f;
            state.LastShipZ = 100001f;
            bool minutesGiven = false;
            string minutesText;
            if (TryArg("-longRunMinutes", out minutesText))
            {
                int minutes;
                if (!int.TryParse(minutesText, NumberStyles.Integer, CultureInfo.InvariantCulture, out minutes) || minutes < 1 || minutes > 720)
                {
                    error = "-longRunMinutes needs an integer from 1 to 720";
                    return null;
                }

                state.Minutes = minutes;
                minutesGiven = true;
            }

            string targetText;
            if (TryArg("-targetWave", out targetText))
            {
                int targetWave;
                if (!int.TryParse(targetText, NumberStyles.Integer, CultureInfo.InvariantCulture, out targetWave) || targetWave < 1 || targetWave > 80)
                {
                    error = "-targetWave needs an integer from 1 to 80";
                    return null;
                }

                state.TargetWave = targetWave;
            }

            if (state.TargetWave > 0 && !minutesGiven)
            {
                state.Minutes = LongRunVerdict.TargetMinutesWhenUnspecified;
            }

            if (HasArg("-diagnosticInvulnerable"))
            {
                state.Invulnerable = 1;
                state.Diagnostic = 1;
            }

            string startWorldText;
            if (TryArg("-startWorld", out startWorldText))
            {
                int startWorld;
                if (!int.TryParse(startWorldText, NumberStyles.Integer, CultureInfo.InvariantCulture, out startWorld) || startWorld < 1 || startWorld > 7)
                {
                    error = "-startWorld needs an integer from 1 to 7";
                    return null;
                }

                state.StartWorld = startWorld;
                state.Skipped = 1;
                state.Diagnostic = 1;
            }

            string startWaveText;
            if (TryArg("-startWave", out startWaveText))
            {
                int startWave;
                if (!int.TryParse(startWaveText, NumberStyles.Integer, CultureInfo.InvariantCulture, out startWave) || startWave < 1 || startWave > 80)
                {
                    error = "-startWave needs an integer from 1 to 80";
                    return null;
                }

                state.StartWaveArg = startWave;
                state.Skipped = 1;
                state.Diagnostic = 1;
            }

            string loadout;
            if (TryArg("-loadout", out loadout))
            {
                state.Loadout = loadout;
                state.Skipped = 1;
                state.Diagnostic = 1;
            }

            if (HasArg("-menuProbe"))
            {
                state.MenuProbe = 1;
            }

            string seedText;
            if (TryArg("-seed", out seedText))
            {
                int seed;
                if (!int.TryParse(seedText, NumberStyles.Integer, CultureInfo.InvariantCulture, out seed) || seed < 0)
                {
                    error = "-seed needs an integer >= 0";
                    return null;
                }

                state.Seed = seed;
            }

            string worldText;
            if (TryArg("-resumeAfterWorld", out worldText))
            {
                int world;
                if (!int.TryParse(worldText, NumberStyles.Integer, CultureInfo.InvariantCulture, out world) || world < 0 || world > 7)
                {
                    error = "-resumeAfterWorld needs an integer from 0 to 7";
                    return null;
                }

                state.ResumeWorld = world;
            }

            string resumeMinutesText;
            if (TryArg("-resumeAfterMinutes", out resumeMinutesText))
            {
                int resumeMinutes;
                if (!int.TryParse(resumeMinutesText, NumberStyles.Integer, CultureInfo.InvariantCulture, out resumeMinutes) || resumeMinutes < 0 || resumeMinutes > 720)
                {
                    error = "-resumeAfterMinutes needs an integer from 0 to 720";
                    return null;
                }

                state.ResumeMinutes = resumeMinutes;
            }

            string stamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
            string chosen;
            if (!TryArg("-longRunOut", out chosen) || chosen.Length == 0)
            {
                state.OutDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "Captures", "LongRun", stamp));
            }
            else if (Path.IsPathRooted(chosen))
            {
                state.OutDir = Path.Combine(chosen, stamp);
            }
            else
            {
                state.OutDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), chosen, stamp));
            }

            state.Profile = Path.Combine(state.OutDir, "profile");
            state.Backup = Path.Combine(state.OutDir, "backup");
            state.Live = Application.persistentDataPath;
            state.Phase = "wait";
            state.StartUtc = DateTime.UtcNow.Ticks;
            state.WaitUtc = state.StartUtc;
            state.BoughtWave = -1;
            state.BoonWave = -1;
            state.LastWave = -1;
            state.LastLives = -1;
            state.MaxWave = 1;
            state.MaxWorld = 1;
            return state;
        }

        static bool TryArg(string flag, out string value)
        {
            value = string.Empty;
            string[] args = Environment.GetCommandLineArgs();
            for (int index = 0; index < args.Length - 1; index++)
            {
                if (args[index] == flag)
                {
                    value = args[index + 1];
                    return true;
                }
            }

            return false;
        }

        static bool HasArg(string flag)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int index = 0; index < args.Length; index++)
            {
                if (args[index] == flag)
                {
                    return true;
                }
            }

            return false;
        }

        static double Elapsed(LongRunState state)
        {
            if (state.StartUtc <= 0)
            {
                return 0.0;
            }

            return (DateTime.UtcNow - new DateTime(state.StartUtc, DateTimeKind.Utc)).TotalSeconds;
        }

        static double Since(long ticks)
        {
            if (ticks <= 0)
            {
                return 0.0;
            }

            return (DateTime.UtcNow - new DateTime(ticks, DateTimeKind.Utc)).TotalSeconds;
        }

        static string Json(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            return text.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n");
        }

        static void Save(LongRunState state)
        {
            SessionState.SetString(KeyOut, state.OutDir);
            Directory.CreateDirectory(state.OutDir);
            StringBuilder builder = new StringBuilder();
            AppendField(builder, "phase", state.Phase);
            AppendField(builder, "startUtc", state.StartUtc.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "waitUtc", state.WaitUtc.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "minutes", state.Minutes.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "seed", state.Seed.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "resumeWorld", state.ResumeWorld.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "resumeMinutes", state.ResumeMinutes.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "profile", state.Profile);
            AppendField(builder, "backup", state.Backup);
            AppendField(builder, "live", state.Live);
            AppendField(builder, "deaths", state.Deaths.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "continues", state.Continues.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "purchases", state.Purchases.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "maxWave", state.MaxWave.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "maxWorld", state.MaxWorld.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "diedEarly", state.DiedEarly.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "saveFailed", state.SaveFailed.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "resumeFailed", state.ResumeFailed.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "resumeDone", state.ResumeDone.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "resumeReached", state.ResumeReached.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "harnessFault", state.HarnessFault.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "fault", state.Fault.Replace("\r", " ").Replace("\n", " "));
            AppendField(builder, "boughtWave", state.BoughtWave.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "boonWave", state.BoonWave.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "lastWave", state.LastWave.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "lastWorld", state.LastWorld.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "lastLives", state.LastLives.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "lastPhase", state.LastPhase);
            AppendField(builder, "settle", state.Settle.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "exceptions", state.Exceptions.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "errors", state.Errors.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "shot", state.Shot.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "exitAsked", state.ExitAsked.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "targetWave", state.TargetWave.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "diagnostic", state.Diagnostic.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "invulnerable", state.Invulnerable.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "skipped", state.Skipped.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "startWorld", state.StartWorld.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "startWaveArg", state.StartWaveArg.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "loadout", state.Loadout);
            AppendField(builder, "menuProbe", state.MenuProbe.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "watchdog", state.Watchdog.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "progressUtc", state.ProgressUtc.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "stepUtc", state.StepUtc.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "logBytes", state.LogBytes.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "logCapped", state.LogCapped.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "lifeLogs", state.LifeLogs.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "continueChecks", state.ContinueChecks.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "clearedWaves", state.ClearedWaves.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "overContinue", state.OverContinue.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "newRunNo", state.NewRunNo.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "newRunYes", state.NewRunYes.ToString(CultureInfo.InvariantCulture));
            AppendField(builder, "lastShipX", state.LastShipX.ToString("0.00", CultureInfo.InvariantCulture));
            AppendField(builder, "lastShipZ", state.LastShipZ.ToString("0.00", CultureInfo.InvariantCulture));
            File.WriteAllText(Path.Combine(state.OutDir, "state.txt"), builder.ToString());
        }

        static LongRunState Load()
        {
            string dir = SessionState.GetString(KeyOut, string.Empty);
            if (dir.Length == 0)
            {
                return null;
            }

            string path = Path.Combine(dir, "state.txt");
            if (!File.Exists(path))
            {
                return null;
            }

            LongRunState state = new LongRunState();
            state.OutDir = dir;
            state.BoughtWave = -1;
            state.BoonWave = -1;
            state.LastWave = -1;
            state.LastLives = -1;
            string[] lines = File.ReadAllLines(path);
            for (int index = 0; index < lines.Length; index++)
            {
                string line = lines[index];
                int split = line.IndexOf('=');
                if (split <= 0)
                {
                    continue;
                }

                string key = line.Substring(0, split);
                string value = line.Substring(split + 1);
                Apply(state, key, value);
            }

            return state;
        }

        static void Apply(LongRunState state, string key, string value)
        {
            if (key == "phase")
            {
                state.Phase = value;
            }
            else if (key == "startUtc")
            {
                state.StartUtc = ParseLong(value);
            }
            else if (key == "waitUtc")
            {
                state.WaitUtc = ParseLong(value);
            }
            else if (key == "minutes")
            {
                state.Minutes = ParseInt(value);
            }
            else if (key == "seed")
            {
                state.Seed = ParseInt(value);
            }
            else if (key == "resumeWorld")
            {
                state.ResumeWorld = ParseInt(value);
            }
            else if (key == "resumeMinutes")
            {
                state.ResumeMinutes = ParseInt(value);
            }
            else if (key == "profile")
            {
                state.Profile = value;
            }
            else if (key == "backup")
            {
                state.Backup = value;
            }
            else if (key == "live")
            {
                state.Live = value;
            }
            else if (key == "deaths")
            {
                state.Deaths = ParseInt(value);
            }
            else if (key == "continues")
            {
                state.Continues = ParseInt(value);
            }
            else if (key == "purchases")
            {
                state.Purchases = ParseInt(value);
            }
            else if (key == "maxWave")
            {
                state.MaxWave = ParseInt(value);
            }
            else if (key == "maxWorld")
            {
                state.MaxWorld = ParseInt(value);
            }
            else if (key == "diedEarly")
            {
                state.DiedEarly = ParseInt(value);
            }
            else if (key == "saveFailed")
            {
                state.SaveFailed = ParseInt(value);
            }
            else if (key == "resumeFailed")
            {
                state.ResumeFailed = ParseInt(value);
            }
            else if (key == "resumeDone")
            {
                state.ResumeDone = ParseInt(value);
            }
            else if (key == "resumeReached")
            {
                state.ResumeReached = ParseInt(value);
            }
            else if (key == "harnessFault")
            {
                state.HarnessFault = ParseInt(value);
            }
            else if (key == "fault")
            {
                state.Fault = value;
            }
            else if (key == "boughtWave")
            {
                state.BoughtWave = ParseInt(value);
            }
            else if (key == "boonWave")
            {
                state.BoonWave = ParseInt(value);
            }
            else if (key == "lastWave")
            {
                state.LastWave = ParseInt(value);
            }
            else if (key == "lastWorld")
            {
                state.LastWorld = ParseInt(value);
            }
            else if (key == "lastLives")
            {
                state.LastLives = ParseInt(value);
            }
            else if (key == "lastPhase")
            {
                state.LastPhase = value;
            }
            else if (key == "settle")
            {
                state.Settle = ParseInt(value);
            }
            else if (key == "exceptions")
            {
                state.Exceptions = ParseInt(value);
            }
            else if (key == "errors")
            {
                state.Errors = ParseInt(value);
            }
            else if (key == "shot")
            {
                state.Shot = ParseInt(value);
            }
            else if (key == "exitAsked")
            {
                state.ExitAsked = ParseInt(value);
            }
            else if (key == "targetWave")
            {
                state.TargetWave = ParseInt(value);
            }
            else if (key == "diagnostic")
            {
                state.Diagnostic = ParseInt(value);
            }
            else if (key == "invulnerable")
            {
                state.Invulnerable = ParseInt(value);
            }
            else if (key == "skipped")
            {
                state.Skipped = ParseInt(value);
            }
            else if (key == "startWorld")
            {
                state.StartWorld = ParseInt(value);
            }
            else if (key == "startWaveArg")
            {
                state.StartWaveArg = ParseInt(value);
            }
            else if (key == "loadout")
            {
                state.Loadout = value;
            }
            else if (key == "menuProbe")
            {
                state.MenuProbe = ParseInt(value);
            }
            else if (key == "watchdog")
            {
                state.Watchdog = ParseInt(value);
            }
            else if (key == "progressUtc")
            {
                state.ProgressUtc = ParseLong(value);
            }
            else if (key == "stepUtc")
            {
                state.StepUtc = ParseLong(value);
            }
            else if (key == "logBytes")
            {
                state.LogBytes = ParseInt(value);
            }
            else if (key == "logCapped")
            {
                state.LogCapped = ParseInt(value);
            }
            else if (key == "lifeLogs")
            {
                state.LifeLogs = ParseInt(value);
            }
            else if (key == "continueChecks")
            {
                state.ContinueChecks = ParseInt(value);
            }
            else if (key == "clearedWaves")
            {
                state.ClearedWaves = ParseInt(value);
            }
            else if (key == "overContinue")
            {
                state.OverContinue = ParseInt(value);
            }
            else if (key == "newRunNo")
            {
                state.NewRunNo = ParseInt(value);
            }
            else if (key == "newRunYes")
            {
                state.NewRunYes = ParseInt(value);
            }
            else if (key == "lastShipX")
            {
                state.LastShipX = ParseFloat(value);
            }
            else if (key == "lastShipZ")
            {
                state.LastShipZ = ParseFloat(value);
            }
        }

        static float ParseFloat(string value)
        {
            float number;
            if (!float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out number))
            {
                return 0f;
            }

            return number;
        }

        static int ParseInt(string value)
        {
            int number;
            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
            {
                return 0;
            }

            return number;
        }

        static long ParseLong(string value)
        {
            long number;
            if (!long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
            {
                return 0L;
            }

            return number;
        }
    }

    public sealed class LongRunState
    {
        public string Phase = "wait";
        public long StartUtc;
        public long WaitUtc;
        public int Minutes = LongRunVerdict.DefaultMinutes;
        public int Seed = -1;
        public int ResumeWorld = 1;
        public int ResumeMinutes;
        public string OutDir = string.Empty;
        public string Profile = string.Empty;
        public string Backup = string.Empty;
        public string Live = string.Empty;
        public int Deaths;
        public int Continues;
        public int Purchases;
        public int MaxWave = 1;
        public int MaxWorld = 1;
        public int DiedEarly;
        public int SaveFailed;
        public int ResumeFailed;
        public int ResumeDone;
        public int ResumeReached;
        public int HarnessFault;
        public string Fault = string.Empty;
        public int BoughtWave = -1;
        public int BoonWave = -1;
        public int LastWave = -1;
        public int LastWorld = -1;
        public int LastLives = -1;
        public string LastPhase = string.Empty;
        public int Settle;
        public int Exceptions;
        public int Errors;
        public int Shot;
        public int ExitAsked;
        public int TargetWave;
        public int Diagnostic;
        public int Invulnerable;
        public int Skipped;
        public int StartWorld;
        public int StartWaveArg;
        public string Loadout = string.Empty;
        public int MenuProbe;
        public int Watchdog;
        public long ProgressUtc;
        public long StepUtc;
        public int LogBytes;
        public int LogCapped;
        public int LifeLogs;
        public int ContinueChecks;
        public int ClearedWaves;
        public int OverContinue;
        public int NewRunNo;
        public int NewRunYes;
        public float LastShipX = 100001f;
        public float LastShipZ = 100001f;
    }
}
