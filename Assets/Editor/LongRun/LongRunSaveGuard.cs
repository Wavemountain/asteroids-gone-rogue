using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace AsteroidsGoneRogue.EditorTools
{
    /// <summary>
    /// Backup, hash, and restore check for the long-run profile. File saves are
    /// redirected with AGR_LONGRUN_PROFILE. This type never writes the live
    /// save files; it only reads them and copies into the backup folder.
    /// Mirrors Tools/longrun_policy.py.
    /// </summary>
    public static class LongRunSaveGuard
    {
        public const string Absent = "ABSENT";
        public const string EnvProfile = "AGR_LONGRUN_PROFILE";

        public static readonly string[] SaveNames = new string[]
        {
            "run_save.json",
            "meta_save.json",
            "run_save.json.bak",
            "meta_save.json.bak",
            "run_save.json.tmp",
            "meta_save.json.tmp",
        };

        public static bool ProfileIsIsolated(string liveDir, string profileDir)
        {
            if (string.IsNullOrEmpty(liveDir) || string.IsNullOrEmpty(profileDir))
            {
                return false;
            }

            string live = Norm(liveDir);
            string profile = Norm(profileDir);
            if (string.Equals(live, profile, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string livePrefix = live + Path.DirectorySeparatorChar;
            string profilePrefix = profile + Path.DirectorySeparatorChar;
            if (profile.StartsWith(livePrefix, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (live.StartsWith(profilePrefix, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return true;
        }

        public static string HashFile(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return Absent;
            }

            byte[] bytes = File.ReadAllBytes(path);
            return HashBytes(bytes);
        }

        public static string HashBytes(byte[] bytes)
        {
            if (bytes == null)
            {
                return Absent;
            }

            using (SHA256 sha = SHA256.Create())
            {
                byte[] hash = sha.ComputeHash(bytes);
                return ToHex(hash);
            }
        }

        public static bool BackupVerified(string sourceHash, string backupHash)
        {
            if (string.IsNullOrEmpty(sourceHash) || string.IsNullOrEmpty(backupHash))
            {
                return false;
            }

            return string.Equals(sourceHash, backupHash, StringComparison.OrdinalIgnoreCase);
        }

        public static string CopyAndVerify(string sourcePath, string backupPath)
        {
            string sourceHash = HashFile(sourcePath);
            if (sourceHash == Absent)
            {
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }
            }
            else
            {
                string parent = Path.GetDirectoryName(backupPath);
                if (!string.IsNullOrEmpty(parent))
                {
                    Directory.CreateDirectory(parent);
                }

                File.Copy(sourcePath, backupPath, true);
            }

            string backupHash = HashFile(backupPath);
            if (!BackupVerified(sourceHash, backupHash))
            {
                throw new IOException("backup hash mismatch for " + sourcePath);
            }

            return sourceHash;
        }

        public static bool LiveUntouched(string livePath, string originalHash)
        {
            return BackupVerified(originalHash, HashFile(livePath));
        }

        public static void WriteManifest(string manifestPath, string[] names, string[] hashes)
        {
            StringBuilder builder = new StringBuilder();
            int count = names == null ? 0 : names.Length;
            for (int index = 0; index < count; index++)
            {
                string hash = Absent;
                if (hashes != null && index < hashes.Length && !string.IsNullOrEmpty(hashes[index]))
                {
                    hash = hashes[index];
                }

                builder.Append(names[index]);
                builder.Append('\t');
                builder.Append(hash);
                builder.Append('\n');
            }

            string parent = Path.GetDirectoryName(manifestPath);
            if (!string.IsNullOrEmpty(parent))
            {
                Directory.CreateDirectory(parent);
            }

            File.WriteAllText(manifestPath, builder.ToString());
            string reread = File.ReadAllText(manifestPath);
            if (!string.Equals(reread, builder.ToString(), StringComparison.Ordinal))
            {
                throw new IOException("manifest reread mismatch");
            }
        }

        public static bool ManifestMatchesLive(string manifestPath, string liveDir, out string detail)
        {
            detail = string.Empty;
            if (!File.Exists(manifestPath))
            {
                detail = "manifest missing";
                return false;
            }

            string[] lines = File.ReadAllLines(manifestPath);
            bool ok = true;
            StringBuilder problems = new StringBuilder();
            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                string line = lines[lineIndex];
                if (line.Length == 0)
                {
                    continue;
                }

                int split = line.IndexOf('\t');
                if (split <= 0)
                {
                    continue;
                }

                string name = line.Substring(0, split);
                string expected = line.Substring(split + 1);
                string livePath = Path.Combine(liveDir, name);
                string actual = HashFile(livePath);
                if (!BackupVerified(expected, actual))
                {
                    ok = false;
                    if (problems.Length > 0)
                    {
                        problems.Append("; ");
                    }

                    problems.Append(name);
                    problems.Append(" expected ");
                    problems.Append(expected);
                    problems.Append(" actual ");
                    problems.Append(actual);
                }
            }

            detail = problems.ToString();
            return ok;
        }

        public static string SnapshotPrefs(string path)
        {
            StringBuilder builder = new StringBuilder();
            string[] rows = PrefRows();
            for (int rowIndex = 0; rowIndex < rows.Length; rowIndex++)
            {
                string[] parts = rows[rowIndex].Split('\t');
                string kind = parts[0];
                string key = parts[1];
                if (!PlayerPrefs.HasKey(key))
                {
                    builder.Append("M\t");
                    builder.Append(key);
                    builder.Append('\n');
                    continue;
                }

                if (kind == "I")
                {
                    builder.Append("I\t");
                    builder.Append(key);
                    builder.Append('\t');
                    builder.Append(PlayerPrefs.GetInt(key).ToString(CultureInfo.InvariantCulture));
                    builder.Append('\n');
                }
                else if (kind == "F")
                {
                    builder.Append("F\t");
                    builder.Append(key);
                    builder.Append('\t');
                    builder.Append(PlayerPrefs.GetFloat(key).ToString("R", CultureInfo.InvariantCulture));
                    builder.Append('\n');
                }
                else
                {
                    string raw = PlayerPrefs.GetString(key, string.Empty);
                    string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
                    builder.Append("S\t");
                    builder.Append(key);
                    builder.Append('\t');
                    builder.Append(encoded);
                    builder.Append('\n');
                }
            }

            string parent = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(parent))
            {
                Directory.CreateDirectory(parent);
            }

            string text = builder.ToString();
            File.WriteAllText(path, text);
            string reread = File.ReadAllText(path);
            if (!string.Equals(text, reread, StringComparison.Ordinal))
            {
                throw new IOException("prefs snapshot reread mismatch");
            }

            return HashBytes(Encoding.UTF8.GetBytes(reread));
        }

        public static void RestorePrefs(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("prefs snapshot missing", path);
            }

            string[] lines = File.ReadAllLines(path);
            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                string line = lines[lineIndex];
                if (line.Length == 0)
                {
                    continue;
                }

                string[] parts = line.Split('\t');
                if (parts.Length < 2)
                {
                    continue;
                }

                string kind = parts[0];
                string key = parts[1];
                if (kind == "M")
                {
                    if (PlayerPrefs.HasKey(key))
                    {
                        PlayerPrefs.DeleteKey(key);
                    }

                    continue;
                }

                if (parts.Length < 3)
                {
                    continue;
                }

                string payload = parts[2];
                if (kind == "I")
                {
                    int number;
                    if (int.TryParse(payload, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
                    {
                        PlayerPrefs.SetInt(key, number);
                    }
                }
                else if (kind == "F")
                {
                    float number;
                    if (float.TryParse(payload, NumberStyles.Float, CultureInfo.InvariantCulture, out number))
                    {
                        PlayerPrefs.SetFloat(key, number);
                    }
                }
                else if (kind == "S")
                {
                    byte[] bytes = Convert.FromBase64String(payload);
                    PlayerPrefs.SetString(key, Encoding.UTF8.GetString(bytes));
                }
            }

            PlayerPrefs.Save();
        }

        static string Norm(string path)
        {
            string full = Path.GetFullPath(path);
            char sep = Path.DirectorySeparatorChar;
            char alt = Path.AltDirectorySeparatorChar;
            while (full.Length > 1 && (full[full.Length - 1] == sep || full[full.Length - 1] == alt))
            {
                full = full.Substring(0, full.Length - 1);
            }

            return full;
        }

        static string ToHex(byte[] hash)
        {
            StringBuilder builder = new StringBuilder(hash.Length * 2);
            for (int index = 0; index < hash.Length; index++)
            {
                builder.Append(hash[index].ToString("x2", CultureInfo.InvariantCulture));
            }

            return builder.ToString();
        }

        static string[] PrefRows()
        {
            return new string[]
            {
                "I\tagr.settings.version",
                "I\tagr.settings.screenShake",
                "I\tagr.settings.hintMode",
                "I\tagr.settings.hintSizeStep",
                "I\tagr.settings.confirmRestartInPlay",
                "I\tagr.settings.confirmRestartNewRun",
                "I\tagr.settings.padNavSource",
                "I\tagr.settings.assistMode",
                "I\tagr.settings.reduceEffects",
                "I\tagr.settings.windowMode",
                "I\tagr.settings.resolutionW",
                "I\tagr.settings.resolutionH",
                "I\tagr.settings.vSync",
                "I\tagr.settings.fpsCap",
                "I\tagr.settings.promptScheme",
                "I\tagr.settings.tutorialCards",
                "I\tagr.settings.filmGrain",
                "I\tagr.ui.firstHangarHint",
                "I\tagr.ui.doctrineHint",
                "I\tagr.audio.mute",
                "I\tagr.audio.curve",
                "I\tagr.daily.played",
                "I\tagr.achievements.mask",
                "I\tagr.hangar.medals",
                "I\tagr.best.score",
                "I\tagr.best.wave",
                "I\tagr.best.world",
                "I\tagr.wave.bestCombo",
                "F\tagr.audio.sfx",
                "F\tagr.audio.music",
                "F\tagr.settings.turnSpeed",
                "F\tagr.wave.bestSeconds",
                "S\tagr.settings.bindings",
                "S\tagr.ui.language",
                "S\tagr.sink.profile",
                "S\tagr.daily.board",
                "S\tagr.difficulty",
            };
        }
    }
}
