namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Action glyph paths under Resources. A missing action (keyboard cycle_prev)
    /// returns no path so the caller falls back to text. hi/ is for canvas
    /// scale above <see cref="HiScaleThreshold"/>.
    /// </summary>
    public static class PromptCatalog
    {
        public const string ResourceRoot = "UI/InputPrompts";
        public const float HiScaleThreshold = 1.25f;

        private static readonly string[] PadActions = new string[]
        {
            "aim",
            "cancel",
            "confirm",
            "cycle",
            "cycle_alt",
            "cycle_prev",
            "fire",
            "fire_alt",
            "move",
            "nav",
            "pause",
            "settings",
            "utility",
        };

        private static readonly string[] KeyboardActions = new string[]
        {
            "aim",
            "cancel",
            "confirm",
            "cycle",
            "fire",
            "fire_alt",
            "move",
            "nav",
            "pause",
            "settings",
            "utility",
            "utility_alt",
        };

        public static bool UseHi(float canvasScale)
        {
            return canvasScale > HiScaleThreshold;
        }

        public static bool Has(InputScheme scheme, string action)
        {
            if (string.IsNullOrEmpty(action))
            {
                return false;
            }

            string[] table = scheme == InputScheme.Keyboard ? KeyboardActions : PadActions;
            int count = table.Length;
            for (int index = 0; index < count; index++)
            {
                if (table[index] == action)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Resources path without extension, or null when that scheme has no glyph.
        /// </summary>
        public static string ResourcePath(InputScheme scheme, string action, bool hi)
        {
            if (!Has(scheme, action))
            {
                return null;
            }

            string folder = InputSchemeRules.Folder(scheme);
            if (hi)
            {
                return ResourceRoot + "/" + folder + "/hi/" + action;
            }

            return ResourceRoot + "/" + folder + "/" + action;
        }
    }
}
