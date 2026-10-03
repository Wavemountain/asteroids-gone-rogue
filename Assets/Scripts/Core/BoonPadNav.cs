namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Three-card boon row. Left and right only. Not part of the hangar grid,
    /// so doctrine-space reachability stays on HangarPadNav.
    /// </summary>
    public static class BoonPadNav
    {
        public static int Clamp(int slot, int count)
        {
            if (count < 1)
            {
                return 0;
            }

            if (slot < 0)
            {
                return 0;
            }

            if (slot >= count)
            {
                return count - 1;
            }

            return slot;
        }

        public static int Step(int slot, int dx, int count)
        {
            int current = Clamp(slot, count);
            if (count < 1 || dx == 0)
            {
                return current;
            }

            int next = dx > 0 ? current + 1 : current - 1;
            return Clamp(next, count);
        }
    }
}
