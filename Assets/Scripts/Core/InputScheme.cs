namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Last-used control family. Keyboard covers mouse. Deck is a Steam Deck
    /// pad that otherwise reports itself as an Xbox pad.
    /// </summary>
    public enum InputScheme
    {
        Keyboard = 0,
        Xbox = 1,
        PlayStation = 2,
        Deck = 3,
    }

    /// <summary>
    /// Settings override. Auto follows <see cref="InputSchemeDetector"/>.
    /// </summary>
    public enum InputSchemePreference
    {
        Auto = 0,
        Xbox = 1,
        PlayStation = 2,
        Deck = 3,
        Keyboard = 4,
    }

    /// <summary>
    /// Pure scheme names and the settings cycle. No Unity input.
    /// </summary>
    public static class InputSchemeRules
    {
        public const int PreferenceCount = 5;

        public static InputSchemePreference Normalize(int value)
        {
            if (value == (int)InputSchemePreference.Xbox)
            {
                return InputSchemePreference.Xbox;
            }

            if (value == (int)InputSchemePreference.PlayStation)
            {
                return InputSchemePreference.PlayStation;
            }

            if (value == (int)InputSchemePreference.Deck)
            {
                return InputSchemePreference.Deck;
            }

            if (value == (int)InputSchemePreference.Keyboard)
            {
                return InputSchemePreference.Keyboard;
            }

            return InputSchemePreference.Auto;
        }

        public static InputSchemePreference Step(InputSchemePreference current, int direction)
        {
            int index = (int)Normalize((int)current);
            if (direction > 0)
            {
                index += 1;
            }
            else if (direction < 0)
            {
                index -= 1;
            }

            if (index < 0)
            {
                index = 0;
            }

            if (index >= PreferenceCount)
            {
                index = PreferenceCount - 1;
            }

            return Normalize(index);
        }

        public static InputScheme FromPreference(InputSchemePreference preference)
        {
            InputSchemePreference normalized = Normalize((int)preference);
            if (normalized == InputSchemePreference.Xbox)
            {
                return InputScheme.Xbox;
            }

            if (normalized == InputSchemePreference.PlayStation)
            {
                return InputScheme.PlayStation;
            }

            if (normalized == InputSchemePreference.Deck)
            {
                return InputScheme.Deck;
            }

            if (normalized == InputSchemePreference.Keyboard)
            {
                return InputScheme.Keyboard;
            }

            return InputScheme.Keyboard;
        }

        public static bool IsOverride(InputSchemePreference preference)
        {
            return Normalize((int)preference) != InputSchemePreference.Auto;
        }

        public static string Folder(InputScheme scheme)
        {
            if (scheme == InputScheme.Xbox)
            {
                return "xbox";
            }

            if (scheme == InputScheme.PlayStation)
            {
                return "playstation";
            }

            if (scheme == InputScheme.Deck)
            {
                return "deck";
            }

            return "keyboard";
        }
    }
}
