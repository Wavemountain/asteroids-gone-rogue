using UnityEngine;

namespace AsteroidsGoneRogue
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ShipController : MonoBehaviour
    {
        public const float Thrust = 28f;
        public const float MaxSpeed = 16f;
        public const float TurnDegreesPerSecond = 480f;
        public const float PlayHeight = 0.4f;

        private Rigidbody _body;
        private ShipShooter _shooter;
        private Camera _camera;
        private bool _inputEnabled;
        private bool _padAim;

        public ShipHealth Health { get; private set; }
        public ShipShooter Shooter { get; private set; }
        public ShipVisuals Visuals { get; private set; }

        public void Bind(ShipHealth health, ShipShooter shooter, ShipVisuals visuals, Camera camera)
        {
            Health = health;
            _shooter = shooter;
            Shooter = shooter;
            Visuals = visuals;
            _camera = camera;
            _body = GetComponent<Rigidbody>();
        }

        public void SetInputEnabled(bool enabled)
        {
            _inputEnabled = enabled;
            if (!enabled && _body != null)
            {
                _body.linearVelocity = Vector3.zero;
                _body.angularVelocity = Vector3.zero;
            }
        }

        public void ResetForWave(LoadoutState loadout)
        {
            transform.SetPositionAndRotation(new Vector3(0f, PlayHeight, 0f), Quaternion.identity);
            if (_body != null)
            {
                _body.linearVelocity = Vector3.zero;
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

            Aim();
            if (GamepadInput.CyclePressed())
            {
                _shooter.CycleFireMode();
            }

            if (GamepadInput.FireHeld())
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

            Vector2 stick = GamepadInput.MoveStick();
            Vector3 input = new Vector3(stick.x, 0f, stick.y);
            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            _body.AddForce(input * Thrust, ForceMode.Acceleration);
            if (_body.linearVelocity.sqrMagnitude > MaxSpeed * MaxSpeed)
            {
                _body.linearVelocity = _body.linearVelocity.normalized * MaxSpeed;
            }

            ClampToArena();
        }

        private void Aim()
        {
            Vector2 padAim = GamepadInput.AimStick();
            if (padAim.sqrMagnitude > 0.01f)
            {
                _padAim = true;
                AimDirection(new Vector3(padAim.x, 0f, padAim.y));
                return;
            }

            Vector2 padMove = GamepadInput.PadMoveStick();
            if (padMove.sqrMagnitude > 0.01f)
            {
                _padAim = true;
                AimDirection(new Vector3(padMove.x, 0f, padMove.y));
                return;
            }

            if (GamepadInput.MouseDelta().sqrMagnitude > 0.0001f || Input.GetMouseButton(0))
            {
                _padAim = false;
            }

            if (!_padAim)
            {
                AimAtMouse();
            }
        }

        private void AimDirection(Vector3 dir)
        {
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.04f)
            {
                return;
            }

            Quaternion target = Quaternion.LookRotation(dir.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                target,
                TurnDegreesPerSecond * Time.deltaTime);
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
            pos.y = PlayHeight;
            Vector3 planar = new Vector3(pos.x, 0f, pos.z);
            float limit = WaveManager.ArenaRadius - 1.4f;
            if (planar.sqrMagnitude > limit * limit)
            {
                planar = planar.normalized * limit;
                pos.x = planar.x;
                pos.z = planar.z;
                transform.position = pos;
                Vector3 vel = _body.linearVelocity;
                vel += -planar.normalized * 2f;
                vel.y = 0f;
                _body.linearVelocity = vel;
            }
            else if (Mathf.Abs(transform.position.y - PlayHeight) > 0.01f)
            {
                transform.position = pos;
            }
        }
    }
}
