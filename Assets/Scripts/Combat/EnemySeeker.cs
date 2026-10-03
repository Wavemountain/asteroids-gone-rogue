using System.Collections.Generic;
using UnityEngine;

namespace AsteroidsGoneRogue
{
    public sealed class EnemySeeker : MonoBehaviour, IDamageable, IThreat
    {
        public const int HitPoints = 3;
        public const float Speed = 6.5f;
        public const float TurnDegreesPerSecond = 220f;

        private Transform _target;
        private WaveManager _waves;
        private EnemyKind _kind;
        private int _hp;
        private float _speed;
        private float _turn;
        private bool _dead;
        private bool _training;
        private Rigidbody _body;
        private ContentFactory _factory;
        private MonsterPresence _presence;
        private float _nextShot;
        private float _nextSpawn;
        private float _chargeUntil;
        private float _restUntil;
        private readonly List<EnemySeeker> _minions = new List<EnemySeeker>();
        private int _maxHp = 1;
        private int _fireRatePercent = 100;
        private bool _boss;
        private bool _elite;
        private int _bossPattern;
        private float _bossNext;
        private float _radialAt;
        private float _aimedFireAt;
        private Vector3 _aimedDir;
        private bool _aimedWinding;
        private float _bossBaseScale;
        private float _windupUntil;
        private float _boltWindupUntil;
        private Vector3 _boltAim;
        private bool _nestTold;
        private float _harmAt;

        public bool CanHarm
        {
            get { return Time.time >= _harmAt; }
        }

        public EnemyKind Kind
        {
            get { return _kind; }
        }

        public bool IsBoss
        {
            get { return _boss && !_dead; }
        }

        public bool IsElite
        {
            get { return _elite; }
        }

        public int CurrentHp
        {
            get { return _dead ? 0 : _hp; }
        }

        public int MaxHp
        {
            get { return _maxHp < 1 ? 1 : _maxHp; }
        }

        public void Initialize(Transform target, WaveManager waves)
        {
            Initialize(target, waves, EnemyKind.Mid01);
        }

        public void Initialize(Transform target, WaveManager waves, EnemyKind kind)
        {
            _target = target;
            _waves = waves;
            _kind = kind;
            int waveNumber = 1;
            if (waves != null)
            {
                waveNumber = waves.ActiveWave;
                if (WorldCatalog.NumberForWave(waves.ActiveWave) < 1)
                {
                    waveNumber = 1;
                }
            }

            _hp = DifficultyCurve.ScaleHp(EnemyCatalog.HitPoints(kind), waveNumber, DifficultySettings.Current);
            _maxHp = _hp;
            _speed = EnemyCatalog.Speed(kind);
            _turn = EnemyCatalog.TurnDegreesPerSecond(kind);
            _fireRatePercent = DifficultyCurve.ForWave(waveNumber).FireRatePercent;
            WaveModifierKind waveMod = waves != null ? waves.Modifier : WaveModifierKind.None;
            if (waveMod == WaveModifierKind.FasterEnemies)
            {
                _speed = _speed * WaveModifier.FasterPercent / 100f;
                _turn = _turn * WaveModifier.FasterPercent / 100f;
                if (EnemyCatalog.FiresBolts(kind))
                {
                    _fireRatePercent = _fireRatePercent * WaveModifier.FasterPercent / 100;
                }
            }
            else if (EnemyCatalog.FiresBolts(kind) && _fireRatePercent > 100)
            {
                _speed = _speed * _fireRatePercent / 100f;
            }

            int worldFire = WorldRules.EnemyFirePercentFor(waveNumber);
            if (worldFire > 100 && EnemyCatalog.FiresBolts(kind))
            {
                _fireRatePercent = _fireRatePercent * worldFire / 100;
            }

            _dead = false;
            _training = false;
            _boss = false;
            _elite = false;
            _bossPattern = 0;
            _bossNext = 0f;
            _radialAt = 0f;
            _aimedFireAt = 0f;
            _aimedWinding = false;
            _bossBaseScale = 1f;
            _body = GetComponent<Rigidbody>();
            _factory = Object.FindAnyObjectByType<ContentFactory>();
            _presence = GetComponent<MonsterPresence>();
            _nextShot = Time.time + 0.85f;
            if (kind == EnemyKind.Sniper || kind == EnemyKind.Gunner)
            {
                _nextShot = Time.time + 0.85f - EnemyCatalog.BoltWindupSeconds;
            }

            _nextSpawn = Time.time + 1.6f;
            _chargeUntil = 0f;
            _restUntil = 0f;
            _windupUntil = 0f;
            _boltWindupUntil = 0f;
            _nestTold = false;
            _harmAt = 0f;
        }

