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
                if (!File.Exists(path))
                {
                    return false;
                }

                string json = File.ReadAllText(path);
                RunSaveData utility = JsonUtility.FromJson<RunSaveData>(json);
                if (!RunSaveCodec.TryParse(json, out data))
                {
                    data = null;
                    if (utility != null && RunSaveCodec.IsValid(utility) && RunSaveCodec.TryParse(JsonUtility.ToJson(utility), out data))
                    {
                        return data != null;
                    }

                    data = null;
                    return false;
                }

                return true;
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
                if (!File.Exists(path))
                {
                    return MetaData.Fresh();
                }

                string json = File.ReadAllText(path);
                MetaData parsed;
                if (!MetaCodec.TryParse(json, out parsed))
                {
                    MetaData utility = JsonUtility.FromJson<MetaData>(json);
                    if (utility != null && MetaCodec.IsValid(utility))
                    {
                        return utility;
                    }

                    return MetaData.Fresh();
                }

                return parsed;
            }
            catch (Exception)
            {
                return MetaData.Fresh();
            }
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
                string backup = dest + ".bak";
                File.Replace(temp, dest, backup);
                if (File.Exists(backup))
                {
                    File.Delete(backup);
                }
            }
            else
            {
                File.Move(temp, dest);
            }

            return true;
        }
    }
}
