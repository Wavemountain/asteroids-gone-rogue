using UnityEngine;

namespace AsteroidsGoneRogue
{
    public sealed class FollowCamera : MonoBehaviour
    {
        public Vector3 Offset = new Vector3(0f, 26f, -16f);
        public float Follow = 8f;

        private Transform _target;

        public void SetTarget(Transform target)
        {
            _target = target;
            if (_target != null)
            {
                transform.position = _target.position + Offset;
                transform.LookAt(_target.position);
            }
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                return;
            }

            Vector3 desired = _target.position + Offset;
            transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-Follow * Time.deltaTime));
            transform.LookAt(_target.position);
        }
    }
}
