namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Short hangar cards: end-of-run stats, wave 1–5 continue lines, and medal announce copy.
    /// Scout Wing / Deep Orbit / Far Drift persist in <see cref="HangarPersist"/> / the hangar badge row.
    /// World 3 hangar line is a New sector beat with no medal.
    /// Pure C# so tests can check the copy without the Editor.
    /// </summary>
    public static class RunSummary
    {
        public const int World2StartsAtWave = 6;
        public const int World3StartsAtWave = 11;
        public const int AlmostHadItMax = 3;
        public const string Wave3MedalTitle = "Scout Wing";

        public static string Title(GamePhase phase, string failReason)
        {
            if (phase == GamePhase.Failed)
            {
                if (string.IsNullOrEmpty(failReason))
                {
                    return Loc.T("run.ship_lost", "SHIP LOST");
                }

                return Loc.Tf("run.ship_lost_reason", "SHIP LOST  ·  {0}", failReason);
            }

            if (phase == GamePhase.CampaignClear)
            {
                return CampaignCap.WinLine();
            }

            if (phase == GamePhase.WaveClear)
            {
                return Loc.T("run.wave_clear", "WAVE CLEAR");
            }

            return Loc.T("run.run", "RUN");
        }

        public static string StatsLine(int score, int wave, int world)
        {
            return Loc.Tf("run.stats", "Score {0}  ·  Wave {1}  ·  World {2}", score, wave, world);
        }

        public static string CreditsLine(int credits, int awarded)
        {
            if (awarded > 0)
            {
                return Loc.Tf("run.credits_plus", "Credits {0}  (+{1})", credits, awarded);
            }

            return Loc.Tf("run.credits", "Credits {0}", credits);
        }

        public static string UpgradesLine(LoadoutState loadout)
        {
            if (loadout == null)
            {
                return Loc.T("run.upgrades_none", "Upgrades —");
            }

            string names = string.Empty;
            AppendOwned(ref names, loadout.BodyUpgrade01, Loc.T("up.Body", "Body"));
            AppendOwned(ref names, loadout.BodyUpgrade02, Loc.T("up.Hull02", "Hull 02"));
            AppendOwned(ref names, loadout.NoseHardpoint, Loc.T("up.Nose", "Nose"));
            AppendOwned(ref names, loadout.NoseUpgrade02, Loc.T("up.Nose02", "Nose 02"));
            AppendOwned(ref names, loadout.NoseUpgrade03, Loc.T("up.Nose03", "Nose 03"));
            AppendOwned(ref names, loadout.RapidFire, Loc.T("up.Rapid", "Rapid Fire"));
            AppendOwned(ref names, loadout.EngineUpgrade02, Loc.T("up.Engine02", "Engine 02"));
            AppendOwned(ref names, loadout.EngineUpgrade03, Loc.T("up.Engine03", "Engine 03"));
            AppendOwned(ref names, loadout.Overcharger, Loc.T("up.Overcharger", "Overcharger"));
            AppendOwned(ref names, loadout.Afterburner, Loc.T("up.Afterburner", "Afterburner"));
            AppendOwned(ref names, loadout.SpreadBolt, Loc.T("up.Spread", "Spread"));
            AppendOwned(ref names, loadout.Pierce, Loc.T("up.Pierce", "Pierce"));
            AppendOwned(ref names, loadout.TwinGuns, Loc.T("up.Twin", "Twin"));
            AppendOwned(ref names, loadout.Seeker, Loc.T("up.Seeker", "Seeker"));
            AppendOwned(ref names, loadout.Ricochet, Loc.T("up.Ricochet", "Ricochet"));
            if (loadout.ShieldCharges > 0)
            {
                AppendOwned(ref names, true, Loc.Tf("up.Shield", "Shield x{0}", loadout.ShieldCharges));
            }

            AppendOwned(ref names, loadout.ShieldMatrix, Loc.T("up.Matrix", "Matrix"));

            return string.IsNullOrEmpty(names)
                ? Loc.T("run.upgrades_none", "Upgrades —")
                : Loc.Tf("run.upgrades", "Upgrades  {0}", names);
        }

        public static bool ShowAfterWave1Hint(int lastResolvedWave, GamePhase phase)
        {
            return lastResolvedWave == 1 && phase == GamePhase.WaveClear;
        }

        public static bool ShowContinueHint(int lastResolvedWave, GamePhase phase)
        {
            if (phase == GamePhase.CampaignClear)
            {
                return true;
            }

            return phase == GamePhase.WaveClear && lastResolvedWave >= 1 && lastResolvedWave <= 9;
        }

        public static bool ShowFailContinue(GamePhase phase)
        {
            return phase == GamePhase.Failed;
        }

        public static string CampaignWinHint()
        {
            return CampaignCap.HangarWinHint();
        }

        public static string FailContinueHint(string failReason, int waveIndex)
        {
            return FailContinueHint(failReason, waveIndex, 0);
        }

        public static string FailContinueHint(string failReason, int waveIndex, int remainingThreats)
        {
            string almost = AlmostHadIt(remainingThreats);
            string keep = Loc.T("run.fail_keep", "Your hull. Run over — start from the hangar.");
            string retry = Loc.T("run.fail_retry", "RETRY  ·  New Run from the hangar.");
            string tease = MonsterTeaser(waveIndex);
            string line = string.IsNullOrEmpty(almost) ? retry + "  ·  " + keep : almost + "  ·  " + retry;
            if (!string.IsNullOrEmpty(tease))
            {
                return line + "\n" + tease;
            }

            return line;
        }

        public static string AlmostHadIt(int remainingThreats)
        {
            if (remainingThreats <= 0)
            {
                return string.Empty;
            }

            if (remainingThreats == 1)
            {
                return Loc.T("fail.almost_one", "One left. Almost had it.");
            }

            if (remainingThreats <= AlmostHadItMax)
            {
                return Loc.Tf("fail.almost_n", "Almost had it — {0} left.", remainingThreats);
            }

            return Loc.Tf("fail.left_n", "{0} left.", remainingThreats);
        }

        public static string MonsterTeaser(int nextWave)
        {
            if (nextWave == 5)
            {
                return Loc.T("run.tease_brute", "Watch  ·  Wave 5 Brute — sidestep the charge");
            }

            if (nextWave == 6)
            {
                return Loc.T("run.tease_swarm", "Watch  ·  Wave 6 Swarm — break the nest");
            }

            if (nextWave >= 7)
            {
                return Loc.T("run.tease_both", "Watch  ·  Brute charges  ·  Swarm nests drop Swarmlings");
            }

            return string.Empty;
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
                    ? Loc.Tf("run.buy_gunner", "Buy " + next.Title + " before Gunner", next.Title)
                    : Loc.T("run.push_gunner", "Push for a new best before Gunner");
            }

            if (lastResolvedWave == 4)
            {
                string cap = Loc.Tf(
                    "run.next_sector",
                    "Next  ·  SECTOR CLEAR at wave {0}",
                    CampaignCap.FinalWave);
                return next != null
                    ? cap + "  ·  " + Loc.Tf("run.buy", "Buy {0}", next.Title)
                    : cap;
            }

            if (lastResolvedWave == 5)
            {
                string tease = DeepOrbitTeaser(lastResolvedWave);
                return next != null
                    ? tease + "  ·  " + Loc.Tf("run.buy", "Buy {0}", next.Title)
                    : tease;
            }

            if (lastResolvedWave >= 7 && lastResolvedWave <= 9)
            {
                string tease = FarDriftTeaser(lastResolvedWave);
                return next != null
                    ? tease + "  ·  " + Loc.Tf("run.buy", "Buy {0}", next.Title)
                    : tease;
            }

            string buy = next != null
                ? Loc.Tf("run.buy", "Buy {0}", next.Title)
                : Loc.T("run.push_best", "Push for a new best.");
            return NextUnlockLandmark(lastResolvedWave) + "  ·  " + buy;
        }

        public static string DeepOrbitTeaser(int lastResolvedWave)
        {
            if (lastResolvedWave == 5)
            {
                return Loc.Tf(
                    "run.deep_orbit_now",
                    "World 2  ·  ★ " + MedalCatalog.DeepOrbitTitle,
                    MedalCatalog.Title(MedalId.DeepOrbit));
            }

            return Loc.Tf(
                "run.deep_orbit_at",
                "★ " + MedalCatalog.DeepOrbitTitle + " at wave " + World2StartsAtWave,
                MedalCatalog.Title(MedalId.DeepOrbit),
                World2StartsAtWave);
        }

        public static string FarDriftTeaser(int lastResolvedWave)
        {
            if (lastResolvedWave == 9)
            {
                return Loc.Tf(
                    "run.far_drift_clear",
                    "Clear wave 10  ·  ★ " + MedalCatalog.FarDriftTitle,
                    MedalCatalog.Title(MedalId.FarDrift));
            }

            return Loc.Tf(
                "run.far_drift_at",
                "★ " + MedalCatalog.FarDriftTitle + " at wave " + MedalCatalog.FarDriftClearsAtWave,
                MedalCatalog.Title(MedalId.FarDrift),
                MedalCatalog.FarDriftClearsAtWave);
        }

        public static bool ShowWaveMedal(int lastResolvedWave, GamePhase phase)
        {
            if (lastResolvedWave == MedalCatalog.ScoutWingClearsAtWave)
            {
                return phase == GamePhase.WaveClear;
            }

            if (lastResolvedWave == CampaignCap.FinalWave)
            {
                return phase == GamePhase.CampaignClear;
            }

            if (lastResolvedWave == World2StartsAtWave)
            {
                return phase == GamePhase.WaveClear || phase == GamePhase.Failed;
            }

            if (lastResolvedWave == MedalCatalog.FarDriftClearsAtWave)
            {
                return phase == GamePhase.WaveClear;
            }

            if (lastResolvedWave == World3StartsAtWave)
            {
                return phase == GamePhase.WaveClear || phase == GamePhase.Failed;
            }

            return false;
        }

        public static bool IsWorld3EntryLine(int lastResolvedWave)
        {
            return lastResolvedWave == World3StartsAtWave;
        }

        public static string WaveMedal(int lastResolvedWave)
        {
            if (lastResolvedWave == MedalCatalog.ScoutWingClearsAtWave)
            {
                return MedalCatalog.AwardLine(MedalId.ScoutWing)
                    + "  ·  " + Loc.Tf("run.world2_at", "World 2 at wave {0}", World2StartsAtWave);
            }

            if (lastResolvedWave == CampaignCap.FinalWave)
            {
                return CampaignCap.WinLine();
            }

            if (lastResolvedWave == World2StartsAtWave)
            {
                return MedalCatalog.AwardLine(MedalId.DeepOrbit)
                    + "  ·  " + Loc.Tf("run.world3_at", "World 3 at wave {0}", World3StartsAtWave);
            }

            if (lastResolvedWave == MedalCatalog.FarDriftClearsAtWave)
            {
                return MedalCatalog.AwardLine(MedalId.FarDrift)
                    + "  ·  " + Loc.Tf("run.world3_at", "World 3 at wave {0}", World3StartsAtWave);
            }

            if (lastResolvedWave == World3StartsAtWave)
            {
                return MedalCatalog.World3HangarLine();
            }

            return string.Empty;
        }

        /// <summary>
        /// First-run / hangar carrot so the medal ladder has a next step in the first ~5 minutes.
        /// </summary>
        public static string NextMedalHook(int nextWave)
        {
            if (nextWave <= MedalCatalog.ScoutWingClearsAtWave)
            {
                return Loc.Tf(
                    "run.next_medal",
                    "Next  ·  ★ " + MedalCatalog.ScoutWingTitle
                        + " at wave " + MedalCatalog.ScoutWingClearsAtWave,
                    MedalCatalog.Title(MedalId.ScoutWing),
                    MedalCatalog.ScoutWingClearsAtWave);
            }

            if (nextWave <= CampaignCap.FinalWave)
            {
                return Loc.Tf(
                    "run.next_sector",
                    "Next  ·  SECTOR CLEAR at wave {0}",
                    CampaignCap.FinalWave);
            }

            if (nextWave <= World2StartsAtWave)
            {
                return Loc.Tf(
                    "run.next_medal",
                    "Next  ·  ★ " + MedalCatalog.DeepOrbitTitle
                        + " at wave " + World2StartsAtWave,
                    MedalCatalog.Title(MedalId.DeepOrbit),
                    World2StartsAtWave);
            }

            if (nextWave <= MedalCatalog.FarDriftClearsAtWave)
            {
                return Loc.Tf(
                    "run.next_medal",
                    "Next  ·  ★ " + MedalCatalog.FarDriftTitle
                        + " at wave " + MedalCatalog.FarDriftClearsAtWave,
                    MedalCatalog.Title(MedalId.FarDrift),
                    MedalCatalog.FarDriftClearsAtWave);
            }

            if (nextWave <= World3StartsAtWave)
            {
                return Loc.Tf("run.next_world3", "Next  ·  World 3 at wave {0}", World3StartsAtWave);
            }

            return string.Empty;
        }

        public static string NextUnlockLandmark(int lastResolvedWave)
        {
            if (lastResolvedWave == 2)
            {
                return Loc.T("run.gunner_wave4", "Gunner at wave 4");
            }

            if (lastResolvedWave == 3)
            {
                return Loc.T("run.before_gunner", "before Gunner");
            }

            if (lastResolvedWave == 4 || lastResolvedWave == 5)
            {
                return DeepOrbitTeaser(lastResolvedWave);
            }

            if (lastResolvedWave >= 7 && lastResolvedWave <= 9)
            {
                return FarDriftTeaser(lastResolvedWave);
            }

            if (lastResolvedWave == 1)
            {
                return Loc.Tf(
                    "run.star_at",
                    "★ " + MedalCatalog.ScoutWingTitle + " at wave "
                        + MedalCatalog.ScoutWingClearsAtWave,
                    MedalCatalog.Title(MedalId.ScoutWing),
                    MedalCatalog.ScoutWingClearsAtWave);
            }

            return Loc.Tf("run.world2_at", "World 2 at wave {0}", World2StartsAtWave);
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
