using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Scene-facing wrapper around <see cref="LoadoutState"/>.
    /// </summary>
    public sealed class PlayerLoadout : MonoBehaviour
    {
        public LoadoutState State { get; private set; }

        public void Bind(LoadoutState state)
        {
            State = state;
        }
    }
}
