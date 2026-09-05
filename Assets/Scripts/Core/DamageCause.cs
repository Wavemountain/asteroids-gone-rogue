namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Sources that can damage the player. Only causes that exist in play are listed.
    /// </summary>
    public enum DamageCause
    {
        Unknown,
        AsteroidCollision,
        EnemyContact
    }

    public static class DamageCauseText
    {
        public static string FailReason(DamageCause cause)
        {
            switch (cause)
            {
                case DamageCause.AsteroidCollision:
                    return "Asteroid collision";
                case DamageCause.EnemyContact:
                    return "Enemy contact";
                default:
                    return "Unknown cause";
            }
        }
    }
}
