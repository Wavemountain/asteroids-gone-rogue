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
            if (item == null)
            {
                return false;
            }

            int world = ShopWorld();
            bool mk2 = !_loadout.State.CanApply(id) && _loadout.State.CanBuyMk2(id);
            if (!_loadout.State.CanApply(id) && !mk2)
            {
                return false;
            }

            int price = mk2 ? _loadout.State.Mk2Price(item, world) : _loadout.State.EffectiveCost(item, world);
            if (!_session.TrySpend(price))
            {
                return false;
            }

            _loadout.State.ConsumeFirstDiscount();
            if (mk2)
            {
                _loadout.State.GrantMk2(id);
            }
            else
            {
                _loadout.State.Apply(id);
                _loadout.State.AutoEquipAfterPurchase(id);
            }
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayHangarPurchase();
            }

            _game.NotifyLoadoutChanged();
            return true;
        }

        public bool TryEquip(UpgradeId id)
        {
            if (_session == null || !_session.ShopOpen || _loadout == null || _loadout.State == null)
            {
                return false;
            }

            if (!_loadout.State.TryEquip(id))
            {
                return false;
            }

            _game.NotifyLoadoutChanged();
            return true;
        }

        public bool TryPickDoctrine(DoctrineId id)
        {
            if (_session == null || !_session.ShopOpen || _loadout == null || _loadout.State == null)
            {
                return false;
            }

            if (!DoctrineRules.HangarUnlocked(_session.WaveIndex))
            {
                return false;
            }

            LoadoutState state = _loadout.State;
            if (!state.CanPickDoctrine(id))
            {
                return false;
            }

            int cost = state.DoctrinePickCost(id);
            if (cost > 0 && !_session.TrySpend(cost))
            {
                return false;
            }

            if (cost > 0)
            {
                state.ConsumeFirstDiscount();
            }

            state.SetDoctrine(id);
            if (AudioCues.Instance != null)
            {
                AudioCues.Instance.PlayDoctrinePick();
            }

            _game.NotifyDoctrinePicked(id);
            _game.NotifyLoadoutChanged();
            return true;
        }

        public bool TryRepairHull()
        {
            return _game != null && _game.TryBuyHullRepair();
        }

        public bool TryRefillShield()
        {
            return _game != null && _game.TryBuyShieldRefill();
        }

        public bool TryBuyExtraLife()
        {
            return _game != null && _game.TryBuyExtraLife();
        }

        public bool TryBankCredits()
        {
            return _game != null && _game.TryBankCredits();
        }

        private int ShopWorld()
        {
            int wave = _session != null ? _session.WaveIndex : 1;
            return WorldCatalog.NumberForWave(wave);
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
