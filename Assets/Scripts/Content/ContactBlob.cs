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

            worldY = transform.TransformPoint(new Vector3(local.x, LocalY, local.z)).y;
            return true;
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

                for (int t = 0; t < tris.Length; t++)
                {
                    int index = tris[t];
                    if (index < 0 || index >= verts.Length)
                    {
                        continue;
                    }

                    Vector3 local = verts[index];
                    Vector3 world = xform.TransformPoint(local);
                    if (world.y > minWorld + FloorBand)
                    {
                        continue;
                    }

                    if (!picked)
                    {
                        topWorld = world.y;
                        localY = local.y;
                        minX = local.x;
                        maxX = local.x;
                        minZ = local.z;
                        maxZ = local.z;
                        picked = true;
                    }
                    else
                    {
                        if (world.y > topWorld)
                        {
                            topWorld = world.y;
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
            return true;
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
        private static Material _shared;

        public static void Attach(Transform host)
        {
            if (host == null || host.Find("ContactBlob") != null)
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
            quad.transform.SetParent(host, false);
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
            if (blob != null)
            {
                blob._host = host;
                blob.Pin();
            }
        }

        private void LateUpdate()
        {
            Pin();
        }

        private void Pin()
        {
            Transform host = _host;
            if (host == null)
            {
                host = transform.parent;
            }

            if (host == null)
            {
                return;
            }

            // Decor camera, queue 2990. The floor, mines, and beams share that
            // depth buffer. Y is the real floor top, so a mine or beam closer
            // to the camera rejects the blob. Gameplay draws afterwards.
            gameObject.layer = DecorCameraStack.LayerIndex();
            float radius = Footprint(host) * RadiusPad;
            if (radius < MinRadius)
            {
                radius = MinRadius;
            }

            ApplyScale(host, radius);
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

            transform.SetPositionAndRotation(
                new Vector3(host.position.x, y, host.position.z),
                Quaternion.Euler(90f, 0f, 0f));
        }

        private void ApplyScale(Transform host, float radius)
        {
            float axis = 1f;
            if (host != null)
            {
                axis = host.lossyScale.x;
                if (host.lossyScale.y > axis)
                {
                    axis = host.lossyScale.y;
                }

                if (host.lossyScale.z > axis)
                {
                    axis = host.lossyScale.z;
                }
            }

            if (axis < 0.001f)
            {
                axis = 1f;
            }

            float local = (radius * 2f) / axis;
            transform.localScale = new Vector3(local, local, 1f);
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
