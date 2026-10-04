namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Screen anchors for the New Run chooser. Font 18 stays at least 12 px
    /// on the 1280×800 canvas scale.
    /// </summary>
    public static class RunSetupLayout
    {
        public const int Font = 18;
        public const float PanelMinX = 0.30f;
        public const float PanelMinY = 0.30f;
        public const float PanelMaxX = 0.70f;
        public const float PanelMaxY = 0.74f;

        public static void Header(out float minX, out float minY, out float maxX, out float maxY)
        {
            minX = 0.34f;
            minY = 0.64f;
            maxX = 0.66f;
            maxY = 0.70f;
        }

        public static void Blurb(out float minX, out float minY, out float maxX, out float maxY)
        {
            minX = 0.34f;
            minY = 0.56f;
            maxX = 0.66f;
            maxY = 0.62f;
        }

        public static void Choice(int slot, out float minX, out float minY, out float maxX, out float maxY)
        {
            if (slot == RunSetupNav.DailySlot)
            {
                minX = 0.52f;
                minY = 0.42f;
                maxX = 0.66f;
                maxY = 0.54f;
                return;
            }

            if (slot == RunSetupNav.CancelSlot)
            {
                minX = 0.40f;
                minY = 0.32f;
                maxX = 0.60f;
                maxY = 0.40f;
                return;
            }

            minX = 0.34f;
            minY = 0.42f;
            maxX = 0.50f;
            maxY = 0.54f;
        }
    }
}
