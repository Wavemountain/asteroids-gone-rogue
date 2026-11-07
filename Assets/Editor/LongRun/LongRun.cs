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
                + " profile=" + state.Profile);
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
            _logs = true;
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
                }

                return;
            }

            EnsureProbe();
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
                }

                return;
            }

            if (state.Phase == "waitResume")
            {
                Resume(state, game, session);
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
            if (state.Phase == "play")
            {
                Menu(state, game, session);
            }

            if (state.Phase == "play" && session.Phase == GamePhase.Playing)
            {
                Steer();
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

            if (!game.HasContinueOffer)
            {
                state.ResumeFailed = 1;
                state.Fault = "continue offer missing";
                LogEvent(state, "resume", "resume", WorldCatalog.NumberForWave(session.WaveIndex), session.WaveIndex, session.Credits, session.Lives, ",\"result\":\"FAIL\",\"mismatches\":\"continue offer missing\"");
                state.Phase = "stop";
                state.ExitAsked = 0;
                Save(state);
                return;
            }

            game.AcceptContinue();
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
            state.Phase = "play";
            state.Settle = 0;
            state.LastPhase = string.Empty;
            Save(state);
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
                    if (Elapsed(state) < state.Minutes * 60.0)
                    {
                        state.DiedEarly = 1;
                    }

                    state.Phase = "stop";
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
                }

                LogEvent(state, "transition", name, world, wave, session.Credits, lives, "");
                if (phaseChanged || waveChanged)
                {
                    state.Settle = 0;
                }
            }

            state.LastPhase = phase;
            state.LastWave = wave;
            state.LastWorld = world;
            state.LastLives = lives;
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

                Seed(state);
                game.ConfirmFirstStart(DifficultyGrade.Normal);
                LogEvent(state, "transition", "hangar", WorldOf(session), WaveOf(session), session.Credits, session.Lives, ",\"choice\":\"normal\"");
                state.Settle = 0;
                return;
            }

            if (game.TutorialActive)
            {
                return;
            }

            if (game.BoonChoiceOpen)
            {
                if (state.Settle < 5 || state.BoonWave == session.WaveIndex)
                {
                    return;
                }

                int offer = game.Boons != null ? game.Boons.OfferAt(0) : -1;
                game.TryChooseBoon(0);
                state.BoonWave = session.WaveIndex;
                LogEvent(
                    state,
                    "transition",
                    "boon",
                    WorldOf(session),
                    session.WaveIndex,
                    session.Credits,
                    session.Lives,
                    ",\"offer\":" + offer.ToString(CultureInfo.InvariantCulture));
                state.Settle = 0;
                return;
            }

            if (state.Settle < 15)
            {
                return;
            }

            if (state.BoughtWave != session.WaveIndex)
            {
                TryPurchase(state, session);
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

            Seed(state);
            game.StartWave();
            state.Settle = 0;
        }

        static void TryPurchase(LongRunState state, GameSession session)
        {
            HangarShop shop = UnityEngine.Object.FindAnyObjectByType<HangarShop>();
            if (shop == null || session.Phase == GamePhase.Failed)
            {
                return;
            }

            int before = session.Credits;
            if (session.Lives == 1 && shop.TryBuyExtraLife())
            {
                NotePurchase(state, session, "ExtraLife", before);
                return;
            }

            UpgradeId[] ids = (UpgradeId[])Enum.GetValues(typeof(UpgradeId));
            for (int index = 0; index < ids.Length; index++)
            {
                if (shop.TryBuy(ids[index]))
                {
                    NotePurchase(state, session, ids[index].ToString(), before);
                    return;
                }
            }
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
                + ",\"creditsBefore\":" + before.ToString(CultureInfo.InvariantCulture)
                + ",\"creditsAfter\":" + after.ToString(CultureInfo.InvariantCulture));
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

        static void Steer()
        {
            ShipController ship = UnityEngine.Object.FindAnyObjectByType<ShipController>();
            if (ship == null)
            {
                GamepadInput.SetEditorPump(true, 0f, 0f, 0f, 0f, false);
                return;
            }

            Vector3 pos = ship.transform.position;
            Vector3 fwd = ship.transform.forward;
            int count = 0;
            count = CollectAsteroids(pos, count);
            count = CollectEnemies(pos, count);
            count = CollectBolts(pos, count);
            float edge = 30f;
            ArenaShape shape = ArenaShapeCatalog.Active;
            if (shape != null)
            {
                edge = shape.DistanceToEdge(pos.x, pos.z);
            }

            LongRunPilot.Stick stick = LongRunPilot.Decide(pos.x, pos.z, fwd.x, fwd.z, Threats, count, edge);
            GamepadInput.SetEditorPump(true, stick.MoveX, stick.MoveY, stick.AimX, stick.AimY, stick.Fire);
        }

        static int CollectAsteroids(Vector3 pos, int count)
        {
            Asteroid[] found = UnityEngine.Object.FindObjectsByType<Asteroid>();
            if (found == null)
            {
                return count;
            }

            for (int index = 0; index < found.Length; index++)
            {
                Asteroid rock = found[index];
                if (rock == null)
                {
                    continue;
                }

                Vector3 at = rock.transform.position;
                count = Keep(pos, at.x, at.z, count);
            }

            return count;
        }

        static int CollectEnemies(Vector3 pos, int count)
        {
            EnemySeeker[] found = UnityEngine.Object.FindObjectsByType<EnemySeeker>();
            if (found == null)
            {
                return count;
            }

            for (int index = 0; index < found.Length; index++)
            {
                EnemySeeker enemy = found[index];
                if (enemy == null)
                {
                    continue;
                }

                Vector3 at = enemy.transform.position;
                count = Keep(pos, at.x, at.z, count);
            }

            return count;
        }

        static int CollectBolts(Vector3 pos, int count)
        {
            Projectile[] found = UnityEngine.Object.FindObjectsByType<Projectile>();
            if (found == null)
            {
                return count;
            }

            for (int index = 0; index < found.Length; index++)
            {
                Projectile bolt = found[index];
                if (bolt == null || !bolt.Hostile)
                {
                    continue;
                }

                Vector3 at = bolt.transform.position;
                count = Keep(pos, at.x, at.z, count);
            }

            return count;
        }

        static int Keep(Vector3 pos, float x, float z, int count)
        {
            if (count < LongRunPilot.ThreatCap)
            {
                Threats[count].X = x;
                Threats[count].Z = z;
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
            }

            return count;
        }

        static void Seed(LongRunState state)
        {
            if (state.Seed < 0)
            {
                return;
            }

            UnityEngine.Random.InitState(state.Seed);
        }

        static void EnsureProbe()
        {
            if (UnityEngine.Object.FindAnyObjectByType<LongRunFrameProbe>() != null)
            {
                return;
            }

            GameObject host = new GameObject("LongRunFrameProbe");
            host.hideFlags = HideFlags.HideAndDontSave;
            host.AddComponent<LongRunFrameProbe>();
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

            LongRunFrames.Flush();
            FlushLogs(state);
            string verdict = VerdictOf(state);
            string summaryPath = Path.Combine(state.OutDir, "summary.md");
            File.WriteAllText(summaryPath, Summary(state, verdict, savesOk, saveDetail, prefsOk, prefsDetail));
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

            EditorApplication.Exit(code);
        }

        static string VerdictOf(LongRunState state)
        {
            bool resumeConfigured = state.ResumeWorld > 0 || state.ResumeMinutes > 0;
            bool resumeNotRun = resumeConfigured && state.ResumeReached == 0;
            return LongRunVerdict.Classify(
                state.SaveFailed != 0,
                state.ResumeFailed != 0,
                state.HarnessFault != 0,
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
            builder.Append("\n- Elapsed seconds: ");
            builder.Append(Elapsed(state).ToString("0.0", CultureInfo.InvariantCulture));
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
            builder.Append("\n- Exceptions and error logs: ");
            builder.Append((state.Exceptions + state.Errors).ToString(CultureInfo.InvariantCulture));
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
            File.AppendAllText(Path.Combine(state.OutDir, "events.jsonl"), builder.ToString());
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
    }
}
