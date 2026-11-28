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
        public const float PopSeconds = 0.09f;
        public const float PopPeak = 1.06f;

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
        private Vector3 _baseScale;
        private bool _scaleCaptured;
        private bool _popping;
        private float _popStart;

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

            if (FeelScale.FlashScale(SettingsState.FlashLevelValue) <= 0f)
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

        public static void Pop(Transform target)
        {
            if (target == null || SettingsState.ReduceEffectsEnabled)
            {
                return;
            }

            MeshHitFlash flash = target.GetComponent<MeshHitFlash>();
            if (flash == null)
            {
                flash = target.gameObject.AddComponent<MeshHitFlash>();
            }

            flash.BeginPop();
        }

        public void Begin(Color tint, float seconds)
        {
            _renderers = GetComponentsInChildren<Renderer>(false);
            bool reduce = SettingsState.ReduceEffectsEnabled;
            float flashScale = FeelScale.FlashScale(SettingsState.FlashLevelValue);
            if (flashScale <= 0f)
            {
                return;
            }

            seconds = EffectScale.MeshSeconds(reduce, seconds);
            bool fresh = _until <= Time.time;
            _until = Time.time + seconds;
            _tint = tint;
            _tint.a *= flashScale;
            _emission = tint * EffectScale.MeshEmission(reduce) * flashScale;
            if (fresh)
            {
                Capture();
            }

            Apply(true);
        }

        private void LateUpdate()
        {
            if (_until > 0f && Time.time >= _until)
            {
                Apply(false);
                _until = 0f;
            }

            TickPop();
        }

        public void BeginPop()
        {
            if (!_scaleCaptured)
            {
                _baseScale = transform.localScale;
                _scaleCaptured = true;
            }

            _popping = true;
            _popStart = Time.unscaledTime;
        }

        private void TickPop()
        {
            if (!_popping)
            {
                return;
            }

            float t = (Time.unscaledTime - _popStart) / PopSeconds;
            if (t >= 1f)
            {
                transform.localScale = _baseScale;
                _popping = false;
                return;
            }

            float shaped = t < 0.5f ? t / 0.5f : (1f - t) / 0.5f;
            float mul = 1f + ((PopPeak - 1f) * shaped);
            transform.localScale = _baseScale * mul;
        }

        private void OnDisable()
        {
            Apply(false);
            _until = 0f;
            if (_popping && _scaleCaptured)
            {
                transform.localScale = _baseScale;
            }

            _popping = false;
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
