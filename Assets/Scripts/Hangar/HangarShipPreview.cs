using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Hangar bay: playable ship sits on the right, idle-spins at hero / showcase
    /// scale so upgrades and hover-preview read clearly.
    /// </summary>
    public sealed class HangarShipPreview : MonoBehaviour
    {
        public const float IdleSpinDegrees = 18f;
        public const float PreviewX = 6.35f;
        public const float PreviewZ = 0.4f;
        public const float ShowcaseScale = 2.25f;
        public const float PlayScale = 1f;

        private Transform _slots;
        private bool _active;
        private Rigidbody _body;

        public void Bind(Transform slots)
        {
            _slots = slots;
            _body = GetComponent<Rigidbody>();
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
    }
}
