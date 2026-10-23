using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Bundled Kenney CC0 fonts for HUD / hangar typography.
    /// LegacyRuntime is the Unity 6.6 builtin fallback only — never Arial.
    /// </summary>
    public static class UiFonts
    {
        public const string DisplayResource = "Fonts/KenneyFuture";
        public const string BodyResource = "Fonts/KenneyFutureNarrow";
        public const string LegacyBuiltin = "LegacyRuntime.ttf";

        public static Font Display()
        {
            return Load(DisplayResource);
        }

        public static Font Body()
        {
            Font body = Resources.Load<Font>(BodyResource);
            if (body != null)
            {
                return body;
            }

            return Display();
        }

        public static Font Load(string resource)
        {
            Font font = Resources.Load<Font>(resource);
            if (font != null)
            {
                return font;
            }

            return Legacy();
        }

        /// <summary>
        /// Digits for the hangar counters and the play HUD score.
        /// MakeDigits (credit / strip / damage on <see cref="Display"/>,
        /// life / hull / shield values on <see cref="Body"/>) and the play
        /// score and bar counts used to force <see cref="FontStyle.Bold"/>.
        /// Both Kenney faces are regular weight, and Unity 6000.6 draws the
        /// missing-glyph box for digits in that style. Probe 0-9 and fall
        /// back to LegacyRuntime when the face cannot draw them.
        /// </summary>
        public const string DigitProbe = "0123456789";

        public static Font ForDigits(Font preferred, int size, out FontStyle style)
        {
            int pixel = size < 1 ? 1 : size;
            style = FontStyle.Normal;
            if (CoversDigits(preferred, pixel, style))
            {
                preferred.RequestCharactersInTexture(HudProbe, pixel, style);
                return preferred;
            }

            Font legacy = Legacy();
            if (CoversDigits(legacy, pixel, style))
            {
                legacy.RequestCharactersInTexture(HudProbe, pixel, style);
                return legacy;
            }

            if (preferred != null)
            {
                preferred.RequestCharactersInTexture(HudProbe, pixel, style);
                return preferred;
            }

            return legacy;
        }

        const string HudProbe = "0123456789×+·-/.:% ";

        static bool CoversDigits(Font font, int size, FontStyle style)
        {
            if (font == null)
            {
                return false;
            }

            font.RequestCharactersInTexture(DigitProbe, size, style);
            for (int index = 0; index < DigitProbe.Length; index++)
            {
                CharacterInfo info;
                if (!font.GetCharacterInfo(DigitProbe[index], out info, size, style))
                {
                    return false;
                }
            }

            return true;
        }

        static Font Legacy()
        {
            Font font = Resources.GetBuiltinResource<Font>(LegacyBuiltin);
            if (font == null)
            {
                font = Font.CreateDynamicFontFromOSFont("LegacyRuntime", 16);
            }

            return font;
        }
    }
}
