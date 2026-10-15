using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Persistent point lights for rail shots, heavy kills, and pierce shots.
    /// Lights stay on this object and follow by copying position, so a
    /// destroyed projectile cannot take the pool with it.
    /// </summary>
    public sealed class ProjectileLightPool : MonoBehaviour
    {
        private static ProjectileLightPool _active;

        private readonly Light[] _lights = new Light[ProjectileLightBudget.PoolSize];
        private readonly ProjectileLightSlot[] _slots = new ProjectileLightSlot[ProjectileLightBudget.PoolSize];
        private readonly Transform[] _follow = new Transform[ProjectileLightBudget.PoolSize];
        private readonly bool[] _tracked = new bool[ProjectileLightBudget.PoolSize];

        public static void Ensure(Transform parent)
        {
            if (_active != null)
            {
                return;
            }

            GameObject root = new GameObject("ProjectileLightPool");
            if (parent != null)
            {
                root.transform.SetParent(parent, false);
            }

            _active = root.AddComponent<ProjectileLightPool>();
        }

        public static void Pulse(Vector3 position, int kind, int colorId, Transform follow)
        {
            if (_active == null)
            {
                return;
            }

            _active.Arm(position, kind, colorId, follow);
        }

        private void Awake()
        {
            _active = this;
            int index = 0;
            while (index < ProjectileLightBudget.PoolSize)
            {
                GameObject child = new GameObject("ProjectileLight" + index);
                child.transform.SetParent(transform, false);
                Light lamp = child.AddComponent<Light>();
                lamp.type = LightType.Point;
                lamp.shadows = LightShadows.None;
                lamp.enabled = false;
                _lights[index] = lamp;
                index++;
            }
        }

        private void OnDestroy()
        {
            if (_active == this)
            {
                _active = null;
            }
        }

        private void Arm(Vector3 position, int kind, int colorId, Transform follow)
        {
            bool reduce = SettingsState.ReduceEffectsEnabled;
            int cap = ProjectileLightBudget.ActiveCap(reduce, QualitySettings.pixelLightCount);
            int index = ProjectileLightBudget.TryAcquire(
                _slots,
                cap,
                kind,
                Time.unscaledTime,
                ProjectileLightBudget.SecondsFor(kind),
                colorId);
            if (index < 0)
            {
                return;
            }

            Light lamp = _lights[index];
            if (lamp == null)
            {
                return;
            }

            lamp.enabled = true;
            lamp.type = LightType.Point;
            lamp.shadows = LightShadows.None;
            lamp.intensity = ProjectileLightBudget.IntensityFor(kind, reduce);
            lamp.range = ProjectileLightBudget.RangeFor(kind);
            float red;
            float green;
            float blue;
            EmissionTargets.Rgb(colorId, out red, out green, out blue);
            lamp.color = new Color(red, green, blue, 1f);
            _follow[index] = follow;
            _tracked[index] = follow != null;
            lamp.transform.position = position;
        }

        private void LateUpdate()
        {
            bool reduce = SettingsState.ReduceEffectsEnabled;
            int cap = ProjectileLightBudget.ActiveCap(reduce, QualitySettings.pixelLightCount);
            float now = Time.unscaledTime;
            int index = 0;
            while (index < ProjectileLightBudget.PoolSize)
            {
                bool lostFollow = _tracked[index] && _follow[index] == null;
                bool drop = index >= cap
                    || _slots[index].Kind == ProjectileLightBudget.KindNone
                    || _slots[index].Until <= now
                    || lostFollow;
                Light lamp = _lights[index];
                if (drop)
                {
                    if (lamp != null)
                    {
                        lamp.enabled = false;
                    }

                    _slots[index].Kind = ProjectileLightBudget.KindNone;
                    _follow[index] = null;
                    _tracked[index] = false;
                }
                else if (lamp != null)
                {
                    if (_follow[index] != null)
                    {
                        lamp.transform.position = _follow[index].position;
                    }

                    lamp.intensity = ProjectileLightBudget.IntensityFor(_slots[index].Kind, reduce);
                    lamp.enabled = true;
                }

                index++;
            }
        }
    }
}
