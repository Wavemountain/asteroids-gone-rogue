using System;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Fixed convex contour. Samples are a closed polygon around the origin.
    /// Unity-free so the layer gate can prove contains, normals, and no trap.
    /// DistanceToEdge is positive inside the polygon.
    /// </summary>
    public sealed class ArenaShape
    {
        public const int SampleCount = 48;
        const float Tau = 6.28318530718f;

        readonly float[] _x;
        readonly float[] _z;
        readonly float _area;
        readonly float _bound;
        readonly float _min;

        ArenaShape(float[] x, float[] z, float area, float bound, float min)
        {
            _x = x;
            _z = z;
            _area = area;
            _bound = bound;
            _min = min;
        }

        public int PointCount
        {
            get { return _x.Length; }
        }

        public float Area
        {
            get { return _area; }
        }

        public float BoundingRadius
        {
            get { return _bound; }
        }

        public float MinRadius
        {
            get { return _min; }
        }

        public static ArenaShape FromSamples(float[] radii)
        {
            int count = SampleCount;
            float[] x = new float[count];
            float[] z = new float[count];
            float step = Tau / count;
            for (int i = 0; i < count; i++)
            {
                float radius = 1f;
                if (radii != null && i < radii.Length)
                {
                    radius = radii[i];
                }

                if (radius < 0.5f)
                {
                    radius = 0.5f;
                }

                float angle = i * step;
                x[i] = (float)Math.Cos(angle) * radius;
                z[i] = (float)Math.Sin(angle) * radius;
            }

            float area = 0f;
            float bound = 0f;
            float min = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                int next = i + 1;
                if (next == count)
                {
                    next = 0;
                }

                area += (_xCross(x[i], z[i], x[next], z[next]));
                float radius = (float)Math.Sqrt((x[i] * x[i]) + (z[i] * z[i]));
                if (radius > bound)
                {
                    bound = radius;
                }

                if (radius < min)
                {
                    min = radius;
                }
            }

            if (area < 0f)
            {
                area = -area;
            }

            area *= 0.5f;
            if (min > bound)
            {
                min = bound;
            }

            return new ArenaShape(x, z, area, bound, min);
        }

        static float _xCross(float ax, float az, float bx, float bz)
        {
            return (ax * bz) - (az * bx);
        }

        public bool Contains(float px, float pz)
        {
            if (float.IsNaN(px) || float.IsNaN(pz) || float.IsInfinity(px) || float.IsInfinity(pz))
            {
                return false;
            }

            int count = _x.Length;
            for (int i = 0; i < count; i++)
            {
                int next = i + 1;
                if (next == count)
                {
                    next = 0;
                }

                float ex = _x[next] - _x[i];
                float ez = _z[next] - _z[i];
                float left = (ex * (pz - _z[i])) - (ez * (px - _x[i]));
                if (left < -0.02f)
                {
                    return false;
                }
            }

            return true;
        }

        public float RadiusAt(float angle)
        {
            float dx = (float)Math.Cos(angle);
            float dz = (float)Math.Sin(angle);
            float best = _bound > 1f ? _bound : 1f;
            bool hit = false;
            int count = _x.Length;
            for (int i = 0; i < count; i++)
            {
                int next = i + 1;
                if (next == count)
                {
                    next = 0;
                }

                float ex = _x[next] - _x[i];
                float ez = _z[next] - _z[i];
                float denom = (ez * dx) - (ex * dz);
                if (denom < 0.0000001f && denom > -0.0000001f)
                {
                    continue;
                }

                float t = ((_x[i] * dz) - (_z[i] * dx)) / denom;
                if (t < -0.0001f || t > 1.0001f)
                {
                    continue;
                }

                float along;
                if (dx > 0.0001f || dx < -0.0001f)
                {
                    along = (_x[i] + (t * ex)) / dx;
                }
                else
                {
                    along = (_z[i] + (t * ez)) / dz;
                }

                if (along > 0.01f && along < best)
                {
                    best = along;
                    hit = true;
                }
            }

            if (!hit)
            {
                return _bound;
            }

            return best;
        }

        public float DistanceToEdge(float px, float pz)
        {
            float cx;
            float cz;
            ClosestPoint(px, pz, out cx, out cz);
            float dx = px - cx;
            float dz = pz - cz;
            float distance = (float)Math.Sqrt((dx * dx) + (dz * dz));
            if (Contains(px, pz))
            {
                return distance;
            }

            return -distance;
        }

        public void ClosestPoint(float px, float pz, out float ox, out float oz)
        {
            int count = _x.Length;
            float best = float.MaxValue;
            ox = _x[0];
            oz = _z[0];
            for (int i = 0; i < count; i++)
            {
                int next = i + 1;
                if (next == count)
                {
                    next = 0;
                }

                float qx;
                float qz;
                SegmentPoint(px, pz, _x[i], _z[i], _x[next], _z[next], out qx, out qz);
                float dx = px - qx;
                float dz = pz - qz;
                float sqr = (dx * dx) + (dz * dz);
                if (sqr < best)
                {
                    best = sqr;
                    ox = qx;
                    oz = qz;
                }
            }
        }

        static void SegmentPoint(
            float px,
            float pz,
            float ax,
            float az,
            float bx,
            float bz,
            out float ox,
            out float oz)
        {
            float ex = bx - ax;
            float ez = bz - az;
            float len = (ex * ex) + (ez * ez);
            float t = 0f;
            if (len > 0.0000001f)
            {
                t = (((px - ax) * ex) + ((pz - az) * ez)) / len;
            }

            if (t < 0f)
            {
                t = 0f;
            }

            if (t > 1f)
            {
                t = 1f;
            }

            ox = ax + (ex * t);
            oz = az + (ez * t);
        }

        public void EdgeNormal(float px, float pz, out float nx, out float nz)
        {
            int count = _x.Length;
            int best = 0;
            float bestSqr = float.MaxValue;
            float qx = 0f;
            float qz = 0f;
            for (int i = 0; i < count; i++)
            {
                int next = i + 1;
                if (next == count)
                {
                    next = 0;
                }

                float sx;
                float sz;
                SegmentPoint(px, pz, _x[i], _z[i], _x[next], _z[next], out sx, out sz);
                float dx = px - sx;
                float dz = pz - sz;
                float sqr = (dx * dx) + (dz * dz);
                if (sqr < bestSqr)
                {
                    bestSqr = sqr;
                    best = i;
                    qx = sx;
                    qz = sz;
                }
            }

            int end = best + 1;
            if (end == count)
            {
                end = 0;
            }

            if (Contains(px, pz) || bestSqr < 0.0001f)
            {
                float ex = _x[end] - _x[best];
                float ez = _z[end] - _z[best];
                float len = (float)Math.Sqrt((ex * ex) + (ez * ez));
                if (len < 0.0001f)
                {
                    nx = 1f;
                    nz = 0f;
                    return;
                }

                nx = ez / len;
                nz = -ex / len;
                return;
            }

            float ox = px - qx;
            float oz = pz - qz;
            float outLen = (float)Math.Sqrt((ox * ox) + (oz * oz));
            if (outLen < 0.0001f)
            {
                nx = 1f;
                nz = 0f;
                return;
            }

            nx = ox / outLen;
            nz = oz / outLen;
        }

        public ArenaShape Inset(float margin)
        {
            float[] radii = new float[SampleCount];
            float step = Tau / SampleCount;
            for (int i = 0; i < SampleCount; i++)
            {
                float radius = RadiusAt(i * step) - margin;
                if (radius < 0.5f)
                {
                    radius = 0.5f;
                }

                radii[i] = radius;
            }

            return FromSamples(radii);
        }

        public void SpawnRing(float angle, float margin, out float ox, out float oz)
        {
            float radius = RadiusAt(angle) + margin;
            if (radius < 0.5f)
            {
                radius = 0.5f;
            }

            ox = (float)Math.Cos(angle) * radius;
            oz = (float)Math.Sin(angle) * radius;
        }

        public bool IsConvex()
        {
            int count = _x.Length;
            for (int i = 0; i < count; i++)
            {
                int a = i;
                int b = i + 1;
                if (b == count)
                {
                    b = 0;
                }

                int c = b + 1;
                if (c == count)
                {
                    c = 0;
                }

                float ex = _x[b] - _x[a];
                float ez = _z[b] - _z[a];
                float fx = _x[c] - _x[b];
                float fz = _z[c] - _z[b];
                float turn = (ex * fz) - (ez * fx);
                if (turn <= 0.0001f)
                {
                    return false;
                }
            }

            return true;
        }

        public void WriteDistance(float[] pixels, int size, float reach)
        {
            if (pixels == null || size < 2 || reach < 1f)
            {
                return;
            }

            if (pixels.Length < size * size)
            {
                return;
            }

            float step = (reach * 2f) / size;
            for (int y = 0; y < size; y++)
            {
                float z = -reach + ((y + 0.5f) * step);
                int row = y * size;
                for (int x = 0; x < size; x++)
                {
                    float px = -reach + ((x + 0.5f) * step);
                    float distance = DistanceToEdge(px, z);
                    float encoded = 0.5f + (distance / (2f * reach));
                    if (encoded < 0f)
                    {
                        encoded = 0f;
                    }

                    if (encoded > 1f)
                    {
                        encoded = 1f;
                    }

                    pixels[row + x] = encoded;
                }
            }
        }
    }
}
