namespace AsteroidsGoneRogue
{
    /// <summary>
    /// One-line binding blob for PlayerPrefs. Version 7 matches SettingsState.
    /// A corrupt blob, or any settings save older than v7, loads defaults.
    /// </summary>
    public static class BindingStore
    {
        public const int FormatVersion = 7;
        public const string PrefsKey = "agr.settings.bindings";
        public const int PartCount = 21;

        public static string Serialize(BindingMap map)
        {
            BindingMap source = map ?? BindingMap.CreateDefault();
            string blob = FormatVersion.ToString();
            for (int action = 0; action < BindAction.Count; action++)
            {
                blob += ";" + EncodeKeys(source, action);
            }

            for (int action = 0; action < BindAction.Count; action++)
            {
                blob += ";" + EncodePad(source, action);
            }

            return blob;
        }

        public static BindingMap Parse(string blob, int settingsVersion)
        {
            if (settingsVersion < 7)
            {
                return BindingMap.CreateDefault();
            }

            return ParseBlob(blob);
        }

        public static BindingMap ParseBlob(string blob)
        {
            if (string.IsNullOrEmpty(blob))
            {
                return BindingMap.CreateDefault();
            }

            string[] parts = blob.Split(';');
            if (parts.Length != PartCount)
            {
                return BindingMap.CreateDefault();
            }

            if (parts[0] != FormatVersion.ToString())
            {
                return BindingMap.CreateDefault();
            }

            BindingMap map = BindingMap.CreateDefault();
            map.ClearAll();
            for (int action = 0; action < BindAction.Count; action++)
            {
                if (!DecodeKeys(parts[1 + action], map, action))
                {
                    return BindingMap.CreateDefault();
                }
            }

            for (int action = 0; action < BindAction.Count; action++)
            {
                if (!DecodePad(parts[11 + action], map, action))
                {
                    return BindingMap.CreateDefault();
                }
            }

            if (!map.OwnersAreValid())
            {
                return BindingMap.CreateDefault();
            }

            return map;
        }

        private static string EncodeKeys(BindingMap map, int action)
        {
            string built = string.Empty;
            for (int slot = 0; slot < 4; slot++)
            {
                if (slot > 0)
                {
                    built += ",";
                }

                BindControl control = map.GetKey(action, slot);
                if (control.Empty())
                {
                    built += "0:0";
                }
                else
                {
                    built += control.Kind.ToString() + ":" + control.Code.ToString();
                }
            }

            return built;
        }

        private static string EncodePad(BindingMap map, int action)
        {
            if (map.Pad0[action].Kind != BindKind.Pad)
            {
                return string.Empty;
            }

            return map.Pad0[action].Code.ToString();
        }

        private static bool DecodeKeys(string text, BindingMap map, int action)
        {
            if (string.IsNullOrEmpty(text))
            {
                return true;
            }

            string[] tokens = text.Split(',');
            if (tokens.Length < 1 || tokens.Length > 4)
            {
                return false;
            }

            for (int index = 0; index < tokens.Length; index++)
            {
                int kind;
                int code;
                if (!SplitPair(tokens[index], out kind, out code))
                {
                    return false;
                }

                if (kind == BindKind.None)
                {
                    continue;
                }

                if (kind != BindKind.Key && kind != BindKind.Mouse)
                {
                    return false;
                }

                if (kind == BindKind.Key && (code < 1 || code > 319))
                {
                    return false;
                }

                if (kind == BindKind.Mouse && (code < 0 || code > 6))
                {
                    return false;
                }

                BindControl control = kind == BindKind.Mouse ? BindControl.Mouse(code) : BindControl.Key(code);
                if (index == 0)
                {
                    map.Key0[action] = control;
                }
                else if (index == 1)
                {
                    map.Key1[action] = control;
                }
                else if (index == 2)
                {
                    map.Key2[action] = control;
                }
                else
                {
                    map.Key3[action] = control;
                }
            }

            return true;
        }

        private static bool DecodePad(string text, BindingMap map, int action)
        {
            if (string.IsNullOrEmpty(text))
            {
                return true;
            }

            int code;
            if (!ReadInt(text, out code))
            {
                return false;
            }

            if (code < 0 || code > 19)
            {
                return false;
            }

            map.Pad0[action] = BindControl.Pad(code);
            return true;
        }

        private static bool SplitPair(string token, out int kind, out int code)
        {
            kind = 0;
            code = 0;
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            int colon = token.IndexOf(':');
            if (colon <= 0 || colon >= token.Length - 1)
            {
                return false;
            }

            if (!ReadInt(token.Substring(0, colon), out kind))
            {
                return false;
            }

            return ReadInt(token.Substring(colon + 1), out code);
        }

        private static bool ReadInt(string text, out int value)
        {
            value = 0;
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            int sign = 1;
            int index = 0;
            if (text[0] == '-')
            {
                sign = -1;
                index = 1;
            }

            if (index >= text.Length)
            {
                return false;
            }

            int built = 0;
            for (; index < text.Length; index++)
            {
                char mark = text[index];
                if (mark < '0' || mark > '9')
                {
                    return false;
                }

                built = (built * 10) + (mark - '0');
            }

            value = built * sign;
            return true;
        }
    }
}
