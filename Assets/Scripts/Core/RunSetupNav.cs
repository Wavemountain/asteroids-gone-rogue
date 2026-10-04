namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Pad grid for the New Run chooser. Normal and Daily sit on the bottom
    /// row, six mutators stack above them, and Back is under that row.
    /// Down the screen is positive dy. Unity-free.
    /// </summary>
    public static class RunSetupNav
    {
        public const int NormalSlot = 0;
        public const int DailySlot = 1;
        public const int CancelSlot = 2;
        public const int Mutator0 = 3;

        public static int SlotCount
        {
            get { return Mutator0 + MutatorCatalog.Count; }
        }

        public static int MutatorSlot(int id)
        {
            return Mutator0 + id;
        }

        public static bool IsMutator(int slot)
        {
            return slot >= Mutator0 && slot < SlotCount;
        }

        public static int MutatorId(int slot)
        {
            return slot - Mutator0;
        }

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

            if (current >= SlotCount)
            {
                current = SlotCount - 1;
            }

            if (dy > 0)
            {
                if (IsMutator(current))
                {
                    int downId = MutatorId(current);
                    if (downId >= MutatorCatalog.Count - 1)
                    {
                        return NormalSlot;
                    }

                    return MutatorSlot(downId + 1);
                }

                if (current == NormalSlot || current == DailySlot)
                {
                    return CancelSlot;
                }

                return current;
            }

            if (dy < 0)
            {
                if (current == CancelSlot)
                {
                    return NormalSlot;
                }

                if (current == NormalSlot || current == DailySlot)
                {
                    return MutatorSlot(MutatorCatalog.Count - 1);
                }

                if (IsMutator(current))
                {
                    int upId = MutatorId(current);
                    if (upId <= 0)
                    {
                        return current;
                    }

                    return MutatorSlot(upId - 1);
                }

                return current;
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
            int lastMutator = MutatorSlot(MutatorCatalog.Count - 1);
            return Step(NormalSlot, 1, 0) == DailySlot
                && Step(DailySlot, -1, 0) == NormalSlot
                && Step(NormalSlot, 0, 1) == CancelSlot
                && Step(DailySlot, 0, 1) == CancelSlot
                && Step(CancelSlot, 0, -1) == NormalSlot
                && Step(NormalSlot, 0, -1) == lastMutator
                && Step(lastMutator, 0, 1) == NormalSlot
                && Step(Mutator0, 0, 1) == MutatorSlot(1)
                && Step(MutatorSlot(1), 0, -1) == Mutator0
                && Step(Mutator0, 0, -1) == Mutator0
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
