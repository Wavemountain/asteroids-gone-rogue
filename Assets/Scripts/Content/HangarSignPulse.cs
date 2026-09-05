using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Slow amber pulse on the LaunchSign light + emissive so GO reads from the hangar camera.
    /// </summary>
    public sealed class HangarSignPulse : MonoBehaviour
    {
        public const float PulseHz = 1.15f;
        public const float PulseScale = 0.42f;

        private static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

        public Light SignLight;
        public float BaseIntensity = 2.6f;
        public Color BaseEmission = new Color(1f, 0.62f, 0.12f) * 3.4f;

        private Renderer[] _renderers;
        private MaterialPropertyBlock _block;

        private void Awake()
        {
            _renderers = GetComponentsInChildren<Renderer>(true);
            _block = new MaterialPropertyBlock();
        }

        private void LateUpdate()
        {
            float wave = 0.55f + 0.45f * Mathf.Sin(Time.unscaledTime * (Mathf.PI * 2f * PulseHz));
            if (SignLight != null)
            {
                SignLight.intensity = BaseIntensity * (1f + PulseScale * wave);
            }

            if (_renderers == null)
            {
                return;
            }

            Color emission = BaseEmission * (0.72f + 0.55f * wave);
            _block.SetColor(EmissionId, emission);
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null && _renderers[i].GetComponent<TextMesh>() == null)
                {
                    _renderers[i].SetPropertyBlock(_block);
                }
            }
        }
    }
}
