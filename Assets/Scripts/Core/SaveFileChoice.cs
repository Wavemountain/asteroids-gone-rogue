namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Pick a save body. A missing main file is "no save". A present but
    /// invalid main file falls back to the backup. Unity-free.
    /// </summary>
    public static class SaveFileChoice
    {
        public const string BackupSuffix = ".bak";

        public static string Choose(string mainText, bool mainExists, string backupText, bool backupExists, System.Func<string, bool> accept)
        {
            if (!mainExists)
            {
                return null;
            }

            if (accept != null && mainText != null && accept(mainText))
            {
                return mainText;
            }

            if (backupExists && accept != null && backupText != null && accept(backupText))
            {
                return backupText;
            }

            return null;
        }
    }
}
