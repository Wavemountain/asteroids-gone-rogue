using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Enemy shot rim. Pulses at 6 Hz in Full. Reduce holds a static rim.
    /// The shared material is not rewritten.
    /// </summary>
    public sealed class ShotRim : MonoBehaviour
    {
        private Renderer _renderer;
        private MaterialPropertyBlock _block;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            _block = new MaterialPropertyBlock();
        }

        private void Update()
        {
            if (_renderer == null)
            {
                return;
            }

            bool reduce = SettingsState.ReduceEffectsEnabled;
            float wave = 1f;
            if (LookPolish.EnemyRimPulses(reduce))
            {
                wave = 1f + (0.20f * Mathf.Sin(Time.time * LookPolish.EnemyPulseHz * 6.2831853f));
            }

            Color rim = new Color(LookPolish.EnemyRimR, LookPolish.EnemyRimG, LookPolish.EnemyRimB, 1f);
            _renderer.GetPropertyBlock(_block);
            _block.SetColor("_Color", rim);
            _block.SetColor("_EmissionColor", rim * (LookPolish.EnemyRimEmission * wave));
            _renderer.SetPropertyBlock(_block);
        }
    }
}
