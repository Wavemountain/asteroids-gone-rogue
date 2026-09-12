using UnityEngine;

namespace AsteroidsGoneRogue
{
    public sealed class Pickup : MonoBehaviour
    {
        public enum Kind
        {
            Score,
            Shield,
            Health,
            RapidFire,
            ExtraLife
        }

        private Kind _kind;
        private bool _taken;
        private bool _expires;
        private float _expireAt;
        private Vector3 _origin;
        private Vector3 _baseScale = Vector3.one;

        public void Bind(Kind kind)
        {
            Bind(kind, 0f);
        }

        public void Bind(Kind kind, float timeoutSeconds)
        {
            _kind = kind;
            _origin = transform.position;
            _baseScale = transform.localScale;
            if (timeoutSeconds > 0f)
            {
                _expires = true;
                _expireAt = Time.time + timeoutSeconds;
            }
        }

        public static Kind KindFromName(string visualName)
        {
            if (string.IsNullOrEmpty(visualName))
            {
                return Kind.Score;
            }

            if (visualName.IndexOf("ExtraLife", System.StringComparison.OrdinalIgnoreCase) >= 0
                || visualName.IndexOf("1UP", System.StringComparison.OrdinalIgnoreCase) >= 0
                || visualName.IndexOf("Heart", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return Kind.ExtraLife;
            }

            if (visualName.IndexOf("Shield", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return Kind.Shield;
            }

            if (visualName.IndexOf("Health", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return Kind.Health;
            }

            if (visualName.IndexOf("Rapid", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return Kind.RapidFire;
            }

            return Kind.Score;
        }

        private void Update()
        {
            if (_taken)
            {
                return;
            }

            if (_expires && Time.time >= _expireAt)
            {
                Destroy(gameObject);
                return;
            }

            float pulse = 1f + Mathf.Sin(Time.time * 6.2f) * 0.12f;
            if (_kind == Kind.ExtraLife)
            {
                pulse = 1.08f + Mathf.Sin(Time.time * 7.4f) * 0.18f;
                float remain = _expires ? Mathf.Clamp01((_expireAt - Time.time) / 1.4f) : 1f;
                if (remain < 1f)
                {
                    pulse *= 0.55f + remain * 0.45f;
                }
            }

            transform.localScale = _baseScale * pulse;
            Vector3 pos = _origin;
            pos.y = _origin.y + Mathf.Sin(Time.time * 3.4f) * 0.16f;
            transform.position = pos;
            transform.Rotate(0f, 80f * Time.deltaTime, 0f, Space.World);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_taken || other == null || !other.CompareTag(GameTags.Player))
            {
                return;
            }

            ShipController ship = other.GetComponentInParent<ShipController>();
            if (ship == null)
            {
                return;
            }

            _taken = true;
            Apply(ship);
            Destroy(gameObject);
        }

        private void Apply(ShipController ship)
        {
            switch (_kind)
            {
                case Kind.Shield:
                    if (ship.Health != null)
                    {
                        ship.Health.TryAddShield();
                    }

                    break;
                case Kind.Health:
                    if (ship.Health != null)
                    {
                        ship.Health.TryHeal(1);
                    }

                    break;
                case Kind.RapidFire:
                    if (ship.Shooter != null)
                    {
                        ship.Shooter.GrantRapidBoost(8f);
                    }

                    break;
                case Kind.ExtraLife:
                    GameManager extra = Object.FindAnyObjectByType<GameManager>();
                    if (extra != null)
                    {
                        extra.TryGrantExtraLife();
                    }

                    break;
                default:
                    GameManager game = Object.FindAnyObjectByType<GameManager>();
                    if (game != null)
                    {
                        game.AddBonusScore(ScoreValues.SmallAsteroid);
                    }

                    break;
            }

            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayHangarPurchase();
            }
        }
    }
}
