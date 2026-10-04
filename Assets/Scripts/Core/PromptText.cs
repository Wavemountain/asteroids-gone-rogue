using System.Collections.Generic;

namespace AsteroidsGoneRogue
{
    public enum PromptPieceKind
    {
        Text = 0,
        Icon = 1,
    }

    /// <summary>
    /// One run of hint copy. Icon pieces name a catalog action. Text pieces are
    /// literal copy or the fallback label when the glyph is missing.
    /// </summary>
    public sealed class PromptPiece
    {
        public PromptPieceKind Kind;
        public string Text;
        public string Action;
        public bool Important;

        public static PromptPiece MakeText(string text)
        {
            PromptPiece piece = new PromptPiece();
            piece.Kind = PromptPieceKind.Text;
            piece.Text = text ?? string.Empty;
            piece.Action = string.Empty;
            piece.Important = false;
            return piece;
        }

        public static PromptPiece MakeIcon(string action, bool important)
        {
            PromptPiece piece = new PromptPiece();
            piece.Kind = PromptPieceKind.Icon;
            piece.Text = string.Empty;
            piece.Action = action ?? string.Empty;
            piece.Important = important;
            return piece;
        }
    }

    /// <summary>
    /// Splits hint copy on tokens such as {fire} and {pause}. Unknown braces
    /// stay as text. A known token with no glyph becomes its short label.
    /// </summary>
    public static class PromptText
    {
        public static readonly string[] Tokens = new string[]
        {
            "fire",
            "fire_alt",
            "utility",
            "utility_alt",
            "cycle",
            "cycle_prev",
            "cycle_alt",
            "confirm",
            "cancel",
            "pause",
            "move",
            "aim",
            "nav",
            "settings",
        };

