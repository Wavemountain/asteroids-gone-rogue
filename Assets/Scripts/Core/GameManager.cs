using System;
using UnityEngine;

namespace AsteroidsGoneRogue
{
    public sealed class GameManager : MonoBehaviour
    {
        private GameSession _session;
        private PlayerLoadout _loadout;
        private WaveManager _waves;
        private HangarShop _shop;
        private GameUi _ui;
        private ContentFactory _factory;
        private ShipController _ship;
        private HangarShipPreview _hangarPreview;
        private FollowCamera _follow;

        public GameSession Session
        {
            get { return _session; }
        }

        public event Action StateChanged;

        public bool LastRunWasNewBest { get; private set; }

        public LocalBest Best { get; private set; }

        public LocalBest SessionBest { get; private set; }

        public HangarPersist Persist { get; private set; }

        public SinkProfileData Sinks { get; private set; }

        public AchievementPersist Achievements { get; private set; }

        public MetaData Meta { get; private set; }

        public RunSaveData PendingContinue { get; private set; }

        public int ActiveRunId { get; private set; }

        public ShipHealth PlayerHealth
        {
            get { return _ship != null ? _ship.Health : null; }
        }

        private readonly BoonRun _boonRun = new BoonRun();
        private int _boonChosenFrame = -1;

        public BoonRun Boons
        {
            get { return _boonRun; }
        }

        public bool BoonChoiceOpen
        {
            get { return _boonRun != null && _boonRun.Pending; }
        }

        private bool _runLaunched;

        private bool _legacyApplied;

        private readonly TutorialRun _tutorial = new TutorialRun();

        private bool _skipTutorialRedirect;

        private float _tutorialEmptySeconds;

        private int _tutorialLives = DifficultySettings.NormalStartLives;

        private int _tutorialStreak;

        public bool TutorialActive
        {
            get { return _tutorial != null && _tutorial.Active; }
        }

        public int TutorialPrompt
        {
            get { return _tutorial != null ? _tutorial.Prompt : 0; }
        }

        public bool TutorialPending
        {
            get { return FirstRunRules.TutorialPending(Meta); }
        }

        public bool ShowDifficultyChooser
        {
            get
            {
                if (HasContinueOffer)
                {
                    return false;
                }

                int chosenFlag = Meta != null ? Meta.DifficultyChosen : 0;
                bool prefsPresent = DifficultySettings.HasSavedChoice();
                bool hasProgress = FirstRunRules.HasRunOrHighscore(Meta);
                if (!hasProgress && Best != null && Best.HasRecord)
                {
                    hasProgress = true;
                }

                return FirstRunRules.ShowDifficultyChooser(chosenFlag, prefsPresent, hasProgress);
            }
        }

        public bool HasContinueOffer
        {
            get { return PendingContinue != null; }
        }

        public bool LegacyShopOpen
        {
            get
            {
                GamePhase phase = _session != null ? _session.Phase : GamePhase.Hangar;
                return LegacyProgress.ShopVisible(phase, HasContinueOffer, _runLaunched);
            }
        }

        public void Initialize(
            GameSession session,
            PlayerLoadout loadout,
            WaveManager waves,
            HangarShop shop,
            GameUi ui,
            ContentFactory factory,
            ShipController ship)
        {
            _session = session;
            _loadout = loadout;
            _waves = waves;
            _shop = shop;
            _ui = ui;
            _factory = factory;
            _ship = ship;
            _hangarPreview = ship != null ? ship.GetComponent<HangarShipPreview>() : null;
            _follow = UnityEngine.Object.FindAnyObjectByType<FollowCamera>();
            Best = LocalBest.Load();
            SessionBest = new LocalBest();
            Persist = HangarPersist.Load();
            Sinks = SinkProfileStore.Load();
            SinkRuntime.Apply(Sinks);
            Achievements = AchievementPersist.Load();
            Meta = RunSaveStore.LoadMeta();
            RunSaveData loaded;
            if (RunSaveStore.TryLoadRun(out loaded))
            {
                PendingContinue = loaded;
            }

            LastRunWasNewBest = false;
        }

        public void EnterHangar()
        {
            DifficultySettings.EnsureLoaded();
            if (PendingContinue != null && RunSaveCodec.AbandonedWaveEndsRun(PendingContinue))
            {
                ConcludeAbandonedLastLife(PendingContinue);
                return;
            }

            if (_session.Lives <= 0)
            {
                ResetFullRun();
            }
            else if (PendingContinue == null)
            {
                EnsureRunId();
                ApplyLegacyToNewRun();
            }

            _session.ReturnToHangar();
            _ship.SetInputEnabled(false);
            _ship.ResetForWave(_loadout.State);
            _factory.ApplyLoadoutVisuals(_ship, _loadout.State);
            PushPreviewFittings(_loadout != null ? _loadout.State : null);
            RaiseStateChanged();
        }

        private void PushPreviewFittings(LoadoutState state)
        {
            if (_hangarPreview == null)
            {
                return;
            }

            int upgradeMask = state != null ? RunSaveCodec.PackUpgrades(state) : 0;
            int mk2Mask = state != null ? state.Mk2Mask : 0;
            int paintId = Sinks != null ? Sinks.Paint : -1;
            int trailId = Sinks != null ? Sinks.Trail : -1;
            _hangarPreview.SetFittings(upgradeMask, mk2Mask, paintId, trailId);
        }

        public void SetDifficulty(DifficultyGrade grade)
        {
            DifficultySettings.SetGrade(grade);
            if (Meta == null)
            {
                Meta = MetaData.Fresh();
            }

            FirstRunRules.MarkDifficultyChosen(Meta);
            RunSaveStore.TrySaveMeta(Meta);
            if (_session != null && _session.Phase != GamePhase.Playing)
            {
                _session.ResetLives(DifficultySettings.StartLives);
            }

            if (_ship != null && _session != null && _session.Phase != GamePhase.Playing)
            {
                _ship.ResetForWave(_loadout.State);
                _factory.ApplyLoadoutVisuals(_ship, _loadout.State);
            }

            RaiseStateChanged();
        }

