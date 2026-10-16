using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Flat dark disc under a body. The shader keeps RGB black and fades alpha
    /// to the edge. Colour and alpha stay put when Reduce is on.
    /// </summary>
    public sealed class ContactBlob : MonoBehaviour
    {
        public const float Alpha = 0.8f;
        public const float FloorY = 0.08f;
        public const float RadiusPad = 1.5f;
        public const float MinRadius = 0.45f;

        private Transform _host;
        private static Material _shared;

        public static void Attach(Transform host)
        {
            if (host == null || host.Find("ContactBlob") != null)
            {
                return;
            }

            float radius = Footprint(host) * RadiusPad;
            if (radius < MinRadius)
            {
                radius = MinRadius;
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

            float axis = host.lossyScale.x;
            if (host.lossyScale.y > axis)
            {
                axis = host.lossyScale.y;
            }

            if (host.lossyScale.z > axis)
            {
                axis = host.lossyScale.z;
            }

            if (axis < 0.001f)
            {
                axis = 1f;
            }

            float local = (radius * 2f) / axis;
            quad.transform.localScale = new Vector3(local, local, 1f);
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
            // redraw the decor floor, so the quad darkens W3/W4 as well as W1.
            // A decor-layer disc would lose the depth test against those floors.
            gameObject.layer = 0;
            float y = FloorAbove(host);
            float ceiling = host.position.y - 0.12f;
            if (y > ceiling)
            {
                y = ceiling;
            }

            if (y < 0.02f)
            {
                y = 0.02f;
            }

            transform.SetPositionAndRotation(
                new Vector3(host.position.x, y, host.position.z),
                Quaternion.Euler(90f, 0f, 0f));
        }

        private static int _floorFrame = -1;
        private static Renderer[] _floors;

        private static float FloorAbove(Transform host)
        {
            float y = FloorY;
            if (host == null)
            {
                return y;
            }

            if (_floorFrame != Time.frameCount)
            {
                _floorFrame = Time.frameCount;
                _floors = UnityEngine.Object.FindObjectsByType<Renderer>();
            }

            if (_floors == null)
            {
                return y;
            }

            int decor = DecorCameraStack.LayerIndex();
            float x = host.position.x;
            float z = host.position.z;
            float shipY = host.position.y;
            for (int i = 0; i < _floors.Length; i++)
            {
                Renderer floor = _floors[i];
                if (floor == null || !floor.enabled || floor.gameObject == null || floor.gameObject.layer == decor)
                {
                    continue;
                }

                if (floor.transform == host || floor.transform.IsChildOf(host))
                {
                    continue;
                }

                Bounds box = floor.bounds;
                if (x < box.min.x || x > box.max.x || z < box.min.z || z > box.max.z)
                {
                    continue;
                }

                if (box.max.y >= shipY - 0.05f || box.max.y < -0.2f)
                {
                    continue;
                }

                float top = box.max.y + 0.04f;
                if (top > y)
                {
                    y = top;
                }
            }

            return y;
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
