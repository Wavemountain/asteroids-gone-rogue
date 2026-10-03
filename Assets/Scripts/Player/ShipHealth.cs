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
        private int _maxShield = LoadoutState.MaxShieldCharges;
        private bool _dead;
        private float _invulnerableUntil;
        private int _iframePercent = 100;
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

        public int MaxHull
        {
            get { return _maxHull; }
        }

        public int MaxShield
        {
            get { return _maxShield; }
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
            ResetForWave(loadout, true, false);
        }

        public void ResetForWave(LoadoutState loadout, bool refillHull)
        {
            ResetForWave(loadout, refillHull, false);
        }

        public void ResetForWave(LoadoutState loadout, bool refillHull, bool applyWaveShield)
        {
            _dead = false;
            int hullBonus = DifficultySettings.PlayerHullBonus;
            int previousShieldMax = _maxShield;
            _maxHull = (loadout != null ? loadout.CurrentHullHitPoints : LoadoutState.HullHitPoints) + hullBonus;
            _maxShield = loadout != null ? loadout.CurrentMaxShield : LoadoutState.MaxShieldCharges;
            if (refillHull || _hull <= 0)
            {
                _hull = _maxHull;
            }
            else if (_hull > _maxHull)
            {
                _hull = _maxHull;
            }

            if (refillHull)
            {
                _shield = loadout != null ? loadout.ShieldCharges : 0;
            }
            else
            {
                int gainedShield = _maxShield - previousShieldMax;
                if (gainedShield > 0)
                {
                    _shield += gainedShield;
                }

                if (_shield > _maxShield)
                {
                    _shield = _maxShield;
                }

                if (_shield < 0)
                {
                    _shield = 0;
                }
            }

            if (applyWaveShield)
            {
                int waveShield = BoonHooks.StartingShield;
                if (waveShield > 0 && _shield < _maxShield)
                {
                    int raisedShield = _shield + waveShield;
                    _shield = raisedShield > _maxShield ? _maxShield : raisedShield;
                }
            }

            _lastCause = DamageCause.Unknown;
            _lastEnemyKind = EnemyKind.Mid01;
            ClearInvulnerability();
            if (_visuals != null)
            {
                _visuals.SetShieldVisible(_shield > 0);
            }
        }

        public void SetHull(int hull)
        {
            if (hull < 0)
            {
                hull = 0;
            }

            if (hull > _maxHull)
            {
                hull = _maxHull;
            }

            _hull = hull;
            _dead = _hull <= 0;
        }

        public void SetShield(int shield)
        {
            if (shield < 0)
            {
                shield = 0;
            }

            if (shield > _maxShield)
            {
                shield = _maxShield;
            }

            _shield = shield;
            if (_visuals != null)
            {
                _visuals.SetShieldVisible(_shield > 0);
            }
        }

        public void SetIFramePercent(int percent)
        {
            _iframePercent = percent < 100 ? 100 : percent;
        }

        public bool RefillHull()
        {
            if (_dead || _hull >= _maxHull)
            {
                return false;
            }

            _hull = _maxHull;
            if (_game != null)
            {
                _game.RefreshHud();
            }

            return true;
        }

        public bool RefillShield()
        {
            if (_dead || _maxShield <= 0 || _shield >= _maxShield)
            {
                return false;
            }

            _shield = _maxShield;
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
            if (_dead || _shield >= _maxShield)
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

            amount = DifficultySettings.ScaleIncomingDamage(amount, cause);
            if (amount > 0 && BoonHooks.TryIgnoreHit())
            {
                BeginInvulnerability();
                return;
            }

            if (BoonHooks.IncomingPercent < 100 && amount > 0)
            {
                int resisted = amount * BoonHooks.IncomingPercent / 100;
                amount = resisted < 1 ? 1 : resisted;
            }

            if (amount <= 0)
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

            if (_game != null)
            {
                _game.NotifyPlayerHit();
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
                    _game.NotifyPlayerDestroyed(cause, enemyKind);
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
                int contactWave = _game != null && _game.Session != null ? _game.Session.WaveIndex : 1;
                int contactDamage = DifficultyCurve.ScaleOutgoingDamage(1, contactWave);
                ApplyDamage(contactDamage, cause, kind);
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

        public void GrantRespawnIFrames()
        {
            _dead = false;
            BeginInvulnerability();
        }

        private void BeginInvulnerability()
        {
            float iframeSeconds = HitInvulnerabilitySeconds * _iframePercent / 100f;
            _invulnerableUntil = Time.time + iframeSeconds;
            if (_visuals != null)
            {
                _visuals.PlayHitBlink(iframeSeconds);
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
