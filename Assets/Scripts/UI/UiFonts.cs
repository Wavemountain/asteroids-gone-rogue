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

            font = Resources.GetBuiltinResource<Font>(LegacyBuiltin);
            if (font == null)
            {
                font = Font.CreateDynamicFontFromOSFont("LegacyRuntime", 16);
            }

            return font;
        }
    }
}
