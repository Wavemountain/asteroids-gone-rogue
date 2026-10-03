using System;
using System.Globalization;
using System.Text;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Meta progress that survives a failed run: legacy points, a small perk
    /// shop, and a best score per difficulty. Unity-free.
    /// </summary>
    [Serializable]
    public sealed class MetaData
    {
        public int Version;
        public int LegacyPoints;
        public int CreditPerk;
        public int DiscountPerk;
        public int ShieldPerk;
        public int HullPerk;
        public int NextRunId;
        public string Awarded = string.Empty;
        public int BestScore0;
        public int BestWave0;
        public int BestWorld0;
        public int BestScore1;
        public int BestWave1;
        public int BestWorld1;
        public int BestScore2;
        public int BestWave2;
        public int BestWorld2;

        /// <summary>
        /// High-water bank counts per run id, "id:count,id:count".
        /// Missing on older meta files, which means nothing was recorded.
        /// </summary>
        public string BankedRuns = string.Empty;

        public static MetaData Fresh()
        {
            MetaData data = new MetaData();
            data.Version = LegacyProgress.CurrentVersion;
            data.NextRunId = 1;
            data.Awarded = string.Empty;
            data.BankedRuns = string.Empty;
            return data;
        }
    }

    public static class LegacyProgress
    {
        public const int CurrentVersion = 1;
        public const int PerkCount = 4;
        public const int MaxLevel = 3;
        public const int CreditsPerk = 0;
        public const int DiscountPerk = 1;
        public const int ShieldPerk = 2;
        public const int HullPerk = 3;
        public const int CreditsPerLevel = 10;
        public const int DiscountPercentPerLevel = 7;
        public const int DiscountCapPercent = 20;
        public const int ShieldBonusCap = 2;
        public const int HullBonusCap = 2;
        public const int ShieldDurationStep = 10;
        public const int HullRegenStep = 5;
        public const int AwardMemory = 32;

        public static int ClampLevel(int level)
        {
            if (level < 0)
            {
                return 0;
            }

            if (level > MaxLevel)
            {
                return MaxLevel;
            }

            return level;
        }

        public static int Level(MetaData meta, int perk)
        {
            if (meta == null)
            {
                return 0;
            }

            int raw;
            switch (perk)
            {
                case CreditsPerk:
                    raw = meta.CreditPerk;
                    break;
                case DiscountPerk:
                    raw = meta.DiscountPerk;
                    break;
                case ShieldPerk:
                    raw = meta.ShieldPerk;
                    break;
                case HullPerk:
                    raw = meta.HullPerk;
                    break;
                default:
                    return 0;
            }

            return ClampLevel(raw);
        }

        public static int StartingCredits(int level)
        {
            return CreditsPerLevel * ClampLevel(level);
        }

        public static int StartingCredits(MetaData meta)
        {
            return StartingCredits(Level(meta, CreditsPerk));
        }

        public static int DiscountPercent(int level)
        {
            int raw = DiscountPercentPerLevel * ClampLevel(level);
            if (raw > DiscountCapPercent)
            {
                return DiscountCapPercent;
            }

            return raw;
        }

        public static int DiscountPercent(MetaData meta)
        {
            return DiscountPercent(Level(meta, DiscountPerk));
        }

        public static int StartingShield(int level)
        {
            int rank = ClampLevel(level);
            if (rank <= 0)
            {
                return 0;
            }

            if (rank >= MaxLevel)
            {
                return ShieldBonusCap;
            }

            return 1;
        }

        /// <summary>100, 100, 110, 120 for levels 0-3. Level 2 is the first longer shield.</summary>
        public static int ShieldDurationPercent(int level)
        {
            int rank = ClampLevel(level);
            if (rank < 2)
            {
                return 100;
            }

            int bonus = (rank - 1) * ShieldDurationStep;
            return 100 + bonus;
        }

        public static int StartingShield(MetaData meta)
        {
            return StartingShield(Level(meta, ShieldPerk));
        }

        public static int HullBonus(int level)
        {
            int rank = ClampLevel(level);
            if (rank <= 0)
            {
                return 0;
            }

            if (rank >= MaxLevel)
            {
                return HullBonusCap;
            }

            return 1;
        }

        /// <summary>0, 0, 5, 10. Level 2 mends between waves; level 3 also adds the second hull hit.</summary>
        public static int HullRegenPercent(int level)
        {
            int rank = ClampLevel(level);
            if (rank < 2)
            {
                return 0;
            }

            return (rank - 1) * HullRegenStep;
        }

        public static int MendHull(int current, int maxHull, int level)
        {
            if (maxHull < 1)
            {
                return current < 0 ? 0 : current;
            }

            if (current < 0)
            {
                current = 0;
            }

            if (current > maxHull)
            {
                current = maxHull;
            }

            int percent = HullRegenPercent(level);
            if (percent <= 0 || current >= maxHull)
            {
                return current;
            }

            int missing = maxHull - current;
            int gained = missing * percent / 100;
            if (gained < 1)
            {
                gained = 1;
            }

            int mended = current + gained;
            if (mended > maxHull)
            {
                mended = maxHull;
            }

            return mended;
        }

        public static int HullBonus(MetaData meta)
        {
            return HullBonus(Level(meta, HullPerk));
        }

        public static int ApplyDiscount(int cost, int percent)
        {
            if (cost <= 0)
            {
                return 0;
            }

            int pct = percent < 0 ? 0 : percent;
            if (pct > DiscountCapPercent)
            {
                pct = DiscountCapPercent;
            }

            int reduced = (cost * (100 - pct)) / 100;
            if (reduced < 1)
            {
                return 1;
            }

            return reduced;
        }

        /// <summary>
        /// Cost of the next level. Level 0 costs 2, then 3, then 4.
        /// </summary>
        public static int CostToRaise(int level)
        {
            int current = ClampLevel(level);
            if (current >= MaxLevel)
            {
                return 0;
            }

            return 2 + current;
        }

        public static bool ShopVisible(GamePhase phase, bool continueOffer, bool runLaunched)
        {
            if (continueOffer)
            {
                return true;
            }

            if (phase == GamePhase.Failed)
            {
                return true;
            }

            return phase == GamePhase.Hangar && !runLaunched;
        }

        /// <summary>
        /// 1 point per cleared world plus 1 point per 5 waves reached.
        /// Both inputs are monotonic: raising either never lowers the award.
        /// </summary>
        public static int AwardPoints(int worldsCleared, int highestWave)
        {
            int worlds = worldsCleared < 0 ? 0 : worldsCleared;
            int wave = highestWave < 0 ? 0 : highestWave;
            return worlds + (wave / 5);
        }

        public static void ProgressOf(int waveIndex, bool diedOnWave, out int worldsCleared, out int highestWave)
        {
            int index = waveIndex < 1 ? 1 : waveIndex;
            if (diedOnWave)
            {
                highestWave = index;
                int cleared = index - 1;
                if (cleared < 0)
                {
                    cleared = 0;
                }

                worldsCleared = cleared / 5;
                return;
            }

            highestWave = index - 1;
            if (highestWave < 0)
            {
                highestWave = 0;
            }

            worldsCleared = highestWave / 5;
        }

        public static bool AlreadyAwarded(string awarded, int runId)
        {
            if (runId < 1 || string.IsNullOrEmpty(awarded))
            {
                return false;
            }

            string token = runId.ToString(CultureInfo.InvariantCulture);
            int cursor = 0;
            while (cursor < awarded.Length)
            {
                int comma = awarded.IndexOf(',', cursor);
                int stop = comma < 0 ? awarded.Length : comma;
                string part = awarded.Substring(cursor, stop - cursor);
                if (part == token)
                {
                    return true;
                }

                if (comma < 0)
                {
                    break;
                }

                cursor = comma + 1;
            }

            return false;
        }

        public static string Remember(string awarded, int runId)
        {
            string token = runId.ToString(CultureInfo.InvariantCulture);
            string next = string.IsNullOrEmpty(awarded) ? token : awarded + "," + token;
            int count = 1;
            for (int i = 0; i < next.Length; i++)
            {
                if (next[i] == ',')
                {
                    count += 1;
                }
            }

            while (count > AwardMemory)
            {
                int comma = next.IndexOf(',');
                if (comma < 0)
                {
                    break;
                }

                next = next.Substring(comma + 1);
                count -= 1;
            }

            return next;
        }

        /// <summary>
        /// Awards once per run id. Returns the points added (0 when repeated).
        /// </summary>
        public static int TryAward(MetaData meta, int runId, int worldsCleared, int highestWave)
        {
            if (meta == null || runId < 1)
            {
                return 0;
            }

            if (AlreadyAwarded(meta.Awarded, runId))
            {
                return 0;
            }

            int gained = AwardPoints(worldsCleared, highestWave);
            if (gained < 0)
            {
                gained = 0;
            }

            meta.LegacyPoints += gained;
            meta.Awarded = Remember(meta.Awarded, runId);
            return gained;
        }

        public static int TakeRunId(MetaData meta)
        {
            if (meta == null)
            {
                return 1;
            }

            if (meta.NextRunId < 1)
            {
                meta.NextRunId = 1;
            }

            int id = meta.NextRunId;
            meta.NextRunId = id + 1;
            return id;
        }

        public static bool TryBuy(MetaData meta, int perk)
        {
            if (meta == null || perk < 0 || perk >= PerkCount)
            {
                return false;
            }

            int level = Level(meta, perk);
            if (level >= MaxLevel)
            {
                return false;
            }

            int cost = CostToRaise(level);
            if (meta.LegacyPoints < cost)
            {
                return false;
            }

            meta.LegacyPoints -= cost;
            SetLevel(meta, perk, level + 1);
            return true;
        }

        public static bool TryRecordBest(MetaData meta, int difficulty, int score, int wave, int world)
        {
            if (meta == null)
            {
                return false;
            }

            int grade = difficulty;
            if (grade < 0)
            {
                grade = 0;
            }

            if (grade > 2)
            {
                grade = 2;
            }

            int bestScore;
            int bestWave;
            int bestWorld;
            ReadBest(meta, grade, out bestScore, out bestWave, out bestWorld);
            bool has = bestWave > 0 || bestScore > 0;
            if (has && !LocalBest.IsBetter(bestScore, bestWave, bestWorld, score, wave, world))
            {
                return false;
            }

            WriteBest(meta, grade, score, wave, world);
            return true;
        }

        public static void ReadBest(MetaData meta, int difficulty, out int score, out int wave, out int world)
        {
            score = 0;
            wave = 0;
            world = 0;
            if (meta == null)
            {
                return;
            }

            if (difficulty <= 0)
            {
                score = meta.BestScore0;
                wave = meta.BestWave0;
                world = meta.BestWorld0;
                return;
            }

            if (difficulty == 1)
            {
                score = meta.BestScore1;
                wave = meta.BestWave1;
                world = meta.BestWorld1;
                return;
            }

            score = meta.BestScore2;
            wave = meta.BestWave2;
            world = meta.BestWorld2;
        }

        public static string HangarLine(int points, int score, int wave, int world)
        {
            bool has = wave > 0 || score > 0;
            if (!has)
            {
                return Loc.Tf("ui.legacy.line_empty", "Legacy {0}  ·  Best —", points);
            }

            return Loc.Tf(
                "ui.legacy.line",
                "Legacy {0}  ·  Best {1}  ·  Wave {2}  ·  World {3}",
                points,
                score,
                wave,
                world);
        }

        public static string PerkLabel(MetaData meta, int perk)
        {
            int level = Level(meta, perk);
            int shownLevel = level >= MaxLevel ? MaxLevel : level + 1;
            string effect;
            switch (perk)
            {
                case DiscountPerk:
                    effect = Loc.Tf(
                        "ui.legacy.discount",
                        "First -{0}%",
                        DiscountPercent(shownLevel));
                    break;
                case ShieldPerk:
                    effect = Loc.Tf(
                        "ui.legacy.shield",
                        "Shield +{0}",
                        StartingShield(shownLevel));
                    if (ShieldDurationPercent(shownLevel) > 100)
                    {
                        effect += Loc.Tf(
                            "ui.legacy.shield_dur",
                            "  ·  {0}% time",
                            ShieldDurationPercent(shownLevel));
                    }

                    break;
                case HullPerk:
                    effect = Loc.Tf(
                        "ui.legacy.hull",
                        "Hull +{0}",
                        HullBonus(shownLevel));
                    if (HullRegenPercent(shownLevel) > 0)
                    {
                        effect += Loc.Tf(
                            "ui.legacy.hull_regen",
                            "  ·  +{0}% mend",
                            HullRegenPercent(shownLevel));
                    }

                    break;
                default:
                    effect = Loc.Tf(
                        "ui.legacy.credits",
                        "+{0} cr",
                        StartingCredits(shownLevel));
                    break;
            }

            if (level >= MaxLevel)
            {
                return effect + "  " + Loc.T("ui.legacy.max", "MAX");
            }

            return effect + "  " + CostToRaise(level).ToString(CultureInfo.InvariantCulture);
        }

        private static void SetLevel(MetaData meta, int perk, int level)
        {
            int clamped = ClampLevel(level);
            switch (perk)
            {
                case CreditsPerk:
                    meta.CreditPerk = clamped;
                    break;
                case DiscountPerk:
                    meta.DiscountPerk = clamped;
                    break;
                case ShieldPerk:
                    meta.ShieldPerk = clamped;
                    break;
                case HullPerk:
                    meta.HullPerk = clamped;
                    break;
                default:
                    break;
            }
        }

        private static void WriteBest(MetaData meta, int difficulty, int score, int wave, int world)
        {
            if (difficulty <= 0)
            {
                meta.BestScore0 = score;
                meta.BestWave0 = wave;
                meta.BestWorld0 = world;
                return;
            }

            if (difficulty == 1)
            {
                meta.BestScore1 = score;
                meta.BestWave1 = wave;
                meta.BestWorld1 = world;
                return;
            }

            meta.BestScore2 = score;
            meta.BestWave2 = wave;
            meta.BestWorld2 = world;
        }

        /// <summary>
        /// Credits already banked for this run id in the meta file.
        /// A missing entry is 0, which is the pre-field behaviour.
        /// </summary>
        public static int BankedCount(MetaData meta, int runId)
        {
            if (meta == null || runId < 1 || string.IsNullOrEmpty(meta.BankedRuns))
            {
                return 0;
            }

            string[] parts = meta.BankedRuns.Split(',');
            for (int index = 0; index < parts.Length; index++)
            {
                int id;
                int count;
                if (!TryReadBankToken(parts[index], out id, out count))
                {
                    continue;
                }

                if (id == runId)
                {
                    return ClampBank(count);
                }
            }

            return 0;
        }

        /// <summary>
        /// The cap is the maximum of the run save and the meta high-water mark.
        /// Restoring an older run_save.json cannot lower a count already banked,
        /// and a higher run-save count is kept if the meta write was missed.
        /// </summary>
        public static int MergedBanked(int runCount, int metaCount)
        {
            int fromRun = runCount < 0 ? 0 : runCount;
            int fromMeta = metaCount < 0 ? 0 : metaCount;
            int high = fromRun > fromMeta ? fromRun : fromMeta;
            return ClampBank(high);
        }

        public static void RememberBanked(MetaData meta, int runId, int count)
        {
            if (meta == null || runId < 1)
            {
                return;
            }

            int stored = MergedBanked(count, BankedCount(meta, runId));
            string[] parts = string.IsNullOrEmpty(meta.BankedRuns)
                ? new string[0]
                : meta.BankedRuns.Split(',');
            StringBuilder builder = new StringBuilder(64);
            int kept = 0;
            bool replaced = false;
            int start = 0;
            if (parts.Length > AwardMemory)
            {
                start = parts.Length - AwardMemory;
            }

            for (int index = start; index < parts.Length; index++)
            {
                int id;
                int previous;
                if (!TryReadBankToken(parts[index], out id, out previous))
                {
                    continue;
                }

                if (id == runId)
                {
                    previous = stored;
                    replaced = true;
                }

                if (kept > 0)
                {
                    builder.Append(',');
                }

                builder.Append(id.ToString(CultureInfo.InvariantCulture));
                builder.Append(':');
                builder.Append(ClampBank(previous).ToString(CultureInfo.InvariantCulture));
                kept += 1;
            }

            if (!replaced)
            {
                if (kept > 0)
                {
                    builder.Append(',');
                }

                builder.Append(runId.ToString(CultureInfo.InvariantCulture));
                builder.Append(':');
                builder.Append(stored.ToString(CultureInfo.InvariantCulture));
            }

            meta.BankedRuns = builder.ToString();
        }

        private static int ClampBank(int count)
        {
            if (count < 0)
            {
                return 0;
            }

            if (count > ShopPrices.BankMaxPerRun)
            {
                return ShopPrices.BankMaxPerRun;
            }

            return count;
        }

        private static bool TryReadBankToken(string token, out int runId, out int count)
        {
            runId = 0;
            count = 0;
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            int colon = token.IndexOf(':');
            if (colon <= 0 || colon >= token.Length - 1)
            {
                return false;
            }

            string idText = token.Substring(0, colon);
            string countText = token.Substring(colon + 1);
            if (!int.TryParse(idText, NumberStyles.None, CultureInfo.InvariantCulture, out runId))
            {
                return false;
            }

            if (!int.TryParse(countText, NumberStyles.None, CultureInfo.InvariantCulture, out count))
            {
                return false;
            }

            return runId >= 1;
        }
    }

    public static class MetaCodec
    {
        public static bool IsValid(MetaData data)
        {
            if (data == null)
            {
                return false;
            }

            if (data.Version != LegacyProgress.CurrentVersion)
            {
                return false;
            }

            if (data.LegacyPoints < 0 || data.NextRunId < 1)
            {
                return false;
            }

            if (!LevelOk(data.CreditPerk) || !LevelOk(data.DiscountPerk))
            {
                return false;
            }

            if (!LevelOk(data.ShieldPerk) || !LevelOk(data.HullPerk))
            {
                return false;
            }

            if (data.Awarded == null)
            {
                return false;
            }

            if (data.BankedRuns == null)
            {
                data.BankedRuns = string.Empty;
            }

            if (data.BestScore0 < 0 || data.BestWave0 < 0 || data.BestWorld0 < 0)
            {
                return false;
            }

            if (data.BestScore1 < 0 || data.BestWave1 < 0 || data.BestWorld1 < 0)
            {
                return false;
            }

            if (data.BestScore2 < 0 || data.BestWave2 < 0 || data.BestWorld2 < 0)
            {
                return false;
            }

            return true;
        }

        public static string ToJson(MetaData data)
        {
            if (data == null)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder(256);
            builder.Append('{');
            AppendInt(builder, "Version", data.Version, true);
            AppendInt(builder, "LegacyPoints", data.LegacyPoints, false);
            AppendInt(builder, "CreditPerk", data.CreditPerk, false);
            AppendInt(builder, "DiscountPerk", data.DiscountPerk, false);
            AppendInt(builder, "ShieldPerk", data.ShieldPerk, false);
            AppendInt(builder, "HullPerk", data.HullPerk, false);
            AppendInt(builder, "NextRunId", data.NextRunId, false);
            AppendInt(builder, "BestScore0", data.BestScore0, false);
            AppendInt(builder, "BestWave0", data.BestWave0, false);
            AppendInt(builder, "BestWorld0", data.BestWorld0, false);
            AppendInt(builder, "BestScore1", data.BestScore1, false);
            AppendInt(builder, "BestWave1", data.BestWave1, false);
            AppendInt(builder, "BestWorld1", data.BestWorld1, false);
            AppendInt(builder, "BestScore2", data.BestScore2, false);
            AppendInt(builder, "BestWave2", data.BestWave2, false);
            AppendInt(builder, "BestWorld2", data.BestWorld2, false);
            builder.Append(",\"Awarded\":\"");
            builder.Append(data.Awarded == null ? string.Empty : data.Awarded);
            builder.Append("\",\"BankedRuns\":\"");
            builder.Append(data.BankedRuns == null ? string.Empty : data.BankedRuns);
            builder.Append("\"}");
            return builder.ToString();
        }

        public static bool TryParse(string json, out MetaData data)
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

            MetaData parsed = MetaData.Fresh();
            parsed.Awarded = string.Empty;
            parsed.BankedRuns = string.Empty;
            bool sawVersion = false;
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
                if (key == "Awarded" || key == "BankedRuns")
                {
                    string text;
                    if (!ReadString(trimmed, ref cursor, out text))
                    {
                        return false;
                    }

                    if (key == "Awarded")
                    {
                        parsed.Awarded = text;
                    }
                    else
                    {
                        parsed.BankedRuns = text;
                    }
                }
                else
                {
                    int number;
                    if (!ReadInt(trimmed, ref cursor, out number))
                    {
                        return false;
                    }

                    if (key == "Version")
                    {
                        parsed.Version = number;
                        sawVersion = true;
                    }
                    else if (!Assign(parsed, key, number))
                    {
                        return false;
                    }
                }
            }

            if (!sawVersion || !IsValid(parsed))
            {
                return false;
            }

            data = parsed;
            return true;
        }

        private static bool LevelOk(int level)
        {
            return level >= 0 && level <= LegacyProgress.MaxLevel;
        }

        private static bool Assign(MetaData data, string key, int number)
        {
            switch (key)
            {
                case "LegacyPoints":
                    data.LegacyPoints = number;
                    return true;
                case "CreditPerk":
                    data.CreditPerk = number;
                    return true;
                case "DiscountPerk":
                    data.DiscountPerk = number;
                    return true;
                case "ShieldPerk":
                    data.ShieldPerk = number;
                    return true;
                case "HullPerk":
                    data.HullPerk = number;
                    return true;
                case "NextRunId":
                    data.NextRunId = number;
                    return true;
                case "BestScore0":
                    data.BestScore0 = number;
                    return true;
                case "BestWave0":
                    data.BestWave0 = number;
                    return true;
                case "BestWorld0":
                    data.BestWorld0 = number;
                    return true;
                case "BestScore1":
                    data.BestScore1 = number;
                    return true;
                case "BestWave1":
                    data.BestWave1 = number;
                    return true;
                case "BestWorld1":
                    data.BestWorld1 = number;
                    return true;
                case "BestScore2":
                    data.BestScore2 = number;
                    return true;
                case "BestWave2":
                    data.BestWave2 = number;
                    return true;
                case "BestWorld2":
                    data.BestWorld2 = number;
                    return true;
                default:
                    return true;
            }
        }

        private static void AppendInt(StringBuilder builder, string key, int value, bool first)
        {
            if (!first)
            {
                builder.Append(',');
            }

            builder.Append('"');
            builder.Append(key);
            builder.Append("\":");
            builder.Append(value.ToString(CultureInfo.InvariantCulture));
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
            StringBuilder builder = new StringBuilder();
            while (cursor < text.Length)
            {
                char ch = text[cursor];
                cursor += 1;
                if (ch == '"')
                {
                    value = builder.ToString();
                    return true;
                }

                if (ch == '\\')
                {
                    return false;
                }

                builder.Append(ch);
            }

            return false;
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
            int value = 0;
            while (cursor < text.Length && text[cursor] >= '0' && text[cursor] <= '9')
            {
                digits += 1;
                if (digits > 9)
                {
                    return false;
                }

                value = (value * 10) + (text[cursor] - '0');
                cursor += 1;
            }

            number = value * sign;
            return true;
        }
    }
}
