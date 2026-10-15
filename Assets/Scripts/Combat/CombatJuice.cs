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
        public const float BossPhaseShake = 0.08f;

        private static Vector3 _hitDir;

        public static void NoteHitDirection(Vector3 direction)
        {
            if (direction.sqrMagnitude > 0.0001f)
            {
                _hitDir = direction;
            }
        }

        public static void PlayerDamaged(bool lethal)
        {
            PlayerHullHit();
            if (lethal)
            {
                FlashScreen(HeavyKillFlash, UiTheme.Primary, HitFlashLimiter.DefaultDecay, true);
                return;
            }
        }

        public static void PlayerHullHit()
        {
            Vector3 direction = ConsumeHitDirection();
            Shake(PlayerHitShake, direction);
            RequestStop(HitstopBudget.HullSeconds);
            FlashScreen(PlayerHitFlash, UiTheme.Danger, HitFlashLimiter.DefaultDecay, false);
            if (GameUi.Instance != null)
            {
                GameUi.Instance.PulseHullEdge();
            }
        }

        public static void PlayerShieldHit(Transform ship)
        {
            _hitDir = Vector3.zero;
            Shake(ShieldHitShake);
            if (ship != null)
            {
                MeshHitFlash.Play(ship, UiTheme.Secondary, 0.09f);
                Ripple(ship.position, UiTheme.Secondary, 0.14f);
            }
        }

        public static void PlayerShieldBreak(Transform ship)
        {
            _hitDir = Vector3.zero;
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
            ShakeWorld(shake, position);
            RequestStop(HitstopBudget.HeavySeconds);
            FlashScreen(HeavyKillFlash, UiTheme.Primary, HitFlashLimiter.DefaultDecay, true);
        }

        public static void BossPhase(Vector3 origin, Vector3 direction)
        {
            if (direction.sqrMagnitude > 0.0001f)
            {
                Shake(BossPhaseShake, direction);
            }
            else
            {
                ShakeWorld(BossPhaseShake, origin);
            }

            RequestStop(HitstopBudget.BossSeconds);
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

            if (target != null)
            {
                ShakeWorld(exploded ? ExplosionShake : ThreatHitShake, target.position);
            }
            else
            {
                Shake(exploded ? ExplosionShake : ThreatHitShake);
            }

            if (exploded && heavy)
            {
                RequestStop(HitstopBudget.HeavySeconds);
            }

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
            Shake(amplitude, Vector3.zero);
        }

        public static void Shake(float amplitude, Vector3 direction)
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                return;
            }

            FollowCamera follow = camera.GetComponent<FollowCamera>();
            if (follow != null)
            {
                follow.AddShake(amplitude, direction);
            }
        }

        public static void ShakeWorld(float amplitude, Vector3 worldPosition)
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                return;
            }

            FollowCamera follow = camera.GetComponent<FollowCamera>();
            if (follow != null)
            {
                follow.AddShakeWorld(amplitude, worldPosition);
            }
        }

        private static Vector3 ConsumeHitDirection()
        {
            Vector3 direction = _hitDir;
            _hitDir = Vector3.zero;
            return direction;
        }

        private static void RequestStop(float seconds)
        {
            GameManager live = GameManager.Live;
            if (live == null)
            {
                return;
            }

            live.RequestHitstop(seconds);
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