        public void PreviewUpgrade(UpgradeId id)
        {
            if (_session == null || _session.Phase == GamePhase.Playing || _loadout == null)
            {
                return;
            }

            LoadoutState preview = _loadout.State.WithPreview(id);
            _factory.ApplyLoadoutVisuals(_ship, preview, _loadout.State);
            if (_hangarPreview != null)
            {
                _hangarPreview.SetInteractionHold(true);
                PushPreviewFittings(preview);
                _hangarPreview.NotifyVisualsChanged();
            }
        }

        public void ClearUpgradePreview()
        {
            if (_ship == null || _loadout == null)
            {
                return;
            }

            _factory.ApplyLoadoutVisuals(_ship, _loadout.State);
            if (_hangarPreview != null)
            {
                _hangarPreview.SetInteractionHold(false);
                PushPreviewFittings(_loadout.State);
                _hangarPreview.NotifyVisualsChanged();
            }
        }

        public bool TryBuySink(int sinkId)
        {
            if (_session == null || !_session.ShopOpen)
            {
                return false;
            }

            if (Sinks == null)
            {
                Sinks = SinkProfileCodec.Fresh();
            }

            SinkProfileData next;
            int price;
            if (!SinkRules.TryPurchase(Sinks, sinkId, _session.Credits, out next, out price))
            {
                return false;
            }

            if (price > 0 && !_session.TrySpend(price))
            {
                return false;
            }

            Sinks = next;
            SinkProfileStore.Save(Sinks);
            SinkRuntime.Apply(Sinks);
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayHangarPurchase();
            }

            NotifyLoadoutChanged();
            return true;
        }

        public bool TryGrantExtraLife()
        {
            if (TutorialActive)
            {
                return false;
            }

            if (_session == null || !_session.TryGainLife())
            {
                return false;
            }

            _session.NoteExtraLife();
            if (AchievementCatalog.ShouldUnlockExtraLifeStreak(_session.ExtraLifeStreak))
            {
                TryUnlockAchievement(AchievementId.ExtraLifeStreak);
            }

            RaiseStateChanged();
            return true;
        }

        public void NotifyPlayerHit()
        {
            if (_session == null || _session.Phase != GamePhase.Playing)
            {
                return;
            }

            _session.MarkWaveHit();
            PersistWaveVitals();
        }

        /// <summary>
        /// Editor stop and a real quit do not pass through the hangar writer.
        /// Refresh the in-progress snapshot so Continue keeps the hull and
        /// shield the ship actually has, not the full bar from wave start.
        /// </summary>
        private void OnApplicationQuit()
        {
            PersistWaveVitals();
        }

        public bool TryChooseBoon(int index)
        {
            if (_boonRun == null || Time.frameCount == _boonChosenFrame)
            {
                return false;
            }

            if (!_boonRun.TryChoose(index))
            {
                return false;
            }

            _boonChosenFrame = Time.frameCount;
            BoonHooks.Sync(_boonRun);
            RaiseStateChanged();
            return true;
        }

        public void StartWave()
        {
            bool fromRetry = _skipTutorialRedirect;
            _skipTutorialRedirect = false;
            if (!_session.CanStartWave)
            {
                return;
            }

            if (!fromRetry && ShowDifficultyChooser)
            {
                return;
            }

            if (!fromRetry && _session.Phase == GamePhase.Hangar && TutorialPending)
            {
                StartTutorial();
                return;
            }

            if (BoonChoiceOpen)
            {
                return;
            }

            bool retrying = _session.Phase == GamePhase.Failed;
            if (GameSession.PrimaryRestartsRun(_session.Phase))
            {
                ResetFullRun();
                if (_ship != null)
                {
                    _ship.SetInputEnabled(false);
                    _ship.ResetForWave(_loadout.State);
                    _factory.ApplyLoadoutVisuals(_ship, _loadout.State);
                }

                RaiseStateChanged();
                return;
            }

            if (_hangarPreview != null)
            {
                _hangarPreview.SetActive(false);
            }

            _runLaunched = true;
            EnsureRunId();
            _session.BeginWave();
            _session.MarkWaveStarted();
            BoonHooks.RunSeed = ActiveRunId > 0 ? ActiveRunId : 1;
            _ship.ResetForWave(_loadout.State, false, true);
            ApplyWaveFairness();
            _factory.ApplyLoadoutVisuals(_ship, _loadout.State);
            _ship.SetInputEnabled(true);
            int world = ContentFactory.WorldIndexForWave(_session.WaveIndex);
            TryAwardWorldMedal(world);
            if (AchievementCatalog.ShouldUnlockDeepOrbit(_session.WaveIndex))
            {
                TryUnlockAchievement(AchievementId.DeepOrbit);
            }

            _waves.SpawnWave(_session.WaveIndex);
            bool eliteWave = WaveModifier.IsElite(_session.WaveIndex);
            if (eliteWave && _ui != null)
            {
                _ui.AnnounceEliteWave(_session.WaveIndex);
            }
            else
            {
                string beat = MedalCatalog.WorldEntryBeat(world);
                if (!string.IsNullOrEmpty(beat) && _ui != null)
                {
                    _ui.AnnounceMedalBeat(beat, MedalCatalog.WorldEntryFlashSeconds(world));
                }
            }

            if (!retrying && eliteWave && AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayEliteSting();
            }

            RaiseStateChanged();
            WriteWaveProgress();
        }

        public void ContinueFromResults()
        {
            if (_session.Phase == GamePhase.WaveClear || _session.Phase == GamePhase.Failed)
            {
                _session.ReturnToHangar();
                _ship.SetInputEnabled(false);
                PreserveShip(true);
                RaiseStateChanged();
            }
        }

        public void AbortWave()
        {
            if (TutorialActive)
            {
                SkipTutorial();
                return;
            }

            if (_session == null || _session.Phase != GamePhase.Playing)
            {
                return;
            }

            if (_ship != null)
            {
                _ship.SetInputEnabled(false);
            }

            if (_waves != null)
            {
                _waves.DespawnAll();
            }

            _session.AbortToHangar();
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayAbortWhoosh();
            }

            if (_ship != null)
            {
                PreserveShip(true);
            }

