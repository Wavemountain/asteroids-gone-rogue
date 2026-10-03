using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Brief renderer tint via MaterialPropertyBlock. The block that was on the
    /// renderer before the flash is restored, so per-enemy emission survives.
    /// </summary>
    public sealed class MeshHitFlash : MonoBehaviour
    {
        public const float Duration = 0.09f;

        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");
        private static readonly Color FlashColor = new Color(1f, 0.96f, 0.92f, 1f);
        private static readonly Color FlashEmission = new Color(1.15f, 1.1f, 1.02f, 1f);

        private float _until;
        private Renderer[] _renderers;
        private MaterialPropertyBlock[] _saved;
        private Color _tint = FlashColor;
        private Color _emission = FlashEmission;
        private MaterialPropertyBlock _flashBlock;

        public static void Play(Transform target)
        {
            Play(target, FlashColor, Duration);
        }

        public static void Play(Transform target, Color tint, float seconds)
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

            flash.Begin(tint, seconds);
        }

        public void Begin(Color tint, float seconds)
        {
            _renderers = GetComponentsInChildren<Renderer>(false);
            bool reduce = SettingsState.ReduceEffectsEnabled;
            seconds = EffectScale.MeshSeconds(reduce, seconds);
            bool fresh = _until <= Time.time;
            _until = Time.time + seconds;
            _tint = tint;
            _emission = tint * EffectScale.MeshEmission(reduce);
            if (fresh)
            {
                Capture();
            }

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

        private void Capture()
        {
            if (_renderers == null)
            {
                return;
            }

            _saved = new MaterialPropertyBlock[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
            {
                MaterialPropertyBlock saved = new MaterialPropertyBlock();
                if (_renderers[i] != null)
                {
                    _renderers[i].GetPropertyBlock(saved);
                }

                _saved[i] = saved;
            }
        }

        private void Apply(bool flashing)
        {
            if (_renderers == null)
            {
                return;
            }

            if (flashing && _flashBlock == null)
            {
                _flashBlock = new MaterialPropertyBlock();
            }

            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] == null)
                {
                    continue;
                }

                if (flashing)
                {
                    _flashBlock.Clear();
                    _flashBlock.SetColor(ColorId, _tint);
                    _flashBlock.SetColor(EmissionId, _emission);
                    _renderers[i].SetPropertyBlock(_flashBlock);
                }
                else if (_saved != null && i < _saved.Length && _saved[i] != null)
                {
                    _renderers[i].SetPropertyBlock(_saved[i]);
                }
            }
        }
    }
}
