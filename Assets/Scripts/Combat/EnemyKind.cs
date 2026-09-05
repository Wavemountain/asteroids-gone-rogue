namespace AsteroidsGoneRogue
{
    public enum EnemyKind
    {
        Mid01,
        Scout,
        Gunner,
        Drone
    }

    public static class EnemyCatalog
    {
        public static string VisualName(EnemyKind kind)
        {
            switch (kind)
            {
                case EnemyKind.Scout:
                    return "Enemy_Scout";
                case EnemyKind.Gunner:
                    return "Enemy_Gunner";
                case EnemyKind.Drone:
                    return "Enemy_Drone";
                default:
                    return "Enemy_01";
            }
        }

        public static int HitPoints(EnemyKind kind)
        {
            switch (kind)
            {
                case EnemyKind.Scout:
                    return 2;
                case EnemyKind.Gunner:
                    return 5;
                case EnemyKind.Drone:
                    return 2;
                default:
                    return 3;
            }
        }

        public static float Speed(EnemyKind kind)
        {
            switch (kind)
            {
                case EnemyKind.Scout:
                    return 10.5f;
                case EnemyKind.Gunner:
                    return 4.2f;
                case EnemyKind.Drone:
                    return 8.2f;
                default:
                    return 6.5f;
            }
        }

        public static float TurnDegreesPerSecond(EnemyKind kind)
        {
            switch (kind)
            {
                case EnemyKind.Scout:
                    return 320f;
                case EnemyKind.Gunner:
                    return 140f;
                case EnemyKind.Drone:
                    return 260f;
                default:
                    return 220f;
            }
        }

        public static int Score(EnemyKind kind)
        {
            switch (kind)
            {
                case EnemyKind.Scout:
                    return 40;
                case EnemyKind.Gunner:
                    return 80;
                case EnemyKind.Drone:
                    return 35;
                default:
                    return ScoreValues.Enemy;
            }
        }

        public static float ColliderRadius(EnemyKind kind)
        {
            switch (kind)
            {
                case EnemyKind.Scout:
                    return 0.32f;
                case EnemyKind.Gunner:
                    return 0.7f;
                case EnemyKind.Drone:
                    return 0.28f;
                default:
                    return 0.45f;
            }
        }

        public static float ColliderHeight(EnemyKind kind)
        {
            switch (kind)
            {
                case EnemyKind.Scout:
                    return 1.3f;
                case EnemyKind.Gunner:
                    return 3f;
                case EnemyKind.Drone:
                    return 1.1f;
                default:
                    return ContentFactory.EnemyMeters;
            }
        }

        public static EnemyKind FromVisual(string visualName)
        {
            if (visualName == "Enemy_Scout")
            {
                return EnemyKind.Scout;
            }

            if (visualName == "Enemy_Gunner")
            {
                return EnemyKind.Gunner;
            }

            if (visualName == "Enemy_Drone")
            {
                return EnemyKind.Drone;
            }

            return EnemyKind.Mid01;
        }
    }
}
