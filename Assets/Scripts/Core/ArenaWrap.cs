using System;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Classic circular wrap: leave one edge, appear opposite, keep velocity.
    /// Unity-free so the math can be checked without the Editor.
    /// </summary>
    public static class ArenaWrap
    {
        public const float EdgeInset = 0.05f;
        public const float SoftLockSlack = 2f;
        public const float SoftLockSeconds = 3f;
        /// <summary>Fight plane Y. Matches <see cref="ShipController.PlayHeight"/>.</summary>
        public const float PlayY = 0.4f;
        /// <summary>AstroFloor / arena top after SinkPlaySurface.</summary>
        public const float FloorY = -0.08f;
        /// <summary>Above this, a rock has left the camera/play volume.</summary>
        public const float MaxPlayY = 3f;

        public static bool ShouldWrap(float x, float z, float radius)
        {
            return (x * x + z * z) > radius * radius;
        }

        public static bool IsBeyondSoftLock(float x, float z, float radius)
        {
            float limit = radius + SoftLockSlack;
            return (x * x + z * z) > limit * limit;
        }

        public static bool IsInvalidXz(float x, float z)
        {
            return float.IsNaN(x) || float.IsNaN(z) || float.IsInfinity(x) || float.IsInfinity(z);
        }

        public static bool IsInvalidY(float y)
        {
            return float.IsNaN(y) || float.IsInfinity(y);
        }

        public static bool IsInvalid(float x, float y, float z)
        {
            return IsInvalidXz(x, z) || IsInvalidY(y);
        }

        /// <summary>Under the AstroFloor / arena surface, or flown above the play volume.</summary>
        public static bool IsOutOfPlayY(float y)
        {
            return IsInvalidY(y) || y < FloorY || y > MaxPlayY;
        }

        public static bool IsOutOfPlay(float x, float y, float z, float radius)
        {
            return IsInvalid(x, y, z) || IsOutOfPlayY(y) || IsBeyondSoftLock(x, z, radius);
        }

        /// <summary>
        /// Lock onto the fight plane. Never below the floor, never left at a NaN/inf Y.
        /// </summary>
        public static float ClampY(float y)
        {
            if (IsInvalidY(y) || y < PlayY || y > MaxPlayY)
            {
                return PlayY;
            }

            return PlayY;
        }

        public static void WrapXz(float x, float z, float radius, out float ox, out float oz)
        {
            float inner = radius - EdgeInset;
            if (inner < 0.01f)
            {
                inner = radius;
            }

            if (IsInvalidXz(x, z))
            {
                ox = -inner;
                oz = 0f;
                return;
            }

            float magSq = x * x + z * z;
            if (magSq <= radius * radius)
            {
                ox = x;
                oz = z;
                return;
            }

            float mag = (float)Math.Sqrt(magSq);
            if (mag < 0.0001f)
            {
                ox = -inner;
                oz = 0f;
                return;
            }

            float scale = -inner / mag;
            ox = x * scale;
            oz = z * scale;
        }
    }
}
