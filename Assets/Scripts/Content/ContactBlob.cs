using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Top of the floor or plate under a world XZ. Raycast plus renderer
    /// bounds, so a mesh without a collider still counts. Gameplay bodies,
    /// the spawn disc, and contact blobs are not a floor.
    /// </summary>
    public static class FloorSurface
    {
        public const float Lift = 0.035f;
        public const float MinLift = 0.02f;
        public const float MaxLift = 0.05f;

        private static int _frame = -1;
        private static Renderer[] _renderers;

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

        public static bool TryTop(float x, float z, float ceiling, out float top)
        {
            top = -4f;
            bool any = false;
            float hitTop;
            if (TryRay(x, z, ceiling, out hitTop) && hitTop > top)
            {
                top = hitTop;
                any = true;
            }

            Renderer[] renderers = Renderers();
            if (renderers != null)
            {
                for (int i = 0; i < renderers.Length; i++)
                {
                    Renderer renderer = renderers[i];
                    if (!FloorRenderer(renderer))
                    {
                        continue;
                    }

                    Bounds box = renderer.bounds;
                    if (x < box.min.x || x > box.max.x || z < box.min.z || z > box.max.z)
                    {
                        continue;
                    }

                    if (box.extents.x < 0.4f && box.extents.z < 0.4f)
                    {
                        continue;
                    }

                    // A building AABB that happens to cover this XZ is not the
                    // floor. Plates and the grid are thin; walls are not.
                    if (box.extents.y > 1f)
                    {
                        continue;
                    }

                    if (box.max.y >= ceiling || box.max.y < -2f)
                    {
                        continue;
                    }

                    if (!any || box.max.y > top)
                    {
                        top = box.max.y;
                        any = true;
                    }
                }
            }

            return any;
        }

        private static bool TryRay(float x, float z, float ceiling, out float top)
        {
            top = -4f;
            Vector3 origin = new Vector3(x, ceiling + 6f, z);
            RaycastHit[] hits = Physics.RaycastAll(
                origin,
                Vector3.down,
                16f,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Collide);
            if (hits == null)
            {
                return false;
            }

            bool any = false;
            for (int i = 0; i < hits.Length; i++)
            {
                Collider collider = hits[i].collider;
                if (collider == null || SkipCollider(collider))
                {
                    continue;
                }

                float y = hits[i].point.y;
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

        private static bool SkipCollider(Collider collider)
        {
            string name = collider.gameObject.name;
            if (name == "ContactBlob" || name == "Arena_SpawnContrast" || name == "ShieldBubble")
            {
                return true;
            }

            if (collider.GetComponentInParent<ShipController>() != null)
            {
                return true;
            }

            if (collider.GetComponentInParent<EnemySeeker>() != null)
            {
                return true;
            }

            if (collider.GetComponentInParent<Asteroid>() != null)
            {
                return true;
            }

            if (collider.GetComponentInParent<Pickup>() != null)
            {
                return true;
            }

            return collider.GetComponentInParent<Projectile>() != null;
        }

        private static Renderer[] Renderers()
        {
            if (_frame != Time.frameCount || _renderers == null)
            {
                _frame = Time.frameCount;
                _renderers = UnityEngine.Object.FindObjectsByType<Renderer>();
            }

            return _renderers;
        }

        private static bool FloorRenderer(Renderer renderer)
        {
            if (renderer == null || !renderer.enabled || renderer.gameObject == null || !renderer.gameObject.activeInHierarchy)
            {
                return false;
            }

            if (renderer is ParticleSystemRenderer || renderer is LineRenderer || renderer is TrailRenderer)
            {
                return false;
            }

            if (renderer.GetComponentInParent<Canvas>() != null)
            {
                return false;
            }

            string name = renderer.gameObject.name;
            if (name == "ContactBlob" || name == "Arena_SpawnContrast" || name == "ShieldBubble" || name == "ArenaLip")
            {
                return false;
            }

            if (name == "VoidField" || name == "OuterNebula" || name == "NearDust" || name == "BeltRock")
            {
                return false;
            }

            if (name == "StarFar" || name == "StarMid" || name == "StarNear" || name == "NebulaDome" || name == "NebulaInner")
            {
                return false;
            }

            if (renderer.GetComponentInParent<ShipController>() != null)
            {
                return false;
            }

            if (renderer.GetComponentInParent<EnemySeeker>() != null)
            {
                return false;
            }

            if (renderer.GetComponentInParent<Asteroid>() != null)
            {
                return false;
            }

            if (renderer.GetComponentInParent<Pickup>() != null)
            {
                return false;
            }

            return renderer.GetComponentInParent<Projectile>() == null;
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
            quad.layer = 0;
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

            // Layer 0, queue 3000. The play camera clears depth and does not
            // redraw the decor floor, so the quad darkens whatever the decor
            // pass already wrote. Y still sits just above that floor so a
            // play-camera plate cannot depth-reject the blob.
            gameObject.layer = 0;
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
            _shared.renderQueue = 3000;
            return _shared;
        }
    }
}
