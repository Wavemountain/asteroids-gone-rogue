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

        public HangarPersist Persist { get; private set; }

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
            Persist = HangarPersist.Load();
            LastRunWasNewBest = false;
        }

        public void EnterHangar()
        {
            DifficultySettings.EnsureLoaded();
            if (_session.Lives <= 0)
            {
                ResetFullRun();
            }

            _session.ReturnToHangar();
            _ship.SetInputEnabled(false);
            _ship.ResetForWave(_loadout.State);
            _factory.ApplyLoadoutVisuals(_ship, _loadout.State);
            RaiseStateChanged();
        }

        public void SetDifficulty(DifficultyGrade grade)
        {
            DifficultySettings.SetGrade(grade);
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
                _hangarPreview.NotifyVisualsChanged();
            }
        }

        public bool TryGrantExtraLife()
        {
            if (_session == null || !_session.TryGainLife())
            {
                return false;
            }

            RaiseStateChanged();
            return true;
        }

        public void StartWave()
        {
            if (!_session.CanStartWave)
            {
                return;
            }

            if (_session.Phase == GamePhase.Failed || _session.Lives <= 0)
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

            _session.BeginWave();
            _ship.ResetForWave(_loadout.State);
            _factory.ApplyLoadoutVisuals(_ship, _loadout.State);
            _ship.SetInputEnabled(true);
            int world = ContentFactory.WorldIndexForWave(_session.WaveIndex);
            TryAwardWorldMedal(world);
            _waves.SpawnWave(_session.WaveIndex);
            string beat = MedalCatalog.WorldEntryBeat(world);
            if (!string.IsNullOrEmpty(beat) && _ui != null)
            {
                _ui.AnnounceMedalBeat(beat, MedalCatalog.WorldEntryFlashSeconds(world));
            }

            RaiseStateChanged();
        }

        public void ContinueFromResults()
        {
            if (_session.Phase == GamePhase.WaveClear || _session.Phase == GamePhase.Failed)
            {
                _session.ReturnToHangar();
                _ship.SetInputEnabled(false);
                _ship.ResetForWave(_loadout.State);
                _factory.ApplyLoadoutVisuals(_ship, _loadout.State);
                RaiseStateChanged();
            }
        }

        public void AbortWave()
        {
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
                _ship.ResetForWave(_loadout.State);
                _factory.ApplyLoadoutVisuals(_ship, _loadout.State);
            }

            RaiseStateChanged();
        }

        public void AddBonusScore(int amount)
        {
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

            _session.AddScore(scoreValue);
            if (_waves.RemainingThreats <= 0)
            {
                CompleteWave();
            }
            else
            {
                RaiseStateChanged();
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
            return _session != null && _session.Phase == GamePhase.Playing;
        }

        private bool TryRespawnAfterLifeLoss()
        {
            if (_session == null || !_session.TryLoseLife())
            {
                return false;
            }

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

            if (_ui != null)
            {
                _ui.AnnounceLifeLost(_session.Lives);
            }

            RaiseStateChanged();
            return true;
        }

        private void FailRun(string cause, bool structured, DamageCause failCause, EnemyKind kind)
        {
            int remaining = _waves != null ? _waves.RemainingThreats : 0;
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
            RaiseStateChanged();
        }

        private void ResetFullRun()
        {
            if (_loadout != null && _loadout.State != null)
            {
                _loadout.State.Reset();
            }

            if (_session != null)
            {
                _session.ResetRun();
            }
        }

        public void NotifyLoadoutChanged()
        {
            if (_ship != null)
            {
                _factory.ApplyLoadoutVisuals(_ship, _loadout.State);
                if (_session.Phase == GamePhase.Playing)
                {
                    _ship.ResetForWave(_loadout.State);
                }
                else if (_ship.Health != null)
                {
                    _ship.Health.ResetForWave(_loadout.State);
                }

                if (_hangarPreview != null)
                {
                    _hangarPreview.NotifyVisualsChanged();
                }
            }

            RaiseStateChanged();
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
            int clearedWave = _session.WaveIndex;
            _ship.SetInputEnabled(false);
            _waves.DespawnAll();
            _session.CompleteWave(ScoreValues.WaveClearBonus, DifficultySettings.WaveClearCredits);
            RecordBest(clearedWave);
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
            if (Best == null)
            {
                Best = LocalBest.Load();
            }

            int world = ContentFactory.WorldIndexForWave(wave);
            LastRunWasNewBest = Best.TryRecord(_session.Score, wave, world);
            if (LastRunWasNewBest)
            {
                Best.Save();
            }
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
        }
    }
}
