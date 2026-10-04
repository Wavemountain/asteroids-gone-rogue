namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Turns a saved binding into the label the prompt row shows. Default
    /// controls keep the E1 stock word so the catalog icon still matches.
    /// </summary>
    public static class BindingGlyph
    {
        public static string PromptValue(string action, InputScheme scheme, BindingMap map)
        {
            BindingMap used = map ?? BindingMap.CreateDefault();
            string custom = CustomLabel(action, scheme, used);
            if (custom != null)
            {
                return custom;
            }

            return PromptText.StockLabel(scheme, action);
        }

        public static bool ShowsIcon(InputScheme scheme, string action, BindingMap map)
        {
            BindingMap used = map ?? BindingMap.CreateDefault();
            return CustomLabel(action, scheme, used) == null;
        }

        public static string BindingLine(int action, InputScheme scheme, BindingMap map)
        {
            BindingMap used = map ?? BindingMap.CreateDefault();
            string keys = KeyboardText(used, action);
            string pad = PadSide(used, action, scheme);
            return keys + "  ·  " + pad;
        }

        public static string KeyboardText(BindingMap map, int action)
        {
            if (map == null || action < 0 || action >= BindAction.Count)
            {
                return "-";
            }

            string built = string.Empty;
            for (int slot = 0; slot < 4; slot++)
            {
                BindControl control = map.GetKey(action, slot);
                if (control.Empty())
                {
                    continue;
                }

                if (built.Length > 0)
                {
                    built += " ";
                }

                built += ControlLabel(control, InputScheme.Keyboard);
            }

            if (built.Length == 0)
            {
                return "-";
            }

            return built;
        }

        private static string CustomLabel(string action, InputScheme scheme, BindingMap map)
        {
            int index = BindAction.Find(action);
            if (index < 0 || map == null)
            {
                return null;
            }

            if (scheme == InputScheme.Keyboard)
            {
                if (map.KeyboardIsDefault(index))
                {
                    return null;
                }

                return KeyboardText(map, index);
            }

            if (map.PadIsDefault(index))
            {
                return null;
            }

            if (map.Pad0[index].Kind != BindKind.Pad)
            {
                return "-";
            }

            return PadButton(map.Pad0[index].Code, scheme);
        }

        private static string PadSide(BindingMap map, int action, InputScheme scheme)
        {
            if (map.PadIsDefault(action))
            {
                return StockPad(action, scheme);
            }

            if (map.Pad0[action].Kind != BindKind.Pad)
            {
                return "-";
            }

            return PadButton(map.Pad0[action].Code, scheme);
        }

        private static string StockPad(int action, InputScheme scheme)
        {
            if (action == BindAction.Move)
            {
                return scheme == InputScheme.PlayStation || scheme == InputScheme.Deck ? "L" : "LS";
            }

            if (action == BindAction.Fire)
            {
                if (scheme == InputScheme.PlayStation || scheme == InputScheme.Deck)
                {
                    return "R2";
                }

                return "RT";
            }

            if (action == BindAction.Utility)
            {
                if (scheme == InputScheme.PlayStation || scheme == InputScheme.Deck)
                {
                    return "L2";
                }

                return "LT";
            }

            if (action == BindAction.FireAlt || action == BindAction.Confirm)
            {
                if (scheme == InputScheme.PlayStation)
                {
                    return "Cross";
                }

                return "A";
            }

            if (action == BindAction.Cycle)
            {
                if (scheme == InputScheme.PlayStation || scheme == InputScheme.Deck)
                {
                    return "L1";
                }

                return "LB";
            }

            if (action == BindAction.CyclePrev)
            {
                if (scheme == InputScheme.PlayStation || scheme == InputScheme.Deck)
                {
                    return "R1";
                }

                return "RB";
            }

            if (action == BindAction.CycleAlt)
            {
                if (scheme == InputScheme.PlayStation)
                {
                    return "Square";
                }

                return "X";
            }

            if (action == BindAction.Pause)
            {
                if (scheme == InputScheme.PlayStation || scheme == InputScheme.Deck)
                {
                    return "Options";
                }

                return "Start";
            }

            if (action == BindAction.Cancel)
            {
                if (scheme == InputScheme.PlayStation)
                {
                    return "Circle";
                }

                return "B";
            }

            return "-";
        }

        public static string ControlLabel(BindControl control, InputScheme scheme)
        {
            if (control.Kind == BindKind.Mouse)
            {
                if (control.Code == 0)
                {
                    return "LMB";
                }

                if (control.Code == 1)
                {
                    return "RMB";
                }

                if (control.Code == 2)
                {
                    return "MMB";
                }

                return "M" + control.Code.ToString();
            }

            if (control.Kind == BindKind.Pad)
            {
                return PadButton(control.Code, scheme);
            }

            if (control.Kind == BindKind.Key)
            {
                return KeyLabel(control.Code);
            }

            return "-";
        }

        public static string PadButton(int button, InputScheme scheme)
        {
            bool playstation = scheme == InputScheme.PlayStation;
            if (button == 0)
            {
                return playstation ? "Cross" : "A";
            }

            if (button == 1)
            {
                return playstation ? "Circle" : "B";
            }

            if (button == 2)
            {
                return playstation ? "Square" : "X";
            }

            if (button == 3)
            {
                return playstation ? "Triangle" : "Y";
            }

            if (button == 4)
            {
                return playstation || scheme == InputScheme.Deck ? "L1" : "LB";
            }

            if (button == 5)
            {
                return playstation || scheme == InputScheme.Deck ? "R1" : "RB";
            }

            if (button == 6)
            {
                return playstation ? "Create" : "View";
            }

            if (button == 7)
            {
                return playstation || scheme == InputScheme.Deck ? "Options" : "Start";
            }

            return "B" + button.ToString();
        }

        public static string KeyLabel(int code)
        {
            if (code >= BindCodes.A && code <= 122)
            {
                char letter = (char)code;
                if (letter >= 'a' && letter <= 'z')
                {
                    letter = (char)(letter - 32);
                }

                return letter.ToString();
            }

            if (code == BindCodes.Space)
            {
                return "Space";
            }

            if (code == BindCodes.Escape)
            {
                return "Esc";
            }

            if (code == BindCodes.Return)
            {
                return "Enter";
            }

            if (code == BindCodes.LeftControl || code == 305)
            {
                return "Ctrl";
            }

            if (code == 304 || code == 303)
            {
                return "Shift";
            }

            if (code == 308 || code == 307)
            {
                return "Alt";
            }

            if (code == 273)
            {
                return "Up";
            }

            if (code == 274)
            {
                return "Down";
            }

            if (code == 275)
            {
                return "Right";
            }

            if (code == 276)
            {
                return "Left";
            }

            if (code >= 282 && code <= 296)
            {
                return "F" + (code - 281).ToString();
            }

            if (code >= 48 && code <= 57)
            {
                return ((char)code).ToString();
            }

            return "K" + code.ToString();
        }
    }
}
