using UnityEngine;

namespace AsteroidsGoneRogue
{
    public sealed class FollowCamera : MonoBehaviour
    {
        public Vector3 Offset = new Vector3(0f, 35f, -22f);
        public float Follow = 8f;
        public const float ShakeDecay = 11f;
        public const float MaxShake = 0.36f;
        public static readonly Vector3 HangarLook = new Vector3(0.25f, 0.4f, 0f);
        public static readonly Vector3 HangarOffset = new Vector3(0f, 31f, -19f);

        private Transform _target;
        private float _shake;
        private bool _hangarFraming;
        private bool _captureHold;
        private Vector3 _capturePos;
        private Vector3 _captureLook;
        private float _captureFov = 54f;
        private float _springX;
        private float _springY;
        private float _springZ;
        private float _springVx;
        private float _springVy;
        private float _springVz;
        private bool _springReady;
        private float _ox;
        private float _oy;
        private float _oz;
        private float _vx;
        private float _vy;
        private float _vz;
        private WaveManager _pressureWaves;

        public void HoldCapturePose(Vector3 position, Vector3 look, float fov)
        {
            _captureHold = true;
            _capturePos = position;
            _captureLook = look;
            _captureFov = fov > 1f ? fov : 54f;
            transform.position = _capturePos;
            transform.LookAt(_captureLook);
            Camera cam = GetComponent<Camera>();
            if (cam != null)
            {
                cam.fieldOfView = _captureFov;
            }
        }

        public void ClearCapturePose()
        {
            _captureHold = false;
        }

        public void SetTarget(Transform target)
        {
            _target = target;
            if (_target != null)
            {
                Vector3 snapped = _target.position + PlayOffset();
                transform.position = snapped;
                transform.LookAt(_target.position);
                _springX = snapped.x;
                _springY = snapped.y;
                _springZ = snapped.z;
                _springVx = 0f;
                _springVy = 0f;
                _springVz = 0f;
                _springReady = true;
            }
        }

        public void SetHangarFraming(bool hangar)
        {
            _hangarFraming = hangar;
            if (hangar)
            {
                transform.position = HangarLook + HangarOffset;
                transform.LookAt(HangarLook);
                _springReady = false;
            }
        }

        public void AddShake(float amplitude)
        {
            AddShake(amplitude, Vector3.zero);
        }

        public void AddShake(float amplitude, Vector3 direction)
        {
            if (!SettingsState.ScreenShakeEnabled)
            {
                ClearShake();
                return;
            }

            float applied = SettingsState.ShakeAmplitude(true, amplitude);
            applied = EffectScale.ShakeAdd(true, SettingsState.ReduceEffectsEnabled, applied);
            if (applied <= 0f)
            {
                return;
            }

            CameraFeel.Impulse(ref _vx, ref _vy, ref _vz, direction.x, direction.y, direction.z, applied);
            float shakeCap = EffectScale.ShakeCap(SettingsState.ReduceEffectsEnabled, MaxShake);
            _shake = shakeCap;
        }

        public void AddShakeWorld(float amplitude, Vector3 worldPosition)
        {
            Vector3 direction = worldPosition;
            if (_target != null)
            {
                direction = worldPosition - _target.position;
            }

            AddShake(amplitude, direction);
        }

        private void ClearShake()
        {
            _shake = 0f;
            _ox = 0f;
            _oy = 0f;
            _oz = 0f;
            _vx = 0f;
            _vy = 0f;
            _vz = 0f;
        }

