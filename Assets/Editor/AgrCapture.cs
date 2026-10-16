using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace AsteroidsGoneRogue.EditorTools
{
    /// <summary>
    /// Batch capture for SpeltestBot side-by-side shots. Editor folder only, so
    /// the player build never includes it. The same file compiles on 0.47 and
    /// 0.48: gameplay types are reached by name, and a missing feature is logged
    /// and skipped. Do not pass -nographics; the post stack needs a GPU.
    ///
    /// "C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe" -batchmode -projectPath . -executeMethod AsteroidsGoneRogue.EditorTools.AgrCapture.Run -logFile - -captureOut Captures\compare -reduce 0 -seed 48048
    ///
    /// Play mode is entered with EditorApplication.EnterPlaymode. 6000.6 keeps
    /// the scripting domain by default (scene reload only) unless Enter Play
    /// Mode Options say otherwise; SessionState plus playModeStateChanged cover
    /// both. A missing GameSession fails in seconds. PlayerPrefs and
    /// run_save.json / meta_save.json are copied to a timestamped temp folder
    /// before anything else, restored on every exit path, and left on disk.
    /// </summary>
    [InitializeOnLoad]
    public static class AgrCapture
    {
        const int Width = 1920;
        const int Height = 1080;
        const int ShotCount = 6;
        const int SettleFrames = 60;
        const int CardFrames = 3;
        const double TimeoutSeconds = 300.0;
        const double EnterLimitSeconds = 20.0;
        const double SessionLimitSeconds = 15.0;
        const float FallbackDeathMid = 0.2f;
        const string PlayScene = "Assets/Scenes/Play.unity";

        const string KeyRunning = "AgrCapture.Running";
        const string KeyOut = "AgrCapture.Out";
        const string KeyReduce = "AgrCapture.Reduce";
        const string KeySeed = "AgrCapture.Seed";
        const string KeyBackup = "AgrCapture.Backup";
        const string KeyPhase = "AgrCapture.Phase";
        const string KeyShot = "AgrCapture.Shot";
        const string KeyWaited = "AgrCapture.Waited";
        const string KeySeen = "AgrCapture.Seen";
        const string KeyError = "AgrCapture.Error";
        const string KeyHud = "AgrCapture.Hud";
        const string KeyStart = "AgrCapture.Start";
        const string KeyPlayStart = "AgrCapture.PlayStart";
        const string KeyRestored = "AgrCapture.Restored";
        const string KeySceneLogged = "AgrCapture.SceneLogged";
        const string KeyLoggedPhase = "AgrCapture.LoggedPhase";
        const string KeyBootKicked = "AgrCapture.BootKicked";
        const float PlayerClearRadius = 8f;
        const float ScreenCenterClear = 12f;

        const int PhaseEnter = 0;
        const int PhaseWait = 1;
        const int PhaseStage = 2;
        const int PhaseSettle = 3;
        const int PhaseArm = 4;
        const int PhaseFire = 5;
        const int PhaseCard = 6;
        const int PhaseStop = 7;
        const int PhaseWaitStop = 8;
        const int PhaseRestore = 9;

        const System.Reflection.BindingFlags Flags =
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Static |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic;

        static System.Collections.Generic.Dictionary<string, System.Type> _types;
        static bool _loggedPost;
        static string _backupPath = string.Empty;

        static AgrCapture()
        {
            HookLifecycle();
            if (SessionState.GetBool(KeyRunning, false))
            {
                _backupPath = SessionState.GetString(KeyBackup, string.Empty);
                EnsureHook();
                Debug.Log("AgrCapture phase editor reloaded; capture still running backup=" + _backupPath);
            }
        }

        [InitializeOnEnterPlayMode]
        static void OnEnterPlayMode(EnterPlayModeOptions options)
        {
            if (!SessionState.GetBool(KeyRunning, false))
            {
                return;
            }

            Debug.Log(
                "AgrCapture phase entered play mode options=" + options.ToString()
                + " frame=" + Time.frameCount.ToString());
            EnsureHook();
            EditorApplication.QueuePlayerLoopUpdate();
        }

        public static void Run()
        {
            string outDir = string.Empty;
            int reduce = 0;
            int seed = 0;
            string parseError = ParseArgs(out outDir, out reduce, out seed);
            if (parseError.Length > 0)
            {
                Debug.LogError("AgrCapture FAIL: " + parseError);
                Quit(1);
                return;
            }

            if (SessionState.GetBool(KeyRunning, false))
            {
                Debug.LogError("AgrCapture FAIL: capture is already running");
                Quit(1);
                return;
            }

            string backup = string.Empty;
            bool handedOff = false;
            try
            {
                backup = MakeBackupFolder();
                _backupPath = backup;
                ParkUserData(backup);
                Debug.Log("AgrCapture backup " + backup);
                LogSaveBackup(backup);
                ApplyTempPrefs(reduce);
                System.IO.Directory.CreateDirectory(outDir);
                EditorSceneManager.OpenScene(PlayScene);
                Debug.Log("AgrCapture phase scene opened " + PlayScene);
                SessionState.SetBool(KeyRunning, true);
                SessionState.SetString(KeyOut, outDir);
                SessionState.SetInt(KeyReduce, reduce);
                SessionState.SetInt(KeySeed, seed);
                SessionState.SetString(KeyBackup, backup);
                SessionState.SetInt(KeyPhase, PhaseEnter);
                SessionState.SetInt(KeyLoggedPhase, -1);
                SessionState.SetInt(KeyShot, 0);
                SessionState.SetInt(KeyWaited, 0);
                SessionState.SetInt(KeySeen, -1);
                SessionState.SetString(KeyError, string.Empty);
                SessionState.SetString(KeyHud, string.Empty);
                SessionState.SetBool(KeyRestored, false);
                SessionState.SetBool(KeySceneLogged, false);
                string now = StampNow();
                SessionState.SetString(KeyStart, now);
                SessionState.SetString(KeyPlayStart, now);
                Debug.Log(
                    "AgrCapture: out=" + outDir
                    + " reduce=" + reduce.ToString()
                    + " seed=" + seed.ToString()
                    + " res=" + Width.ToString() + "x" + Height.ToString()
                    + " backup=" + backup
                    + " W1=wave 1, W3=world 3 wave 11, W4=world 4 wave 16, death=wave 1, waveclear=wave 1");
                LogPlayModeSettings();
                HookLifecycle();
                EnsureHook();
                SessionState.SetString(KeyPlayStart, StampNow());
                Debug.Log("AgrCapture phase enter play mode");
                EditorApplication.EnterPlaymode();
                handedOff = true;
            }
            catch (System.Exception bootEx)
            {
                Debug.LogError("AgrCapture FAIL: " + bootEx);
                RestoreOrLog(backup);
                ClearSession();
                EditorApplication.update -= Tick;
                Quit(1);
            }
            finally
            {
                if (!handedOff)
                {
                    RestoreOrLog(backup);
                }
            }
        }

        static void HookLifecycle()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeState;
            EditorApplication.playModeStateChanged += OnPlayModeState;
            EditorApplication.quitting -= OnQuitting;
            EditorApplication.quitting += OnQuitting;
        }

        static void OnPlayModeState(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(KeyRunning, false))
            {
                return;
            }

            Debug.Log("AgrCapture phase playModeStateChanged " + state.ToString());
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                string scenePath = UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
                Debug.Log(
                    "AgrCapture phase entered play mode scene=" + scenePath
                    + " frame=" + Time.frameCount.ToString());
                EnsureHook();
                EditorApplication.QueuePlayerLoopUpdate();
                return;
            }

            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                Debug.Log("AgrCapture phase exiting play mode");
                return;
            }

            if (state == PlayModeStateChange.EnteredEditMode)
            {
                int phase = SessionState.GetInt(KeyPhase, PhaseEnter);
                if (phase == PhaseEnter && !HasError())
                {
                    Debug.Log("AgrCapture phase entered edit mode before play; backup left in place");
                    return;
                }

                Debug.Log("AgrCapture phase entered edit mode; restoring saves");
                RestoreUserData();
                EnsureHook();
            }
        }

        static void OnQuitting()
        {
            if (!SessionState.GetBool(KeyRunning, false))
            {
                return;
            }

            Debug.Log("AgrCapture phase quitting; restoring saves");
            RestoreUserData();
        }

        static void EnsureHook()
        {
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
        }

        static void Tick()
        {
            bool failed = false;
            try
            {
                TickBody();
            }
            catch (System.Exception tickEx)
            {
                failed = true;
                Debug.LogError("AgrCapture FAIL: " + tickEx);
            }
            finally
            {
                if (failed)
                {
                    RestoreUserData();
                    ClearSession();
                    EditorApplication.update -= Tick;
                    Quit(1);
                }
            }
        }

        static void TickBody()
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.QueuePlayerLoopUpdate();
            }

            int phase = SessionState.GetInt(KeyPhase, PhaseEnter);
            NotePhase(phase);
            NoteScene();
            if (TimedOut() && phase != PhaseRestore)
            {
                if (phase == PhaseWaitStop)
                {
                    Debug.LogError("AgrCapture FAIL: play mode did not exit; restoring saves");
                    SessionState.SetString(KeyError, "play mode did not exit");
                    RestoreUserData();
                    SessionState.SetInt(KeyPhase, PhaseRestore);
                    phase = PhaseRestore;
                }
                else
                {
                    Fail("timed out after 300s");
                    phase = SessionState.GetInt(KeyPhase, PhaseEnter);
                }
            }

            if (phase == PhaseEnter)
            {
                if (EditorApplication.isPlaying)
                {
                    SessionState.SetString(KeyPlayStart, StampNow());
                    Debug.Log(
                        "AgrCapture phase play mode active scene="
                        + UnityEngine.SceneManagement.SceneManager.GetActiveScene().path
                        + " frame=" + Time.frameCount.ToString());
                    SessionState.SetInt(KeyPhase, PhaseWait);
                    return;
                }

                if (Since(KeyPlayStart) > EnterLimitSeconds)
                {
                    string why = "play mode transition stalled after EnterPlaymode. ";
                    if (!EditorApplication.isPlayingOrWillChangePlaymode)
                    {
                        why = "play mode did not start within 20s after EnterPlaymode. ";
                    }

                    Fail(why + PlayModeSettingText());
                }

                return;
            }

            if (phase == PhaseWait)
            {
                Nudge();
                Component game = FindComponent("GameManager");
                object session = null;
                if (game != null)
                {
                    session = GetProp(game, "Session");
                }

                int frame = Time.frameCount;
                if (session == null)
                {
                    Debug.Log(
                        "AgrCapture phase wait GameSession missing frame=" + frame.ToString()
                        + " scene=" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().path
                        + " game=" + (game != null ? "found" : "missing"));
                    if (game == null && Since(KeyPlayStart) > 1.0)
                    {
                        EnsureBootstrap();
                    }

                    if (Since(KeyPlayStart) > SessionLimitSeconds)
                    {
                        Fail(
                            "GameSession missing after "
                            + SessionLimitSeconds.ToString("0", System.Globalization.CultureInfo.InvariantCulture)
                            + "s (frame " + frame.ToString()
                            + "). UI setup may have aborted before the session was created.");
                    }

                    return;
                }

                Debug.Log("AgrCapture phase wait GameSession found frame=" + frame.ToString());
                ApplyRuntime();
                Debug.Log(
                    "AgrCapture: play ready, screen "
                    + Screen.width.ToString() + "x" + Screen.height.ToString());
                SessionState.SetInt(KeyPhase, PhaseStage);
                return;
            }

            if (phase == PhaseStage)
            {
                StageCurrent();
                if (HasError())
                {
                    return;
                }

                SessionState.SetInt(KeyWaited, 0);
                SessionState.SetInt(KeySeen, -1);
                SessionState.SetInt(KeyPhase, PhaseSettle);
                return;
            }

            if (phase == PhaseSettle)
            {
                Nudge();
                if (!AdvanceFrame())
                {
                    return;
                }

                int shot = SessionState.GetInt(KeyShot, 0);
                HoldForShot(shot);
                if (shot != 0)
                {
                    PinShip();
                    KeepShipAlive();
                }

                QuietBanners();
                int waited = SessionState.GetInt(KeyWaited, 0);
                if (waited < SettleFrames)
                {
                    return;
                }

                if (shot == 4)
                {
                    SessionState.SetInt(KeyPhase, PhaseArm);
                    return;
                }

                if (shot == 5)
                {
                    PinWaveClear();
                }

                CaptureCurrent();
                if (HasError())
                {
                    return;
                }

                AdvanceShot();
                return;
            }

            if (phase == PhaseArm)
            {
                Component ui = FindComponent("GameUi");
                Behaviour uiBehaviour = ui as Behaviour;
                if (uiBehaviour != null)
                {
                    uiBehaviour.enabled = false;
                }
                else
                {
                    Debug.Log("AgrCapture: GameUi missing; death clock may advance");
                }

                EditorApplication.isPaused = true;
                SessionState.SetInt(KeyPhase, PhaseFire);
                return;
            }

            if (phase == PhaseFire)
            {
                FireDeath();
                return;
            }

            if (phase == PhaseCard)
            {
                Nudge();
                if (!AdvanceFrame())
                {
                    return;
                }

                if (SessionState.GetInt(KeyWaited, 0) < CardFrames)
                {
                    return;
                }

                CaptureCurrent();
                if (HasError())
                {
                    return;
                }

                AdvanceShot();
                return;
            }

            if (phase == PhaseStop)
            {
                EditorApplication.isPaused = false;
                EnableUi();
                SessionState.SetInt(KeyPhase, PhaseWaitStop);
                if (EditorApplication.isPlaying)
                {
                    EditorApplication.isPlaying = false;
                }

                return;
            }

            if (phase == PhaseWaitStop)
            {
                if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    return;
                }

                SessionState.SetInt(KeyPhase, PhaseRestore);
                return;
            }

            if (phase == PhaseRestore)
            {
                RestoreUserData();
                string kept = CurrentBackup();
                if (kept.Length > 0)
                {
                    Debug.Log("AgrCapture backup kept " + kept);
                }

                int code = CaptureContinue.ExitCode(HasError(), HasHudFail());

                ClearSession();
                EditorApplication.update -= Tick;
                Quit(code);
            }
        }

        static void StageCurrent()
        {
            int shot = SessionState.GetInt(KeyShot, 0);
            bool reduce = SessionState.GetInt(KeyReduce, 0) != 0;
            Debug.Log(
                "AgrCapture phase shot " + shot.ToString()
                + " " + ShotFile(shot, reduce)
                + " frame=" + Time.frameCount.ToString());
            Component game = FindComponent("GameManager");
            if (game == null)
            {
                Fail("GameManager missing");
                return;
            }

            ClearFeel(game);
            object session = GetProp(game, "Session");
            if (session == null)
            {
                Fail("GameSession missing");
                return;
            }

            if (PhaseName(session) == "Playing")
            {
                if (!InvokeZero(game, "AbortWave"))
                {
                    Fail("AbortWave missing");
                    return;
                }
            }

            if (shot == 0)
            {
                if (!InvokeZero(game, "EnterHangar"))
                {
                    Fail("EnterHangar missing");
                    return;
                }

                SnapHangar();
                HoldForShot(0);
                return;
            }

            int wave = WaveFor(shot);
            if (!InvokeInts(session, "RestoreHangar", new int[] { wave, 0, 0, 3, 0, 0, 0, 0 }))
            {
                Fail("RestoreHangar missing");
                return;
            }

            SkipTutorial(game);
            ApplySeed(game);
            ApplyReduceCache();
            if (!InvokeZero(game, "StartWave"))
            {
                Fail("StartWave missing");
                return;
            }

            if (PhaseName(session) != "Playing")
            {
                Fail("wave did not start, phase " + PhaseName(session));
                return;
            }

            SnapWorld(WorldFor(shot));
            KeepShipAlive();
            HoldForShot(shot);
            QuietBanners();
            PinShip();
        }

        static void FireDeath()
        {
            Component game = FindComponent("GameManager");
            if (game == null)
            {
                Fail("GameManager missing at death");
                return;
            }

            object session = GetProp(game, "Session");
            if (session == null)
            {
                Fail("GameSession missing at death");
                return;
            }

            if (!SetProp(session, "Lives", 1))
            {
                Debug.Log("AgrCapture: could not set lives to 1; death may respawn");
            }

            if (!InvokeString(game, "NotifyPlayerDestroyed", "Capture"))
            {
                Fail("NotifyPlayerDestroyed missing");
                return;
            }

            bool pinned = PinDeath(game);
            if (!pinned)
            {
                Debug.Log("AgrCapture: death slow-mo timeline is not in this build; captured the fail card");
                EnableUi();
                EditorApplication.isPaused = false;
                SessionState.SetInt(KeyWaited, 0);
                SessionState.SetInt(KeySeen, Time.frameCount);
                SessionState.SetInt(KeyPhase, PhaseCard);
                return;
            }

            QuietBanners();
            CaptureCurrent();
            ClearFeel(game);
            EnableUi();
            EditorApplication.isPaused = false;
            if (HasError())
            {
                return;
            }

            AdvanceShot();
        }

        static void CaptureCurrent()
        {
            QuietBanners();
            ApplyReduceCache();
            int shot = SessionState.GetInt(KeyShot, 0);
            if (shot != 0)
            {
                HoldForShot(shot);
                SeparateOpening(true);
            }
            bool reduce = SessionState.GetInt(KeyReduce, 0) != 0;
            string folder = SessionState.GetString(KeyOut, string.Empty);
            string fileName = ShotFile(shot, reduce);
            string path = System.IO.Path.Combine(folder, fileName);
            byte[] png = RenderFrame();
            if (png == null || png.Length < 64)
            {
                Fail("empty capture " + fileName);
                return;
            }

            System.IO.File.WriteAllBytes(path, png);
            Debug.Log("AgrCapture: wrote " + path + " (" + png.Length.ToString() + " bytes)");
            LogGrade("shot " + shot.ToString());
            CheckHudLayout(shot);
        }

        static void AdvanceShot()
        {
            int shot = SessionState.GetInt(KeyShot, 0);
            int next = CaptureContinue.NextShot(shot, HasHudFail(), false);
            shot = next;
            SessionState.SetInt(KeyShot, shot);
            SessionState.SetInt(KeyWaited, 0);
            SessionState.SetInt(KeySeen, -1);
            if (shot >= ShotCount)
            {
                SessionState.SetInt(KeyPhase, PhaseStop);
                return;
            }

            SessionState.SetInt(KeyPhase, PhaseStage);
        }

        static bool AdvanceFrame()
        {
            int seen = SessionState.GetInt(KeySeen, -1);
            int frame = Time.frameCount;
            if (frame == seen)
            {
                return false;
            }

            SessionState.SetInt(KeySeen, frame);
            SessionState.SetInt(KeyWaited, SessionState.GetInt(KeyWaited, 0) + 1);
            return true;
        }

        static void ApplyRuntime()
        {
            Time.captureDeltaTime = 1f / 60f;
            AudioListener.volume = 0f;
            AudioListener.pause = true;
            Screen.SetResolution(Width, Height, FullScreenMode.Windowed);
            ApplyReduceCache();
            ForceNormal();
            ApplyAssistOff();
            Random.InitState(SessionState.GetInt(KeySeed, 1));
        }

        static void ApplySeed(object game)
        {
            int seed = SessionState.GetInt(KeySeed, 1);
            if (!SetProp(game, "ActiveDailySeed", seed))
            {
                Debug.Log("AgrCapture: ActiveDailySeed is not writable; RunRng may stay on the engine stream");
            }

            System.Type rng = FindType("RunRng");
            if (rng == null)
            {
                Debug.Log("AgrCapture: RunRng missing; spawn uses the engine random stream");
            }
            else
            {
                System.Reflection.MethodInfo setDaily = rng.GetMethod("SetDaily", new System.Type[] { typeof(int) });
                if (setDaily != null)
                {
                    setDaily.Invoke(null, new object[] { seed });
                }
                else
                {
                    Debug.Log("AgrCapture: RunRng.SetDaily missing");
                }
            }

            Random.InitState(seed);
        }

        static void SkipTutorial(object game)
        {
            System.Reflection.FieldInfo skip = game.GetType().GetField("_skipTutorialRedirect", Flags);
            if (skip != null)
            {
                skip.SetValue(game, true);
            }
            else
            {
                Debug.Log("AgrCapture: tutorial redirect flag missing; wave 1 may start the tutorial");
            }

            object meta = GetProp(game, "Meta");
            if (meta == null)
            {
                return;
            }

            System.Reflection.FieldInfo done = meta.GetType().GetField("TutorialDone", Flags);
            if (done != null)
            {
                done.SetValue(meta, 1);
            }

            System.Reflection.FieldInfo chosen = meta.GetType().GetField("DifficultyChosen", Flags);
            if (chosen != null)
            {
                chosen.SetValue(meta, 1);
            }
        }

        static void HoldForShot(int shot)
        {
            if (shot == 0)
            {
                HoldCamera(new Vector3(0f, 31f, -19f), new Vector3(0.25f, 0.4f, 0f), 54f);
                return;
            }

            HoldCamera(new Vector3(0f, 35f, -22f), new Vector3(0f, 0f, 0f), 54f);
        }

        static void HoldCamera(Vector3 pos, Vector3 look, float fov)
        {
            Component follow = FindComponent("FollowCamera");
            Camera cam = Camera.main;
            if (follow != null)
            {
                System.Reflection.MethodInfo hold = follow.GetType().GetMethod(
                    "HoldCapturePose",
                    new System.Type[] { typeof(Vector3), typeof(Vector3), typeof(float) });
                if (hold != null)
                {
                    hold.Invoke(follow, new object[] { pos, look, fov });
                    return;
                }

                Behaviour followBehaviour = follow as Behaviour;
                if (followBehaviour != null)
                {
                    followBehaviour.enabled = false;
                }

                Debug.Log("AgrCapture: HoldCapturePose missing; FollowCamera disabled for the shot");
            }

            if (cam == null)
            {
                return;
            }

            cam.transform.position = pos;
            cam.transform.LookAt(look);
            cam.fieldOfView = fov;
            cam.rect = new Rect(0f, 0f, 1f, 1f);
        }

        static void PinShip()
        {
            Component ship = FindComponent("ShipController");
            if (ship == null)
            {
                return;
            }

            ship.transform.position = Vector3.zero;
            ship.transform.rotation = Quaternion.identity;
            System.Reflection.MethodInfo input = ship.GetType().GetMethod(
                "SetInputEnabled",
                new System.Type[] { typeof(bool) });
            if (input != null)
            {
                input.Invoke(ship, new object[] { false });
            }

            Rigidbody body = ship.GetComponent<Rigidbody>();
            if (body != null)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }

            ClearAroundPlayer();
            SeparateOpening(false);
        }

        static void SeparateOpening(bool log)
        {
            System.Type type = FindType("OpeningClearance");
            if (type == null)
            {
                Debug.Log("AgrCapture keep-out OpeningClearance missing");
                return;
            }

            string methodName = log ? "LogAndHold" : "Hold";
            System.Reflection.MethodInfo method = type.GetMethod(methodName, Flags);
            if (method == null)
            {
                Debug.Log("AgrCapture keep-out " + methodName + " missing");
                return;
            }

            method.Invoke(null, null);
        }

        static void ClearAroundPlayer()
        {
            string[] names = new string[] { "Asteroid", "EnemySeeker", "ArenaHazard", "Pickup" };
            for (int nameIndex = 0; nameIndex < names.Length; nameIndex++)
            {
                PushClear(names[nameIndex], nameIndex);
            }
        }

        static void PushClear(string typeName, int lane)
        {
            System.Type type = FindType(typeName);
            if (type == null || !typeof(Component).IsAssignableFrom(type))
            {
                return;
            }

            UnityEngine.Object[] found = FindAllOf(type);
            if (found == null)
            {
                return;
            }

            for (int pieceIndex = 0; pieceIndex < found.Length; pieceIndex++)
            {
                Component piece = found[pieceIndex] as Component;
                if (piece == null)
                {
                    continue;
                }

                Vector3 center = piece.transform.position;
                float reach = VisualReach(piece, ref center);
                Vector3 flat = new Vector3(center.x, 0f, center.z);
                float dist = flat.magnitude;
                float need = PlayerClearRadius;
                if (ScreenCenterClear > need)
                {
                    need = ScreenCenterClear;
                }

                need += reach;
                if (dist >= need)
                {
                    continue;
                }

                Vector3 dir;
                if (dist > 0.2f)
                {
                    dir = flat / dist;
                }
                else
                {
                    float angle = (lane * 1.7f) + (pieceIndex * 0.9f);
                    dir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                }

                Vector3 target = dir * need;
                piece.transform.position += new Vector3(target.x - center.x, 0f, target.z - center.z);
                Rigidbody body = piece.GetComponent<Rigidbody>();
                if (body != null)
                {
                    body.position = piece.transform.position;
                    body.linearVelocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                }
            }
        }

        static float VisualReach(Component piece, ref Vector3 center)
        {
            float reach = 0.5f;
            if (piece == null)
            {
                return reach;
            }

            bool any = false;
            Bounds bounds = new Bounds(piece.transform.position, Vector3.zero);
            Renderer[] renderers = piece.GetComponentsInChildren<Renderer>(true);
            if (renderers != null)
            {
                for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
                {
                    Renderer renderer = renderers[rendererIndex];
                    if (renderer == null)
                    {
                        continue;
                    }

                    if (!any)
                    {
                        bounds = renderer.bounds;
                        any = true;
                    }
                    else
                    {
                        bounds.Encapsulate(renderer.bounds);
                    }
                }
            }

            Collider[] colliders = piece.GetComponentsInChildren<Collider>(true);
            if (colliders != null)
            {
                for (int colliderIndex = 0; colliderIndex < colliders.Length; colliderIndex++)
                {
                    Collider collider = colliders[colliderIndex];
                    if (collider == null)
                    {
                        continue;
                    }

                    float rad = 0f;
                    SphereCollider sphere = collider as SphereCollider;
                    if (sphere != null)
                    {
                        Vector3 scale = sphere.transform.lossyScale;
                        float axis = scale.x;
                        if (scale.y > axis)
                        {
                            axis = scale.y;
                        }

                        if (scale.z > axis)
                        {
                            axis = scale.z;
                        }

                        if (axis < 0f)
                        {
                            axis = -axis;
                        }

                        rad = sphere.radius * axis;
                    }
                    else
                    {
                        Vector3 ext = collider.bounds.extents;
                        rad = ext.x;
                        if (ext.y > rad)
                        {
                            rad = ext.y;
                        }

                        if (ext.z > rad)
                        {
                            rad = ext.z;
                        }
                    }

                    if (rad > reach)
                    {
                        reach = rad;
                    }
                }
            }

            if (any)
            {
                center = bounds.center;
                Vector3 ext = bounds.extents;
                float visual = ext.x;
                if (ext.y > visual)
                {
                    visual = ext.y;
                }

                if (ext.z > visual)
                {
                    visual = ext.z;
                }

                if (visual > reach)
                {
                    reach = visual;
                }
            }

            return reach;
        }

        static void KeepShipAlive()
        {
            System.Type healthType = FindType("ShipHealth");
            if (healthType == null)
            {
                return;
            }

            Component ship = FindComponent("ShipController");
            if (ship == null)
            {
                return;
            }

            Component health = ship.GetComponent(healthType);
            if (health == null)
            {
                return;
            }

            System.Reflection.FieldInfo until = healthType.GetField("_invulnerableUntil", Flags);
            if (until != null)
            {
                until.SetValue(health, Time.time + 30f);
            }

            System.Reflection.FieldInfo armed = healthType.GetField("_invulnCueArmed", Flags);
            if (armed != null)
            {
                armed.SetValue(health, false);
            }
        }

        static void QuietBanners()
        {
            Component ui = FindComponent("GameUi");
            if (ui == null)
            {
                return;
            }

            System.Reflection.FieldInfo until = ui.GetType().GetField("_worldFlashUntil", Flags);
            if (until != null)
            {
                until.SetValue(ui, 0f);
            }

            System.Reflection.FieldInfo beat = ui.GetType().GetField("_medalBeat", Flags);
            if (beat != null)
            {
                beat.SetValue(ui, string.Empty);
            }

            System.Reflection.FieldInfo elite = ui.GetType().GetField("_eliteBanner", Flags);
            if (elite != null)
            {
                elite.SetValue(ui, string.Empty);
            }

            System.Reflection.MethodInfo refresh = ui.GetType().GetMethod(
                "RefreshWorldBadge",
                Flags,
                (System.Reflection.Binder)null,
                System.Type.EmptyTypes,
                (System.Reflection.ParameterModifier[])null);
            if (refresh != null)
            {
                refresh.Invoke(ui, new object[0]);
            }

            if (SessionState.GetInt(KeyShot, 0) != 0)
            {
                InvokeZero(ui, "HideProductTitle");
            }
        }

        static bool PinDeath(object game)
        {
            InvokeZero(game, "CapturePinDeath");
            System.Reflection.FieldInfo feelField = game.GetType().GetField("_feel", Flags);
            if (feelField == null)
            {
                return false;
            }

            object feel = feelField.GetValue(game);
            if (feel == null)
            {
                return false;
            }

            float mid = FallbackDeathMid;
            System.Reflection.FieldInfo slowField = feel.GetType().GetField("DeathSlowSeconds", Flags);
            if (slowField != null)
            {
                object raw = slowField.GetRawConstantValue();
                if (raw is float)
                {
                    float slowSeconds = (float)raw;
                    mid = slowSeconds * 0.5f;
                }
            }

            System.Reflection.FieldInfo elapsedField = feel.GetType().GetField("Elapsed", Flags);
            if (elapsedField != null)
            {
                elapsedField.SetValue(feel, mid);
            }

            PinVignette(DeathVignettePin());
            bool reduce = SessionState.GetInt(KeyReduce, 0) != 0;
            float scale = 1f;
            System.Reflection.MethodInfo timeScale = feel.GetType().GetMethod("TimeScale", new System.Type[] { typeof(bool) });
            if (timeScale != null)
            {
                object scaled = timeScale.Invoke(feel, new object[] { reduce });
                if (scaled is float)
                {
                    scale = (float)scaled;
                }
            }

            Time.timeScale = scale;
            RememberGrade();
            System.Type stack = FindType("PostStack");
            float vignette = ReadStaticFloat(stack, "LastVignette");
            float saturation = ReadStaticFloat(stack, "LastSaturation");
            float flash = ReadStaticFloat(stack, "LastDeathFlash");
            Debug.Log(
                "AgrCapture: pinned death timescale="
                + scale.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)
                + " vignette="
                + vignette.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)
                + " saturation="
                + saturation.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)
                + " flash="
                + flash.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));
            return true;
        }

        static float DeathVignettePin()
        {
            System.Type grade = FindType("PostGrade");
            if (grade == null)
            {
                return FallbackDeathMid;
            }

            System.Reflection.FieldInfo field = grade.GetField("DeathVignetteSeconds", Flags);
            if (field == null)
            {
                return FallbackDeathMid;
            }

            object raw = field.GetRawConstantValue();
            if (raw is float)
            {
                return (float)raw;
            }

            return FallbackDeathMid;
        }

        static void PinWaveClear()
        {
            Component game = FindComponent("GameManager");
            if (game == null)
            {
                Debug.Log("AgrCapture: wave-clear pin skipped, GameManager missing");
                return;
            }

            if (!InvokeZero(game, "CaptureWaveClear"))
            {
                Debug.Log("AgrCapture: CaptureWaveClear missing; shot is the live wave");
            }

            QuietBanners();
        }

        static void RememberGrade()
        {
            System.Type stack = FindType("PostStack");
            if (stack == null)
            {
                return;
            }

            System.Reflection.MethodInfo remember = stack.GetMethod("RememberGrade", Flags);
            if (remember == null)
            {
                return;
            }

            remember.Invoke(null, new object[0]);
        }

        static void LogGrade(string where)
        {
            System.Type stack = FindType("PostStack");
            if (stack == null)
            {
                Debug.Log("AgrCapture grade " + where + " PostStack missing");
                return;
            }

            float bloom = ReadStaticFloat(stack, "LastBloom");
            float grain = ReadStaticFloat(stack, "LastGrain");
            float vignette = ReadStaticFloat(stack, "LastVignette");
            int reduce = ReadStaticInt(stack, "LastReduce");
            Debug.Log(
                "AgrCapture grade " + where
                + " reduce=" + reduce.ToString(System.Globalization.CultureInfo.InvariantCulture)
                + " bloom=" + bloom.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)
                + " grain=" + grain.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)
                + " vignette=" + vignette.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));
        }

        static float ReadStaticFloat(System.Type type, string fieldName)
        {
            if (type == null)
            {
                return 0f;
            }

            System.Reflection.FieldInfo field = type.GetField(fieldName, Flags);
            if (field == null)
            {
                return 0f;
            }

            object raw = field.GetValue(null);
            if (raw is float)
            {
                return (float)raw;
            }

            return 0f;
        }

        static int ReadStaticInt(System.Type type, string fieldName)
        {
            if (type == null)
            {
                return 0;
            }

            System.Reflection.FieldInfo field = type.GetField(fieldName, Flags);
            if (field == null)
            {
                return 0;
            }

            object raw = field.GetValue(null);
            if (raw is int)
            {
                return (int)raw;
            }

            return 0;
        }

        static void PinVignette(float seconds)
        {
            Component stack = FindComponent("PostStack");
            if (stack == null)
            {
                if (!_loggedPost)
                {
                    _loggedPost = true;
                    Debug.Log("AgrCapture: PostStack is not in this build; death vignette pin skipped");
                }

                return;
            }

            System.Reflection.FieldInfo stateField = stack.GetType().GetField("_state", Flags);
            if (stateField == null)
            {
                Debug.Log("AgrCapture: PostStack death clock missing; vignette pin skipped");
                return;
            }

            object state = stateField.GetValue(stack);
            if (state == null)
            {
                return;
            }

            System.Reflection.FieldInfo deathField = state.GetType().GetField("DeathElapsed", Flags);
            if (deathField == null)
            {
                Debug.Log("AgrCapture: PostStack has no death vignette clock; pin skipped");
                return;
            }

            deathField.SetValue(state, seconds);
            stateField.SetValue(stack, state);
            System.Reflection.MethodInfo pin = stack.GetType().GetMethod("PinDeathClock", Flags);
            if (pin != null)
            {
                pin.Invoke(null, new object[] { seconds });
            }
        }

        static void ClearFeel(object game)
        {
            if (game == null)
            {
                return;
            }

            System.Reflection.FieldInfo feelField = game.GetType().GetField("_feel", Flags);
            if (feelField == null)
            {
                return;
            }

            object feel = feelField.GetValue(game);
            if (feel == null)
            {
                return;
            }

            System.Reflection.MethodInfo clear = feel.GetType().GetMethod("Clear", System.Type.EmptyTypes);
            if (clear != null)
            {
                clear.Invoke(feel, new object[0]);
            }
        }

        static void SnapHangar()
        {
            System.Type post = FindType("PostStack");
            if (post == null)
            {
                if (!_loggedPost)
                {
                    _loggedPost = true;
                    Debug.Log("AgrCapture: PostStack is not in this build; hangar grade snap skipped");
                }

                return;
            }

            System.Reflection.MethodInfo snap = post.GetMethod("SnapHangar", new System.Type[] { typeof(bool) });
            if (snap == null)
            {
                Debug.Log("AgrCapture: PostStack.SnapHangar missing");
                return;
            }

            bool reduce = SessionState.GetInt(KeyReduce, 0) != 0;
            snap.Invoke(null, new object[] { reduce });
        }

        static void SnapWorld(int world)
        {
            System.Type post = FindType("PostStack");
            if (post == null)
            {
                if (!_loggedPost)
                {
                    _loggedPost = true;
                    Debug.Log("AgrCapture: PostStack is not in this build; world grade snap skipped");
                }

                return;
            }

            bool reduce = SessionState.GetInt(KeyReduce, 0) != 0;
            System.Reflection.MethodInfo snap = post.GetMethod(
                "SnapWorld",
                new System.Type[] { typeof(int), typeof(bool) });
            if (snap != null)
            {
                snap.Invoke(null, new object[] { world, reduce });
                return;
            }

            System.Reflection.MethodInfo snapWorldOnly = post.GetMethod("SnapWorld", new System.Type[] { typeof(int) });
            if (snapWorldOnly != null)
            {
                snapWorldOnly.Invoke(null, new object[] { world });
                return;
            }

            Debug.Log("AgrCapture: PostStack.SnapWorld missing; grade blends on its own");
        }

        static void ApplyReduceCache()
        {
            System.Type settings = FindType("SettingsState");
            if (settings == null)
            {
                Debug.Log("AgrCapture: SettingsState missing; reduce pref is set but the cache was not forced");
                return;
            }

            bool reduce = SessionState.GetInt(KeyReduce, 0) != 0;
            System.Reflection.FieldInfo cached = settings.GetField("_reduceCached", Flags);
            if (cached != null)
            {
                cached.SetValue(null, reduce);
            }

            System.Reflection.FieldInfo ready = settings.GetField("_reduceReady", Flags);
            if (ready != null)
            {
                ready.SetValue(null, true);
            }
        }

        static void ApplyAssistOff()
        {
            System.Type settings = FindType("SettingsState");
            if (settings == null)
            {
                return;
            }

            System.Reflection.FieldInfo cached = settings.GetField("_assistCached", Flags);
            if (cached != null)
            {
                cached.SetValue(null, false);
            }

            System.Reflection.FieldInfo ready = settings.GetField("_assistReady", Flags);
            if (ready != null)
            {
                ready.SetValue(null, true);
            }
        }

        static void ForceNormal()
        {
            System.Type grades = FindType("DifficultyGrade");
            System.Type settings = FindType("DifficultySettings");
            if (grades == null || settings == null)
            {
                Debug.Log("AgrCapture: difficulty types missing; pref normal still applied");
                return;
            }

            System.Reflection.MethodInfo setGrade = settings.GetMethod("SetGrade", new System.Type[] { grades });
            if (setGrade == null)
            {
                Debug.Log("AgrCapture: DifficultySettings.SetGrade missing");
                return;
            }

            object normal = System.Enum.ToObject(grades, 1);
            setGrade.Invoke(null, new object[] { normal });
        }

        static void ReleaseRuntimeCache()
        {
            System.Type settings = FindType("SettingsState");
            if (settings != null)
            {
                System.Reflection.FieldInfo reduceReady = settings.GetField("_reduceReady", Flags);
                if (reduceReady != null)
                {
                    reduceReady.SetValue(null, false);
                }

                System.Reflection.FieldInfo assistReady = settings.GetField("_assistReady", Flags);
                if (assistReady != null)
                {
                    assistReady.SetValue(null, false);
                }
            }

            System.Type difficulty = FindType("DifficultySettings");
            if (difficulty == null)
            {
                return;
            }

            System.Reflection.FieldInfo loaded = difficulty.GetField("_loaded", Flags);
            if (loaded != null)
            {
                loaded.SetValue(null, false);
            }
        }

        static byte[] RenderFrame()
        {
            Camera[] cameras = UnityEngine.Object.FindObjectsByType<Camera>();
            int screenCount = 0;
            if (cameras != null)
            {
                for (int countIndex = 0; countIndex < cameras.Length; countIndex++)
                {
                    Camera counted = cameras[countIndex];
                    if (IsSceneCamera(counted))
                    {
                        screenCount++;
                    }
                }
            }

            if (screenCount == 0)
            {
                Debug.LogError("AgrCapture FAIL: no scene camera");
                return null;
            }

            Camera[] screenCams = new Camera[screenCount];
            int slot = 0;
            for (int fillIndex = 0; fillIndex < cameras.Length; fillIndex++)
            {
                Camera filled = cameras[fillIndex];
                if (!IsSceneCamera(filled))
                {
                    continue;
                }

                screenCams[slot] = filled;
                slot++;
            }

            for (int outer = 0; outer < screenCams.Length; outer++)
            {
                for (int inner = outer + 1; inner < screenCams.Length; inner++)
                {
                    if (screenCams[inner].depth < screenCams[outer].depth)
                    {
                        Camera swap = screenCams[outer];
                        screenCams[outer] = screenCams[inner];
                        screenCams[inner] = swap;
                    }
                }
            }

            Camera mainCam = Camera.main;
            RenderTexture hdr = new RenderTexture(Width, Height, 24, RenderTextureFormat.DefaultHDR);
            hdr.antiAliasing = 1;
            hdr.Create();
            RenderTexture ldr = new RenderTexture(Width, Height, 0, RenderTextureFormat.ARGB32);
            ldr.Create();
            RenderTexture previousActive = RenderTexture.active;
            RenderTexture[] previousTargets = new RenderTexture[screenCams.Length];
            Rect[] previousRects = new Rect[screenCams.Length];
            bool[] previousForce = new bool[screenCams.Length];
            float[] previousFov = new float[screenCams.Length];
            for (int snapIndex = 0; snapIndex < screenCams.Length; snapIndex++)
            {
                Camera snapCam = screenCams[snapIndex];
                previousTargets[snapIndex] = snapCam.targetTexture;
                previousRects[snapIndex] = snapCam.rect;
                previousForce[snapIndex] = snapCam.forceIntoRenderTexture;
                previousFov[snapIndex] = snapCam.fieldOfView;
            }

            try
            {
                for (int drawIndex = 0; drawIndex < screenCams.Length; drawIndex++)
                {
                    Camera drawCam = screenCams[drawIndex];
                    if (mainCam != null && drawCam != mainCam)
                    {
                        drawCam.fieldOfView = mainCam.fieldOfView;
                    }

                    drawCam.rect = new Rect(0f, 0f, 1f, 1f);
                    drawCam.forceIntoRenderTexture = true;
                    drawCam.targetTexture = hdr;
                    drawCam.Render();
                }

                CompositeOverlay(hdr);
                Graphics.Blit(hdr, ldr);
                RenderTexture.active = ldr;
                Texture2D image = new Texture2D(Width, Height, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0f, 0f, Width, Height), 0, 0);
                image.Apply();
                byte[] png = image.EncodeToPNG();
                UnityEngine.Object.DestroyImmediate(image);
                return png;
            }
            finally
            {
                for (int restoreIndex = 0; restoreIndex < screenCams.Length; restoreIndex++)
                {
                    Camera restoreCam = screenCams[restoreIndex];
                    if (restoreCam == null)
                    {
                        continue;
                    }

                    restoreCam.targetTexture = previousTargets[restoreIndex];
                    restoreCam.rect = previousRects[restoreIndex];
                    restoreCam.forceIntoRenderTexture = previousForce[restoreIndex];
                    restoreCam.fieldOfView = previousFov[restoreIndex];
                }

                RenderTexture.active = previousActive;
                hdr.Release();
                UnityEngine.Object.DestroyImmediate(hdr);
                ldr.Release();
                UnityEngine.Object.DestroyImmediate(ldr);
            }
        }

        static bool IsSceneCamera(Camera cam)
        {
            if (cam == null || !cam.enabled)
            {
                return false;
            }

            if (cam.targetTexture != null)
            {
                return false;
            }

            if (cam.name == "AgrCaptureUiCamera")
            {
                return false;
            }

            return true;
        }

        static void CompositeOverlay(RenderTexture target)
        {
            Canvas[] canvases = UnityEngine.Object.FindObjectsByType<Canvas>();
            int overlayCount = 0;
            if (canvases != null)
            {
                for (int canvasIndex = 0; canvasIndex < canvases.Length; canvasIndex++)
                {
                    Canvas counted = canvases[canvasIndex];
                    if (counted != null && counted.isActiveAndEnabled && counted.renderMode == RenderMode.ScreenSpaceOverlay)
                    {
                        overlayCount++;
                    }
                }
            }

            if (overlayCount == 0)
            {
                Debug.Log("AgrCapture: no overlay canvas; world capture only");
                return;
            }

            Canvas[] overlays = new Canvas[overlayCount];
            RenderMode[] modes = new RenderMode[overlayCount];
            Camera[] oldCams = new Camera[overlayCount];
            float[] planes = new float[overlayCount];
            int mask = 0;
            int filled = 0;
            for (int pickIndex = 0; pickIndex < canvases.Length; pickIndex++)
            {
                Canvas picked = canvases[pickIndex];
                if (picked == null || !picked.isActiveAndEnabled || picked.renderMode != RenderMode.ScreenSpaceOverlay)
                {
                    continue;
                }

                overlays[filled] = picked;
                modes[filled] = picked.renderMode;
                oldCams[filled] = picked.worldCamera;
                planes[filled] = picked.planeDistance;
                int layer = picked.gameObject.layer;
                if (layer >= 0 && layer < 32)
                {
                    mask |= 1 << layer;
                }

                filled++;
            }

            if (mask == 0)
            {
                mask = (1 << 0) | (1 << 5);
            }

            GameObject uiObject = new GameObject("AgrCaptureUiCamera");
            uiObject.hideFlags = HideFlags.HideAndDontSave;
            Camera uiCam = uiObject.AddComponent<Camera>();
            uiCam.enabled = false;
            uiCam.clearFlags = CameraClearFlags.Depth;
            uiCam.cullingMask = mask;
            uiCam.orthographic = true;
            uiCam.nearClipPlane = 0.01f;
            uiCam.farClipPlane = 200f;
            uiCam.rect = new Rect(0f, 0f, 1f, 1f);
            uiCam.allowHDR = false;
            uiCam.targetTexture = target;
            try
            {
                for (int assignIndex = 0; assignIndex < overlays.Length; assignIndex++)
                {
                    Canvas assign = overlays[assignIndex];
                    assign.renderMode = RenderMode.ScreenSpaceCamera;
                    assign.worldCamera = uiCam;
                    assign.planeDistance = 10f;
                }

                Canvas.ForceUpdateCanvases();
                uiCam.Render();
            }
            finally
            {
                for (int backIndex = 0; backIndex < overlays.Length; backIndex++)
                {
                    Canvas back = overlays[backIndex];
                    if (back == null)
                    {
                        continue;
                    }

                    back.renderMode = modes[backIndex];
                    back.worldCamera = oldCams[backIndex];
                    back.planeDistance = planes[backIndex];
                }

                uiCam.targetTexture = null;
                UnityEngine.Object.DestroyImmediate(uiObject);
            }
        }

        static void EnableUi()
        {
            Component ui = FindComponent("GameUi");
            Behaviour uiBehaviour = ui as Behaviour;
            if (uiBehaviour != null)
            {
                uiBehaviour.enabled = true;
            }
        }

        static void Nudge()
        {
            EditorApplication.QueuePlayerLoopUpdate();
        }

        static UnityEngine.Object[] FindAllOf(System.Type type)
        {
            if (type == null)
            {
                return null;
            }

            System.Reflection.MethodInfo[] methods = typeof(UnityEngine.Object).GetMethods(
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            for (int methodIndex = 0; methodIndex < methods.Length; methodIndex++)
            {
                System.Reflection.MethodInfo method = methods[methodIndex];
                if (method.Name != "FindObjectsByType" || !method.IsGenericMethodDefinition)
                {
                    continue;
                }

                if (method.GetParameters().Length != 0)
                {
                    continue;
                }

                System.Reflection.MethodInfo closed = method.MakeGenericMethod(type);
                object raw = closed.Invoke(null, new object[0]);
                return raw as UnityEngine.Object[];
            }

            return null;
        }

        static void EnsureBootstrap()
        {
            if (SessionState.GetInt(KeyBootKicked, 0) != 0)
            {
                return;
            }

            UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            string path = scene.IsValid() ? scene.path : string.Empty;
            int roots = scene.IsValid() ? scene.rootCount : 0;
            Debug.Log(
                "AgrCapture bootstrap scene=" + path
                + " roots=" + roots.ToString()
                + " frame=" + Time.frameCount.ToString());
            if (path.Length == 0 || !path.EndsWith("Play.unity"))
            {
                Debug.Log("AgrCapture bootstrap loading Play");
                try
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene("Play");
                }
                catch (System.Exception loadEx)
                {
                    SessionState.SetInt(KeyBootKicked, 1);
                    Debug.Log("AgrCapture bootstrap load failed: " + loadEx.Message);
                }

                return;
            }

            SessionState.SetInt(KeyBootKicked, 1);
            if (FindComponent("GameManager") != null)
            {
                return;
            }

            Component bootstrap = FindComponent("GameBootstrap");
            if (bootstrap == null)
            {
                System.Type type = FindType("GameBootstrap");
                if (type == null)
                {
                    Debug.Log("AgrCapture bootstrap GameBootstrap type missing");
                    return;
                }

                GameObject created = new GameObject("GameSystems");
                created.AddComponent(type);
                Debug.Log("AgrCapture bootstrap added GameSystems");
                return;
            }

            System.Reflection.MethodInfo boot = bootstrap.GetType().GetMethod("Boot", Flags);
            if (boot == null)
            {
                Debug.Log("AgrCapture bootstrap Boot missing");
                return;
            }

            try
            {
                boot.Invoke(bootstrap, new object[0]);
                Debug.Log("AgrCapture bootstrap invoked Boot");
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                System.Exception inner = ex.InnerException != null ? ex.InnerException : ex;
                Debug.Log("AgrCapture bootstrap Boot failed: " + inner.GetType().Name + " " + inner.Message);
            }
        }

        static Component FindComponent(string typeName)
        {
            System.Type type = FindType(typeName);
            if (type == null || !typeof(Component).IsAssignableFrom(type))
            {
                return null;
            }

            UnityEngine.Object found = UnityEngine.Object.FindAnyObjectByType(type, FindObjectsInactive.Include);
            return found as Component;
        }

        static System.Type FindType(string typeName)
        {
            if (_types == null)
            {
                _types = new System.Collections.Generic.Dictionary<string, System.Type>();
                System.Reflection.Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
                for (int assemblyIndex = 0; assemblyIndex < assemblies.Length; assemblyIndex++)
                {
                    System.Type[] types = null;
                    try
                    {
                        types = assemblies[assemblyIndex].GetTypes();
                    }
                    catch (System.Reflection.ReflectionTypeLoadException loadEx)
                    {
                        types = loadEx.Types;
                    }

                    if (types == null)
                    {
                        continue;
                    }

                    for (int typeIndex = 0; typeIndex < types.Length; typeIndex++)
                    {
                        System.Type candidate = types[typeIndex];
                        if (candidate == null || candidate.Namespace != "AsteroidsGoneRogue")
                        {
                            continue;
                        }

                        if (!_types.ContainsKey(candidate.Name))
                        {
                            _types.Add(candidate.Name, candidate);
                        }
                    }
                }
            }

            System.Type match;
            if (_types.TryGetValue(typeName, out match))
            {
                return match;
            }

            return null;
        }

        static bool InvokeZero(object target, string name)
        {
            if (target == null)
            {
                return false;
            }

            System.Reflection.MethodInfo method = target.GetType().GetMethod(name, System.Type.EmptyTypes);
            if (method == null)
            {
                return false;
            }

            method.Invoke(target, new object[0]);
            return true;
        }

        static bool InvokeString(object target, string name, string value)
        {
            if (target == null)
            {
                return false;
            }

            System.Reflection.MethodInfo method = target.GetType().GetMethod(name, new System.Type[] { typeof(string) });
            if (method == null)
            {
                return false;
            }

            method.Invoke(target, new object[] { value });
            return true;
        }

        static bool InvokeInts(object target, string name, int[] values)
        {
            if (target == null || values == null)
            {
                return false;
            }

            System.Type[] signature = new System.Type[values.Length];
            object[] args = new object[values.Length];
            for (int argIndex = 0; argIndex < values.Length; argIndex++)
            {
                signature[argIndex] = typeof(int);
                args[argIndex] = values[argIndex];
            }

            System.Reflection.MethodInfo method = target.GetType().GetMethod(name, signature);
            if (method == null)
            {
                return false;
            }

            method.Invoke(target, args);
            return true;
        }

        static object GetProp(object target, string name)
        {
            if (target == null)
            {
                return null;
            }

            System.Reflection.PropertyInfo prop = target.GetType().GetProperty(name, Flags);
            if (prop == null)
            {
                return null;
            }

            return prop.GetValue(target);
        }

        static bool SetProp(object target, string name, object value)
        {
            if (target == null)
            {
                return false;
            }

            System.Reflection.PropertyInfo prop = target.GetType().GetProperty(name, Flags);
            if (prop == null)
            {
                return false;
            }

            System.Reflection.MethodInfo setter = prop.GetSetMethod(true);
            if (setter == null)
            {
                return false;
            }

            setter.Invoke(target, new object[] { value });
            return true;
        }

        static string PhaseName(object session)
        {
            object phase = GetProp(session, "Phase");
            if (phase == null)
            {
                return string.Empty;
            }

            return phase.ToString();
        }

        static int WaveFor(int shot)
        {
            if (shot == 2)
            {
                return 11;
            }

            if (shot == 3)
            {
                return 16;
            }

            return 1;
        }

        static int WorldFor(int shot)
        {
            if (shot == 2)
            {
                return 3;
            }

            if (shot == 3)
            {
                return 4;
            }

            return 1;
        }

        static string ShotFile(int shot, bool reduce)
        {
            string sceneName = "death";
            if (shot == 0)
            {
                sceneName = "hangar";
            }
            else if (shot == 1)
            {
                sceneName = "w1";
            }
            else if (shot == 2)
            {
                sceneName = "w3";
            }
            else if (shot == 3)
            {
                sceneName = "w4";
            }
            else if (shot == 5)
            {
                sceneName = "waveclear";
            }

            string flag = "off";
            if (reduce)
            {
                flag = "on";
            }

            return sceneName + "_reduce" + flag + ".png";
        }

        static string ParseArgs(out string outDir, out int reduce, out int seed)
        {
            outDir = string.Empty;
            reduce = -1;
            seed = 0;
            bool sawOut = false;
            bool sawReduce = false;
            bool sawSeed = false;
            string[] args = System.Environment.GetCommandLineArgs();
            for (int argIndex = 0; argIndex < args.Length; argIndex++)
            {
                string token = args[argIndex];
                if (token == "-captureOut" && argIndex + 1 < args.Length)
                {
                    outDir = args[argIndex + 1];
                    sawOut = true;
                }
                else if (token == "-reduce" && argIndex + 1 < args.Length)
                {
                    string reduceText = args[argIndex + 1];
                    if (reduceText == "0")
                    {
                        reduce = 0;
                        sawReduce = true;
                    }
                    else if (reduceText == "1")
                    {
                        reduce = 1;
                        sawReduce = true;
                    }
                    else
                    {
                        return "-reduce must be 0 or 1";
                    }
                }
                else if (token == "-seed" && argIndex + 1 < args.Length)
                {
                    int parsed;
                    if (!int.TryParse(args[argIndex + 1], out parsed) || parsed <= 0)
                    {
                        return "-seed must be a positive int";
                    }

                    seed = parsed;
                    sawSeed = true;
                }
            }

            if (!sawOut || outDir.Length == 0)
            {
                return "missing -captureOut <dir>";
            }

            if (!sawReduce)
            {
                return "missing -reduce 0|1";
            }

            if (!sawSeed)
            {
                return "missing -seed <int>";
            }

            if (!System.IO.Path.IsPathRooted(outDir))
            {
                outDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), outDir));
            }

            return string.Empty;
        }

        static void ParkUserData(string backup)
        {
            string data = Application.persistentDataPath;
            string parked = System.IO.Path.Combine(backup, "persistent");
            CopyTree(data, parked);
            WritePrefs(System.IO.Path.Combine(backup, "prefs.txt"));
            HideSaves(data);
        }

        static void ApplyTempPrefs(int reduce)
        {
            int version = 7;
            System.Type settings = FindType("SettingsState");
            if (settings != null)
            {
                System.Reflection.FieldInfo versionField = settings.GetField("CurrentVersion", Flags);
                if (versionField != null)
                {
                    object raw = versionField.GetRawConstantValue();
                    if (raw is int)
                    {
                        version = (int)raw;
                    }
                }
            }

            PlayerPrefs.SetInt("agr.settings.version", version);
            PlayerPrefs.SetInt("agr.settings.reduceEffects", reduce);
            PlayerPrefs.SetString("agr.difficulty", "normal");
            PlayerPrefs.SetInt("agr.ui.firstHangarHint", 1);
            PlayerPrefs.SetInt("agr.ui.doctrineHint", 1);
            PlayerPrefs.SetInt("agr.settings.assistMode", 0);
            PlayerPrefs.Save();
        }

        static void RestoreUserData()
        {
            string backup = CurrentBackup();
            if (backup.Length == 0)
            {
                Debug.LogError("AgrCapture restore FAIL: backup path is empty");
                return;
            }

            if (SessionState.GetBool(KeyRestored, false))
            {
                Debug.Log("AgrCapture restore OK " + backup);
                return;
            }

            try
            {
                string prefsPath = System.IO.Path.Combine(backup, "prefs.txt");
                if (!System.IO.File.Exists(prefsPath))
                {
                    throw new System.IO.FileNotFoundException("prefs snapshot missing", prefsPath);
                }

                ReadPrefs(prefsPath);
                ReleaseRuntimeCache();
                PlayerPrefs.Save();
                string data = Application.persistentDataPath;
                MirrorBack(System.IO.Path.Combine(backup, "persistent"), data);
                Time.captureDeltaTime = 0f;
                AudioListener.pause = false;
                AudioListener.volume = 1f;
                EditorApplication.isPaused = false;
                SessionState.SetBool(KeyRestored, true);
                Debug.Log("AgrCapture restore OK " + backup);
                Debug.Log("AgrCapture backup kept " + backup);
            }
            catch (System.Exception restoreEx)
            {
                Debug.LogError("AgrCapture restore FAIL " + backup + ": " + restoreEx.Message);
            }
        }

        static void RestoreOrLog(string backup)
        {
            if (backup.Length == 0)
            {
                return;
            }

            _backupPath = backup;
            SessionState.SetString(KeyBackup, backup);
            RestoreUserData();
        }

        static string CurrentBackup()
        {
            if (_backupPath.Length > 0)
            {
                return _backupPath;
            }

            _backupPath = SessionState.GetString(KeyBackup, string.Empty);
            return _backupPath;
        }

        static string MakeBackupFolder()
        {
            string stamp = System.DateTime.UtcNow.ToString(
                "yyyyMMdd-HHmmss",
                System.Globalization.CultureInfo.InvariantCulture);
            string name = "agr-capture-" + stamp + "-" + System.Diagnostics.Process.GetCurrentProcess().Id.ToString();
            string backup = System.IO.Path.Combine(System.IO.Path.GetTempPath(), name);
            System.IO.Directory.CreateDirectory(backup);
            return backup;
        }

        static void LogSaveBackup(string backup)
        {
            string parked = System.IO.Path.Combine(backup, "persistent");
            LogBackedFile(parked, "run_save.json");
            LogBackedFile(parked, "meta_save.json");
        }

        static void LogBackedFile(string folder, string name)
        {
            string path = System.IO.Path.Combine(folder, name);
            string state = "absent";
            if (System.IO.File.Exists(path))
            {
                state = "present";
            }

            Debug.Log("AgrCapture backup " + name + " " + state + " " + path);
        }

        static void LogPlayModeSettings()
        {
            Debug.Log("AgrCapture phase " + PlayModeSettingText());
        }

        static string PlayModeSettingText()
        {
            bool enabled = EditorSettings.enterPlayModeOptionsEnabled;
            EnterPlayModeOptions options = EditorSettings.enterPlayModeOptions;
            bool disableDomain = enabled
                && (options & EnterPlayModeOptions.DisableDomainReload) != 0;
            bool disableScene = enabled
                && (options & EnterPlayModeOptions.DisableSceneReload) != 0;
            string fallback = "6000.6 default keeps the domain and reloads the scene";
            if (enabled)
            {
                fallback = "project Enter Play Mode Options are enabled";
            }

            return "playmode optionsEnabled=" + enabled.ToString()
                + " options=" + options.ToString()
                + " disableDomain=" + disableDomain.ToString()
                + " disableScene=" + disableScene.ToString()
                + " (" + fallback + ")";
        }

        static string StampNow()
        {
            return EditorApplication.timeSinceStartup.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        static double Since(string key)
        {
            string startText = SessionState.GetString(key, string.Empty);
            if (startText.Length == 0)
            {
                return 0.0;
            }

            double start;
            if (!double.TryParse(
                startText,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out start))
            {
                return 0.0;
            }

            return EditorApplication.timeSinceStartup - start;
        }

        static void NotePhase(int phase)
        {
            int logged = SessionState.GetInt(KeyLoggedPhase, -1);
            if (logged == phase)
            {
                return;
            }

            SessionState.SetInt(KeyLoggedPhase, phase);
            Debug.Log(
                "AgrCapture phase " + PhaseLabel(phase)
                + " shot=" + SessionState.GetInt(KeyShot, 0).ToString()
                + " frame=" + Time.frameCount.ToString());
        }

        static void NoteScene()
        {
            if (!EditorApplication.isPlaying || SessionState.GetBool(KeySceneLogged, false))
            {
                return;
            }

            UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return;
            }

            SessionState.SetBool(KeySceneLogged, true);
            Debug.Log(
                "AgrCapture phase scene loaded " + scene.path
                + " frame=" + Time.frameCount.ToString());
        }

        static string PhaseLabel(int phase)
        {
            if (phase == PhaseEnter)
            {
                return "enter";
            }

            if (phase == PhaseWait)
            {
                return "wait";
            }

            if (phase == PhaseStage)
            {
                return "stage";
            }

            if (phase == PhaseSettle)
            {
                return "settle";
            }

            if (phase == PhaseArm)
            {
                return "arm";
            }

            if (phase == PhaseFire)
            {
                return "fire";
            }

            if (phase == PhaseCard)
            {
                return "card";
            }

            if (phase == PhaseStop)
            {
                return "stop";
            }

            if (phase == PhaseWaitStop)
            {
                return "wait-stop";
            }

            return "restore";
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
                "F\tagr.audio.sfx",
                "F\tagr.audio.music",
                "S\tagr.settings.bindings",
                "S\tagr.ui.language",
                "S\tagr.sink.profile",
                "S\tagr.daily.board",
                "S\tagr.difficulty",
            };
        }

        static void WritePrefs(string path)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
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

                if (kind == "I")
                {
                    builder.Append("I\t");
                    builder.Append(key);
                    builder.Append('\t');
                    builder.Append(PlayerPrefs.GetInt(key).ToString(System.Globalization.CultureInfo.InvariantCulture));
                    builder.Append('\n');
                }
                else if (kind == "F")
                {
                    builder.Append("F\t");
                    builder.Append(key);
                    builder.Append('\t');
                    builder.Append(PlayerPrefs.GetFloat(key).ToString(System.Globalization.CultureInfo.InvariantCulture));
                    builder.Append('\n');
                }
                else
                {
                    string raw = PlayerPrefs.GetString(key, string.Empty);
                    string encoded = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(raw));
                    builder.Append("S\t");
                    builder.Append(key);
                    builder.Append('\t');
                    builder.Append(encoded);
                    builder.Append('\n');
                }
            }

            System.IO.File.WriteAllText(path, builder.ToString());
        }

        static void ReadPrefs(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                Debug.LogError("AgrCapture FAIL: prefs snapshot missing");
                return;
            }

            string[] lines = System.IO.File.ReadAllLines(path);
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
                    if (int.TryParse(payload, out number))
                    {
                        PlayerPrefs.SetInt(key, number);
                    }
                }
                else if (kind == "F")
                {
                    float number;
                    if (float.TryParse(
                        payload,
                        System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out number))
                    {
                        PlayerPrefs.SetFloat(key, number);
                    }
                }
                else if (kind == "S")
                {
                    byte[] bytes = System.Convert.FromBase64String(payload);
                    PlayerPrefs.SetString(key, System.Text.Encoding.UTF8.GetString(bytes));
                }
            }
        }

        static void HideSaves(string dataDir)
        {
            if (!System.IO.Directory.Exists(dataDir))
            {
                return;
            }

            string[] names = new string[]
            {
                "run_save.json",
                "meta_save.json",
                "run_save.json.bak",
                "meta_save.json.bak",
                "run_save.json.tmp",
                "meta_save.json.tmp",
            };
            for (int nameIndex = 0; nameIndex < names.Length; nameIndex++)
            {
                string path = System.IO.Path.Combine(dataDir, names[nameIndex]);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
            }
        }

        static void CopyTree(string fromDir, string toDir)
        {
            if (!System.IO.Directory.Exists(fromDir))
            {
                return;
            }

            System.IO.Directory.CreateDirectory(toDir);
            string[] files = System.IO.Directory.GetFiles(fromDir, "*", System.IO.SearchOption.AllDirectories);
            for (int fileIndex = 0; fileIndex < files.Length; fileIndex++)
            {
                string sourcePath = files[fileIndex];
                string relative = sourcePath.Substring(fromDir.Length).TrimStart(
                    System.IO.Path.DirectorySeparatorChar,
                    System.IO.Path.AltDirectorySeparatorChar);
                string destPath = System.IO.Path.Combine(toDir, relative);
                string destParent = System.IO.Path.GetDirectoryName(destPath);
                if (!string.IsNullOrEmpty(destParent))
                {
                    System.IO.Directory.CreateDirectory(destParent);
                }

                System.IO.File.Copy(sourcePath, destPath, true);
            }
        }

        static void MirrorBack(string fromDir, string toDir)
        {
            if (!System.IO.Directory.Exists(fromDir))
            {
                return;
            }

            if (!System.IO.Directory.Exists(toDir))
            {
                System.IO.Directory.CreateDirectory(toDir);
            }

            string[] live = System.IO.Directory.GetFiles(toDir, "*", System.IO.SearchOption.AllDirectories);
            for (int liveIndex = 0; liveIndex < live.Length; liveIndex++)
            {
                string livePath = live[liveIndex];
                string relative = livePath.Substring(toDir.Length).TrimStart(
                    System.IO.Path.DirectorySeparatorChar,
                    System.IO.Path.AltDirectorySeparatorChar);
                string backed = System.IO.Path.Combine(fromDir, relative);
                if (!System.IO.File.Exists(backed))
                {
                    System.IO.File.Delete(livePath);
                }
            }

            CopyTree(fromDir, toDir);
        }

        static bool TimedOut()
        {
            string startText = SessionState.GetString(KeyStart, string.Empty);
            if (startText.Length == 0)
            {
                return false;
            }

            double start;
            if (!double.TryParse(
                startText,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out start))
            {
                return false;
            }

            return EditorApplication.timeSinceStartup - start > TimeoutSeconds;
        }

        const float HudSlop = 2f;

        sealed class HudMark
        {
            public string Name;
            public string Block;
            public bool Lane;
            public bool Raycast;
            public float MinX;
            public float MinY;
            public float MaxX;
            public float MaxY;
        }

        static void CheckHudLayout(int shot)
        {
            if (shot < 1 || shot > 4)
            {
                return;
            }

            int[] widths = new int[] { 1920, 1280, 3440 };
            int[] heights = new int[] { 1080, 800, 1440 };
            CanvasScaler[] scalers = UnityEngine.Object.FindObjectsByType<CanvasScaler>();
            Vector2[] savedResolution = scalers != null ? new Vector2[scalers.Length] : new Vector2[0];
            if (scalers != null)
            {
                for (int scalerIndex = 0; scalerIndex < scalers.Length; scalerIndex++)
                {
                    CanvasScaler scaler = scalers[scalerIndex];
                    savedResolution[scalerIndex] = scaler != null ? scaler.referenceResolution : new Vector2(1920f, 1080f);
                }
            }

            GameUi[] uis = UnityEngine.Object.FindObjectsByType<GameUi>();
            try
            {
                for (int probe = 0; probe < widths.Length; probe++)
                {
                    int width = widths[probe];
                    int height = heights[probe];
                    if (scalers != null)
                    {
                        for (int scalerIndex = 0; scalerIndex < scalers.Length; scalerIndex++)
                        {
                            CanvasScaler scaler = scalers[scalerIndex];
                            if (scaler != null)
                            {
                                scaler.referenceResolution = new Vector2(width, height);
                            }
                        }
                    }

                    if (uis != null)
                    {
                        for (int uiIndex = 0; uiIndex < uis.Length; uiIndex++)
                        {
                            GameUi ui = uis[uiIndex];
                            if (ui != null)
                            {
                                ui.ApplyPlayHudLayout(width, height);
                            }
                        }
                    }

                    Canvas.ForceUpdateCanvases();
                    if (uis != null)
                    {
                        for (int uiIndex = 0; uiIndex < uis.Length; uiIndex++)
                        {
                            GameUi ui = uis[uiIndex];
                            if (ui != null)
                            {
                                ui.RefitPlayHint();
                            }
                        }
                    }

                    Canvas.ForceUpdateCanvases();
                    MeasureHud(width, height);
                }
            }
            finally
            {
                if (scalers != null)
                {
                    for (int scalerIndex = 0; scalerIndex < scalers.Length; scalerIndex++)
                    {
                        CanvasScaler scaler = scalers[scalerIndex];
                        if (scaler != null)
                        {
                            scaler.referenceResolution = savedResolution[scalerIndex];
                        }
                    }
                }

                int restoreW = Screen.width > 1 ? Screen.width : 1920;
                int restoreH = Screen.height > 1 ? Screen.height : 1080;
                if (uis != null)
                {
                    for (int uiIndex = 0; uiIndex < uis.Length; uiIndex++)
                    {
                        GameUi ui = uis[uiIndex];
                        if (ui != null)
                        {
                            ui.ApplyPlayHudLayout(restoreW, restoreH);
                        }
                    }
                }

                Canvas.ForceUpdateCanvases();
                if (uis != null)
                {
                    for (int uiIndex = 0; uiIndex < uis.Length; uiIndex++)
                    {
                        GameUi ui = uis[uiIndex];
                        if (ui != null)
                        {
                            ui.RefitPlayHint();
                        }
                    }
                }

                Canvas.ForceUpdateCanvases();
            }
        }

        static void MeasureHud(int width, int height)
        {
            Graphic[] graphics = UnityEngine.Object.FindObjectsByType<Graphic>();
            System.Collections.Generic.List<HudMark> marks = new System.Collections.Generic.List<HudMark>();
            if (graphics != null)
            {
                for (int graphicIndex = 0; graphicIndex < graphics.Length; graphicIndex++)
                {
                    NoteTextOverflow(graphics[graphicIndex]);
                    HudMark mark;
                    if (TryHudMark(graphics[graphicIndex], width, height, out mark))
                    {
                        marks.Add(mark);
                    }
                }
            }

            string res = width.ToString(System.Globalization.CultureInfo.InvariantCulture)
                + "x"
                + height.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (marks.Count == 0)
            {
                Debug.Log("AgrCapture hud res=" + res + " none");
                return;
            }

            float safeMinX = 0f;
            float safeMinY = 0f;
            float safeMaxX = width;
            float safeMaxY = height;
            string firstOutside = string.Empty;
            string firstOverlap = string.Empty;
            for (int markIndex = 0; markIndex < marks.Count; markIndex++)
            {
                HudMark shown = marks[markIndex];
                Debug.Log(
                    "AgrCapture hud res="
                    + res
                    + " name="
                    + shown.Name
                    + " block="
                    + shown.Block
                    + " rect="
                    + HudRectText(shown.MinX, shown.MinY, shown.MaxX, shown.MaxY));
                if (HudOutside(shown, safeMinX, safeMinY, safeMaxX, safeMaxY))
                {
                    Debug.Log(
                        "AgrCapture hud outside name="
                        + shown.Name
                        + " res="
                        + res
                        + " rect="
                        + HudRectText(shown.MinX, shown.MinY, shown.MaxX, shown.MaxY)
                        + " safe="
                        + HudRectText(safeMinX, safeMinY, safeMaxX, safeMaxY));
                    if (firstOutside.Length == 0)
                    {
                        firstOutside = "hud outside " + shown.Name;
                    }
                }
            }

            for (int left = 0; left < marks.Count; left++)
            {
                for (int right = left + 1; right < marks.Count; right++)
                {
                    HudMark leftMark = marks[left];
                    HudMark rightMark = marks[right];
                    if (leftMark.Block == rightMark.Block || !HudOverlaps(leftMark, rightMark))
                    {
                        continue;
                    }

                    if (HudOverlay(leftMark, width, height) || HudOverlay(rightMark, width, height))
                    {
                        continue;
                    }

                    bool bothLanes = leftMark.Lane && rightMark.Lane;
                    if (!bothLanes && (HudContains(leftMark, rightMark) || HudContains(rightMark, leftMark)))
                    {
                        continue;
                    }

                    Debug.Log(
                        "AgrCapture hud overlap a="
                        + leftMark.Name
                        + " b="
                        + rightMark.Name
                        + " res="
                        + res
                        + " blockA="
                        + leftMark.Block
                        + " blockB="
                        + rightMark.Block
                        + " rectA="
                        + HudRectText(leftMark.MinX, leftMark.MinY, leftMark.MaxX, leftMark.MaxY)
                        + " rectB="
                        + HudRectText(rightMark.MinX, rightMark.MinY, rightMark.MaxX, rightMark.MaxY));
                    if (firstOverlap.Length == 0)
                    {
                        firstOverlap = "hud overlap " + leftMark.Block + " x " + rightMark.Block;
                    }
                }
            }

            NoteGlyphOverlaps(graphics, width, height, res, ref firstOverlap);
            if (firstOverlap.Length > 0)
            {
                NoteHudFail(firstOverlap + " " + res);
            }

            if (firstOutside.Length > 0)
            {
                NoteHudFail(firstOutside + " " + res);
            }
        }

        /// <summary>
        /// Text overlap uses the glyph box (preferred width and height, placed
        /// by alignment). A wide rect with a short label does not count as a hit,
        /// and an overflowing line does.
        /// </summary>
        static void NoteGlyphOverlaps(
            Graphic[] graphics,
            int width,
            int height,
            string res,
            ref string firstOverlap)
        {
            if (graphics == null)
            {
                return;
            }

            System.Collections.Generic.List<HudMark> ink = new System.Collections.Generic.List<HudMark>();
            for (int graphicIndex = 0; graphicIndex < graphics.Length; graphicIndex++)
            {
                Text label = graphics[graphicIndex] as Text;
                HudMark mark;
                if (label == null || !TryGlyphMark(label, width, height, out mark))
                {
                    continue;
                }

                ink.Add(mark);
            }

            for (int left = 0; left < ink.Count; left++)
            {
                for (int right = left + 1; right < ink.Count; right++)
                {
                    HudMark leftMark = ink[left];
                    HudMark rightMark = ink[right];
                    if (leftMark.Block == rightMark.Block || !HudOverlaps(leftMark, rightMark))
                    {
                        continue;
                    }

                    Debug.Log(
                        "AgrCapture hud glyph overlap a="
                        + leftMark.Name
                        + " b="
                        + rightMark.Name
                        + " res="
                        + res
                        + " blockA="
                        + leftMark.Block
                        + " blockB="
                        + rightMark.Block
                        + " rectA="
                        + HudRectText(leftMark.MinX, leftMark.MinY, leftMark.MaxX, leftMark.MaxY)
                        + " rectB="
                        + HudRectText(rightMark.MinX, rightMark.MinY, rightMark.MaxX, rightMark.MaxY));
                    if (firstOverlap.Length == 0)
                    {
                        firstOverlap = "hud glyph overlap " + leftMark.Block + " x " + rightMark.Block;
                    }
                }
            }
        }

        static bool TryGlyphMark(Text label, int targetW, int targetH, out HudMark mark)
        {
            mark = null;
            if (label == null || label.font == null || !HudShown(label))
            {
                return false;
            }

            if (label.preferredWidth < 1f || label.preferredHeight < 1f)
            {
                return false;
            }

            RectTransform rect = label.rectTransform;
            if (rect == null)
            {
                return false;
            }

            Rect area = rect.rect;
            float inkW = label.preferredWidth;
            float inkH = label.preferredHeight;
            bool clipWidth = label.horizontalOverflow != HorizontalWrapMode.Overflow;
            bool clipHeight = label.verticalOverflow == VerticalWrapMode.Truncate;
            if (clipWidth && area.width > 1f && inkW > area.width)
            {
                inkW = area.width;
            }

            if (clipHeight && area.height > 1f && inkH > area.height)
            {
                inkH = area.height;
            }

            TextAnchor align = label.alignment;
            bool right = align == TextAnchor.UpperRight || align == TextAnchor.MiddleRight || align == TextAnchor.LowerRight;
            bool centerX = align == TextAnchor.UpperCenter || align == TextAnchor.MiddleCenter || align == TextAnchor.LowerCenter;
            bool upper = align == TextAnchor.UpperLeft || align == TextAnchor.UpperCenter || align == TextAnchor.UpperRight;
            bool middle = align == TextAnchor.MiddleLeft || align == TextAnchor.MiddleCenter || align == TextAnchor.MiddleRight;
            float x0 = area.x;
            float y0 = area.y;
            if (right)
            {
                x0 = area.x + area.width - inkW;
            }
            else if (centerX)
            {
                x0 = area.x + ((area.width - inkW) * 0.5f);
            }

            if (upper)
            {
                y0 = area.y + area.height - inkH;
            }
            else if (middle)
            {
                y0 = area.y + ((area.height - inkH) * 0.5f);
            }

            Vector3 minCorner = rect.TransformPoint(new Vector3(x0, y0, 0f));
            Vector3 maxCorner = rect.TransformPoint(new Vector3(x0 + inkW, y0 + inkH, 0f));
            float minX = minCorner.x;
            float minY = minCorner.y;
            float maxX = maxCorner.x;
            float maxY = maxCorner.y;
            if (maxX < minX)
            {
                float swap = minX;
                minX = maxX;
                maxX = swap;
            }

            if (maxY < minY)
            {
                float swap = minY;
                minY = maxY;
                maxY = swap;
            }

            if (maxX - minX <= 0.01f || maxY - minY <= 0.01f)
            {
                return false;
            }

            float scaleX = 1f;
            float scaleY = 1f;
            float screenW = Screen.width;
            float screenH = Screen.height;
            if (screenW < 1f)
            {
                screenW = 1f;
            }

            if (screenH < 1f)
            {
                screenH = 1f;
            }

            if (targetW > 1)
            {
                scaleX = targetW / screenW;
            }

            if (targetH > 1)
            {
                scaleY = targetH / screenH;
            }

            mark = new HudMark();
            mark.Name = label.gameObject.name;
            mark.Block = HudBlock(label.transform);
            mark.MinX = minX * scaleX;
            mark.MinY = minY * scaleY;
            mark.MaxX = maxX * scaleX;
            mark.MaxY = maxY * scaleY;
            return true;
        }

        /// <summary>
        /// Truncation is a preferred size that no longer fits the rect.
        /// Horizontal Overflow is allowed to pass the rect on width alone:
        /// the score caption is one line drawn that way and is not cut.
        /// </summary>
        static void NoteTextOverflow(Graphic graphic)
        {
            Text label = graphic as Text;
            if (label == null || label.font == null || label.resizeTextForBestFit || !HudShown(graphic))
            {
                return;
            }

            Canvas canvas = graphic.GetComponentInParent<Canvas>();
            if (canvas == null || canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                return;
            }

            RectTransform rect = label.rectTransform;
            if (rect == null)
            {
                return;
            }

            Rect area = rect.rect;
            if (area.width <= 1f || area.height <= 1f)
            {
                return;
            }

            float widthOver = label.preferredWidth - area.width;
            float heightOver = label.preferredHeight - area.height;
            bool heightCut = heightOver > HudSlop && label.verticalOverflow == VerticalWrapMode.Truncate;
            bool widthCut = widthOver > HudSlop
                && label.horizontalOverflow != HorizontalWrapMode.Overflow
                && label.verticalOverflow == VerticalWrapMode.Truncate
                && heightOver > HudSlop;
            if (!heightCut && !widthCut)
            {
                return;
            }

            NoteHudFail("hud text overflow " + label.gameObject.name);
        }

        static bool TryHudMark(Graphic graphic, int targetW, int targetH, out HudMark mark)
        {
            mark = null;
            if (!HudShown(graphic))
            {
                return false;
            }

            Canvas canvas = graphic.GetComponentInParent<Canvas>();
            if (canvas == null || canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                return false;
            }

            RectTransform rect = graphic.rectTransform;
            if (rect == null)
            {
                return false;
            }

            Vector3[] corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            float minX = corners[0].x;
            float minY = corners[0].y;
            float maxX = corners[0].x;
            float maxY = corners[0].y;
            for (int corner = 1; corner < corners.Length; corner++)
            {
                if (corners[corner].x < minX)
                {
                    minX = corners[corner].x;
                }

                if (corners[corner].y < minY)
                {
                    minY = corners[corner].y;
                }

                if (corners[corner].x > maxX)
                {
                    maxX = corners[corner].x;
                }

                if (corners[corner].y > maxY)
                {
                    maxY = corners[corner].y;
                }
            }

            if (maxX - minX <= 0.01f || maxY - minY <= 0.01f)
            {
                return false;
            }

            string block = HudBlock(graphic.transform);
            float scaleX = 1f;
            float scaleY = 1f;
            float screenW = Screen.width;
            float screenH = Screen.height;
            if (screenW < 1f)
            {
                screenW = 1f;
            }

            if (screenH < 1f)
            {
                screenH = 1f;
            }

            if (targetW > 1)
            {
                scaleX = targetW / screenW;
            }

            if (targetH > 1)
            {
                scaleY = targetH / screenH;
            }

            mark = new HudMark();
            mark.Name = graphic.gameObject.name;
            mark.Block = block;
            mark.Lane = HudLane(block);
            mark.Raycast = graphic.raycastTarget;
            mark.MinX = minX * scaleX;
            mark.MinY = minY * scaleY;
            mark.MaxX = maxX * scaleX;
            mark.MaxY = maxY * scaleY;
            return true;
        }

        static bool HudShown(Graphic graphic)
        {
            if (graphic == null || graphic.gameObject == null || !graphic.isActiveAndEnabled || !graphic.gameObject.activeInHierarchy)
            {
                return false;
            }

            float alpha = graphic.color.a * HudGroupAlpha(graphic.transform);
            if (alpha <= 0.01f)
            {
                return false;
            }

            Text label = graphic as Text;
            if (label != null && string.IsNullOrWhiteSpace(label.text))
            {
                return false;
            }

            if (label == null && HudWrapsChildren(graphic))
            {
                return false;
            }

            return true;
        }

        static bool HudWrapsChildren(Graphic graphic)
        {
            Transform root = graphic.transform;
            int count = root.childCount;
            for (int childIndex = 0; childIndex < count; childIndex++)
            {
                Transform child = root.GetChild(childIndex);
                if (child == null || !child.gameObject.activeInHierarchy)
                {
                    continue;
                }

                Graphic nested = child.GetComponent<Graphic>();
                if (nested == null || !nested.isActiveAndEnabled)
                {
                    continue;
                }

                float childAlpha = nested.color.a * HudGroupAlpha(child);
                if (childAlpha <= 0.01f)
                {
                    continue;
                }

                Text childText = nested as Text;
                if (childText != null && string.IsNullOrWhiteSpace(childText.text))
                {
                    continue;
                }

                RectTransform childRect = nested.rectTransform;
                if (childRect != null && (childRect.rect.width <= 0.01f || childRect.rect.height <= 0.01f))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        static float HudGroupAlpha(Transform node)
        {
            float alpha = 1f;
            Transform cursor = node;
            while (cursor != null)
            {
                CanvasGroup group = cursor.GetComponent<CanvasGroup>();
                if (group != null)
                {
                    alpha *= group.alpha;
                }

                cursor = cursor.parent;
            }

            return alpha;
        }

        static string HudBlock(Transform node)
        {
            Transform cursor = node;
            while (cursor != null && cursor.parent != null && cursor.parent.GetComponent<Canvas>() == null)
            {
                cursor = cursor.parent;
            }

            if (cursor == null)
            {
                return string.Empty;
            }

            return cursor.name;
        }

        static bool HudLane(string block)
        {
            if (block == "Hint" || block == "HealthRack" || block == "AbortWave")
            {
                return true;
            }

            if (block == "UtilityHud" || block == "DoctrineBadge" || block == "BossBar")
            {
                return true;
            }

            return false;
        }

        static void HudSafe(out float minX, out float minY, out float maxX, out float maxY)
        {
            Rect safe = Screen.safeArea;
            minX = safe.x;
            minY = safe.y;
            maxX = safe.x + safe.width;
            maxY = safe.y + safe.height;
            if (minX < 0f)
            {
                minX = 0f;
            }

            if (minY < 0f)
            {
                minY = 0f;
            }

            float screenW = Screen.width;
            float screenH = Screen.height;
            if (maxX > screenW)
            {
                maxX = screenW;
            }

            if (maxY > screenH)
            {
                maxY = screenH;
            }

            if (maxX - minX < 1f || maxY - minY < 1f)
            {
                minX = 0f;
                minY = 0f;
                maxX = screenW;
                maxY = screenH;
            }
        }

        static bool HudOutside(HudMark mark, float safeMinX, float safeMinY, float safeMaxX, float safeMaxY)
        {
            return mark.MinX < safeMinX - HudSlop
                || mark.MinY < safeMinY - HudSlop
                || mark.MaxX > safeMaxX + HudSlop
                || mark.MaxY > safeMaxY + HudSlop;
        }

        static bool HudOverlaps(HudMark leftMark, HudMark rightMark)
        {
            float overlapX = Mathf.Min(leftMark.MaxX, rightMark.MaxX) - Mathf.Max(leftMark.MinX, rightMark.MinX);
            float overlapY = Mathf.Min(leftMark.MaxY, rightMark.MaxY) - Mathf.Max(leftMark.MinY, rightMark.MinY);
            return overlapX > HudSlop && overlapY > HudSlop;
        }

        static bool HudContains(HudMark outer, HudMark inner)
        {
            return inner.MinX >= outer.MinX - HudSlop
                && inner.MaxX <= outer.MaxX + HudSlop
                && inner.MinY >= outer.MinY - HudSlop
                && inner.MaxY <= outer.MaxY + HudSlop;
        }

        static string HudRectText(float minX, float minY, float maxX, float maxY)
        {
            return minX.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)
                + ","
                + minY.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)
                + ","
                + maxX.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)
                + ","
                + maxY.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
        }

        static bool HasError()
        {
            return SessionState.GetString(KeyError, string.Empty).Length > 0;
        }

        static bool HasHudFail()
        {
            return SessionState.GetString(KeyHud, string.Empty).Length > 0;
        }

        static bool HudOverlay(HudMark mark, int width, int height)
        {
            if (mark == null)
            {
                return false;
            }

            if (mark.Name == "PlayVignette")
            {
                return true;
            }

            if (mark.Raycast)
            {
                return false;
            }

            float spanX = mark.MaxX - mark.MinX;
            float spanY = mark.MaxY - mark.MinY;
            float wide = width > 1 ? width : 1f;
            float tall = height > 1 ? height : 1f;
            return spanX >= wide * 0.9f && spanY >= tall * 0.9f;
        }

        static void NoteHudFail(string reason)
        {
            Debug.LogError("AgrCapture FAIL: " + reason);
            string prior = SessionState.GetString(KeyHud, string.Empty);
            if (prior.Length == 0)
            {
                SessionState.SetString(KeyHud, reason);
                return;
            }

            if (prior.IndexOf(reason, System.StringComparison.Ordinal) >= 0)
            {
                return;
            }

            SessionState.SetString(KeyHud, prior + " | " + reason);
        }

        static void Fail(string reason)
        {
            if (!HasError())
            {
                Debug.LogError("AgrCapture FAIL: " + reason);
                SessionState.SetString(KeyError, reason);
            }

            bool playing = EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode;
            int phase = SessionState.GetInt(KeyPhase, PhaseEnter);
            if (playing && phase != PhaseWaitStop && phase != PhaseStop)
            {
                EditorApplication.isPaused = false;
                SessionState.SetInt(KeyPhase, PhaseStop);
                if (EditorApplication.isPlaying)
                {
                    EditorApplication.isPlaying = false;
                }

                return;
            }

            if (!playing && phase != PhaseRestore)
            {
                SessionState.SetInt(KeyPhase, PhaseRestore);
                RestoreUserData();
            }
        }

        static void ClearSession()
        {
            SessionState.SetBool(KeyRunning, false);
            SessionState.EraseString(KeyOut);
            SessionState.EraseInt(KeyReduce);
            SessionState.EraseInt(KeySeed);
            SessionState.EraseString(KeyBackup);
            SessionState.EraseInt(KeyPhase);
            SessionState.EraseInt(KeyShot);
            SessionState.EraseInt(KeyWaited);
            SessionState.EraseInt(KeySeen);
            SessionState.EraseString(KeyError);
            SessionState.EraseString(KeyHud);
            SessionState.EraseString(KeyStart);
            SessionState.EraseString(KeyPlayStart);
            SessionState.EraseBool(KeyRestored);
            SessionState.EraseBool(KeySceneLogged);
            SessionState.EraseInt(KeyLoggedPhase);
            _backupPath = string.Empty;
        }

        static void Quit(int code)
        {
            if (code == 0)
            {
                Debug.Log("AgrCapture OK");
            }
            else
            {
                Debug.LogError("AgrCapture FAIL code " + code.ToString());
            }

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(code);
            }
        }
    }
}
