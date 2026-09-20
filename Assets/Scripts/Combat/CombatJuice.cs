using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Hit flash + camera shake + kill/heart blooms. Player death stays quiet (no extra burst).
    /// </summary>
    public static class CombatJuice
    {
        public const float PlayerHitShake = 0.14f;
        public const float ThreatHitShake = 0.07f;
        public const float ExplosionShake = 0.22f;
        public const float PlayerHitFlash = 0.2f;
        public const float ThreatHitFlash = 0.1f;
        public const float ExplosionFlash = 0.16f;
        public const float ExtraLifeFlash = 0.28f;
        public const float ExtraLifeShake = 0.12f;

        public static void PlayerDamaged(bool lethal)
        {
            if (lethal)
            {
                return;
            }

            Shake(PlayerHitShake);
            FlashScreen(PlayerHitFlash);
        }

        public static void ThreatDamaged(Transform target, bool exploded)
        {
            bool heavy = false;
            if (target != null)
            {
                EnemySeeker seeker = target.GetComponent<EnemySeeker>();
                if (seeker != null)
                {
                    heavy = AudioCues.UsesMonsterThreatSfx(seeker.Kind);
                }
                else
                {
                    Asteroid asteroid = target.GetComponent<Asteroid>();
                    if (asteroid != null)
                    {
                        heavy = asteroid.Size == AsteroidSize.Large;
                    }
                }
            }

            ThreatDamaged(target, exploded, heavy);
        }

        public static void ThreatDamaged(Transform target, bool exploded, bool heavy)
        {
            if (target != null && !exploded)
            {
                MeshHitFlash.Play(target);
                JuiceBurst.HitSpark(target.position);
            }

            if (target != null && exploded)
            {
                JuiceBurst.KillBloom(target.position, heavy);
            }

            Shake(exploded ? ExplosionShake : ThreatHitShake);
            FlashScreen(exploded ? ExplosionFlash : ThreatHitFlash);
        }

        public static void ExtraLifeSpawn(Vector3 position)
        {
            JuiceBurst.HeartBloom(position);
        }

        public static void ExtraLifeTaken(Vector3 position)
        {
            JuiceBurst.HeartBloom(position);
            Shake(ExtraLifeShake);
            FlashScreen(ExtraLifeFlash);
        }

        public static void Shake(float amplitude)
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                return;
            }

            FollowCamera follow = camera.GetComponent<FollowCamera>();
            if (follow != null)
            {
                follow.AddShake(amplitude);
            }
        }

        public static void FlashScreen(float strength)
        {
            if (GameUi.Instance != null)
            {
                GameUi.Instance.FlashHit(strength);
            }
        }
    }
}
