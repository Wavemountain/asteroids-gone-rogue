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
        private static readonly Color StudioClear = new Color(0.028f, 0.038f, 0.058f, 1f);

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
            ApplyLightRecipe();
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
            if (_rt == null)
            {
                _rt = new RenderTexture(ViewportWidth, ViewportHeight, 16, RenderTextureFormat.ARGB32);
                _rt.name = "HangarShipPreviewRT";
                _rt.antiAliasing = 1;
                _rt.useMipMap = false;
                _rt.Create();
                ClearStudioTarget();
            }

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

            _studio.targetTexture = _rt;
            _studio.stereoTargetEye = StereoTargetEyeMask.None;
            _studio.transform.localPosition = CameraLocal;
            _studio.transform.localRotation = Quaternion.LookRotation(CameraLookLocal - CameraLocal);
            _studio.fieldOfView = CameraFov;
            if (_key == null)
            {
                _key = CreateStudioLight(
                    "HangarPreviewKey",
                    new Vector3(-1.55f, 2.85f, -2.6f),
                    new Color(HangarPreviewRig.KeyR, HangarPreviewRig.KeyG, HangarPreviewRig.KeyB),
                    HangarPreviewRig.KeyIntensity);
                _fill = CreateStudioLight(
                    "HangarPreviewFill",
                    new Vector3(2.1f, 0.7f, -1.1f),
                    new Color(HangarPreviewRig.FillR, HangarPreviewRig.FillG, HangarPreviewRig.FillB),
                    HangarPreviewRig.FillIntensity);
                _rim = CreateStudioLight(
                    "HangarPreviewRim",
                    new Vector3(0.2f, 2.4f, 3.1f),
                    new Color(HangarPreviewRig.RimR, HangarPreviewRig.RimG, HangarPreviewRig.RimB),
                    HangarPreviewRig.RimIntensity);
            }

            ApplyLightRecipe();

            AssignViewport();
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

            _viewport.color = Color.white;
            _viewport.raycastTarget = false;
            _viewport.enabled = true;
            if (!_viewport.gameObject.activeSelf)
            {
                _viewport.gameObject.SetActive(true);
            }
        }

        private Light CreateStudioLight(string name, Vector3 local, Color color, float intensity)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = local;
            Light light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = 14f;
            light.color = color;
            light.intensity = intensity;
            light.cullingMask = 1 << PreviewLayer;
            light.enabled = false;
            return light;
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

            return _rim != null && (node == _rim.transform || node.IsChildOf(_rim.transform));
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
            TuneLight(_key, HangarPreviewRig.KeyR, HangarPreviewRig.KeyG, HangarPreviewRig.KeyB, HangarPreviewRig.KeyIntensity);
            TuneLight(_fill, HangarPreviewRig.FillR, HangarPreviewRig.FillG, HangarPreviewRig.FillB, HangarPreviewRig.FillIntensity);
            TuneLight(_rim, HangarPreviewRig.RimR, HangarPreviewRig.RimG, HangarPreviewRig.RimB, HangarPreviewRig.RimIntensity);
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
            float emission = EffectScale.MeshEmission(SettingsState.ReduceEffectsEnabled);
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
                    ShipPaint.ClearRenderer(renderer);
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

                ShipPaint.TintRenderer(renderer, red, green, blue, emission);
            }

            SetMarkerActive(_mk2Marker, accent.Mk2);
            if (_mk2Marker != null && accent.Mk2)
            {
                Renderer markerRenderer = _mk2Marker.GetComponent<Renderer>();
                ShipPaint.TintRenderer(markerRenderer, 0.831f, 0.627f, 0.29f, emission);
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
                ShipPaint.TintRenderer(trailRenderer, trailR, trailG, trailB, emission);
            }
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
