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
                    return Loc.T("mut.glass.desc", "You and enemies take +50% damage. Score ×1.50, credits ×1.20.");
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

        /// <summary>
        /// Glass stays on the chooser so a normal run can still toggle it.
        /// The row says it will not ride a daily seed.
        /// </summary>
        public static string DailyBlockedLabel(bool selected)
        {
            string mark = selected ? "\u2022 " : string.Empty;
            return mark + Name(MutatorCatalog.Glass) + "  ·  " + Loc.T("mut.glass.daily", "Off in a Daily run");
        }

        public static string Rejected()
        {
            return Loc.T("mut.rejected", "\u2022 At most two. Some pairs cannot combine.");
        }

        public static string StackLine(int mask)
        {
            return RiskLine(mask);
        }

        public static string RiskLine(int mask)
        {
            int pips = MutatorRules.RiskPips(mask);
            if (pips < 1)
            {
                return string.Empty;
            }

            string bullets = MutatorRules.RiskBullets(pips);
            string score = MutatorRules.CompactFactor(MutatorRules.ScorePercent(mask));
            int credits = MutatorRules.CreditPercent(mask);
            if (credits == 100)
            {
                return Loc.Tf("mut.stack", "Risk {0} / Score x{1}", bullets, score);
            }

            return Loc.Tf(
                "mut.risk.pay",
                "Risk {0} / Score x{1} / Credits x{2}",
                bullets,
                score,
                MutatorRules.CompactFactor(credits));
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
