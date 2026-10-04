namespace AsteroidsGoneRogue
{
    /// <summary>
    /// EN fallbacks for the mutator chooser. Swedish lives in <see cref="Loc"/>.
    /// </summary>
    public static class MutatorCopy
    {
        public static string Name(int id)
        {
            switch (id)
            {
                case MutatorCatalog.Swarm:
                    return Loc.T("mut.swarm", "Swarm Season");
                case MutatorCatalog.Heavy:
                    return Loc.T("mut.heavy", "Heavy Rocks");
                case MutatorCatalog.Quiet:
                    return Loc.T("mut.quiet", "Quiet Space");
                case MutatorCatalog.Overclock:
                    return Loc.T("mut.overclock", "Overclock");
                case MutatorCatalog.LongHaul:
                    return Loc.T("mut.long", "Long Haul");
                default:
                    return Loc.T("mut.glass", "Glass Cannon");
            }
        }

        public static string Description(int id)
        {
            switch (id)
            {
                case MutatorCatalog.Swarm:
                    return Loc.T("mut.swarm.desc", "More swarmlings, fewer heavies.");
                case MutatorCatalog.Heavy:
                    return Loc.T("mut.heavy.desc", "Asteroids are tougher and split once more.");
                case MutatorCatalog.Quiet:
                    return Loc.T("mut.quiet.desc", "Fewer pickups. Credit rewards rise.");
                case MutatorCatalog.Overclock:
                    return Loc.T("mut.overclock.desc", "Enemies fire faster.");
                case MutatorCatalog.LongHaul:
                    return Loc.T("mut.long.desc", "The roster uses the next wave rung.");
                default:
                    return Loc.T("mut.glass.desc", "You and enemies take +50% damage.");
            }
        }

        public static string Row(int id, bool selected)
        {
            string mark = selected ? "\u2022 " : string.Empty;
            string factors = Loc.Tf(
                "mut.factors",
                "score ×{0}  ·  credits ×{1}",
                MutatorRules.Factor(SingleScore(id)),
                MutatorRules.Factor(SingleCredit(id)));
            return mark + Name(id) + "  ·  " + factors;
        }

        public static string Rejected()
        {
            return Loc.T("mut.rejected", "\u2022 At most two. Some pairs cannot combine.");
        }

        public static string StackLine(int mask)
        {
            int clean = MutatorRules.Sanitize(mask);
            if (clean == 0)
            {
                return string.Empty;
            }

            return Loc.Tf(
                "mut.stack",
                "Chosen  ·  score ×{0}  ·  credits ×{1}",
                MutatorRules.Factor(MutatorRules.ScorePercent(clean)),
                MutatorRules.Factor(MutatorRules.CreditPercent(clean)));
        }

        public static string Hud(int mask)
        {
            int clean = MutatorRules.Sanitize(mask);
            if (clean == 0)
            {
                return string.Empty;
            }

            string line = string.Empty;
            for (int id = 0; id < MutatorCatalog.Count; id++)
            {
                if (!MutatorRules.Has(clean, id))
                {
                    continue;
                }

                if (line.Length > 0)
                {
                    line += "  ·  ";
                }

                line += Name(id);
            }

            return Loc.Tf("mut.hud", "Mutators {0}", line);
        }

        private static int SingleScore(int id)
        {
            return MutatorRules.ScorePercent(MutatorCatalog.Bit(id));
        }

        private static int SingleCredit(int id)
        {
            return MutatorRules.CreditPercent(MutatorCatalog.Bit(id));
        }
    }
}
