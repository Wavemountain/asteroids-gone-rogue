using System.Collections.Generic;
using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Arena floor marked by <see cref="ContentFactory"/> via <see cref="FloorPlate.Adopt"/>.
    /// Mines, beams, hazards, pickups, and other thin gameplay meshes are not a floor.
    /// </summary>
    public static class FloorSurface
    {
        public const float Lift = 0.035f;
        public const float MinLift = 0.02f;
        public const float MaxLift = 0.05f;

        private static int _frame = -1;
        private static FloorPlate[] _plates;

        public static float PlaceY(float surface, float ceiling)
        {
            float y = surface + Lift;
            float band = MaxLift;
            if (y > surface + band)
            {
                y = surface + band;
            }

            float cap = ceiling - 0.05f;
            if (y > cap)
            {
                y = cap;
            }

            if (y < surface + MinLift)
            {
                y = surface + MinLift;
            }

            return y;
        }

        public static bool IsFloor(Renderer renderer)
        {
            if (renderer == null || renderer.gameObject == null)
            {
                return false;
            }

            FloorPlate plate = renderer.GetComponent<FloorPlate>();
            return plate != null && plate.Ready;
        }

        /// <summary>
        /// World Y of the real arena floor under XZ. Only <see cref="FloorPlate"/> counts.
        /// </summary>
        public static bool TryTop(float x, float z, float ceiling, out float top)
        {
            top = -4f;
            bool any = false;
            FloorPlate[] plates = Plates();
            if (plates == null)
            {
                return false;
            }

            for (int i = 0; i < plates.Length; i++)
            {
                FloorPlate plate = plates[i];
                if (plate == null || !plate.Ready || !plate.gameObject.activeInHierarchy)
                {
                    continue;
                }

                float y;
                if (!plate.Contains(x, z, out y))
                {
                    continue;
                }

                if (y >= ceiling || y < -2f)
                {
                    continue;
                }

                if (!any || y > top)
                {
                    top = y;
                    any = true;
                }
            }

            return any;
        }

        private static FloorPlate[] Plates()
        {
            if (_frame != Time.frameCount || _plates == null)
            {
                _frame = Time.frameCount;
                _plates = UnityEngine.Object.FindObjectsByType<FloorPlate>();
            }

            return _plates;
        }

        /// <summary>
        /// Drop a cached list that still points at a floor retired this frame.
        /// </summary>
        public static void Invalidate()
        {
            _frame = -1;
            _plates = null;
        }
    }

    /// <summary>
    /// Explicit arena-floor whitelist. ContentFactory marks the play surface
    /// after the mesh is dressed. Hazards, mines, beams, and decals are never marked.
    /// </summary>
    public sealed class FloorPlate : MonoBehaviour
    {
        public float LocalY;
        public float MinX;
        public float MaxX;
        public float MinZ;
        public float MaxZ;
        public bool Ready;

        private Vector3[] _surface;

        private const float FloorBand = 0.25f;
        private const float BoundsPad = 0.35f;

        public bool Contains(float worldX, float worldZ, out float worldY)
        {
            worldY = 0f;
            if (!Ready)
            {
                return false;
            }

            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null && !renderer.enabled)
            {
                return false;
            }

            Vector3 local = transform.InverseTransformPoint(new Vector3(worldX, transform.position.y, worldZ));
            if (local.x < MinX || local.x > MaxX || local.z < MinZ || local.z > MaxZ)
            {
                return false;
            }

            float localY = LocalY;
            float sampled;
            if (TrySurface(local.x, local.z, out sampled))
            {
                localY = sampled;
            }

            worldY = transform.TransformPoint(new Vector3(local.x, localY, local.z)).y;
            return true;
        }

        /// <summary>
        /// Highest triangle under this local XZ. A curved plate is not one Y.
        /// </summary>
        private bool TrySurface(float x, float z, out float localY)
        {
            localY = 0f;
            Vector3[] surface = _surface;
            if (surface == null || surface.Length < 3)
            {
                return false;
            }

            bool hit = false;
            for (int i = 0; i + 2 < surface.Length; i += 3)
            {
                float y;
                if (!WorldFloorExposure.TriangleHeight(
                    x,
                    z,
                    surface[i].x,
                    surface[i].y,
                    surface[i].z,
                    surface[i + 1].x,
                    surface[i + 1].y,
                    surface[i + 1].z,
                    surface[i + 2].x,
                    surface[i + 2].y,
                    surface[i + 2].z,
                    out y))
                {
                    continue;
                }

                if (!hit || y > localY)
                {
                    localY = y;
                    hit = true;
                }
            }

            return hit;
        }

        private void RememberSurface(System.Collections.Generic.List<Vector3> triangles)
        {
            if (triangles == null || triangles.Count < 3)
            {
                _surface = null;
                return;
            }

            _surface = triangles.ToArray();
        }

        public static void Adopt(GameObject visual)
        {
            if (visual == null)
            {
                return;
            }

            Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
            if (renderers == null)
            {
                return;
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                if (!TryConfigure(renderer))
                {
                    FloorPlate stale = renderer.GetComponent<FloorPlate>();
                    if (stale != null)
                    {
                        stale.Ready = false;
                    }
                }
            }
        }

        private static bool TryConfigure(Renderer renderer)
        {
            if (renderer is ParticleSystemRenderer || renderer is LineRenderer || renderer is TrailRenderer)
            {
                return false;
            }

            if (Denied(renderer.gameObject.name))
            {
                return false;
            }

            MeshFilter filter = renderer.GetComponent<MeshFilter>();
            Mesh mesh = filter != null ? filter.sharedMesh : null;
            if (mesh != null && mesh.isReadable && mesh.vertices != null && mesh.vertices.Length > 0)
            {
                return ConfigureReadable(renderer, mesh);
            }

            return ConfigureBounds(renderer);
        }

        private static bool ConfigureReadable(Renderer renderer, Mesh mesh)
        {
            Vector3[] verts = mesh.vertices;
            Material[] mats = renderer.sharedMaterials;
            int subCount = mesh.subMeshCount;
            if (subCount < 1)
            {
                subCount = 1;
            }

            Transform xform = renderer.transform;
            float minWorld = 0f;
            bool any = false;
            System.Collections.Generic.List<Vector3> surface = new System.Collections.Generic.List<Vector3>();
            for (int sub = 0; sub < subCount; sub++)
            {
                if (SkipSubmesh(mats, sub))
                {
                    continue;
                }

                int[] tris = mesh.GetTriangles(sub);
                if (tris == null)
                {
                    continue;
                }

                for (int t = 0; t < tris.Length; t++)
                {
                    int index = tris[t];
                    if (index < 0 || index >= verts.Length)
                    {
                        continue;
                    }

                    float y = xform.TransformPoint(verts[index]).y;
                    if (!any || y < minWorld)
                    {
                        minWorld = y;
                        any = true;
                    }
                }
            }

            if (!any)
            {
                return false;
            }

            float topWorld = minWorld;
            float localY = 0f;
            float minX = 0f;
            float maxX = 0f;
            float minZ = 0f;
            float maxZ = 0f;
            bool picked = false;
            for (int sub = 0; sub < subCount; sub++)
            {
                if (SkipSubmesh(mats, sub))
                {
                    continue;
                }

                int[] tris = mesh.GetTriangles(sub);
                if (tris == null)
                {
                    continue;
                }

                for (int t = 0; t + 2 < tris.Length; t += 3)
                {
                    int i0 = tris[t];
                    int i1 = tris[t + 1];
                    int i2 = tris[t + 2];
                    if (i0 < 0 || i1 < 0 || i2 < 0 || i0 >= verts.Length || i1 >= verts.Length || i2 >= verts.Length)
                    {
                        continue;
                    }

                    Vector3 v0 = verts[i0];
                    Vector3 v1 = verts[i1];
                    Vector3 v2 = verts[i2];
                    float y0 = xform.TransformPoint(v0).y;
                    float y1 = xform.TransformPoint(v1).y;
                    float y2 = xform.TransformPoint(v2).y;
                    float high = y0;
                    if (y1 > high)
                    {
                        high = y1;
                    }

                    if (y2 > high)
                    {
                        high = y2;
                    }

                    if (high > minWorld + FloorBand)
                    {
                        continue;
                    }

                    surface.Add(v0);
                    surface.Add(v1);
                    surface.Add(v2);
                    Consider(v0, y0, ref picked, ref topWorld, ref localY, ref minX, ref maxX, ref minZ, ref maxZ);
                    Consider(v1, y1, ref picked, ref topWorld, ref localY, ref minX, ref maxX, ref minZ, ref maxZ);
                    Consider(v2, y2, ref picked, ref topWorld, ref localY, ref minX, ref maxX, ref minZ, ref maxZ);
                }
            }

            if (!picked)
            {
                return false;
            }

            if ((maxX - minX) < 0.4f && (maxZ - minZ) < 0.4f)
            {
                return false;
            }

            Write(renderer, localY, minX - BoundsPad, maxX + BoundsPad, minZ - BoundsPad, maxZ + BoundsPad);
            FloorPlate plate = renderer.GetComponent<FloorPlate>();
            if (plate != null)
            {
                plate.RememberSurface(surface);
            }

            return true;
        }

        private static void Consider(
            Vector3 local,
            float worldY,
            ref bool picked,
            ref float topWorld,
            ref float localY,
            ref float minX,
            ref float maxX,
            ref float minZ,
            ref float maxZ)
        {
            if (!picked)
            {
                topWorld = worldY;
                localY = local.y;
                minX = local.x;
                maxX = local.x;
                minZ = local.z;
                maxZ = local.z;
                picked = true;
                return;
            }

            if (worldY > topWorld)
            {
                topWorld = worldY;
                localY = local.y;
            }

            if (local.x < minX)
            {
                minX = local.x;
            }

            if (local.x > maxX)
            {
                maxX = local.x;
            }

            if (local.z < minZ)
            {
                minZ = local.z;
            }

            if (local.z > maxZ)
            {
                maxZ = local.z;
            }
        }

        private static bool ConfigureBounds(Renderer renderer)
        {
            string materialName = renderer.sharedMaterial != null ? renderer.sharedMaterial.name : string.Empty;
            if (Denied(materialName))
            {
                return false;
            }

            Bounds box = renderer.bounds;
            if (box.extents.y > 1.2f)
            {
                return false;
            }

            if (box.extents.x < 0.4f && box.extents.z < 0.4f)
            {
                return false;
            }

            Transform xform = renderer.transform;
            Vector3 localTop = xform.InverseTransformPoint(new Vector3(box.center.x, box.max.y, box.center.z));
            Vector3 c0 = xform.InverseTransformPoint(new Vector3(box.min.x, box.center.y, box.min.z));
            Vector3 c1 = xform.InverseTransformPoint(new Vector3(box.max.x, box.center.y, box.max.z));
            float minX = c0.x < c1.x ? c0.x : c1.x;
            float maxX = c0.x > c1.x ? c0.x : c1.x;
            float minZ = c0.z < c1.z ? c0.z : c1.z;
            float maxZ = c0.z > c1.z ? c0.z : c1.z;
            Write(renderer, localTop.y, minX - BoundsPad, maxX + BoundsPad, minZ - BoundsPad, maxZ + BoundsPad);
            return true;
        }

        private static bool SkipSubmesh(Material[] mats, int sub)
        {
            if (mats == null || sub < 0 || sub >= mats.Length || mats[sub] == null)
            {
                return false;
            }

            return Denied(mats[sub].name);
        }

        private static bool Denied(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            string[] tokens =
            {
                "Beam",
                "Accent",
                "Glow",
                "Mine",
                "Hazard",
                "Spike",
                "Rim",
                "Wash",
                "Decal",
                "Grid",
                "Lip",
                "Pylon",
                "Pillar",
            };
            for (int i = 0; i < tokens.Length; i++)
            {
                if (value.IndexOf(tokens[i], System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static void Write(Renderer renderer, float localY, float minX, float maxX, float minZ, float maxZ)
        {
            FloorPlate plate = renderer.GetComponent<FloorPlate>();
            if (plate == null)
            {
                plate = renderer.gameObject.AddComponent<FloorPlate>();
            }

            plate.LocalY = localY;
            plate.MinX = minX;
            plate.MaxX = maxX;
            plate.MinZ = minZ;
            plate.MaxZ = maxZ;
            plate.Ready = true;
        }
    }

    /// <summary>
    /// Flat dark disc under a body. The shader keeps RGB black and fades alpha
    /// to the edge. Colour and alpha stay put when Reduce is on.
    /// </summary>
    public sealed class ContactBlob : MonoBehaviour
    {
        public const float Alpha = 0.9f;
        public const float RadiusPad = 2f;
        public const float MinRadius = 0.45f;

        /// <summary>
        /// Below transparent gameplay and decor beams (3000) and above the grid
        /// (2900). Opaque mines write depth first, so the blob fails the depth
        /// test where a mine or raised strip is closer than the floor.
        /// </summary>
        public const int RenderQueue = 2990;

        private Transform _host;
        private EntityId _hostId;
        private static Material _shared;
        private static readonly Dictionary<EntityId, ContactBlob> Live = new Dictionary<EntityId, ContactBlob>();

        public static void Attach(Transform host)
        {
            if (host == null)
            {
                return;
            }

            EntityId id = host.GetEntityId();
            ContactBlob prior;
            if (Live.TryGetValue(id, out prior) && prior != null)
            {
                return;
            }

            GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            if (quad == null)
            {
                return;
            }

            quad.name = "ContactBlob";
            quad.layer = DecorCameraStack.LayerIndex();
            Collider collider = quad.GetComponent<Collider>();
            if (collider != null)
            {
                // CreatePrimitive adds a concave MeshCollider. Destroy is deferred,
                // so the next physics step logs the error under the body's Rigidbody.
                DestroyImmediate(collider);
            }

            Renderer renderer = quad.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = SharedMaterial();
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = false;
            }

            ContactBlob blob = quad.AddComponent<ContactBlob>();
            if (blob == null)
            {
                return;
            }

            blob._host = host;
            blob._hostId = id;
            Live[id] = blob;
            blob.Pin();
        }

        /// <summary>
        /// Match the shared disc to the floor that was just painted.
        /// A dark World 2 plate cannot take the full 0.9 alpha.
        /// </summary>
        public static void AdoptFloor(int world, float floorR, float floorG, float floorB)
        {
            float alpha = WorldFloorExposure.ContactAlpha(world, floorR, floorG, floorB, Alpha);
            Material material = SharedMaterial();
            if (material == null)
            {
                return;
            }

            material.color = new Color(0f, 0f, 0f, alpha);
        }

        private void LateUpdate()
        {
            Pin();
        }

        private void OnDestroy()
        {
            ContactBlob current;
            if (Live.TryGetValue(_hostId, out current) && current == this)
            {
                Live.Remove(_hostId);
            }
        }

        private void Pin()
        {
            Transform host = _host;
            if (host == null)
            {
                Renderer dead = GetComponent<Renderer>();
                if (dead != null)
                {
                    dead.enabled = false;
                }

                Destroy(gameObject);
                return;
            }

            // A pooled or despawned host is still a transform. Leave the disc
            // off until that body is in the arena again.
            if (!host.gameObject.activeInHierarchy)
            {
                Renderer hidden = GetComponent<Renderer>();
                if (hidden != null)
                {
                    hidden.enabled = false;
                }

                return;
            }

            Renderer shown = GetComponent<Renderer>();
            if (shown != null && !shown.enabled)
            {
                shown.enabled = true;
            }

            // World scale, not a child of a rotated body, so the quad stays a
            // disc on the floor instead of a sheared parallelogram.
            gameObject.layer = DecorCameraStack.LayerIndex();
            float radius = Footprint(host) * RadiusPad;
            if (radius < MinRadius)
            {
                radius = MinRadius;
            }

            float ceiling = host.position.y;
            float surface;
            float y;
            if (FloorSurface.TryTop(host.position.x, host.position.z, ceiling, out surface))
            {
                y = FloorSurface.PlaceY(surface, ceiling);
            }
            else
            {
                y = 0.06f;
                if (y > ceiling - 0.08f)
                {
                    y = ceiling - 0.08f;
                }
            }

            float clear = radius * 0.012f;
            if (clear > 0.05f)
            {
                clear = 0.05f;
            }

            y += clear;
            float diameter = radius * 2f;
            transform.SetParent(null, false);
            transform.localScale = new Vector3(diameter, diameter, 1f);
            transform.SetPositionAndRotation(
                new Vector3(host.position.x, y, host.position.z),
                Quaternion.Euler(90f, 0f, 0f));
        }

        private static float Footprint(Transform host)
        {
            float radius = 0.8f;
            if (host == null)
            {
                return radius;
            }

            float scale = host.lossyScale.x;
            if (host.lossyScale.y > scale)
            {
                scale = host.lossyScale.y;
            }

            if (host.lossyScale.z > scale)
            {
                scale = host.lossyScale.z;
            }

            if (scale < 0.001f)
            {
                scale = 1f;
            }

            SphereCollider sphere = host.GetComponent<SphereCollider>();
            if (sphere != null)
            {
                radius = sphere.radius * scale;
            }

            CapsuleCollider capsule = host.GetComponent<CapsuleCollider>();
            if (capsule != null)
            {
                float radial = capsule.radius;
                float half = capsule.height * 0.5f;
                if (half > radial)
                {
                    radial = half;
                }

                radius = radial * scale;
            }

            Renderer[] renderers = host.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (!SpawnContrastDisc.HullRenderer(renderer))
                {
                    continue;
                }

                float wide = renderer.bounds.extents.x;
                if (renderer.bounds.extents.z > wide)
                {
                    wide = renderer.bounds.extents.z;
                }

                if (wide > radius)
                {
                    radius = wide;
                }
            }

            return radius;
        }

        private static Material SharedMaterial()
        {
            if (_shared != null)
            {
                return _shared;
            }

            Shader shader = Shader.Find("AGR/BlobShadow");
            if (shader == null)
            {
                shader = Shader.Find("Unlit/Transparent");
            }

            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            _shared = new Material(shader);
            _shared.name = "Mat_ContactBlob";
            _shared.color = new Color(0f, 0f, 0f, Alpha);
            _shared.renderQueue = RenderQueue;
            return _shared;
        }
    }
}
