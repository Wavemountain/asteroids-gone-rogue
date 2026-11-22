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
        public const float ShooterStandoff = 12f;
        public const float BossStandoff = 14f;
        public const float FireRange = 26f;
        public const float FireAlign = 0.55f;
        public const float EdgePush = 8f;
        public const float ReactionSeconds = 0.22f;
        public const float InputHoldSeconds = 0.20f;
        public const int KindBody = 0;
        public const int KindLarge = 1;
        public const int KindShot = 2;
        public const int RoleRock = 0;
        public const int RoleEnemy = 1;
        public const int RoleShooter = 2;
        public const int RoleGuardian = 3;

        public struct Threat
        {
            public float X;
            public float Z;
            public float Vx;
            public float Vz;
            public float Radius;
            public int Kind;
            public int Role;
        }

        public struct Stick
        {
            public float MoveX;
            public float MoveY;
            public float AimX;
            public float AimY;
            public bool Fire;
            public bool Utility;
            public string Maneuver;
            public string FireReason;
            public string Prio;
            public float StandoffDist;
        }

        public static int RoleOf(string name, int kind)
        {
            if (kind == KindShot || name == null)
            {
                return RoleRock;
            }

            if (name.IndexOf("WorldGuardian", System.StringComparison.Ordinal) >= 0)
            {
                return RoleGuardian;
            }

            if (name.IndexOf("Scout", System.StringComparison.Ordinal) >= 0
                || name.IndexOf("Drone", System.StringComparison.Ordinal) >= 0)
            {
                return RoleShooter;
            }

            if (name.StartsWith("Enemy", System.StringComparison.Ordinal) || name.StartsWith("Brute", System.StringComparison.Ordinal))
            {
                return RoleEnemy;
            }

            return RoleRock;
        }

        static int AimRank(int role, float dist)
        {
            if (role == RoleShooter)
            {
                return 0;
            }

            if (role == RoleEnemy)
            {
                return 1;
            }

            if (role == RoleRock && dist < EvadeRadius)
            {
                return 2;
            }

            if (role == RoleGuardian && dist < 10f)
            {
                return 3;
            }

            return 99;
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
            stick.FireReason = "no-target";
            stick.Prio = "none";
            stick.StandoffDist = -1f;
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
            bool imminent = false;
            bool bodyThreat = false;
            float closeX = shipX;
            float closeZ = shipZ;
            float closeDist = 1000000f;
            int closeRole = RoleRock;
            int aimRank = 99;
            bool seenShip = false;
            float shooterX = shipX;
            float shooterZ = shipZ;
            float shooterDist = 1000000f;
            bool haveDodge = false;
            float dodgeVx = 0f;
            float dodgeVz = 1f;
            float dodgeSpeed = 1f;
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
                float keep = Standoff;
                if (threat.Role == RoleShooter)
                {
                    keep = ShooterStandoff;
                }
                else if (threat.Role == RoleGuardian || threat.Kind == KindLarge)
                {
                    keep = BossStandoff;
                }
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
                            if (time < 0.25f)
                            {
                                imminent = true;
                            }

                            if (!haveDodge || time < 0.35f)
                            {
                                haveDodge = true;
                                dodgeVx = threat.Vx;
                                dodgeVz = threat.Vz;
                                dodgeSpeed = (float)System.Math.Sqrt(speedSq);
                            }
                        }
                    }
                }

                if (threat.Kind != KindShot)
                {
                    int rank = AimRank(threat.Role, dist);
                    if (rank < aimRank || (rank == aimRank && dist < nearest))
                    {
                        aimRank = rank;
                        nearest = dist;
                        aimX = threat.X - shipX;
                        aimZ = threat.Z - shipZ;
                        haveAim = true;
                        if (rank == 0)
                        {
                            stick.Prio = "shooter";
                        }
                        else if (rank == 1)
                        {
                            stick.Prio = "enemy";
                        }
                        else if (rank == 2)
                        {
                            stick.Prio = "rock";
                        }
                        else if (rank == 3)
                        {
                            stick.Prio = "guardian";
                        }
                    }

                    if (threat.Role == RoleShooter || threat.Role == RoleEnemy)
                    {
                        seenShip = true;
                    }

                    if (threat.Role == RoleShooter && dist < shooterDist)
                    {
                        shooterDist = dist;
                        shooterX = threat.X;
                        shooterZ = threat.Z;
                        stick.StandoffDist = dist;
                    }
                }

                if (threat.Kind != KindShot && dist < keep)
                {
                    bodyThreat = true;
                    if (dist < closeDist)
                    {
                        closeDist = dist;
                        closeX = threat.X;
                        closeZ = threat.Z;
                        closeRole = threat.Role;
                    }
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

            float moveX = evadeX;
            float moveZ = evadeZ;
            if (bodyThreat)
            {
                float awayX = shipX - closeX;
                float awayZ = shipZ - closeZ;
                float awayLen = (float)System.Math.Sqrt((awayX * awayX) + (awayZ * awayZ));
                if (awayLen < 0.001f)
                {
                    awayLen = 0.001f;
                }

                awayX /= awayLen;
                awayZ /= awayLen;
                float sideX = -awayZ;
                float sideZ = awayX;
                float inwardX = -shipX;
                float inwardZ = -shipZ;
                float sideScore = (sideX * (inwardX + evadeX)) + (sideZ * (inwardZ + evadeZ));
                if (sideScore < 0f)
                {
                    sideX = -sideX;
                    sideZ = -sideZ;
                }

                float awayWeight = closeDist < 6f || closeRole == RoleShooter ? 1.25f : 0.35f;
                float sideWeight = closeDist < 6f || closeRole == RoleShooter ? 0.4f : 1.1f;
                moveX = (sideX * sideWeight) + (awayX * awayWeight);
                moveZ = (sideZ * sideWeight) + (awayZ * awayWeight);
                stick.Maneuver = closeRole == RoleShooter ? "standoff" : "sidestep";
            }

            if (haveDodge && dodgeSpeed > 0.1f)
            {
                float sideX = dodgeVz / dodgeSpeed;
                float sideZ = -dodgeVx / dodgeSpeed;
                float awayX = shipX - shooterX;
                float awayZ = shipZ - shooterZ;
                float prefer = (sideX * awayX) + (sideZ * awayZ);
                if (shooterDist < 1000000f && prefer < 0f)
                {
                    sideX = -sideX;
                    sideZ = -sideZ;
                }

                if (distanceToEdge < EdgePush)
                {
                    float outward = (sideX * shipX) + (sideZ * shipZ);
                    if (outward > 0f)
                    {
                        sideX = -sideX;
                        sideZ = -sideZ;
                    }
                }

                moveX += sideX * 3.2f;
                moveZ += sideZ * 3.2f;
                if (stick.Maneuver == "hold" || stick.Maneuver == "sidestep")
                {
                    stick.Maneuver = "sidestep-shot";
                }
            }
            else if (!shotThreat && haveAim && nearest < 1000000f && nearest > Standoff)
            {
                moveX += aimX * 0.35f;
                moveZ += aimZ * 0.35f;
            }

            float originSq = (shipX * shipX) + (shipZ * shipZ);
            if (!seenShip)
            {
                if (originSq > 0.25f)
                {
                    float origin = (float)System.Math.Sqrt(originSq);
                    float outX = shipX / origin;
                    float outZ = shipZ / origin;
                    moveX += -outZ;
                    moveZ += outX;
                    if (distanceToEdge < EdgePush)
                    {
                        moveX -= outX;
                        moveZ -= outZ;
                    }
                }
                else
                {
                    moveX += 1f;
                }

                if (stick.Maneuver == "hold" || stick.Maneuver == "approach" || stick.Maneuver == "space")
                {
                    stick.Maneuver = "patrol";
                }
            }
            else if (distanceToEdge < EdgePush && originSq > 0.25f)
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

            if (nearest > FireRange)
            {
                stick.FireReason = "no-target";
            }
            else if (align < FireAlign)
            {
                stick.FireReason = "align";
            }
            else if (imminent)
            {
                stick.FireReason = "imminent-shot";
            }
            else
            {
                stick.FireReason = "ok";
            }

            stick.Fire = stick.FireReason == "ok";
            stick.Utility = bodyThreat;
            if (shotThreat)
            {
                stick.Maneuver = "dodge-projectile";
            }
            else if (bodyThreat)
            {
                stick.Maneuver = "sidestep";
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
