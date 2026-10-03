using UnityEngine;

namespace AsteroidsGoneRogue
{
    public enum TelegraphShape
    {
        Ring = 0,
        Wedge = 1,
        Spokes = 2,
        DoubleRing = 3
    }

    /// <summary>
    /// Short-lived floor ring. Emission fades with alpha so the ring does not pop.
    /// Shape cues (wedge, spokes, double ring) stay readable without colour.
    /// </summary>
    public sealed class TelegraphRing : MonoBehaviour
    {
        Color _color = Color.cyan;
        float _until;
        float _life = 0.55f;
        Transform _mesh;
        Transform _extra;
        Material _mat;
        TelegraphShape _shape = TelegraphShape.Ring;

        public void Play(Color color, float seconds)
        {
            Play(color, seconds, TelegraphShape.Ring, Vector3.forward);
        }

        public void Play(Color color, float seconds, TelegraphShape shape, Vector3 forward)
        {
            _color = color;
            _shape = shape;
            _life = Mathf.Max(0.12f, seconds);
            _until = Time.time + _life;
            if (forward.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
            }

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
            if (_extra != null)
            {
                if (_shape == TelegraphShape.DoubleRing)
                {
                    _extra.localScale = new Vector3(scale * 0.62f, 0.035f, scale * 0.62f);
                }
                else if (_shape == TelegraphShape.Wedge)
                {
                    _extra.localScale = new Vector3(scale * 0.22f, 0.05f, scale * 0.48f);
                    _extra.localPosition = new Vector3(0f, 0.08f, scale * 0.22f);
                }
                else if (_shape == TelegraphShape.Spokes)
                {
                    _extra.localScale = new Vector3(scale, 0.04f, scale);
                }
            }

            if (_mat != null)
            {
                float alpha = 0.55f * (1f - t);
                _mat.color = new Color(_color.r, _color.g, _color.b, alpha);
                _mat.SetColor("_EmissionColor", new Color(_color.r * alpha, _color.g * alpha, _color.b * alpha, 1f));
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
            if (shader != null)
            {
                _mat = new Material(shader);
            }
            else
            {
                _mat = new Material(renderer.sharedMaterial);
            }

            _mat.name = "Mat_TelegraphRing";
            _mat.SetFloat("_Mode", 3f);
            _mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _mat.SetInt("_ZWrite", 0);
            _mat.EnableKeyword("_ALPHABLEND_ON");
            _mat.EnableKeyword("_EMISSION");
            _mat.renderQueue = 3000;
            renderer.sharedMaterial = _mat;
            BuildShape();
        }

        void BuildShape()
        {
            if (_shape == TelegraphShape.DoubleRing)
            {
                _extra = MakePrimitive(PrimitiveType.Cylinder, "DoubleRing", Vector3.zero, new Vector3(0.62f, 0.8f, 0.62f));
            }
            else if (_shape == TelegraphShape.Wedge)
            {
                _extra = MakePrimitive(PrimitiveType.Cube, "AimWedge", new Vector3(0f, 0.08f, 0.35f), new Vector3(0.22f, 1.2f, 0.48f));
            }
            else if (_shape == TelegraphShape.Spokes)
            {
                GameObject hub = new GameObject("Spokes");
                hub.transform.SetParent(transform, false);
                _extra = hub.transform;
                for (int spoke = 0; spoke < 8; spoke++)
                {
                    float angle = spoke * 45f;
                    Quaternion yaw = Quaternion.Euler(0f, angle, 0f);
                    GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    bar.name = "Spoke";
                    bar.transform.SetParent(_extra, false);
                    bar.transform.localRotation = yaw;
                    bar.transform.localPosition = yaw * new Vector3(0f, 0.06f, 0.72f);
                    bar.transform.localScale = new Vector3(0.06f, 0.5f, 0.42f);
                    Collider spokeCollider = bar.GetComponent<Collider>();
                    if (spokeCollider != null)
                    {
                        Destroy(spokeCollider);
                    }

                    Renderer spokeRenderer = bar.GetComponent<Renderer>();
                    if (spokeRenderer != null)
                    {
                        spokeRenderer.sharedMaterial = _mat;
                    }
                }
            }
        }

        Transform MakePrimitive(PrimitiveType kind, string primitiveName, Vector3 localPos, Vector3 localScale)
        {
            GameObject piece = GameObject.CreatePrimitive(kind);
            piece.name = primitiveName;
            piece.transform.SetParent(transform, false);
            piece.transform.localPosition = localPos;
            piece.transform.localScale = localScale;
            Collider pieceCollider = piece.GetComponent<Collider>();
            if (pieceCollider != null)
            {
                Destroy(pieceCollider);
            }

            Renderer pieceRenderer = piece.GetComponent<Renderer>();
            if (pieceRenderer != null)
            {
                pieceRenderer.sharedMaterial = _mat;
            }

            return piece.transform;
        }
    }

    /// <summary>
    /// Thin Danger line for the sniper and gunner wind-up. Softer than the boss ring.
    /// </summary>
    public sealed class TelegraphLaser : MonoBehaviour
    {
        Transform _mesh;
        Material _mat;
        float _until;

        public void Show(Vector3 from, Vector3 to, Color color, float alpha, float seconds)
        {
            _until = Time.time + Mathf.Max(0.05f, seconds);
            EnsureMesh();
            Vector3 delta = to - from;
            delta.y = 0f;
            if (delta.sqrMagnitude < 0.04f)
            {
                delta = Vector3.forward;
            }

            float length = delta.magnitude;
            transform.position = from + delta.normalized * (length * 0.5f);
            transform.rotation = Quaternion.LookRotation(delta.normalized, Vector3.up);
            if (_mesh != null)
            {
                _mesh.localScale = new Vector3(0.045f, 0.02f, length);
            }

            if (_mat != null)
            {
                float shown = alpha;
                if (shown < 0f)
                {
                    shown = 0f;
                }

                if (shown > 0.35f)
                {
                    shown = 0.35f;
                }

                _mat.color = new Color(color.r, color.g, color.b, shown);
                _mat.SetColor("_EmissionColor", new Color(color.r * shown, color.g * shown, color.b * shown, 1f));
            }
        }

        void Update()
        {
            if (Time.time >= _until)
            {
                Destroy(gameObject);
            }
        }

        void EnsureMesh()
        {
            if (_mesh != null)
            {
                return;
            }

            GameObject bar = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bar.name = "LaserMesh";
            bar.transform.SetParent(transform, false);
            Collider collider = bar.GetComponent<Collider>();
            if (collider != null)
            {
                Destroy(collider);
            }

            _mesh = bar.transform;
            Renderer renderer = bar.GetComponent<Renderer>();
            Shader shader = Shader.Find("Standard");
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

            _mat.name = "Mat_TelegraphLaser";
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
