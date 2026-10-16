using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Flat dark disc under a body. The shader keeps RGB black and fades alpha
    /// to the edge. Colour and alpha stay put when Reduce is on.
    /// </summary>
    public sealed class ContactBlob : MonoBehaviour
    {
        public const float Alpha = 0.72f;
        public const float FloorY = 0.05f;
        public const float RadiusPad = 1.25f;
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

            transform.SetPositionAndRotation(
                new Vector3(host.position.x, FloorY, host.position.z),
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
                return sphere.radius * scale;
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

                return radial * scale;
            }

            return radius * scale;
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
