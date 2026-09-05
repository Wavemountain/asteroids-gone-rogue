using System;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Pure Week 1 loop: Hangar → Play → Wave Clear or Fail.
    /// No Unity types so the flow can be reasoned about without the Editor.
    /// </summary>
    public sealed class GameSession
    {
        public GamePhase Phase { get; private set; } = GamePhase.Hangar;
        public int WaveIndex { get; private set; } = 1;
        public int Score { get; private set; }
        public int Credits { get; private set; }

        public bool CanStartWave
        {
            get
            {
                return Phase == GamePhase.Hangar
                    || Phase == GamePhase.WaveClear
                    || Phase == GamePhase.Failed;
            }
        }

        public bool ShopOpen
        {
            get { return Phase != GamePhase.Playing; }
        }

        public void BeginWave()
        {
            if (!CanStartWave)
            {
                throw new InvalidOperationException("Wave can only start from hangar, wave-clear, or fail.");
            }

            Phase = GamePhase.Playing;
        }

        public void AddScore(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException("amount");
            }

            Score += amount;
        }

        public void CompleteWave(int bonusScore, int credits)
        {
            if (Phase != GamePhase.Playing)
            {
                return;
            }

            Score += bonusScore;
            Credits += credits;
            WaveIndex += 1;
            Phase = GamePhase.WaveClear;
        }

        public void FailWave()
        {
            if (Phase != GamePhase.Playing)
            {
                return;
            }

            Phase = GamePhase.Failed;
        }

        public void ReturnToHangar()
        {
            Phase = GamePhase.Hangar;
        }

        public bool TrySpend(int cost)
        {
            if (cost < 0 || Credits < cost)
            {
                return false;
            }

            Credits -= cost;
            return true;
        }
    }
}
