namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Screen anchors for the New Run chooser. Font 18 stays at least 12 px
    /// on the 1280×800 canvas scale. Mutator rows sit between the blurb and
    /// the Normal / Daily buttons.
    /// </summary>
    public static class RunSetupLayout
    {
        public const int Font = 18;
        public const float PanelMinX = 0.24f;
        public const float PanelMinY = 0.06f;
        public const float PanelMaxX = 0.76f;
        public const float PanelMaxY = 0.94f;

        public static void Header(out float minX, out float minY, out float maxX, out float maxY)
        {
            minX = 0.28f;
            minY = 0.86f;
            maxX = 0.72f;
            maxY = 0.92f;
        }

        public static void Blurb(out float minX, out float minY, out float maxX, out float maxY)
        {
            minX = 0.28f;
            minY = 0.80f;
            maxX = 0.72f;
            maxY = 0.85f;
        }

        public static void Choice(int slot, out float minX, out float minY, out float maxX, out float maxY)
        {
            if (RunSetupNav.IsMutator(slot))
            {
                int row = RunSetupNav.MutatorId(slot);
                if (row < 0)
                {
                    row = 0;
                }

                if (row >= MutatorCatalog.Count)
                {
                    row = MutatorCatalog.Count - 1;
                }

                float top = 0.78f - (row * 0.09f);
                minX = 0.28f;
                maxX = 0.72f;
                maxY = top;
                minY = top - 0.08f;
                return;
            }

            if (slot == RunSetupNav.DailySlot)
            {
                minX = 0.52f;
                minY = 0.14f;
                maxX = 0.72f;
                maxY = 0.23f;
                return;
            }

            if (slot == RunSetupNav.CancelSlot)
            {
                minX = 0.40f;
                minY = 0.07f;
                maxX = 0.60f;
                maxY = 0.12f;
                return;
            }

            minX = 0.28f;
            minY = 0.14f;
            maxX = 0.50f;
            maxY = 0.23f;
        }
    }
}
