using UnityEditor;
using UnityEngine;

namespace AsteroidsGoneRogue.EditorTools
{
    public static partial class AgrCapture
    {
        const int HudShotCount = 11;
        const string KeyHudSet = "AgrCapture.HudSet";

        static int ReadHudSetFlag()
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int index = 0; index < args.Length; index++)
            {
                if (args[index] != "-hudSet" || index + 1 >= args.Length)
                {
                    continue;
                }

                if (args[index + 1] == "1")
                {
                    return 1;
                }

                if (args[index + 1] == "0")
                {
                    return 0;
                }
            }

            return 0;
        }

        static bool HudSetOn()
        {
            return SessionState.GetInt(KeyHudSet, 0) != 0;
        }

        static void TickHudSettle()
        {
            Nudge();
            if (!AdvanceFrame())
            {
                return;
            }

            int shot = SessionState.GetInt(KeyShot, 0);
            bool reduce = SessionState.GetInt(KeyReduce, 0) != 0;
            if (!HudShotWrites(shot, reduce))
            {
                AdvanceShot();
                return;
            }

            int waited = SessionState.GetInt(KeyWaited, 0);
            if (waited < 8)
            {
                return;
            }

            WriteHudFrames(shot, reduce);
            if (HasError())
            {
                return;
            }

            AdvanceShot();
        }

        static bool HudShotWrites(int shot, bool reduce)
        {
            if (!reduce)
            {
                return true;
            }

            return shot == 0 || shot == 1 || shot == 5;
        }

        static void WriteHudFrames(int shot, bool reduce)
        {
            GameUi ui = FindHudUi();
            if (ui == null)
            {
                Fail("hud capture missing GameUi");
                return;
            }

            string pose = HudPose(shot);
            int[] widths;
            int[] heights;
            HudResolutions(shot, out widths, out heights);
            string folder = System.IO.Path.Combine(SessionState.GetString(KeyOut, string.Empty), "hud");
            System.IO.Directory.CreateDirectory(folder);
            for (int index = 0; index < widths.Length; index++)
            {
                int width = widths[index];
                int height = heights[index];
                Width = width;
                Height = height;
                Screen.SetResolution(width, height, FullScreenMode.Windowed);
                Canvas.ForceUpdateCanvases();
                ui.CaptureHangarPose(pose);
                Canvas.ForceUpdateCanvases();
                string fileName = HudFile(shot, reduce, width, height);
                string path = System.IO.Path.Combine(folder, fileName);
                byte[] png = RenderFrame();
                if (png == null || png.Length < 64)
                {
                    Fail("empty hud capture " + fileName);
                    return;
                }

                System.IO.File.WriteAllBytes(path, png);
                Debug.Log("AgrCapture hud wrote " + path);
                MeasureHud(width, height);
                if (pose == HangarHudLayout.PoseClearDone || pose == HangarHudLayout.PoseMax)
                {
                    Loc.SetLanguage(GameLanguage.Swedish);
                    ui.Refresh();
                    Canvas.ForceUpdateCanvases();
                    MeasureHud(width, height);
                    Loc.SetLanguage(GameLanguage.English);
                    ui.CaptureHangarPose(pose);
                    Canvas.ForceUpdateCanvases();
                }
            }

            Width = 1920;
            Height = 1080;
            Screen.SetResolution(Width, Height, FullScreenMode.Windowed);
        }

        static GameUi FindHudUi()
        {
            GameUi[] uis = Object.FindObjectsByType<GameUi>();
            if (uis == null || uis.Length == 0)
            {
                return null;
            }

            return uis[0];
        }

        static void HudResolutions(int shot, out int[] widths, out int[] heights)
        {
            bool multi = shot == 0 || shot == 5 || shot == 8 || shot == 9;
            if (multi)
            {
                widths = new int[] { 1920, 1280, 2560 };
                heights = new int[] { 1080, 800, 1440 };
                return;
            }

            widths = new int[] { 1920 };
            heights = new int[] { 1080 };
        }

        static string HudPose(int shot)
        {
            if (shot == 0)
            {
                return HangarHudLayout.PoseClearDone;
            }

            if (shot == 1)
            {
                return HangarHudLayout.PoseClearMid;
            }

            if (shot == 2)
            {
                return HangarHudLayout.PoseRecordMedal;
            }

            if (shot == 3)
            {
                return HangarHudLayout.PoseRecordPill;
            }

            if (shot == 4)
            {
                return HangarHudLayout.PoseDailySv;
            }

            if (shot == 5)
            {
                return HangarHudLayout.PoseMax;
            }

            if (shot == 6)
            {
                return HangarHudLayout.PoseLives7;
            }

            if (shot == 7)
            {
                return HangarHudLayout.PoseDanger;
            }

            if (shot == 8)
            {
                return HangarHudLayout.PoseRepair;
            }

            if (shot == 9)
            {
                return HangarHudLayout.PosePoor;
            }

            return HangarHudLayout.PoseFailed;
        }

        static string HudFile(int shot, bool reduce, int width, int height)
        {
            string mode = reduce ? "reduce" : "full";
            string res = width.ToString() + "x" + height.ToString();
            if (shot == 0)
            {
                return "C1_waveclear_" + mode + "_" + res + ".png";
            }

            if (shot == 1)
            {
                return "C2_countup_" + mode + "_" + res + ".png";
            }

            if (shot == 2)
            {
                return "C3_record_medal_full_" + res + ".png";
            }

            if (shot == 3)
            {
                return "C3_record_pill_full_" + res + ".png";
            }

            if (shot == 4)
            {
                return "C4_daily_sv_full_" + res + ".png";
            }

            if (shot == 5)
            {
                return "C5_max_" + mode + "_" + res + ".png";
            }

            if (shot == 6)
            {
                return "C5_lives7_full_" + res + ".png";
            }

            if (shot == 7)
            {
                return "C6_danger_full_" + res + ".png";
            }

            if (shot == 8)
            {
                return "C7_repair_full_" + res + ".png";
            }

            if (shot == 9)
            {
                return "C7_poor_full_" + res + ".png";
            }

            return "C8_failed_full_" + res + ".png";
        }
    }
}
