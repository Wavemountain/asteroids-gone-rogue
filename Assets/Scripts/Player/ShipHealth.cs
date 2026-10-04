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
        private bool _invulnCueArmed;
        private int _assistRemainder;
        private readonly DamageCauseLog _hits = new DamageCauseLog();

        public DamageCauseLog Hits
        {
            get { return _hits; }
        }

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
            _hits.Clear();
            _invulnCueArmed = false;
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
            ApplyDamage(amount, cause, enemyKind, false, string.Empty);
        }

        public void ApplyDamage(int amount, DamageCause cause, EnemyKind enemyKind, bool elite, string modifier)
        {
            if (_dead || amount <= 0 || IsInvulnerable)
            {
                return;
            }

            amount = DifficultySettings.ScaleIncomingDamage(amount, cause);
            amount = MutatorRules.ScaleDamage(amount, MutatorRuntime.Mask);
            bool assist = SettingsState.AssistEnabled;
            amount = AssistRules.ScaleIncoming(amount, cause, assist, ref _assistRemainder);
            if (assist && _game != null)
            {
                _game.NoteAssistUsed();
            }

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
                if (AssistRules.WasAbsorbed(amount, assist, cause))
                {
                    BeginInvulnerability();
                    if (AudioCues.Instance != null)
                    {
                        AudioCues.Instance.PlayArmorHit();
                    }

                    CombatJuice.PlayerDamaged(false);
                    if (_game != null)
                    {
                        _game.NotifyPlayerHit();
                        _game.RefreshHud();
                    }
                }

                return;
            }

            _lastCause = cause;
            _lastEnemyKind = enemyKind;
            int shieldBefore = _shield;
            int hullBefore = _hull;
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

            int hullLost = hullBefore - _hull;
            if (hullLost < 0)
            {
                hullLost = 0;
            }

            bool shieldBroke = shieldBefore > 0 && _shield <= 0;
            bool shieldOnly = hullLost <= 0;
            int waveNumber = 1;
            int worldNumber = 1;
            if (_game != null && _game.Session != null)
            {
                waveNumber = _game.Session.WaveIndex;
                worldNumber = WorldCatalog.NumberForWave(waveNumber);
            }

            _hits.Record(cause, enemyKind, amount, hullLost, waveNumber, worldNumber, shieldOnly, elite, modifier);
            if (_game != null)
            {
                _game.NotifyPlayerHit();
            }

            if (_visuals != null)
            {
                _visuals.SetShieldVisible(_shield > 0);
            }

            if (_hull <= 0)
            {
                _hull = 0;
                _dead = true;
                if (shieldBroke && AudioCues.Instance != null)
                {
                    AudioCues.Instance.PlayShieldBreak();
                }

                ClearInvulnerability();
                CombatJuice.PlayerDamaged(true);
                if (_game != null)
                {
                    _game.NotifyPlayerDestroyed(cause, enemyKind);
                }

                return;
            }

            if (shieldOnly && shieldBroke)
            {
                if (AudioCues.Instance != null)
                {
                    AudioCues.Instance.PlayShieldBreak();
                }

                CombatJuice.PlayerShieldBreak(transform);
            }
            else if (shieldOnly)
            {
                if (AudioCues.Instance != null)
                {
                    AudioCues.Instance.PlayShieldHit();
                }

                CombatJuice.PlayerShieldHit(transform);
            }
            else
            {
                if (AudioCues.Instance != null)
                {
                    AudioCues.Instance.PlayPlayerDamage();
                    AudioCues.Instance.DuckMusic(AudioCues.HullDuckSeconds, AudioCues.HullDuckScale);
                    if (shieldBroke)
                    {
                        AudioCues.Instance.PlayShieldBreak();
                    }
                }

                CombatJuice.PlayerDamaged(false);
            }

            BeginInvulnerability();
            if (_game != null)
            {
                _game.RefreshHud();
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
                if (seeker != null && !seeker.CanHarm)
                {
                    return;
                }

                EnemyKind kind = seeker != null ? seeker.Kind : EnemyKind.Mid01;
                bool eliteHit = seeker != null && seeker.IsElite;
                int contactWave = _game != null && _game.Session != null ? _game.Session.WaveIndex : 1;
                string eliteName = eliteHit ? WaveModifier.NameForWave(contactWave) : string.Empty;
                int contactDamage = DifficultyCurve.ScaleOutgoingDamage(1, contactWave);
                ApplyDamage(contactDamage, cause, kind, eliteHit, eliteName);
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
            BeginImmunity(SpawnClearance.WaveImmunitySeconds);
        }

        public void GrantWaveImmunity()
        {
            if (_dead)
            {
                return;
            }

            BeginImmunity(SpawnClearance.WaveImmunitySeconds);
        }

        private void Update()
        {
            if (!_invulnCueArmed || IsInvulnerable || _dead)
            {
                return;
            }

            _invulnCueArmed = false;
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayInvulnEnd();
            }
        }

        private void BeginInvulnerability()
        {
            float iframeSeconds = HitInvulnerabilitySeconds * _iframePercent / 100f;
            BeginImmunity(iframeSeconds);
        }

        private void BeginImmunity(float seconds)
        {
            if (seconds < 0.05f)
            {
                seconds = 0.05f;
            }

            _invulnerableUntil = Time.time + seconds;
            _invulnCueArmed = true;
            if (_visuals != null)
            {
                _visuals.PlayHitBlink(seconds);
            }
        }

        private void ClearInvulnerability()
        {
            _invulnerableUntil = 0f;
            _invulnCueArmed = false;
            if (_visuals != null)
            {
                _visuals.StopHitBlink();
            }
        }
    }
}
