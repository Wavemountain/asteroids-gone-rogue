using System;
using System.Globalization;
using System.Text;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Versioned snapshot of the current run. Unity-free: tests can round-trip
    /// the JSON without the Editor. The file wrapper is <see cref="RunSaveStore"/>.
    /// </summary>
    [Serializable]
    public sealed class RunSaveData
    {
        public int Version;
        public int WaveIndex;
        public int Score;
        public int Credits;
        public int Lives;
        public int Hull;
        public int Shield;
        public int Difficulty;
        public int UpgradeMask;
        public int Doctrine;
        public int PrimaryMode;
        public int UtilityMode;
        public int HasUtility;
        public int RunId;
        public int LastResolvedWave;
        public int LastRunScore;
        public int LastCreditsAwarded;
        public int ExtraLifeStreak;
        public int LegacyHull;
        public int FirstDiscount;
        public int FirstDiscountUsed;
        public int BoonLevels;
        public int BoonPending;
        public int BoonOffer;
        public int WaveInProgress;
        public int LivesAtWaveStart;
        public int Mk2Mask;
        public int BankedLegacy;
        public int ExtraLifeWorld;
        public string Timestamp = string.Empty;
    }

    public static class RunSaveCodec
    {
        public const int CurrentVersion = 3;
        public const int UpgradeBitCount = 22;
        public const int MaxWave = 9999;
        public const int MaxScore = 100000000;
        public const int FireModeCount = 7;

        public static bool ShouldWrite(GamePhase phase)
        {
            return phase == GamePhase.Hangar || phase == GamePhase.WaveClear;
        }

        public static bool ShouldDelete(GamePhase phase)
        {
            return phase == GamePhase.Failed;
        }

        /// <summary>
        /// A new file may replace the previous one only when the candidate parses
        /// and matches this version. Invalid JSON never commits.
        /// </summary>
        public static bool CommitAllowed(string json)
        {
            RunSaveData parsed;
            return TryParse(json, out parsed);
        }

        public static RunSaveData Capture(
            GameSession session,
            LoadoutState loadout,
            int difficulty,
            int runId,
            string timestamp,
            BoonRun boons)
        {
            RunSaveData data = new RunSaveData();
            data.Version = CurrentVersion;
            if (session != null)
            {
                data.WaveIndex = session.WaveIndex;
                data.Score = session.Score;
                data.Credits = session.Credits;
                data.Lives = session.Lives;
                data.LastResolvedWave = session.LastResolvedWave;
                data.LastRunScore = session.LastRunScore;
                data.LastCreditsAwarded = session.LastCreditsAwarded;
                data.ExtraLifeStreak = session.ExtraLifeStreak;
            }

            if (loadout != null)
            {
                data.Hull = loadout.CurrentHullHitPoints;
                data.Shield = loadout.ShieldCharges;
                data.UpgradeMask = PackUpgrades(loadout);
                data.Doctrine = (int)loadout.Doctrine;
                data.PrimaryMode = (int)loadout.PrimaryMode;
                data.UtilityMode = (int)loadout.UtilityMode;
                data.HasUtility = loadout.HasUtility ? 1 : 0;
                data.LegacyHull = loadout.LegacyHullBonus;
                data.FirstDiscount = loadout.FirstDiscountPercent;
                data.FirstDiscountUsed = loadout.FirstDiscountUsed ? 1 : 0;
                data.Mk2Mask = loadout.Mk2Mask;
            }

            if (session != null)
            {
                data.WaveInProgress = session.WaveInProgress ? 1 : 0;
                data.LivesAtWaveStart = session.LivesAtWaveStart;
                data.BankedLegacy = session.BankedLegacy;
                data.ExtraLifeWorld = session.ExtraLifeWorld;
            }

            data.Difficulty = difficulty;
            data.RunId = runId;
            data.Timestamp = timestamp == null ? string.Empty : timestamp;
            if (boons != null)
            {
                data.BoonLevels = boons.PackedLevels();
                data.BoonPending = boons.Pending ? 1 : 0;
                data.BoonOffer = boons.PackedOffer();
            }

            return data;
        }

        public static int PackUpgrades(LoadoutState loadout)
        {
            if (loadout == null)
            {
                return 0;
            }

            int mask = 0;
            mask = PutBit(mask, 0, loadout.RapidFire);
            mask = PutBit(mask, 1, loadout.NoseHardpoint);
            mask = PutBit(mask, 2, loadout.BodyUpgrade01);
            mask = PutBit(mask, 3, loadout.BodyUpgrade02);
            mask = PutBit(mask, 4, loadout.NoseUpgrade02);
            mask = PutBit(mask, 5, loadout.NoseUpgrade03);
            mask = PutBit(mask, 6, loadout.EngineUpgrade02);
            mask = PutBit(mask, 7, loadout.EngineUpgrade03);
            mask = PutBit(mask, 8, loadout.SpreadBolt);
            mask = PutBit(mask, 9, loadout.Pierce);
            mask = PutBit(mask, 10, loadout.TwinGuns);
            mask = PutBit(mask, 11, loadout.Seeker);
            mask = PutBit(mask, 12, loadout.Ricochet);
            mask = PutBit(mask, 13, loadout.ShieldMatrix);
            mask = PutBit(mask, 14, loadout.Overcharger);
            mask = PutBit(mask, 15, loadout.Afterburner);
            mask = PutBit(mask, 16, loadout.FlakFeed);
            mask = PutBit(mask, 17, loadout.Storm);
            mask = PutBit(mask, 18, loadout.Rail);
            mask = PutBit(mask, 19, loadout.OverchargeLance);
            mask = PutBit(mask, 20, loadout.SeekerCadence);
            mask = PutBit(mask, 21, loadout.TwinSeek);
            return mask;
        }

        public static bool IsValid(RunSaveData data)
        {
            if (data == null)
            {
                return false;
            }

            if (data.Version != 1 && data.Version != 2 && data.Version != CurrentVersion)
            {
                return false;
            }

            if (data.WaveIndex < 1 || data.WaveIndex > MaxWave)
            {
                return false;
            }

            if (data.Score < 0 || data.Score > MaxScore)
            {
                return false;
            }

            if (data.Credits < 0 || data.Credits > MaxScore)
            {
                return false;
            }

            if (data.Lives < 1 || data.Lives > DifficultySettings.MaxLives + 1)
            {
                return false;
            }

            if (data.Hull < 1 || data.Hull > 12)
            {
                return false;
            }

            if (data.Shield < 0 || data.Shield > LoadoutState.SaveMaxShield)
            {
                return false;
            }

            if (data.Difficulty < 0 || data.Difficulty > (int)DifficultyGrade.Hard)
            {
                return false;
            }

            int maskLimit = 1 << UpgradeBitCount;
            if (data.UpgradeMask < 0 || data.UpgradeMask >= maskLimit)
            {
                return false;
            }

            if (data.Doctrine < 0 || data.Doctrine > (int)DoctrineId.Hunter)
            {
                return false;
            }

            if (data.PrimaryMode < 0 || data.PrimaryMode >= FireModeCount)
            {
                return false;
            }

            if (data.UtilityMode < 0 || data.UtilityMode >= FireModeCount)
            {
                return false;
            }

            if (data.HasUtility != 0 && data.HasUtility != 1)
            {
                return false;
            }

            if (data.RunId < 1)
            {
                return false;
            }

            if (data.LastResolvedWave < 0 || data.LastResolvedWave > MaxWave)
            {
                return false;
            }

            if (data.LastRunScore < 0 || data.LastCreditsAwarded < 0 || data.ExtraLifeStreak < 0)
            {
                return false;
            }

            if (data.LegacyHull < 0 || data.LegacyHull > LegacyProgress.HullBonusCap)
            {
                return false;
            }

            if (data.FirstDiscount < 0 || data.FirstDiscount > LegacyProgress.DiscountCapPercent)
            {
                return false;
            }

            if (data.FirstDiscountUsed != 0 && data.FirstDiscountUsed != 1)
            {
                return false;
            }

            if (string.IsNullOrEmpty(data.Timestamp) || data.Timestamp.Length > 40)
            {
                return false;
            }

            if (data.Version >= 2)
            {
                if (!BoonCatalog.LevelsValid(data.BoonLevels))
                {
                    return false;
                }

                if (data.BoonPending != 0 && data.BoonPending != 1)
                {
                    return false;
                }

                if (!BoonCatalog.OfferValid(data.BoonOffer, data.BoonPending, data.BoonLevels))
                {
                    return false;
                }
            }

            if (data.WaveInProgress != 0 && data.WaveInProgress != 1)
            {
                return false;
            }

            if (data.LivesAtWaveStart < 0 || data.LivesAtWaveStart > DifficultySettings.MaxLives + 1)
            {
                return false;
            }

            int mk2Limit = 1 << UpgradeBitCount;
            if (data.Mk2Mask < 0 || data.Mk2Mask >= mk2Limit)
            {
                return false;
            }

            if (data.BankedLegacy < 0 || data.BankedLegacy > ShopPrices.BankMaxPerRun)
            {
                return false;
            }

            if (data.ExtraLifeWorld < 0 || data.ExtraLifeWorld > MaxWave)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Abandoned wave costs one life from the count stored at wave start.
        /// Never drops below 1, and never ends the run.
        /// </summary>
        public static int LivesAfterAbandonedWave(int lives, int livesAtWaveStart)
        {
            int start = livesAtWaveStart > 0 ? livesAtWaveStart : lives;
            if (start < 1)
            {
                start = 1;
            }

            int next = start - 1;
            if (next < 1)
            {
                next = 1;
            }

            return next;
        }

        public static void ApplyAbandonedWave(RunSaveData data)
        {
            if (data == null || data.WaveInProgress == 0)
            {
                return;
            }

            data.Lives = LivesAfterAbandonedWave(data.Lives, data.LivesAtWaveStart);
            data.WaveInProgress = 0;
            data.LivesAtWaveStart = 0;
        }

        public static bool Same(RunSaveData left, RunSaveData right)
        {
            if (left == null || right == null)
            {
                return false;
            }

            return left.Version == right.Version
                && left.WaveIndex == right.WaveIndex
                && left.Score == right.Score
                && left.Credits == right.Credits
                && left.Lives == right.Lives
                && left.Hull == right.Hull
                && left.Shield == right.Shield
                && left.Difficulty == right.Difficulty
                && left.UpgradeMask == right.UpgradeMask
                && left.Doctrine == right.Doctrine
                && left.PrimaryMode == right.PrimaryMode
                && left.UtilityMode == right.UtilityMode
                && left.HasUtility == right.HasUtility
                && left.RunId == right.RunId
                && left.LastResolvedWave == right.LastResolvedWave
                && left.LastRunScore == right.LastRunScore
                && left.LastCreditsAwarded == right.LastCreditsAwarded
                && left.ExtraLifeStreak == right.ExtraLifeStreak
                && left.LegacyHull == right.LegacyHull
                && left.FirstDiscount == right.FirstDiscount
                && left.FirstDiscountUsed == right.FirstDiscountUsed
                && left.BoonLevels == right.BoonLevels
                && left.BoonPending == right.BoonPending
                && left.BoonOffer == right.BoonOffer
                && left.WaveInProgress == right.WaveInProgress
                && left.LivesAtWaveStart == right.LivesAtWaveStart
                && left.Mk2Mask == right.Mk2Mask
                && left.BankedLegacy == right.BankedLegacy
                && left.ExtraLifeWorld == right.ExtraLifeWorld
                && left.Timestamp == right.Timestamp;
        }

        public static string ToJson(RunSaveData data)
        {
            if (data == null)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder(256);
            builder.Append('{');
            AppendInt(builder, "Version", data.Version, true);
            AppendInt(builder, "WaveIndex", data.WaveIndex, false);
            AppendInt(builder, "Score", data.Score, false);
            AppendInt(builder, "Credits", data.Credits, false);
            AppendInt(builder, "Lives", data.Lives, false);
            AppendInt(builder, "Hull", data.Hull, false);
            AppendInt(builder, "Shield", data.Shield, false);
            AppendInt(builder, "Difficulty", data.Difficulty, false);
            AppendInt(builder, "UpgradeMask", data.UpgradeMask, false);
            AppendInt(builder, "Doctrine", data.Doctrine, false);
            AppendInt(builder, "PrimaryMode", data.PrimaryMode, false);
            AppendInt(builder, "UtilityMode", data.UtilityMode, false);
            AppendInt(builder, "HasUtility", data.HasUtility, false);
            AppendInt(builder, "RunId", data.RunId, false);
            AppendInt(builder, "LastResolvedWave", data.LastResolvedWave, false);
            AppendInt(builder, "LastRunScore", data.LastRunScore, false);
            AppendInt(builder, "LastCreditsAwarded", data.LastCreditsAwarded, false);
            AppendInt(builder, "ExtraLifeStreak", data.ExtraLifeStreak, false);
            AppendInt(builder, "LegacyHull", data.LegacyHull, false);
            AppendInt(builder, "FirstDiscount", data.FirstDiscount, false);
            AppendInt(builder, "FirstDiscountUsed", data.FirstDiscountUsed, false);
            AppendInt(builder, "BoonLevels", data.BoonLevels, false);
            AppendInt(builder, "BoonPending", data.BoonPending, false);
            AppendInt(builder, "BoonOffer", data.BoonOffer, false);
            AppendInt(builder, "WaveInProgress", data.WaveInProgress, false);
            AppendInt(builder, "LivesAtWaveStart", data.LivesAtWaveStart, false);
            AppendInt(builder, "Mk2Mask", data.Mk2Mask, false);
            AppendInt(builder, "BankedLegacy", data.BankedLegacy, false);
            AppendInt(builder, "ExtraLifeWorld", data.ExtraLifeWorld, false);
            builder.Append(",\"Timestamp\":\"");
            builder.Append(Escape(data.Timestamp));
            builder.Append("\"}");
            return builder.ToString();
        }

        public static bool TryParse(string json, out RunSaveData data)
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

            RunSaveData parsed = new RunSaveData();
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
                if (cursor >= end)
                {
                    return false;
                }

                if (key == "Timestamp")
                {
                    string stamp;
                    if (!ReadString(trimmed, ref cursor, out stamp))
                    {
                        return false;
                    }

                    parsed.Timestamp = stamp;
                }
                else
                {
                    int number;
                    if (!ReadInt(trimmed, ref cursor, out number))
                    {
                        return false;
                    }

                    if (!Assign(parsed, key, number))
                    {
                        return false;
                    }

                    if (key == "Version")
                    {
                        sawVersion = true;
                    }
                }
            }

            if (!sawVersion)
            {
                return false;
            }

            if (parsed.Version == 1)
            {
                parsed.BoonLevels = 0;
                parsed.BoonPending = 0;
                parsed.BoonOffer = 0;
            }

            if (parsed.Version < 3)
            {
                parsed.WaveInProgress = 0;
                parsed.LivesAtWaveStart = 0;
                parsed.Mk2Mask = 0;
                parsed.BankedLegacy = 0;
                parsed.ExtraLifeWorld = 0;
            }

            if (!IsValid(parsed))
            {
                return false;
            }

            data = parsed;
            return true;
        }

        private static int PutBit(int mask, int bit, bool on)
        {
            if (!on)
            {
                return mask;
            }

            return mask | (1 << bit);
        }

        private static bool Assign(RunSaveData data, string key, int number)
        {
            switch (key)
            {
                case "Version":
                    data.Version = number;
                    return true;
                case "WaveIndex":
                    data.WaveIndex = number;
                    return true;
                case "Score":
                    data.Score = number;
                    return true;
                case "Credits":
                    data.Credits = number;
                    return true;
                case "Lives":
                    data.Lives = number;
                    return true;
                case "Hull":
                    data.Hull = number;
                    return true;
                case "Shield":
                    data.Shield = number;
                    return true;
                case "Difficulty":
                    data.Difficulty = number;
                    return true;
                case "UpgradeMask":
                    data.UpgradeMask = number;
                    return true;
                case "Doctrine":
                    data.Doctrine = number;
                    return true;
                case "PrimaryMode":
                    data.PrimaryMode = number;
                    return true;
                case "UtilityMode":
                    data.UtilityMode = number;
                    return true;
                case "HasUtility":
                    data.HasUtility = number;
                    return true;
                case "RunId":
                    data.RunId = number;
                    return true;
                case "LastResolvedWave":
                    data.LastResolvedWave = number;
                    return true;
                case "LastRunScore":
                    data.LastRunScore = number;
                    return true;
                case "LastCreditsAwarded":
                    data.LastCreditsAwarded = number;
                    return true;
                case "ExtraLifeStreak":
                    data.ExtraLifeStreak = number;
                    return true;
                case "LegacyHull":
                    data.LegacyHull = number;
                    return true;
                case "FirstDiscount":
                    data.FirstDiscount = number;
                    return true;
                case "FirstDiscountUsed":
                    data.FirstDiscountUsed = number;
                    return true;
                case "BoonLevels":
                    data.BoonLevels = number;
                    return true;
                case "BoonPending":
                    data.BoonPending = number;
                    return true;
                case "BoonOffer":
                    data.BoonOffer = number;
                    return true;
                case "WaveInProgress":
                    data.WaveInProgress = number;
                    return true;
                case "LivesAtWaveStart":
                    data.LivesAtWaveStart = number;
                    return true;
                case "Mk2Mask":
                    data.Mk2Mask = number;
                    return true;
                case "BankedLegacy":
                    data.BankedLegacy = number;
                    return true;
                case "ExtraLifeWorld":
                    data.ExtraLifeWorld = number;
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

        private static string Escape(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
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
                    if (cursor >= text.Length)
                    {
                        return false;
                    }

                    char escaped = text[cursor];
                    cursor += 1;
                    if (escaped == '"' || escaped == '\\')
                    {
                        builder.Append(escaped);
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    builder.Append(ch);
                }
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