        public void BeginSpawnGrace()
        {
            _harmAt = Time.time + SpawnClearance.SpawnGraceSeconds;
            if (_factory != null)
            {
                _factory.SpawnTelegraphRing(
                    transform.position,
                    UiTheme.Secondary,
                    SpawnClearance.SpawnGraceSeconds,
                    TelegraphShape.DoubleRing,
                    transform.forward);
            }
        }

        public void ConfigureTraining(int hp, float speedScale)
        {
            _training = true;
            if (hp < 1)
            {
                hp = 1;
            }

            _hp = hp;
            _maxHp = hp;
            if (speedScale < 0.05f)
            {
                speedScale = 0.05f;
            }

            _speed *= speedScale;
            _turn *= speedScale;
            _nextShot = float.PositiveInfinity;
        }

        public void ConfigureElite(int hp)
        {
            _elite = true;
            if (hp < 1)
            {
                hp = 1;
            }

            _hp = hp;
            _maxHp = hp;
        }

        public void ConfigureBoss(int hp)
        {
            _boss = true;
            if (hp < 1)
            {
                hp = 1;
            }

            _hp = hp;
            _maxHp = hp;
            _speed *= BossRules.SpeedScale;
            _turn *= 0.85f;
            _bossPattern = 0;
            _radialAt = 0f;
            _aimedFireAt = 0f;
            _aimedWinding = false;
            _bossBaseScale = transform.localScale.x;
            if (_bossBaseScale < 0.01f)
            {
                _bossBaseScale = BossRules.VisualScale;
            }

            _bossNext = Time.time + 1.35f;
        }

        public void ApplyDamage(int amount)
        {
            if (_dead || amount <= 0)
            {
                return;
            }

            _hp -= amount;
            if (_hp > 0)
            {
                CombatJuice.ThreatDamaged(transform, false);
                if (AudioCues.Instance != null)
                {
                    AudioCues.Instance.PlayHit(_kind);
                }

                return;
            }

            _dead = true;
            if (_boss)
            {
                if (AudioCues.Instance != null)
                {
                    AudioCues.Instance.PlayBossFinale(transform.position);
                }
                else
                {
                    CombatJuice.HeavyKill(transform.position, CombatJuice.ExplosionShake);
                }
            }
            else
            {
                CombatJuice.ThreatDamaged(transform, true);
            }

            if (_factory != null)
            {
                _factory.SpawnVfx("Vfx_Explosion_Lowpoly", transform.position, 0.45f);
                if (!_training)
                {
                    _factory.MaybeDropPickup(transform.position);
                    _factory.MaybeDropExtraLife(transform.position, _kind);
                }
            }

            if (_waves != null)
            {
                _waves.NotifyDestroyed(this, EnemyCatalog.Score(_kind));
            }

            if (!_boss && AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayEnemyDeath(_kind);
            }

            Destroy(gameObject);
        }

        public void Despawn()
        {
            if (this != null)
            {
                Destroy(gameObject);
            }
        }

