using System.Collections.Generic;
using UnityEngine;

namespace AsteroidsGoneRogue
{
    public sealed class WaveManager : MonoBehaviour
    {
        public const float ArenaRadius = 30f;
        public const float ArenaDesignRadius = 22f;
        public const int LadderWaves = 8;
        private const int BaseLargeAsteroids = DifficultyCurve.BaseLargeAsteroids;
        private const int MaxLargeAsteroids = DifficultyCurve.EarlyAsteroidCap;
        private const int PlateauWave = DifficultyCurve.PlateauWave;
        private const int PlateauAsteroidCap = DifficultyCurve.PlateauAsteroidCap;

        private EnemySeeker _boss;
        private WaveModifierKind _modifier = WaveModifierKind.None;

        public WaveModifierKind Modifier
        {
            get { return _modifier; }
        }

        public bool HasBoss
        {
            get { return _boss != null && _boss.IsBoss; }
        }

        public int BossHp
        {
            get { return HasBoss ? _boss.CurrentHp : 0; }
        }

        public int BossMaxHp
        {
            get { return HasBoss ? _boss.MaxHp : 1; }
        }

        private readonly HashSet<IThreat> _live = new HashSet<IThreat>();
        private readonly Dictionary<IThreat, float> _outsideSeconds = new Dictionary<IThreat, float>();
        private ContentFactory _factory;
        private GameManager _game;
        private Transform _player;
        private float _allStrandedSeconds;
        private int _activeWave = 1;

        public int ActiveWave
        {
            get { return _activeWave < 1 ? 1 : _activeWave; }
        }

        public int RemainingThreats
        {
            get { return _live.Count; }
        }

        public void Initialize(ContentFactory factory, GameManager game, Transform player)
        {
            _factory = factory;
            _game = game;
            _player = player;
        }

        public void SpawnWave(int waveIndex)
        {
            _activeWave = waveIndex < 1 ? 1 : waveIndex;
            _boss = null;
            _modifier = WaveModifier.ForWave(_activeWave);
            DespawnAll();
            _allStrandedSeconds = 0f;
            _factory.ApplyArenaForWave(waveIndex);
            RunRng.BindWave(RunRng.DailySeed, _activeWave);
            float ringPhase = (float)SpawnLayout.PhaseRadians(RunRng.DailySeed, _activeWave);

            int debrisBonus = WorldRules.ExtraAsteroids(waveIndex);
            int largeCount = Mathf.Clamp(
                LargeAsteroidCount(waveIndex) + DifficultySettings.ExtraAsteroids + debrisBonus,
                1,
                PlateauAsteroidCap);
            for (int rock = 0; rock < largeCount; rock++)
            {
                float rockAngle = (Mathf.PI * 2f * rock) / largeCount + 0.35f + ringPhase;
                Vector3 rockPos = ClearSpawn(RingPoint(rockAngle, ScaledRing(14f + (rock % 2) * 2.5f)));
                Register(_factory.CreateLargeAsteroid(rockPos, this));
            }

            bool bossWave = BossRules.IsBossWave(_activeWave);
            bool eliteWave = WaveModifier.IsElite(_activeWave);
            int curveExtras = DifficultyCurve.ForWave(_activeWave).ExtraEnemies;
            int gradeExtras = DifficultySettings.ExtraEnemyCount;
            int extras = curveExtras + gradeExtras;
            int reserved = (bossWave ? 1 : 0) + (eliteWave ? 1 : 0);
            int budget = DifficultyCurve.MaxSpawnedEnemies - reserved;
            if (budget < 1)
            {
                budget = 1;
            }

            int spawned = 0;
            EnemyKind[] roster = WaveRoster.FitEliteBossRoster(RosterForWave(waveIndex), waveIndex, extras);
            for (int rosterIndex = 0; rosterIndex < roster.Length; rosterIndex++)
            {
                if (spawned >= budget)
                {
                    break;
                }

                if (!WaveRoster.IncludeInSpawn(roster[rosterIndex], waveIndex))
                {
                    continue;
                }

                if (!CanSpawn(roster[rosterIndex]))
                {
                    continue;
                }

                float rosterAngle = waveIndex * 0.55f
                    + (Mathf.PI * 2f * spawned) / Mathf.Max(1, roster.Length)
                    + 1.1f
                    + ringPhase;
                Vector3 rosterPos = ClearSpawn(RingPoint(rosterAngle, ScaledRing(16.5f - (spawned % 2) * 1.4f)));
                Register(_factory.CreateEnemy(rosterPos, _player, this, EnemyCatalog.VisualName(roster[rosterIndex])));
                spawned++;
            }

            for (int extraIndex = 0; extraIndex < extras; extraIndex++)
            {
                if (spawned >= budget)
                {
                    break;
                }

                float extraAngle = waveIndex * 0.31f + 2.4f + extraIndex * 0.9f + ringPhase;
                Vector3 extraPos = ClearSpawn(RingPoint(extraAngle, ScaledRing(15.2f)));
                Register(_factory.CreateEnemy(extraPos, _player, this, EnemyCatalog.VisualName(EnemyKind.Mid01)));
                spawned++;
            }

            if (eliteWave)
            {
                SpawnEliteBrute(waveIndex);
            }

            if (bossWave)
            {
                SpawnBoss(waveIndex);
            }

            SpawnWavePickup(waveIndex);
            SpawnExtraPickup(waveIndex);
            if (eliteWave)
            {
                SpawnElitePickup(waveIndex);
            }
        }

