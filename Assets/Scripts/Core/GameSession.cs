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
        public bool CampaignWon { get; private set; }
        public bool WaveTookHit { get; private set; }
        public int ExtraLifeStreak { get; private set; }

        /// <summary>
        /// World index just completed when the latest clear was a world boundary
        /// (waves 5, 10, 15, …). Zero on a normal wave clear, fail, or a fresh run.
        /// </summary>
        public int WorldCleared { get; private set; }

        public bool CanStartWave
        {
            get
            {
                return Phase == GamePhase.Hangar
                    || Phase == GamePhase.WaveClear
                    || Phase == GamePhase.Failed
                    || Phase == GamePhase.CampaignClear;
            }
        }

        /// <summary>
        /// The hangar primary control restarts the run only after all lives are lost.
        /// Wave clear, including a world boundary, keeps the run. Hangar starts the next wave.
        /// </summary>
        public static bool PrimaryRestartsRun(GamePhase phase)
        {
            return phase == GamePhase.Failed;
        }

        /// <summary>
        /// Shop buys on the fail screen would vanish on New Run, so those buttons stay inert.
        /// </summary>
        public static bool ShopLockedForPhase(GamePhase phase)
        {
            return phase == GamePhase.Failed;
        }

        public static bool HasRunProgress(int waveIndex, int score, int credits, bool anyPurchase)
        {
            if (waveIndex > 1)
            {
                return true;
            }

            if (score > 0)
            {
                return true;
            }

            if (credits > 0)
            {
                return true;
            }

            return anyPurchase;
        }

        /// <summary>
        /// Confirm New Run only when the setting is on and the player would lose something.
        /// A fresh hangar (wave 1, no score, no credits, no purchases) starts immediately.
        /// </summary>
        public static bool ShouldConfirmNewRun(bool settingOn, bool hasProgress)
        {
            return settingOn && hasProgress;
        }

        public bool ShopOpen
        {
            get { return Phase != GamePhase.Playing; }
        }

        public void BeginWave()
        {
            if (!CanStartWave)
            {
                throw new InvalidOperationException("Wave can only start from hangar, wave-clear, fail, or campaign-clear.");
            }

            FailReason = string.Empty;
            FailCause = DamageCause.Unknown;
            FailEnemyKind = EnemyKind.Mid01;
            HasStructuredFail = false;
            FailRemainingThreats = 0;
            LastCreditsAwarded = 0;
            CampaignWon = false;
            WaveTookHit = false;
            WorldCleared = 0;
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
            CampaignWon = false;
            WaveTookHit = false;
            ExtraLifeStreak = 0;
            WorldCleared = 0;
            ResetLives(DifficultySettings.StartLives);
            Phase = GamePhase.Hangar;
        }

        public void MarkWaveHit()
        {
            WaveTookHit = true;
        }

        public void NoteExtraLife()
        {
            ExtraLifeStreak += 1;
        }

        public void NoteLifeLost()
        {
            ExtraLifeStreak = 0;
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

            int clearedWave = WaveIndex;
            LastResolvedWave = clearedWave;
            LastCreditsAwarded = credits;
            Score += bonusScore;
            Credits += credits;
            LastRunScore = Score;
            WorldCleared = CampaignCap.IsWorldBoundary(clearedWave)
                ? WorldCatalog.NumberForWave(clearedWave)
                : 0;
            WaveIndex += 1;
            Phase = GamePhase.WaveClear;
        }

        /// <summary>
        /// World 1 content-cap win. Score/credits award like a wave clear, but
        /// the wave index stays on the final wave and play does not continue.
        /// </summary>
        public void CompleteCampaign(int bonusScore, int credits)
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
            CampaignWon = true;
            Phase = GamePhase.CampaignClear;
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
            WorldCleared = 0;
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
            WorldCleared = 0;
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
