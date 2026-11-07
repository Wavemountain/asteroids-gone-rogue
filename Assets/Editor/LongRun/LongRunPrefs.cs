using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace AsteroidsGoneRogue.EditorTools
{
    /// <summary>
    /// Durable PlayerPrefs backup. Key names are watched; the stored type is
    /// detected from the value. Mirrors classify_pref / verify_pref in
    /// Tools/longrun_policy.py.
    /// </summary>
    public static class LongRunPrefs
    {
        public const string MarkerName = "prefs-unrestored.txt";

        public static string MarkerPath()
        {
            return Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "Captures", "LongRun", MarkerName));
        }

        public static string UnrestoredBackup()
        {
            string marker = MarkerPath();
            if (!File.Exists(marker))
            {
                return string.Empty;
            }

            string recorded = File.ReadAllText(marker).Trim();
            if (recorded.Length == 0)
            {
                return marker;
            }

            return recorded;
        }

        public static void Classify(
            bool exists,
            bool intStable,
            int intValue,
            bool floatStable,
            float floatValue,
            bool stringStable,
            string stringValue,
            out string type,
            out string value)
        {
            type = string.Empty;
            value = string.Empty;
            if (!exists)
            {
                return;
            }

            int stableCount = 0;
            if (intStable)
            {
                stableCount += 1;
            }

            if (floatStable)
            {
                stableCount += 1;
            }

            if (stringStable)
            {
                stableCount += 1;
            }

            if (stableCount != 1)
            {
                type = "ambiguous";
                return;
            }

            if (intStable)
            {
                type = "int";
                value = intValue.ToString(CultureInfo.InvariantCulture);
                return;
            }

            if (floatStable)
            {
                type = "float";
                value = floatValue.ToString("R", CultureInfo.InvariantCulture);
                return;
            }

            type = "string";
            value = stringValue ?? string.Empty;
        }

        public static string RestoreAction(bool exists, string kind)
        {
            if (!exists)
            {
                return "delete";
            }

            return "set-" + (kind ?? string.Empty);
        }

        public static string Verify(
            bool exists,
            string kind,
            string stored,
            bool nowExists,
            string nowKind,
            string nowValue)
        {
            if (!exists)
            {
                return nowExists ? "MISMATCH" : "MATCH";
            }

            if (nowExists && nowKind == kind && nowValue == stored)
            {
                return "MATCH";
            }

            return "MISMATCH";
        }

        public static void Capture(string backupJson)
        {
            string[] names = KeyNames();
            PrefEntry[] entries = new PrefEntry[names.Length];
            for (int index = 0; index < names.Length; index++)
            {
                entries[index] = ReadKey(names[index]);
                if (entries[index].Type == "ambiguous")
                {
                    throw new IOException("ambiguous PlayerPrefs type for " + names[index]);
                }
            }

            WriteDurable(backupJson, ToJson(entries));
            string reread = File.ReadAllText(backupJson);
            if (!string.Equals(reread, ToJson(entries), StringComparison.Ordinal))
            {
                throw new IOException("prefs backup reread mismatch");
            }

            WriteDurable(MarkerPath(), Path.GetFullPath(backupJson) + "\n");
        }

        public static bool RestoreAndVerify(string backupPath, out string reportPath)
        {
            reportPath = Path.Combine(Path.GetDirectoryName(backupPath) ?? string.Empty, "prefs-verify.txt");
            if (!File.Exists(backupPath))
            {
                WriteDurable(reportPath, "RESULT MISMATCH\nbackup missing\n");
                return false;
            }

            List<PrefEntry> saved;
            string rejected = RejectReason(File.ReadAllText(backupPath), out saved);
            if (rejected.Length > 0)
            {
                WriteDurable(reportPath, "RESULT MISMATCH\n" + rejected + "\n");
                return false;
            }

            for (int index = 0; index < saved.Count; index++)
            {
                Apply(saved[index]);
            }

            PlayerPrefs.Save();
            StringBuilder report = new StringBuilder();
            bool allMatch = true;
            for (int index = 0; index < saved.Count; index++)
            {
                PrefEntry prior = saved[index];
                PrefEntry now = ReadKey(prior.Name);
                string status = Verify(prior.Exists, prior.Type, prior.Value, now.Exists, now.Type, now.Value);
                if (status != "MATCH")
                {
                    allMatch = false;
                }

                report.Append(prior.Name);
                report.Append(' ');
                report.Append(status);
                report.Append('\n');
            }

            report.Append("RESULT ");
            report.Append(allMatch ? "MATCH" : "MISMATCH");
            report.Append('\n');
            WriteDurable(reportPath, report.ToString());
            if (allMatch)
            {
                ClearMarker(backupPath);
            }

            return allMatch;
        }

        static void Apply(PrefEntry entry)
        {
            if (!entry.Exists || entry.Type.Length == 0)
            {
                PlayerPrefs.DeleteKey(entry.Name);
                return;
            }

            if (entry.Type == "int")
            {
                int number;
                if (int.TryParse(entry.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
                {
                    PlayerPrefs.SetInt(entry.Name, number);
                }

                return;
            }

            if (entry.Type == "float")
            {
                float number;
                if (float.TryParse(entry.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out number))
                {
                    PlayerPrefs.SetFloat(entry.Name, number);
                }

                return;
            }

            PlayerPrefs.SetString(entry.Name, entry.Value ?? string.Empty);
        }

        static PrefEntry ReadKey(string name)
        {
            PrefEntry entry = new PrefEntry();
            entry.Name = name;
            entry.Exists = PlayerPrefs.HasKey(name);
            entry.Type = string.Empty;
            entry.Value = string.Empty;
            if (!entry.Exists)
            {
                return entry;
            }

            int intA = PlayerPrefs.GetInt(name, 111000111);
            int intB = PlayerPrefs.GetInt(name, 222000222);
            bool intStable = intA == intB;
            float floatA = PlayerPrefs.GetFloat(name, 1.25e-20f);
            float floatB = PlayerPrefs.GetFloat(name, 2.5e-20f);
            bool floatStable = floatA == floatB;
            string stringA = PlayerPrefs.GetString(name, "\u0001lrA");
            string stringB = PlayerPrefs.GetString(name, "\u0001lrB");
            bool stringStable = stringA == stringB;
            string detected;
            string stored;
            Classify(
                true,
                intStable,
                intA,
                floatStable,
                floatA,
                stringStable,
                stringA,
                out detected,
                out stored);
            entry.Type = detected;
            entry.Value = stored;
            return entry;
        }

        static void ClearMarker(string backupPath)
        {
            string marker = MarkerPath();
            if (!File.Exists(marker))
            {
                return;
            }

            string recorded = File.ReadAllText(marker).Trim();
            if (!string.Equals(Path.GetFullPath(recorded), Path.GetFullPath(backupPath), StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            File.Delete(marker);
        }

        static void WriteDurable(string path, string text)
        {
            string parent = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(parent))
            {
                Directory.CreateDirectory(parent);
            }

            string temp = path + ".tmp";
            byte[] bytes = Encoding.UTF8.GetBytes(text ?? string.Empty);
            using (FileStream stream = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            File.Move(temp, path);
            using (FileStream committed = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                committed.Flush(true);
            }
        }

        static string ToJson(PrefEntry[] entries)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("[\n{\"count\":");
            builder.Append(entries.Length.ToString(CultureInfo.InvariantCulture));
            builder.Append(",\"checksum\":\"");
            builder.Append(Checksum(entries));
            builder.Append("\"}");
            for (int index = 0; index < entries.Length; index++)
            {
                builder.Append(",\n");
                PrefEntry entry = entries[index];
                builder.Append("{\"name\":\"");
                builder.Append(Escape(entry.Name));
                builder.Append("\",\"exists\":");
                builder.Append(entry.Exists ? "true" : "false");
                builder.Append(",\"type\":\"");
                builder.Append(Escape(entry.Type));
                builder.Append("\",\"value\":\"");
                builder.Append(Escape(entry.Value));
                builder.Append("\"}");
            }

            builder.Append("\n]\n");
            return builder.ToString();
        }

        static string Checksum(PrefEntry[] entries)
        {
            StringBuilder builder = new StringBuilder();
            for (int index = 0; index < entries.Length; index++)
            {
                AppendCanonical(builder, entries[index]);
            }

            return Sha256(builder.ToString());
        }

        static void AppendCanonical(StringBuilder builder, PrefEntry entry)
        {
            builder.Append(entry.Name ?? string.Empty);
            builder.Append('\t');
            builder.Append(entry.Exists ? "1" : "0");
            builder.Append('\t');
            builder.Append(entry.Type ?? string.Empty);
            builder.Append('\t');
            builder.Append(entry.Value ?? string.Empty);
            builder.Append('\n');
        }

        static string Sha256(string text)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(text ?? string.Empty));
                StringBuilder hex = new StringBuilder(hash.Length * 2);
                for (int index = 0; index < hash.Length; index++)
                {
                    hex.Append(hash[index].ToString("x2", CultureInfo.InvariantCulture));
                }

                return hex.ToString();
            }
        }

        static string SyntaxError(string json)
        {
            int index = 0;
            string error = SkipValue(json, ref index);
            if (error.Length > 0)
            {
                return error;
            }

            while (index < json.Length && char.IsWhiteSpace(json[index]))
            {
                index += 1;
            }

            if (index != json.Length)
            {
                return "truncated";
            }

            return string.Empty;
        }

        static string SkipValue(string json, ref int index)
        {
            SkipSpace(json, ref index);
            if (index >= json.Length)
            {
                return "truncated";
            }

            char ch = json[index];
            if (ch == '{')
            {
                return SkipObject(json, ref index);
            }

            if (ch == '[')
            {
                return SkipArray(json, ref index);
            }

            if (ch == '"')
            {
                return SkipString(json, ref index);
            }

            if (ch == 't')
            {
                return SkipLiteral(json, ref index, "true");
            }

            if (ch == 'f')
            {
                return SkipLiteral(json, ref index, "false");
            }

            if (ch == 'n')
            {
                return SkipLiteral(json, ref index, "null");
            }

            if (ch == '-' || (ch >= '0' && ch <= '9'))
            {
                return SkipNumber(json, ref index);
            }

            return "truncated";
        }

        static string SkipObject(string json, ref int index)
        {
            index += 1;
            SkipSpace(json, ref index);
            if (index < json.Length && json[index] == '}')
            {
                index += 1;
                return string.Empty;
            }

            while (index < json.Length)
            {
                string keyError = SkipString(json, ref index);
                if (keyError.Length > 0)
                {
                    return keyError;
                }

                SkipSpace(json, ref index);
                if (index >= json.Length || json[index] != ':')
                {
                    return "truncated";
                }

                index += 1;
                string valueError = SkipValue(json, ref index);
                if (valueError.Length > 0)
                {
                    return valueError;
                }

                SkipSpace(json, ref index);
                if (index >= json.Length)
                {
                    return "truncated";
                }

                if (json[index] == '}')
                {
                    index += 1;
                    return string.Empty;
                }

                if (json[index] != ',')
                {
                    return "truncated";
                }

                index += 1;
                SkipSpace(json, ref index);
            }

            return "truncated";
        }

        static string SkipArray(string json, ref int index)
        {
            index += 1;
            SkipSpace(json, ref index);
            if (index < json.Length && json[index] == ']')
            {
                index += 1;
                return string.Empty;
            }

            while (index < json.Length)
            {
                string valueError = SkipValue(json, ref index);
                if (valueError.Length > 0)
                {
                    return valueError;
                }

                SkipSpace(json, ref index);
                if (index >= json.Length)
                {
                    return "truncated";
                }

                if (json[index] == ']')
                {
                    index += 1;
                    return string.Empty;
                }

                if (json[index] != ',')
                {
                    return "truncated";
                }

                index += 1;
                SkipSpace(json, ref index);
            }

            return "truncated";
        }

        static string SkipString(string json, ref int index)
        {
            SkipSpace(json, ref index);
            if (index >= json.Length || json[index] != '"')
            {
                return "truncated";
            }

            index += 1;
            while (index < json.Length)
            {
                char ch = json[index];
                if (ch == '"')
                {
                    index += 1;
                    return string.Empty;
                }

                if (ch == '\\')
                {
                    if (index + 1 >= json.Length)
                    {
                        return "truncated";
                    }

                    char escaped = json[index + 1];
                    if (escaped == 'u')
                    {
                        if (index + 5 >= json.Length)
                        {
                            return "truncated";
                        }

                        index += 6;
                        continue;
                    }

                    index += 2;
                    continue;
                }

                index += 1;
            }

            return "truncated";
        }

        static string SkipLiteral(string json, ref int index, string literal)
        {
            if (index + literal.Length > json.Length)
            {
                return "truncated";
            }

            if (json.Substring(index, literal.Length) != literal)
            {
                return "truncated";
            }

            index += literal.Length;
            return string.Empty;
        }

        static string SkipNumber(string json, ref int index)
        {
            if (json[index] == '-')
            {
                index += 1;
            }

            if (index >= json.Length || json[index] < '0' || json[index] > '9')
            {
                return "truncated";
            }

            if (json[index] == '0')
            {
                index += 1;
            }
            else
            {
                while (index < json.Length && json[index] >= '0' && json[index] <= '9')
                {
                    index += 1;
                }
            }

            if (index < json.Length && json[index] == '.')
            {
                index += 1;
                if (index >= json.Length || json[index] < '0' || json[index] > '9')
                {
                    return "truncated";
                }

                while (index < json.Length && json[index] >= '0' && json[index] <= '9')
                {
                    index += 1;
                }
            }

            if (index < json.Length && (json[index] == 'e' || json[index] == 'E'))
            {
                index += 1;
                if (index < json.Length && (json[index] == '+' || json[index] == '-'))
                {
                    index += 1;
                }

                if (index >= json.Length || json[index] < '0' || json[index] > '9')
                {
                    return "truncated";
                }

                while (index < json.Length && json[index] >= '0' && json[index] <= '9')
                {
                    index += 1;
                }
            }

            return string.Empty;
        }

        static void SkipSpace(string json, ref int index)
        {
            while (index < json.Length && char.IsWhiteSpace(json[index]))
            {
                index += 1;
            }
        }

        static string RejectReason(string json, out List<PrefEntry> saved)
        {
            saved = new List<PrefEntry>();
            if (string.IsNullOrWhiteSpace(json))
            {
                return "empty";
            }

            string syntax = SyntaxError(json);
            if (syntax.Length > 0)
            {
                return syntax;
            }

            bool sawHeader = false;
            int declared = -1;
            string declaredSum = string.Empty;
            string[] lines = json.Split('\n');
            for (int index = 0; index < lines.Length; index++)
            {
                string line = lines[index].Trim().TrimEnd(',');
                if (line.Length == 0 || line == "[" || line == "]")
                {
                    continue;
                }

                if (!line.StartsWith("{", StringComparison.Ordinal) || !line.EndsWith("}", StringComparison.Ordinal))
                {
                    return "truncated";
                }

                int parsedCount;
                string parsedSum;
                if (TryHeader(line, out parsedCount, out parsedSum))
                {
                    if (sawHeader)
                    {
                        return "truncated";
                    }

                    sawHeader = true;
                    declared = parsedCount;
                    declaredSum = parsedSum;
                    continue;
                }

                PrefEntry entry;
                if (!TryObject(line, out entry))
                {
                    return "truncated";
                }

                saved.Add(entry);
            }

            if (!sawHeader || declaredSum.Length == 0)
            {
                return "empty";
            }

            string[] required = KeyNames();
            for (int index = 0; index < saved.Count; index++)
            {
                string name = saved[index].Name;
                for (int other = index + 1; other < saved.Count; other++)
                {
                    if (saved[other].Name == name)
                    {
                        return "duplicated";
                    }
                }
            }

            for (int index = 0; index < required.Length; index++)
            {
                bool found = false;
                for (int entryIndex = 0; entryIndex < saved.Count; entryIndex++)
                {
                    if (saved[entryIndex].Name == required[index])
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    return "missing key";
                }
            }

            if (declared != required.Length || saved.Count != required.Length)
            {
                return "count";
            }

            for (int index = 0; index < saved.Count; index++)
            {
                string typeError = TypeError(saved[index]);
                if (typeError.Length > 0)
                {
                    return typeError;
                }
            }

            PrefEntry[] ordered = new PrefEntry[required.Length];
            for (int index = 0; index < required.Length; index++)
            {
                for (int entryIndex = 0; entryIndex < saved.Count; entryIndex++)
                {
                    if (saved[entryIndex].Name == required[index])
                    {
                        ordered[index] = saved[entryIndex];
                        break;
                    }
                }
            }

            if (!string.Equals(Checksum(ordered), declaredSum, StringComparison.OrdinalIgnoreCase))
            {
                return "checksum";
            }

            return string.Empty;
        }

        static string TypeError(PrefEntry entry)
        {
            if (!entry.Exists)
            {
                if (entry.Type.Length != 0 || entry.Value.Length != 0)
                {
                    return "invalid type";
                }

                return string.Empty;
            }

            if (entry.Type == "ambiguous" || entry.Type.Length == 0)
            {
                return "ambiguous";
            }

            if (entry.Type == "int")
            {
                return IntExact(entry.Value) ? string.Empty : "invalid type";
            }

            if (entry.Type == "float")
            {
                return FloatExact(entry.Value) ? string.Empty : "invalid type";
            }

            if (entry.Type == "string")
            {
                return string.Empty;
            }

            return "invalid type";
        }

        static bool IntExact(string text)
        {
            int number;
            if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
            {
                return false;
            }

            return number.ToString(CultureInfo.InvariantCulture) == text;
        }

        static bool FloatExact(string text)
        {
            float number;
            if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out number))
            {
                return false;
            }

            return number.ToString("R", CultureInfo.InvariantCulture) == text;
        }

        static bool TryHeader(string line, out int count, out string checksum)
        {
            count = -1;
            checksum = string.Empty;
            if (line.IndexOf("\"name\"", StringComparison.Ordinal) >= 0)
            {
                return false;
            }

            int marker = line.IndexOf("\"count\":", StringComparison.Ordinal);
            if (marker < 0)
            {
                return false;
            }

            int cursor = marker + "\"count\":".Length;
            int end = cursor;
            while (end < line.Length && line[end] >= '0' && line[end] <= '9')
            {
                end += 1;
            }

            if (end == cursor)
            {
                return false;
            }

            if (!int.TryParse(line.Substring(cursor, end - cursor), NumberStyles.Integer, CultureInfo.InvariantCulture, out count))
            {
                return false;
            }

            return TryField(line, "checksum", out checksum) && checksum.Length > 0;
        }

        static bool TryObject(string line, out PrefEntry entry)
        {
            entry = new PrefEntry();
            string name;
            string type;
            string value;
            if (!TryField(line, "name", out name) || !TryField(line, "type", out type) || !TryField(line, "value", out value))
            {
                return false;
            }

            bool existsTrue = line.IndexOf("\"exists\":true", StringComparison.Ordinal) >= 0;
            bool existsFalse = line.IndexOf("\"exists\":false", StringComparison.Ordinal) >= 0;
            if (existsTrue == existsFalse)
            {
                return false;
            }

            entry.Name = name;
            entry.Type = type;
            entry.Value = value;
            entry.Exists = existsTrue;
            return name.Length > 0;
        }

        static bool TryField(string line, string key, out string value)
        {
            value = string.Empty;
            string token = "\"" + key + "\":\"";
            int start = line.IndexOf(token, StringComparison.Ordinal);
            if (start < 0)
            {
                return false;
            }

            int cursor = start + token.Length;
            StringBuilder builder = new StringBuilder();
            while (cursor < line.Length)
            {
                char ch = line[cursor];
                if (ch == '"')
                {
                    value = builder.ToString();
                    return true;
                }

                if (ch == '\\' && cursor + 1 < line.Length)
                {
                    char escaped = line[cursor + 1];
                    if (escaped == 'n')
                    {
                        builder.Append('\n');
                    }
                    else if (escaped == 'r')
                    {
                        builder.Append('\r');
                    }
                    else
                    {
                        builder.Append(escaped);
                    }

                    cursor += 2;
                    continue;
                }

                builder.Append(ch);
                cursor += 1;
            }

            return false;
        }

        static string Escape(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder();
            for (int index = 0; index < text.Length; index++)
            {
                char ch = text[index];
                if (ch == '\\' || ch == '"')
                {
                    builder.Append('\\');
                    builder.Append(ch);
                }
                else if (ch == '\n')
                {
                    builder.Append("\\n");
                }
                else if (ch == '\r')
                {
                    builder.Append("\\r");
                }
                else
                {
                    builder.Append(ch);
                }
            }

            return builder.ToString();
        }

        static string[] KeyNames()
        {
            return new string[]
            {
                "agr.achievements.mask",
                "agr.audio.curve",
                "agr.audio.music",
                "agr.audio.mute",
                "agr.audio.sfx",
                "agr.best.score",
                "agr.best.wave",
                "agr.best.world",
                "agr.daily.board",
                "agr.daily.played",
                "agr.difficulty",
                "agr.hangar.medals",
                "agr.settings.assistMode",
                "agr.settings.bindings",
                "agr.settings.confirmRestartInPlay",
                "agr.settings.confirmRestartNewRun",
                "agr.settings.filmGrain",
                "agr.settings.fpsCap",
                "agr.settings.hintMode",
                "agr.settings.hintSizeStep",
                "agr.settings.padNavSource",
                "agr.settings.promptScheme",
                "agr.settings.reduceEffects",
                "agr.settings.resolutionH",
                "agr.settings.resolutionW",
                "agr.settings.screenShake",
                "agr.settings.tutorialCards",
                "agr.settings.turnSpeed",
                "agr.settings.version",
                "agr.settings.vSync",
                "agr.settings.windowMode",
                "agr.sink.profile",
                "agr.ui.doctrineHint",
                "agr.ui.firstHangarHint",
                "agr.ui.language",
                "agr.wave.bestCombo",
                "agr.wave.bestSeconds",
            };
        }

        struct PrefEntry
        {
            public string Name;
            public bool Exists;
            public string Type;
            public string Value;
        }
    }
}