        private void FixedUpdate()
        {
            if (_dead || _target == null || _body == null)
            {
                return;
            }

            Vector3 toPlayer = _target.position - transform.position;
            toPlayer.y = 0f;
            if (toPlayer.sqrMagnitude < 0.01f)
            {
                if (_boss)
                {
                    TickBoss(transform.forward);
                }

                if (_kind == EnemyKind.Swarm)
                {
                    TelegraphNest();
                    TrySpawnMinion();
                }

                return;
            }

            Vector3 dir = toPlayer.normalized;
            float speed = _speed;
            float turn = _turn;
            if (_boss)
            {
                TickBoss(dir);
            }
            else if (_kind == EnemyKind.Brute)
            {
                TuneBrute(toPlayer.magnitude, ref speed, ref turn);
            }

            Quaternion look = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                look,
                turn * Time.fixedDeltaTime);
            _body.linearVelocity = dir * speed;
            if (_kind == EnemyKind.Swarm)
            {
                TelegraphNest();
                TrySpawnMinion();
            }
            else
            {
                TryFireBolt(dir);
            }

            Vector3 pos = transform.position;
            pos.y = 0f;
            float limit = WaveManager.ArenaRadius - 1.2f;
            if (pos.sqrMagnitude > limit * limit)
            {
                pos = pos.normalized * limit;
                transform.position = pos;
            }
        }

        private void TuneBrute(float distance, ref float speed, ref float turn)
        {
            float now = Time.time;
            if (now < _chargeUntil)
            {
                speed = EnemyCatalog.BruteChargeSpeed;
                turn = EnemyCatalog.BruteChargeTurn;
                if (_presence != null)
                {
                    _presence.SetCharging(true);
                }

                return;
            }

            if (_windupUntil > 0f && now < _windupUntil)
            {
                speed = 0f;
                turn = 480f;
                if (_presence != null)
                {
                    _presence.SetCharging(true);
                }

                return;
            }

            if (_windupUntil > 0f && now >= _windupUntil)
            {
                _windupUntil = 0f;
                _chargeUntil = now + EnemyCatalog.BruteChargeSeconds;
                _restUntil = _chargeUntil + EnemyCatalog.BruteRestSeconds;
                speed = EnemyCatalog.BruteChargeSpeed;
                turn = EnemyCatalog.BruteChargeTurn;
                if (_presence != null)
                {
                    _presence.SetCharging(true);
                }

                return;
            }

            if (_presence != null)
            {
                _presence.SetCharging(false);
            }

            if (now < _restUntil)
            {
                speed = _speed * 0.45f;
                return;
            }

            if (distance < EnemyCatalog.BruteChargeRange && distance > 2.2f)
            {
                _windupUntil = now + EnemyCatalog.BruteWindupSeconds;
                speed = 0f;
                turn = 480f;
                if (_presence != null)
                {
                    _presence.SetCharging(true);
                }

                if (_factory != null)
                {
                    _factory.SpawnTelegraphRing(
                        transform.position,
                        UiTheme.Danger,
                        EnemyCatalog.BruteWindupSeconds,
                        TelegraphShape.Ring,
                        transform.forward);
                }

                if (AudioCues.Instance != null)
                {
                    AudioCues.Instance.PlayBruteTell();
                }
            }
        }

        private void TelegraphNest()
        {
            if (_kind != EnemyKind.Swarm || _presence == null)
            {
                return;
            }

            if (Time.time > _nextSpawn - 0.7f && Time.time < _nextSpawn)
            {
                _presence.PulseNest();
                if (!_nestTold)
                {
                    _nestTold = true;
                    if (_factory != null)
                    {
                        _factory.SpawnTelegraphRing(
                            transform.position,
                            UiTheme.Secondary,
                            0.7f,
                            TelegraphShape.DoubleRing,
                            transform.forward);
                    }

                    if (AudioCues.Instance != null)
                    {
                        AudioCues.Instance.PlayNestTell();
                    }
                }
            }
        }

