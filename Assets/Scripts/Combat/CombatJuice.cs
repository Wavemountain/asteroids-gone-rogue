using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Hit flash + camera shake + kill/heart blooms. Player death stays quiet (no extra burst).
    /// </summary>
    public static class CombatJuice
    {
        public const float PlayerHitShake = 0.14f;
        public const float ShieldHitShake = 0.07f;
        public const float ThreatHitShake = 0.05f;
        public const float ExplosionShake = 0.22f;
        public const float PlayerHitFlash = 0.2f;
        public const float ShieldBreakFlash = 0.16f;
        public const float HeavyKillFlash = 0.12f;
        public const float ExtraLifeFlash = 0.20f;
        public const float ExtraLifeShake = 0.12f;

        public static void PlayerDamaged(bool lethal)
        {
            if (lethal)
            {
                return;
            }

            PlayerHullHit();
        }

        public static void PlayerHullHit()
        {
            Shake(PlayerHitShake);
            FlashScreen(PlayerHitFlash, UiTheme.Danger, HitFlashLimiter.DefaultDecay, false);
        }

        public static void PlayerShieldHit(Transform ship)
        {
            Shake(ShieldHitShake);
            if (ship != null)
            {
                MeshHitFlash.Play(ship, UiTheme.Secondary, 0.09f);
                Ripple(ship.position, UiTheme.Secondary, 0.14f);
            }
        }

        public static void PlayerShieldBreak(Transform ship)
        {
            Shake(PlayerHitShake);
            FlashScreen(ShieldBreakFlash, UiTheme.Secondary, 0.14f, false);
            if (ship != null)
            {
                Ripple(ship.position, UiTheme.Secondary, 0.25f);
            }
        }

        public static void HeavyKill(Vector3 position, float shake)
        {
            JuiceBurst.KillBloom(position, true);
            Shake(shake);
            FlashScreen(HeavyKillFlash, UiTheme.Primary, HitFlashLimiter.DefaultDecay, true);
        }

        public static void Ripple(Vector3 position, Color color, float seconds)
        {
            GameObject root = new GameObject("ShieldRipple");
            root.transform.position = new Vector3(position.x, 0.04f, position.z);
            TelegraphRing ring = root.AddComponent<TelegraphRing>();
            ring.Play(color, seconds);
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
            bool armor = false;
            bool monster = false;
            if (target != null)
            {
                EnemySeeker seeker = target.GetComponent<EnemySeeker>();
                if (seeker != null)
                {
                    armor = seeker.Kind == EnemyKind.Brute || seeker.IsBoss;
                    monster = AudioCues.UsesMonsterThreatSfx(seeker.Kind) || seeker.IsElite || seeker.IsBoss;
                }
            }

            if (target != null && !exploded)
            {
                if (armor)
                {
                    MeshHitFlash.Play(target, UiTheme.Accent, 0.07f);
                    if (AudioCues.Instance != null)
                    {
                        AudioCues.Instance.PlayArmorHit();
                    }
                }
                else
                {
                    MeshHitFlash.Play(target);
                }

                JuiceBurst.HitSpark(target.position);
            }

            if (target != null && exploded)
            {
                JuiceBurst.KillBloom(target.position, heavy);
            }

            Shake(exploded ? ExplosionShake : ThreatHitShake);
            if (exploded && monster)
            {
                FlashScreen(HeavyKillFlash, UiTheme.Primary, HitFlashLimiter.DefaultDecay, true);
            }
        }

        public static void ExtraLifeSpawn(Vector3 position)
        {
            JuiceBurst.HeartBloom(position);
        }

        public static void ExtraLifeTaken(Vector3 position)
        {
            JuiceBurst.HeartBloom(position);
            Shake(ExtraLifeShake);
            FlashScreen(ExtraLifeFlash, new Color(1f, 0.22f, 0.38f), HitFlashLimiter.DefaultDecay, false);
        }

        public static void PickupBloom(Vector3 position, Color color)
        {
            JuiceBurst.ColoredBloom(position, color, 0.42f, 0.2f, 1.15f);
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
            FlashScreen(strength, UiTheme.Danger, HitFlashLimiter.DefaultDecay, false);
        }

        public static void FlashScreen(float strength, Color color, float decay)
        {
            FlashScreen(strength, color, decay, false);
        }

        public static void FlashScreen(float strength, Color color, float decay, bool killFlash)
        {
            if (killFlash && !EffectScale.AllowsKillFlash(SettingsState.ReduceEffectsEnabled))
            {
                return;
            }

            if (GameUi.Instance != null)
            {
                GameUi.Instance.FlashHit(strength, color, decay);
            }
        }
    }
}
