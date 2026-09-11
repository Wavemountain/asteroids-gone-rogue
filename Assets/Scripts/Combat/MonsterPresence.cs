using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Studio-grade monster read: idle emission pulse, rim light, charge glow.
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
        MaterialPropertyBlock _block;
        bool _charging;
        float _phase;

        public void Configure(Color aura, float range)
        {
            AuraColor = aura;
            AuraRange = range;
            EnsureLight();
            ApplyLight();
        }

        public void SetCharging(bool charging)
        {
            _charging = charging;
            ApplyLight();
        }

        void Awake()
        {
            _renderers = GetComponentsInChildren<MeshRenderer>(true);
            _block = new MaterialPropertyBlock();
            EnsureLight();
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
    }
}
