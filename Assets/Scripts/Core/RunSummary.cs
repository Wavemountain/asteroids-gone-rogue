namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Short hangar cards: end-of-run stats, wave 1–5 continue lines, and medal announce copy.
    /// Scout Wing / Deep Orbit / Far Drift persist in <see cref="HangarPersist"/> / the hangar badge row.
    /// Pure C# so tests can check the copy without the Editor.
    /// </summary>
    public static class RunSummary
    {
        public const int World2StartsAtWave = 6;
        public const int World3StartsAtWave = 11;
        public const string Wave3MedalTitle = "Scout Wing";

        public static string Title(GamePhase phase, string failReason)
        {
            if (phase == GamePhase.Failed)
            {
                if (string.IsNullOrEmpty(failReason))
                {
                    return "SHIP LOST";
                }

                return "SHIP LOST  ·  " + failReason;
            }

            if (phase == GamePhase.WaveClear)
            {
                return "WAVE CLEAR";
            }

            return "RUN";
        }

        public static string StatsLine(int score, int wave, int world)
        {
            return "Score " + score + "  ·  Wave " + wave + "  ·  World " + world;
        }

        public static string CreditsLine(int credits, int awarded)
        {
            if (awarded > 0)
            {
                return "Credits " + credits + "  (+" + awarded + ")";
            }

            return "Credits " + credits;
        }

        public static string UpgradesLine(LoadoutState loadout)
        {
            if (loadout == null)
            {
                return "Upgrades —";
            }

            string names = string.Empty;
            AppendOwned(ref names, loadout.BodyUpgrade01, "Body");
            AppendOwned(ref names, loadout.NoseHardpoint, "Nose");
            AppendOwned(ref names, loadout.NoseUpgrade02, "Nose 02");
            AppendOwned(ref names, loadout.RapidFire, "Rapid Fire");
            AppendOwned(ref names, loadout.EngineUpgrade02, "Engine 02");
            AppendOwned(ref names, loadout.SpreadBolt, "Spread");
            AppendOwned(ref names, loadout.Pierce, "Pierce");
            if (loadout.ShieldCharges > 0)
            {
                AppendOwned(ref names, true, "Shield x" + loadout.ShieldCharges);
            }

            return string.IsNullOrEmpty(names) ? "Upgrades —" : "Upgrades  " + names;
        }

        public static bool ShowAfterWave1Hint(int lastResolvedWave, GamePhase phase)
        {
            return lastResolvedWave == 1 && phase == GamePhase.WaveClear;
        }

        public static bool ShowContinueHint(int lastResolvedWave, GamePhase phase)
        {
            return phase == GamePhase.WaveClear && lastResolvedWave >= 1 && lastResolvedWave <= 5;
        }

        public static string AfterWave1Hint(int credits, LoadoutState loadout)
        {
            return ContinueHint(1, credits, loadout);
        }

        public static string ContinueHint(int lastResolvedWave, int credits, LoadoutState loadout)
        {
            ShopItem next = NextUnlock(credits, loadout);
            if (lastResolvedWave == 3)
            {
                return next != null
                    ? "Buy " + next.Title + " before Gunner"
                    : "Push for a new best before Gunner";
            }

            if (lastResolvedWave == 4 || lastResolvedWave == 5)
            {
                string tease = DeepOrbitTeaser(lastResolvedWave);
                return next != null ? tease + "  ·  Buy " + next.Title : tease;
            }

            string buy = next != null ? "Buy " + next.Title : "Push for a new best.";
            return NextUnlockLandmark(lastResolvedWave) + "  ·  " + buy;
        }

        public static string DeepOrbitTeaser(int lastResolvedWave)
        {
            if (lastResolvedWave == 5)
            {
                return "World 2  ·  ★ " + MedalCatalog.DeepOrbitTitle;
            }

            return "★ " + MedalCatalog.DeepOrbitTitle + " at wave " + World2StartsAtWave;
        }

        public static bool ShowWaveMedal(int lastResolvedWave, GamePhase phase)
        {
            if (lastResolvedWave == MedalCatalog.ScoutWingClearsAtWave)
            {
                return phase == GamePhase.WaveClear;
            }

            if (lastResolvedWave == World2StartsAtWave)
            {
                return phase == GamePhase.WaveClear || phase == GamePhase.Failed;
            }

            if (lastResolvedWave == MedalCatalog.FarDriftClearsAtWave)
            {
                return phase == GamePhase.WaveClear;
            }

            return false;
        }

        public static string WaveMedal(int lastResolvedWave)
        {
            if (lastResolvedWave == MedalCatalog.ScoutWingClearsAtWave)
            {
                return MedalCatalog.AwardLine(MedalId.ScoutWing)
                    + "  ·  World 2 at wave " + World2StartsAtWave;
            }

            if (lastResolvedWave == World2StartsAtWave)
            {
                return MedalCatalog.AwardLine(MedalId.DeepOrbit)
                    + "  ·  World 3 at wave " + World3StartsAtWave;
            }

            if (lastResolvedWave == MedalCatalog.FarDriftClearsAtWave)
            {
                return MedalCatalog.AwardLine(MedalId.FarDrift) + "  ·  World 3";
            }

            return string.Empty;
        }

        public static string NextUnlockLandmark(int lastResolvedWave)
        {
            if (lastResolvedWave == 2)
            {
                return "Gunner at wave 4";
            }

            if (lastResolvedWave == 3)
            {
                return "before Gunner";
            }

            if (lastResolvedWave == 4 || lastResolvedWave == 5)
            {
                return DeepOrbitTeaser(lastResolvedWave);
            }

            return "World 2 at wave " + World2StartsAtWave;
        }

        public static ShopItem NextUnlock(int credits, LoadoutState loadout)
        {
            ShopItem cheapestAffordable = null;
            ShopItem cheapestOpen = null;
            for (int i = 0; i < ShopCatalog.Items.Length; i++)
            {
                ShopItem item = ShopCatalog.Items[i];
                if (loadout != null && !loadout.CanApply(item.Id))
                {
                    continue;
                }

                if (cheapestOpen == null || item.Cost < cheapestOpen.Cost)
                {
                    cheapestOpen = item;
                }

                if (credits >= item.Cost
                    && (cheapestAffordable == null || item.Cost < cheapestAffordable.Cost))
                {
                    cheapestAffordable = item;
                }
            }

            return cheapestAffordable != null ? cheapestAffordable : cheapestOpen;
        }

        private static void AppendOwned(ref string names, bool owned, string label)
        {
            if (!owned)
            {
                return;
            }

            if (!string.IsNullOrEmpty(names))
            {
                names += "  ·  ";
            }

            names += label;
        }
    }
}