        public static bool IsToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            int count = Tokens.Length;
            for (int index = 0; index < count; index++)
            {
                if (Tokens[index] == token)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsImportant(string token)
        {
            if (token == "fire")
            {
                return true;
            }

            if (token == "confirm")
            {
                return true;
            }

            return token == "pause";
        }

        public static PromptPiece[] Resolve(string source, InputScheme scheme)
        {
            List<PromptPiece> pieces = new List<PromptPiece>();
            if (string.IsNullOrEmpty(source))
            {
                return pieces.ToArray();
            }

            string pending = string.Empty;
            int cursor = 0;
            int length = source.Length;
            while (cursor < length)
            {
                char mark = source[cursor];
                if (mark == '{')
                {
                    int end = source.IndexOf('}', cursor + 1);
                    if (end > cursor)
                    {
                        string token = source.Substring(cursor + 1, end - cursor - 1);
                        if (IsToken(token))
                        {
                            BindingMap liveMap = BindingMap.ActiveOrDefault();
                            if (PromptCatalog.Has(scheme, token) && BindingGlyph.ShowsIcon(scheme, token, liveMap))
                            {
                                Flush(pieces, pending);
                                pending = string.Empty;
                                pieces.Add(PromptPiece.MakeIcon(token, IsImportant(token)));
                            }
                            else
                            {
                                pending += BindingGlyph.PromptValue(token, scheme, liveMap);
                            }

                            cursor = end + 1;
                            continue;
                        }
                    }
                }

                pending += mark;
                cursor += 1;
            }

            Flush(pieces, pending);
            return pieces.ToArray();
        }

        /// <summary>
        /// Text-only form for labels that cannot host an inline image.
        /// </summary>
        public static string Flatten(string source, InputScheme scheme)
        {
            if (string.IsNullOrEmpty(source))
            {
                return string.Empty;
            }

            string built = string.Empty;
            int cursor = 0;
            int length = source.Length;
            while (cursor < length)
            {
                char mark = source[cursor];
                if (mark == '{')
                {
                    int end = source.IndexOf('}', cursor + 1);
                    if (end > cursor)
                    {
                        string token = source.Substring(cursor + 1, end - cursor - 1);
                        if (IsToken(token))
                        {
                            built += BindingGlyph.PromptValue(token, scheme, BindingMap.ActiveOrDefault());
                            cursor = end + 1;
                            continue;
                        }
                    }
                }

                built += mark;
                cursor += 1;
            }

            return built;
        }

        public static string Fallback(InputScheme scheme, string token)
        {
            return BindingGlyph.PromptValue(token, scheme, BindingMap.ActiveOrDefault());
        }

        public static string StockLabel(InputScheme scheme, string token)
        {
            if (scheme == InputScheme.PlayStation)
            {
                return PlayStationLabel(token);
            }

            if (scheme == InputScheme.Deck)
            {
                return DeckLabel(token);
            }

            if (scheme == InputScheme.Xbox)
            {
                return XboxLabel(token);
            }

            return KeyboardLabel(token);
        }

        private static void Flush(List<PromptPiece> pieces, string pending)
        {
            if (string.IsNullOrEmpty(pending))
            {
                return;
            }

            pieces.Add(PromptPiece.MakeText(pending));
        }

        private static string KeyboardLabel(string token)
        {
            if (token == "move")
            {
                return "WASD";
            }

            if (token == "aim")
            {
                return "Mouse";
            }

            if (token == "fire")
            {
                return "LMB";
            }

            if (token == "fire_alt")
            {
                return "Space";
            }

            if (token == "utility")
            {
                return "RMB";
            }

            if (token == "utility_alt")
            {
                return "E";
            }

            if (token == "cycle")
            {
                return "Q";
            }

            if (token == "cycle_prev" || token == "cycle_alt")
            {
                return Loc.T("ui.prompt.unbound", "unbound");
            }

            if (token == "pause" || token == "cancel")
            {
                return "Esc";
            }

            if (token == "settings")
            {
                return "F1";
            }

            if (token == "confirm")
            {
                return "Enter";
            }

            if (token == "nav")
            {
                return "Arrows";
            }

            return string.Empty;
        }

        private static string XboxLabel(string token)
        {
            if (token == "move")
            {
                return "LS";
            }

            if (token == "aim")
            {
                return "RS";
            }

            if (token == "fire")
            {
                return "RT";
            }

            if (token == "fire_alt" || token == "confirm")
            {
                return "A";
            }

            if (token == "utility" || token == "utility_alt")
            {
                return "LT";
            }

            if (token == "cycle")
            {
                return "LB";
            }

            if (token == "cycle_prev")
            {
                return "RB";
            }

            if (token == "cycle_alt")
            {
                return "X";
            }

            if (token == "pause")
            {
                return "Start";
            }

            if (token == "settings")
            {
                return "View";
            }

            if (token == "cancel")
            {
                return "B";
            }

            if (token == "nav")
            {
                return "D-pad";
            }

            return string.Empty;
        }

        private static string PlayStationLabel(string token)
        {
            if (token == "move")
            {
                return "L";
            }

            if (token == "aim")
            {
                return "R";
            }

            if (token == "fire")
            {
                return "R2";
            }

            if (token == "fire_alt" || token == "confirm")
            {
                return "Cross";
            }

            if (token == "utility" || token == "utility_alt")
            {
                return "L2";
            }

            if (token == "cycle")
            {
                return "L1";
            }

            if (token == "cycle_prev")
            {
                return "R1";
            }

            if (token == "cycle_alt")
            {
                return "Square";
            }

            if (token == "pause")
            {
                return "Options";
            }

            if (token == "settings")
            {
                return "Create";
            }

            if (token == "cancel")
            {
                return "Circle";
            }

            if (token == "nav")
            {
                return "D-pad";
            }

            return string.Empty;
        }

        private static string DeckLabel(string token)
        {
            if (token == "move")
            {
                return "L";
            }

            if (token == "aim")
            {
                return "R";
            }

            if (token == "fire")
            {
                return "R2";
            }

            if (token == "fire_alt" || token == "confirm")
            {
                return "A";
            }

            if (token == "utility" || token == "utility_alt")
            {
                return "L2";
            }

            if (token == "cycle")
            {
                return "L1";
            }

            if (token == "cycle_prev")
            {
                return "R1";
            }

            if (token == "cycle_alt")
            {
                return "X";
            }

            if (token == "pause")
            {
                return "Options";
            }

            if (token == "settings")
            {
                return "View";
            }

            if (token == "cancel")
            {
                return "B";
            }

            if (token == "nav")
            {
                return "D-pad";
            }

            return string.Empty;
        }
    }
}
