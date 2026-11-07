using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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

            return File.ReadAllText(marker).Trim();
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

            if (floatStable && !intStable)
            {
                type = "float";
                value = floatValue.ToString("R", CultureInfo.InvariantCulture);
                return;
            }

            if (intStable && !floatStable)
            {
                type = "int";
                value = intValue.ToString(CultureInfo.InvariantCulture);
                return;
            }

            if (stringStable && !intStable && !floatStable)
            {
                type = "string";
                value = stringValue ?? string.Empty;
                return;
            }

            if (floatStable && intStable)
            {
                bool integral = !float.IsNaN(floatValue)
                    && !float.IsInfinity(floatValue)
                    && floatValue == (float)Math.Round(floatValue);
                bool fraction = FractionText(stringValue);
                if (integral && (int)Math.Round(floatValue) == intValue && !fraction)
                {
                    type = "int";
                    value = intValue.ToString(CultureInfo.InvariantCulture);
                    return;
                }

                type = "float";
                value = floatValue.ToString("R", CultureInfo.InvariantCulture);
                return;
            }

            type = "string";
            value = stringStable && stringValue != null ? stringValue : string.Empty;
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

            List<PrefEntry> saved = Parse(File.ReadAllText(backupPath));
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
            builder.Append("[\n");
            for (int index = 0; index < entries.Length; index++)
            {
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
                if (index + 1 < entries.Length)
                {
                    builder.Append(',');
                }

                builder.Append('\n');
            }

            builder.Append("]\n");
            return builder.ToString();
        }

        static List<PrefEntry> Parse(string json)
        {
            List<PrefEntry> list = new List<PrefEntry>();
            if (string.IsNullOrEmpty(json))
            {
                return list;
            }

            string[] lines = json.Split('\n');
            for (int index = 0; index < lines.Length; index++)
            {
                string line = lines[index].Trim().TrimEnd(',');
                if (!line.StartsWith("{", StringComparison.Ordinal))
                {
                    continue;
                }

                PrefEntry entry;
                if (TryObject(line, out entry))
                {
                    list.Add(entry);
                }
            }

            return list;
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

            entry.Name = name;
            entry.Type = type;
            entry.Value = value;
            entry.Exists = line.IndexOf("\"exists\":true", StringComparison.Ordinal) >= 0;
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

        static bool FractionText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            return text.IndexOf('.') >= 0 || text.IndexOf('e') >= 0 || text.IndexOf('E') >= 0;
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
