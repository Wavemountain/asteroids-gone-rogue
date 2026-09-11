namespace AsteroidsGoneRogue
{
    public enum EnemyKind
    {
        Mid01,
        Scout,
        Gunner,
        Drone,
        Bomber,
        Sniper,
        SwarmPod,
        Brute,
        Swarm,
        Swarmling
    }

    public static class EnemyCatalog
    {
        public const float BruteChargeRange = 13f;
        public const float BruteChargeSpeed = 15f;
        public const float BruteChargeTurn = 210f;
        public const float BruteChargeSeconds = 0.95f;
        public const float BruteRestSeconds = 2.4f;
        public const float NestSpawnSeconds = 4.8f;
        public const int NestMaxMinions = 3;

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
                case EnemyKind.Bomber:
                    return "Enemy_Bomber";
                case EnemyKind.Sniper:
                    return "Enemy_Sniper";
                case EnemyKind.SwarmPod:
                    return "Enemy_SwarmPod";
                case EnemyKind.Brute:
                    return "Monster_Brute";
                case EnemyKind.Swarm:
                    return "Monster_Swarm";
                case EnemyKind.Swarmling:
                    return "Monster_Swarmling";
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
                    return 4;
                case EnemyKind.Drone:
                    return 2;
                case EnemyKind.Bomber:
                    return 5;
                case EnemyKind.Sniper:
                    return 3;
                case EnemyKind.SwarmPod:
                    return 2;
                case EnemyKind.Brute:
                    return 8;
                case EnemyKind.Swarm:
                    return 6;
                case EnemyKind.Swarmling:
                    return 1;
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
                case EnemyKind.Bomber:
                    return 3.6f;
                case EnemyKind.Sniper:
                    return 5.4f;
                case EnemyKind.SwarmPod:
                    return 9.4f;
                case EnemyKind.Brute:
                    return 3.4f;
                case EnemyKind.Swarm:
                    return 1.6f;
                case EnemyKind.Swarmling:
                    return 11.5f;
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
                case EnemyKind.Bomber:
                    return 110f;
                case EnemyKind.Sniper:
                    return 180f;
                case EnemyKind.SwarmPod:
                    return 300f;
                case EnemyKind.Brute:
                    return 90f;
                case EnemyKind.Swarm:
                    return 80f;
                case EnemyKind.Swarmling:
                    return 340f;
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
                case EnemyKind.Bomber:
                    return 90;
                case EnemyKind.Sniper:
                    return 70;
                case EnemyKind.SwarmPod:
                    return 30;
                case EnemyKind.Brute:
                    return 120;
                case EnemyKind.Swarm:
                    return 100;
                case EnemyKind.Swarmling:
                    return 12;
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
                case EnemyKind.Bomber:
                    return 0.85f;
                case EnemyKind.Sniper:
                    return 0.4f;
                case EnemyKind.SwarmPod:
                    return 0.26f;
                case EnemyKind.Brute:
                    return 1.05f;
                case EnemyKind.Swarm:
                    return 0.85f;
                case EnemyKind.Swarmling:
                    return 0.2f;
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
                case EnemyKind.Bomber:
                    return 3.2f;
                case EnemyKind.Sniper:
                    return 2.2f;
                case EnemyKind.SwarmPod:
                    return 1f;
                case EnemyKind.Brute:
                    return 3.8f;
                case EnemyKind.Swarm:
                    return 2.6f;
                case EnemyKind.Swarmling:
                    return 0.75f;
                default:
                    return ContentFactory.EnemyMeters;
            }
        }

        public static EnemyKind FromVisual(string visualName)
        {
            if (visualName == "Enemy_Scout"
                || visualName == "Enemy_Scout_Buffer_v7"
                || visualName == "Enemy_Scout_Buffer_v6"
                || visualName == "Enemy_Scout_Buffer_v5"
                || visualName == "Enemy_Scout_Buffer_v4")
            {
                return EnemyKind.Scout;
            }

            if (visualName == "Enemy_Gunner"
                || visualName == "Enemy_Gunner_Buffer_v7"
                || visualName == "Enemy_Gunner_Buffer_v6"
                || visualName == "Enemy_Gunner_Buffer_v5"
                || visualName == "Enemy_Gunner_Buffer_v4")
            {
                return EnemyKind.Gunner;
            }

            if (visualName == "Enemy_Drone"
                || visualName == "Enemy_Drone_Buffer_v6"
                || visualName == "Enemy_Drone_Buffer_v5"
                || visualName == "Enemy_Drone_Buffer_v4")
            {
                return EnemyKind.Drone;
            }

            if (visualName == "Enemy_Bomber"
                || visualName == "Enemy_Bomber_Buffer_v6"
                || visualName == "Enemy_Bomber_Buffer_v5")
            {
                return EnemyKind.Bomber;
            }

            if (visualName == "Enemy_Sniper"
                || visualName == "Enemy_Sniper_Buffer_v8"
                || visualName == "Enemy_Sniper_Buffer_v5")
            {
                return EnemyKind.Sniper;
            }

            if (visualName == "Enemy_SwarmPod" || visualName == "Enemy_SwarmPod_Buffer_v6")
            {
                return EnemyKind.SwarmPod;
            }

            if (visualName == "Monster_Brute")
            {
                return EnemyKind.Brute;
            }

            if (visualName == "Monster_Swarmling")
            {
                return EnemyKind.Swarmling;
            }

            if (visualName == "Monster_Swarm")
            {
                return EnemyKind.Swarm;
            }

            if (visualName == "Enemy_01" || visualName == "Enemy_01_Buffer_v8")
            {
                return EnemyKind.Mid01;
            }

            return EnemyKind.Mid01;
        }

        public static bool RequiresImportedMesh(EnemyKind kind)
        {
            return kind == EnemyKind.Bomber
                || kind == EnemyKind.SwarmPod;
        }

        public static bool IsMonster(EnemyKind kind)
        {
            return kind == EnemyKind.Brute || kind == EnemyKind.Swarm;
        }

        public static bool FiresBolts(EnemyKind kind)
        {
            return kind == EnemyKind.Gunner || kind == EnemyKind.Sniper;
        }

        public static float FireCooldown(EnemyKind kind)
        {
            return kind == EnemyKind.Sniper ? 2.2f : 1.35f;
        }

        public static float BoltSpeed(EnemyKind kind)
        {
            return kind == EnemyKind.Sniper ? 22f : 16f;
        }
    }
}
