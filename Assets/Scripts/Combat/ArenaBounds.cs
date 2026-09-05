using UnityEngine;

namespace AsteroidsGoneRogue
{
    public sealed class ArenaBounds : MonoBehaviour
    {
        public float Radius = WaveManager.ArenaRadius;

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.3f, 0.7f, 0.8f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, Radius);
        }
    }
}
