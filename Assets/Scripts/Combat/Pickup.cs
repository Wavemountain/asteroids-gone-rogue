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

        public const float ExtraLifePulseHz = 8.6f;
        public const float ExtraLifeBob = 0.28f;
        public const float ExtraLifeUrgentSeconds = 2.4f;

        private Kind _kind;
        private bool _taken;
        private bool _expires;
        private float _expireAt;
        private Vector3 _origin;
        private Vector3 _baseScale = Vector3.one;
        private Light _beacon;
        private Transform _pip;
        private float _ringAt;

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

            if (kind == Kind.ExtraLife)
            {
                DressMustPick();
                CombatJuice.ExtraLifeSpawn(transform.position);
                _ringAt = Time.time;
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
                if (_kind == Kind.ExtraLife && AudioCues.Instance != null)
                {
                    AudioCues.Instance.PlayExtraLifeMiss();
                }

                Destroy(gameObject);
                return;
            }

            float pulse = 1f + Mathf.Sin(Time.time * 6.2f) * 0.12f;
            if (_kind == Kind.ExtraLife)
            {
                float remain = _expires ? (_expireAt - Time.time) : ExtraLifeUrgentSeconds;
                bool urgent = remain < ExtraLifeUrgentSeconds;
                float hz = urgent ? ExtraLifePulseHz + 3.2f : ExtraLifePulseHz;
                pulse = 1.18f + Mathf.Sin(Time.time * hz) * (urgent ? 0.28f : 0.2f);
                if (urgent)
                {
                    pulse *= 0.72f + Mathf.PingPong(Time.time * 6f, 0.4f);
                }

                PulseBeacon(urgent);
                if (Time.time >= _ringAt)
                {
                    _ringAt = Time.time + (urgent ? 0.55f : 0.9f);
                    GameObject root = new GameObject("TelegraphRing");
                    root.transform.position = new Vector3(_origin.x, 0.04f, _origin.z);
                    TelegraphRing ring = root.AddComponent<TelegraphRing>();
                    ring.Play(new Color(1f, 0.18f, 0.32f), urgent ? 0.5f : 0.7f);
                }
            }

            transform.localScale = _baseScale * pulse;
            Vector3 pos = _origin;
            float bob = _kind == Kind.ExtraLife ? ExtraLifeBob : 0.16f;
            pos.y = _origin.y + Mathf.Sin(Time.time * 3.4f) * bob;
            transform.position = pos;
            transform.Rotate(0f, (_kind == Kind.ExtraLife ? 120f : 80f) * Time.deltaTime, 0f, Space.World);
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
                    GameManager extra = UnityEngine.Object.FindAnyObjectByType<GameManager>();
                    if (extra != null)
                    {
                        extra.TryGrantExtraLife();
                    }

                    CombatJuice.ExtraLifeTaken(transform.position);
                    break;
                default:
                    GameManager game = UnityEngine.Object.FindAnyObjectByType<GameManager>();
                    if (game != null)
                    {
                        game.AddBonusScore(ScoreValues.SmallAsteroid);
                    }

                    break;
            }

            if (AudioCues.Instance == null)
            {
                return;
            }

            if (_kind == Kind.ExtraLife)
            {
                AudioCues.Instance.PlayExtraLifePickup();
            }
            else
            {
                AudioCues.Instance.PlayPickupMinor();
            }
        }

        private void DressMustPick()
        {
            GameObject pip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pip.name = "HeartPip";
            pip.transform.SetParent(transform, false);
            pip.transform.localPosition = new Vector3(0f, 1.15f, 0f);
            pip.transform.localScale = new Vector3(0.18f, 0.42f, 0.18f);
            Collider pipCol = pip.GetComponent<Collider>();
            if (pipCol != null)
            {
                Destroy(pipCol);
            }

            _pip = pip.transform;

            GameObject lightGo = new GameObject("HeartBeacon");
            lightGo.transform.SetParent(transform, false);
            lightGo.transform.localPosition = new Vector3(0f, 0.4f, 0f);
            _beacon = lightGo.AddComponent<Light>();
            _beacon.type = LightType.Point;
            _beacon.range = 7.5f;
            _beacon.color = new Color(1f, 0.28f, 0.4f);
            _beacon.intensity = 2.4f;
        }

        private void PulseBeacon(bool urgent)
        {
            if (_beacon != null)
            {
                float pulse = 1.8f + Mathf.Sin(Time.time * (urgent ? 14f : 8f)) * 0.85f;
                if (urgent)
                {
                    pulse += 0.8f * Mathf.PingPong(Time.time * 9f, 1f);
                }

                _beacon.intensity = pulse;
                _beacon.range = urgent ? 9f : 7.5f;
            }

            if (_pip != null)
            {
                float hop = 1.05f + Mathf.Sin(Time.time * 7f) * 0.22f;
                _pip.localPosition = new Vector3(0f, hop, 0f);
            }
        }
    }
}
