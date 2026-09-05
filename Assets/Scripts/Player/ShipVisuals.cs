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
    }
}
