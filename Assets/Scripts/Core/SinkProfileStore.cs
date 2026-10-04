namespace AsteroidsGoneRogue
{
    /// <summary>
    /// PlayerPrefs wrapper for <see cref="SinkProfileCodec"/>. Missing or
    /// unreadable blobs play as an empty profile.
    /// </summary>
    public static class SinkProfileStore
    {
        public const string Key = "agr.sink.profile";

        public static SinkProfileData Load()
        {
            string raw = UnityEngine.PlayerPrefs.GetString(Key, string.Empty);
            if (string.IsNullOrEmpty(raw))
            {
                return SinkProfileCodec.Fresh();
            }

            SinkProfileData parsed;
            if (!SinkProfileCodec.TryParse(raw, out parsed) || parsed == null)
            {
                return SinkProfileCodec.Fresh();
            }

            return parsed;
        }

        public static void Save(SinkProfileData data)
        {
            SinkProfileData stored = data == null ? SinkProfileCodec.Fresh() : data;
            UnityEngine.PlayerPrefs.SetString(Key, SinkProfileCodec.ToJson(stored));
            UnityEngine.PlayerPrefs.Save();
        }
    }
}
