using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Named part slots that share origin (0,0,0) so FBX swaps stay aligned.
    /// </summary>
    public sealed class ShipVisuals : MonoBehaviour
    {
        public Transform BodySlot;
        public Transform NoseSlot;
        public Transform EngineSlot;
        public GameObject DefaultBody;
        public GameObject UpgradedBody;
        public GameObject DefaultNose;
        public GameObject UpgradedNose;
        public GameObject UpgradedNose02;
        public GameObject DefaultEngine;
        public GameObject UpgradedEngine;
        public GameObject UpgradedEngine02;
        public GameObject ShieldBubble;

        public const float BlinkIntervalSeconds = 0.09f;

        private float _blinkUntil;
        private float _nextToggle;
        private bool _blinkHidden;
        private Renderer[] _blinkRenderers;

        public bool IsBlinking
        {
            get { return _blinkUntil > 0f && Time.time < _blinkUntil; }
        }

        public void PlayHitBlink(float duration)
        {
            StopHitBlink();
            if (duration <= 0f)
            {
                return;
            }

            _blinkRenderers = GetComponentsInChildren<Renderer>(false);
            _blinkUntil = Time.time + duration;
            _nextToggle = Time.time;
            _blinkHidden = false;
            ToggleBlink();
        }

        public void StopHitBlink()
        {
            _blinkUntil = 0f;
            _nextToggle = 0f;
            _blinkHidden = false;
            SetBlinkRenderersVisible(true);
            _blinkRenderers = null;
        }

        public void ApplyLoadout(LoadoutState loadout)
        {
            bool bodyUpgrade = loadout != null && loadout.BodyUpgrade01;
            if (DefaultBody != null)
            {
                DefaultBody.SetActive(!bodyUpgrade);
            }

            if (UpgradedBody != null)
            {
                UpgradedBody.SetActive(bodyUpgrade);
            }

            int noseTier = 0;
            if (loadout != null && loadout.NoseUpgrade02)
            {
                noseTier = 2;
            }
            else if (loadout != null && loadout.NoseHardpoint)
            {
                noseTier = 1;
            }

            SetTier(DefaultNose, UpgradedNose, UpgradedNose02, noseTier);

            int engineTier = 0;
            if (loadout != null && loadout.EngineUpgrade02)
            {
                engineTier = 2;
            }
            else if (loadout != null && loadout.RapidFire)
            {
                engineTier = 1;
            }

            SetTier(DefaultEngine, UpgradedEngine, UpgradedEngine02, engineTier);
        }

        private static void SetTier(GameObject tier0, GameObject tier1, GameObject tier2, int tier)
        {
            if (tier0 != null)
            {
                tier0.SetActive(tier == 0);
            }

            if (tier1 != null)
            {
                tier1.SetActive(tier == 1);
            }

            if (tier2 != null)
            {
                tier2.SetActive(tier == 2);
            }
        }

        public void SetShieldVisible(bool visible)
        {
            if (ShieldBubble != null)
            {
                ShieldBubble.SetActive(visible);
            }
        }

        private void Update()
        {
            if (_blinkUntil <= 0f)
            {
                return;
            }

            if (Time.time >= _blinkUntil)
            {
                StopHitBlink();
                return;
            }

            if (Time.time >= _nextToggle)
            {
                ToggleBlink();
            }
        }

        private void ToggleBlink()
        {
            _blinkHidden = !_blinkHidden;
            SetBlinkRenderersVisible(!_blinkHidden);
            _nextToggle = Time.time + BlinkIntervalSeconds;
        }

        private void SetBlinkRenderersVisible(bool visible)
        {
            if (_blinkRenderers == null)
            {
                return;
            }

            for (int i = 0; i < _blinkRenderers.Length; i++)
            {
                if (_blinkRenderers[i] != null)
                {
                    _blinkRenderers[i].enabled = visible;
                }
            }
        }

        private void OnDisable()
        {
            StopHitBlink();
        }
    }
}