        private void LateUpdate()
        {
            if (!SettingsState.ScreenShakeEnabled)
            {
                ClearShake();
            }

            if (_captureHold)
            {
                transform.position = _capturePos;
                transform.LookAt(_captureLook);
                Camera cam = GetComponent<Camera>();
                if (cam != null)
                {
                    cam.fieldOfView = _captureFov;
                    cam.rect = new Rect(0f, 0f, 1f, 1f);
                }

                return;
            }

            if (_hangarFraming)
            {
                Vector3 desiredHangar = HangarLook + HangarOffset;
                transform.position = Vector3.Lerp(transform.position, desiredHangar, 1f - Mathf.Exp(-Follow * Time.deltaTime));
                transform.LookAt(HangarLook);
                Camera hangarCam = GetComponent<Camera>();
                if (hangarCam != null)
                {
                    hangarCam.fieldOfView = ArenaFraming.BaseFov;
                }

                ApplyPlayViewport();
                return;
            }

            if (_target == null)
            {
                ApplyPlayViewport();
                return;
            }

            float aimX;
            float aimZ;
            float speed;
            ReadAim(out aimX, out aimZ, out speed);
            bool reduce = SettingsState.ReduceEffectsEnabled;
            float leadX;
            float leadZ;
            CameraFeel.LookAhead(
                _target.position.x,
                _target.position.z,
                aimX,
                aimZ,
                speed,
                100000f,
                CameraFeel.FairHalfExtent,
                reduce,
                out leadX,
                out leadZ);
            ArenaShape viewShape = ArenaShapeCatalog.Active;
            float focusX = _target.position.x + leadX;
            float focusZ = _target.position.z + leadZ;
            if (viewShape.DistanceToEdge(focusX, focusZ) < 0f)
            {
                float edgeX;
                float edgeZ;
                viewShape.ClosestPoint(focusX, focusZ, out edgeX, out edgeZ);
                leadX = edgeX - _target.position.x;
                leadZ = edgeZ - _target.position.z;
            }
            Vector3 look = _target.position + new Vector3(leadX, 0f, leadZ);
            Vector3 desired = look + PlayOffset();
            float step = Time.deltaTime;
            if (!_springReady)
            {
                _springX = desired.x;
                _springY = desired.y;
                _springZ = desired.z;
                _springVx = 0f;
                _springVy = 0f;
                _springVz = 0f;
                _springReady = true;
            }

            CameraFeel.SpringLimited(ref _springX, ref _springVx, desired.x, CameraFeel.FollowOmega, step, CameraFeel.MaxLag);
            CameraFeel.SpringLimited(ref _springY, ref _springVy, desired.y, CameraFeel.FollowOmega, step, CameraFeel.MaxLag);
            CameraFeel.SpringLimited(ref _springZ, ref _springVz, desired.z, CameraFeel.FollowOmega, step, CameraFeel.MaxLag);

            if (SettingsState.ScreenShakeEnabled)
            {
                float shakeCap = EffectScale.ShakeCap(reduce, MaxShake);
                CameraFeel.DecayShake(
                    ref _ox,
                    ref _oy,
                    ref _oz,
                    ref _vx,
                    ref _vy,
                    ref _vz,
                    CameraFeel.ShakeOmega,
                    step,
                    shakeCap);
                _shake = Mathf.Sqrt((_ox * _ox) + (_oy * _oy) + (_oz * _oz));
            }
            else
            {
                ClearShake();
            }

            transform.position = new Vector3(_springX + _ox, _springY + _oy, _springZ + _oz);
            transform.LookAt(look);
            Camera playCam = GetComponent<Camera>();
            if (playCam != null)
            {
                ArenaShape zoomShape = ArenaShapeCatalog.Active;
                float edge = zoomShape.DistanceToEdge(_target.position.x, _target.position.z);
                float bound = zoomShape.BoundingRadius;
                if (bound < 1f)
                {
                    bound = 1f;
                }

                float zoom = CameraFeel.ZoomAdd(speed, ReadPressure(), bound - edge, bound, reduce);
                playCam.fieldOfView = ArenaFraming.BaseFov + zoom;
            }

            ApplyPlayViewport();
        }

        private void ReadAim(out float aimX, out float aimZ, out float speed)
        {
            aimX = 0f;
            aimZ = 1f;
            speed = 0f;
            if (_target == null)
            {
                return;
            }

            Rigidbody body = _target.GetComponent<Rigidbody>();
            Vector3 velocity = body != null ? body.linearVelocity : Vector3.zero;
            velocity.y = 0f;
            speed = velocity.magnitude;
            if (velocity.sqrMagnitude > 0.25f)
            {
                aimX = velocity.x;
                aimZ = velocity.z;
                return;
            }

            Vector3 forward = _target.forward;
            aimX = forward.x;
            aimZ = forward.z;
        }

        private int ReadPressure()
        {
            if (_hangarFraming || _target == null)
            {
                return 0;
            }

            if (_pressureWaves == null)
            {
                _pressureWaves = UnityEngine.Object.FindAnyObjectByType<WaveManager>();
            }

            if (_pressureWaves == null)
            {
                return 0;
            }

            return _pressureWaves.RemainingThreats;
        }

        /// <summary>
        /// World 1's contour floor is larger than the 0.48 30 m disc. Pull the
        /// same offset back so the plates fill the frame the way they did then.
        /// </summary>
        private Vector3 PlayOffset()
        {
            float pull = 1f;
            if (ReadWorld() == 1)
            {
                pull = ArenaShapeCatalog.World1CameraPull;
            }

            return Offset * pull;
        }

        private int ReadWorld()
        {
            if (_pressureWaves == null)
            {
                _pressureWaves = UnityEngine.Object.FindAnyObjectByType<WaveManager>();
            }

            if (_pressureWaves == null)
            {
                return 1;
            }

            return WorldCatalog.NumberForWave(_pressureWaves.ActiveWave);
        }

        /// <summary>
        /// Pillarbox or letterbox so the 16:9 playfield is neither wider nor cropped.
        /// Vertical FOV stays at ArenaFraming.BaseFov plus a small clamped zoom.
        /// UI is a screen-space overlay. The viewport rect is unchanged.
        /// </summary>
        private void ApplyPlayViewport()
        {
            Camera cam = GetComponent<Camera>();
            if (cam == null)
            {
                return;
            }

            float viewX;
            float viewY;
            float viewW;
            float viewH;
            ArenaFraming.Viewport(Screen.width, Screen.height, out viewX, out viewY, out viewW, out viewH);
            cam.rect = new Rect(viewX, viewY, viewW, viewH);
        }
    }
}