        public void SpawnTutorial()
        {
            _activeWave = 1;
            _boss = null;
            _modifier = WaveModifierKind.None;
            DespawnAll();
            _allStrandedSeconds = 0f;
            _factory.ApplyArenaForWave(1);
            int tutorCount = FirstRunRules.AsteroidCount;
            for (int tutorRock = 0; tutorRock < tutorCount; tutorRock++)
            {
                float tutorAngle = (Mathf.PI * 2f * tutorRock) / tutorCount + 0.4f;
                Vector3 tutorPos = RingPoint(tutorAngle, ScaledRing(16f));
                Vector3 tutorDrift = new Vector3(
                    Mathf.Cos(tutorAngle + 1.2f),
                    0f,
                    Mathf.Sin(tutorAngle + 1.2f));
                tutorDrift *= FirstRunRules.AsteroidSpeed;
                Register(_factory.CreateSmallAsteroid(tutorPos, tutorDrift, this));
            }

            Vector3 tutorEnemyPos = RingPoint(2.2f, ScaledRing(18f));
            EnemySeeker tutorEnemy = _factory.CreateEnemy(
                tutorEnemyPos,
                _player,
                this,
                EnemyCatalog.VisualName(FirstRunRules.WeakEnemy));
            if (tutorEnemy != null)
            {
                tutorEnemy.ConfigureTraining(FirstRunRules.WeakEnemyHp, FirstRunRules.WeakEnemySpeedScale);
                Register(tutorEnemy);
            }

            Vector3 tutorPickupPos = RingPoint(0.6f, ScaledRing(8f));
            _factory.CreatePickup("Pickup_Shield", tutorPickupPos);
        }

        private void SpawnEliteBrute(int waveIndex)
        {
            float eliteAngle = waveIndex * 0.2f + Mathf.PI + (float)SpawnLayout.PhaseRadians(RunRng.DailySeed, waveIndex);
            Vector3 elitePos = ClearSpawn(RingPoint(eliteAngle, ScaledRing(18f)));
            EnemySeeker elite = _factory.CreateEnemy(
                elitePos,
                _player,
                this,
                EnemyCatalog.VisualName(EnemyKind.Brute));
            if (elite == null)
            {
                return;
            }

            int eliteHp = DifficultyCurve.ScaleHp(
                EnemyCatalog.HitPoints(EnemyKind.Brute),
                waveIndex,
                DifficultySettings.Current);
            eliteHp = eliteHp * WaveModifier.EliteHpPercent / 100;
            if (eliteHp < 1)
            {
                eliteHp = 1;
            }

            elite.ConfigureElite(eliteHp);
            _factory.MarkElite(elite.transform);
            Register(elite);
        }

        private void SpawnBoss(int waveIndex)
        {
            float bossAngle = waveIndex * 0.2f + 0.4f + (float)SpawnLayout.PhaseRadians(RunRng.DailySeed, waveIndex);
            Vector3 bossPos = ClearSpawn(RingPoint(bossAngle, ScaledRing(12f)));
            EnemySeeker boss = _factory.CreateEnemy(
                bossPos,
                _player,
                this,
                EnemyCatalog.VisualName(EnemyKind.Brute));
            if (boss == null)
            {
                return;
            }

            boss.gameObject.name = "WorldGuardian";
            boss.transform.localScale = Vector3.one * BossRules.VisualScale;
            boss.ConfigureBoss(BossRules.HitPoints(waveIndex, DifficultySettings.Current));
            _boss = boss;
            Register(boss);
        }

        private void SpawnElitePickup(int waveIndex)
        {
            string[] eliteKinds = { "Pickup_Shield", "Pickup_Health", "Pickup_RapidFire" };
            string eliteVisual = eliteKinds[(waveIndex / WaveModifier.EliteStride) % eliteKinds.Length];
            Vector3 elitePickupPos = RingPoint(waveIndex * 0.7f + 0.2f + (float)SpawnLayout.PhaseRadians(RunRng.DailySeed, waveIndex), ScaledRing(5.5f));
            _factory.CreatePickup(eliteVisual, elitePickupPos);
        }

