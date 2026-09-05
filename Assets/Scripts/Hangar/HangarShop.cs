namespace AsteroidsGoneRogue
{
    public sealed class HangarShop : UnityEngine.MonoBehaviour
    {
        private PlayerLoadout _loadout;
        private GameSession _session;
        private GameManager _game;

        public void Initialize(PlayerLoadout loadout, GameSession session, GameManager game)
        {
            _loadout = loadout;
            _session = session;
            _game = game;
        }

        public bool TryBuy(UpgradeId id)
        {
            if (_session == null || !_session.ShopOpen)
            {
                return false;
            }

            ShopItem item = FindItem(id);
            if (item == null || !_loadout.State.CanApply(id))
            {
                return false;
            }

            if (!_session.TrySpend(item.Cost))
            {
                return false;
            }

            _loadout.State.Apply(id);
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayHangarPurchase();
            }

            _game.NotifyLoadoutChanged();
            return true;
        }

        public static ShopItem FindItem(UpgradeId id)
        {
            for (int i = 0; i < ShopCatalog.Items.Length; i++)
            {
                if (ShopCatalog.Items[i].Id == id)
                {
                    return ShopCatalog.Items[i];
                }
            }

            return null;
        }
    }
}
