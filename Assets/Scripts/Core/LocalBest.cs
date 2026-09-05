namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Highest score / wave / world persisted locally. No Unity types in the compare rules
    /// so tests can reason about a new best without the Editor.
    /// </summary>
    public sealed class LocalBest
    {
        public const string ScoreKey = "agr.best.score";
        public const string WaveKey = "agr.best.wave";
        public const string WorldKey = "agr.best.world";

        public int Score { get; private set; }
        public int Wave { get; private set; }
        public int World { get; private set; }

        public bool HasRecord
        {
            get { return Wave > 0 || Score > 0; }
        }

        public LocalBest()
        {
        }

        public LocalBest(int score, int wave, int world)
        {
            Score = score;
            Wave = wave;
            World = world;
        }

        public static bool IsBetter(int bestScore, int bestWave, int score, int wave)
        {
            return IsBetter(bestScore, bestWave, 0, score, wave, 0);
        }

        public static bool IsBetter(int bestScore, int bestWave, int bestWorld, int score, int wave, int world)
        {
            if (score != bestScore)
            {
                return score > bestScore;
            }

            if (wave != bestWave)
            {
                return wave > bestWave;
            }

            return world > bestWorld;
        }

        public string PlayCompare(int score, int wave, int world)
        {
            if (!HasRecord)
            {
                return string.Empty;
            }

            if (IsBetter(Score, Wave, World, score, wave, world) || score > Score)
            {
                return "  ·  NEW BEST";
            }

            return " / Best " + Score;
        }

        public bool TryRecord(int score, int wave, int world)
        {
            if (HasRecord && !IsBetter(Score, Wave, World, score, wave, world))
            {
                return false;
            }

            bool changed = score != Score || wave != Wave || world != World;
            Score = score;
            Wave = wave;
            World = world;
            return changed;
        }

        public static LocalBest Load()
        {
            return new LocalBest(
                UnityEngine.PlayerPrefs.GetInt(ScoreKey, 0),
                UnityEngine.PlayerPrefs.GetInt(WaveKey, 0),
                UnityEngine.PlayerPrefs.GetInt(WorldKey, 0));
        }

        public void Save()
        {
            UnityEngine.PlayerPrefs.SetInt(ScoreKey, Score);
            UnityEngine.PlayerPrefs.SetInt(WaveKey, Wave);
            UnityEngine.PlayerPrefs.SetInt(WorldKey, World);
            UnityEngine.PlayerPrefs.Save();
        }

        public string CardLine()
        {
            if (!HasRecord)
            {
                return "Best —";
            }

            return "Best " + Score + "  ·  Wave " + Wave + "  ·  World " + World;
        }
    }
}
