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
        private int _savedCull = -1;
        private bool _layersPreview;

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
            AssignViewport();
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

            _slots.Rotate(0f, IdleSpinDegrees * Time.unscaledDeltaTime, 0f, Space.Self);
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
                _key = CreateStudioLight("HangarPreviewKey", new Vector3(1.7f, 2.1f, -1.35f), new Color(1f, 0.82f, 0.52f), 1.4f);
                _fill = CreateStudioLight("HangarPreviewFill", new Vector3(-1.85f, 0.95f, -0.7f), new Color(0.45f, 0.66f, 0.88f), 0.55f);
                _rim = CreateStudioLight("HangarPreviewRim", new Vector3(0.15f, 1.35f, 2.15f), new Color(1f, 0.78f, 0.4f), 0.95f);
            }

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

            if (hangar)
            {
                if (_savedCull < 0)
                {
                    _savedCull = world.cullingMask;
                }

                world.cullingMask = _savedCull & ~(1 << PreviewLayer);
                return;
            }

            if (_savedCull >= 0)
            {
                world.cullingMask = _savedCull;
            }
        }
    }
}
