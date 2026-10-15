namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Play-camera spring, look-ahead, zoom, and directional shake.
    /// The gameplay camera follows the ship. It is not a fixed arena camera,
    /// so look-ahead is a small planar offset inside that follow: capped by
    /// the safe frame margin and pulled back so the focus never leaves the
    /// arena. ArenaFraming / letterbox stay untouched. Unity-free.
    /// </summary>
    public static class CameraFeel
    {
        public const float FollowOmega = 7.5f;
        public const float ShakeOmega = 16f;
        public const float ShakeKick = 28f;
        public const float MaxLookAhead = 1.25f;
        public const float SafeMargin = 0.18f;
        public const float FairHalfExtent = 18f;
        public const float MaxLag = 4f;
        public const float MaxZoomDegrees = 2.2f;
        public const float FullSpeed = 16f;
        public const float FullPressure = 10f;

        public static void Spring(ref float position, ref float velocity, float target, float omega, float deltaSeconds)
        {
            float rate = omega;
            if (rate < 0.01f)
            {
                rate = 0.01f;
            }

            if (deltaSeconds <= 0f)
            {
                return;
            }

            float step = deltaSeconds;
            if (step > 0.05f)
            {
                step = 0.05f;
            }

            float error = position - target;
            double decay = System.Math.Exp(-rate * step);
            double bend = velocity + (rate * (double)error);
            double travelled = error + (bend * step);
            double nextError = travelled * decay;
            double nextVelocity = (bend - (rate * travelled)) * decay;
            if (error > 0f && nextError < 0.0)
            {
                position = target;
                velocity = 0f;
                return;
            }

            if (error < 0f && nextError > 0.0)
            {
                position = target;
                velocity = 0f;
                return;
            }

            position = target + (float)nextError;
            velocity = (float)nextVelocity;
        }

        public static void SpringLimited(ref float position, ref float velocity, float target, float omega, float deltaSeconds, float limit)
        {
            Spring(ref position, ref velocity, target, omega, deltaSeconds);
            float span = limit;
            if (span < 0f)
            {
                span = 0f;
            }

            float error = position - target;
            if (error > span)
            {
                position = target + span;
                if (velocity > 0f)
                {
                    velocity = 0f;
                }
            }
            else if (error < -span)
            {
                position = target - span;
                if (velocity < 0f)
                {
                    velocity = 0f;
                }
            }
        }

        public static float ClampRange(float value, float limit)
        {
            float span = limit;
            if (span < 0f)
            {
                span = 0f;
            }

            if (value > span)
            {
                return span;
            }

            if (value < -span)
            {
                return -span;
            }

            return value;
        }

        public static float MarginCap(float visibleHalf)
        {
            float half = visibleHalf;
            if (half < 0.01f)
            {
                half = 0.01f;
            }

            float margin = SafeMargin;
            if (margin < 0.05f)
            {
                margin = 0.05f;
            }

            if (margin > 0.45f)
            {
                margin = 0.45f;
            }

            float room = (0.5f - margin) * 2f * half;
            if (room < 0f)
            {
                room = 0f;
            }

            return room;
        }

        public static float ScreenInset(float lookAhead, float visibleHalf)
        {
            float half = visibleHalf;
            if (half < 0.01f)
            {
                half = 0.01f;
            }

            float ahead = lookAhead;
            if (ahead < 0f)
            {
                ahead = -ahead;
            }

            float fraction = ahead / (2f * half);
            float inset = 0.5f - fraction;
            if (inset < 0f)
            {
                inset = 0f;
            }

            return inset;
        }

        public static float LookAheadDistance(float speed, float visibleHalf, bool reduce)
        {
            float cap = MarginCap(visibleHalf);
            if (cap > MaxLookAhead)
            {
                cap = MaxLookAhead;
            }

            float along = 0f;
            if (FullSpeed > 0.01f)
            {
                along = speed / FullSpeed;
            }

            if (along < 0f)
            {
                along = 0f;
            }

            if (along > 1f)
            {
                along = 1f;
            }

            return EffectScale.Feel(reduce, cap * along);
        }

        public static void LookAhead(
            float shipX,
            float shipZ,
            float aimX,
            float aimZ,
            float speed,
            float arenaRadius,
            float visibleHalf,
            bool reduce,
            out float outX,
            out float outZ)
        {
            float distance = LookAheadDistance(speed, visibleHalf, reduce);
            float aimMag = (float)System.Math.Sqrt((aimX * aimX) + (aimZ * aimZ));
            if (aimMag < 0.0001f || distance <= 0f)
            {
                outX = 0f;
                outZ = 0f;
                return;
            }

            float lookX = aimX * (distance / aimMag);
            float lookZ = aimZ * (distance / aimMag);
            ClampInsideArena(shipX, shipZ, ref lookX, ref lookZ, arenaRadius);
            outX = lookX;
            outZ = lookZ;
        }

        public static void ClampInsideArena(float shipX, float shipZ, ref float lookX, ref float lookZ, float arenaRadius)
        {
            float arena = arenaRadius;
            if (arena < 0f)
            {
                arena = 0f;
            }

            float focusX = shipX + lookX;
            float focusZ = shipZ + lookZ;
            float focusR = (float)System.Math.Sqrt((focusX * focusX) + (focusZ * focusZ));
            if (focusR <= arena || focusR < 0.0001f)
            {
                return;
            }

            float scale = arena / focusR;
            focusX *= scale;
            focusZ *= scale;
            lookX = focusX - shipX;
            lookZ = focusZ - shipZ;
        }

        public static float ZoomAdd(float speed, float pressure, float shipRadius, float arenaRadius, bool reduce)
        {
            float along = 0f;
            if (FullSpeed > 0.01f)
            {
                along = speed / FullSpeed;
            }

            if (along < 0f)
            {
                along = 0f;
            }

            if (along > 1f)
            {
                along = 1f;
            }

            float crowd = 0f;
            if (FullPressure > 0.01f)
            {
                crowd = pressure / FullPressure;
            }

            if (crowd < 0f)
            {
                crowd = 0f;
            }

            if (crowd > 1f)
            {
                crowd = 1f;
            }

            float add = MaxZoomDegrees * ((0.65f * along) + (0.35f * crowd));
            add = EffectScale.Feel(reduce, add);
            float arena = arenaRadius;
            if (arena < 1f)
            {
                arena = 1f;
            }

            float radius = shipRadius;
            if (radius < 0f)
            {
                radius = 0f;
            }

            float rim = 1f - (radius / arena);
            if (rim < 0f)
            {
                rim = 0f;
            }

            if (rim > 1f)
            {
                rim = 1f;
            }

            add *= rim;
            if (add < 0f)
            {
                add = 0f;
            }

            if (add > MaxZoomDegrees)
            {
                add = MaxZoomDegrees;
            }

            return add;
        }

        public static void Impulse(ref float vx, ref float vy, ref float vz, float dirX, float dirY, float dirZ, float amplitude)
        {
            if (amplitude <= 0f)
            {
                return;
            }

            float mag = (float)System.Math.Sqrt((dirX * dirX) + (dirY * dirY) + (dirZ * dirZ));
            float dx = dirX;
            float dy = dirY;
            float dz = dirZ;
            if (mag < 0.0001f)
            {
                dx = 1f;
                dy = 0f;
                dz = 0f;
                mag = 1f;
            }

            float scale = amplitude * ShakeKick / mag;
            vx += dx * scale;
            vy += dy * scale;
            vz += dz * scale;
        }

        public static void DecayShake(
            ref float ox,
            ref float oy,
            ref float oz,
            ref float vx,
            ref float vy,
            ref float vz,
            float omega,
            float deltaSeconds,
            float cap)
        {
            Spring(ref ox, ref vx, 0f, omega, deltaSeconds);
            Spring(ref oy, ref vy, 0f, omega, deltaSeconds);
            Spring(ref oz, ref vz, 0f, omega, deltaSeconds);
            float limit = cap;
            if (limit < 0f)
            {
                limit = 0f;
            }

            float mag = (float)System.Math.Sqrt((ox * ox) + (oy * oy) + (oz * oz));
            if (mag <= limit || mag < 0.0001f)
            {
                return;
            }

            float scale = limit / mag;
            ox *= scale;
            oy *= scale;
            oz *= scale;
            float outward = (vx * ox) + (vy * oy) + (vz * oz);
            if (outward > 0f)
            {
                vx *= scale;
                vy *= scale;
                vz *= scale;
            }
        }
    }
}
