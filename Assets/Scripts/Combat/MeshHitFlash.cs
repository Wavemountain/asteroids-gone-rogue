using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Brief renderer tint via MaterialPropertyBlock. Does not spawn named VFX objects.
    /// </summary>
    public sealed class MeshHitFlash : MonoBehaviour
    {
        public const float Duration = 0.09f;

        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");
        private static readonly Color FlashColor = new Color(1f, 0.92f, 0.82f, 1f);
        private static readonly Color FlashEmission = new Color(1.4f, 1.1f, 0.7f, 1f);

        private float _until;
        private Renderer[] _renderers;

        public static void Play(Transform target)
        {
            if (target == null)
            {
                return;
            }

            MeshHitFlash flash = target.GetComponent<MeshHitFlash>();
            if (flash == null)
            {
                flash = target.gameObject.AddComponent<MeshHitFlash>();
            }

            flash.Begin();
        }

        public void Begin()
        {
            _renderers = GetComponentsInChildren<Renderer>(false);
            _until = Time.time + Duration;
            Apply(true);
        }

        private void LateUpdate()
        {
            if (_until <= 0f)
            {
                return;
            }

            if (Time.time >= _until)
            {
                Apply(false);
                _until = 0f;
            }
        }

        private void OnDisable()
        {
            Apply(false);
            _until = 0f;
        }

        private void Apply(bool flashing)
        {
            if (_renderers == null)
            {
                return;
            }

            MaterialPropertyBlock block = flashing ? new MaterialPropertyBlock() : null;
            if (flashing)
            {
                block.SetColor(ColorId, FlashColor);
                block.SetColor(EmissionId, FlashEmission);
            }

            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null)
                {
                    _renderers[i].SetPropertyBlock(block);
                }
            }
        }
    }
}
