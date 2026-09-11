using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Short-lived floor ring at a world point (Swarmling drop telegraph).
    /// </summary>
    public sealed class TelegraphRing : MonoBehaviour
    {
        Color _color = Color.cyan;
        float _until;
        float _life = 0.55f;
        Transform _mesh;
        Material _mat;

        public void Play(Color color, float seconds)
        {
            _color = color;
            _life = Mathf.Max(0.12f, seconds);
            _until = Time.time + _life;
            EnsureMesh();
        }

        void Update()
        {
            if (_mesh == null)
            {
                return;
            }

            float remain = _until - Time.time;
            if (remain <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            float t = 1f - Mathf.Clamp01(remain / _life);
            float scale = Mathf.Lerp(0.7f, 3.4f, t);
            _mesh.localScale = new Vector3(scale, 0.04f, scale);
            if (_mat != null)
            {
                float alpha = 0.55f * (1f - t);
                _mat.color = new Color(_color.r, _color.g, _color.b, alpha);
                _mat.SetColor("_EmissionColor", _color * (1.2f + alpha));
            }
        }

        void EnsureMesh()
        {
            if (_mesh != null)
            {
                return;
            }

            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "RingMesh";
            ring.transform.SetParent(transform, false);
            ring.transform.localPosition = Vector3.zero;
            Collider collider = ring.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            _mesh = ring.transform;
            Renderer renderer = ring.GetComponent<Renderer>();
            Shader shader = Shader.Find("Standard");
            _mat = new Material(shader != null ? shader : renderer.sharedMaterial);
            _mat.name = "Mat_TelegraphRing";
            _mat.SetFloat("_Mode", 3f);
            _mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _mat.SetInt("_ZWrite", 0);
            _mat.EnableKeyword("_ALPHABLEND_ON");
            _mat.EnableKeyword("_EMISSION");
            _mat.renderQueue = 3000;
            renderer.sharedMaterial = _mat;
        }
    }
}