        private static bool CanSpawn(EnemyKind kind)
        {
            if (!EnemyCatalog.RequiresImportedMesh(kind))
            {
                return true;
            }

            return ArtImport.LoadPrefab(EnemyCatalog.VisualName(kind)) != null;
        }

        private void SpawnExtraPickup(int waveIndex)
        {
            if (WorldRules.PickupDelta(waveIndex) <= 0 || waveIndex < 2)
            {
                return;
            }

            string[] extraKinds = { "Pickup_Shield", "Pickup_Health", "Pickup_Score" };
            string extraVisual = extraKinds[waveIndex % extraKinds.Length];
            Vector3 extraPos = RingPoint(waveIndex * 0.8f + 2.1f + (float)SpawnLayout.PhaseRadians(RunRng.DailySeed, waveIndex), ScaledRing(6.5f));
            _factory.CreatePickup(extraVisual, extraPos);
        }

        private void SpawnWavePickup(int waveIndex)
        {
            if (waveIndex < 2)
            {
                return;
            }

            if (WorldRules.PickupDelta(waveIndex) < 0)
            {
                return;
            }

            string[] kinds = { "Pickup_Score", "Pickup_Shield", "Pickup_Health", "Pickup_RapidFire" };
            string visual = kinds[(waveIndex - 2) % kinds.Length];
            Vector3 pos = RingPoint(waveIndex * 1.3f + 0.4f + (float)SpawnLayout.PhaseRadians(RunRng.DailySeed, waveIndex), ScaledRing(8.5f));
            _factory.CreatePickup(visual, pos);
        }

        public static int LargeAsteroidCount(int waveIndex)
        {
            int count = DifficultyCurve.AsteroidCount(waveIndex);
            if (count < BaseLargeAsteroids)
            {
                count = BaseLargeAsteroids;
            }

            if (waveIndex > PlateauWave && count > PlateauAsteroidCap)
            {
                count = PlateauAsteroidCap;
            }

            if (count > MaxLargeAsteroids && waveIndex <= PlateauWave)
            {
                count = MaxLargeAsteroids;
            }

            return count;
        }

        public static EnemyKind[] RosterForWave(int waveIndex)
        {
            int rung = Mathf.Clamp(waveIndex, 1, 10);
            return WaveRoster.Extend(BaseRoster(rung), waveIndex);
        }

        private static EnemyKind[] BaseRoster(int rung)
        {
            switch (rung)
            {
                case 1:
                    return new[] { EnemyKind.Mid01, EnemyKind.Mid01 };
                case 2:
                    return new[] { EnemyKind.Scout, EnemyKind.Mid01 };
                case 3:
                    return new[] { EnemyKind.Mid01, EnemyKind.Scout, EnemyKind.Drone };
                case 4:
                    return new[] { EnemyKind.Gunner };
                case 5:
                    return new[] { EnemyKind.Scout, EnemyKind.Drone, EnemyKind.Brute };
                case 6:
                    return new[] { EnemyKind.Gunner, EnemyKind.Scout, EnemyKind.Swarm };
                case 7:
                    return new[] { EnemyKind.Gunner, EnemyKind.Drone, EnemyKind.Scout, EnemyKind.Bomber };
                case 8:
                    return new[] { EnemyKind.Gunner, EnemyKind.Scout, EnemyKind.Drone, EnemyKind.Sniper, EnemyKind.Brute, EnemyKind.Swarm };
                case 9:
                    return new[] { EnemyKind.Gunner, EnemyKind.Mid01, EnemyKind.Drone, EnemyKind.SwarmPod, EnemyKind.Swarm };
                default:
                    return new[]
                    {
                        EnemyKind.Gunner, EnemyKind.Bomber, EnemyKind.Sniper, EnemyKind.SwarmPod,
                        EnemyKind.Scout, EnemyKind.Brute, EnemyKind.Swarm
                    };
            }
        }

        public void Register(IThreat threat)
        {
            if (threat != null)
            {
                _live.Add(threat);
            }
        }

        public void NotifyDestroyed(IThreat threat, int scoreValue)
        {
            if (threat == null)
            {
                return;
            }

            _live.Remove(threat);
            if (_game != null)
            {
                _game.NotifyThreatDestroyed(scoreValue);
            }
        }

        public void DespawnAll()
        {
            _boss = null;
            var snapshot = new List<IThreat>(_live);
            _live.Clear();
            _outsideSeconds.Clear();
            _allStrandedSeconds = 0f;
            for (int i = 0; i < snapshot.Count; i++)
            {
                snapshot[i].Despawn();
            }

            _factory.ClearProjectiles();
            _factory.ClearPickups();
        }

