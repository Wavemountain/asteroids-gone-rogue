using System.Globalization;
using System.Text;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// One local best per UTC date. Separate from the normal highscore keys.
    /// </summary>
    public sealed class DailyBoardData
    {
        public int Version;
        public int Count;
        public int[] Dates;
        public int[] Scores;
        public int[] Waves;
    }

    public static class DailyBoardRules
    {
        public const int CurrentVersion = 1;
        public const int Cap = 32;

        public static DailyBoardData Fresh()
        {
            DailyBoardData data = new DailyBoardData();
            data.Version = CurrentVersion;
            data.Count = 0;
            data.Dates = new int[Cap];
            data.Scores = new int[Cap];
            data.Waves = new int[Cap];
            return data;
        }

        public static bool TryRead(DailyBoardData board, int date, out int score, out int wave)
        {
            score = 0;
            wave = 0;
            int index = Find(board, date);
            if (index < 0)
            {
                return false;
            }

            score = board.Scores[index];
            wave = board.Waves[index];
            return true;
        }

        /// <summary>
        /// Assist runs are ignored. A better score, or the same score and a
        /// higher wave, replaces that date. A new date past the cap drops the
        /// oldest date.
        /// </summary>
        public static bool TrySubmit(DailyBoardData board, int date, int score, int wave, bool assist)
        {
            if (board == null || assist || !DailySeed.DateLooksValid(date))
            {
                return false;
            }

            if (score < 0 || score > RunSaveCodec.MaxScore || wave < 1 || wave > RunSaveCodec.MaxWave)
            {
                return false;
            }

            if (board.Dates == null || board.Scores == null || board.Waves == null)
            {
                return false;
            }

            if (board.Count < 0)
            {
                board.Count = 0;
            }

            if (board.Count > Cap)
            {
                board.Count = Cap;
            }

            int existing = Find(board, date);
            if (existing >= 0)
            {
                if (!IsBetter(board.Scores[existing], board.Waves[existing], score, wave))
                {
                    return false;
                }

                board.Scores[existing] = score;
                board.Waves[existing] = wave;
                return true;
            }

            if (board.Count < Cap)
            {
                int slot = board.Count;
                board.Dates[slot] = date;
                board.Scores[slot] = score;
                board.Waves[slot] = wave;
                board.Count = slot + 1;
                return true;
            }

            int oldest = OldestIndex(board);
            if (oldest < 0)
            {
                return false;
            }

            board.Dates[oldest] = date;
            board.Scores[oldest] = score;
            board.Waves[oldest] = wave;
            return true;
        }

        public static bool IsBetter(int bestScore, int bestWave, int score, int wave)
        {
            if (score != bestScore)
            {
                return score > bestScore;
            }

            return wave > bestWave;
        }

        private static int Find(DailyBoardData board, int date)
        {
            if (board == null || board.Dates == null)
            {
                return -1;
            }

            int count = board.Count;
            if (count > Cap)
            {
                count = Cap;
            }

            for (int index = 0; index < count; index++)
            {
                if (board.Dates[index] == date)
                {
                    return index;
                }
            }

            return -1;
        }

        private static int OldestIndex(DailyBoardData board)
        {
            int count = board.Count;
            if (count > Cap)
            {
                count = Cap;
            }

            if (count < 1)
            {
                return -1;
            }

            int oldest = 0;
            for (int index = 1; index < count; index++)
            {
                if (board.Dates[index] < board.Dates[oldest])
                {
                    oldest = index;
                }
            }

            return oldest;
        }
    }

    public static class DailyBoardCodec
    {
        public static string ToJson(DailyBoardData data)
        {
            if (data == null)
            {
                return string.Empty;
            }

            int count = data.Count;
            if (count < 0)
            {
                count = 0;
            }

            if (count > DailyBoardRules.Cap)
            {
                count = DailyBoardRules.Cap;
            }

            StringBuilder builder = new StringBuilder(64 + (count * 48));
            builder.Append("{\"Version\":");
            builder.Append(data.Version.ToString(CultureInfo.InvariantCulture));
            builder.Append(",\"Count\":");
            builder.Append(count.ToString(CultureInfo.InvariantCulture));
            for (int index = 0; index < count; index++)
            {
                builder.Append(",\"Date");
                builder.Append(index.ToString(CultureInfo.InvariantCulture));
                builder.Append("\":");
                builder.Append(data.Dates[index].ToString(CultureInfo.InvariantCulture));
                builder.Append(",\"Score");
                builder.Append(index.ToString(CultureInfo.InvariantCulture));
                builder.Append("\":");
                builder.Append(data.Scores[index].ToString(CultureInfo.InvariantCulture));
                builder.Append(",\"Wave");
                builder.Append(index.ToString(CultureInfo.InvariantCulture));
                builder.Append("\":");
                builder.Append(data.Waves[index].ToString(CultureInfo.InvariantCulture));
            }

            builder.Append('}');
            return builder.ToString();
        }

        public static bool TryParse(string json, out DailyBoardData data)
        {
            data = null;
            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            string trimmed = json.Trim();
            if (trimmed.Length < 2 || trimmed[0] != '{' || trimmed[trimmed.Length - 1] != '}')
            {
                return false;
            }

            DailyBoardData parsed = DailyBoardRules.Fresh();
            parsed.Count = -1;
            bool sawVersion = false;
            bool[] sawDate = new bool[DailyBoardRules.Cap];
            bool[] sawScore = new bool[DailyBoardRules.Cap];
            bool[] sawWave = new bool[DailyBoardRules.Cap];
            int cursor = 1;
            int end = trimmed.Length - 1;
            while (cursor < end)
            {
                SkipSpace(trimmed, ref cursor);
                if (cursor >= end)
                {
                    break;
                }

                if (trimmed[cursor] == ',')
                {
                    cursor += 1;
                    SkipSpace(trimmed, ref cursor);
                }

                if (cursor >= end)
                {
                    break;
                }

                string key;
                if (!ReadString(trimmed, ref cursor, out key))
                {
                    return false;
                }

                SkipSpace(trimmed, ref cursor);
                if (cursor >= end || trimmed[cursor] != ':')
                {
                    return false;
                }

                cursor += 1;
                SkipSpace(trimmed, ref cursor);
                int number;
                if (!ReadInt(trimmed, ref cursor, out number))
                {
                    return false;
                }

                if (!Assign(parsed, key, number, sawDate, sawScore, sawWave, ref sawVersion))
                {
                    return false;
                }
            }

            if (!sawVersion || parsed.Version != DailyBoardRules.CurrentVersion)
            {
                return false;
            }

            if (parsed.Count < 0 || parsed.Count > DailyBoardRules.Cap)
            {
                return false;
            }

            for (int index = 0; index < parsed.Count; index++)
            {
                if (!sawDate[index] || !sawScore[index] || !sawWave[index])
                {
                    return false;
                }

                if (!DailySeed.DateLooksValid(parsed.Dates[index]))
                {
                    return false;
                }

                if (parsed.Scores[index] < 0 || parsed.Scores[index] > RunSaveCodec.MaxScore)
                {
                    return false;
                }

                if (parsed.Waves[index] < 1 || parsed.Waves[index] > RunSaveCodec.MaxWave)
                {
                    return false;
                }

                for (int earlier = 0; earlier < index; earlier++)
                {
                    if (parsed.Dates[earlier] == parsed.Dates[index])
                    {
                        return false;
                    }
                }
            }

            data = parsed;
            return true;
        }

        private static bool Assign(
            DailyBoardData data,
            string key,
            int number,
            bool[] sawDate,
            bool[] sawScore,
            bool[] sawWave,
            ref bool sawVersion)
        {
            if (key == "Version")
            {
                if (sawVersion)
                {
                    return false;
                }

                data.Version = number;
                sawVersion = true;
                return true;
            }

            if (key == "Count")
            {
                if (data.Count >= 0)
                {
                    return false;
                }

                data.Count = number;
                return true;
            }

            int index;
            if (TryIndex(key, "Date", out index))
            {
                if (sawDate[index])
                {
                    return false;
                }

                data.Dates[index] = number;
                sawDate[index] = true;
                return true;
            }

            if (TryIndex(key, "Score", out index))
            {
                if (sawScore[index])
                {
                    return false;
                }

                data.Scores[index] = number;
                sawScore[index] = true;
                return true;
            }

            if (TryIndex(key, "Wave", out index))
            {
                if (sawWave[index])
                {
                    return false;
                }

                data.Waves[index] = number;
                sawWave[index] = true;
                return true;
            }

            return false;
        }

        private static bool TryIndex(string key, string prefix, out int index)
        {
            index = 0;
            if (key == null || key.Length <= prefix.Length || !key.StartsWith(prefix))
            {
                return false;
            }

            int value = 0;
            for (int cursor = prefix.Length; cursor < key.Length; cursor++)
            {
                char ch = key[cursor];
                if (ch < '0' || ch > '9')
                {
                    return false;
                }

                value = (value * 10) + (ch - '0');
                if (value >= DailyBoardRules.Cap)
                {
                    return false;
                }
            }

            index = value;
            return true;
        }

        private static void SkipSpace(string text, ref int cursor)
        {
            while (cursor < text.Length)
            {
                char ch = text[cursor];
                if (ch != ' ' && ch != '\n' && ch != '\r' && ch != '\t')
                {
                    return;
                }

                cursor += 1;
            }
        }

        private static bool ReadString(string text, ref int cursor, out string value)
        {
            value = string.Empty;
            if (cursor >= text.Length || text[cursor] != '"')
            {
                return false;
            }

            cursor += 1;
            int start = cursor;
            while (cursor < text.Length && text[cursor] != '"')
            {
                cursor += 1;
            }

            if (cursor >= text.Length)
            {
                return false;
            }

            value = text.Substring(start, cursor - start);
            cursor += 1;
            return true;
        }

        private static bool ReadInt(string text, ref int cursor, out int number)
        {
            number = 0;
            if (cursor >= text.Length)
            {
                return false;
            }

            int sign = 1;
            if (text[cursor] == '-')
            {
                sign = -1;
                cursor += 1;
            }

            if (cursor >= text.Length || text[cursor] < '0' || text[cursor] > '9')
            {
                return false;
            }

            int digits = 0;
            while (cursor < text.Length && text[cursor] >= '0' && text[cursor] <= '9')
            {
                digits += 1;
                if (digits > 9)
                {
                    return false;
                }

                number = (number * 10) + (text[cursor] - '0');
                cursor += 1;
            }

            number *= sign;
            return digits > 0;
        }
    }
}
