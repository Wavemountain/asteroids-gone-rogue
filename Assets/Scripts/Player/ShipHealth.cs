using UnityEngine;

namespace AsteroidsGoneRogue
{
    public sealed class ShipHealth : MonoBehaviour, IDamageable
    {
        public const float HitInvulnerabilitySeconds = 1.15f;

        private GameManager _game;
        private ShipVisuals _visuals;
        private int _hull = LoadoutState.HullHitPoints;
        private int _maxHull = LoadoutState.HullHitPoints;
        private int _shield;
        private bool _dead;
        private float _invulnerableUntil;
        private DamageCause _lastCause = DamageCause.Unknown;
        private EnemyKind _lastEnemyKind = EnemyKind.Mid01;

        public int Hull
        {
            get { return _hull; }
        }

        public int Shield
        {
            get { return _shield; }
        }

        public bool IsInvulnerable
        {
            get { return !_dead && Time.time < _invulnerableUntil; }
        }

        public DamageCause LastDamageCause
        {
            get { return _lastCause; }
        }

        public EnemyKind LastEnemyKind
        {
            get { return _lastEnemyKind; }
        }

        public void Bind(GameManager game, ShipVisuals visuals)
        {
            _game = game;
            _visuals = visuals;
        }

        public void ResetForWave(LoadoutState loadout)
        {
            _dead = false;
            _maxHull = loadout != null ? loadout.CurrentHullHitPoints : LoadoutState.HullHitPoints;
            _hull = _maxHull;
            _shield = loadout != null ? loadout.ShieldCharges : 0;
            _lastCause = DamageCause.Unknown;
            _lastEnemyKind = EnemyKind.Mid01;
            ClearInvulnerability();
            if (_visuals != null)
            {
                _visuals.SetShieldVisible(_shield > 0);
            }
        }

        public bool TryHeal(int amount)
        {
            if (_dead || amount <= 0 || _hull >= _maxHull)
            {
                return false;
            }

            _hull = Mathf.Min(_maxHull, _hull + amount);
            if (_game != null)
            {
                _game.RefreshHud();
            }

            return true;
        }

        public bool TryAddShield()
        {
            if (_dead || _shield >= LoadoutState.MaxShieldCharges)
            {
                return false;
            }

            _shield += 1;
            if (_visuals != null)
            {
                _visuals.SetShieldVisible(true);
            }

            if (_game != null)
            {
                _game.RefreshHud();
            }

            return true;
        }

        public void ApplyDamage(int amount)
        {
            ApplyDamage(amount, DamageCause.Unknown);
        }

        public void ApplyDamage(int amount, DamageCause cause)
        {
            ApplyDamage(amount, cause, EnemyKind.Mid01);
        }

        public void ApplyDamage(int amount, DamageCause cause, EnemyKind enemyKind)
        {
            if (_dead || amount <= 0 || IsInvulnerable)
            {
                return;
            }

            _lastCause = cause;
            _lastEnemyKind = enemyKind;
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
                ClearInvulnerability();
                CombatJuice.PlayerDamaged(true);
                if (_game != null)
                {
                    string reason = cause == DamageCause.EnemyContact
                        ? DamageCauseText.FailReason(cause, enemyKind)
                        : DamageCauseText.FailReason(cause);
                    _game.NotifyPlayerDestroyed(reason);
                }
            }
            else
            {
                CombatJuice.PlayerDamaged(false);
                BeginInvulnerability();
                if (_game != null)
                {
                    _game.RefreshHud();
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_dead || IsInvulnerable)
            {
                return;
            }

            Collider other = collision.collider;
            DamageCause cause;
            if (other.CompareTag(GameTags.Asteroid))
            {
                cause = DamageCause.AsteroidCollision;
            }
            else if (other.CompareTag(GameTags.Enemy))
            {
                cause = DamageCause.EnemyContact;
            }
            else
            {
                return;
            }

            if (cause == DamageCause.EnemyContact)
            {
                EnemySeeker seeker = other.GetComponentInParent<EnemySeeker>();
                EnemyKind kind = seeker != null ? seeker.Kind : EnemyKind.Mid01;
                ApplyDamage(1, cause, kind);
            }
            else
            {
                ApplyDamage(1, cause);
            }
            IDamageable damageable = other.GetComponentInParent<IDamageable>();
            if (damageable != null && !ReferenceEquals(damageable, this))
            {
                damageable.ApplyDamage(1);
            }
        }

        private void BeginInvulnerability()
        {
            _invulnerableUntil = Time.time + HitInvulnerabilitySeconds;
            if (_visuals != null)
            {
                _visuals.PlayHitBlink(HitInvulnerabilitySeconds);
            }
        }

        private void ClearInvulnerability()
        {
            _invulnerableUntil = 0f;
            if (_visuals != null)
            {
                _visuals.StopHitBlink();
            }
        }
    }
}
