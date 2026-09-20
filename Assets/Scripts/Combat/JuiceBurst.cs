using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Short expanding bloom for hit / kill / extra-life. Primitive fallback,
    /// no Inspector wiring. Look-bible: retro-modern amber / steel / heart-red.
    /// </summary>
    public sealed class JuiceBurst : MonoBehaviour
    {
        public const float HitSeconds = 0.12f;
        public const float KillSeconds = 0.32f;
        public const float HeartSeconds = 0.42f;

        private float _until;
        private float _life = HitSeconds;
        private float _startScale = 0.18f;
        private float _endScale = 1.1f;
        private Transform _mesh;
        private Material _mat;
        private Color _color = Color.white;

        public static void HitSpark(Vector3 position)
        {
            Spawn(position, new Color(1f, 0.93f, 0.78f), HitSeconds, 0.12f, 0.58f);
        }

        public static void KillBloom(Vector3 position, bool heavy)
        {
            Color color = heavy
                ? new Color(1f, 0.62f, 0.22f)
                : new Color(0.95f, 0.88f, 0.72f);
            float end = heavy ? 2.45f : 1.55f;
            float start = heavy ? 0.38f : 0.22f;
            Spawn(position, color, heavy ? 0.4f : KillSeconds, start, end);
        }

        public static void HeartBloom(Vector3 position)
        {
            Spawn(position, new Color(1f, 0.22f, 0.38f), HeartSeconds, 0.42f, 2.25f);
        }

        private static void Spawn(Vector3 position, Color color, float seconds, float startScale, float endScale)
        {
            GameObject go = new GameObject("JuiceBurst");
            go.transform.position = position;
            JuiceBurst burst = go.AddComponent<JuiceBurst>();
            burst.Play(color, seconds, startScale, endScale);
        }

        private void Play(Color color, float seconds, float startScale, float endScale)
        {
            _color = color;
            _life = Mathf.Max(0.08f, seconds);
            _until = Time.time + _life;
            _startScale = startScale;
            _endScale = endScale;
            EnsureMesh();
        }

        private void Update()
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
            float scale = Mathf.Lerp(_startScale, _endScale, t);
            _mesh.localScale = new Vector3(scale, scale, scale);
            if (_mat != null)
            {
                float alpha = 0.7f * (1f - t);
                Color tint = new Color(_color.r, _color.g, _color.b, alpha);
                _mat.color = tint;
                _mat.SetColor("_EmissionColor", _color * (1.35f + alpha));
            }
        }

        private void EnsureMesh()
        {
            if (_mesh != null)
            {
                return;
            }

            GameObject ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ball.name = "BurstMesh";
            ball.transform.SetParent(transform, false);
            ball.transform.localPosition = Vector3.zero;
            Collider collider = ball.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            _mesh = ball.transform;
            Renderer renderer = ball.GetComponent<Renderer>();
            Shader shader = Shader.Find("Standard");
            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            if (shader != null)
            {
                _mat = new Material(shader);
            }
            else if (renderer != null && renderer.sharedMaterial != null)
            {
                _mat = new Material(renderer.sharedMaterial);
            }

            if (_mat == null)
            {
                return;
            }

            _mat.name = "Mat_JuiceBurst";
            _mat.SetFloat("_Mode", 3f);
            _mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _mat.SetInt("_ZWrite", 0);
            _mat.EnableKeyword("_ALPHABLEND_ON");
            _mat.EnableKeyword("_EMISSION");
            _mat.renderQueue = 3000;
            if (renderer != null)
            {
                renderer.sharedMaterial = _mat;
            }
        }
    }
}
