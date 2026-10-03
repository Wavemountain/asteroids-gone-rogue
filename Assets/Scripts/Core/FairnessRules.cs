using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Spawn clearance for 0.47 Part B. Enemies, hazards, and bolts must not
    /// appear inside <see cref="MinSafeMetres"/> of the player. A candidate that
    /// is already clear is kept; otherwise it is pushed out along the line from
    /// the player (or +X when the points coincide). If a max arena radius is
    /// given and the pushed point falls outside it, the point is flipped to the
    /// opposite side of the player and then clamped.
    /// </summary>
    public static class SpawnClearance
    {
        public const float MinSafeMetres = 6f;
        public const float SpawnGraceSeconds = 0.45f;
        public const float WaveImmunitySeconds = 1f;

        public static bool IsSafe(float sx, float sz, float px, float pz, float minDistance)
        {
            float dx = sx - px;
            float dz = sz - pz;
            float minSq = minDistance * minDistance;
            return dx * dx + dz * dz + 0.00001f >= minSq;
        }

        public static void PushOut(
            float sx,
            float sz,
            float px,
            float pz,
            float minDistance,
            float maxRadius,
            out float ox,
            out float oz)
        {
            float dx = sx - px;
            float dz = sz - pz;
            float distSq = dx * dx + dz * dz;
            float minSq = minDistance * minDistance;
            if (distSq + 0.00001f >= minSq)
            {
                ox = sx;
                oz = sz;
            }
            else
            {
                float dist;
                if (distSq < 0.0001f)
                {
                    dx = 1f;
                    dz = 0f;
                    dist = 1f;
                }
                else
                {
                    dist = Mathf.Sqrt(distSq);
                }

                float scale = minDistance / dist;
                ox = px + dx * scale;
                oz = pz + dz * scale;
            }

            if (maxRadius <= 0f)
            {
                return;
            }

            float outSq = ox * ox + oz * oz;
            float maxSq = maxRadius * maxRadius;
            if (outSq <= maxSq)
            {
                return;
            }

            float awayX = ox - px;
            float awayZ = oz - pz;
            float awaySq = awayX * awayX + awayZ * awayZ;
            float away;
            if (awaySq < 0.0001f)
            {
                awayX = 1f;
                awayZ = 0f;
                away = 1f;
            }
            else
            {
                away = Mathf.Sqrt(awaySq);
            }

            ox = px - awayX / away * minDistance;
            oz = pz - awayZ / away * minDistance;
            outSq = ox * ox + oz * oz;
            if (outSq > maxSq && outSq > 0.0001f)
            {
                float clamp = maxRadius / Mathf.Sqrt(outSq);
                ox *= clamp;
                oz *= clamp;
            }
        }

        /// <summary>
        /// Deterministic ring pick. Several angles are tried from <paramref name="seed"/>;
        /// the first point at least <paramref name="minDistance"/> from the player wins.
        /// Otherwise the point is pushed out.
        /// </summary>
        public static void Choose(
            int seed,
            float radius,
            float px,
            float pz,
            float minDistance,
            out float ox,
            out float oz)
        {
            float minSq = minDistance * minDistance;
            for (int attempt = 0; attempt < 12; attempt++)
            {
                float angle = (seed * 0.618034f + attempt * 0.9f) * 6.2831853f;
                float sx = Mathf.Cos(angle) * radius;
                float sz = Mathf.Sin(angle) * radius;
                float dx = sx - px;
                float dz = sz - pz;
                if (dx * dx + dz * dz + 0.00001f >= minSq)
                {
                    ox = sx;
                    oz = sz;
                    return;
                }
            }

            PushOut(px + radius, pz, px, pz, minDistance, 0f, out ox, out oz);
        }

        public static Vector3 Place(Vector3 spawn, Vector3 player)
        {
            return Place(spawn, player, 0f);
        }

        public static Vector3 Place(Vector3 spawn, Vector3 player, float maxRadius)
        {
            float ox;
            float oz;
            PushOut(spawn.x, spawn.z, player.x, player.z, MinSafeMetres, maxRadius, out ox, out oz);
            return new Vector3(ox, spawn.y, oz);
        }
    }

    /// <summary>
    /// Assist mode numbers. Default off. Enemy and hazard damage is multiplied
    /// by 75 percent and never rounded below 1. Asteroids are unchanged.
    /// An assisted run does not count for the highscore board or purity medals.
    /// </summary>
    public static class AssistRules
    {
        public const int DamagePercent = 75;
        public const int MinDamage = 1;
        public const int BonusShield = 1;

        public static bool Affects(DamageCause cause)
        {
            return cause == DamageCause.EnemyContact
                || cause == DamageCause.EnemyBolt
                || cause == DamageCause.BossBolt
                || cause == DamageCause.HazardContact;
        }

        public static int ScaleIncoming(int amount, DamageCause cause, bool assist)
        {
            if (!assist || amount <= 0 || !Affects(cause))
            {
                return amount;
            }

            int scaled = amount * DamagePercent / 100;
            if (scaled < MinDamage)
            {
                scaled = MinDamage;
            }

            return scaled;
        }

        public static int BonusShieldAtWaveStart(int shield, int maxShield, bool assist)
        {
            if (!assist || BonusShield <= 0 || shield >= maxShield)
            {
                return shield;
            }

            int raised = shield + BonusShield;
            if (raised > maxShield)
            {
                raised = maxShield;
            }

            if (raised < 0)
            {
                raised = 0;
            }

            return raised;
        }

        public static bool CountsForBoard(bool assistUsed)
        {
            return !assistUsed;
        }
    }

    /// <summary>
    /// Full-screen hit flash: alpha at most 0.20, at most 3 Hz, and a hit that
    /// arrives while a flash is showing does not extend it.
    /// </summary>
    public static class HitFlashLimiter
    {
        public const float MinGapSeconds = 0.333f;
        public const float MaxAlpha = 0.20f;
        public const float DefaultDecay = 0.18f;

        public static bool TryBegin(
            float now,
            float lastStart,
            float currentUntil,
            float requestedAlpha,
            float decaySeconds,
            out float alpha,
            out float start,
            out float until)
        {
            alpha = 0f;
            start = lastStart;
            until = currentUntil;
            if (currentUntil > now)
            {
                return false;
            }

            if (lastStart > 0f && now - lastStart < MinGapSeconds)
            {
                return false;
            }

            float capped = requestedAlpha;
            if (capped > MaxAlpha)
            {
                capped = MaxAlpha;
            }

            if (capped < 0f)
            {
                capped = 0f;
            }

            if (capped <= 0.01f)
            {
                return false;
            }

            float decay = decaySeconds;
            if (decay < 0.05f)
            {
                decay = 0.05f;
            }

            alpha = capped;
            start = now;
            until = now + decay;
            return true;
        }

        public static bool TryBegin(
            float now,
            float lastStart,
            float currentUntil,
            float requestedAlpha,
            float decaySeconds,
            float alphaCap,
            float gapSeconds,
            out float alpha,
            out float start,
            out float until)
        {
            alpha = 0f;
            start = lastStart;
            until = currentUntil;
            if (currentUntil > now)
            {
                return false;
            }

            float gap = gapSeconds;
            if (gap < 0f)
            {
                gap = 0f;
            }

            if (lastStart > 0f && now - lastStart < gap)
            {
                return false;
            }

            float cap = alphaCap;
            if (cap < 0f)
            {
                cap = 0f;
            }

            float capped = requestedAlpha;
            if (capped > cap)
            {
                capped = cap;
            }

            if (capped < 0f)
            {
                capped = 0f;
            }

            if (capped <= 0.01f)
            {
                return false;
            }

            float decay = decaySeconds;
            if (decay < 0.05f)
            {
                decay = 0.05f;
            }

            alpha = capped;
            start = now;
            until = now + decay;
            return true;
        }
    }

    /// <summary>
    /// Spike proximity tick. Quiet beyond 3 m, 0.45 s outside 1.6 m, 0.25 s inside.
    /// Pitch goes from 1.0 at the edge to 1.25 on contact. Silent during i-frames.
    /// </summary>
    public static class SpikeProximity
    {
        public const float WarnMetres = 3f;
        public const float CloseMetres = 1.6f;
        public const float FarInterval = 0.45f;
        public const float CloseInterval = 0.25f;
        public const float GlowFar = 1f;
        public const float GlowNear = 1.3f;

        public static float IntervalFor(float distance)
        {
            if (distance < CloseMetres)
            {
                return CloseInterval;
            }

            return FarInterval;
        }

        public static float Pitch01(float distance)
        {
            if (distance >= WarnMetres)
            {
                return 0f;
            }

            if (distance <= 0f)
            {
                return 1f;
            }

            float t = 1f - distance / WarnMetres;
            if (t < 0f)
            {
                t = 0f;
            }

            if (t > 1f)
            {
                t = 1f;
            }

            return t;
        }

        public static bool ShouldTick(float now, float lastTick, float distance, bool invulnerable)
        {
            if (invulnerable)
            {
                return false;
            }

            if (distance >= WarnMetres)
            {
                return false;
            }

            float gap = IntervalFor(distance);
            if (lastTick > 0f && now - lastTick < gap)
            {
                return false;
            }

            return true;
        }

        public static float GlowMul(float distance)
        {
            if (distance >= WarnMetres)
            {
                return GlowFar;
            }

            return GlowFar + (GlowNear - GlowFar) * Pitch01(distance);
        }
    }

    /// <summary>
    /// Voice and gap rules for hit, tell, and pickup cues. Tells do not jitter.
    /// The oldest busy voice is the one a new cue may steal.
    /// </summary>
    public static class CueVoiceBudget
    {
        public const float SameClipGapSeconds = 0.045f;
        public const float PickupGapSeconds = 0.08f;
        public const float TellStaggerSeconds = 0.12f;
        public const int HitVoiceCap = 4;
        public const int TellVoiceCap = 2;
        public const int PickupVoiceCap = 2;

        public static bool GapReady(float now, float last, float gap)
        {
            if (gap <= 0f)
            {
                return true;
            }

            if (last <= 0f)
            {
                return true;
            }

            return now - last >= gap;
        }

        public static bool AllowTell(float now, float lastAny, float lastType, float typeGap)
        {
            if (!GapReady(now, lastType, typeGap))
            {
                return false;
            }

            if (!GapReady(now, lastAny, TellStaggerSeconds))
            {
                return false;
            }

            return true;
        }

        public static int Claim(float now, float[] started, float[] until)
        {
            int count = started != null ? started.Length : 0;
            if (count <= 0)
            {
                return 0;
            }

            int free = -1;
            int oldest = 0;
            for (int i = 0; i < count; i++)
            {
                float busyUntil = until != null && i < until.Length ? until[i] : 0f;
                if (busyUntil <= now)
                {
                    free = i;
                    break;
                }

                if (started[i] < started[oldest])
                {
                    oldest = i;
                }
            }

            if (free >= 0)
            {
                return free;
            }

            return oldest;
        }
    }
}
