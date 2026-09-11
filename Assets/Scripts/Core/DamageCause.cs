namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Sources that can damage the player. Only causes that exist in play are listed.
    /// </summary>
    public enum DamageCause
    {
        Unknown,
        AsteroidCollision,
        EnemyContact,
        HazardContact
    }

    public static class DamageCauseText
    {
        public static string FailReason(DamageCause cause)
        {
            switch (cause)
            {
                case DamageCause.AsteroidCollision:
                    return Loc.T("fail.asteroid", "Asteroid collision");
                case DamageCause.EnemyContact:
                    return Loc.T("fail.enemy", "Enemy contact");
                case DamageCause.HazardContact:
                    return Loc.T("fail.hazard", "Arena hazard");
                default:
                    return Loc.T("fail.unknown", "Unknown cause");
            }
        }

        public static string PlayerFaultLine(string failReason)
        {
            string reason = string.IsNullOrEmpty(failReason)
                ? Loc.T("fail.unknown", "Unknown cause")
                : failReason;
            return Loc.Tf("fail.fault", "{0} — that was you. Loadout stays on Retry Wave.", reason);
        }

        public static string FailReason(DamageCause cause, EnemyKind kind)
        {
            if (cause == DamageCause.EnemyContact)
            {
                string name = Loc.T("enemy." + kind, kind.ToString());
                return Loc.Tf("fail.enemy_kind", "Enemy contact ({0})", name);
            }

            return FailReason(cause);
        }
    }
}
