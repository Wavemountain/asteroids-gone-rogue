using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Light hit flash + camera shake. Player death stays quiet (no extra burst).
    /// </summary>
    public static class CombatJuice
    {
        public const float PlayerHitShake = 0.14f;
        public const float ThreatHitShake = 0.055f;
        public const float ExplosionShake = 0.18f;
        public const float PlayerHitFlash = 0.2f;
        public const float ThreatHitFlash = 0.08f;
        public const float ExplosionFlash = 0.11f;

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
            if (target != null && !exploded)
            {
                MeshHitFlash.Play(target);
            }

            Shake(exploded ? ExplosionShake : ThreatHitShake);
            FlashScreen(exploded ? ExplosionFlash : ThreatHitFlash);
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
