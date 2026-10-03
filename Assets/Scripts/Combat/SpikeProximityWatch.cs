using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// One global spike-tick voice. Pitch follows distance. Silent during i-frames.
    /// </summary>
    public sealed class SpikeProximityWatch : MonoBehaviour
    {
        private Transform _player;
        private ShipHealth _health;
        private float _lastTick;

        public void Bind(Transform player, ShipHealth health)
        {
            _player = player;
            _health = health;
        }

        private void Update()
        {
            if (_player == null)
            {
                return;
            }

            float distance = ArenaHazard.NearestDamagingDistance(_player.position);
            bool invulnerable = _health != null && _health.IsInvulnerable;
            if (!SpikeProximity.ShouldTick(Time.time, _lastTick, distance, invulnerable))
            {
                return;
            }

            _lastTick = Time.time;
            if (AudioCues.Instance == null)
            {
                return;
            }

            AudioCues.Instance.PlaySpikeNearTick(SpikeProximity.Pitch01(distance));
        }
    }
}
