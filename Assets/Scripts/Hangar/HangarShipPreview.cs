using UnityEngine;
using UnityEngine.UI;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Hangar bay: playable ship idle-spins at hero / showcase scale inside a
    /// dedicated RenderTexture viewport (framed on the right of the hangar UI).
    /// Studio is parked behind the hangar camera so a world-space ship can never
    /// leak through the shop list even if a layer cull is missed. Default-layer
    /// mesh renderers are disabled while shopping so the main camera is empty.
    /// </summary>
    [DefaultExecutionOrder(10000)]
    public sealed class HangarShipPreview : MonoBehaviour
    {
        public const float IdleSpinDegrees = 18f;
        public const float PreviewX = 6.35f;
        public const float PreviewZ = 0.4f;
        public const float StudioZ = -140f;
        public const float ShowcaseScale = 1.25f;
        public const float PlayScale = 1f;
        public const int PreviewLayer = 8;
        public const int ViewportWidth = 768;
        public const int ViewportHeight = 960;
        public const float CameraFov = 40f;

        private static readonly Vector3 CameraLocal = new Vector3(0.2f, 4.55f, -10f);
        private static readonly Vector3 CameraLookLocal = new Vector3(0f, 0.08f, 0f);

        private static Color StudioClear
        {
            get
            {
                return new Color(HangarPreviewRig.BackdropEdgeR, HangarPreviewRig.BackdropEdgeG, HangarPreviewRig.BackdropEdgeB, 1f);
            }
        }

        private Transform _slots;
        private bool _active;
        private Rigidbody _body;
        private bool _savedKinematic;
        private Camera _studio;
        private RenderTexture _rt;
        private RawImage _viewport;
        private Light _key;
        private Light _fill;
        private Light _rim;
        private Light _bounce;
        private GameObject _backdrop;
        private Texture2D _backdropTex;
        private int _savedPlayCull = -1;
        private int _savedDecorCull = -1;
        private bool _layersPreview;
        private bool _interactionHold;
        private int _upgradeMask;
        private int _mk2Mask;
        private int _paintId = -1;
        private int _trailId = -1;
        private GameObject _mk2Marker;
        private GameObject _trailRibbon;
        private int _previewFormat = -1;

        public void Bind(Transform slots)
        {
            _slots = slots;
            _body = GetComponent<Rigidbody>();
        }

        public void BindViewport(RawImage viewport)
        {
            _viewport = viewport;
            EnsureRig();
            AssignViewport();
        }

        public void NotifyVisualsChanged()
        {
            if (!_active)
            {
                return;
            }

            _layersPreview = false;
            ApplyPreviewLayer(true);
            HideDefaultRenderers(true);
            ApplyAccents();
            AssignViewport();
        }

        public void SetInteractionHold(bool hold)
        {
            _interactionHold = hold;
        }

        public void SetFittings(int upgradeMask, int mk2Mask, int paintId, int trailId)
        {
            _upgradeMask = upgradeMask;
            _mk2Mask = mk2Mask;
            _paintId = paintId;
            _trailId = trailId;
            if (_active)
            {
                ApplyAccents();
            }
        }

        public void SetActive(bool active)
        {
            _active = active;
            ApplyPreviewScale(active);
            if (!active && _slots != null)
            {
                _slots.localRotation = Quaternion.identity;
            }

            EnsureRig();
            ApplyPreviewLayer(active);
            HideDefaultRenderers(active);
            ApplyWorldCull(active);
            PoseForMode(active);
            if (_studio != null)
            {
                _studio.targetTexture = _rt;
                _studio.enabled = active;
                _studio.stereoTargetEye = StereoTargetEyeMask.None;
            }

            SetLightActive(_key, active);
            SetLightActive(_fill, active);
            SetLightActive(_rim, active);
            SetLightActive(_bounce, active);
            SetBackdropActive(active);
            ApplyLightRecipe();
            PoseBackdrop();
            if (!active)
            {
                SetMarkerActive(_mk2Marker, false);
                SetMarkerActive(_trailRibbon, false);
            }

            AssignViewport();
            if (active)
            {
                RenderStudio();
            }
        }

        private void Update()
        {
            if (!_active || _slots == null)
            {
                return;
            }

            EnsurePreviewTarget();
            bool reduceSpin = SettingsState.ReduceEffectsEnabled;
            bool holdSpin = _interactionHold || PointerOverPreview();
            float spin = HangarPreviewRig.SpinDegreesPerSecond(holdSpin, reduceSpin);
            if (spin != 0f)
            {
                _slots.Rotate(0f, spin * Time.unscaledDeltaTime, 0f, Space.Self);
            }

            ApplyPreviewScale(true);
            PoseForMode(true);
            ApplyPreviewLayer(true);
            HideDefaultRenderers(true);
            AssignViewport();
        }

        private void LateUpdate()
        {
            if (!_active)
            {
                return;
            }

            PoseForMode(true);
            ApplyPreviewLayer(true);
            HideDefaultRenderers(true);
            ApplyWorldCull(true);
            ApplyLightRecipe();
            PoseBackdrop();
            ApplyAccents();
            AssignViewport();
            RenderStudio();
        }

        private void OnDestroy()
        {
            ApplyPreviewLayer(false);
            HideDefaultRenderers(false);
            ApplyWorldCull(false);
            if (_viewport != null)
            {
                _viewport.texture = null;
            }

            if (_rt != null)
            {
                _rt.Release();
                Destroy(_rt);
                _rt = null;
            }

            if (_backdropTex != null)
            {
                Destroy(_backdropTex);
                _backdropTex = null;
            }
        }

        private void ApplyPreviewScale(bool showcase)
        {
            if (_slots == null)
            {
                return;
            }

            float scale = showcase ? ShowcaseScale : PlayScale;
            Vector3 desired = Vector3.one * scale;
            if ((_slots.localScale - desired).sqrMagnitude > 0.0001f)
            {
                _slots.localScale = desired;
            }
        }

        private Vector3 StudioWorld()
        {
            return new Vector3(PreviewX, ShipController.PlayHeight, StudioZ);
        }

        private void PoseForMode(bool hangar)
        {
            if (!hangar)
            {
                RestoreBodyKinematic();
                return;
            }

            Vector3 pos = StudioWorld();
            if (_body != null)
            {
                if (!_body.isKinematic)
                {
                    _savedKinematic = _body.isKinematic;
                    _body.isKinematic = true;
                }

                _body.interpolation = RigidbodyInterpolation.None;
                _body.linearVelocity = Vector3.zero;
                _body.angularVelocity = Vector3.zero;
                _body.position = pos;
                _body.rotation = Quaternion.identity;
            }

            transform.SetPositionAndRotation(pos, Quaternion.identity);
        }

        private void RestoreBodyKinematic()
        {
            if (_body != null)
            {
                _body.isKinematic = _savedKinematic;
            }
        }

        private void EnsureRig()
        {
            if (_studio == null)
            {
                GameObject camObject = new GameObject("HangarPreviewCamera");
                camObject.transform.SetParent(transform, false);
                camObject.transform.localPosition = CameraLocal;
                camObject.transform.localRotation = Quaternion.LookRotation(CameraLookLocal - CameraLocal);
                _studio = camObject.AddComponent<Camera>();
                _studio.clearFlags = CameraClearFlags.SolidColor;
                _studio.backgroundColor = StudioClear;
                _studio.fieldOfView = CameraFov;
                _studio.nearClipPlane = 0.2f;
                _studio.farClipPlane = 24f;
                _studio.cullingMask = 1 << PreviewLayer;
                _studio.depth = 20f;
                _studio.allowHDR = false;
                _studio.allowMSAA = false;
                _studio.stereoTargetEye = StereoTargetEyeMask.None;
                _studio.enabled = false;
            }

            EnsurePreviewTarget();
            _studio.targetTexture = _rt;
            _studio.stereoTargetEye = StereoTargetEyeMask.None;
            _studio.transform.localPosition = CameraLocal;
            _studio.transform.localRotation = Quaternion.LookRotation(CameraLookLocal - CameraLocal);
            _studio.fieldOfView = CameraFov;
            if (_key == null)
            {
                _key = CreateStudioLight(
                    "HangarPreviewKey",
                    new Vector3(HangarPreviewRig.KeyLocalX, HangarPreviewRig.KeyLocalY, HangarPreviewRig.KeyLocalZ),
                    new Color(HangarPreviewRig.KeyR, HangarPreviewRig.KeyG, HangarPreviewRig.KeyB),
                    HangarPreviewRig.KeyIntensity,
                    LightType.Spot,
                    HangarPreviewRig.KeyRange,
                    HangarPreviewRig.KeySpotAngle);
                _fill = CreateStudioLight(
                    "HangarPreviewFill",
                    new Vector3(HangarPreviewRig.FillLocalX, HangarPreviewRig.FillLocalY, HangarPreviewRig.FillLocalZ),
                    new Color(HangarPreviewRig.FillR, HangarPreviewRig.FillG, HangarPreviewRig.FillB),
                    HangarPreviewRig.FillIntensity,
                    LightType.Point,
                    HangarPreviewRig.FillRange,
                    0f);
                _rim = CreateStudioLight(
                    "HangarPreviewRim",
                    new Vector3(HangarPreviewRig.RimLocalX, HangarPreviewRig.RimLocalY, HangarPreviewRig.RimLocalZ),
                    new Color(HangarPreviewRig.RimR, HangarPreviewRig.RimG, HangarPreviewRig.RimB),
                    HangarPreviewRig.RimIntensity,
                    LightType.Spot,
                    HangarPreviewRig.RimRange,
                    HangarPreviewRig.RimSpotAngle);
                _bounce = CreateStudioLight(
                    "HangarPreviewBounce",
                    new Vector3(HangarPreviewRig.BounceLocalX, HangarPreviewRig.BounceLocalY, HangarPreviewRig.BounceLocalZ),
                    new Color(HangarPreviewRig.BounceR, HangarPreviewRig.BounceG, HangarPreviewRig.BounceB),
                    HangarPreviewRig.BounceIntensity,
                    LightType.Point,
                    HangarPreviewRig.BounceRange,
                    0f);
            }

            EnsureBackdrop();
            ApplyLightRecipe();
            PoseBackdrop();

            AssignViewport();
        }

        private void EnsurePreviewTarget()
        {
            bool halfSupported;
            bool defaultSupported;
            QueryPreviewHdr(out halfSupported, out defaultSupported);
            int format = HangarPreviewHdr.Format(
                SettingsState.ReduceEffectsEnabled,
                halfSupported,
                defaultSupported);
            if (_rt != null && _previewFormat == format)
            {
                if (_studio != null)
                {
                    _studio.allowHDR = HangarPreviewHdr.AllowsHdr(format);
                    _studio.targetTexture = _rt;
                }

                return;
            }

            ReleasePreviewTarget();
            _previewFormat = format;
            RenderTextureFormat textureFormat = RenderTextureFormat.ARGB32;
            if (format == HangarPreviewHdr.ArgbHalf)
            {
                textureFormat = RenderTextureFormat.ARGBHalf;
            }
            else if (format == HangarPreviewHdr.DefaultHdr)
            {
                textureFormat = RenderTextureFormat.DefaultHDR;
            }

            _rt = new RenderTexture(ViewportWidth, ViewportHeight, 16, textureFormat);
            _rt.name = "HangarShipPreviewRT";
            _rt.antiAliasing = 1;
            _rt.useMipMap = false;
            _rt.Create();
            ClearStudioTarget();
            if (_studio != null)
            {
                _studio.allowHDR = HangarPreviewHdr.AllowsHdr(format);
                _studio.targetTexture = _rt;
            }
        }

        private static void QueryPreviewHdr(out bool halfSupported, out bool defaultSupported)
        {
            halfSupported = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf);
            defaultSupported = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.DefaultHDR);
        }

        private void ReleasePreviewTarget()
        {
            if (_studio != null)
            {
                _studio.targetTexture = null;
            }

            if (_viewport != null)
            {
                _viewport.texture = null;
            }

            if (_rt == null)
            {
                return;
            }

            _rt.Release();
            Destroy(_rt);
            _rt = null;
        }

        private void ClearStudioTarget()
        {
            if (_rt == null)
            {
                return;
            }

            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = _rt;
            GL.Clear(true, true, StudioClear);
            RenderTexture.active = prev;
        }

        private void RenderStudio()
        {
            if (_studio == null || _rt == null)
            {
                return;
            }

            _studio.targetTexture = _rt;
            _studio.cullingMask = 1 << PreviewLayer;
            _studio.stereoTargetEye = StereoTargetEyeMask.None;
            _studio.Render();
        }

        private void AssignViewport()
        {
            if (_viewport == null || _rt == null)
            {
                return;
            }

            if (_viewport.texture != _rt)
            {
                _viewport.texture = _rt;
            }

            float exposure = PremiumLook.PreviewExposure;
            _viewport.color = new Color(exposure, exposure, exposure, 1f);
            _viewport.raycastTarget = false;
            _viewport.enabled = true;
            if (!_viewport.gameObject.activeSelf)
            {
                _viewport.gameObject.SetActive(true);
            }
        }

        private Light CreateStudioLight(
            string name,
            Vector3 local,
            Color color,
            float intensity,
            LightType type,
            float range,
            float spotAngle)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = local;
            Light light = go.AddComponent<Light>();
            light.type = type;
            light.range = range;
            light.color = color;
            light.intensity = intensity;
            light.shadows = LightShadows.None;
            light.cullingMask = 1 << PreviewLayer;
            if (type == LightType.Spot)
            {
                light.spotAngle = spotAngle;
                light.innerSpotAngle = spotAngle * HangarPreviewRig.SpotInnerFraction;
            }

            AimStudioLight(light, local);
            light.enabled = false;
            return light;
        }

        private static void AimStudioLight(Light light, Vector3 local)
        {
            if (light == null || light.type != LightType.Spot)
            {
                return;
            }

            Vector3 aim = CameraLookLocal - local;
            if (aim.sqrMagnitude < 0.0001f)
            {
                return;
            }

            light.transform.localRotation = Quaternion.LookRotation(aim, Vector3.up);
        }

        private static void SetLightActive(Light light, bool on)
        {
            if (light != null)
            {
                light.enabled = on;
            }
        }

        private bool IsStudioNode(Transform node)
        {
            if (node == null)
            {
                return false;
            }

            if (_studio != null && (node == _studio.transform || node.IsChildOf(_studio.transform)))
            {
                return true;
            }

            if (_key != null && (node == _key.transform || node.IsChildOf(_key.transform)))
            {
                return true;
            }

            if (_fill != null && (node == _fill.transform || node.IsChildOf(_fill.transform)))
            {
                return true;
            }

            if (_rim != null && (node == _rim.transform || node.IsChildOf(_rim.transform)))
            {
                return true;
            }

            return _bounce != null && (node == _bounce.transform || node.IsChildOf(_bounce.transform));
        }

        private void ApplyPreviewLayer(bool preview)
        {
            int layer = preview ? PreviewLayer : 0;
            Transform[] nodes = GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < nodes.Length; i++)
            {
                Transform node = nodes[i];
                if (node == null || IsStudioNode(node))
                {
                    continue;
                }

                node.gameObject.layer = layer;
            }

            _layersPreview = preview;
        }

        private void HideDefaultRenderers(bool hangar)
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null || IsStudioNode(renderer.transform))
                {
                    continue;
                }

                if (hangar && renderer.gameObject.layer != PreviewLayer)
                {
                    renderer.enabled = false;
                    continue;
                }

                renderer.enabled = true;
            }
        }

        private void ApplyWorldCull(bool hangar)
        {
            Camera[] cameras = Camera.allCameras;
            bool sawMain = false;
            for (int i = 0; i < cameras.Length; i++)
            {
                Camera world = cameras[i];
                if (world == Camera.main)
                {
                    sawMain = true;
                }

                CullWorldCamera(world, hangar);
            }

            if (!sawMain)
            {
                CullWorldCamera(Camera.main, hangar);
            }
        }

        private void CullWorldCamera(Camera world, bool hangar)
        {
            if (world == null || world == _studio)
            {
                return;
            }

            bool decorCam = world.gameObject.name == DecorCameraStack.CameraName;
            if (hangar)
            {
                if (decorCam)
                {
                    if (_savedDecorCull < 0)
                    {
                        _savedDecorCull = world.cullingMask;
                    }

                    world.cullingMask = _savedDecorCull & ~(1 << PreviewLayer);
                }
                else
                {
                    if (_savedPlayCull < 0)
                    {
                        _savedPlayCull = world.cullingMask;
                    }

                    world.cullingMask = _savedPlayCull & ~(1 << PreviewLayer);
                }

                return;
            }

            if (decorCam && _savedDecorCull >= 0)
            {
                world.cullingMask = _savedDecorCull;
                return;
            }

            if (!decorCam && _savedPlayCull >= 0)
            {
                world.cullingMask = _savedPlayCull;
            }
        }

        private bool PointerOverPreview()
        {
            if (_viewport == null)
            {
                return false;
            }

            RectTransform rect = _viewport.rectTransform;
            if (rect == null)
            {
                return false;
            }

            return RectTransformUtility.RectangleContainsScreenPoint(rect, Input.mousePosition, null);
        }

        private void ApplyLightRecipe()
        {
            float gain = PremiumLook.PreviewStudioGain;
            TuneLight(_key, HangarPreviewRig.KeyR, HangarPreviewRig.KeyG, HangarPreviewRig.KeyB, HangarPreviewRig.KeyIntensity * gain);
            TuneLight(_fill, HangarPreviewRig.FillR, HangarPreviewRig.FillG, HangarPreviewRig.FillB, HangarPreviewRig.FillIntensity * gain);
            float rim = HangarPreviewRig.StudioRim(SettingsState.ReduceEffectsEnabled) * gain;
            TuneLight(_rim, HangarPreviewRig.RimR, HangarPreviewRig.RimG, HangarPreviewRig.RimB, rim);
            TuneLight(_bounce, HangarPreviewRig.BounceR, HangarPreviewRig.BounceG, HangarPreviewRig.BounceB, HangarPreviewRig.BounceIntensity * gain);
            AimStudioLight(_key, new Vector3(HangarPreviewRig.KeyLocalX, HangarPreviewRig.KeyLocalY, HangarPreviewRig.KeyLocalZ));
            AimStudioLight(_rim, new Vector3(HangarPreviewRig.RimLocalX, HangarPreviewRig.RimLocalY, HangarPreviewRig.RimLocalZ));
        }

        private void EnsureBackdrop()
        {
            if (_backdrop != null)
            {
                return;
            }

            Shader unlit = Shader.Find("Unlit/Texture");
            if (unlit == null)
            {
                unlit = Shader.Find("Sprites/Default");
            }

            if (unlit == null)
            {
                unlit = Shader.Find("Standard");
            }

            if (unlit == null)
            {
                return;
            }

            _backdropTex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            _backdropTex.name = "HangarPreviewBackdrop";
            _backdropTex.wrapMode = TextureWrapMode.Clamp;
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    float u = x / 63f;
                    float v = y / 63f;
                    float red;
                    float green;
                    float blue;
                    HangarPreviewRig.BackdropRgb(u, v, out red, out green, out blue);
                    _backdropTex.SetPixel(x, y, new Color(red, green, blue, 1f));
                }
            }

            _backdropTex.Apply(false, true);
            _backdrop = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _backdrop.name = "HangarPreviewBackdrop";
            _backdrop.transform.SetParent(transform, false);
            Collider backdropCollider = _backdrop.GetComponent<Collider>();
            if (backdropCollider != null)
            {
                backdropCollider.enabled = false;
                Destroy(backdropCollider);
            }

            Renderer backdropRenderer = _backdrop.GetComponent<Renderer>();
            Material backdropMaterial = new Material(unlit);
            backdropMaterial.name = "Mat_HangarPreviewBackdrop";
            backdropMaterial.mainTexture = _backdropTex;
            backdropMaterial.color = Color.white;
            if (backdropRenderer != null)
            {
                backdropRenderer.sharedMaterial = backdropMaterial;
                backdropRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                backdropRenderer.receiveShadows = false;
            }

            _backdrop.layer = PreviewLayer;
            _backdrop.SetActive(false);
        }

        private void PoseBackdrop()
        {
            if (_backdrop == null)
            {
                return;
            }

            Vector3 direction = CameraLookLocal - CameraLocal;
            float distance = direction.magnitude;
            if (distance < 0.01f)
            {
                return;
            }

            direction /= distance;
            float placeDistance = distance + HangarPreviewRig.BackdropPastLook;
            _backdrop.transform.localPosition = CameraLocal + (direction * placeDistance);
            _backdrop.transform.localRotation = Quaternion.LookRotation(-direction, Vector3.up);
            float half = placeDistance * Mathf.Tan(CameraFov * 0.5f * Mathf.Deg2Rad);
            float height = half * 2f * HangarPreviewRig.BackdropMargin;
            float width = height * (ViewportWidth / (float)ViewportHeight);
            _backdrop.transform.localScale = new Vector3(width, height, 1f);
            if (_backdrop.layer != PreviewLayer)
            {
                _backdrop.layer = PreviewLayer;
            }
        }

        private void SetBackdropActive(bool on)
        {
            if (_backdrop != null && _backdrop.activeSelf != on)
            {
                _backdrop.SetActive(on);
            }
        }

        private static void TuneLight(Light light, float red, float green, float blue, float intensity)
        {
            if (light == null)
            {
                return;
            }

            light.color = new Color(red, green, blue, 1f);
            light.intensity = intensity;
            light.shadows = LightShadows.None;
        }

        private void ApplyAccents()
        {
            if (!_active || _slots == null)
            {
                return;
            }

            EnsureMarkers();
            HangarPreviewRig.Accent accent = HangarPreviewRig.FromMasks(_upgradeMask, _mk2Mask, _paintId, _trailId);
            float meshEmission = EffectScale.MeshEmission(SettingsState.ReduceEffectsEnabled);
            Renderer[] renderers = _slots.GetComponentsInChildren<Renderer>(true);
            for (int accentIndex = 0; accentIndex < renderers.Length; accentIndex++)
            {
                Renderer renderer = renderers[accentIndex];
                if (renderer == null)
                {
                    continue;
                }

                if (renderer.gameObject == _mk2Marker || renderer.gameObject == _trailRibbon)
                {
                    continue;
                }

                string nodeName = renderer.gameObject.name;
                bool hardpoint = accent.Hardpoint && (NameHas(nodeName, "Nose_Upgrade") || NameHas(nodeName, "RailHardpoint") || NameHas(nodeName, "Overcharger"));
                bool engine = accent.Engine && (NameHas(nodeName, "Engine_Upgrade") || NameHas(nodeName, "Afterburner"));
                bool painted = accent.PaintId >= 0 && (NameHas(nodeName, "Ship_Body") || NameHas(nodeName, "Hull"));
                if (!hardpoint && !engine && !painted)
                {
                    CapPreviewBody(renderer);
                    continue;
                }

                float red;
                float green;
                float blue;
                if (painted)
                {
                    ShopSinkCatalog.PaintRgb(accent.PaintId, out red, out green, out blue);
                }
                else if (hardpoint)
                {
                    red = 1f;
                    green = 0.62f;
                    blue = 0.28f;
                }
                else
                {
                    red = 0.45f;
                    green = 0.78f;
                    blue = 1f;
                }

                float emission = PremiumLook.PreviewAccentEmission(meshEmission, red, green, blue);
                ShipPaint.TintRenderer(renderer, red, green, blue, emission);
            }

            SetMarkerActive(_mk2Marker, accent.Mk2);
            if (_mk2Marker != null && accent.Mk2)
            {
                Renderer markerRenderer = _mk2Marker.GetComponent<Renderer>();
                float markerEmission = PremiumLook.PreviewAccentEmission(meshEmission, 0.831f, 0.627f, 0.29f);
                ShipPaint.TintRenderer(markerRenderer, 0.831f, 0.627f, 0.29f, markerEmission);
            }

            bool showTrail = ShopSinkCatalog.IsTrail(accent.TrailId);
            SetMarkerActive(_trailRibbon, showTrail);
            if (_trailRibbon != null && showTrail)
            {
                float trailR;
                float trailG;
                float trailB;
                ShopSinkCatalog.TrailRgb(accent.TrailId, out trailR, out trailG, out trailB);
                Renderer trailRenderer = _trailRibbon.GetComponent<Renderer>();
                float trailEmission = PremiumLook.PreviewAccentEmission(meshEmission, trailR, trailG, trailB);
                ShipPaint.TintRenderer(trailRenderer, trailR, trailG, trailB, trailEmission);
            }
        }

        private static void CapPreviewBody(Renderer renderer)
        {
            if (renderer == null)
            {
                return;
            }

            Material material = renderer.sharedMaterial;
            if (material == null || !material.HasProperty("_Color"))
            {
                ShipPaint.ClearRenderer(renderer);
                return;
            }

            Color albedo = material.GetColor("_Color");
            float lift = PremiumLook.PreviewAlbedoLift;
            albedo.r = albedo.r * lift;
            albedo.g = albedo.g * lift;
            albedo.b = albedo.b * lift;
            if (albedo.r > 1f)
            {
                albedo.r = 1f;
            }

            if (albedo.g > 1f)
            {
                albedo.g = 1f;
            }

            if (albedo.b > 1f)
            {
                albedo.b = 1f;
            }

            albedo.a = 1f;
            Color emit = Color.black;
            if (material.HasProperty("_EmissionColor"))
            {
                Color raw = material.GetColor("_EmissionColor");
                float cappedR;
                float cappedG;
                float cappedB;
                PremiumLook.CapPreviewEmission(raw.r, raw.g, raw.b, out cappedR, out cappedG, out cappedB);
                emit = new Color(cappedR, cappedG, cappedB, 1f);
            }

            MaterialPropertyBlock block = new MaterialPropertyBlock();
            block.SetColor("_Color", albedo);
            block.SetColor("_EmissionColor", emit);
            renderer.SetPropertyBlock(block);
        }

        private void EnsureMarkers()
        {
            if (_slots == null)
            {
                return;
            }

            if (_mk2Marker == null)
            {
                _mk2Marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                _mk2Marker.name = "Mk2Marker";
                _mk2Marker.transform.SetParent(_slots, false);
                _mk2Marker.transform.localPosition = new Vector3(0f, 1.15f, 0.05f);
                _mk2Marker.transform.localScale = new Vector3(0.16f, 0.16f, 0.16f);
                _mk2Marker.layer = PreviewLayer;
                StripCollider(_mk2Marker);
                _mk2Marker.SetActive(false);
            }

            if (_trailRibbon == null)
            {
                _trailRibbon = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _trailRibbon.name = "TrailRibbon";
                _trailRibbon.transform.SetParent(_slots, false);
                _trailRibbon.transform.localPosition = new Vector3(0f, 0.15f, -1.35f);
                _trailRibbon.transform.localScale = new Vector3(0.12f, 0.08f, 0.85f);
                _trailRibbon.layer = PreviewLayer;
                StripCollider(_trailRibbon);
                _trailRibbon.SetActive(false);
            }
        }

        private static void StripCollider(GameObject marker)
        {
            if (marker == null)
            {
                return;
            }

            Collider markerCollider = marker.GetComponent<Collider>();
            if (markerCollider != null)
            {
                markerCollider.enabled = false;
                Destroy(markerCollider);
            }
        }

        private static void SetMarkerActive(GameObject marker, bool on)
        {
            if (marker != null && marker.activeSelf != on)
            {
                marker.SetActive(on);
            }
        }

        private static bool NameHas(string nodeName, string token)
        {
            if (string.IsNullOrEmpty(nodeName) || string.IsNullOrEmpty(token))
            {
                return false;
            }

            return nodeName.IndexOf(token, System.StringComparison.Ordinal) >= 0;
        }
    }
}
