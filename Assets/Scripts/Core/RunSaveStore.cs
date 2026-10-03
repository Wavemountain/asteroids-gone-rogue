using System;
using System.IO;
using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Thin file wrapper. JSON is written with JsonUtility, checked by the pure
    /// codec, and swapped in only after the temp file parses. A bad candidate
    /// leaves the previous file in place.
    /// </summary>
    public static class RunSaveStore
    {
        public const string RunFileName = "run_save.json";
        public const string MetaFileName = "meta_save.json";
        public const string TempSuffix = ".tmp";

        public static bool TryLoadRun(out RunSaveData data)
        {
            data = null;
            try
            {
                string path = Path.Combine(Application.persistentDataPath, RunFileName);
                string backupPath = path + SaveFileChoice.BackupSuffix;
                bool mainExists = File.Exists(path);
                string mainText = mainExists ? File.ReadAllText(path) : null;
                bool backupExists = File.Exists(backupPath);
                string backupText = backupExists ? File.ReadAllText(backupPath) : null;
                string chosen = SaveFileChoice.Choose(mainText, mainExists, backupText, backupExists, RunSaveCodec.CommitAllowed);
                if (chosen == null)
                {
                    return false;
                }

                if (RunSaveCodec.TryParse(chosen, out data))
                {
                    return true;
                }

                RunSaveData utility = JsonUtility.FromJson<RunSaveData>(chosen);
                if (utility != null && RunSaveCodec.IsValid(utility) && RunSaveCodec.TryParse(JsonUtility.ToJson(utility), out data))
                {
                    return data != null;
                }

                data = null;
                return false;
            }
            catch (Exception)
            {
                data = null;
                return false;
            }
        }

        public static bool TrySaveRun(RunSaveData data)
        {
            try
            {
                if (!RunSaveCodec.IsValid(data))
                {
                    return false;
                }

                string json = JsonUtility.ToJson(data);
                if (!RunSaveCodec.CommitAllowed(json))
                {
                    json = RunSaveCodec.ToJson(data);
                }

                if (!RunSaveCodec.CommitAllowed(json))
                {
                    return false;
                }

                return TryAtomicWrite(RunFileName, json, RunSaveCodec.CommitAllowed);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static void DeleteRun()
        {
            try
            {
                string path = Path.Combine(Application.persistentDataPath, RunFileName);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                string runBackup = path + SaveFileChoice.BackupSuffix;
                if (File.Exists(runBackup))
                {
                    File.Delete(runBackup);
                }
            }
            catch (Exception)
            {
                // A failed delete must not take the run down with it.
            }
        }

        public static MetaData LoadMeta()
        {
            try
            {
                string path = Path.Combine(Application.persistentDataPath, MetaFileName);
                string backupPath = path + SaveFileChoice.BackupSuffix;
                bool mainExists = File.Exists(path);
                string mainText = mainExists ? File.ReadAllText(path) : null;
                bool backupExists = File.Exists(backupPath);
                string backupText = backupExists ? File.ReadAllText(backupPath) : null;
                string chosen = SaveFileChoice.Choose(mainText, mainExists, backupText, backupExists, MetaJsonOk);
                if (chosen == null)
                {
                    return FinishMeta(MetaData.Fresh(), null);
                }

                MetaData parsed;
                if (!MetaCodec.TryParse(chosen, out parsed))
                {
                    MetaData utility = JsonUtility.FromJson<MetaData>(chosen);
                    if (utility != null && MetaCodec.IsValid(utility))
                    {
                        return FinishMeta(utility, chosen);
                    }

                    return FinishMeta(MetaData.Fresh(), null);
                }

                return FinishMeta(parsed, chosen);
            }
            catch (Exception)
            {
                return FinishMeta(MetaData.Fresh(), null);
            }
        }

        private static MetaData FinishMeta(MetaData data, string json)
        {
            if (data == null)
            {
                data = MetaData.Fresh();
            }

            bool sawTutorial = json != null && json.IndexOf("\"TutorialDone\"", System.StringComparison.Ordinal) >= 0;
            bool sawDifficulty = json != null && json.IndexOf("\"DifficultyChosen\"", System.StringComparison.Ordinal) >= 0;
            FirstRunRules.Migrate(data, sawTutorial, sawDifficulty);
            if (!sawTutorial)
            {
                LocalBest storedBest = LocalBest.Load();
                if (storedBest != null && FirstRunRules.HasLocalBest(storedBest.Score, storedBest.Wave))
                {
                    data.TutorialDone = 1;
                }
            }

            if (!sawDifficulty && DifficultySettings.HasSavedChoice())
            {
                data.DifficultyChosen = 1;
            }

            return data;
        }

        public static bool TrySaveMeta(MetaData data)
        {
            try
            {
                if (!MetaCodec.IsValid(data))
                {
                    return false;
                }

                string json = JsonUtility.ToJson(data);
                if (!MetaCodec.TryParse(json, out MetaData check) || check == null)
                {
                    json = MetaCodec.ToJson(data);
                }

                MetaData confirmed;
                if (!MetaCodec.TryParse(json, out confirmed))
                {
                    return false;
                }

                return TryAtomicWrite(MetaFileName, json, MetaJsonOk);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool MetaJsonOk(string json)
        {
            MetaData parsed;
            return MetaCodec.TryParse(json, out parsed);
        }

        private static bool TryAtomicWrite(string fileName, string json, Func<string, bool> accept)
        {
            if (accept == null || !accept(json))
            {
                return false;
            }

            string dest = Path.Combine(Application.persistentDataPath, fileName);
            string temp = dest + TempSuffix;
            File.WriteAllText(temp, json);
            string reread = File.ReadAllText(temp);
            if (!accept(reread))
            {
                if (File.Exists(temp))
                {
                    File.Delete(temp);
                }

                return false;
            }

            if (File.Exists(dest))
            {
                string backup = dest + SaveFileChoice.BackupSuffix;
                File.Replace(temp, dest, backup);
            }
            else
            {
                File.Move(temp, dest);
            }

            File.WriteAllText(dest + SaveFileChoice.BackupSuffix, json);
            return true;
        }
    }
}
