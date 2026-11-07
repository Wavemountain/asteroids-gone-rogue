using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEngine;
using AsteroidsGoneRogue;

namespace AsteroidsGoneRogue.EditorTools
{
    /// <summary>
    /// Samples unscaled frame time while the long run is in play mode.
    /// Editor assembly only, so player builds do not include it.
    /// </summary>
    public static class LongRunFrames
    {
        const int BucketCount = 201;
        const int FlushEvery = 120;

        static int[] _buckets = new int[BucketCount];
        static int _count;
        static double _sum;
        static float _max;
        static int _hitches;
        static int _sinceFlush;
        static bool _ready;
        static string _dir = string.Empty;
        static float _lastRealtime = -1f;
        static bool _wasPlaying;
        static double _playingSeconds;
        static double _realElapsed;

        public static int Count
        {
            get
            {
                Ensure();
                return _count;
            }
        }

        public static void NoteRealtime(float now, bool playing)
        {
            Ensure();
            if (_lastRealtime < 0f)
            {
                _lastRealtime = now;
                _wasPlaying = playing;
                return;
            }

            float step = now - _lastRealtime;
            _lastRealtime = now;
            if (step <= 0f || step > 2f)
            {
                _wasPlaying = playing;
                return;
            }

            _realElapsed += step;
            if (playing && _wasPlaying)
            {
                _playingSeconds += step;
                Note(step);
            }

            _wasPlaying = playing;
        }

        public static double PlayingSeconds
        {
            get
            {
                Ensure();
                return _playingSeconds;
            }
        }

        public static double RealElapsedSeconds
        {
            get
            {
                Ensure();
                return _realElapsed;
            }
        }

        public static double SumSeconds
        {
            get
            {
                Ensure();
                return _sum;
            }
        }

        static void Note(float delta)
        {
            if (delta <= 0f)
            {
                return;
            }

            _count += 1;
            _sum += delta;
            if (delta > _max)
            {
                _max = delta;
            }

            if (delta > LongRunVerdict.HitchSeconds)
            {
                _hitches += 1;
            }

            int ms = (int)(delta * 1000f);
            if (ms < 0)
            {
                ms = 0;
            }

            if (ms >= BucketCount)
            {
                ms = BucketCount - 1;
            }

            _buckets[ms] += 1;
            _sinceFlush += 1;
            if (_sinceFlush >= FlushEvery)
            {
                _sinceFlush = 0;
                Write();
            }
        }

        public static float Average()
        {
            Ensure();
            if (_count <= 0)
            {
                return 0f;
            }

            return (float)(_sum / _count);
        }

        public static float Max
        {
            get
            {
                Ensure();
                return _max;
            }
        }

        public static int Hitches
        {
            get
            {
                Ensure();
                return _hitches;
            }
        }

        public static float Percentile(float p)
        {
            Ensure();
            if (_count <= 0)
            {
                return 0f;
            }

            double fraction = p;
            if (fraction < 0.0)
            {
                fraction = 0.0;
            }

            if (fraction > 1.0)
            {
                fraction = 1.0;
            }

            int target = (int)System.Math.Ceiling(_count * fraction);
            if (target < 1)
            {
                target = 1;
            }

            int seen = 0;
            for (int ms = 0; ms < _buckets.Length; ms++)
            {
                seen += _buckets[ms];
                if (seen >= target)
                {
                    if (ms >= BucketCount - 1)
                    {
                        return _max;
                    }

                    return ms / 1000f;
                }
            }

            return _max;
        }

        public static void Flush()
        {
            Ensure();
            Write();
        }

        static void Ensure()
        {
            if (_ready)
            {
                return;
            }

            _ready = true;
            _dir = SessionState.GetString("LongRun.Out", string.Empty);
            Load();
        }

        static string BinPath()
        {
            if (_dir.Length == 0)
            {
                _dir = SessionState.GetString("LongRun.Out", string.Empty);
            }

            if (_dir.Length == 0)
            {
                return string.Empty;
            }

            return Path.Combine(_dir, "frametime.bins");
        }

        static void Load()
        {
            string path = BinPath();
            if (path.Length == 0 || !File.Exists(path))
            {
                return;
            }

            string[] lines = File.ReadAllLines(path);
            if (lines.Length < 2)
            {
                return;
            }

            string[] head = lines[0].Split(' ');
            if (head.Length < 4)
            {
                return;
            }

            int count;
            double sum;
            float max;
            int hitches;
            if (!int.TryParse(head[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out count))
            {
                return;
            }

            if (!double.TryParse(head[1], NumberStyles.Float, CultureInfo.InvariantCulture, out sum))
            {
                return;
            }

            if (!float.TryParse(head[2], NumberStyles.Float, CultureInfo.InvariantCulture, out max))
            {
                return;
            }

            if (!int.TryParse(head[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out hitches))
            {
                return;
            }

            string[] bins = lines[1].Split(' ');
            if (bins.Length < BucketCount)
            {
                return;
            }

            for (int index = 0; index < BucketCount; index++)
            {
                int value;
                if (!int.TryParse(bins[index], NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                {
                    value = 0;
                }

                _buckets[index] = value;
            }

            _count = count;
            _sum = sum;
            _max = max;
            _hitches = hitches;
            _playingSeconds = head.Length >= 6 ? ParseDouble(head[4]) : sum;
            _realElapsed = head.Length >= 6 ? ParseDouble(head[5]) : sum;
        }

        static double ParseDouble(string text)
        {
            double number;
            if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out number))
            {
                return 0.0;
            }

            return number;
        }

        static void Write()
        {
            string path = BinPath();
            if (path.Length == 0)
            {
                return;
            }

            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.Append(_count.ToString(CultureInfo.InvariantCulture));
            builder.Append(' ');
            builder.Append(_sum.ToString("R", CultureInfo.InvariantCulture));
            builder.Append(' ');
            builder.Append(_max.ToString("R", CultureInfo.InvariantCulture));
            builder.Append(' ');
            builder.Append(_hitches.ToString(CultureInfo.InvariantCulture));
            builder.Append(' ');
            builder.Append(_playingSeconds.ToString("R", CultureInfo.InvariantCulture));
            builder.Append(' ');
            builder.Append(_realElapsed.ToString("R", CultureInfo.InvariantCulture));
            builder.Append('\n');
            for (int index = 0; index < BucketCount; index++)
            {
                if (index > 0)
                {
                    builder.Append(' ');
                }

                builder.Append(_buckets[index].ToString(CultureInfo.InvariantCulture));
            }

            builder.Append('\n');
            File.WriteAllText(path, builder.ToString());
        }
    }

    public sealed class LongRunFrameProbe : MonoBehaviour
    {
        void Update()
        {
            bool playing = false;
            GameManager game = GameManager.Live;
            if (game != null && game.Session != null && game.Session.Phase == GamePhase.Playing)
            {
                playing = true;
            }

            LongRunFrames.NoteRealtime(Time.realtimeSinceStartup, playing);
        }
    }
}
