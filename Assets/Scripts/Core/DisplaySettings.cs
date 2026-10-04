namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Window, resolution, vsync, and FPS cap. Unity-free. Invalid values fall
    /// back to the launch defaults (borderless, 1920x1080, vsync on, 60).
    /// An unavailable windowed resolution falls back to the current size.
    /// </summary>
    public enum WindowModeId
    {
        Windowed = 0,
        Borderless = 1,
        Fullscreen = 2,
    }

    public static class DisplaySettings
    {
        public const int DefaultWindowMode = (int)WindowModeId.Borderless;
        public const int DefaultWidth = 1920;
        public const int DefaultHeight = 1080;
        public const int DefaultVSync = 1;
        public const int DefaultFpsCap = 60;
        public const int Uncapped = 0;
        public const int MinWidth = 640;
        public const int MinHeight = 480;

        // UnityEngine.FullScreenMode values. Kept as ints so this file stays Unity-free.
        public const int ModeExclusive = 0;
        public const int ModeBorderless = 1;
        public const int ModeWindowed = 3;

        public static readonly int[] CatalogWidths = new int[]
        {
            1280, 1280, 1366, 1440, 1600, 1920, 1920, 2560, 2560, 3440,
        };

        public static readonly int[] CatalogHeights = new int[]
        {
            720, 800, 768, 900, 900, 1080, 1200, 1080, 1440, 1440,
        };

        public static int CatalogCount
        {
            get { return CatalogWidths.Length; }
        }

        public static int NormalizeWindow(int mode)
        {
            if (mode == (int)WindowModeId.Windowed)
            {
                return (int)WindowModeId.Windowed;
            }

            if (mode == (int)WindowModeId.Fullscreen)
            {
                return (int)WindowModeId.Fullscreen;
            }

            return DefaultWindowMode;
        }

        public static int NormalizeFps(int cap)
        {
            if (cap == 30 || cap == 60 || cap == 120 || cap == Uncapped)
            {
                return cap;
            }

            return DefaultFpsCap;
        }

        public static int NormalizeVSync(int stored)
        {
            return stored != 0 ? 1 : 0;
        }

        public static int ClampDimension(int value, int minimum, int fallback)
        {
            if (value < minimum)
            {
                return fallback;
            }

            return value;
        }

        public static int ToFullScreenMode(int windowMode)
        {
            int mode = NormalizeWindow(windowMode);
            if (mode == (int)WindowModeId.Windowed)
            {
                return ModeWindowed;
            }

            if (mode == (int)WindowModeId.Fullscreen)
            {
                return ModeExclusive;
            }

            return ModeBorderless;
        }

        /// <summary>
        /// Cap actually written to Application.targetFrameRate.
        /// Uncapped is -1. VSync does not change this number: when vSyncCount
        /// is 1 Unity ignores targetFrameRate, which the settings row states.
        /// </summary>
        public static int TargetFrameRate(int fpsCap)
        {
            int cap = NormalizeFps(fpsCap);
            if (cap == Uncapped)
            {
                return -1;
            }

            return cap;
        }

        public static int VSyncCount(int vsync)
        {
            return NormalizeVSync(vsync);
        }

        public static bool Listed(int width, int height, int[] widths, int[] heights)
        {
            if (widths == null || heights == null)
            {
                return false;
            }

            int count = widths.Length;
            if (heights.Length < count)
            {
                count = heights.Length;
            }

            for (int index = 0; index < count; index++)
            {
                if (widths[index] == width && heights[index] == height)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Windowed size. A size missing from <paramref name="availableWidths"/>
        /// falls back to the current screen size, then to 1920x1080.
        /// An empty availability list accepts the requested size (editor / unknown).
        /// </summary>
        public static void Resolve(
            int width,
            int height,
            int[] availableWidths,
            int[] availableHeights,
            int currentWidth,
            int currentHeight,
            out int resolvedWidth,
            out int resolvedHeight)
        {
            int safeW = ClampDimension(width, MinWidth, DefaultWidth);
            int safeH = ClampDimension(height, MinHeight, DefaultHeight);
            bool known = availableWidths != null && availableWidths.Length > 0;
            if (!known || Listed(safeW, safeH, availableWidths, availableHeights))
            {
                resolvedWidth = safeW;
                resolvedHeight = safeH;
                return;
            }

            int currentW = ClampDimension(currentWidth, MinWidth, DefaultWidth);
            int currentH = ClampDimension(currentHeight, MinHeight, DefaultHeight);
            if (!known || Listed(currentW, currentH, availableWidths, availableHeights))
            {
                resolvedWidth = currentW;
                resolvedHeight = currentH;
                return;
            }

            resolvedWidth = DefaultWidth;
            resolvedHeight = DefaultHeight;
        }

        public static int IndexOf(int width, int height)
        {
            for (int index = 0; index < CatalogCount; index++)
            {
                if (CatalogWidths[index] == width && CatalogHeights[index] == height)
                {
                    return index;
                }
            }

            return -1;
        }

        public static int StepWindow(int current, int direction)
        {
            int index = 1;
            int mode = NormalizeWindow(current);
            if (mode == (int)WindowModeId.Windowed)
            {
                index = 0;
            }
            else if (mode == (int)WindowModeId.Fullscreen)
            {
                index = 2;
            }

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

            if (index > 2)
            {
                index = 2;
            }

            if (index == 0)
            {
                return (int)WindowModeId.Windowed;
            }

            if (index == 2)
            {
                return (int)WindowModeId.Fullscreen;
            }

            return (int)WindowModeId.Borderless;
        }

        public static int StepFps(int current, int direction)
        {
            int[] order = new int[] { 30, 60, 120, Uncapped };
            int cap = NormalizeFps(current);
            int index = 1;
            for (int slot = 0; slot < order.Length; slot++)
            {
                if (order[slot] == cap)
                {
                    index = slot;
                    break;
                }
            }

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

            if (index >= order.Length)
            {
                index = order.Length - 1;
            }

            return order[index];
        }

        public static void StepResolution(int width, int height, int direction, out int nextWidth, out int nextHeight)
        {
            int index = IndexOf(width, height);
            if (index < 0)
            {
                index = IndexOf(DefaultWidth, DefaultHeight);
            }

            if (index < 0)
            {
                index = 0;
            }

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

            if (index >= CatalogCount)
            {
                index = CatalogCount - 1;
            }

            nextWidth = CatalogWidths[index];
            nextHeight = CatalogHeights[index];
        }
    }
}
