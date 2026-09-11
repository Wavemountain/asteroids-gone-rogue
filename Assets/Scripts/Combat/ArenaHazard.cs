using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Static arena prop. Damaging spikes hit the ship; blockers only reshape the floor.
    /// </summary>
    public sealed class ArenaHazard : MonoBehaviour
    {
        public const int DamagingSpikeDamage = 2;
        public int Damage = 1;
        public bool Damaging;
        public bool PulseVisual;

        private MeshRenderer[] _renderers;
        private Light _glow;
        private MaterialPropertyBlock _block;
        private Color _baseEmit = new Color(0.722f, 0.353f, 0.157f);
        private float _phase;

        public void DressPulse(Color emit, Light glow)
        {
            PulseVisual = true;
            _baseEmit = emit;
            _glow = glow;
            _renderers = GetComponentsInChildren<MeshRenderer>(true);
            _block = new MaterialPropertyBlock();
        }

        private void Update()
        {
            if (!PulseVisual)
            {
                return;
            }

            if (_renderers == null)
            {
                _renderers = GetComponentsInChildren<MeshRenderer>(true);
                _block = new MaterialPropertyBlock();
            }

            _phase += Time.deltaTime * (Damaging ? 5.4f : 2.1f);
            float pulse = 0.72f + Mathf.Sin(_phase) * (Damaging ? 0.38f : 0.16f);
            Color emit = _baseEmit * pulse;
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

            if (_glow != null)
            {
                _glow.intensity = Damaging ? 1.55f * pulse : 0.72f * pulse;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            TryHit(collision != null ? collision.collider : null);
        }

        private void OnTriggerEnter(Collider other)
        {
            TryHit(other);
        }

        private void TryHit(Collider other)
        {
            if (!Damaging || other == null)
            {
                return;
            }

            ShipHealth health = other.GetComponentInParent<ShipHealth>();
            if (health != null)
            {
                if (AudioCues.Instance != null)
                {
                    AudioCues.Instance.PlayHazardHit();
                }

                health.ApplyDamage(Damage, DamageCause.HazardContact);
            }
        }
    }
}
