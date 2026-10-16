using System.Globalization;
using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Moves wave-start bodies whose renderer bounds cover the player on screen.
    /// The push is along the camera's right, so a rock on the look axis leaves
    /// the middle of the frame. Floor meshes are not moved as one AABB: only the
    /// central cluster (the World 4 column) is shifted. Does not read RunRng.
    /// </summary>
    public static class OpeningClearance
    {
        private static readonly System.Collections.Generic.List<string> Notes = new System.Collections.Generic.List<string>();
        private static readonly System.Collections.Generic.List<Transform> Moved = new System.Collections.Generic.List<Transform>();

        public static void ClearWaveStart()
        {
            Separate(false, false);
        }

        public static void Hold()
        {
            Separate(true, false);
        }

        public static void LogAndHold()
        {
            Separate(true, true);
            if (Notes.Count == 0)
            {
                Debug.Log("AgrCapture keep-out none");
                return;
            }

            for (int i = 0; i < Notes.Count; i++)
            {
                Debug.Log(Notes[i]);
            }
        }

        private static void Separate(bool holdStill, bool log)
        {
            Notes.Clear();
            Moved.Clear();
            if (log && !OpeningView.SelfCheck())
            {
                Notes.Add("AgrCapture keep-out OpeningView.SelfCheck failed");
            }

            Vector3 ship;
            Vector3 eye;
            Vector3 look;
            float aspect;
            float minU;
            float minV;
            float maxU;
            float maxV;
            if (!TryFrame(out ship, out eye, out look, out aspect, out minU, out minV, out maxU, out maxV))
            {
                return;
            }

            if (log)
            {
                Notes.Add("AgrCapture keep-out player screen=" + RectText(minU, minV, maxU, maxV));
            }

            SeparateActors(eye, look, aspect, minU, minV, maxU, maxV, holdStill, log);
            SeparateBelt(eye, look, aspect, minU, minV, maxU, maxV, holdStill, log);
            SeparateFloors(ship, eye, look, aspect, minU, minV, maxU, maxV, log);
        }

        private static bool TryFrame(
            out Vector3 ship,
            out Vector3 eye,
            out Vector3 look,
            out float aspect,
            out float minU,
            out float minV,
            out float maxU,
            out float maxV)
        {
            ship = Vector3.zero;
            GameObject player = GameObject.FindGameObjectWithTag(GameTags.Player);
            if (player != null)
            {
                ship = player.transform.position;
            }

            eye = ship + new Vector3(0f, DecorDepthRules.CameraOffsetY, DecorDepthRules.CameraOffsetZ);
            look = ship;
            aspect = 16f / 9f;
            if (Screen.width > 1 && Screen.height > 1)
            {
                aspect = Screen.width / (float)Screen.height;
            }

            float cx = ship.x;
            float cy = ship.y + OpeningView.PlayerCenterY;
            float cz = ship.z;
            float ex = OpeningView.PlayerHalfX;
            float ey = OpeningView.PlayerHalfY;
            float ez = OpeningView.PlayerHalfZ;
            if (player != null)
            {
                Renderer[] renderers = player.GetComponentsInChildren<Renderer>(true);
                bool any = false;
                Bounds bounds = new Bounds(player.transform.position, Vector3.zero);
                for (int i = 0; i < renderers.Length; i++)
                {
                    Renderer renderer = renderers[i];
                    if (renderer == null)
                    {
                        continue;
                    }

                    if (!any)
                    {
                        bounds = renderer.bounds;
                        any = true;
                    }
                    else
                    {
                        bounds.Encapsulate(renderer.bounds);
                    }
                }

                if (any && bounds.extents.sqrMagnitude > 0.01f)
                {
                    cx = bounds.center.x;
                    cy = bounds.center.y;
                    cz = bounds.center.z;
                    ex = ClampHalf(bounds.extents.x, OpeningView.PlayerHalfX);
                    ey = ClampHalf(bounds.extents.y, OpeningView.PlayerHalfY);
                    ez = ClampHalf(bounds.extents.z, OpeningView.PlayerHalfZ);
                }
            }

            OpeningView.PlayerRect(eye.x, eye.y, eye.z, look.x, look.y, look.z, cx, cy, cz, ex, ey, ez, aspect, out minU, out minV, out maxU, out maxV);
            return true;
        }

        private static float ClampHalf(float measured, float fallback)
        {
            if (measured < 0.35f)
            {
                return fallback;
            }

            if (measured > 3.2f)
            {
                return 3.2f;
            }

            return measured;
        }

        private static void SeparateActors(
            Vector3 eye,
            Vector3 look,
            float aspect,
            float minU,
            float minV,
            float maxU,
            float maxV,
            bool holdStill,
            bool log)
        {
            Asteroid[] rocks = UnityEngine.Object.FindObjectsByType<Asteroid>();
            for (int i = 0; i < rocks.Length; i++)
            {
                Asteroid rock = rocks[i];
                if (rock == null)
                {
                    continue;
                }

                NudgeBody(rock.transform, eye, look, aspect, minU, minV, maxU, maxV, holdStill, log);
            }
        }

        private static void SeparateBelt(
            Vector3 eye,
            Vector3 look,
            float aspect,
            float minU,
            float minV,
            float maxU,
            float maxV,
            bool holdStill,
            bool log)
        {
            MeshFilter[] filters = UnityEngine.Object.FindObjectsByType<MeshFilter>();
            for (int i = 0; i < filters.Length; i++)
            {
                MeshFilter filter = filters[i];
                if (filter == null)
                {
                    continue;
                }

                Transform slot = BeltSlot(filter.transform);
                if (slot == null || Moved.Contains(slot))
                {
                    continue;
                }

                NudgeBody(slot, eye, look, aspect, minU, minV, maxU, maxV, holdStill, log);
            }
        }

        private static Transform BeltSlot(Transform node)
        {
            Transform cursor = node;
            while (cursor != null)
            {
                if (cursor.name.StartsWith("Belt_", System.StringComparison.Ordinal))
                {
                    return cursor;
                }

                cursor = cursor.parent;
            }

            return null;
        }

        private static void NudgeBody(
            Transform body,
            Vector3 eye,
            Vector3 look,
            float aspect,
            float minU,
            float minV,
            float maxU,
            float maxV,
            bool holdStill,
            bool log)
        {
            if (body == null || Moved.Contains(body))
            {
                return;
            }

            Bounds bounds;
            if (!TryRendererBounds(body, out bounds))
            {
                return;
            }

            float push;
            bool hit = OpeningView.TrySidestep(
                eye.x,
                eye.y,
                eye.z,
                look.x,
                look.y,
                look.z,
                bounds.center.x,
                bounds.center.y,
                bounds.center.z,
                bounds.extents.x,
                bounds.extents.y,
                bounds.extents.z,
                aspect,
                minU,
                minV,
                maxU,
                maxV,
                out push);
            if (hit)
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
                OpeningView.Basis(eye.x, eye.y, eye.z, look.x, look.y, look.z, out fx, out fy, out fz, out rx, out ry, out rz, out ux, out uy, out uz);
                body.position += new Vector3(rx * push, 0f, rz * push);
                if (holdStill)
                {
                    Rigidbody rigidbody = body.GetComponent<Rigidbody>();
                    if (rigidbody != null)
                    {
                        rigidbody.position = body.position;
                        rigidbody.linearVelocity = Vector3.zero;
                        rigidbody.angularVelocity = Vector3.zero;
                    }
                }

                if (!TryRendererBounds(body, out bounds))
                {
                    bounds = new Bounds(body.position, Vector3.zero);
                }
            }

            Moved.Add(body);

            if (!log)
            {
                return;
            }

            float boxMinU;
            float boxMinV;
            float boxMaxU;
            float boxMaxV;
            bool any;
            ScreenOf(eye, look, bounds, aspect, out boxMinU, out boxMinV, out boxMaxU, out boxMaxV, out any);
            int overlap = 0;
            if (any && boxMinU < maxU && boxMaxU > minU && boxMinV < maxV && boxMaxV > minV)
            {
                overlap = 1;
            }

            Notes.Add(
                "AgrCapture keep-out name="
                + body.name
                + " world="
                + body.position.x.ToString("0.00", CultureInfo.InvariantCulture)
                + ","
                + body.position.y.ToString("0.00", CultureInfo.InvariantCulture)
                + ","
                + body.position.z.ToString("0.00", CultureInfo.InvariantCulture)
                + " screen="
                + RectText(boxMinU, boxMinV, boxMaxU, boxMaxV)
                + " overlap="
                + overlap.ToString(CultureInfo.InvariantCulture)
                + " push="
                + push.ToString("0.00", CultureInfo.InvariantCulture));
        }

        private static void ScreenOf(
            Vector3 eye,
            Vector3 look,
            Bounds bounds,
            float aspect,
            out float minU,
            out float minV,
            out float maxU,
            out float maxV,
            out bool any)
        {
            minU = 0f;
            minV = 0f;
            maxU = 0f;
            maxV = 0f;
            any = false;
            Vector3 center = bounds.center;
            Vector3 extents = bounds.extents;
            for (int corner = 0; corner < 8; corner++)
            {
                float sx = (corner & 1) == 0 ? -extents.x : extents.x;
                float sy = (corner & 2) == 0 ? -extents.y : extents.y;
                float sz = (corner & 4) == 0 ? -extents.z : extents.z;
                float u;
                float v;
                if (!OpeningView.Project(eye.x, eye.y, eye.z, look.x, look.y, look.z, center.x + sx, center.y + sy, center.z + sz, aspect, out u, out v))
                {
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

        private static bool TryRendererBounds(Transform body, out Bounds bounds)
        {
            bounds = new Bounds(body.position, Vector3.zero);
            Renderer[] renderers = body.GetComponentsInChildren<Renderer>(true);
            bool any = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                if (!any)
                {
                    bounds = renderer.bounds;
                    any = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return any;
        }

        private static void SeparateFloors(
            Vector3 ship,
            Vector3 eye,
            Vector3 look,
            float aspect,
            float minU,
            float minV,
            float maxU,
            float maxV,
            bool log)
        {
            MeshFilter[] filters = UnityEngine.Object.FindObjectsByType<MeshFilter>();
            for (int i = 0; i < filters.Length; i++)
            {
                MeshFilter filter = filters[i];
                if (filter == null || filter.sharedMesh == null)
                {
                    continue;
                }

                if (BeltSlot(filter.transform) != null)
                {
                    continue;
                }

                if (filter.GetComponentInParent<Asteroid>() != null)
                {
                    continue;
                }

                Renderer renderer = filter.GetComponent<Renderer>();
                if (renderer == null)
                {
                    continue;
                }

                Vector3 ext = renderer.bounds.extents;
                if (ext.x < 12f && ext.z < 12f)
                {
                    continue;
                }

                CarveCluster(filter, ship, eye, look, aspect, minU, minV, maxU, maxV, log);
            }
        }

        private static void CarveCluster(
            MeshFilter filter,
            Vector3 ship,
            Vector3 eye,
            Vector3 look,
            float aspect,
            float minU,
            float minV,
            float maxU,
            float maxV,
            bool log)
        {
            Mesh source = filter.sharedMesh;
            if (source == null || !source.isReadable)
            {
                if (log)
                {
                    Notes.Add("AgrCapture keep-out floor=" + filter.name + " readable=0");
                }

                return;
            }

            if (source.name.EndsWith("_Opening", System.StringComparison.Ordinal))
            {
                return;
            }

            Vector3[] verts = source.vertices;
            if (verts == null || verts.Length == 0)
            {
                return;
            }

            int[] cluster = new int[verts.Length];
            int count = 0;
            Transform meshTransform = filter.transform;
            float minX = 0f;
            float minY = 0f;
            float minZ = 0f;
            float maxX = 0f;
            float maxY = 0f;
            float maxZ = 0f;
            for (int i = 0; i < verts.Length; i++)
            {
                Vector3 world = meshTransform.TransformPoint(verts[i]);
                float dx = world.x - ship.x;
                float dz = world.z - ship.z;
                if ((dx * dx) + (dz * dz) > OpeningView.FloorClusterMetres * OpeningView.FloorClusterMetres)
                {
                    continue;
                }

                float u;
                float v;
                if (!OpeningView.Project(eye.x, eye.y, eye.z, look.x, look.y, look.z, world.x, world.y, world.z, aspect, out u, out v))
                {
                    continue;
                }

                if (u < minU || u > maxU || v < minV || v > maxV)
                {
                    continue;
                }

                cluster[count] = i;
                if (count == 0)
                {
                    minX = world.x;
                    maxX = world.x;
                    minY = world.y;
                    maxY = world.y;
                    minZ = world.z;
                    maxZ = world.z;
                }
                else
                {
                    if (world.x < minX)
                    {
                        minX = world.x;
                    }

                    if (world.x > maxX)
                    {
                        maxX = world.x;
                    }

                    if (world.y < minY)
                    {
                        minY = world.y;
                    }

                    if (world.y > maxY)
                    {
                        maxY = world.y;
                    }

                    if (world.z < minZ)
                    {
                        minZ = world.z;
                    }

                    if (world.z > maxZ)
                    {
                        maxZ = world.z;
                    }
                }

                count++;
            }

            if (count == 0 || count > verts.Length / 3)
            {
                return;
            }

            float cx = (minX + maxX) * 0.5f;
            float cy = (minY + maxY) * 0.5f;
            float cz = (minZ + maxZ) * 0.5f;
            float ex = (maxX - minX) * 0.5f;
            float ey = (maxY - minY) * 0.5f;
            float ez = (maxZ - minZ) * 0.5f;
            float push;
            if (!OpeningView.TrySidestep(eye.x, eye.y, eye.z, look.x, look.y, look.z, cx, cy, cz, ex, ey, ez, aspect, minU, minV, maxU, maxV, out push))
            {
                return;
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
            OpeningView.Basis(eye.x, eye.y, eye.z, look.x, look.y, look.z, out fx, out fy, out fz, out rx, out ry, out rz, out ux, out uy, out uz);
            Mesh copy = UnityEngine.Object.Instantiate(source);
            copy.name = source.name + "_Opening";
            Vector3[] moved = copy.vertices;
            for (int i = 0; i < count; i++)
            {
                int index = cluster[i];
                Vector3 world = meshTransform.TransformPoint(moved[index]);
                world.x += rx * push;
                world.z += rz * push;
                moved[index] = meshTransform.InverseTransformPoint(world);
            }

            copy.vertices = moved;
            copy.RecalculateBounds();
            filter.sharedMesh = copy;
            if (log)
            {
                Notes.Add(
                    "AgrCapture keep-out floor="
                    + filter.name
                    + " cluster="
                    + count.ToString(CultureInfo.InvariantCulture)
                    + " push="
                    + push.ToString("0.00", CultureInfo.InvariantCulture)
                    + " overlap=0");
            }
        }

        private static string RectText(float minU, float minV, float maxU, float maxV)
        {
            return minU.ToString("0.000", CultureInfo.InvariantCulture)
                + ","
                + minV.ToString("0.000", CultureInfo.InvariantCulture)
                + ","
                + maxU.ToString("0.000", CultureInfo.InvariantCulture)
                + ","
                + maxV.ToString("0.000", CultureInfo.InvariantCulture);
        }
    }
}
