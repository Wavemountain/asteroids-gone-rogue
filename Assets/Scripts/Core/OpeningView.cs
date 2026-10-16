namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Wave-start keep-out in the fight camera's screen, not on the XZ plane.
    /// The play camera sits at the ship plus (0, 35, -22), FOV 54, and looks at
    /// the ship. A body on that look axis stays in the middle of the frame no
    /// matter how far an XZ push sends it. Unity-free. Does not read the daily roll.
    /// </summary>
    public static class OpeningView
    {
        public const float FovDegrees = 54f;
        public const float Margin = 0.045f;
        public const float PlayerHalfX = 2.30f;
        public const float PlayerHalfY = 1.05f;
        public const float PlayerHalfZ = 2.40f;
        public const float PlayerCenterY = 0.40f;
        public const float SidestepMetres = 1.15f;
        public const int SidestepSteps = 28;
        public const float FloorClusterMetres = 6f;

        public static void Basis(
            float eyeX,
            float eyeY,
            float eyeZ,
            float lookX,
            float lookY,
            float lookZ,
            out float fx,
            out float fy,
            out float fz,
            out float rx,
            out float ry,
            out float rz,
            out float ux,
            out float uy,
            out float uz)
        {
            float dx = lookX - eyeX;
            float dy = lookY - eyeY;
            float dz = lookZ - eyeZ;
            float flen = (float)System.Math.Sqrt((dx * dx) + (dy * dy) + (dz * dz));
            if (flen < 0.0001f)
            {
                flen = 0.0001f;
            }

            fx = dx / flen;
            fy = dy / flen;
            fz = dz / flen;
            rx = fz;
            ry = 0f;
            rz = -fx;
            float rlen = (float)System.Math.Sqrt((rx * rx) + (rz * rz));
            if (rlen < 0.0001f)
            {
                rx = 1f;
                ry = 0f;
                rz = 0f;
            }
            else
            {
                rx /= rlen;
                rz /= rlen;
            }

            ux = (fy * rz) - (fz * ry);
            uy = (fz * rx) - (fx * rz);
            uz = (fx * ry) - (fy * rx);
        }

        public static bool Project(
            float eyeX,
            float eyeY,
            float eyeZ,
            float lookX,
            float lookY,
            float lookZ,
            float x,
            float y,
            float z,
            float aspect,
            out float u,
            out float v)
        {
            float fx;
            float fy;
            float fz;
            float rx;
            float ry;
            float rz;
            float ux;
            float uy;
            float uz;
            Basis(eyeX, eyeY, eyeZ, lookX, lookY, lookZ, out fx, out fy, out fz, out rx, out ry, out rz, out ux, out uy, out uz);
            float vx = x - eyeX;
            float vy = y - eyeY;
            float vz = z - eyeZ;
            float cx = (vx * rx) + (vy * ry) + (vz * rz);
            float cy = (vx * ux) + (vy * uy) + (vz * uz);
            float cz = (vx * fx) + (vy * fy) + (vz * fz);
            u = 0.5f;
            v = 0.5f;
            if (cz <= 0.05f)
            {
                return false;
            }

            float wide = aspect;
            if (wide < 0.2f)
            {
                wide = 16f / 9f;
            }

            float half = (float)System.Math.Tan(FovDegrees * System.Math.PI / 360.0);
            if (half < 0.01f)
            {
                half = 0.01f;
            }

            float ndcX = (cx / cz) / (half * wide);
            float ndcY = (cy / cz) / half;
            u = (ndcX + 1f) * 0.5f;
            v = (ndcY + 1f) * 0.5f;
            return true;
        }

        public static void PlayerRect(
            float eyeX,
            float eyeY,
            float eyeZ,
            float lookX,
            float lookY,
            float lookZ,
            float cx,
            float cy,
            float cz,
            float ex,
            float ey,
            float ez,
            float aspect,
            out float minU,
            out float minV,
            out float maxU,
            out float maxV)
        {
            bool any;
            BoxRect(eyeX, eyeY, eyeZ, lookX, lookY, lookZ, cx, cy, cz, ex, ey, ez, aspect, out minU, out minV, out maxU, out maxV, out any);
            if (!any)
            {
                minU = 0.5f - 0.06f;
                minV = 0.5f - 0.08f;
                maxU = 0.5f + 0.06f;
                maxV = 0.5f + 0.08f;
            }

            minU -= Margin;
            minV -= Margin;
            maxU += Margin;
            maxV += Margin;
        }

        /// <summary>
        /// True when the world AABB covers the player screen rect. A box that
        /// straddles the near plane covers the lens and counts as a hit.
        /// </summary>
        public static bool BoxHits(
            float eyeX,
            float eyeY,
            float eyeZ,
            float lookX,
            float lookY,
            float lookZ,
            float cx,
            float cy,
            float cz,
            float ex,
            float ey,
            float ez,
            float aspect,
            float minU,
            float minV,
            float maxU,
            float maxV)
        {
            float boxMinU;
            float boxMinV;
            float boxMaxU;
            float boxMaxV;
            bool any;
            int behind;
            BoxRect(eyeX, eyeY, eyeZ, lookX, lookY, lookZ, cx, cy, cz, ex, ey, ez, aspect, out boxMinU, out boxMinV, out boxMaxU, out boxMaxV, out any, out behind);
            if (!any)
            {
                return false;
            }

            if (behind > 0)
            {
                return true;
            }

            return boxMinU < maxU && boxMaxU > minU && boxMinV < maxV && boxMaxV > minV;
        }

        public static bool TrySidestep(
            float eyeX,
            float eyeY,
            float eyeZ,
            float lookX,
            float lookY,
            float lookZ,
            float cx,
            float cy,
            float cz,
            float ex,
            float ey,
            float ez,
            float aspect,
            float minU,
            float minV,
            float maxU,
            float maxV,
            out float push)
        {
            push = 0f;
            if (!BoxHits(eyeX, eyeY, eyeZ, lookX, lookY, lookZ, cx, cy, cz, ex, ey, ez, aspect, minU, minV, maxU, maxV))
            {
                return false;
            }

            float fx;
            float fy;
            float fz;
            float rx;
            float ry;
            float rz;
            float ux;
            float uy;
            float uz;
            Basis(eyeX, eyeY, eyeZ, lookX, lookY, lookZ, out fx, out fy, out fz, out rx, out ry, out rz, out ux, out uy, out uz);
            float sign = cx >= lookX ? 1f : -1f;
            if (TrySigned(sign, eyeX, eyeY, eyeZ, lookX, lookY, lookZ, cx, cy, cz, ex, ey, ez, aspect, minU, minV, maxU, maxV, rx, rz, out push))
            {
                return true;
            }

            return TrySigned(-sign, eyeX, eyeY, eyeZ, lookX, lookY, lookZ, cx, cy, cz, ex, ey, ez, aspect, minU, minV, maxU, maxV, rx, rz, out push);
        }

        public static bool SelfCheck()
        {
            float aspect = 16f / 9f;
            float u;
            float v;
            if (!Project(0f, 35f, -22f, 0f, 0f, 0f, 0f, 0f, 0f, aspect, out u, out v))
            {
                return false;
            }

            if (u < 0.49f || u > 0.51f || v < 0.49f || v > 0.51f)
            {
                return false;
            }

            float minU;
            float minV;
            float maxU;
            float maxV;
            PlayerRect(0f, 35f, -22f, 0f, 0f, 0f, 0f, PlayerCenterY, 0f, PlayerHalfX, PlayerHalfY, PlayerHalfZ, aspect, out minU, out minV, out maxU, out maxV);
            if (!BoxHits(0f, 35f, -22f, 0f, 0f, 0f, 0f, 0.3f, 0f, 1.36f, 0.7f, 1.36f, aspect, minU, minV, maxU, maxV))
            {
                return false;
            }

            if (BoxHits(0f, 35f, -22f, 0f, 0f, 0f, 10f, 0.3f, 0f, 1.36f, 0.7f, 1.36f, aspect, minU, minV, maxU, maxV))
            {
                return false;
            }

            if (BoxHits(0f, 35f, -22f, 0f, 0f, 0f, -1.2f, 0.4f, 19.1f, 2.679f, 2.679f, 2.679f, aspect, minU, minV, maxU, maxV))
            {
                return false;
            }

            float push;
            if (!TrySidestep(0f, 35f, -22f, 0f, 0f, 0f, 0f, 0.3f, 0f, 1.36f, 0.7f, 1.36f, aspect, minU, minV, maxU, maxV, out push))
            {
                return false;
            }

            if (push < 4f)
            {
                return false;
            }

            if (FlashCoverage(0.5f, 0.5f) < 0.99f || FlashCoverage(0f, 0.5f) > 0.001f)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Death-flash mask. Opaque at the lens centre, clear at the frame edge,
        /// so the white wash cannot lift the vignette.
        /// </summary>
        public static float FlashCoverage(float u, float v)
        {
            float du = (u - 0.5f) * (16f / 9f);
            float dv = v - 0.5f;
            float dist = (float)System.Math.Sqrt((du * du) + (dv * dv)) / 0.85f;
            float t = (dist - 0.18f) / 0.55f;
            if (t < 0f)
            {
                t = 0f;
            }

            if (t > 1f)
            {
                t = 1f;
            }

            float smooth = t * t * (3f - (2f * t));
            return 1f - smooth;
        }

        private static bool TrySigned(
            float sign,
            float eyeX,
            float eyeY,
            float eyeZ,
            float lookX,
            float lookY,
            float lookZ,
            float cx,
            float cy,
            float cz,
            float ex,
            float ey,
            float ez,
            float aspect,
            float minU,
            float minV,
            float maxU,
            float maxV,
            float rx,
            float rz,
            out float push)
        {
            push = 0f;
            for (int step = 1; step <= SidestepSteps; step++)
            {
                float distance = SidestepMetres * step * sign;
                float nx = cx + (rx * distance);
                float nz = cz + (rz * distance);
                if (!BoxHits(eyeX, eyeY, eyeZ, lookX, lookY, lookZ, nx, cy, nz, ex, ey, ez, aspect, minU, minV, maxU, maxV))
                {
                    push = distance;
                    return true;
                }
            }

            return false;
        }

        private static void BoxRect(
            float eyeX,
            float eyeY,
            float eyeZ,
            float lookX,
            float lookY,
            float lookZ,
            float cx,
            float cy,
            float cz,
            float ex,
            float ey,
            float ez,
            float aspect,
            out float minU,
            out float minV,
            out float maxU,
            out float maxV,
            out bool any)
        {
            int behind;
            BoxRect(eyeX, eyeY, eyeZ, lookX, lookY, lookZ, cx, cy, cz, ex, ey, ez, aspect, out minU, out minV, out maxU, out maxV, out any, out behind);
        }

        private static void BoxRect(
            float eyeX,
            float eyeY,
            float eyeZ,
            float lookX,
            float lookY,
            float lookZ,
            float cx,
            float cy,
            float cz,
            float ex,
            float ey,
            float ez,
            float aspect,
            out float minU,
            out float minV,
            out float maxU,
            out float maxV,
            out bool any,
            out int behind)
        {
            minU = 0f;
            minV = 0f;
            maxU = 0f;
            maxV = 0f;
            any = false;
            behind = 0;
            float hx = ex < 0f ? 0f : ex;
            float hy = ey < 0f ? 0f : ey;
            float hz = ez < 0f ? 0f : ez;
            for (int corner = 0; corner < 8; corner++)
            {
                float sx = (corner & 1) == 0 ? -hx : hx;
                float sy = (corner & 2) == 0 ? -hy : hy;
                float sz = (corner & 4) == 0 ? -hz : hz;
                float u;
                float v;
                if (!Project(eyeX, eyeY, eyeZ, lookX, lookY, lookZ, cx + sx, cy + sy, cz + sz, aspect, out u, out v))
                {
                    behind++;
                    continue;
                }

                if (!any)
                {
                    minU = u;
                    maxU = u;
                    minV = v;
                    maxV = v;
                    any = true;
                }
                else
                {
                    if (u < minU)
                    {
                        minU = u;
                    }

                    if (u > maxU)
                    {
                        maxU = u;
                    }

                    if (v < minV)
                    {
                        minV = v;
                    }

                    if (v > maxV)
                    {
                        maxV = v;
                    }
                }
            }
        }
    }
}
