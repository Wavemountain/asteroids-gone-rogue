using System.IO;
using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// F12 captures the Game view. F9 cycles Atmos store poses. Writes under
    /// <see cref="StoreCapturePoses.OutFolder"/> in the Editor, otherwise
    /// persistentDataPath/StoreCaptures. Placeholders live in Docs/StoreCaptures.
    /// </summary>
    public sealed class StoreCaptureDirector : MonoBehaviour
    {
        public const KeyCode CaptureKey = KeyCode.F12;
        public const KeyCode CycleKey = KeyCode.F9;

        private FollowCamera _follow;
        private Camera _camera;
        private int _poseIndex;
        private bool _holding;
        private float _shotArmed = -1f;
        private string _pendingName = string.Empty;

        public static StoreCaptureDirector Instance { get; private set; }

        public static StoreCaptureDirector Ensure()
        {
            if (Instance != null)
            {
                return Instance;
            }

            GameObject go = new GameObject("StoreCaptureDirector");
            Instance = go.AddComponent<StoreCaptureDirector>();
            return Instance;
        }

        private void Awake()
        {
            Instance = this;
            _camera = Camera.main;
            _follow = _camera != null ? _camera.GetComponent<FollowCamera>() : null;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(CycleKey))
            {
                CyclePose();
            }

            if (Input.GetKeyDown(CaptureKey))
            {
                CaptureCurrent();
            }

            if (_shotArmed > 0f && Time.unscaledTime >= _shotArmed)
            {
                _shotArmed = -1f;
                FlushPendingShot();
            }
        }

        public void CyclePose()
        {
            _poseIndex = (_poseIndex + 1) % StoreCapturePoses.ShotIds.Length;
            ApplyPose(StoreCapturePoses.ShotIds[_poseIndex]);
        }

        public void ApplyPose(string id)
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }

            if (_follow == null && _camera != null)
            {
                _follow = _camera.GetComponent<FollowCamera>();
            }

            float px, py, pz, lx, ly, lz, fov;
            StoreCapturePoses.Pose(id, out px, out py, out pz, out lx, out ly, out lz, out fov);
            Vector3 pos = new Vector3(px, py, pz);
            Vector3 look = new Vector3(lx, ly, lz);
            if (_follow != null)
            {
                _follow.HoldCapturePose(pos, look, fov);
            }
            else if (_camera != null)
            {
                _camera.transform.position = pos;
                _camera.transform.LookAt(look);
                _camera.fieldOfView = fov;
            }

            _holding = true;
            _pendingName = StoreCapturePoses.FileName(id);
        }

        public void ReleasePose()
        {
            _holding = false;
            if (_follow != null)
            {
                _follow.ClearCapturePose();
            }
        }

        public void CaptureCurrent()
        {
            if (string.IsNullOrEmpty(_pendingName))
            {
                _pendingName = "capture_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
            }

            _shotArmed = Time.unscaledTime + 0.05f;
        }

        private void FlushPendingShot()
        {
            string folder = ResolveOutFolder();
            Directory.CreateDirectory(folder);
            string path = Path.Combine(folder, _pendingName);
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log("Store capture → " + path);
        }

        public static string ResolveOutFolder()
        {
#if UNITY_EDITOR
            return Path.GetFullPath(StoreCapturePoses.OutFolder);
#else
            return Path.Combine(Application.persistentDataPath, "StoreCaptures");
#endif
        }

        public bool Holding
        {
            get { return _holding; }
        }
    }
}
