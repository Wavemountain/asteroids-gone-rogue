using UnityEngine;
using UnityEngine.UI;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Hangar bay: playable ship idle-spins at hero / showcase scale inside a
    /// dedicated RenderTexture viewport (framed on the right of the hangar UI).
    /// </summary>
    public sealed class HangarShipPreview : MonoBehaviour
    {
        public const float IdleSpinDegrees = 18f;
        public const float PreviewX = 6.35f;
        public const float PreviewZ = 0.4f;
        public const float ShowcaseScale = 2.25f;
        public const float PlayScale = 1f;
        public const int PreviewLayer = 8;
        public const int ViewportWidth = 768;
        public const int ViewportHeight = 960;
        public const float CameraFov = 32f;

        private static readonly Vector3 CameraLocal = new Vector3(0.2f, 2.05f, -5.7f);
        private static readonly Vector3 CameraLookLocal = new Vector3(0f, 0.42f, 0f);
        private static readonly Color StudioClear = new Color(0.028f, 0.038f, 0.058f, 1f);

        private Transform _slots;
        private bool _active;
        private Rigidbody _body;
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
            if (_viewport != null)
            {
                _viewport.texture = _rt;
                _viewport.color = Color.white;
                _viewport.raycastTarget = false;
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

            if (active)
            {
                Vector3 pos = new Vector3(PreviewX, ShipController.PlayHeight, PreviewZ);
                transform.SetPositionAndRotation(pos, Quaternion.identity);
                if (_body != null)
                {
                    _body.linearVelocity = Vector3.zero;
                    _body.angularVelocity = Vector3.zero;
                }
            }

            EnsureRig();
            ApplyPreviewLayer(active);
            ApplyWorldCull(active);
            if (_studio != null)
            {
                _studio.enabled = active;
            }

            SetLightActive(_key, active);
            SetLightActive(_fill, active);
            SetLightActive(_rim, active);
        }

        private void Update()
        {
            if (!_active || _slots == null)
            {
                return;
            }

            _slots.Rotate(0f, IdleSpinDegrees * Time.unscaledDeltaTime, 0f, Space.Self);
            ApplyPreviewScale(true);
            Vector3 pos = new Vector3(PreviewX, ShipController.PlayHeight, PreviewZ);
            if ((transform.position - pos).sqrMagnitude > 0.0004f)
            {
                transform.position = pos;
            }
        }

        private void OnDestroy()
        {
            ApplyPreviewLayer(false);
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

        private void EnsureRig()
        {
            if (_rt == null)
            {
                _rt = new RenderTexture(ViewportWidth, ViewportHeight, 16);
                _rt.name = "HangarShipPreviewRT";
                _rt.antiAliasing = 4;
                _rt.Create();
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
                _studio.farClipPlane = 18f;
                _studio.cullingMask = 1 << PreviewLayer;
                _studio.depth = -20f;
                _studio.allowHDR = false;
                _studio.allowMSAA = true;
                _studio.enabled = false;
            }

            _studio.targetTexture = _rt;
            if (_key == null)
            {
                _key = CreateStudioLight("HangarPreviewKey", new Vector3(1.7f, 2.1f, -1.35f), new Color(1f, 0.82f, 0.52f), 1.4f);
                _fill = CreateStudioLight("HangarPreviewFill", new Vector3(-1.85f, 0.95f, -0.7f), new Color(0.45f, 0.66f, 0.88f), 0.55f);
                _rim = CreateStudioLight("HangarPreviewRim", new Vector3(0.15f, 1.35f, 2.15f), new Color(1f, 0.78f, 0.4f), 0.95f);
            }

            if (_viewport != null && _viewport.texture != _rt)
            {
                _viewport.texture = _rt;
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

        private void ApplyPreviewLayer(bool preview)
        {
            if (preview == _layersPreview)
            {
                return;
            }

            _layersPreview = preview;
            int layer = preview ? PreviewLayer : 0;
            Transform[] nodes = GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < nodes.Length; i++)
            {
                Transform node = nodes[i];
                if (node == null || node.GetComponent<Camera>() != null || node.GetComponent<Light>() != null)
                {
                    continue;
                }

                node.gameObject.layer = layer;
            }
        }

        private void ApplyWorldCull(bool hangar)
        {
            Camera world = Camera.main;
            if (world == null)
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
