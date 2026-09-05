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
        public GameObject DefaultNose;
        public GameObject UpgradedNose;
        public GameObject DefaultEngine;
        public GameObject UpgradedEngine;
        public GameObject ShieldBubble;

        public void ApplyLoadout(LoadoutState loadout)
        {
            bool hardpoint = loadout != null && loadout.NoseHardpoint;
            if (DefaultNose != null)
            {
                DefaultNose.SetActive(!hardpoint);
            }

            if (UpgradedNose != null)
            {
                UpgradedNose.SetActive(hardpoint);
            }

            bool engineUpgrade = loadout != null && loadout.RapidFire;
            if (DefaultEngine != null)
            {
                DefaultEngine.SetActive(!engineUpgrade);
            }

            if (UpgradedEngine != null)
            {
                UpgradedEngine.SetActive(engineUpgrade);
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
