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
            if (!SettingsState.ReduceEffectsEnabled)
            {
                Shake(PlayerHitShake, direction);
            }

            RequestStop(HitstopBudget.HullSeconds);
            if (GameUi.Instance != null)
            {
                GameUi.Instance.ShowHitVignette(direction, false);
                GameUi.Instance.PulseHullEdge();
            }
        }

        public static void PlayerShieldHit(Transform ship)
        {
            Vector3 direction = ConsumeHitDirection();
            if (!SettingsState.ReduceEffectsEnabled)
            {
                Shake(ShieldHitShake, direction);
            }

            if (GameUi.Instance != null)
            {
                GameUi.Instance.ShowHitVignette(direction, true);
            }
            if (ship != null)
            {
                MeshHitFlash.Play(ship, UiTheme.Secondary, 0.09f);
                Ripple(ship.position, UiTheme.Secondary, 0.14f);
                bool reduce = SettingsState.ReduceEffectsEnabled;
                VfxKit.Shock(ship.position, 1f, 1.6f, 0.25f, reduce);
                VfxKit.Sparks(ship.position, 4, UiTheme.Secondary, reduce);
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
                bool reduce = SettingsState.ReduceEffectsEnabled;
                VfxKit.Shock(ship.position, 1f, 1.6f, 0.25f, reduce);
                VfxKit.Shock(ship.position, 1f, 1.9f, 0.25f, reduce);
                VfxKit.Sparks(ship.position, 10, UiTheme.Secondary, reduce);
            }
        }

        public static void HeavyKill(Vector3 position, float shake)
        {
            ProjectileLightPool.Pulse(position, ProjectileLightBudget.KindKill, EmissionTargets.Danger, null);
            JuiceBurst.KillBloom(position, true);
            VfxKit.Explode(position, VfxBudget.Large, SettingsState.ReduceEffectsEnabled);
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

            bool reduce = SettingsState.ReduceEffectsEnabled;
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
                VfxKit.Sparks(
                    target.position,
                    4,
                    new Color(LookPolish.CoreR, LookPolish.CoreG, LookPolish.CoreB, 1f),
                    reduce);
            }

            int tier = ExplosionSize(target);
            if (target != null && exploded)
            {
                JuiceBurst.KillBloom(target.position, heavy);
                VfxKit.Explode(target.position, tier, reduce);
            }

            if (target != null && !(exploded && reduce))
            {
                float amp = exploded ? ExplosionShake : ThreatHitShake;
                if (exploded && tier == LookPolish.TierLarge)
                {
                    amp = LookPolish.LargeShake;
                }

                ShakeWorld(amp, target.position);
            }
            else if (!(exploded && reduce))
            {
                Shake(exploded ? ExplosionShake : ThreatHitShake);
            }

            if (exploded && tier == LookPolish.TierLarge)
            {
                if (EffectScale.AllowsHitstop(reduce))
                {
                    RequestStop(LookPolish.LargeHitstop);
                }
            }
            else if (exploded && heavy)
            {
                if (EffectScale.AllowsHitstop(reduce))
                {
                    RequestStop(HitstopBudget.HeavySeconds);
                }
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

        private static int ExplosionSize(Transform target)
        {
            if (target == null)
            {
                return VfxBudget.Mid;
            }

            EnemySeeker seeker = target.GetComponent<EnemySeeker>();
            if (seeker != null)
            {
                return LookPolish.TierFor((int)seeker.Kind, seeker.IsBoss);
            }

            Asteroid asteroid = target.GetComponent<Asteroid>();
            if (asteroid != null && asteroid.Size == AsteroidSize.Small)
            {
                return VfxBudget.Small;
            }

            if (asteroid != null)
            {
                return VfxBudget.Mid;
            }

            return VfxBudget.Mid;
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
