using UnityEngine;

namespace AsteroidsGoneRogue
{
    public sealed class FollowCamera : MonoBehaviour
    {
        public Vector3 Offset = new Vector3(0f, 26f, -16f);
        public float Follow = 8f;
        public const float ShakeDecay = 11f;
        public const float MaxShake = 0.36f;

        private Transform _target;
        private float _shake;

        public void SetTarget(Transform target)
        {
            _target = target;
            if (_target != null)
            {
                transform.position = _target.position + Offset;
                transform.LookAt(_target.position);
            }
        }

        public void AddShake(float amplitude)
        {
            if (amplitude <= 0f)
            {
                return;
            }

            _shake = Mathf.Min(MaxShake, _shake + amplitude);
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                return;
            }

            Vector3 desired = _target.position + Offset;
            if (_shake > 0.001f)
            {
                Vector2 n = Random.insideUnitCircle * _shake;
                desired += new Vector3(n.x, 0f, n.y);
                _shake = Mathf.MoveTowards(_shake, 0f, ShakeDecay * Time.deltaTime);
            }

            transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-Follow * Time.deltaTime));
            transform.LookAt(_target.position);
        }
    }
}
