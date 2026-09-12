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
        public string FailReason { get; private set; } = string.Empty;
        public DamageCause FailCause { get; private set; }
        public EnemyKind FailEnemyKind { get; private set; }
        public bool HasStructuredFail { get; private set; }
        public int FailRemainingThreats { get; private set; }
        public int LastResolvedWave { get; private set; }
        public int LastCreditsAwarded { get; private set; }
        public int LastRunScore { get; private set; }
        public int Lives { get; private set; } = DifficultySettings.NormalStartLives;
        public int MaxLives { get; private set; } = DifficultySettings.MaxLives;

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

            FailReason = string.Empty;
            FailCause = DamageCause.Unknown;
            FailEnemyKind = EnemyKind.Mid01;
            HasStructuredFail = false;
            FailRemainingThreats = 0;
            LastCreditsAwarded = 0;
            if (Lives <= 0)
            {
                ResetLives(DifficultySettings.StartLives);
            }

            Phase = GamePhase.Playing;
        }

        public void ResetLives(int startLives)
        {
            int cap = DifficultySettings.MaxLives;
            MaxLives = cap;
            if (startLives < 1)
            {
                startLives = 1;
            }

            Lives = startLives > cap ? cap : startLives;
        }

        /// <summary>
        /// Full run reset back to hangar / start. Wave 1, empty score and credits.
        /// LastResolvedWave / LastRunScore stay so the fail card can still read the run.
        /// </summary>
        public void ResetRun()
        {
            WaveIndex = 1;
            Score = 0;
            Credits = 0;
            LastCreditsAwarded = 0;
            FailReason = string.Empty;
            FailCause = DamageCause.Unknown;
            FailEnemyKind = EnemyKind.Mid01;
            HasStructuredFail = false;
            FailRemainingThreats = 0;
            ResetLives(DifficultySettings.StartLives);
            Phase = GamePhase.Hangar;
        }

        /// <summary>
        /// Spend one life. True when the run continues (lives remain).
        /// </summary>
        public bool TryLoseLife()
        {
            if (Phase != GamePhase.Playing || Lives <= 0)
            {
                return false;
            }

            Lives -= 1;
            return Lives > 0;
        }

        public bool TryGainLife()
        {
            if (Lives >= MaxLives)
            {
                return false;
            }

            Lives += 1;
            return true;
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

            LastResolvedWave = WaveIndex;
            LastCreditsAwarded = credits;
            Score += bonusScore;
            Credits += credits;
            LastRunScore = Score;
            WaveIndex += 1;
            Phase = GamePhase.WaveClear;
        }

        public void FailWave()
        {
            FailWave(null);
        }

        public void FailWave(string reason)
        {
            FailWave(reason, 0);
        }

        public void FailWave(string reason, int remainingThreats)
        {
            if (Phase != GamePhase.Playing)
            {
                return;
            }

            HasStructuredFail = false;
            FailCause = DamageCause.Unknown;
            FailEnemyKind = EnemyKind.Mid01;
            FailReason = string.IsNullOrEmpty(reason) ? "Unknown cause" : reason;
            FailRemainingThreats = remainingThreats < 0 ? 0 : remainingThreats;
            LastResolvedWave = WaveIndex;
            LastCreditsAwarded = 0;
            LastRunScore = Score;
            Phase = GamePhase.Failed;
        }

        public void FailWave(DamageCause cause, EnemyKind kind)
        {
            FailWave(cause, kind, 0);
        }

        public void FailWave(DamageCause cause, EnemyKind kind, int remainingThreats)
        {
            if (Phase != GamePhase.Playing)
            {
                return;
            }

            HasStructuredFail = true;
            FailCause = cause;
            FailEnemyKind = kind;
            FailReason = cause == DamageCause.EnemyContact
                ? DamageCauseText.FailReason(cause, kind)
                : DamageCauseText.FailReason(cause);
            FailRemainingThreats = remainingThreats < 0 ? 0 : remainingThreats;
            LastResolvedWave = WaveIndex;
            LastCreditsAwarded = 0;
            LastRunScore = Score;
            Phase = GamePhase.Failed;
        }

        public void ReturnToHangar()
        {
            Phase = GamePhase.Hangar;
        }

        /// <summary>
        /// Leave a live wave without the clear bonus or wave increment. Loadout is untouched.
        /// </summary>
        public void AbortToHangar()
        {
            if (Phase != GamePhase.Playing)
            {
                return;
            }

            FailReason = string.Empty;
            FailCause = DamageCause.Unknown;
            FailEnemyKind = EnemyKind.Mid01;
            HasStructuredFail = false;
            FailRemainingThreats = 0;
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
