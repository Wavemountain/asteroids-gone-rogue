using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Marks a ship part root that shares origin (0,0,0) with sibling slots.
    /// Drop replacement FBX under this transform without moving the slot.
    /// </summary>
    public sealed class PartSlot : MonoBehaviour
    {
        public string SlotId;
    }
}