        private void TrySpawnMinion()
        {
            if (_kind != EnemyKind.Swarm || _factory == null || _waves == null)
            {
                return;
            }

            PruneMinions();
            if (_minions.Count >= EnemyCatalog.NestMaxMinions || Time.time < _nextSpawn)
            {
                return;
            }

            _nextSpawn = Time.time + EnemyCatalog.NestSpawnSeconds;
            _nestTold = false;
            float angle = Random.Range(0f, Mathf.PI * 2f);
            Vector3 pos = transform.position + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 1.6f;
            pos.y = 0f;
            if (_target != null)
            {
                pos = SpawnClearance.Place(pos, _target.position, WaveManager.ArenaRadius - 1.5f);
            }

            if (_presence != null)
            {
                _presence.PulseNest();
            }

            _factory.SpawnTelegraphRing(pos, UiTheme.Secondary, 0.55f, TelegraphShape.Ring, transform.forward);
            EnemySeeker minion = _factory.CreateEnemy(pos, _target, _waves, EnemyCatalog.VisualName(EnemyKind.Swarmling));
            if (minion != null)
            {
                _minions.Add(minion);
                _waves.Register(minion);
            }
        }

        private void PruneMinions()
        {
            for (int i = _minions.Count - 1; i >= 0; i--)
            {
                if (_minions[i] == null)
                {
                    _minions.RemoveAt(i);
                }
            }
        }

        private void TryFireBolt(Vector3 toPlayerDir)
        {
            if (!EnemyCatalog.FiresBolts(_kind) || _factory == null || !CanHarm)
            {
                return;
            }

            if (_boltWindupUntil > 0f)
            {
                if (Time.time < _boltWindupUntil)
                {
                    return;
                }

                _boltWindupUntil = 0f;
                FireBolt(_boltAim.sqrMagnitude > 0.01f ? _boltAim : toPlayerDir);
                return;
            }

            if (Time.time < _nextShot)
            {
                return;
            }

            if (Vector3.Dot(transform.forward, toPlayerDir) < 0.62f)
            {
                return;
            }

            float cooldown = EnemyCatalog.FireCooldown(_kind);
            int firePercent = _fireRatePercent < 100 ? 100 : _fireRatePercent;
            cooldown = cooldown * 100f / firePercent;
            bool aimedGun = _kind == EnemyKind.Sniper || _kind == EnemyKind.Gunner;
            if (aimedGun)
            {
                float windup = EnemyCatalog.BoltWindupSeconds;
                if (windup > cooldown)
                {
                    windup = cooldown;
                }

                _boltAim = toPlayerDir;
                _boltWindupUntil = Time.time + windup;
                _nextShot = _boltWindupUntil + (cooldown - windup);
                Vector3 far = transform.position + transform.forward * 18f;
                _factory.SpawnTelegraphLaser(transform.position, far, UiTheme.Danger, 0.35f, windup);
                if (AudioCues.Instance != null)
                {
                    AudioCues.Instance.PlayGunnerTell();
                }

                return;
            }

            _nextShot = Time.time + cooldown;
            FireBolt(transform.forward);
        }

