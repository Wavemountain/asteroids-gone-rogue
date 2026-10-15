using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace AsteroidsGoneRogue.EditorTools
{
    /// <summary>
    /// Batch capture for SpeltestBot side-by-side shots. Editor folder only, so
    /// the player build never includes it. The same file compiles on 0.47 and
    /// 0.48: gameplay types are reached by name, and a missing feature is logged
    /// and skipped. Do not pass -nographics; the post stack needs a GPU.
    ///
    /// "C:\Program Files\Unity\Hub\Editor\6000.6.0f1\Editor\Unity.exe" -batchmode -projectPath . -executeMethod AsteroidsGoneRogue.EditorTools.AgrCapture.Run -logFile - -captureOut Captures\compare -reduce 0 -seed 48048
    /// </summary>
    [InitializeOnLoad]
    public static class AgrCapture
    {
        const int Width = 1920;
        const int Height = 1080;
        const int ShotCount = 5;
        const int SettleFrames = 60;
        const int CardFrames = 3;
        const double TimeoutSeconds = 300.0;
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
        const string KeyStart = "AgrCapture.Start";
        const string KeyRestored = "AgrCapture.Restored";

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

        static AgrCapture()
        {
            if (SessionState.GetBool(KeyRunning, false))
            {
                EnsureHook();
            }
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

            try
            {
                string backup = System.IO.Path.Combine(
                    System.IO.Path.GetTempPath(),
                    "agr-capture-" + System.Diagnostics.Process.GetCurrentProcess().Id.ToString());
                System.IO.Directory.CreateDirectory(backup);
                ParkUserData(backup);
                ApplyTempPrefs(reduce);
                System.IO.Directory.CreateDirectory(outDir);
                EditorSceneManager.OpenScene(PlayScene);
                SessionState.SetBool(KeyRunning, true);
                SessionState.SetString(KeyOut, outDir);
                SessionState.SetInt(KeyReduce, reduce);
                SessionState.SetInt(KeySeed, seed);
                SessionState.SetString(KeyBackup, backup);
                SessionState.SetInt(KeyPhase, PhaseEnter);
                SessionState.SetInt(KeyShot, 0);
                SessionState.SetInt(KeyWaited, 0);
                SessionState.SetInt(KeySeen, -1);
                SessionState.SetString(KeyError, string.Empty);
                SessionState.SetBool(KeyRestored, false);
                SessionState.SetString(
                    KeyStart,
                    EditorApplication.timeSinceStartup.ToString(System.Globalization.CultureInfo.InvariantCulture));
                Debug.Log(
                    "AgrCapture: out=" + outDir
                    + " reduce=" + reduce.ToString()
                    + " seed=" + seed.ToString()
                    + " res=" + Width.ToString() + "x" + Height.ToString()
                    + " W1=wave 1, W3=world 3 wave 11, W4=world 4 wave 16, death=wave 1");
                EnsureHook();
            }
            catch (System.Exception bootEx)
            {
                Debug.LogError("AgrCapture FAIL: " + bootEx.Message);
                Quit(1);
            }
        }

        static void EnsureHook()
        {
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
        }

        static void Tick()
        {
            try
            {
                TickBody();
            }
            catch (System.Exception tickEx)
            {
                Debug.LogError("AgrCapture FAIL: " + tickEx);
                try
                {
                    RestoreUserData();
                }
                catch (System.Exception restoreEx)
                {
                    Debug.LogError("AgrCapture FAIL: restore " + restoreEx.Message);
                }

                ClearSession();
                EditorApplication.update -= Tick;
                Quit(1);
            }
        }

        static void TickBody()
        {
            int phase = SessionState.GetInt(KeyPhase, PhaseEnter);
            if (TimedOut() && phase != PhaseRestore && phase != PhaseWaitStop)
            {
                Fail("timed out after 300s");
                phase = SessionState.GetInt(KeyPhase, PhaseEnter);
            }

            if (phase == PhaseEnter)
            {
                if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    EditorApplication.isPlaying = true;
                    return;
                }

                if (EditorApplication.isPlaying)
                {
                    SessionState.SetInt(KeyPhase, PhaseWait);
                }

                return;
            }

            if (phase == PhaseWait)
            {
                Nudge();
                Component game = FindComponent("GameManager");
                if (game == null)
                {
                    return;
                }

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
                if (!SessionState.GetBool(KeyRestored, false))
                {
                    RestoreUserData();
                    SessionState.SetBool(KeyRestored, true);
                    string backup = SessionState.GetString(KeyBackup, string.Empty);
                    if (backup.Length > 0 && System.IO.Directory.Exists(backup))
                    {
                        System.IO.Directory.Delete(backup, true);
                    }
                }

                int code = 0;
                if (HasError())
                {
                    code = 1;
                }

                ClearSession();
                EditorApplication.update -= Tick;
                Quit(code);
            }
        }

        static void StageCurrent()
        {
            int shot = SessionState.GetInt(KeyShot, 0);
            bool reduce = SessionState.GetInt(KeyReduce, 0) != 0;
            Debug.Log("AgrCapture: staging " + ShotFile(shot, reduce));
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
            PinShip();
            KeepShipAlive();
            HoldForShot(shot);
            QuietBanners();
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
            int shot = SessionState.GetInt(KeyShot, 0);
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
        }

        static void AdvanceShot()
        {
            int shot = SessionState.GetInt(KeyShot, 0) + 1;
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
            if (body == null)
            {
                return;
            }

            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
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
        }

        static bool PinDeath(object game)
        {
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

            PinVignette(mid);
            if (SessionState.GetInt(KeyReduce, 0) != 0)
            {
                Debug.Log("AgrCapture: reduce is on; slow-mo stays off. Death vignette pinned at " + mid.ToString("0.00") + "s");
            }
            else
            {
                Debug.Log("AgrCapture: pinned death mid slow-mo at " + mid.ToString("0.00") + "s");
            }

            return true;
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
            PlayerPrefs.SetInt("agr.settings.reduceEffects", reduce);
            PlayerPrefs.SetString("agr.difficulty", "normal");
            PlayerPrefs.SetInt("agr.ui.firstHangarHint", 1);
            PlayerPrefs.SetInt("agr.ui.doctrineHint", 1);
            PlayerPrefs.SetInt("agr.settings.assistMode", 0);
            PlayerPrefs.Save();
        }

        static void RestoreUserData()
        {
            string backup = SessionState.GetString(KeyBackup, string.Empty);
            if (backup.Length == 0)
            {
                return;
            }

            ReadPrefs(System.IO.Path.Combine(backup, "prefs.txt"));
            ReleaseRuntimeCache();
            PlayerPrefs.Save();
            string data = Application.persistentDataPath;
            MirrorBack(System.IO.Path.Combine(backup, "persistent"), data);
            Time.captureDeltaTime = 0f;
            AudioListener.pause = false;
            AudioListener.volume = 1f;
            EditorApplication.isPaused = false;
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

        static bool HasError()
        {
            return SessionState.GetString(KeyError, string.Empty).Length > 0;
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
                return;
            }

            if (!playing && phase != PhaseRestore)
            {
                SessionState.SetInt(KeyPhase, PhaseRestore);
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
            SessionState.EraseString(KeyStart);
            SessionState.EraseBool(KeyRestored);
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
