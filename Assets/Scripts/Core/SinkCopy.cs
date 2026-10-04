namespace AsteroidsGoneRogue
{
    /// <summary>
    /// EN fallbacks for the bay overlay. Swedish lives in <see cref="Loc"/>.
    /// </summary>
    public static class SinkCopy
    {
        public static string Entry()
        {
            return Loc.T("sink.entry", "Bay");
        }

        public static string Title()
        {
            return Loc.T("sink.title", "Bay fittings");
        }

        public static string Close()
        {
            return Loc.T("sink.close", "Close");
        }

        public static string Hint()
        {
            string line = Loc.T("sink.hint", "{confirm} buy  ·  {cancel} close");
            return PromptText.Flatten(line, InputSchemeDriver.Current);
        }

        public static string Blurb()
        {
            return Loc.T("sink.blurb", "\u2022 Paint and trail stay on this profile. Shield and reach start the next run.");
        }

        public static string Name(int sinkId)
        {
            switch (sinkId)
            {
                case ShopSinkCatalog.PaintSteel:
                    return Loc.T("sink.paint.steel", "Steel hull");
                case ShopSinkCatalog.TrailCyan:
                    return Loc.T("sink.trail.cyan", "Cyan trail");
                case ShopSinkCatalog.TrailAmber:
                    return Loc.T("sink.trail.amber", "Amber trail");
                case ShopSinkCatalog.StartShield:
                    return Loc.T("sink.shield", "Start shield");
                case ShopSinkCatalog.PickupReach:
                    return Loc.T("sink.reach", "Pickup reach");
                default:
                    return Loc.T("sink.paint.amber", "Amber hull");
            }
        }

        public static string Detail(int sinkId)
        {
            switch (sinkId)
            {
                case ShopSinkCatalog.PaintSteel:
                    return Loc.T("sink.desc.steel", "Cool steel paint on the hangar hull.");
                case ShopSinkCatalog.TrailCyan:
                    return Loc.T("sink.desc.trail.cyan", "Cyan player trail. Cosmetic.");
                case ShopSinkCatalog.TrailAmber:
                    return Loc.T("sink.desc.trail.amber", "Amber player trail. Cosmetic.");
                case ShopSinkCatalog.StartShield:
                    return Loc.T("sink.desc.shield", "Next run starts with +1 shield. Cap 1.");
                case ShopSinkCatalog.PickupReach:
                    return Loc.T("sink.desc.reach", "Next run pickup radius +5% per rank. Cap 2.");
                default:
                    return Loc.T("sink.desc.amber", "Amber paint on the hangar hull.");
            }
        }

        public static string Status(int state, int price)
        {
            int shown = price < 0 ? 0 : price;
            switch (state)
            {
                case SinkRules.StatePoor:
                    return Loc.Tf("sink.status.need", "NEED {0}", shown);
                case SinkRules.StateOwned:
                    return Loc.T("sink.status.owned", "OWNED");
                case SinkRules.StateFitted:
                    return Loc.T("sink.status.fitted", "FITTED");
                case SinkRules.StateCapped:
                    return Loc.T("sink.status.capped", "CAPPED");
                default:
                    return Loc.Tf("sink.status.price", "{0}", shown);
            }
        }
    }
}
