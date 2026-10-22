namespace AsteroidsGoneRogue
{
    /// <summary>
    /// File names for the arena-edge capture. Unity-free so the list can be
    /// reported without the editor. Full and Reduce, seven worlds, one edge
    /// frame plus a four-frame fly-in.
    /// </summary>
    public static class ArenaEdgeShots
    {
        public const int WorldCount = 7;
        public const int FlyInFrames = 4;

        public static int Count
        {
            get { return WorldCount * 2 * (1 + FlyInFrames); }
        }

        public static string FileName(int index)
        {
            int count = Count;
            int slot = index;
            if (slot < 0)
            {
                slot = 0;
            }

            if (slot >= count)
            {
                slot = count - 1;
            }

            int perMode = 1 + FlyInFrames;
            int perWorld = perMode * 2;
            int world = (slot / perWorld) + 1;
            int rest = slot % perWorld;
            string mode = "full";
            if (rest >= perMode)
            {
                mode = "reduce";
                rest -= perMode;
            }

            if (rest == 0)
            {
                return "arena_w" + world.ToString() + "_" + mode + "_edge.png";
            }

            int frame = rest - 1;
            return "arena_w" + world.ToString() + "_" + mode + "_flyin_" + frame.ToString() + ".png";
        }
    }
}
