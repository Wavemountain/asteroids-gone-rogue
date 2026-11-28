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
        public const float ThreatHitShake = FeelScale.ThreatHitShake;
        public const float ExplosionShake = 0.22f;
        public const float PlayerHitFlash = 0.2f;
        public const float ShieldBreakFlash = 0.16f;
        public const float HeavyKillFlash = 0.12f;
        public const float ExtraLifeFlash = 0.20f;
        public const float ExtraLifeShake = 0.12f;
        public const float BossPhaseShake = FeelScale.BossPhaseShake;

        private static Vector3 _hitDir;
        private static float _lastHitShake;

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
            Shake(FeelScale.ShieldBreakShake);
            RequestStop(HitstopBudget.ShieldBreakSeconds);
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
            HeavyKill(position, shake, true);
        }

        public static void HeavyKill(Vector3 position, float shake, bool flash)
        {
            ProjectileLightPool.Pulse(position, ProjectileLightBudget.KindKill, EmissionTargets.Danger, null);
            JuiceBurst.KillBloom(position, true);
            VfxKit.Explode(position, VfxBudget.Large, SettingsState.ReduceEffectsEnabled);
            ShakeWorld(shake, position);
            RequestStop(HitstopBudget.HeavySeconds);
            if (flash)
            {
                FlashScreen(HeavyKillFlash, UiTheme.Primary, HitFlashLimiter.DefaultDecay, true);
            }
        }

        /// <summary>
        /// Cosmetic boss death. The wave is already notified; this object
        /// outlives the destroyed boss. One hitstop, on the killing blow.
        /// </summary>
        public static void BossKill(Vector3 position)
        {
            HeavyKill(position, FeelScale.BossKillShake, false);
            float radius = LookPolish.EnemyBossDiameter * BossRules.VisualScale;
            BossKillSequence.Begin(position, radius);
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
            FlashScreen(FeelScale.BossPhaseFlash, UiTheme.Secondary, HitFlashLimiter.DefaultDecay, false);
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
                    MeshHitFlash.Pop(target);
                }

                EnemySeeker bossHit = target.GetComponent<EnemySeeker>();
                if (bossHit != null && bossHit.IsBoss && GameUi.Instance != null)
                {
                    GameUi.Instance.PulseBossBar();
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
                float amp = ThreatShake(exploded, tier, armor);
                if (PassShakeGate(exploded))
                {
                    ShakeWorld(amp, target.position);
                }
            }
            else if (!(exploded && reduce))
            {
                float amp = ThreatShake(exploded, tier, armor);
                if (PassShakeGate(exploded))
                {
                    Shake(amp);
                }
            }

            if (exploded && tier == LookPolish.TierLarge)
            {
                if (EffectScale.AllowsHitstop(reduce))
                {
                    RequestStop(LookPolish.LargeHitstop);
                }
            }
            else if (exploded && tier == LookPolish.TierMid)
            {
                if (EffectScale.AllowsHitstop(reduce))
                {
                    RequestStop(HitstopBudget.KillMidSeconds);
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

        private static float ThreatShake(bool exploded, int tier, bool armor)
        {
            if (!exploded)
            {
                return armor ? FeelScale.ArmorHitShake : FeelScale.ThreatHitShake;
            }

            if (tier == LookPolish.TierLarge)
            {
                return FeelScale.LargeKillShake;
            }

            if (tier == LookPolish.TierMid)
            {
                return FeelScale.MidKillShake;
            }

            return FeelScale.SmallKillShake;
        }

        private static bool PassShakeGate(bool exploded)
        {
            if (exploded)
            {
                return true;
            }

            float now = Time.unscaledTime;
            if (now - _lastHitShake < FeelScale.HitShakeGateSeconds)
            {
                return false;
            }

            _lastHitShake = now;
            return true;
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

    /// <summary>
    /// Boss-death cosmetics on their own object. The boss is already destroyed
    /// and the wave has been notified. Offsets are fixed so a run stays reproducible.
    /// </summary>
    public sealed class BossKillSequence : MonoBehaviour
    {
        private static readonly float[] Angles = new float[] { 0.4f, 1.7f, 3.0f, 4.3f, 5.5f };

        private Vector3 _position;
        private float _radius;
        private float _born;
        private int _fired;
        private bool _finale;
        private bool _reduce;

        public static void Begin(Vector3 position, float radius)
        {
            GameObject root = new GameObject("BossKillSequence");
            root.transform.position = position;
            BossKillSequence sequence = root.AddComponent<BossKillSequence>();
            sequence._position = position;
            sequence._radius = radius > 0f ? radius : 0.5f;
            sequence._born = Time.unscaledTime;
            sequence._reduce = SettingsState.ReduceEffectsEnabled;
        }

        private void Update()
        {
            float elapsed = Time.unscaledTime - _born;
            if (!_reduce)
            {
                int count = Angles.Length;
                if (count > FeelScale.BossKillBursts)
                {
                    count = FeelScale.BossKillBursts;
                }

                float span = 0.42f;
                while (_fired < count)
                {
                    float when = 0.08f;
                    if (count > 1)
                    {
                        when = 0.08f + (span * _fired / (count - 1));
                    }

                    if (elapsed < when)
                    {
                        break;
                    }

                    float angle = Angles[_fired];
                    float reach = _radius * 0.65f;
                    Vector3 at = _position + new Vector3(Mathf.Cos(angle) * reach, 0f, Mathf.Sin(angle) * reach);
                    VfxKit.Explode(at, VfxBudget.Mid, false);
                    CombatJuice.ShakeWorld(FeelScale.SecondaryKillShake, at);
                    _fired++;
                }
            }

            if (!_finale && elapsed >= 0.55f)
            {
                _finale = true;
                VfxKit.Explode(_position, VfxBudget.Large, _reduce);
                if (!_reduce)
                {
                    CombatJuice.FlashScreen(
                        FeelScale.BossKillFlash,
                        UiTheme.Primary,
                        HitFlashLimiter.DefaultDecay,
                        true);
                }
            }

            if (elapsed >= 0.75f)
            {
                Destroy(gameObject);
            }
        }
    }
}
