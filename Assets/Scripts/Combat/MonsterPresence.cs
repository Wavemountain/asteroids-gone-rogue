using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Studio-grade monster telegraph: idle pulse, charge ring, spawn burst.
    /// Visual polish only — combat numbers stay on <see cref="EnemySeeker"/>.
    /// </summary>
    public sealed class MonsterPresence : MonoBehaviour
    {
        public Color AuraColor = new Color(0.95f, 0.42f, 0.78f, 1f);
        public float AuraRange = 7.2f;
        public float IdlePulse = 0.22f;
        public float ChargePulse = 0.55f;

        MeshRenderer[] _renderers;
        Light _aura;
        Transform _ring;
        Renderer _ringRenderer;
        Material _ringMat;
        MaterialPropertyBlock _block;
        bool _charging;
        float _phase;
        float _burstUntil;
        float _nestUntil;

        public void Configure(Color aura, float range)
        {
            AuraColor = aura;
            AuraRange = range;
            EnsureLight();
            EnsureRing();
            ApplyLight();
            PlaySpawnRing();
        }

        public void SetCharging(bool charging)
        {
            _charging = charging;
            ApplyLight();
        }

        public void PlaySpawnRing()
        {
            _burstUntil = Time.time + 0.62f;
            EnsureRing();
        }

        public void PulseNest()
        {
            _nestUntil = Time.time + 0.72f;
            EnsureRing();
        }

        void Awake()
        {
            _renderers = GetComponentsInChildren<MeshRenderer>(true);
            _block = new MaterialPropertyBlock();
            EnsureLight();
            EnsureRing();
        }

        void Update()
        {
            _phase += Time.deltaTime * (_charging ? 7.4f : 2.6f);
            float pulse = 0.78f + Mathf.Sin(_phase) * (_charging ? ChargePulse : IdlePulse);
            Color emit = AuraColor * pulse;

            if (_renderers != null)
            {
                for (int i = 0; i < _renderers.Length; i++)
                {
                    MeshRenderer renderer = _renderers[i];
                    if (renderer == null)
                    {
                        continue;
                    }

                    renderer.GetPropertyBlock(_block);
                    _block.SetColor("_EmissionColor", emit);
                    renderer.SetPropertyBlock(_block);
                }
            }

            ApplyLight();
            ApplyRing();
        }

        void EnsureLight()
        {
            if (_aura != null)
            {
                return;
            }

            Transform existing = transform.Find("Aura");
            GameObject lightGo = existing != null ? existing.gameObject : new GameObject("Aura");
            lightGo.transform.SetParent(transform, false);
            lightGo.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            _aura = lightGo.GetComponent<Light>();
            if (_aura == null)
            {
                _aura = lightGo.AddComponent<Light>();
            }

            _aura.type = LightType.Point;
            _aura.shadows = LightShadows.None;
            ApplyLight();
        }

        void EnsureRing()
        {
            if (_ring != null)
            {
                return;
            }

            Transform existing = transform.Find("TelegraphRing");
            GameObject ringGo;
            if (existing != null)
            {
                ringGo = existing.gameObject;
            }
            else
            {
                ringGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                ringGo.name = "TelegraphRing";
                ringGo.transform.SetParent(transform, false);
                Collider collider = ringGo.GetComponent<Collider>();
                if (collider != null)
                {
                    Destroy(collider);
                }
            }

            ringGo.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            _ring = ringGo.transform;
            _ringRenderer = ringGo.GetComponent<Renderer>();
            if (_ringMat == null)
            {
                Shader shader = Shader.Find("Standard");
                if (shader != null)
                {
                    _ringMat = new Material(shader);
                }
                else
                {
                    _ringMat = new Material(_ringRenderer.sharedMaterial);
                }

                _ringMat.name = "Mat_Monster_Telegraph";
                _ringMat.SetFloat("_Mode", 3f);
                _ringMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                _ringMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                _ringMat.SetInt("_ZWrite", 0);
                _ringMat.EnableKeyword("_ALPHABLEND_ON");
                _ringMat.EnableKeyword("_EMISSION");
                _ringMat.renderQueue = 3000;
            }

            if (_ringRenderer != null)
            {
                _ringRenderer.sharedMaterial = _ringMat;
            }

            ringGo.SetActive(false);
        }

        void ApplyLight()
        {
            if (_aura == null)
            {
                return;
            }

            _aura.color = AuraColor;
            _aura.range = _charging ? AuraRange * 1.28f : AuraRange;
            _aura.intensity = _charging ? 2.15f : 1.05f;
        }

        void ApplyRing()
        {
            if (_ring == null)
            {
                return;
            }

            float now = Time.time;
            bool burst = now < _burstUntil;
            bool nest = now < _nestUntil;
            bool show = _charging || burst || nest;
            _ring.gameObject.SetActive(show);
            if (!show)
            {
                return;
            }

            float scale;
            float alpha;
            if (_charging)
            {
                scale = 2.35f + Mathf.Sin(_phase) * 0.55f;
                alpha = 0.55f;
            }
            else if (nest)
            {
                float t = 1f - Mathf.Clamp01((_nestUntil - now) / 0.72f);
                scale = Mathf.Lerp(1.1f, 2.6f, t);
                alpha = 0.48f;
            }
            else
            {
                float t = 1f - Mathf.Clamp01((_burstUntil - now) / 0.62f);
                scale = Mathf.Lerp(0.55f, 3.1f, t);
                alpha = 0.5f * (1f - t);
            }

            _ring.localScale = new Vector3(scale, 0.035f, scale);
            if (_ringMat != null)
            {
                Color col = new Color(AuraColor.r, AuraColor.g, AuraColor.b, alpha);
                _ringMat.color = col;
                _ringMat.SetColor("_EmissionColor", AuraColor * (1.4f + alpha));
            }
        }
    }
}
