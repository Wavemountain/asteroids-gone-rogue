using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Gameplay draws that a daily seed owns. Seed 0 keeps UnityEngine.Random.
    /// Each wave restarts the stream from Mix(seed, wave) so the same day and
    /// wave replay the same drop and asteroid-visual sequence.
    /// </summary>
    public static class RunRng
    {
        private static int _daily;
        private static SeedStream _stream;

        public static int DailySeed
        {
            get { return _daily; }
        }

        public static void SetDaily(int seed)
        {
            _daily = seed > 0 ? seed : 0;
            if (_daily == 0)
            {
                _stream = null;
            }
        }

        public static void Clear()
        {
            _daily = 0;
            _stream = null;
        }

        public static void BindWave(int dailySeed, int waveIndex)
        {
            if (dailySeed <= 0)
            {
                _daily = 0;
                _stream = null;
                return;
            }

            _daily = dailySeed;
            int wave = waveIndex < 1 ? 1 : waveIndex;
            _stream = new SeedStream(DailySeed.Mix(dailySeed, wave));
        }

        public static float Unit()
        {
            if (_stream == null)
            {
                return Random.value;
            }

            return _stream.Value();
        }

        public static int Index(int length)
        {
            if (length <= 1)
            {
                return 0;
            }

            if (_stream == null)
            {
                return Random.Range(0, length);
            }

            return _stream.Range(0, length);
        }
    }
}
