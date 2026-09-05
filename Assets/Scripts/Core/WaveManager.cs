using System.Collections.Generic;
using UnityEngine;

namespace AsteroidsGoneRogue
{
    public sealed class WaveManager : MonoBehaviour
    {
        public const float ArenaRadius = 22f;
        public const int LadderWaves = 8;
        private const int BaseLargeAsteroids = 4;
        private const int MaxLargeAsteroids = 7;

        private readonly HashSet<IThreat> _live = new HashSet<IThreat>();
        private ContentFactory _factory;
        private GameManager _game;
        private Transform _player;

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
            DespawnAll();

            int largeCount = LargeAsteroidCount(waveIndex);
            for (int i = 0; i < largeCount; i++)
            {
                float angle = (Mathf.PI * 2f * i) / largeCount + 0.35f;
                Vector3 pos = RingPoint(angle, 14f + (i % 2) * 2.5f);
                Register(_factory.CreateLargeAsteroid(pos, this));
            }

            EnemyKind[] roster = RosterForWave(waveIndex);
            int spawned = 0;
            for (int i = 0; i < roster.Length; i++)
            {
                if (!CanSpawn(roster[i]))
                {
                    continue;
                }

                float angle = waveIndex * 0.55f + (Mathf.PI * 2f * spawned) / Mathf.Max(1, roster.Length) + 1.1f;
                Vector3 pos = RingPoint(angle, 16.5f - (spawned % 2) * 1.4f);
                Register(_factory.CreateEnemy(pos, _player, this, EnemyCatalog.VisualName(roster[i])));
                spawned++;
            }

            SpawnWavePickup(waveIndex);
        }

        private static bool CanSpawn(EnemyKind kind)
        {
            if (!EnemyCatalog.RequiresImportedMesh(kind))
            {
                return true;
            }

            return ArtImport.LoadPrefab(EnemyCatalog.VisualName(kind)) != null;
        }

        private void SpawnWavePickup(int waveIndex)
        {
            if (waveIndex < 2)
            {
                return;
            }

            string[] kinds = { "Pickup_Score", "Pickup_Shield", "Pickup_Health", "Pickup_RapidFire" };
            string visual = kinds[(waveIndex - 2) % kinds.Length];
            Vector3 pos = RingPoint(waveIndex * 1.3f + 0.4f, 8.5f);
            _factory.CreatePickup(visual, pos);
        }

        public static int LargeAsteroidCount(int waveIndex)
        {
            return Mathf.Clamp(BaseLargeAsteroids + (waveIndex - 1), BaseLargeAsteroids, MaxLargeAsteroids);
        }

        public static EnemyKind[] RosterForWave(int waveIndex)
        {
            int rung = Mathf.Clamp(waveIndex, 1, 10);
            switch (rung)
            {
                case 1:
                    return new[] { EnemyKind.Mid01 };
                case 2:
                    return new[] { EnemyKind.Scout };
                case 3:
                    return new[] { EnemyKind.Mid01, EnemyKind.Scout };
                case 4:
                    return new[] { EnemyKind.Gunner };
                case 5:
                    return new[] { EnemyKind.Scout, EnemyKind.Drone };
                case 6:
                    return new[] { EnemyKind.Gunner, EnemyKind.Scout };
                case 7:
                    return new[] { EnemyKind.Gunner, EnemyKind.Drone, EnemyKind.Scout, EnemyKind.Bomber };
                case 8:
                    return new[] { EnemyKind.Gunner, EnemyKind.Scout, EnemyKind.Drone, EnemyKind.Sniper };
                case 9:
                    return new[] { EnemyKind.Gunner, EnemyKind.Mid01, EnemyKind.Drone, EnemyKind.SwarmPod };
                default:
                    return new[] { EnemyKind.Gunner, EnemyKind.Bomber, EnemyKind.Sniper, EnemyKind.SwarmPod, EnemyKind.Scout };
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
            var snapshot = new List<IThreat>(_live);
            _live.Clear();
            for (int i = 0; i < snapshot.Count; i++)
            {
                snapshot[i].Despawn();
            }

            _factory.ClearProjectiles();
            _factory.ClearPickups();
        }

        public static Vector3 RingPoint(float angle, float radius)
        {
            return new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
        }
    }
}
