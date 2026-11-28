namespace AsteroidsGoneRogue.EditorTools
{
    /// <summary>
    /// Stick policy for the long-run harness. The harness applies it only by
    /// writing the axes ShipController already reads. Perception numbers, owned
    /// by the caller: samples are 220 ms old (inside the 200–250 ms human
    /// window) and a new stick is held for at least 200 ms.
    /// Kind 0 is a body, 1 is a large body (boss or big rock), 2 is a shot.
    /// Velocity is observed motion, not a private projectile field.
    /// </summary>
    public static class LongRunPilot
    {
        public const int ThreatCap = 48;
        public const float EvadeRadius = 14f;
        public const float Standoff = 8f;
        /// <summary>
        /// Scout, Drone, and Gunner bolts travel at 16 m/s (EnemyCatalog.BoltSpeed)
        /// and live 2.7 s (Projectile.Lifetime), so they fly about 43 m, past the
        /// 30 m arena. A player cannot stand outside that range. 16 m is one
        /// second of flight, longer than the 220 ms reaction plus the 200 ms hold.
        /// Sniper bolts are 22 m/s; 16 m is still 0.73 s of flight.
        /// </summary>
        public const float ShooterStandoff = 16f;
        public const float BossStandoff = 14f;
        public const float ContactGap = 6f;
        public const float BoltSpeed = 16f;
        public const float ContactHorizon = 1.05f;
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
            public float Score;
            public string Runner;
            public float RunnerScore;
            public float StandoffDist;
            public float CenterDist;
            public string Metric;
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
                || name.IndexOf("Drone", System.StringComparison.Ordinal) >= 0
                || name.IndexOf("Gunner", System.StringComparison.Ordinal) >= 0
                || name.IndexOf("Bomber", System.StringComparison.Ordinal) >= 0
                || name.IndexOf("Sniper", System.StringComparison.Ordinal) >= 0)
            {
                return RoleShooter;
            }

            if (name.StartsWith("Enemy", System.StringComparison.Ordinal)
                || name.IndexOf("Brute", System.StringComparison.Ordinal) >= 0
                || name.IndexOf("Swarm", System.StringComparison.Ordinal) >= 0)
            {
                return RoleEnemy;
            }

            return RoleRock;
        }

        public static string TypeName(int role)
        {
            if (role == RoleGuardian)
            {
                return "guardian";
            }

            if (role == RoleShooter)
            {
                return "shooter";
            }

            if (role == RoleEnemy)
            {
                return "enemy";
            }

            return "rock";
        }

        public static void FillBoltVelocity(Threat[] threats, int count, float shipX, float shipZ)
        {
            if (threats == null || count <= 0)
            {
                return;
            }

            int limit = count < threats.Length ? count : threats.Length;
            for (int index = 0; index < limit; index++)
            {
                if (threats[index].Kind != KindShot)
                {
                    continue;
                }

                float speedSq = (threats[index].Vx * threats[index].Vx) + (threats[index].Vz * threats[index].Vz);
                if (speedSq > 0.25f)
                {
                    continue;
                }

                float originX = shipX;
                float originZ = shipZ;
                bool found = false;
                float best = 1000000f;
                for (int body = 0; body < limit; body++)
                {
                    if (threats[body].Kind == KindShot)
                    {
                        continue;
                    }

                    if (threats[body].Role != RoleShooter && threats[body].Role != RoleGuardian)
                    {
                        continue;
                    }

                    float dx = threats[index].X - threats[body].X;
                    float dz = threats[index].Z - threats[body].Z;
                    float distSq = (dx * dx) + (dz * dz);
                    if (distSq < 0.04f || distSq >= best)
                    {
                        continue;
                    }

                    best = distSq;
                    originX = threats[body].X;
                    originZ = threats[body].Z;
                    found = true;
                }

                float dirX = found ? threats[index].X - originX : shipX - threats[index].X;
                float dirZ = found ? threats[index].Z - originZ : shipZ - threats[index].Z;
                float len = (float)System.Math.Sqrt((dirX * dirX) + (dirZ * dirZ));
                if (len < 0.05f)
                {
                    dirX = 0f;
                    dirZ = -1f;
                    len = 1f;
                }

                threats[index].Vx = dirX / len * BoltSpeed;
                threats[index].Vz = dirZ / len * BoltSpeed;
            }
        }

        static float ThreatScore(int role, float surface, float closingSpeed, bool shotInFlight)
        {
            float score = 12f / (1f + surface);
            if (role == RoleGuardian)
            {
                score += 80f;
            }
            else if (role == RoleShooter)
            {
                score += 42f;
            }
            else if (role == RoleEnemy)
            {
                score += 24f;
            }
            else
            {
                score += 3f;
            }

            if (role == RoleShooter || role == RoleGuardian)
            {
                score += 18f;
                if (shotInFlight)
                {
                    score += 12f;
                }
            }

            bool imminentHit = false;
            if (closingSpeed > 1f)
            {
                float ttc = surface / closingSpeed;
                if (ttc < 1.2f)
                {
                    score += (1.2f - ttc) * 30f;
                }

                if (ttc < 0.55f)
                {
                    imminentHit = true;
                }
            }

            if (role == RoleRock && imminentHit)
            {
                score += 130f;
            }
            else if (role == RoleEnemy && imminentHit)
            {
                score += 40f;
            }

            return score;
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
            stick.Runner = "none";
            stick.StandoffDist = -1f;
            stick.CenterDist = -1f;
            stick.Metric = "surface";
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

            FillBoltVelocity(threats, count, shipX, shipZ);
            bool shotInFlight = false;
            for (int shotIndex = 0; shotIndex < count; shotIndex++)
            {
                if (threats[shotIndex].Kind == KindShot)
                {
                    shotInFlight = true;
                    break;
                }
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
            float closeSurface = 1000000f;
            float closeTtc = 99f;
            float closeClosing = 0f;
            bool closeTouching = false;
            int closeRole = RoleRock;
            float bestScore = -1f;
            float runnerScore = -1f;
            bool anyBody = false;
            float dodgeTti = 99f;
            float shooterX = shipX;
            float shooterZ = shipZ;
            float shooterDist = 1000000f;
            float closeVx = 0f;
            float closeVz = 0f;
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
                float bodyRadius = threat.Radius > 0f ? threat.Radius : 0f;
                float surface = dist - bodyRadius;
                if (surface < 0f)
                {
                    surface = 0f;
                }

                float reach = ContactGap;
                if (threat.Kind == KindLarge || threat.Role == RoleGuardian)
                {
                    float bodySpeed = (float)System.Math.Sqrt((threat.Vx * threat.Vx) + (threat.Vz * threat.Vz));
                    reach = ContactGap + bodyRadius + (bodySpeed * 0.45f);
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
                        float tti = dist / (float)System.Math.Sqrt(speedSq);
                        if (closing > 0f && tti < 1.6f && miss < 8f)
                        {
                            shotThreat = true;
                            if (tti < 0.45f)
                            {
                                imminent = true;
                            }

                            if (!haveDodge || tti < dodgeTti)
                            {
                                haveDodge = true;
                                dodgeTti = tti;
                                dodgeVx = threat.Vx;
                                dodgeVz = threat.Vz;
                                dodgeSpeed = (float)System.Math.Sqrt(speedSq);
                            }
                        }
                    }
                }

                if (threat.Kind != KindShot)
                {
                    anyBody = true;
                    bool ranged = threat.Role == RoleShooter || threat.Role == RoleGuardian;
                    float along = ((shipX - threat.X) * threat.Vx) + ((shipZ - threat.Z) * threat.Vz);
                    float closingSpeed = along > 0f ? along / dist : 0f;
                    bool touching = surface < 1.5f
                        || surface < reach
                        || (closingSpeed > 1f && (surface / closingSpeed) < ContactHorizon);
                    float ttc = 99f;
                    if (closingSpeed > 1f)
                    {
                        ttc = surface / closingSpeed;
                    }
                    else if (surface < 1.5f)
                    {
                        ttc = 0.05f;
                    }
                    else if (touching)
                    {
                        ttc = 1.5f;
                    }

                    float score = ThreatScore(threat.Role, surface, closingSpeed, shotInFlight);
                    string typeName = TypeName(threat.Role);
                    if (score > bestScore)
                    {
                        if (bestScore >= 0f)
                        {
                            runnerScore = bestScore;
                            stick.RunnerScore = stick.Score;
                            stick.Runner = stick.Prio;
                        }

                        bestScore = score;
                        nearest = surface;
                        aimX = threat.X - shipX;
                        aimZ = threat.Z - shipZ;
                        haveAim = true;
                        stick.Score = score;
                        stick.Prio = typeName;
                    }
                    else if (score > runnerScore)
                    {
                        runnerScore = score;
                        stick.RunnerScore = score;
                        stick.Runner = typeName;
                    }

                    if (ranged && (stick.StandoffDist < 0f || surface < stick.StandoffDist))
                    {
                        stick.StandoffDist = surface;
                        stick.CenterDist = dist;
                    }

                    if (threat.Role == RoleShooter && dist < shooterDist)
                    {
                        shooterDist = dist;
                        shooterX = threat.X;
                        shooterZ = threat.Z;
                    }

                    float band = threat.Role == RoleGuardian ? BossStandoff : ShooterStandoff;
                    bool inBand = ranged && surface < band;
                    if (touching || inBand)
                    {
                        bodyThreat = true;
                        bool take = false;
                        if (touching && (!closeTouching || ttc < closeTtc))
                        {
                            take = true;
                        }
                        else if (!touching && !closeTouching && surface < closeSurface)
                        {
                            take = true;
                        }

                        if (take)
                        {
                            closeTouching = touching;
                            closeTtc = touching ? ttc : closeTtc;
                            closeSurface = surface;
                            closeClosing = closingSpeed;
                            closeX = threat.X;
                            closeZ = threat.Z;
                            closeVx = threat.Vx;
                            closeVz = threat.Vz;
                            closeRole = threat.Role;
                        }
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

            bool inContact = closeTouching;
            float moveX = evadeX * 0.25f;
            float moveZ = evadeZ * 0.25f;
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
                float closeSpeedSq = (closeVx * closeVx) + (closeVz * closeVz);
                float closing = ((shipX - closeX) * closeVx) + ((shipZ - closeZ) * closeVz);
                if (closing > 0.5f && closeSpeedSq > 0.25f && inContact)
                {
                    float closeSpeed = (float)System.Math.Sqrt(closeSpeedSq);
                    sideX = closeVz / closeSpeed;
                    sideZ = -closeVx / closeSpeed;
                }

                float inwardX = -shipX;
                float inwardZ = -shipZ;
                float sideScore = (sideX * (awayX + inwardX)) + (sideZ * (awayZ + inwardZ));
                if (sideScore < 0f)
                {
                    sideX = -sideX;
                    sideZ = -sideZ;
                }

                float awayWeight = 0.45f;
                float sideWeight = 1.05f;
                if (inContact && closeClosing > 6f)
                {
                    awayWeight = 0.35f;
                    sideWeight = 1.85f;
                }
                else if (inContact)
                {
                    awayWeight = 1.1f;
                    sideWeight = 0.9f;
                }
                else if (closeRole == RoleShooter || closeRole == RoleGuardian)
                {
                    awayWeight = 0.7f;
                    sideWeight = 1.7f;
                }

                moveX += (sideX * sideWeight) + (awayX * awayWeight);
                moveZ += (sideZ * sideWeight) + (awayZ * awayWeight);
            }

            if (haveDodge && dodgeSpeed > 0.1f)
            {
                float sideX = dodgeVz / dodgeSpeed;
                float sideZ = -dodgeVx / dodgeSpeed;
                float keepSide = SideScore(sideX, sideZ, shipX, shipZ, distanceToEdge, threats, count, shooterX, shooterZ, shooterDist);
                float flipSide = SideScore(-sideX, -sideZ, shipX, shipZ, distanceToEdge, threats, count, shooterX, shooterZ, shooterDist);
                if (flipSide > keepSide)
                {
                    sideX = -sideX;
                    sideZ = -sideZ;
                }

                float shotWeight = imminent ? 2.2f : 0.85f;
                moveX += sideX * shotWeight;
                moveZ += sideZ * shotWeight;
            }
            else if (!shotThreat && !inContact && haveAim && nearest < 1000000f && nearest > Standoff)
            {
                float aimLen = (float)System.Math.Sqrt((aimX * aimX) + (aimZ * aimZ));
                if (aimLen > 0.001f)
                {
                    moveX += (aimX / aimLen) * 0.35f;
                    moveZ += (aimZ / aimLen) * 0.35f;
                }
            }

            float originSq = (shipX * shipX) + (shipZ * shipZ);
            if (!anyBody)
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
            else if (haveDodge && dodgeTti < 0.22f)
            {
                stick.FireReason = "imminent-shot";
            }
            else
            {
                stick.FireReason = "ok";
            }

            stick.Fire = stick.FireReason == "ok";
            stick.Utility = bodyThreat;
            bool shotSooner = haveDodge && (!inContact || dodgeTti + 0.02f < closeTtc);
            if (shotSooner)
            {
                stick.Maneuver = "sidestep-shot";
            }
            else if (inContact)
            {
                stick.Maneuver = "contact";
            }
            else if (bodyThreat && (closeRole == RoleShooter || closeRole == RoleGuardian))
            {
                stick.Maneuver = "standoff";
            }
            else if (bodyThreat)
            {
                stick.Maneuver = "sidestep";
            }
            else if (!anyBody)
            {
                stick.Maneuver = "patrol";
                stick.Prio = "patrol";
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

        static float SideScore(
            float sideX,
            float sideZ,
            float shipX,
            float shipZ,
            float distanceToEdge,
            Threat[] threats,
            int count,
            float shooterX,
            float shooterZ,
            float shooterDist)
        {
            float score = 0f;
            if (shooterDist < 1000000f)
            {
                score += (sideX * (shipX - shooterX)) + (sideZ * (shipZ - shooterZ));
            }

            if (distanceToEdge < EdgePush)
            {
                float outward = (sideX * shipX) + (sideZ * shipZ);
                if (outward > 0f)
                {
                    score -= 4f;
                }
            }

            if (threats == null || count <= 0)
            {
                return score;
            }

            float nextX = shipX + (sideX * 4f);
            float nextZ = shipZ + (sideZ * 4f);
            int limit = count < threats.Length ? count : threats.Length;
            for (int index = 0; index < limit; index++)
            {
                Threat threat = threats[index];
                if (threat.Kind == KindShot)
                {
                    continue;
                }

                float nowX = threat.X - shipX;
                float nowZ = threat.Z - shipZ;
                float laterX = threat.X - nextX;
                float laterZ = threat.Z - nextZ;
                float now = (nowX * nowX) + (nowZ * nowZ);
                float later = (laterX * laterX) + (laterZ * laterZ);
                if (later + 0.2f < now)
                {
                    score -= 6f;
                }

                float futureX = threat.X + (threat.Vx * 0.45f);
                float futureZ = threat.Z + (threat.Vz * 0.45f);
                float nowFx = futureX - shipX;
                float nowFz = futureZ - shipZ;
                float laterFx = futureX - nextX;
                float laterFz = futureZ - nextZ;
                float nowFuture = (nowFx * nowFx) + (nowFz * nowFz);
                float laterFuture = (laterFx * laterFx) + (laterFz * laterFz);
                if (laterFuture + 0.2f < nowFuture)
                {
                    score -= 6f;
                }
            }

            return score;
        }

        public static string SelfCheck()
        {
            Threat[] scout = new Threat[1];
            scout[0].X = 0f;
            scout[0].Z = 9f;
            scout[0].Kind = KindBody;
            scout[0].Role = RoleShooter;
            scout[0].Radius = 0.5f;
            Stick standoff = Decide(0f, 0f, 0f, 1f, scout, 1, 30f);
            if (standoff.Maneuver != "standoff" || standoff.Prio != "shooter" || standoff.MoveX > -0.5f && standoff.MoveX < 0.5f)
            {
                return "standoff " + standoff.Maneuver + " " + standoff.Prio;
            }

            Threat[] inside = new Threat[2];
            inside[0].X = 0f;
            inside[0].Z = 10f;
            inside[0].Kind = KindBody;
            inside[0].Role = RoleShooter;
            inside[0].Radius = 0.5f;
            inside[1].X = 0f;
            inside[1].Z = 8f;
            inside[1].Vx = 0f;
            inside[1].Vz = -16f;
            inside[1].Kind = KindShot;
            inside[1].Radius = 0.4f;
            Stick insideShot = Decide(0f, 0f, 0f, 1f, inside, 2, 30f);
            if (insideShot.Maneuver != "sidestep-shot")
            {
                return "shot-inside-standoff " + insideShot.Maneuver;
            }

            Threat[] blend = new Threat[2];
            blend[0].X = 4.4f;
            blend[0].Z = 0f;
            blend[0].Kind = KindBody;
            blend[0].Role = RoleRock;
            blend[0].Radius = 1.2f;
            blend[1].X = 0f;
            blend[1].Z = 8f;
            blend[1].Vx = 0f;
            blend[1].Vz = -16f;
            blend[1].Kind = KindShot;
            blend[1].Radius = 0.4f;
            Stick blended = Decide(0f, 0f, 0f, 1f, blend, 2, 30f);
            if (blended.Maneuver != "sidestep-shot" || blended.MoveX >= 0f)
            {
                return "blend " + blended.Maneuver + " " + blended.MoveX.ToString("0.00");
            }

            Threat[] skin = new Threat[1];
            skin[0].X = 0f;
            skin[0].Z = 7.5f;
            skin[0].Kind = KindBody;
            skin[0].Role = RoleRock;
            skin[0].Radius = 1.6f;
            Stick surface = Decide(0f, 0f, 0f, 1f, skin, 1, 30f);
            if (surface.Maneuver != "contact" || surface.Metric != "surface")
            {
                return "surface " + surface.Maneuver + " " + surface.StandoffDist.ToString("0.00");
            }

            Threat[] large = new Threat[1];
            large[0].X = 0f;
            large[0].Z = 12f;
            large[0].Vz = -8f;
            large[0].Kind = KindLarge;
            large[0].Role = RoleGuardian;
            large[0].Radius = 3f;
            Stick early = Decide(0f, 0f, 0f, 1f, large, 1, 30f);
            if (early.Maneuver != "contact")
            {
                return "large " + early.Maneuver;
            }

            Threat[] farRock = new Threat[1];
            farRock[0].X = 0f;
            farRock[0].Z = 12f;
            farRock[0].Kind = KindBody;
            farRock[0].Role = RoleRock;
            farRock[0].Radius = 0.5f;
            Stick notEarly = Decide(0f, 0f, 0f, 1f, farRock, 1, 30f);
            if (notEarly.Maneuver == "contact")
            {
                return "small-not-early " + notEarly.Maneuver;
            }

            Threat[] shot = new Threat[1];
            shot[0].X = 0f;
            shot[0].Z = 8f;
            shot[0].Vx = 0f;
            shot[0].Vz = -16f;
            shot[0].Kind = KindShot;
            shot[0].Radius = 0.4f;
            Stick dodge = Decide(0f, 0f, 0f, 1f, shot, 1, 30f);
            if (dodge.Maneuver != "sidestep-shot")
            {
                return "sidestep-shot " + dodge.Maneuver + " " + dodge.Prio;
            }

            Threat[] fresh = new Threat[1];
            fresh[0].X = 0f;
            fresh[0].Z = 8f;
            fresh[0].Kind = KindShot;
            fresh[0].Radius = 0.4f;
            Stick firstBolt = Decide(0f, 0f, 0f, 1f, fresh, 1, 30f);
            if (firstBolt.Maneuver != "sidestep-shot")
            {
                return "single-sample " + firstBolt.Maneuver;
            }

            Threat[] brute = new Threat[1];
            brute[0].X = 0f;
            brute[0].Z = 2.1f;
            brute[0].Vz = -6f;
            brute[0].Kind = KindLarge;
            brute[0].Role = RoleEnemy;
            brute[0].Radius = 1.2f;
            Stick contact = Decide(0f, 0f, 0f, 1f, brute, 1, 30f);
            if (contact.Maneuver != "contact" || contact.Prio != "enemy")
            {
                return "contact " + contact.Maneuver + " " + contact.Prio;
            }

            Threat[] fast = new Threat[1];
            fast[0].X = 0f;
            fast[0].Z = 8f;
            fast[0].Vz = -10.5f;
            fast[0].Kind = KindBody;
            fast[0].Role = RoleShooter;
            fast[0].Radius = 0.5f;
            Stick earlyScout = Decide(0f, 0f, 0f, 1f, fast, 1, 30f);
            if (earlyScout.Maneuver != "contact" || !earlyScout.Fire || earlyScout.MoveX > -0.4f && earlyScout.MoveX < 0.4f)
            {
                return "fast-scout " + earlyScout.Maneuver + " fire=" + earlyScout.Fire.ToString() + " x=" + earlyScout.MoveX.ToString("0.00");
            }

            Threat[] rock = new Threat[1];
            rock[0].X = 0f;
            rock[0].Z = 10f;
            rock[0].Kind = KindBody;
            rock[0].Role = RoleRock;
            rock[0].Radius = 0.8f;
            Stick onlyRock = Decide(0f, 0f, 0f, 1f, rock, 1, 30f);
            if (onlyRock.Maneuver == "standoff" || onlyRock.StandoffDist >= 0f)
            {
                return "rock-standoff " + onlyRock.Maneuver + " " + onlyRock.StandoffDist.ToString("0.00");
            }

            Threat[] crowd = new Threat[2];
            crowd[0].X = 12f;
            crowd[0].Z = 0f;
            crowd[0].Kind = KindBody;
            crowd[0].Role = RoleRock;
            crowd[0].Radius = 1f;
            crowd[1].X = 0f;
            crowd[1].Z = 16f;
            crowd[1].Kind = KindLarge;
            crowd[1].Role = RoleGuardian;
            crowd[1].Radius = 2f;
            Stick bossAim = Decide(0f, 0f, 0f, 1f, crowd, 2, 30f);
            if (bossAim.Prio != "guardian" || bossAim.AimY <= bossAim.AimX)
            {
                return "boss-aim " + bossAim.Prio + " " + bossAim.AimX.ToString("0.00") + "," + bossAim.AimY.ToString("0.00");
            }

            Stick patrol = Decide(0f, 0f, 0f, 1f, null, 0, 30f);
            if (patrol.Maneuver != "patrol" || patrol.Prio != "patrol")
            {
                return "patrol " + patrol.Maneuver + " " + patrol.Prio;
            }

            return string.Empty;
        }
    }
}
