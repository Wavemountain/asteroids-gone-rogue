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
