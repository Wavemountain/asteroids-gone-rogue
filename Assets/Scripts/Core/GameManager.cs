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
            _waves.SpawnWave(_session.WaveIndex);
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
            if (_session.Phase != GamePhase.Playing)
            {
                return;
            }

            _ship.SetInputEnabled(false);
            _waves.DespawnAll();
            _session.FailWave();
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
            _ship.SetInputEnabled(false);
            _waves.DespawnAll();
            _session.CompleteWave(ScoreValues.WaveClearBonus, ScoreValues.WaveClearCredits);
            RaiseStateChanged();
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
        }
    }
}