            RaiseStateChanged();
        }

        public void NotifySoftLockAbort()
        {
            if (TutorialActive)
            {
                int threatsLeft = _waves != null ? _waves.RemainingThreats : 0;
                if (FirstRunRules.ShouldFinishEmptyTutorial(threatsLeft, TutorialPrompt, _tutorialEmptySeconds))
                {
                    FinishTutorial();
                    return;
                }

                if (threatsLeft <= 0)
                {
                    return;
                }

                SoftResetTutorialShip();
                return;
            }

            AbortWave();
        }

        public void SetSoftLockHint(bool stranded)
        {
            if (_ui != null)
            {
                _ui.SetAbortUrgent(stranded);
            }
        }

        public void AddBonusScore(int amount)
        {
            if (TutorialActive)
            {
                return;
            }

            if (_session == null || _session.Phase != GamePhase.Playing || amount <= 0)
            {
                return;
            }

            _session.AddScore(amount);
            RaiseStateChanged();
        }

        public void NotifyThreatDestroyed(int scoreValue)
        {
            if (_session.Phase != GamePhase.Playing)
            {
                return;
            }

            if (!TutorialActive)
            {
                _session.AddScore(scoreValue);
            }

            if (_waves.RemainingThreats <= 0)
            {
                if (TutorialActive)
                {
                    FinishTutorial();
                }
                else
                {
                    CompleteWave();
                }
            }
            else
            {
                RaiseStateChanged();
            }
        }

        public void NoteTutorialPickup()
        {
            if (_tutorial == null || !TutorialActive)
            {
                return;
            }

            _tutorial.NotePickup();
        }

        public void TickTutorial(bool moved, bool fired)
        {
            if (_tutorial == null || !TutorialActive)
            {
                return;
            }

            int before = _tutorial.Prompt;
            if (moved)
            {
                _tutorial.NoteMove();
            }

            if (_tutorial.Prompt == before && fired)
            {
                _tutorial.NoteFire();
            }

            if (_tutorial.Prompt == before && _waves != null)
            {
                _tutorial.NoteThreats(_waves.RemainingThreats);
            }

            int threatsLeft = _waves != null ? _waves.RemainingThreats : 0;
            float delta = Time.unscaledDeltaTime;
            _tutorialEmptySeconds = FirstRunRules.NextEmptySeconds(_tutorialEmptySeconds, threatsLeft, delta);
            if (FirstRunRules.ShouldFinishEmptyTutorial(threatsLeft, _tutorial.Prompt, _tutorialEmptySeconds))
            {
                FinishTutorial();
            }
        }

        public void SkipTutorial()
        {
            if (TutorialActive)
            {
                FinishTutorial();
                return;
            }

            if (!TutorialPending)
            {
                return;
            }

            if (Meta == null)
            {
                Meta = MetaData.Fresh();
            }

            FirstRunRules.MarkTutorialDone(Meta);
            RunSaveStore.TrySaveMeta(Meta);
            RaiseStateChanged();
        }

        /// <summary>
        /// Close the first-start card. Stores the highlighted grade, or Normal
        /// when the player never moved off the default, and skips the guided wave.
        /// </summary>
        public void SkipFirstStart(DifficultyGrade grade)
        {
            DifficultyGrade stored = FirstRunRules.GradeOnFirstSkip(true, grade);
            if (TutorialActive)
            {
                SetDifficulty(stored);
                FinishTutorial();
                return;
            }

            if (Meta == null)
            {
                Meta = MetaData.Fresh();
            }

            FirstRunRules.MarkTutorialDone(Meta);
            SetDifficulty(stored);
            if (_session != null && _session.Phase != GamePhase.Playing)
            {
                _session.ReturnToHangar();
            }

            RaiseStateChanged();
        }

        public void ConfirmFirstStart(DifficultyGrade grade)
        {
            if (_session != null && _session.Phase == GamePhase.Playing)
            {
                return;
            }

            SetDifficulty(grade);
            if (TutorialPending)
            {
                StartTutorial();
                return;
            }

            StartWave();
        }

        public void OneMoreTry()
        {
            if (_session == null || !FirstRunRules.OneMoreTryIsPrimary(_session.Phase))
            {
                return;
            }

            DifficultyGrade kept = DifficultySettings.Current;
            _skipTutorialRedirect = true;
            ResetFullRun();
            if (DifficultySettings.Current != kept)
            {
                DifficultySettings.SetGrade(kept);
            }

            StartWave();
            if (_ui != null)
            {
                _ui.FlashRetry();
            }
        }

        public void NotifyPlayerDestroyed()
        {
            NotifyPlayerDestroyed("Unknown cause");
        }

        public void NotifyPlayerDestroyed(string cause)
        {
            if (!BeginPlayerDeath())
            {
                return;
            }

            if (TryRespawnAfterLifeLoss())
            {
                return;
            }

            FailRun(cause, false, DamageCause.Unknown, EnemyKind.Mid01);
        }

        public void NotifyPlayerDestroyed(DamageCause cause, EnemyKind kind)
        {
            if (!BeginPlayerDeath())
            {
                return;
            }

            if (TryRespawnAfterLifeLoss())
            {
                return;
            }

            FailRun(null, true, cause, kind);
        }

        private bool BeginPlayerDeath()
        {
            if (_session == null || _session.Phase != GamePhase.Playing)
            {
                return false;
            }

            if (TutorialActive)
            {
                SoftResetTutorialShip();
                return false;
            }

            return true;
        }

        private void SoftResetTutorialShip()
        {
            if (_ship == null)
            {
                return;
            }

            LoadoutState shipLoadout = _loadout != null ? _loadout.State : null;
            _ship.ResetForWave(shipLoadout);
            if (_factory != null)
            {
                _factory.ApplyLoadoutVisuals(_ship, shipLoadout);
            }

            if (_ship.Health != null)
            {
                _ship.Health.GrantRespawnIFrames();
            }

            _ship.SetInputEnabled(true);
        }

        private void StartTutorial()
        {
            if (_session == null || !_session.CanStartWave || _waves == null)
            {
                return;
            }

            if (_hangarPreview != null)
            {
                _hangarPreview.SetActive(false);
            }

            if (_tutorial != null)
            {
                _tutorial.Begin();
            }

            _tutorialLives = _session.Lives;
            _tutorialStreak = _session.ExtraLifeStreak;
            _tutorialEmptySeconds = 0f;
            _session.BeginWave();
            if (_ship != null)
            {
                LoadoutState shipLoadout = _loadout != null ? _loadout.State : null;
                _ship.ResetForWave(shipLoadout);
                if (_factory != null)
                {
                    _factory.ApplyLoadoutVisuals(_ship, shipLoadout);
                }

                _ship.SetInputEnabled(true);
            }

            _waves.SpawnTutorial();
            RaiseStateChanged();
        }

        private void FinishTutorial()
        {
            if (_waves != null)
            {
                _waves.DespawnAll();
            }

            if (_tutorial != null)
            {
                _tutorial.Clear();
            }

            _tutorialEmptySeconds = 0f;
            if (_session != null)
            {
                _session.FinishTutorialToHangar();
                int keptLives = FirstRunRules.LivesAfterTutorial(_tutorialLives);
                _session.RestoreAfterTutorial(keptLives, _tutorialStreak);
            }

            if (Meta == null)
            {
                Meta = MetaData.Fresh();
            }

            FirstRunRules.MarkTutorialDone(Meta);
            RunSaveStore.TrySaveMeta(Meta);
            if (_ship != null)
            {
                _ship.SetInputEnabled(false);
                LoadoutState shipLoadout = _loadout != null ? _loadout.State : null;
                _ship.ResetForWave(shipLoadout);
                if (_factory != null)
                {
                    _factory.ApplyLoadoutVisuals(_ship, shipLoadout);
                }

                if (_ship.Shooter != null)
                {
                    _ship.Shooter.ClearRapidBoost();
                }
            }

            RaiseStateChanged();
        }

        public void NoteAssistUsed()
        {
            if (_session != null)
            {
                _session.NoteAssist();
            }
        }

        private void ApplyWaveFairness()
        {
            if (SettingsState.AssistEnabled)
            {
                NoteAssistUsed();
            }

            if (_ship == null || _ship.Health == null)
            {
                return;
            }

            ArenaHazard.SetListener(_ship.transform);
            SpikeProximityWatch watch = GetComponent<SpikeProximityWatch>();
            if (watch == null)
            {
                watch = gameObject.AddComponent<SpikeProximityWatch>();
            }

            watch.Bind(_ship.transform, _ship.Health);
            if (SettingsState.AssistEnabled)
            {
                int raised = AssistRules.BonusShieldAtWaveStart(
                    _ship.Health.Shield,
                    _ship.Health.MaxShield,
                    true);
                _ship.Health.SetShield(raised);
            }

            _ship.Health.GrantWaveImmunity();
        }

        private void RememberShipDeath()
        {
            string card = string.Empty;
            if (_ship != null && _ship.Health != null)
            {
                card = _ship.Health.Hits.CardText();
            }

            if (_session != null)
            {
                _session.RememberDeathCard(card);
            }
        }

        private bool TryRespawnAfterLifeLoss()
        {
            if (_session == null || !_session.TryLoseLife())
            {
                return false;
            }

            RememberShipDeath();
            _session.NoteLifeLost();
            if (_ship != null)
            {
                _ship.ResetForWave(_loadout.State);
                _factory.ApplyLoadoutVisuals(_ship, _loadout.State);
                if (_ship.Health != null)
                {
                    _ship.Health.GrantRespawnIFrames();
                }

                _ship.SetInputEnabled(true);
            }

            // The wave-start file still has the old life count. Refresh the
            // in-progress snapshot (same writer as hull and shield) after the
            // respawn so a quit keeps the lives the ship actually has.
            WriteWaveProgress();

            if (_ui != null)
            {
                _ui.AnnounceLifeLost(_session.Lives, _session.DeathCard);
            }

            RaiseStateChanged();
            return true;
        }

        private void FailRun(string cause, bool structured, DamageCause failCause, EnemyKind kind)
        {
            if (TutorialActive || !FirstRunRules.CanFailRun(TutorialActive))
            {
                return;
            }

            int remaining = _waves != null ? _waves.RemainingThreats : 0;
            RememberShipDeath();
            if (_ship != null)
            {
                _ship.SetInputEnabled(false);
            }

            if (_waves != null)
            {
                _waves.DespawnAll();
            }

            if (structured)
            {
                _session.FailWave(failCause, kind, remaining);
            }
            else
            {
                _session.FailWave(cause, remaining);
            }

            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayWaveFail();
            }

            RecordBest(_session.WaveIndex);
            AwardLegacy(true);
            PendingContinue = null;
            RunSaveStore.DeleteRun();
            ClearBoons();
            RaiseStateChanged();
        }

        public void AcceptContinue()
        {
            if (PendingContinue == null || _session == null || _loadout == null || _loadout.State == null)
            {
                return;
            }

            if (Meta == null)
            {
                Meta = MetaData.Fresh();
            }

            Meta.TutorialDone = FirstRunRules.TutorialFlagAfterContinue();
            FirstRunRules.MarkDifficultyChosen(Meta);
            RunSaveData data = PendingContinue;
            if (RunSaveCodec.AbandonedWaveEndsRun(data))
            {
                ConcludeAbandonedLastLife(data);
                return;
            }

            PendingContinue = null;
            ActiveRunId = data.RunId;
            _runLaunched = true;
            _legacyApplied = true;
            DifficultyGrade grade = DifficultyGrade.Normal;
            if (data.Difficulty == (int)DifficultyGrade.Easy)
            {
                grade = DifficultyGrade.Easy;
            }
            else if (data.Difficulty == (int)DifficultyGrade.Hard)
            {
                grade = DifficultyGrade.Hard;
            }

            DifficultySettings.SetGrade(grade);
            if (Meta == null)
            {
                Meta = MetaData.Fresh();
            }

            int metaBank = LegacyProgress.BankedCount(Meta, data.RunId);
            int mergedBank = LegacyProgress.MergedBanked(data.BankedLegacy, metaBank);
            data.BankedLegacy = mergedBank;
            LegacyProgress.RememberBanked(Meta, data.RunId, mergedBank);
            RunSaveStore.TrySaveMeta(Meta);
            _session.ReadContinue(0, 0, data.BankedLegacy, data.ExtraLifeWorld);
            RunSaveCodec.ApplyAbandonedWave(data);
            _loadout.State.RestoreSnapshot(
                data.UpgradeMask,
                data.Shield,
                data.Doctrine,
                data.PrimaryMode,
                data.UtilityMode,
                data.HasUtility,
                data.LegacyHull,
                data.FirstDiscount,
                data.FirstDiscountUsed);
            _session.RestoreHangar(
                data.WaveIndex,
                data.Score,
                data.Credits,
                data.Lives,
                data.LastResolvedWave,
                data.LastRunScore,
                data.LastCreditsAwarded,
                data.ExtraLifeStreak);
            _session.ReadContinue(data.WaveInProgress, data.LivesAtWaveStart, data.BankedLegacy, data.ExtraLifeWorld);
            if (data.AssistUsed != 0)
            {
                _session.NoteAssist();
            }
            _loadout.State.SetMk2Mask(data.Mk2Mask);
            _boonRun.ReadSave(data.BoonLevels, data.BoonPending, data.BoonOffer);
            BoonHooks.Sync(_boonRun);
            BoonHooks.RunSeed = ActiveRunId > 0 ? ActiveRunId : 1;
            if (_ship != null)
            {
                _ship.SetInputEnabled(false);
                bool refillVitals = data.HullNow < 0;
                _ship.ResetForWave(_loadout.State, refillVitals, false);
                if (!refillVitals && _ship.Health != null)
                {
                    _ship.Health.SetHull(data.HullNow);
                    if (data.ShieldNow >= 0)
                    {
                        _ship.Health.SetShield(data.ShieldNow);
                    }
                }

                if (_factory != null)
                {
                    _factory.ApplyLoadoutVisuals(_ship, _loadout.State);
                }
            }

            RaiseStateChanged();
        }

        /// <summary>
        /// Mid-wave quit on the last life ends the run. Legacy is awarded once
        /// for this run id and the save is deleted. Continue is not offered.
        /// </summary>
        private void ConcludeAbandonedLastLife(RunSaveData doomedSave)
        {
            PendingContinue = null;
            if (doomedSave == null || _session == null)
            {
                RunSaveStore.DeleteRun();
                return;
            }

            ActiveRunId = doomedSave.RunId;
            _runLaunched = true;
            if (_loadout != null && _loadout.State != null)
            {
                _loadout.State.RestoreSnapshot(
                    doomedSave.UpgradeMask,
                    doomedSave.Shield,
                    doomedSave.Doctrine,
                    doomedSave.PrimaryMode,
                    doomedSave.UtilityMode,
                    doomedSave.HasUtility,
                    doomedSave.LegacyHull,
                    doomedSave.FirstDiscount,
                    doomedSave.FirstDiscountUsed);
                _loadout.State.SetMk2Mask(doomedSave.Mk2Mask);
            }

            _session.RestoreHangar(
                doomedSave.WaveIndex,
                doomedSave.Score,
                doomedSave.Credits,
                doomedSave.Lives,
                doomedSave.LastResolvedWave,
                doomedSave.LastRunScore,
                doomedSave.LastCreditsAwarded,
                doomedSave.ExtraLifeStreak);
            _session.ReadContinue(0, 0, doomedSave.BankedLegacy, doomedSave.ExtraLifeWorld);
            _session.FailAbandonedRun();
            if (Meta == null)
            {
                Meta = MetaData.Fresh();
            }

            int closedWorlds;
            int closedHighest;
            LegacyProgress.ProgressOf(doomedSave.WaveIndex, true, out closedWorlds, out closedHighest);
            LegacyProgress.TryAward(Meta, doomedSave.RunId, closedWorlds, closedHighest);
            if (closedHighest > 0)
            {
                int closedWorldNumber = WorldCatalog.NumberForWave(closedHighest);
                LegacyProgress.TryRecordBest(Meta, doomedSave.Difficulty, doomedSave.Score, closedHighest, closedWorldNumber);
            }

            RunSaveStore.TrySaveMeta(Meta);
            RunSaveStore.DeleteRun();
            ClearBoons();
            if (_ship != null)
            {
                _ship.SetInputEnabled(false);
                _ship.ResetForWave(_loadout.State);
                if (_factory != null)
                {
                    _factory.ApplyLoadoutVisuals(_ship, _loadout.State);
                }
            }

            RaiseStateChanged();
        }

        public void AbandonSavedRun()
        {
            RunSaveData pending = PendingContinue;
            if (pending != null)
            {
                int worldsCleared;
                int highestWave;
                LegacyProgress.ProgressOf(pending.WaveIndex, false, out worldsCleared, out highestWave);
                if (Meta == null)
                {
                    Meta = MetaData.Fresh();
                }

                LegacyProgress.TryAward(Meta, pending.RunId, worldsCleared, highestWave);
                if (highestWave > 0)
                {
                    int worldNumber = WorldCatalog.NumberForWave(highestWave);
                    LegacyProgress.TryRecordBest(Meta, pending.Difficulty, pending.Score, highestWave, worldNumber);
                }

                RunSaveStore.TrySaveMeta(Meta);
            }

            PendingContinue = null;
            RunSaveStore.DeleteRun();
            ResetFullRun();
            if (_ship != null)
            {
                _ship.SetInputEnabled(false);
                _ship.ResetForWave(_loadout.State);
                if (_factory != null)
                {
                    _factory.ApplyLoadoutVisuals(_ship, _loadout.State);
                }
            }

            RaiseStateChanged();
        }

        public bool TryBuyLegacy(int perk)
        {
            if (Meta == null || !LegacyShopOpen)
            {
                return false;
            }

            if (!LegacyProgress.TryBuy(Meta, perk))
            {
                return false;
            }

            RunSaveStore.TrySaveMeta(Meta);
            if (FreshEnoughToReinit())
            {
                ReapplyLegacy();
            }

            RaiseStateChanged();
            return true;
        }

        private void ResetFullRun()
        {
            _runLaunched = false;
            _legacyApplied = false;
            ActiveRunId = 0;
            if (_loadout != null && _loadout.State != null)
            {
                _loadout.State.Reset();
            }

            if (_session != null)
            {
                _session.ResetRun();
            }

            ClearBoons();
            BoonHooks.ResetRunCounters();
            EnsureRunId();
            BoonHooks.RunSeed = ActiveRunId > 0 ? ActiveRunId : 1;
            ApplyLegacyToNewRun();
        }

        public void GrantBankedLegacy()
        {
            if (Meta == null)
            {
                Meta = MetaData.Fresh();
            }

            Meta.LegacyPoints += 1;
            RunSaveStore.TrySaveMeta(Meta);
        }

        public bool TryBuyHullRepair()
        {
            if (!ServiceShopOpen() || _ship == null || _ship.Health == null)
            {
                return false;
            }

            if (_ship.Health.Hull >= _ship.Health.MaxHull || _ship.Health.Hull <= 0)
            {
                return false;
            }

            int repairPrice = ShopPrices.ApplyWorld(ShopPrices.HullRepairCost, ShopWorld());
            if (!_session.TrySpend(repairPrice))
            {
                return false;
            }

            _ship.Health.RefillHull();
            PlayServicePurchase();
            NotifyLoadoutChanged();
            return true;
        }

        public bool TryBuyShieldRefill()
        {
            if (!ServiceShopOpen() || _ship == null || _ship.Health == null)
            {
                return false;
            }

            if (_ship.Health.MaxShield <= 0 || _ship.Health.Shield >= _ship.Health.MaxShield)
            {
                return false;
            }

            int refillPrice = ShopPrices.ApplyWorld(ShopPrices.ShieldRefillCost, ShopWorld());
            if (!_session.TrySpend(refillPrice))
            {
                return false;
            }

            _ship.Health.RefillShield();
            PlayServicePurchase();
            NotifyLoadoutChanged();
            return true;
        }

        public bool TryBuyExtraLife()
        {
            if (!ServiceShopOpen())
            {
                return false;
            }

            int lifePrice = ShopPrices.ApplyWorld(ShopPrices.ExtraLifeCost, ShopWorld());
            if (_session.Credits < lifePrice || !_session.CanBuyExtraLife(ShopWorld()))
            {
                return false;
            }

            if (!_session.TrySpend(lifePrice))
            {
                return false;
            }

            if (!_session.TryBuyExtraLife(ShopWorld()))
            {
                _session.GrantCredits(lifePrice);
                return false;
            }

            PlayServicePurchase();
            NotifyLoadoutChanged();
            return true;
        }

        public bool TryBankCredits()
        {
            if (!ServiceShopOpen())
            {
                return false;
            }

            if (!_session.TryBankCredits())
            {
                return false;
            }

            if (Meta == null)
            {
                Meta = MetaData.Fresh();
            }

            EnsureRunId();
            LegacyProgress.RememberBanked(Meta, ActiveRunId, _session.BankedLegacy);
            GrantBankedLegacy();
            PlayServicePurchase();
            NotifyLoadoutChanged();
            return true;
        }

        private bool ServiceShopOpen()
        {
            return _session != null && _session.ShopOpen && _loadout != null;
        }

        private int ShopWorld()
        {
            int wave = _session != null ? _session.WaveIndex : 1;
            return WorldCatalog.NumberForWave(wave);
        }

        private static void PlayServicePurchase()
        {
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayHangarPurchase();
            }
        }

        private void PreserveShip(bool mend)
        {
            if (_ship == null || _loadout == null)
            {
                return;
            }

            _ship.ResetForWave(_loadout.State, false, false);
            if (mend)
            {
                MendBetweenWorlds();
            }

            if (_factory != null)
            {
                _factory.ApplyLoadoutVisuals(_ship, _loadout.State);
            }
        }

        private void MendBetweenWorlds()
        {
            if (_ship == null || _ship.Health == null || Meta == null)
            {
                return;
            }

            int hullLevel = LegacyProgress.Level(Meta, LegacyProgress.HullPerk);
            int mended = LegacyProgress.MendHull(_ship.Health.Hull, _ship.Health.MaxHull, hullLevel);
            _ship.Health.SetHull(mended);
        }

        private void ApplyShieldDuration()
        {
            if (_ship == null || _ship.Health == null || Meta == null)
            {
                return;
            }

            int shieldLevel = LegacyProgress.Level(Meta, LegacyProgress.ShieldPerk);
            _ship.Health.SetIFramePercent(LegacyProgress.ShieldDurationPercent(shieldLevel));
        }

        /// <summary>
        /// A missing or destroyed ship must not replace the live snapshot with
        /// the "full" sentinel (-1). Death deletes the save on its own path.
        /// </summary>
        private void PersistWaveVitals()
        {
            if (_ship == null || _ship.Health == null || _ship.Health.Hull < 1)
            {
                return;
            }

            WriteWaveProgress();
        }

        private void WriteWaveProgress()
        {
            if (TutorialActive)
            {
                return;
            }

            if (_session == null || !_session.WaveInProgress || _loadout == null || _loadout.State == null)
            {
                return;
            }

            if (PendingContinue != null)
            {
                return;
            }

            EnsureRunId();
            string waveStamp = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture);
            int waveHull;
            int waveShield;
            ReadShipVitals(out waveHull, out waveShield);
            RunSaveData waveData = RunSaveCodec.Capture(_session, _loadout.State, (int)DifficultySettings.Current, ActiveRunId, waveStamp, _boonRun, waveHull, waveShield);
            if (!RunSaveCodec.IsValid(waveData))
            {
                return;
            }

            RunSaveStore.TrySaveRun(waveData);
        }

        public void NotifyLoadoutChanged()
        {
            if (_ship != null)
            {
                _factory.ApplyLoadoutVisuals(_ship, _loadout.State);
                if (_session.Phase == GamePhase.Playing)
                {
                    _ship.ResetForWave(_loadout.State, false, false);
                }
                else if (_ship.Health != null)
                {
                    _ship.Health.ResetForWave(_loadout.State, false, false);
                }

                if (_ship.Shooter != null)
                {
                    _ship.Shooter.SyncFromLoadout();
                }

                if (_hangarPreview != null)
                {
                    PushPreviewFittings(_loadout != null ? _loadout.State : null);
                    _hangarPreview.NotifyVisualsChanged();
                }
            }

            TryUnlockCapstones();
            RaiseStateChanged();
        }

        private void TryUnlockCapstones()
        {
            if (_loadout == null || _loadout.State == null)
            {
                return;
            }

            LoadoutState state = _loadout.State;
            TryCapstone(UpgradeId.Storm, state.Storm);
            TryCapstone(UpgradeId.OverchargeLance, state.OverchargeLance);
            TryCapstone(UpgradeId.TwinSeek, state.TwinSeek);
        }

        private void TryCapstone(UpgradeId upgrade, bool owned)
        {
            if (!AchievementCatalog.ShouldUnlockCapstone(upgrade, owned))
            {
                return;
            }

            AchievementId id;
            if (AchievementCatalog.TryCapstoneAchievement(upgrade, out id))
            {
                TryUnlockAchievement(id);
            }
        }

        public void RefreshHud()
        {
            if (_ui != null)
            {
                _ui.Refresh();
            }
        }

        private void CompleteWave()
        {
            if (TutorialActive)
            {
                FinishTutorial();
                return;
            }

            int clearedWave = _session.WaveIndex;
            bool noHit = _session != null && !_session.WaveTookHit;
            _ship.SetInputEnabled(false);
            _waves.DespawnAll();
            int clearCredits = DifficultyCurve.ScaleCredits(DifficultySettings.WaveClearCredits, clearedWave);
            clearCredits = BoonHooks.ScaleCredits(clearCredits);
            _session.CompleteWave(ScoreValues.WaveClearBonus, clearCredits);
            OfferBoonChoice(clearedWave);

            RecordBest(clearedWave);
            TryUnlockWaveAchievements(clearedWave, noHit);
            MedalId waveMedal;
            bool awardedMedal = MedalCatalog.TryForClearedWave(clearedWave, out waveMedal)
                && TryAwardMedal(waveMedal);
            if (AudioCues.Instance != null)
            {
                if (awardedMedal && waveMedal == MedalId.FarDrift)
                {
                    AudioCues.Instance.PlayFarDriftAward();
                }
                else
                {
                    AudioCues.Instance.PlayWaveClear();
                }
            }

            RaiseStateChanged();
        }

        private void TryUnlockWaveAchievements(int clearedWave, bool noHit)
        {
            if (AchievementCatalog.ShouldUnlockFirstClear(clearedWave))
            {
                TryUnlockAchievement(AchievementId.FirstClear);
            }

            if (AchievementCatalog.ShouldUnlockNoHit(noHit))
            {
                TryUnlockAchievement(AchievementId.NoHitWave);
            }

            if (AchievementCatalog.ShouldUnlockHardClear(clearedWave, DifficultySettings.Current)
                && (_session == null || AssistRules.CountsForBoard(_session.AssistUsed)))
            {
                TryUnlockAchievement(AchievementId.HardClear);
            }

            if (AchievementCatalog.ShouldUnlockDeepOrbit(clearedWave))
            {
                TryUnlockAchievement(AchievementId.DeepOrbit);
            }

            if (AchievementCatalog.ShouldUnlockFarDrift(clearedWave))
            {
                TryUnlockAchievement(AchievementId.FarDrift);
            }
        }

        public void NotifyDoctrinePicked(DoctrineId id)
        {
            if (!AchievementCatalog.ShouldUnlockDoctrine(id))
            {
                return;
            }

            TryUnlockAchievement(AchievementId.Doctrine);
        }

        public void NotifyRailCharged(float heldSeconds)
        {
            if (!AchievementCatalog.ShouldUnlockRailCharge(heldSeconds))
            {
                return;
            }

            TryUnlockAchievement(AchievementId.RailCharge);
        }

        private bool TryUnlockAchievement(AchievementId id)
        {
            if (Achievements == null)
            {
                Achievements = AchievementPersist.Load();
            }

            if (!Achievements.TryUnlock(id))
            {
                return false;
            }

            Achievements.Save();
            if (_ui != null)
            {
                _ui.AnnounceAchievement(id);
            }

            return true;
        }

        private bool TryAwardWaveMedal(int clearedWave)
        {
            MedalId medal;
            if (!MedalCatalog.TryForClearedWave(clearedWave, out medal))
            {
                return false;
            }

            return TryAwardMedal(medal);
        }

        private bool TryAwardWorldMedal(int world)
        {
            MedalId medal;
            if (!MedalCatalog.TryForWorldEntry(world, out medal))
            {
                return false;
            }

            return TryAwardMedal(medal);
        }

        private bool TryAwardMedal(MedalId medal)
        {
            if (Persist == null)
            {
                Persist = HangarPersist.Load();
            }

            if (!Persist.TryAward(medal))
            {
                return false;
            }

            Persist.Save();
            return true;
        }

        private void RecordBest(int wave)
        {
            if (TutorialActive || !FirstRunRules.CountsForHighscore(TutorialActive))
            {
                return;
            }

            if (_session != null && !AssistRules.CountsForBoard(_session.AssistUsed))
            {
                return;
            }

            if (Best == null)
            {
                Best = LocalBest.Load();
            }

            int world = WorldCatalog.NumberForWave(wave);
            if (SessionBest == null)
            {
                SessionBest = new LocalBest();
            }

            SessionBest.TryRecord(_session.Score, wave, world);
            LastRunWasNewBest = Best.TryRecord(_session.Score, wave, world);
            if (LastRunWasNewBest)
            {
                Best.Save();
            }

            if (Meta == null)
            {
                Meta = MetaData.Fresh();
            }

            if (LegacyProgress.TryRecordBest(Meta, (int)DifficultySettings.Current, _session.Score, wave, world))
            {
                RunSaveStore.TrySaveMeta(Meta);
            }
        }

        private void EnsureRunId()
        {
            if (ActiveRunId > 0)
            {
                return;
            }

            if (Meta == null)
            {
                Meta = MetaData.Fresh();
            }

            ActiveRunId = LegacyProgress.TakeRunId(Meta);
            RunSaveStore.TrySaveMeta(Meta);
        }

        private void ApplyLegacyToNewRun()
        {
            if (_legacyApplied || Meta == null || _session == null || _loadout == null || _loadout.State == null)
            {
                return;
            }

            _legacyApplied = true;
            int bonusCredits = LegacyProgress.StartingCredits(Meta);
            if (bonusCredits > 0)
            {
                _session.GrantCredits(bonusCredits);
            }

            LoadoutState state = _loadout.State;
            int bonusShield = LegacyProgress.StartingShield(Meta);
            bonusShield += SinkRules.StartingShieldBonus(Sinks);
            int shieldIndex = 0;
            while (shieldIndex < bonusShield && state.CanApply(UpgradeId.ShieldCell))
            {
                state.Apply(UpgradeId.ShieldCell);
                shieldIndex += 1;
            }

            state.SetLegacyBonuses(LegacyProgress.HullBonus(Meta), LegacyProgress.DiscountPercent(Meta));
        }

        private void AwardLegacy(bool diedOnWave)
        {
            if (TutorialActive || !FirstRunRules.CountsForLegacy(TutorialActive))
            {
                return;
            }

            if (Meta == null || _session == null || ActiveRunId < 1)
            {
                return;
            }

            int worldsCleared;
            int highestWave;
            LegacyProgress.ProgressOf(_session.WaveIndex, diedOnWave, out worldsCleared, out highestWave);
            if (LegacyProgress.TryAward(Meta, ActiveRunId, worldsCleared, highestWave) >= 0)
            {
                RunSaveStore.TrySaveMeta(Meta);
            }
        }

        private bool FreshEnoughToReinit()
        {
            if (_runLaunched || PendingContinue != null || _session == null || _loadout == null || _loadout.State == null)
            {
                return false;
            }

            if (_session.Phase != GamePhase.Hangar || _session.WaveIndex > 1 || _session.Score > 0)
            {
                return false;
            }

            LoadoutState state = _loadout.State;
            if (state.FirstDiscountUsed)
            {
                return false;
            }

            if (RunSaveCodec.PackUpgrades(state) != 0)
            {
                return false;
            }

            int perkShield = Meta != null ? LegacyProgress.StartingShield(Meta) : 0;
            if (state.ShieldCharges > perkShield)
            {
                return false;
            }

            return true;
        }

        private void ReapplyLegacy()
        {
            _legacyApplied = false;
            if (_loadout != null && _loadout.State != null)
            {
                _loadout.State.Reset();
            }

            if (_session != null)
            {
                int keptLives = _session.Lives;
                _session.ResetRun();
                _session.ResetLives(keptLives);
            }

            ApplyLegacyToNewRun();
            if (_ship != null && _loadout != null)
            {
                _ship.ResetForWave(_loadout.State);
                if (_factory != null)
                {
                    _factory.ApplyLoadoutVisuals(_ship, _loadout.State);
                }
            }
        }

        private void MaybeWriteRun()
        {
            if (TutorialActive)
            {
                return;
            }

            if (_session == null || _loadout == null || _loadout.State == null || PendingContinue != null)
            {
                return;
            }

            if (!RunSaveCodec.ShouldWrite(_session.Phase))
            {
                return;
            }

            bool purchased = _loadout.State != null && _loadout.State.HasPurchase();
            if (!GameSession.HasRunProgress(_session.WaveIndex, _session.Score, _session.Credits, purchased))
            {
                return;
            }

            EnsureRunId();
            string stamp = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", System.Globalization.CultureInfo.InvariantCulture);
            int savedHull;
            int savedShield;
            ReadShipVitals(out savedHull, out savedShield);
            RunSaveData data = RunSaveCodec.Capture(_session, _loadout.State, (int)DifficultySettings.Current, ActiveRunId, stamp, _boonRun, savedHull, savedShield);
            if (!RunSaveCodec.IsValid(data))
            {
                return;
            }

            RunSaveStore.TrySaveRun(data);
        }

        private void ReadShipVitals(out int hullNow, out int shieldNow)
        {
            hullNow = -1;
            shieldNow = -1;
            if (_ship == null || _ship.Health == null || _ship.Health.Hull < 1)
            {
                return;
            }

            hullNow = _ship.Health.Hull;
            shieldNow = _ship.Health.Shield < 0 ? 0 : _ship.Health.Shield;
        }

        private void RaiseStateChanged()
        {
            if (StateChanged != null)
            {
                StateChanged();
            }

            if (_ui != null)
            {
                _ui.Refresh();
            }

            ApplyShieldDuration();
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.SyncMusicToPhase(_session.Phase, _session.WaveIndex);
            }

            if (_factory != null)
            {
                _factory.SetHangarDressingVisible(_session.Phase != GamePhase.Playing);
            }

            bool hangar = _session.Phase != GamePhase.Playing;
            if (_hangarPreview != null)
            {
                _hangarPreview.SetActive(hangar);
            }

            if (_ui != null)
            {
                _ui.EnsureHangarPreview(_ship);
            }

            if (_follow == null)
            {
                _follow = UnityEngine.Object.FindAnyObjectByType<FollowCamera>();
            }

            if (_follow != null)
            {
                _follow.SetHangarFraming(hangar);
            }

            MaybeWriteRun();
        }

        private void OfferBoonChoice(int clearedWave)
        {
            if (!CampaignCap.IsWorldBoundary(clearedWave) || _boonRun == null)
            {
                return;
            }

            EnsureRunId();
            int clearedWorld = WorldCatalog.NumberForWave(clearedWave);
            int[] offer = BoonCatalog.Draw(ActiveRunId, clearedWorld, _boonRun.Levels);
            if (offer.Length < BoonCatalog.OfferCount)
            {
                return;
            }

            _boonRun.SetPending(offer);
        }

        private void ClearBoons()
        {
            if (_boonRun != null)
            {
                _boonRun.Clear();
            }

            BoonHooks.Reset();
        }
    }
}
