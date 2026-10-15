using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Built-in OnRenderImage stack for the gameplay camera only.
    /// Decor and the letterbox camera stay ungraded. Grade and Neutral
    /// tonemap share the uber pass. enabled false, or a missing shader,
    /// blits the source unchanged so the picture never goes pink.
    /// </summary>
    public sealed class PostStack : MonoBehaviour
    {
        public const int PassPrefilter = 0;
        public const int PassDown = 1;
        public const int PassUp = 2;
        public const int PassUber = 3;

        public static float LastBloom;
        public static float LastGrain;
        public static float LastVignette;
        public static float LastVignetteSmooth;
        public static float LastSaturation;
        public static float LastDeathFlash;
        public static int LastReduce;

        private static PostStack _active;

        private float _waveBloomUntil;
        private float _waveBloomAdd;

        private Camera _camera;
        private Material _material;
        private PostBlendState _state;
        private int _passThrough;
        private RenderTexture[] _chain;
        private int _chainWidth;
        private int _chainHeight;
        private int _chainFormat;

        public static void Attach(Camera gameplay)
        {
            if (gameplay == null)
            {
                return;
            }

            gameplay.allowHDR = true;
            PostStack stack = gameplay.GetComponent<PostStack>();
            if (stack == null)
            {
                stack = gameplay.gameObject.AddComponent<PostStack>();
            }

            stack._camera = gameplay;
            stack.enabled = true;
            _active = stack;
        }

        public static void SetWorld(int world, bool reduce)
        {
            if (_active == null)
            {
                return;
            }

            _active.RequestWorld(world, reduce);
        }

        public static void SnapHangar(bool reduce)
        {
            if (_active == null)
            {
                return;
            }

            _active._state = PostGrade.SnapTo(PostGrade.Hangar);
            _active._passThrough = 0;
        }

        public static void SnapWorld(int world, bool reduce)
        {
            if (_active == null)
            {
                return;
            }

            _active._state = PostGrade.SnapTo(world);
        }

        public static void BeginDeathVignette()
        {
            if (_active == null)
            {
                return;
            }

            _active._state = PostGrade.BeginDeath(_active._state);
        }

        public static void BeginWaveBloom()
        {
            if (_active == null)
            {
                return;
            }

            float add = PremiumLook.WaveBloomAdd;
            if (SettingsState.ReduceEffectsEnabled)
            {
                add *= EffectScale.PostBloomMul;
            }

            _active._waveBloomAdd = add;
            _active._waveBloomUntil = Time.unscaledTime + PremiumLook.WaveBloomSeconds;
        }

        public static void PinWaveBloom()
        {
            BeginWaveBloom();
        }

        public static void NoteDeathFlash(float alpha)
        {
            LastDeathFlash = alpha;
        }

        /// <summary>
        /// Pins the death vignette clock at a known second so a still can sit
        /// inside the desaturation and vignette, not at the start of the ramp.
        /// </summary>
        public static void PinDeathClock(float seconds)
        {
            if (_active == null)
            {
                return;
            }

            PostBlendState state = _active._state;
            state.DeathElapsed = seconds;
            _active._state = state;
        }

        /// <summary>
        /// Samples the live grade, including Reduce, without waiting for a camera blit.
        /// OnRenderImage writes the same fields from the row it actually draws.
        /// </summary>
        public static void RememberGrade()
        {
            if (_active == null)
            {
                return;
            }

            bool reduce = SettingsState.ReduceEffectsEnabled;
            PostGradeRow shown;
            float vignette;
            _active.ApplyPresented(reduce, 0, out shown, out vignette);
            StoreGrade(shown, vignette, reduce);
        }

        private static void StoreGrade(PostGradeRow shown, float vignette, bool reduce)
        {
            LastBloom = shown.Bloom;
            LastGrain = shown.Grain;
            LastVignette = vignette;
            LastVignetteSmooth = shown.VignetteSmooth;
            LastSaturation = shown.Saturation;
            LastReduce = reduce ? 1 : 0;
        }

        private void ApplyPresented(bool reduce, int memoryMb, out PostGradeRow shown, out float vignette)
        {
            PostGrade.Sample(_state, reduce, memoryMb, out shown, out vignette);
            shown.Vignette = vignette;
            float deathU = 0f;
            if (_state.DeathElapsed > 0f && PostGrade.DeathVignetteSeconds > 0.0001f)
            {
                deathU = _state.DeathElapsed / PostGrade.DeathVignetteSeconds;
            }

            if (deathU > 1f)
            {
                deathU = 1f;
            }

            shown.Saturation = PremiumLook.DeathSaturation(shown.Saturation, deathU, reduce);
            shown.Vignette = PremiumLook.PresentedVignette(shown.Vignette, deathU, reduce);
            vignette = shown.Vignette;
            if (Time.unscaledTime < _waveBloomUntil)
            {
                shown.Bloom += _waveBloomAdd;
            }

            if (_state.FromWorld == PostGrade.Hangar && _state.ToWorld == PostGrade.Hangar)
            {
                shown.Bloom *= PremiumLook.HangarBloomGain;
                shown.Exposure += PremiumLook.HangarExposureEv;
            }
        }

        private void Awake()
        {
            _state = PostGrade.SnapTo(PostGrade.Hangar);
            _camera = GetComponent<Camera>();
            if (_camera != null)
            {
                _camera.allowHDR = true;
            }

            _active = this;
            BuildMaterial();
        }

        private void OnEnable()
        {
            _active = this;
        }

        private void OnDisable()
        {
            ReleaseChain();
        }

        private void OnDestroy()
        {
            ReleaseChain();
        }

        private void Update()
        {
            _state = PostGrade.AdvanceClock(_state, Time.unscaledDeltaTime);
        }

        private void LateUpdate()
        {
            if (_camera != null)
            {
                _camera.allowHDR = true;
            }
        }

        private void RequestWorld(int world, bool reduce)
        {
            int folded = PostGrade.Fold(world);
            if (folded == _state.ToWorld)
            {
                return;
            }

            _state = PostGrade.Retarget(_state, folded);
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (source == null || _passThrough != 0 || _material == null || !enabled)
            {
                Graphics.Blit(source, destination);
                return;
            }

            int memoryMb = 0;
            if (DeckThisFrame(source))
            {
                memoryMb = PostGrade.DeckMemoryMb;
            }

            bool reduce = SettingsState.ReduceEffectsEnabled;
            PostGradeRow shown;
            float vignette;
            ApplyPresented(reduce, memoryMb, out shown, out vignette);
            StoreGrade(shown, vignette, reduce);
            PushUniforms(shown, source);

            int iterations = shown.Iterations;
            if (iterations < 1)
            {
                iterations = 1;
            }

            if (iterations > 5)
            {
                iterations = 5;
            }

            int width = source.width;
            int height = source.height;
            if (width < 2)
            {
                width = 2;
            }

            if (height < 2)
            {
                height = 2;
            }

            RenderTexture[] chain = Chain(width, height, (int)RenderTextureFormat.DefaultHDR);
            int levels = 0;
            RenderTexture bloom = null;
            int bloomIsExtra = 0;
            try
            {
                int tw = width / 2;
                int th = height / 2;
                if (tw < 2)
                {
                    tw = 2;
                }

                if (th < 2)
                {
                    th = 2;
                }

                chain[0] = RenderTexture.GetTemporary(tw, th, 0, RenderTextureFormat.DefaultHDR);
                levels = 1;
                Graphics.Blit(source, chain[0], _material, PassPrefilter);

                int step = 1;
                while (step < iterations)
                {
                    tw = tw / 2;
                    th = th / 2;
                    if (tw < 2)
                    {
                        tw = 2;
                    }

                    if (th < 2)
                    {
                        th = 2;
                    }

                    chain[step] = RenderTexture.GetTemporary(tw, th, 0, RenderTextureFormat.DefaultHDR);
                    Graphics.Blit(chain[step - 1], chain[step], _material, PassDown);
                    levels = step + 1;
                    step++;
                }

                bloom = chain[levels - 1];
                int up = levels - 2;
                while (up >= 0)
                {
                    RenderTexture wider = RenderTexture.GetTemporary(chain[up].width, chain[up].height, 0, RenderTextureFormat.DefaultHDR);
                    _material.SetTexture("_BloomTex", chain[up]);
                    Graphics.Blit(bloom, wider, _material, PassUp);
                    if (bloomIsExtra != 0)
                    {
                        RenderTexture.ReleaseTemporary(bloom);
                    }

                    bloom = wider;
                    bloomIsExtra = 1;
                    up--;
                }

                _material.SetTexture("_BloomTex", bloom);
                if (destination == null)
                {
                    Graphics.Blit(source, (RenderTexture)null, _material, PassUber);
                }
                else
                {
                    Graphics.Blit(source, destination, _material, PassUber);
                }
            }
            finally
            {
                int release = 0;
                while (release < levels)
                {
                    if (chain[release] != null)
                    {
                        RenderTexture.ReleaseTemporary(chain[release]);
                        chain[release] = null;
                    }

                    release++;
                }

                if (bloomIsExtra != 0 && bloom != null)
                {
                    RenderTexture.ReleaseTemporary(bloom);
                }
            }
        }

        private void PushUniforms(PostGradeRow shown, RenderTexture source)
        {
            _material.SetFloat("_Threshold", shown.Threshold);
            _material.SetFloat("_SoftKnee", shown.SoftKnee);
            _material.SetFloat("_ClampMax", shown.ClampMax);
            _material.SetFloat("_Intensity", shown.Bloom);
            _material.SetFloat("_Scatter", shown.Scatter);
            _material.SetFloat("_Exposure", shown.Exposure);
            _material.SetFloat("_Saturation", shown.Saturation);
            _material.SetFloat("_Contrast", shown.Contrast);
            _material.SetFloat("_Vignette", shown.Vignette);
            _material.SetFloat("_VignetteSmooth", shown.VignetteSmooth);
            _material.SetFloat("_Grain", shown.Grain);
            _material.SetFloat("_GrainLum", 0.85f);
            _material.SetFloat("_GrainTime", Time.unscaledTime);
            float aspect = 1.6f;
            if (source != null && source.height > 0)
            {
                aspect = source.width / (float)source.height;
            }

            _material.SetFloat("_Aspect", aspect);
            float balanceR;
            float balanceG;
            float balanceB;
            PostGrade.WhiteBalance(shown.Temperature, shown.Tint, out balanceR, out balanceG, out balanceB);
            _material.SetColor("_Balance", new Color(balanceR, balanceG, balanceB, 1f));
            float liftR;
            float liftG;
            float liftB;
            float gainR;
            float gainG;
            float gainB;
            PostGrade.LiftGain(shown, out liftR, out liftG, out liftB, out gainR, out gainG, out gainB);
            _material.SetColor("_Lift", new Color(liftR, liftG, liftB, 1f));
            _material.SetColor("_Gain", new Color(gainR, gainG, gainB, 1f));
            bool linear = QualitySettings.activeColorSpace == ColorSpace.Linear;
            float vignetteR;
            float vignetteG;
            float vignetteB;
            PostGrade.VignetteRgb(linear, out vignetteR, out vignetteG, out vignetteB);
            _material.SetColor("_VignetteColor", new Color(vignetteR, vignetteG, vignetteB, 1f));
        }

        private void BuildMaterial()
        {
            Shader postShader = Shader.Find(ShaderWarmup.PostShaderName);
            if (postShader == null)
            {
                postShader = Resources.Load<Shader>(ShaderWarmup.PostResource);
            }

            if (postShader == null || !postShader.isSupported)
            {
                _material = null;
                return;
            }

            _material = new Material(postShader);
            _material.name = "Mat_PostStack";
            if (_material.shader == null || !_material.shader.isSupported)
            {
                Destroy(_material);
                _material = null;
            }
        }

        private RenderTexture[] Chain(int width, int height, int format)
        {
            int slots = 0;
            if (_chain != null)
            {
                slots = _chain.Length;
            }

            if (!PostGrade.TargetsNeedRebuild(_chainWidth, _chainHeight, _chainFormat, slots, width, height, format))
            {
                return _chain;
            }

            ReleaseChain();
            _chain = new RenderTexture[PostGrade.BloomChainSlots];
            _chainWidth = width;
            _chainHeight = height;
            _chainFormat = format;
            return _chain;
        }

        private void ReleaseChain()
        {
            if (_chain != null)
            {
                int index = 0;
                while (index < _chain.Length)
                {
                    if (_chain[index] != null)
                    {
                        RenderTexture.ReleaseTemporary(_chain[index]);
                        _chain[index] = null;
                    }

                    index++;
                }
            }

            _chain = null;
            _chainWidth = 0;
            _chainHeight = 0;
            _chainFormat = 0;
        }

        private static bool DeckThisFrame(RenderTexture source)
        {
            try
            {
                int memory = 0;
                if (SystemInfo.graphicsMemorySize > 0)
                {
                    memory = SystemInfo.graphicsMemorySize;
                }

                string deviceName = SystemInfo.graphicsDeviceName;
                string deviceModel = SystemInfo.deviceModel;
                if (PostGrade.LooksLikeDeck(Screen.width, Screen.height, deviceName, deviceModel, memory))
                {
                    return true;
                }

                if (source != null && PostGrade.LooksLikeDeck(source.width, source.height, deviceName, deviceModel, memory))
                {
                    return true;
                }
            }
            catch (System.Exception)
            {
                return false;
            }

            return false;
        }
    }
}