        private void FireBolt(Vector3 direction)
        {
            int boltDamage = 1;
            if (_waves != null)
            {
                boltDamage = DifficultyCurve.ScaleOutgoingDamage(1, _waves.ActiveWave);
            }

            Vector3 aim = direction.sqrMagnitude > 0.01f ? direction.normalized : transform.forward;
            Vector3 origin = transform.position + aim * 1.15f;
            Projectile shot = _factory.SpawnEnemyProjectile(origin, aim, EnemyCatalog.BoltSpeed(_kind), boltDamage, _kind);
            NoteShot(shot);
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayEnemyShoot();
            }
        }

        private void NoteShot(Projectile shot)
        {
            if (shot == null)
            {
                return;
            }

            int waveNumber = _waves != null ? _waves.ActiveWave : 1;
            string eliteName = _elite ? WaveModifier.NameForWave(waveNumber) : string.Empty;
            shot.NoteHostileSource(_boss, _elite, eliteName);
        }

        private void TickBoss(Vector3 toPlayerDir)
        {
            if (_factory == null || !CanHarm)
            {
                return;
            }

            float now = Time.time;
            if (_radialAt > 0f)
            {
                if (now < _radialAt)
                {
                    return;
                }

                FireRadialRing();
                _radialAt = 0f;
                _bossPattern = 0;
                _bossNext = now + BossRules.RadialGapSeconds;
                return;
            }

            if (_aimedWinding)
            {
                float wind = BossRules.AimedWindupSeconds;
                float elapsed = wind <= 0.0001f ? 1f : 1f - ((_aimedFireAt - now) / wind);
                if (elapsed < 0f)
                {
                    elapsed = 0f;
                }

                if (elapsed > 1f)
                {
                    elapsed = 1f;
                }

                float pulse = EffectScale.ScalePulse(
                    SettingsState.ReduceEffectsEnabled,
                    1f + 0.12f * Mathf.Sin(elapsed * Mathf.PI));
                transform.localScale = Vector3.one * (_bossBaseScale * pulse);
                if (now < _aimedFireAt)
                {
                    return;
                }

                _aimedWinding = false;
                transform.localScale = Vector3.one * _bossBaseScale;
                FireAimedBurst(_aimedDir);
                _bossPattern = 1;
                _bossNext = now + BossRules.AimedGapSeconds;
                return;
            }

            if (now < _bossNext)
            {
                return;
            }

            if (_bossPattern == 0)
            {
                Vector3 locked = toPlayerDir;
                if (locked.sqrMagnitude < 0.01f)
                {
                    locked = transform.forward;
                }

                _aimedDir = locked.normalized;
                _aimedFireAt = now + BossRules.AimedWindupSeconds;
                _aimedWinding = true;
                Vector3 windOrigin = transform.position + _aimedDir * 2.4f;
                _factory.SpawnTelegraphRing(
                    windOrigin,
                    UiTheme.Focus,
                    BossRules.AimedWindupSeconds,
                    TelegraphShape.Wedge,
                    _aimedDir);
                if (AudioCues.Instance != null)
                {
                    AudioCues.Instance.PlayBossAimedTell();
                }

                return;
            }

            Vector3 telegraphAt = transform.position;
            _factory.SpawnTelegraphRing(
                telegraphAt,
                UiTheme.Danger,
                BossRules.TelegraphSeconds,
                TelegraphShape.Spokes,
                transform.forward);
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayBossRadialTell();
            }

            _radialAt = now + BossRules.TelegraphSeconds;
        }

        private void FireAimedBurst(Vector3 toPlayerDir)
        {
            int burstDamage = BossBoltDamage();
            float burstSpeed = EnemyCatalog.BoltSpeed(EnemyKind.Gunner);
            for (int shot = 0; shot < BossRules.AimedBurstCount; shot++)
            {
                float yaw = (shot - 1) * 14f;
                Vector3 shotDir = Quaternion.Euler(0f, yaw, 0f) * toPlayerDir;
                if (shotDir.sqrMagnitude < 0.01f)
                {
                    shotDir = transform.forward;
                }

                Vector3 burstOrigin = transform.position + shotDir.normalized * 1.8f;
                Projectile burstShot = _factory.SpawnEnemyProjectile(burstOrigin, shotDir.normalized, burstSpeed, burstDamage, _kind);
                NoteShot(burstShot);
            }

            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayEnemyShoot();
            }
        }

        private void FireRadialRing()
        {
            int ringDamage = BossBoltDamage();
            float ringSpeed = EnemyCatalog.BoltSpeed(EnemyKind.Gunner) * 0.85f;
            int ringCount = BossRules.RadialCount;
            for (int spoke = 0; spoke < ringCount; spoke++)
            {
                float spokeAngle = (Mathf.PI * 2f * spoke) / ringCount;
                Vector3 spokeDir = new Vector3(Mathf.Cos(spokeAngle), 0f, Mathf.Sin(spokeAngle));
                Vector3 ringOrigin = transform.position + spokeDir * 2.2f;
                Projectile ringShot = _factory.SpawnEnemyProjectile(ringOrigin, spokeDir, ringSpeed, ringDamage, _kind);
                NoteShot(ringShot);
            }

            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayEnemyShoot();
            }
        }

        private int BossBoltDamage()
        {
            int waveNumber = _waves != null ? _waves.ActiveWave : 1;
            return DifficultyCurve.ScaleOutgoingDamage(BossRules.BoltDamage, waveNumber);
        }
    }
}
