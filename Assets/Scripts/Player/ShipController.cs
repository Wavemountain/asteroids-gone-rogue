using UnityEngine;

namespace AsteroidsGoneRogue
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ShipController : MonoBehaviour
    {
        public const float Thrust = 28f;
        public const float MaxSpeed = 16f;
        public const float TurnDegreesPerSecond = 540f;

        private Rigidbody _body;
        private ShipShooter _shooter;
        private Camera _camera;
        private bool _inputEnabled;

        public ShipHealth Health { get; private set; }
        public ShipVisuals Visuals { get; private set; }

        public void Bind(ShipHealth health, ShipShooter shooter, ShipVisuals visuals, Camera camera)
        {
            Health = health;
            _shooter = shooter;
            Visuals = visuals;
            _camera = camera;
            _body = GetComponent<Rigidbody>();
        }

        public void SetInputEnabled(bool enabled)
        {
            _inputEnabled = enabled;
            if (!enabled && _body != null)
            {
                _body.velocity = Vector3.zero;
                _body.angularVelocity = Vector3.zero;
            }
        }

        public void ResetForWave(LoadoutState loadout)
        {
            transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            if (_body != null)
            {
                _body.velocity = Vector3.zero;
                _body.angularVelocity = Vector3.zero;
            }

            if (Health != null)
            {
                Health.ResetForWave(loadout);
            }
        }

        private void Update()
        {
            if (!_inputEnabled)
            {
                return;
            }

            AimAtMouse();
            if (Input.GetButton("Fire1") || Input.GetKey(KeyCode.Space))
            {
                _shooter.TryFire();
            }
        }

        private void FixedUpdate()
        {
            if (!_inputEnabled || _body == null)
            {
                return;
            }

            Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            _body.AddForce(input * Thrust, ForceMode.Acceleration);
            if (_body.velocity.sqrMagnitude > MaxSpeed * MaxSpeed)
            {
                _body.velocity = _body.velocity.normalized * MaxSpeed;
            }

            ClampToArena();
        }

        private void AimAtMouse()
        {
            if (_camera == null)
            {
                return;
            }

            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.up, transform.position);
            float enter;
            if (!plane.Raycast(ray, out enter))
            {
                return;
            }

            Vector3 point = ray.GetPoint(enter);
            Vector3 dir = point - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.05f)
            {
                return;
            }

            Quaternion target = Quaternion.LookRotation(dir.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                target,
                TurnDegreesPerSecond * Time.deltaTime);
        }

        private void ClampToArena()
        {
            Vector3 pos = transform.position;
            pos.y = 0f;
            float limit = WaveManager.ArenaRadius - 1.4f;
            if (pos.sqrMagnitude > limit * limit)
            {
                pos = pos.normalized * limit;
                transform.position = pos;
                Vector3 vel = _body.velocity;
                vel += -pos.normalized * 2f;
                vel.y = 0f;
                _body.velocity = vel;
            }
            else if (Mathf.Abs(transform.position.y) > 0.01f)
            {
                pos.y = 0f;
                transform.position = pos;
            }
        }
    }
}
