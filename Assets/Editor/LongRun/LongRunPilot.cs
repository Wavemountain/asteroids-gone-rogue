namespace AsteroidsGoneRogue.EditorTools
{
    /// <summary>
    /// Stick policy for the long-run harness. The harness applies it only by
    /// writing the axes ShipController already reads. Perception numbers, owned
    /// by the caller: samples are 220 ms old (inside the 200–250 ms human
    /// window) and a new stick is held for at least 80 ms.
    /// Kind 0 is a body, 1 is a large body (boss or big rock), 2 is a shot.
    /// Velocity is observed motion, not a private projectile field.
    /// </summary>
    public static class LongRunPilot
    {
        public const int ThreatCap = 48;
        public const float EvadeRadius = 14f;
        public const float Standoff = 8f;
        public const float BossStandoff = 14f;
        public const float FireRange = 26f;
        public const float FireAlign = 0.55f;
        public const float EdgePush = 8f;
        public const float ReactionSeconds = 0.22f;
        public const float InputHoldSeconds = 0.08f;
        public const int KindBody = 0;
        public const int KindLarge = 1;
        public const int KindShot = 2;

        public struct Threat
        {
            public float X;
            public float Z;
            public float Vx;
            public float Vz;
            public float Radius;
            public int Kind;
        }

        public struct Stick
        {
            public float MoveX;
            public float MoveY;
            public float AimX;
            public float AimY;
            public bool Fire;
            public string Maneuver;
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
            stick.Maneuver = "hold";
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
            bool shotThreat = false;
            bool bodyThreat = false;
            for (int index = 0; index < count; index++)
            {
                Threat threat = threats[index];
                float dx = shipX - threat.X;
                float dz = shipZ - threat.Z;
                float distSq = (dx * dx) + (dz * dz);
                if (distSq < 0.04f)
                {
                    distSq = 0.04f;
                }

                float dist = (float)System.Math.Sqrt(distSq);
                float keep = threat.Kind == KindLarge ? BossStandoff : Standoff;
                if (threat.Radius > 0.2f)
                {
                    keep += threat.Radius;
                }

                if (threat.Kind == KindShot)
                {
                    float speedSq = (threat.Vx * threat.Vx) + (threat.Vz * threat.Vz);
                    if (speedSq > 0.25f)
                    {
                        float closing = ((shipX - threat.X) * threat.Vx) + ((shipZ - threat.Z) * threat.Vz);
                        float time = 0f;
                        if (closing > 0f)
                        {
                            time = closing / speedSq;
                        }

                        if (time > 1.2f)
                        {
                            time = 1.2f;
                        }

                        float missX = (threat.X + (threat.Vx * time)) - shipX;
                        float missZ = (threat.Z + (threat.Vz * time)) - shipZ;
                        float miss = (float)System.Math.Sqrt((missX * missX) + (missZ * missZ));
                        float hitRadius = threat.Radius + 1.1f;
                        if (hitRadius < 1.6f)
                        {
                            hitRadius = 1.6f;
                        }

                        if (miss < hitRadius && time < 1.2f)
                        {
                            shotThreat = true;
                            float side = (threat.Vz * dx) - (threat.Vx * dz);
                            float sign = side >= 0f ? 1f : -1f;
                            float speed = (float)System.Math.Sqrt(speedSq);
                            evadeX += (threat.Vz / speed) * sign * 4f;
                            evadeZ += (-threat.Vx / speed) * sign * 4f;
                        }
                    }
                }

                if (dist < nearest && threat.Kind != KindShot)
                {
                    nearest = dist;
                    aimX = threat.X - shipX;
                    aimZ = threat.Z - shipZ;
                    haveAim = true;
                }

                if (threat.Kind != KindShot && dist < keep)
                {
                    bodyThreat = true;
                }

                if (dist < EvadeRadius)
                {
                    float weight = (EvadeRadius - dist) / distSq;
                    if (threat.Kind == KindShot)
                    {
                        weight *= 0.35f;
                    }

                    evadeX += dx * weight;
                    evadeZ += dz * weight;
                }
            }

            bool threatened = shotThreat || bodyThreat;
            float moveX = evadeX;
            float moveZ = evadeZ;
            if (!threatened && haveAim && nearest < 1000000f && nearest > Standoff)
            {
                moveX += aimX * 0.35f;
                moveZ += aimZ * 0.35f;
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

            stick.Fire = !threatened && nearest <= FireRange && align >= FireAlign;
            if (shotThreat)
            {
                stick.Maneuver = "dodge-projectile";
            }
            else if (bodyThreat)
            {
                stick.Maneuver = "retreat";
            }
            else if (haveAim && nearest > Standoff)
            {
                stick.Maneuver = "approach";
            }
            else if (moveSq > 0.06f)
            {
                stick.Maneuver = "space";
            }

            return stick;
        }
    }
}
