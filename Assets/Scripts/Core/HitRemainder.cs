namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Unused percent (0–99) carried between hits. Clear at every run and wave
    /// configure so a daily replay starts from the same fraction as a fresh ship.
    /// </summary>
    public struct HitRemainder
    {
        public int Value;

        public void Clear()
        {
            Value = 0;
        }
    }
}
