using UnityEngine;

namespace AsteroidsGoneRogue
{
    public sealed class ShipHealth : MonoBehaviour, IDamageable
    {
        private GameManager _game;
        private ShipVisuals _visuals;
        private int _hull = LoadoutState.HullHitPoints;
        private int _shield;
        private bool _dead;

        public int Hull
        {
            get { return _hull; }
        }

        public int Shield
        {
            get { return _shield; }
        }

        public void Bind(GameManager game, ShipVisuals visuals)
        {
            _game = game;
            _visuals = visuals;
        }

        public void ResetForWave(LoadoutState loadout)
        {
            _dead = false;
            _hull = loadout != null ? loadout.CurrentHullHitPoints : LoadoutState.HullHitPoints;
            _shield = loadout != null ? loadout.ShieldCharges : 0;
            if (_visuals != null)
            {
                _visuals.SetShieldVisible(_shield > 0);
            }
        }

        public void ApplyDamage(int amount)
        {
            if (_dead || amount <= 0)
            {
                return;
            }

            int remaining = amount;
            if (_shield > 0)
            {
                int absorbed = Mathf.Min(_shield, remaining);
                _shield -= absorbed;
                remaining -= absorbed;
            }

            if (remaining > 0)
            {
                _hull -= remaining;
            }

            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayPlayerDamage();
            }

            if (_visuals != null)
            {
                _visuals.SetShieldVisible(_shield > 0);
            }

            if (_hull <= 0)
            {
                _hull = 0;
                _dead = true;
                if (_game != null)
                {
                    _game.NotifyPlayerDestroyed();
                }
            }
            else if (_game != null)
            {
                _game.RefreshHud();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            Collider other = collision.collider;
            if (other.CompareTag(GameTags.Asteroid) || other.CompareTag(GameTags.Enemy))
            {
                ApplyDamage(1);
                IDamageable damageable = other.GetComponentInParent<IDamageable>();
                if (damageable != null && !ReferenceEquals(damageable, this))
                {
                    damageable.ApplyDamage(1);
                }
            }
        }
    }
}
