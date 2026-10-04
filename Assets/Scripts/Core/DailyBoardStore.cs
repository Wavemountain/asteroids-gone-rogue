using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// PlayerPrefs blob for the daily board. A missing or unreadable blob
    /// loads empty and is not written back until a record is saved.
    /// </summary>
    public static class DailyBoardStore
    {
        public const string Key = "agr.daily.board";

        public static DailyBoardData Load()
        {
            string json = PlayerPrefs.GetString(Key, string.Empty);
            DailyBoardData data;
            if (!DailyBoardCodec.TryParse(json, out data))
            {
                return DailyBoardRules.Fresh();
            }

            return data;
        }

        public static void Save(DailyBoardData data)
        {
            if (data == null)
            {
                return;
            }

            PlayerPrefs.SetString(Key, DailyBoardCodec.ToJson(data));
            PlayerPrefs.Save();
        }
    }
}
