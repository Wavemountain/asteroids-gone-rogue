using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Hangar bay: playable ship sits on the right, idle-spins, and shows loadout / hover preview.
    /// </summary>
    public sealed class HangarShipPreview : MonoBehaviour
    {
        public const float IdleSpinDegrees = 18f;
        public const float PreviewX = 6.35f;
        public const float PreviewZ = 0.4f;

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
            Vector3 pos = new Vector3(PreviewX, ShipController.PlayHeight, PreviewZ);
            if ((transform.position - pos).sqrMagnitude > 0.0004f)
            {
                transform.position = pos;
            }
        }
    }
}
