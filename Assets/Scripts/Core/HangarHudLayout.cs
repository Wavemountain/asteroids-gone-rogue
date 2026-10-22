namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Screen anchors for the hangar ship-status panel and the wave strip.
    /// The HUD placement is <see cref="Hud"/> alone. Move that one rect.
    /// Unity-free so overlap can be tested without the editor.
    /// </summary>
    public static class HangarHudLayout
    {
        public const float PreviewMaxY = 0.468f;

        public static readonly ScreenBand Hud = new ScreenBand(0.562f, 0.480f, 0.986f, 0.596f);
        public static readonly ScreenBand Preview = new ScreenBand(0.562f, 0.080f, 0.986f, PreviewMaxY);
        public static readonly ScreenBand Doctrine = new ScreenBand(0.562f, 0.608f, 0.986f, 0.898f);
        public static readonly ScreenBand TopBar = new ScreenBand(0.012f, 0.905f, 0.988f, 0.995f);
        public static readonly ScreenBand Title = new ScreenBand(0.012f, 0.905f, 0.340f, 0.995f);
        public static readonly ScreenBand WorldRow = new ScreenBand(0.490f, 0.906f, 0.735f, 0.948f);
        public static readonly ScreenBand MedalRow = new ScreenBand(0.490f, 0.952f, 0.735f, 0.992f);
        public static readonly ScreenBand HangarPanel = new ScreenBand(0.014f, 0.080f, 0.55f, 0.888f);

        public const float StripMinX = 0.03f;
        public const float StripMinY = 0.14f;
        public const float StripMaxX = 0.68f;
        public const float StripMaxY = 0.62f;
        public const float CardMinX = 0.02f;
        public const float CardMinY = 0.816f;
        public const float CardMaxX = 0.98f;
        public const float CardMaxY = 0.946f;
        public const float TitleMinY = 0.66f;
        public const float TitleMaxY = 0.86f;
        public const float TitleMaxX = 0.70f;
        public const float NewRunMinX = 0.74f;
        public const float NewRunMinY = 0.48f;
        public const float NewRunMaxX = 0.97f;
        public const float NewRunMaxY = 0.64f;
        public const float BoonMinY = 0.801f;
        public const float BoonMaxY = 0.814f;

        public const string PoseClearDone = "c1";
        public const string PoseClearMid = "c2";
        public const string PoseRecordMedal = "c3medal";
        public const string PoseRecordPill = "c3pill";
        public const string PoseDailySv = "c4";
        public const string PoseMax = "c5";
        public const string PoseLives7 = "c5x";
        public const string PoseDanger = "c6";
        public const string PoseRepair = "c7repair";
        public const string PosePoor = "c7poor";
        public const string PoseFailed = "c8";

        public static bool Shows(GamePhase phase)
        {
            return phase == GamePhase.Hangar
                || phase == GamePhase.WaveClear
                || phase == GamePhase.CampaignClear
                || phase == GamePhase.Failed;
        }

        public static bool Separated(ScreenBand left, ScreenBand right)
        {
            return !left.Overlaps(right);
        }

        public static bool HudClearsNeighbours()
        {
            if (!Separated(Hud, Preview))
            {
                return false;
            }

            if (!Separated(Hud, Doctrine))
            {
                return false;
            }

            if (!Separated(Hud, TopBar))
            {
                return false;
            }

            if (!Separated(Hud, Title))
            {
                return false;
            }

            if (!Separated(Hud, WorldRow))
            {
                return false;
            }

            if (!Separated(Hud, MedalRow))
            {
                return false;
            }

            if (!Separated(Hud, HangarPanel))
            {
                return false;
            }

            return true;
        }

        public static ScreenBand Card()
        {
            return HangarPanel.Map(CardMinX, CardMinY, CardMaxX, CardMaxY);
        }

        public static ScreenBand Strip()
        {
            return Card().Map(StripMinX, StripMinY, StripMaxX, StripMaxY);
        }

        public static ScreenBand SummaryTitle()
        {
            return Card().Map(0.03f, TitleMinY, TitleMaxX, TitleMaxY);
        }

        public static ScreenBand NewRun()
        {
            return Card().Map(NewRunMinX, NewRunMinY, NewRunMaxX, NewRunMaxY);
        }

        public static ScreenBand BoonRow()
        {
            return HangarPanel.Map(0.03f, BoonMinY, 0.97f, BoonMaxY);
        }

        public static bool StripClearsCardMates()
        {
            ScreenBand strip = Strip();
            if (!Separated(strip, SummaryTitle()))
            {
                return false;
            }

            if (!Separated(strip, NewRun()))
            {
                return false;
            }

            if (!Separated(strip, BoonRow()))
            {
                return false;
            }

            if (!Separated(Card(), TopBar))
            {
                return false;
            }

            return true;
        }

        public static float PreviewHeightPx(float screenHeight)
        {
            return (PreviewMaxY - Preview.MinY) * screenHeight;
        }
    }

    public struct ScreenBand
    {
        public readonly float MinX;
        public readonly float MinY;
        public readonly float MaxX;
        public readonly float MaxY;

        public ScreenBand(float minX, float minY, float maxX, float maxY)
        {
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
        }

        public bool Overlaps(ScreenBand other)
        {
            return MinX < other.MaxX && MaxX > other.MinX && MinY < other.MaxY && MaxY > other.MinY;
        }

        public ScreenBand Map(float minX, float minY, float maxX, float maxY)
        {
            float width = MaxX - MinX;
            float height = MaxY - MinY;
            return new ScreenBand(
                MinX + (minX * width),
                MinY + (minY * height),
                MinX + (maxX * width),
                MinY + (maxY * height));
        }
    }
}
