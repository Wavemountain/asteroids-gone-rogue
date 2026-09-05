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
            Best = LocalBest.Load();
            Persist = HangarPersist.Load();
            LastRunWasNewBest = false;
        }

        public void EnterHangar()
        {
            _session.ReturnToHangar();
            _ship.SetInputEnabled(false);
            _ship.ResetForWave(_loadout.State);
            _factory.ApplyLoadoutVisuals(_ship, _loadout.State);
            RaiseStateChanged();
        }

        public void StartWave()
        {
            if (!_session.CanStartWave)
            {
                return;
            }

            _session.BeginWave();
            _ship.ResetForWave(_loadout.State);
            _factory.ApplyLoadoutVisuals(_ship, _loadout.State);
            _ship.SetInputEnabled(true);
            int world = ContentFactory.WorldIndexForWave(_session.WaveIndex);
            bool worldMedal = TryAwardWorldMedal(world);
            _waves.SpawnWave(_session.WaveIndex);
            if (worldMedal && _ui != null)
            {
                _ui.AnnounceMedalBeat(MedalCatalog.WorldEntryBeat(world));
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
            if (_session.Phase != GamePhase.Playing)
            {
                return;
            }

            _ship.SetInputEnabled(false);
            _waves.DespawnAll();
            _session.FailWave(cause);
            RecordBest(_session.WaveIndex);
            RaiseStateChanged();
        }

        public void NotifyLoadoutChanged()
        {
            if (_ship != null)
            {
                _factory.ApplyLoadoutVisuals(_ship, _loadout.State);
                if (_session.Phase != GamePhase.Playing)
                {
                    _ship.ResetForWave(_loadout.State);
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
            _session.CompleteWave(ScoreValues.WaveClearBonus, ScoreValues.WaveClearCredits);
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
                AudioCues.Instance.SyncMusicToPhase(_session.Phase);
            }

            if (_factory != null)
            {
                _factory.SetHangarDressingVisible(_session.Phase != GamePhase.Playing);
            }
        }
    }
}
