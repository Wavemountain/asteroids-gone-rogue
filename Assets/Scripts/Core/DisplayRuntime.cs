using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Applies <see cref="DisplaySettings"/> immediately. VSync on sets
    /// vSyncCount to 1, which makes Unity ignore targetFrameRate.
    /// </summary>
    public static class DisplayRuntime
    {
        public static void Apply(SettingsState state)
        {
            if (state == null)
            {
                return;
            }

            state.Normalize();
            int mode = DisplaySettings.ToFullScreenMode(state.WindowMode);
            int width = state.ResolutionWidth;
            int height = state.ResolutionHeight;
            if (state.WindowMode != (int)WindowModeId.Windowed)
            {
                Resolution desktop = Screen.currentResolution;
                if (desktop.width >= DisplaySettings.MinWidth && desktop.height >= DisplaySettings.MinHeight)
                {
                    width = desktop.width;
                    height = desktop.height;
                }
            }
            else
            {
                Resolution[] listed = Screen.resolutions;
                int count = listed != null ? listed.Length : 0;
                int[] availW = new int[count];
                int[] availH = new int[count];
                for (int index = 0; index < count; index++)
                {
                    availW[index] = listed[index].width;
                    availH[index] = listed[index].height;
                }

                int resolvedW;
                int resolvedH;
                DisplaySettings.Resolve(
                    width,
                    height,
                    availW,
                    availH,
                    Screen.width,
                    Screen.height,
                    out resolvedW,
                    out resolvedH);
                width = resolvedW;
                height = resolvedH;
            }

            if (width < DisplaySettings.MinWidth)
            {
                width = DisplaySettings.DefaultWidth;
            }

            if (height < DisplaySettings.MinHeight)
            {
                height = DisplaySettings.DefaultHeight;
            }

            Screen.SetResolution(width, height, (FullScreenMode)mode);
            QualitySettings.vSyncCount = DisplaySettings.VSyncCount(state.VSync ? 1 : 0);
            Application.targetFrameRate = DisplaySettings.TargetFrameRate(state.FpsCap);
        }
    }
}
