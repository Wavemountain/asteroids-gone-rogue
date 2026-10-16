using System.Globalization;
using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Moves wave-start bodies whose renderer covers the player on the camera
    /// that actually draws them. Every renderer on every camera is logged,
    /// including shells, lines, dust, the player, late spawns, and enemies.
    /// Belt rocks that imported at file scale 100 are fitted before the test.
    /// The push does not read the daily roll.
    /// </summary>
    public static class OpeningClearance
    {
        public const float WatchSeconds = 2f;
        public const float BeltFitRadius = 2.2f;
        public const float BeltFitLimit = 6f;

        private const float WatchStride = 0.2f;
        private const float ShellExtent = 40f;
        private const float FloorExtent = 12f;
        private const float StandingClearance = 0.12f;
        private const int CarveCap = 64;
        private const float VisualMeshPad = 3.2f;

        private static readonly System.Collections.Generic.List<string> Notes = new System.Collections.Generic.List<string>();
        private static readonly System.Collections.Generic.List<Component> Entities = new System.Collections.Generic.List<Component>();
        private static readonly System.Collections.Generic.List<float> EntityPush = new System.Collections.Generic.List<float>();
        private static readonly System.Collections.Generic.List<Transform> Moved = new System.Collections.Generic.List<Transform>();
        private static readonly System.Collections.Generic.List<MeshFilter> Carved = new System.Collections.Generic.List<MeshFilter>();
        private static readonly System.Collections.Generic.List<ShotCam> Cams = new System.Collections.Generic.List<ShotCam>();
        private static float _watchLeft;
        private static float _sinceStride;

        public static void ClearWaveStart()
        {
            _watchLeft = WatchSeconds;
            _sinceStride = WatchStride;
            Separate(false, false);
        }

        public static void Tick()
        {
            if (_watchLeft <= 0f)
            {
                return;
            }

            float dt = Time.deltaTime;
            if (dt < 0f)
            {
                dt = 0f;
            }

            _watchLeft -= dt;
            _sinceStride += dt;
            if (_sinceStride < WatchStride)
            {
                return;
            }

            _sinceStride = 0f;
            Separate(true, false);
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

        /// <summary>
        /// Slide one spawn off the player screen rect. Used for enemies before
        /// their telegraph is placed. Does not read the daily roll.
        /// </summary>
        public static void ScreenPlace(Transform body)
        {
            if (body == null)
            {
                return;
            }

            Vector3 ship;
            ShotCam view;
            float minU;
            float minV;
            float maxU;
            float maxV;
            if (!TryFrame(out ship, out view, out minU, out minV, out maxU, out maxV))
            {
                return;
            }

            NudgeRoot(body, view, minU, minV, maxU, maxV, true, false);
        }

        public static void FitBelt(Transform slot)
        {
            if (slot == null)
            {
                return;
            }

            Bounds bounds;
            if (!TryRendererBounds(slot, out bounds))
            {
                return;
            }

            float radius = bounds.extents.x;
            if (bounds.extents.z > radius)
            {
                radius = bounds.extents.z;
            }

            if (radius <= BeltFitLimit || radius < 0.01f)
            {
                return;
            }

            float factor = BeltFitRadius / radius;
            Vector3 scale = slot.localScale;
            slot.localScale = new Vector3(scale.x * factor, scale.y * factor, scale.z * factor);
        }

        private static void Separate(bool holdStill, bool log)
        {
            Notes.Clear();
            Moved.Clear();
            Carved.Clear();
            RepairPlayMask();
            if (log && !OpeningView.SelfCheck())
            {
                Notes.Add("AgrCapture keep-out OpeningView.SelfCheck failed");
            }

            Vector3 ship;
            ShotCam view;
            float minU;
            float minV;
            float maxU;
            float maxV;
            if (!TryFrame(out ship, out view, out minU, out minV, out maxU, out maxV))
            {
                return;
            }

            if (log)
            {
                Notes.Add("AgrCapture keep-out player screen=" + RectText(minU, minV, maxU, maxV) + " camera=" + view.Name);
                NoteCameras();
            }

            Transform[] nodes = UnityEngine.Object.FindObjectsByType<Transform>();
            for (int nodeIndex = 0; nodeIndex < nodes.Length; nodeIndex++)
            {
                Transform node = nodes[nodeIndex];
                if (node == null || node.parent != null)
                {
                    continue;
                }

                Renderer[] renderers = node.GetComponentsInChildren<Renderer>(true);
                for (int rendererIndex = 0; rendererIndex < renderers.Length; rendererIndex++)
                {
                    Consider(renderers[rendererIndex], ship, view, minU, minV, maxU, maxV, holdStill, log);
                }
            }

            ClearListed(ship, view, minU, minV, maxU, maxV, holdStill, log);
        }

        private static void Consider(
            Renderer renderer,
            Vector3 ship,
            ShotCam playerView,
            float minU,
            float minV,
            float maxU,
            float maxV,
            bool holdStill,
            bool log)
        {
            if (renderer == null)
            {
                return;
            }

            if (!renderer.enabled && !log)
            {
                return;
            }

            GameObject host = renderer.gameObject;
            if (host == null)
            {
                return;
            }

            Transform belt = BeltSlot(host.transform);
            if (belt != null)
            {
                FitBelt(belt);
            }

            int layer = host.layer;
            ShotCam view = CameraFor(layer, playerView);
            Bounds bounds = renderer.bounds;
            int behind;
            bool overlap = OpeningView.Hits(
                view.EyeX,
                view.EyeY,
                view.EyeZ,
                view.Fx,
                view.Fy,
                view.Fz,
                view.Rx,
                view.Ry,
                view.Rz,
                view.Ux,
                view.Uy,
                view.Uz,
                view.Fov,
                view.Aspect,
                view.RectX,
                view.RectY,
                view.RectW,
                view.RectH,
                bounds.center.x,
                bounds.center.y,
                bounds.center.z,
                bounds.extents.x,
                bounds.extents.y,
                bounds.extents.z,
                minU,
                minV,
                maxU,
                maxV,
                out behind);
            float push = 0f;
            string kind = "body";
            bool player = IsPlayer(host);
            bool canvas = renderer.GetComponentInParent<Canvas>() != null;
            bool cameraHost = host.GetComponent<Camera>() != null;
            bool line = renderer is LineRenderer;
            if (host != null && (host.name == "ArenaLip" || host.name == "ContactBlob"))
            {
                line = true;
            }
            bool dust = IsDust(host);
            bool shell = IsShell(bounds);
            if (player)
            {
                kind = "player";
            }
            else if (canvas)
            {
                kind = "canvas";
            }
            else if (cameraHost)
            {
                kind = "camera";
            }
            else if (line)
            {
                kind = "line";
            }
            else if (dust)
            {
                kind = "dust";
            }
            else if (shell)
            {
                kind = "shell";
            }
            else if (!renderer.enabled)
            {
                kind = "off";
            }
            else if (overlap && IsFloor(bounds) && belt == null && renderer.GetComponentInParent<Asteroid>() == null)
            {
                kind = "floor";
                int remaining;
                push = CarveStanding(renderer, ship, view, minU, minV, maxU, maxV, out remaining);
                overlap = remaining > 0;
            }
            else if (overlap)
            {
                Transform body = MovableRoot(host.transform, belt);
                push = NudgeRoot(body, view, minU, minV, maxU, maxV, holdStill, true);
                bounds = renderer.bounds;
                overlap = OpeningView.Hits(
                    view.EyeX,
                    view.EyeY,
                    view.EyeZ,
                    view.Fx,
                    view.Fy,
                    view.Fz,
                    view.Rx,
                    view.Ry,
                    view.Rz,
                    view.Ux,
                    view.Uy,
                    view.Uz,
                    view.Fov,
                    view.Aspect,
                    view.RectX,
                    view.RectY,
                    view.RectW,
                    view.RectH,
                    bounds.center.x,
                    bounds.center.y,
                    bounds.center.z,
                    bounds.extents.x,
                    bounds.extents.y,
                    bounds.extents.z,
                    minU,
                    minV,
                    maxU,
                    maxV,
                    out behind);
            }

            if (!log)
            {
                return;
            }

            float boxMinU;
            float boxMinV;
            float boxMaxU;
            float boxMaxV;
            ScreenBox(bounds, view, out boxMinU, out boxMinV, out boxMaxU, out boxMaxV);
            int overlapBit = overlap ? 1 : 0;
            Notes.Add(
                "AgrCapture keep-out name="
                + host.name
                + " layer="
                + LayerText(layer)
                + " camera="
                + view.Name
                + " kind="
                + kind
                + " screen="
                + RectText(boxMinU, boxMinV, boxMaxU, boxMaxV)
                + " overlap="
                + overlapBit.ToString(CultureInfo.InvariantCulture)
                + " push="
                + push.ToString("0.00", CultureInfo.InvariantCulture)
                + " behind="
                + behind.ToString(CultureInfo.InvariantCulture));
        }

        private static float NudgeRoot(
            Transform body,
            ShotCam view,
            float minU,
            float minV,
            float maxU,
            float maxV,
            bool holdStill,
            bool once)
        {
            if (body == null || IsPlayer(body.gameObject))
            {
                return 0f;
            }

            if (once && Moved.Contains(body))
            {
                return 0f;
            }

            Bounds bounds = VisualBounds(body);
            float push;
            bool hit = OpeningView.PushAside(
                view.EyeX,
                view.EyeY,
                view.EyeZ,
                view.Fx,
                view.Fy,
                view.Fz,
                view.Rx,
                view.Ry,
                view.Rz,
                view.Ux,
                view.Uy,
                view.Uz,
                view.Fov,
                view.Aspect,
                view.RectX,
                view.RectY,
                view.RectW,
                view.RectH,
                bounds.center.x,
                bounds.center.y,
                bounds.center.z,
                bounds.extents.x,
                bounds.extents.y,
                bounds.extents.z,
                minU,
                minV,
                maxU,
                maxV,
                out push);
            if (!hit)
            {
                int covered;
                bool overlaps = OpeningView.Hits(
                    view.EyeX,
                    view.EyeY,
                    view.EyeZ,
                    view.Fx,
                    view.Fy,
                    view.Fz,
                    view.Rx,
                    view.Ry,
                    view.Rz,
                    view.Ux,
                    view.Uy,
                    view.Uz,
                    view.Fov,
                    view.Aspect,
                    view.RectX,
                    view.RectY,
                    view.RectW,
                    view.RectH,
                    bounds.center.x,
                    bounds.center.y,
                    bounds.center.z,
                    bounds.extents.x,
                    bounds.extents.y,
                    bounds.extents.z,
                    minU,
                    minV,
                    maxU,
                    maxV,
                    out covered);
                if (!overlaps)
                {
                    return 0f;
                }

                push = OpeningView.SidestepMetres * OpeningView.SidestepSteps;
            }

            float flatX = view.Rx;
            float flatZ = view.Rz;
            float flatLen = Mathf.Sqrt((flatX * flatX) + (flatZ * flatZ));
            if (flatLen < 0.0001f)
            {
                flatX = 1f;
                flatZ = 0f;
            }
            else
            {
                flatX /= flatLen;
                flatZ /= flatLen;
            }

            body.position += new Vector3(flatX * push, 0f, flatZ * push);
            Rigidbody rigidbody = body.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                rigidbody.position = body.position;
                if (holdStill)
                {
                    rigidbody.linearVelocity = Vector3.zero;
                    rigidbody.angularVelocity = Vector3.zero;
                }
            }

            if (!Moved.Contains(body))
            {
                Moved.Add(body);
            }

            return push;
        }

        private static float CarveStanding(
            Renderer renderer,
            Vector3 ship,
            ShotCam view,
            float minU,
            float minV,
            float maxU,
            float maxV,
            out int remaining)
        {
            remaining = 1;
            MeshFilter filter = renderer.GetComponent<MeshFilter>();
            if (filter == null || filter.sharedMesh == null || Carved.Contains(filter))
            {
                return 0f;
            }

            Mesh source = filter.sharedMesh;
            if (!source.isReadable)
            {
                return 0f;
            }

            Vector3[] verts = source.vertices;
            if (verts == null || verts.Length == 0)
            {
                return 0f;
            }

            Carved.Add(filter);
            int[] cluster = new int[CarveCap];
            float[] clusterDist = new float[CarveCap];
            int count = 0;
            Transform meshTransform = filter.transform;
            float minX = 0f;
            float minY = 0f;
            float minZ = 0f;
            float maxX = 0f;
            float maxY = 0f;
            float maxZ = 0f;
            float clusterReach = OpeningView.FloorClusterMetres * OpeningView.FloorClusterMetres;
            for (int vertIndex = 0; vertIndex < verts.Length; vertIndex++)
            {
                Vector3 world = meshTransform.TransformPoint(verts[vertIndex]);
                if (world.y < ship.y + StandingClearance)
                {
                    continue;
                }

                float dx = world.x - ship.x;
                float dz = world.z - ship.z;
                float dist = (dx * dx) + (dz * dz);
                if (dist > clusterReach)
                {
                    continue;
                }

                float viewU;
                float viewV;
                if (!OpeningView.ProjectAxes(
                    view.EyeX,
                    view.EyeY,
                    view.EyeZ,
                    view.Fx,
                    view.Fy,
                    view.Fz,
                    view.Rx,
                    view.Ry,
                    view.Rz,
                    view.Ux,
                    view.Uy,
                    view.Uz,
                    view.Fov,
                    world.x,
                    world.y,
                    world.z,
                    view.Aspect,
                    out viewU,
                    out viewV))
                {
                    continue;
                }

                float screenU;
                float screenV;
                OpeningView.ViewportToScreen(viewU, viewV, view.RectX, view.RectY, view.RectW, view.RectH, out screenU, out screenV);
                if (screenU < minU || screenU > maxU || screenV < minV || screenV > maxV)
                {
                    continue;
                }

                if (count < CarveCap)
                {
                    cluster[count] = vertIndex;
                    clusterDist[count] = dist;
                    count++;
                }
                else
                {
                    int farthest = 0;
                    for (int slot = 1; slot < CarveCap; slot++)
                    {
                        if (clusterDist[slot] > clusterDist[farthest])
                        {
                            farthest = slot;
                        }
                    }

                    if (dist >= clusterDist[farthest])
                    {
                        continue;
                    }

                    cluster[farthest] = vertIndex;
                    clusterDist[farthest] = dist;
                }

                if (count == 1)
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
            }

            if (count == 0)
            {
                remaining = 0;
                return 0f;
            }

            float cx = (minX + maxX) * 0.5f;
            float cy = (minY + maxY) * 0.5f;
            float cz = (minZ + maxZ) * 0.5f;
            float ex = (maxX - minX) * 0.5f;
            float ey = (maxY - minY) * 0.5f;
            float ez = (maxZ - minZ) * 0.5f;
            float push;
            if (!OpeningView.PushAside(
                view.EyeX,
                view.EyeY,
                view.EyeZ,
                view.Fx,
                view.Fy,
                view.Fz,
                view.Rx,
                view.Ry,
                view.Rz,
                view.Ux,
                view.Uy,
                view.Uz,
                view.Fov,
                view.Aspect,
                view.RectX,
                view.RectY,
                view.RectW,
                view.RectH,
                cx,
                cy,
                cz,
                ex,
                ey,
                ez,
                minU,
                minV,
                maxU,
                maxV,
                out push))
            {
                push = OpeningView.SidestepMetres * OpeningView.SidestepSteps;
            }

            float flatX = view.Rx;
            float flatZ = view.Rz;
            float flatLen = Mathf.Sqrt((flatX * flatX) + (flatZ * flatZ));
            if (flatLen < 0.0001f)
            {
                flatX = 1f;
                flatZ = 0f;
            }
            else
            {
                flatX /= flatLen;
                flatZ /= flatLen;
            }

            Mesh copy = UnityEngine.Object.Instantiate(source);
            string nextName = source.name;
            if (!nextName.EndsWith("_Opening", System.StringComparison.Ordinal))
            {
                nextName += "_Opening";
            }

            copy.name = nextName;
            Vector3[] moved = copy.vertices;
            remaining = 0;
            for (int clusterIndex = 0; clusterIndex < count; clusterIndex++)
            {
                int index = cluster[clusterIndex];
                Vector3 world = meshTransform.TransformPoint(moved[index]);
                world.x += flatX * push;
                world.z += flatZ * push;
                moved[index] = meshTransform.InverseTransformPoint(world);
                float afterU;
                float afterV;
                if (!OpeningView.ProjectAxes(
                    view.EyeX,
                    view.EyeY,
                    view.EyeZ,
                    view.Fx,
                    view.Fy,
                    view.Fz,
                    view.Rx,
                    view.Ry,
                    view.Rz,
                    view.Ux,
                    view.Uy,
                    view.Uz,
                    view.Fov,
                    world.x,
                    world.y,
                    world.z,
                    view.Aspect,
                    out afterU,
                    out afterV))
                {
                    remaining++;
                    continue;
                }

                float afterScreenU;
                float afterScreenV;
                OpeningView.ViewportToScreen(afterU, afterV, view.RectX, view.RectY, view.RectW, view.RectH, out afterScreenU, out afterScreenV);
                if (afterScreenU >= minU && afterScreenU <= maxU && afterScreenV >= minV && afterScreenV <= maxV)
                {
                    remaining++;
                }
            }

            copy.vertices = moved;
            copy.RecalculateBounds();
            filter.sharedMesh = copy;
            return push;
        }

        private static void ClearListed(
            Vector3 ship,
            ShotCam view,
            float minU,
            float minV,
            float maxU,
            float maxV,
            bool holdStill,
            bool log)
        {
            Entities.Clear();
            EntityPush.Clear();
            WaveManager[] waves = UnityEngine.Object.FindObjectsByType<WaveManager>();
            if (waves != null)
            {
                for (int waveIndex = 0; waveIndex < waves.Length; waveIndex++)
                {
                    WaveManager wave = waves[waveIndex];
                    if (wave != null)
                    {
                        wave.CollectLive(Entities);
                    }
                }
            }

            ArenaHazard.CollectLive(Entities);
            for (int entityIndex = 0; entityIndex < Entities.Count; entityIndex++)
            {
                Component entity = Entities[entityIndex];
                if (entity == null)
                {
                    EntityPush.Add(0f);
                    continue;
                }

                float push = NudgeRoot(entity.transform, view, minU, minV, maxU, maxV, holdStill, true);
                EntityPush.Add(push);
            }

            if (!log)
            {
                return;
            }

            for (int entityIndex = 0; entityIndex < Entities.Count; entityIndex++)
            {
                Component entity = Entities[entityIndex];
                if (entity == null)
                {
                    continue;
                }

                float pushed = entityIndex < EntityPush.Count ? EntityPush[entityIndex] : 0f;
                NoteEntity(entity, view, minU, minV, maxU, maxV, pushed);
            }
        }

        private static void NoteEntity(Component entity, ShotCam playerView, float minU, float minV, float maxU, float maxV, float push)
        {
            Transform body = entity.transform;
            int layer = entity.gameObject.layer;
            ShotCam view = CameraFor(layer, playerView);
            Bounds bounds = VisualBounds(body);
            int behind;
            bool overlap = OpeningView.Hits(
                view.EyeX,
                view.EyeY,
                view.EyeZ,
                view.Fx,
                view.Fy,
                view.Fz,
                view.Rx,
                view.Ry,
                view.Rz,
                view.Ux,
                view.Uy,
                view.Uz,
                view.Fov,
                view.Aspect,
                view.RectX,
                view.RectY,
                view.RectW,
                view.RectH,
                bounds.center.x,
                bounds.center.y,
                bounds.center.z,
                bounds.extents.x,
                bounds.extents.y,
                bounds.extents.z,
                minU,
                minV,
                maxU,
                maxV,
                out behind);
            float boxMinU;
            float boxMinV;
            float boxMaxU;
            float boxMaxV;
            ScreenBox(bounds, view, out boxMinU, out boxMinV, out boxMaxU, out boxMaxV);
            int overlapBit = overlap ? 1 : 0;
            Notes.Add(
                "AgrCapture keep-out verify name="
                + entity.gameObject.name
                + " layer="
                + LayerText(layer)
                + " camera="
                + view.Name
                + " kind=entity screen="
                + RectText(boxMinU, boxMinV, boxMaxU, boxMaxV)
                + " overlap="
                + overlapBit.ToString(CultureInfo.InvariantCulture)
                + " push="
                + push.ToString("0.00", CultureInfo.InvariantCulture)
                + " behind="
                + behind.ToString(CultureInfo.InvariantCulture));
        }

        private static Bounds VisualBounds(Transform body)
        {
            Bounds bounds;
            if (body == null || !TryRendererBounds(body, out bounds))
            {
                Vector3 origin = body != null ? body.position : Vector3.zero;
                bounds = new Bounds(origin, new Vector3(2.4f, 2.4f, 2.4f));
            }

            if (body != null)
            {
                Collider[] colliders = body.GetComponentsInChildren<Collider>(true);
                for (int colliderIndex = 0; colliderIndex < colliders.Length; colliderIndex++)
                {
                    Collider collider = colliders[colliderIndex];
                    if (collider == null || !collider.enabled)
                    {
                        continue;
                    }

                    bounds.Encapsulate(collider.bounds);
                }
            }

            Vector3 grow = bounds.size * (VisualMeshPad - 1f);
            bounds.size += grow;
            return bounds;
        }

        private static void RepairPlayMask()
        {
            Camera[] cameras = UnityEngine.Object.FindObjectsByType<Camera>();
            if (cameras == null)
            {
                return;
            }

            int decor = DecorCameraStack.LayerIndex();
            int playMask = ~0 & ~(1 << decor);
            for (int cameraIndex = 0; cameraIndex < cameras.Length; cameraIndex++)
            {
                Camera cam = cameras[cameraIndex];
                if (cam == null || cam.gameObject == null || cam.gameObject.name != "Main Camera")
                {
                    continue;
                }

                if (cam.cullingMask == 0)
                {
                    cam.cullingMask = playMask;
                }
            }
        }

        private static bool TryFrame(
            out Vector3 ship,
            out ShotCam view,
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

            Cams.Clear();
            Camera[] cameras = UnityEngine.Object.FindObjectsByType<Camera>();
            for (int cameraIndex = 0; cameraIndex < cameras.Length; cameraIndex++)
            {
                ShotCam shot;
                if (TryShotCam(cameras[cameraIndex], ship, out shot))
                {
                    Cams.Add(shot);
                }
            }

            if (Cams.Count == 0)
            {
                Cams.Add(FallbackCam(ship));
            }

            view = CameraFor(0, FallbackCam(ship));
            float cx = ship.x;
            float cy = ship.y + OpeningView.PlayerCenterY;
            float cz = ship.z;
            float ex = OpeningView.PlayerHalfX;
            float ey = OpeningView.PlayerHalfY;
            float ez = OpeningView.PlayerHalfZ;
            if (player != null)
            {
                Bounds bounds;
                if (TryRendererBounds(player.transform, out bounds) && bounds.extents.sqrMagnitude > 0.01f)
                {
                    cx = bounds.center.x;
                    cy = bounds.center.y;
                    cz = bounds.center.z;
                    ex = ClampHalf(bounds.extents.x, OpeningView.PlayerHalfX);
                    ey = ClampHalf(bounds.extents.y, OpeningView.PlayerHalfY);
                    ez = ClampHalf(bounds.extents.z, OpeningView.PlayerHalfZ);
                }
            }

            OpeningView.ScreenPlayerRect(
                view.EyeX,
                view.EyeY,
                view.EyeZ,
                view.Fx,
                view.Fy,
                view.Fz,
                view.Rx,
                view.Ry,
                view.Rz,
                view.Ux,
                view.Uy,
                view.Uz,
                view.Fov,
                view.Aspect,
                view.RectX,
                view.RectY,
                view.RectW,
                view.RectH,
                cx,
                cy,
                cz,
                ex,
                ey,
                ez,
                out minU,
                out minV,
                out maxU,
                out maxV);
            return view.Ready;
        }

        private static ShotCam CameraFor(int layer, ShotCam fallback)
        {
            ShotCam best = fallback;
            bool found = false;
            int bit = 0;
            if (layer >= 0 && layer <= 30)
            {
                bit = 1 << layer;
            }

            for (int i = 0; i < Cams.Count; i++)
            {
                ShotCam cam = Cams[i];
                bool playCam = cam.Name == "Main Camera";
                if (cam.Mask == 0 && !playCam)
                {
                    continue;
                }

                bool draws = layer < 0 || layer > 30 || (cam.Mask & bit) != 0 || (playCam && cam.Mask == 0);
                if (!draws)
                {
                    continue;
                }

                if (!found || cam.Depth > best.Depth)
                {
                    best = cam;
                    found = true;
                }
            }

            return best;
        }

        private static bool TryShotCam(Camera cam, Vector3 ship, out ShotCam shot)
        {
            shot = new ShotCam();
            if (cam == null || !cam.enabled || cam.gameObject == null)
            {
                return false;
            }

            Transform xform = cam.transform;
            Vector3 eye = xform.position;
            Vector3 forward = xform.forward;
            Vector3 right = xform.right;
            Vector3 up = xform.up;
            if (forward.sqrMagnitude < 0.01f)
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
                OpeningView.Basis(eye.x, eye.y, eye.z, ship.x, ship.y, ship.z, out fx, out fy, out fz, out rx, out ry, out rz, out ux, out uy, out uz);
                forward = new Vector3(fx, fy, fz);
                right = new Vector3(rx, ry, rz);
                up = new Vector3(ux, uy, uz);
            }

            Rect rect = cam.rect;
            float rectW = rect.width;
            float rectH = rect.height;
            if (rectW < 0.001f)
            {
                rectW = 1f;
            }

            if (rectH < 0.001f)
            {
                rectH = 1f;
            }

            float aspect = ArenaFraming.ReferenceAspect;

            shot.Name = cam.gameObject.name;
            shot.Mask = cam.cullingMask;
            shot.Depth = cam.depth;
            shot.EyeX = eye.x;
            shot.EyeY = eye.y;
            shot.EyeZ = eye.z;
            shot.Fx = forward.x;
            shot.Fy = forward.y;
            shot.Fz = forward.z;
            shot.Rx = right.x;
            shot.Ry = right.y;
            shot.Rz = right.z;
            shot.Ux = up.x;
            shot.Uy = up.y;
            shot.Uz = up.z;
            shot.Fov = cam.fieldOfView;
            shot.Aspect = aspect;
            shot.RectX = rect.x;
            shot.RectY = rect.y;
            shot.RectW = rectW;
            shot.RectH = rectH;
            shot.Ready = true;
            return true;
        }

        private static ShotCam FallbackCam(Vector3 ship)
        {
            Vector3 eye = ship + new Vector3(0f, DecorDepthRules.CameraOffsetY, DecorDepthRules.CameraOffsetZ);
            float fx;
            float fy;
            float fz;
            float rx;
            float ry;
            float rz;
            float ux;
            float uy;
            float uz;
            OpeningView.Basis(eye.x, eye.y, eye.z, ship.x, ship.y, ship.z, out fx, out fy, out fz, out rx, out ry, out rz, out ux, out uy, out uz);
            float aspect = ArenaFraming.ReferenceAspect;

            ShotCam shot = new ShotCam();
            shot.Name = "PlayFallback";
            shot.Mask = ~0;
            shot.Depth = 0f;
            shot.EyeX = eye.x;
            shot.EyeY = eye.y;
            shot.EyeZ = eye.z;
            shot.Fx = fx;
            shot.Fy = fy;
            shot.Fz = fz;
            shot.Rx = rx;
            shot.Ry = ry;
            shot.Rz = rz;
            shot.Ux = ux;
            shot.Uy = uy;
            shot.Uz = uz;
            shot.Fov = OpeningView.FovDegrees;
            shot.Aspect = aspect;
            shot.RectX = 0f;
            shot.RectY = 0f;
            shot.RectW = 1f;
            shot.RectH = 1f;
            shot.Ready = true;
            return shot;
        }

        private static void NoteCameras()
        {
            Camera[] cameras = UnityEngine.Object.FindObjectsByType<Camera>();
            if (cameras == null || cameras.Length == 0)
            {
                Notes.Add("AgrCapture keep-out camera name=none layer=0 enabled=0 depth=0 mask=0 rect=0,0,0,0");
                return;
            }

            for (int cameraIndex = 0; cameraIndex < cameras.Length; cameraIndex++)
            {
                Camera cam = cameras[cameraIndex];
                if (cam == null || cam.gameObject == null)
                {
                    continue;
                }

                int enabledBit = cam.enabled ? 1 : 0;
                Rect frame = cam.rect;
                Notes.Add(
                    "AgrCapture keep-out camera name="
                    + cam.gameObject.name
                    + " layer="
                    + LayerText(cam.gameObject.layer)
                    + " enabled="
                    + enabledBit.ToString(CultureInfo.InvariantCulture)
                    + " depth="
                    + cam.depth.ToString("0.00", CultureInfo.InvariantCulture)
                    + " mask="
                    + cam.cullingMask.ToString(CultureInfo.InvariantCulture)
                    + " rect="
                    + RectText(frame.x, frame.y, frame.x + frame.width, frame.y + frame.height));
            }
        }

        private static void ScreenBox(Bounds bounds, ShotCam view, out float minU, out float minV, out float maxU, out float maxV)
        {
            minU = 0f;
            minV = 0f;
            maxU = 0f;
            maxV = 0f;
            bool any = false;
            Vector3 center = bounds.center;
            Vector3 extents = bounds.extents;
            for (int corner = 0; corner < 8; corner++)
            {
                float sx = (corner & 1) == 0 ? -extents.x : extents.x;
                float sy = (corner & 2) == 0 ? -extents.y : extents.y;
                float sz = (corner & 4) == 0 ? -extents.z : extents.z;
                float viewU;
                float viewV;
                if (!OpeningView.ProjectAxes(
                    view.EyeX,
                    view.EyeY,
                    view.EyeZ,
                    view.Fx,
                    view.Fy,
                    view.Fz,
                    view.Rx,
                    view.Ry,
                    view.Rz,
                    view.Ux,
                    view.Uy,
                    view.Uz,
                    view.Fov,
                    center.x + sx,
                    center.y + sy,
                    center.z + sz,
                    view.Aspect,
                    out viewU,
                    out viewV))
                {
                    continue;
                }

                float screenU;
                float screenV;
                OpeningView.ViewportToScreen(viewU, viewV, view.RectX, view.RectY, view.RectW, view.RectH, out screenU, out screenV);
                if (!any)
                {
                    minU = screenU;
                    maxU = screenU;
                    minV = screenV;
                    maxV = screenV;
                    any = true;
                }
                else
                {
                    if (screenU < minU)
                    {
                        minU = screenU;
                    }

                    if (screenU > maxU)
                    {
                        maxU = screenU;
                    }

                    if (screenV < minV)
                    {
                        minV = screenV;
                    }

                    if (screenV > maxV)
                    {
                        maxV = screenV;
                    }
                }
            }
        }

        private static Transform MovableRoot(Transform node, Transform belt)
        {
            if (belt != null)
            {
                return belt;
            }

            Transform cursor = node;
            while (cursor != null)
            {
                if (cursor.GetComponent<Asteroid>() != null
                    || cursor.GetComponent<EnemySeeker>() != null
                    || cursor.GetComponent<Pickup>() != null)
                {
                    return cursor;
                }

                cursor = cursor.parent;
            }

            return node;
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

        private static bool IsPlayer(GameObject host)
        {
            Transform cursor = host != null ? host.transform : null;
            while (cursor != null)
            {
                if (cursor.CompareTag(GameTags.Player))
                {
                    return true;
                }

                cursor = cursor.parent;
            }

            return false;
        }

        private static bool IsDust(GameObject host)
        {
            Transform cursor = host.transform;
            while (cursor != null)
            {
                if (cursor.name == "NearDust")
                {
                    return true;
                }

                cursor = cursor.parent;
            }

            return false;
        }

        private static bool IsShell(Bounds bounds)
        {
            return bounds.extents.x > ShellExtent && bounds.extents.y > ShellExtent && bounds.extents.z > ShellExtent;
        }

        private static bool IsFloor(Bounds bounds)
        {
            return bounds.extents.x >= FloorExtent || bounds.extents.z >= FloorExtent;
        }

        private static bool TryRendererBounds(Transform body, out Bounds bounds)
        {
            bounds = new Bounds(body.position, Vector3.zero);
            Renderer[] renderers = body.GetComponentsInChildren<Renderer>(true);
            bool any = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || renderer.gameObject.name == "ContactBlob")
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

        private static string LayerText(int layer)
        {
            string layerName = LayerMask.LayerToName(layer);
            if (string.IsNullOrEmpty(layerName))
            {
                return layer.ToString(CultureInfo.InvariantCulture);
            }

            return layerName;
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

        private struct ShotCam
        {
            public string Name;
            public int Mask;
            public float Depth;
            public float EyeX;
            public float EyeY;
            public float EyeZ;
            public float Fx;
            public float Fy;
            public float Fz;
            public float Rx;
            public float Ry;
            public float Rz;
            public float Ux;
            public float Uy;
            public float Uz;
            public float Fov;
            public float Aspect;
            public float RectX;
            public float RectY;
            public float RectW;
            public float RectH;
            public bool Ready;
        }
    }
}
