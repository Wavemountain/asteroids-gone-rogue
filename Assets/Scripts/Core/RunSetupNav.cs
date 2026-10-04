namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Pad grid for the New Run chooser. Normal and Daily sit on one row.
    /// Back is the row below. Unity-free.
    /// </summary>
    public static class RunSetupNav
    {
        public const int NormalSlot = 0;
        public const int DailySlot = 1;
        public const int CancelSlot = 2;
        public const int SlotCount = 3;

        public static int DefaultSlot(RunSetupKind kind)
        {
            if (kind == RunSetupKind.Abandon)
            {
                return NormalSlot;
            }

            return DailySlot;
        }

        public static int Step(int slot, int dx, int dy)
        {
            int current = slot;
            if (current < NormalSlot)
            {
                current = NormalSlot;
            }

            if (current > CancelSlot)
            {
                current = CancelSlot;
            }

            if (dy > 0)
            {
                return CancelSlot;
            }

            if (dy < 0)
            {
                return current == CancelSlot ? NormalSlot : current;
            }

            if (dx > 0 && current == NormalSlot)
            {
                return DailySlot;
            }

            if (dx < 0 && current == DailySlot)
            {
                return NormalSlot;
            }

            return current;
        }

        public static bool SelfCheck()
        {
            return Step(NormalSlot, 1, 0) == DailySlot
                && Step(DailySlot, -1, 0) == NormalSlot
                && Step(NormalSlot, 0, 1) == CancelSlot
                && Step(DailySlot, 0, 1) == CancelSlot
                && Step(CancelSlot, 0, -1) == NormalSlot
                && Step(CancelSlot, 1, 0) == CancelSlot
                && DefaultSlot(RunSetupKind.Abandon) == NormalSlot
                && DefaultSlot(RunSetupKind.Retry) == DailySlot
                && DefaultSlot(RunSetupKind.Hangar) == DailySlot;
        }
    }

    public enum RunSetupKind
    {
        Abandon = 0,
        Retry = 1,
        Hangar = 2,
    }
}
