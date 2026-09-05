using System.Collections.Generic;
using UnityEngine;

namespace AsteroidsGoneRogue
{
    public sealed class WaveManager : MonoBehaviour
    {
        public const float ArenaRadius = 22f;
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

            int largeCount = Mathf.Clamp(BaseLargeAsteroids + (waveIndex - 1), BaseLargeAsteroids, MaxLargeAsteroids);
            for (int i = 0; i < largeCount; i++)
            {
                float angle = (Mathf.PI * 2f * i) / largeCount + 0.35f;
                Vector3 pos = RingPoint(angle, 14f + (i % 2) * 2.5f);
                Register(_factory.CreateLargeAsteroid(pos, this));
            }

            Vector3 enemyPos = RingPoint(waveIndex * 0.7f, 16.5f);
            Register(_factory.CreateEnemy(enemyPos, _player, this));
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
        }

        public static Vector3 RingPoint(float angle, float radius)
        {
            return new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
        }
    }
}
