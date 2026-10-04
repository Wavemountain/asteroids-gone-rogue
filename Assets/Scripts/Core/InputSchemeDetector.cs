namespace AsteroidsGoneRogue
{
    /// <summary>
    /// One frame of device evidence. The driver fills this; tests build it directly.
    /// AxisAbs is the largest absolute axis this frame. PadButton is a joystick
    /// button, not a keyboard key. SteamDeck is the SteamDeck=1 environment flag.
    /// </summary>
    public struct SchemeSample
    {
        public bool KeyDown;
        public bool MouseActive;
        public bool PadButton;
        public float AxisAbs;
        public string JoystickName;
        public string[] JoystickNames;
        public bool SteamDeck;
    }

    /// <summary>
    /// Last-used device. Axis noise below 0.3 does not flip the scheme.
    /// A pad button or an axis past 0.5 selects the pad family and wins over
    /// a keyboard or mouse event in the same sample. An override skips detection.
    /// </summary>
    public static class InputSchemeDetector
    {
        public const float AxisNoise = 0.3f;
        public const float AxisEngage = 0.5f;

        public static InputScheme Decide(InputScheme current, InputSchemePreference preference, SchemeSample sample)
        {
            if (InputSchemeRules.IsOverride(preference))
            {
                return InputSchemeRules.FromPreference(preference);
            }

            InputScheme next = current;
            if (sample.KeyDown || sample.MouseActive)
            {
                next = InputScheme.Keyboard;
            }

            bool engaged = sample.AxisAbs > AxisEngage;
            bool pad = sample.PadButton || engaged;
            if (!sample.PadButton && sample.AxisAbs < AxisNoise)
            {
                pad = false;
            }

            if (pad && !sample.PadButton && !engaged)
            {
                pad = false;
            }

            if (pad)
            {
                if (sample.JoystickNames != null && sample.JoystickNames.Length > 0)
                {
                    next = PadKind(sample.JoystickNames, sample.SteamDeck);
                }
                else
                {
                    next = PadKind(sample.JoystickName, sample.SteamDeck);
                }
            }

            return next;
        }

        public static InputScheme PadKind(string joystickName, bool steamDeck)
        {
            string[] names = new string[] { joystickName };
            return PadKind(names, steamDeck);
        }

        public static InputScheme PadKind(string[] names, bool steamDeck)
        {
            bool deckName = false;
            if (names != null)
            {
                int count = names.Length;
                for (int index = 0; index < count; index++)
                {
                    string name = names[index];
                    if (string.IsNullOrEmpty(name))
                    {
                        continue;
                    }

                    if (IsPlayStation(name))
                    {
                        return InputScheme.PlayStation;
                    }

                    if (IsDeckName(name))
                    {
                        deckName = true;
                    }
                }
            }

            if (steamDeck || deckName)
            {
                return InputScheme.Deck;
            }

            return InputScheme.Xbox;
        }

        public static bool IsPlayStation(string name)
        {
            if (Contains(name, "Wireless Controller"))
            {
                return true;
            }

            if (Contains(name, "DualSense"))
            {
                return true;
            }

            if (Contains(name, "DualShock"))
            {
                return true;
            }

            if (Contains(name, "PS5"))
            {
                return true;
            }

            return Contains(name, "PS4");
        }

        public static bool IsDeckName(string name)
        {
            if (Contains(name, "SteamDeck"))
            {
                return true;
            }

            return Contains(name, "Steam Deck");
        }

        private static bool Contains(string name, string needle)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(needle))
            {
                return false;
            }

            return name.IndexOf(needle, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
