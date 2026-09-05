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
            RapidFire
        }

        private Kind _kind;
        private bool _taken;

        public void Bind(Kind kind)
        {
            _kind = kind;
        }

        public static Kind KindFromName(string visualName)
        {
            if (string.IsNullOrEmpty(visualName))
            {
                return Kind.Score;
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
                default:
                    GameManager game = Object.FindObjectOfType<GameManager>();
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