        private void Update()
        {
            RescueStrandedThreats();
        }

        private void RescueStrandedThreats()
        {
            if (_game != null && _game.Session != null && _game.Session.Phase != GamePhase.Playing)
            {
                _allStrandedSeconds = 0f;
                if (_game != null)
                {
                    _game.SetSoftLockHint(false);
                }

                return;
            }

            if (_live.Count == 0)
            {
                _allStrandedSeconds = 0f;
                if (_game != null)
                {
                    _game.SetSoftLockHint(false);
                }

                return;
            }

            bool removed = false;
            bool anyStranded = false;
            int live = 0;
            int stranded = 0;
            var snapshot = new List<IThreat>(_live);
            for (int i = 0; i < snapshot.Count; i++)
            {
                IThreat threat = snapshot[i];
                Component component = threat as Component;
                if (component == null)
                {
                    _live.Remove(threat);
                    _outsideSeconds.Remove(threat);
                    removed = true;
                    continue;
                }

                live++;
                Vector3 pos = component.transform.position;
                if (ArenaWrap.IsInvalid(pos.x, pos.y, pos.z))
                {
                    _live.Remove(threat);
                    _outsideSeconds.Remove(threat);
                    threat.Despawn();
                    removed = true;
                    continue;
                }

                if (ArenaWrap.IsOutOfPlayY(pos.y) || ArenaWrap.IsBeyondSoftLock(pos.x, pos.z, ArenaRadius))
                {
                    ForceWrapOrDespawn(threat, component);
                    pos = component.transform.position;
                }

                bool outOfPlay = ArenaWrap.IsOutOfPlay(pos.x, pos.y, pos.z, ArenaRadius);
                if (!outOfPlay)
                {
                    _outsideSeconds.Remove(threat);
                    continue;
                }

                anyStranded = true;
                stranded++;
                float elapsed;
                _outsideSeconds.TryGetValue(threat, out elapsed);
                elapsed += Time.deltaTime;
                _outsideSeconds[threat] = elapsed;
                if (elapsed <= ArenaWrap.SoftLockSeconds)
                {
                    continue;
                }

                ForceWrapOrDespawn(threat, component);
                pos = component.transform.position;
                if (!ArenaWrap.IsOutOfPlay(pos.x, pos.y, pos.z, ArenaRadius))
                {
                    _outsideSeconds.Remove(threat);
                    continue;
                }

                if (ArenaWrap.IsInvalid(pos.x, pos.y, pos.z))
                {
                    _live.Remove(threat);
                    _outsideSeconds.Remove(threat);
                    threat.Despawn();
                    removed = true;
                }
            }

            if (_game != null)
            {
                _game.SetSoftLockHint(anyStranded);
            }

            if (live > 0 && stranded == live)
            {
                _allStrandedSeconds += Time.deltaTime;
                if (_allStrandedSeconds > ArenaWrap.SoftLockSeconds && _game != null)
                {
                    _allStrandedSeconds = 0f;
                    _game.NotifySoftLockAbort();
                    return;
                }
            }
            else
            {
                _allStrandedSeconds = 0f;
            }

            if (removed && _live.Count == 0 && _game != null)
            {
                _game.NotifySoftLockAbort();
            }
        }

        private void ForceWrapOrDespawn(IThreat threat, Component component)
        {
            Vector3 pos = component.transform.position;
            float ox;
            float oz;
            ArenaWrap.WrapXz(pos.x, pos.z, ArenaRadius, out ox, out oz);
            Vector3 wrapped = new Vector3(ox, ArenaWrap.PlayY, oz);

            Asteroid asteroid = component.GetComponent<Asteroid>();
            if (asteroid != null)
            {
                asteroid.KeepInPlay();
                pos = component.transform.position;
                if (!ArenaWrap.IsOutOfPlay(pos.x, pos.y, pos.z, ArenaRadius))
                {
                    return;
                }
            }

            Rigidbody body = component.GetComponent<Rigidbody>();
            if (body != null)
            {
                body.position = wrapped;
                Vector3 vel = body.linearVelocity;
                vel.y = 0f;
                body.linearVelocity = vel;
            }

            component.transform.position = wrapped;
        }

        private Vector3 ClearSpawn(Vector3 spawn)
        {
            if (_player == null)
            {
                return spawn;
            }

            return SpawnClearance.Place(spawn, _player.position, ArenaRadius - 1.5f);
        }

        public static float ScaledRing(float designRadius)
        {
            return designRadius * (ArenaRadius / ArenaDesignRadius);
        }

        public static Vector3 RingPoint(float angle, float radius)
        {
            return new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
        }
    }
}
