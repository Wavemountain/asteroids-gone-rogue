namespace AsteroidsGoneRogue.EditorTools
{
    /// <summary>
    /// Aim, fire, and evade for the long-run harness. a0abecd has no separate
    /// autopilot type, so this policy lives here and is applied only by writing
    /// the axes ShipController already reads. It does not skip waves or teleport.
    /// </summary>
    public static class LongRunPilot
    {
        public const int ThreatCap = 48;
        public const float EvadeRadius = 14f;
        public const float Standoff = 7f;
        public const float FireRange = 26f;
        public const float FireAlign = 0.55f;
        public const float EdgePush = 8f;

        public struct Threat
        {
            public float X;
            public float Z;
        }

        public struct Stick
        {
            public float MoveX;
            public float MoveY;
            public float AimX;
            public float AimY;
            public bool Fire;
        }

        public static Stick Decide(
            float shipX,
            float shipZ,
            float forwardX,
            float forwardZ,
            Threat[] threats,
            int threatCount,
            float distanceToEdge)
        {
            Stick stick = new Stick();
            int count = threatCount;
            if (threats == null)
            {
                count = 0;
            }
            else if (count > threats.Length)
            {
                count = threats.Length;
            }

            if (count < 0)
            {
                count = 0;
            }

            float nearest = 1000000f;
            float aimX = forwardX;
            float aimZ = forwardZ;
            bool haveAim = (forwardX * forwardX) + (forwardZ * forwardZ) > 0.0001f;
            float evadeX = 0f;
            float evadeZ = 0f;
            for (int index = 0; index < count; index++)
            {
                float dx = shipX - threats[index].X;
                float dz = shipZ - threats[index].Z;
                float distSq = (dx * dx) + (dz * dz);
                if (distSq < 0.04f)
                {
                    distSq = 0.04f;
                }

                float dist = (float)System.Math.Sqrt(distSq);
                if (dist < nearest)
                {
                    nearest = dist;
                    aimX = threats[index].X - shipX;
                    aimZ = threats[index].Z - shipZ;
                    haveAim = true;
                }

                if (dist < EvadeRadius)
                {
                    float weight = (EvadeRadius - dist) / distSq;
                    evadeX += dx * weight;
                    evadeZ += dz * weight;
                }
            }

            float moveX = evadeX;
            float moveZ = evadeZ;
            if (haveAim && nearest < 1000000f && nearest > Standoff)
            {
                moveX += aimX * 0.45f;
                moveZ += aimZ * 0.45f;
            }

            float originSq = (shipX * shipX) + (shipZ * shipZ);
            if (distanceToEdge < EdgePush && originSq > 0.25f)
            {
                float origin = (float)System.Math.Sqrt(originSq);
                float push = (EdgePush - distanceToEdge) / EdgePush;
                if (push < 1f)
                {
                    push = 1f;
                }

                moveX -= (shipX / origin) * (1.6f * push);
                moveZ -= (shipZ / origin) * (1.6f * push);
            }
            else if (!haveAim || count == 0)
            {
                if (originSq > 1f)
                {
                    float origin = (float)System.Math.Sqrt(originSq);
                    moveX -= shipX / origin;
                    moveZ -= shipZ / origin;
                }
                else
                {
                    moveX += 1f;
                }
            }

            float moveSq = (moveX * moveX) + (moveZ * moveZ);
            if (moveSq < 0.06f)
            {
                if (haveAim)
                {
                    moveX = -aimZ;
                    moveZ = aimX;
                    moveSq = (moveX * moveX) + (moveZ * moveZ);
                }
                else
                {
                    moveX = 1f;
                    moveZ = 0f;
                    moveSq = 1f;
                }
            }

            if (moveSq > 1f)
            {
                float moveLen = (float)System.Math.Sqrt(moveSq);
                moveX /= moveLen;
                moveZ /= moveLen;
            }

            stick.MoveX = moveX;
            stick.MoveY = moveZ;
            if (haveAim)
            {
                float aimSq = (aimX * aimX) + (aimZ * aimZ);
                if (aimSq > 0.0001f)
                {
                    float aimLen = (float)System.Math.Sqrt(aimSq);
                    stick.AimX = aimX / aimLen;
                    stick.AimY = aimZ / aimLen;
                }
            }

            if (!haveAim)
            {
                stick.AimX = stick.MoveX;
                stick.AimY = stick.MoveY;
            }

            float forwardSq = (forwardX * forwardX) + (forwardZ * forwardZ);
            float align = 1f;
            if (forwardSq > 0.0001f && haveAim)
            {
                float forwardLen = (float)System.Math.Sqrt(forwardSq);
                align = ((forwardX * stick.AimX) + (forwardZ * stick.AimY)) / forwardLen;
            }

            stick.Fire = nearest <= FireRange && align >= FireAlign;
            return stick;
        }
    }
}
