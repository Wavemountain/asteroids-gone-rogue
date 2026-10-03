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

        /// <summary>
        /// Names shown on the wave-clear upgrades row before "+K more".
        /// Seven keeps a fully upgraded EN/SV line inside two wrapped lines
        /// of the hangar card from 1280x800 through 3440x1440.
        /// </summary>
        public const int UpgradesLineMaxShown = 7;
        public const string Wave3MedalTitle = "Scout Wing";

        public static string RunOverTitle(int wave)
        {
            int shown = wave < 1 ? 1 : wave;
            return Loc.Tf("run.over_title", "RUN OVER - wave {0}", shown);
        }

        public static string RunOverExplain()
        {
            return Loc.T(
                "run.over_explain",
                "Your ship, upgrades and credits reset on New Run.");
        }

        public static string PrimaryActionLabel(GamePhase phase, int worldCleared, int nextWorld)
        {
            if (phase == GamePhase.Failed)
            {
                return Loc.T("ui.new_run_reset", "New Run (reset)");
            }

            if (phase == GamePhase.WaveClear && worldCleared > 0)
            {
                int world = nextWorld < 1 ? worldCleared + 1 : nextWorld;
                return Loc.Tf("ui.continue_world", "Continue to World {0}", world);
            }

            if (phase == GamePhase.WaveClear || phase == GamePhase.CampaignClear)
            {
                return Loc.T("ui.next_wave", "Next Wave");
            }

            return Loc.T("ui.start_wave", "Start Wave");
        }

        public static string ContinueRunLabel(int worldNumber, int waveIndex)
        {
            int world = worldNumber < 1 ? 1 : worldNumber;
            int wave = waveIndex < 1 ? 1 : waveIndex;
            return Loc.Tf("ui.continue_run", "Continue W{0} wave {1}", world, wave);
        }

        public const float PrimaryMinWidth = 900f;
        public const float PrimaryMinHeight = 48f;
        public const int PrimaryFont = 20;

        public static string ContinueSubtitle(int waveIndex)
        {
            string line = WorldCatalog.ContinueSubtitle(waveIndex);
            int loopNumber = WorldCatalog.LoopForWave(waveIndex);
            if (loopNumber > WorldCatalog.FirstLoop)
            {
                line += "  ·  " + Loc.Tf("world.loop", "Loop {0}", loopNumber);
            }

            return line;
        }

        public static string ContinueRuleLine(int waveIndex)
        {
            return WorldRules.ShortLineForWave(waveIndex);
        }

        public static string BoonLine(int[] levels)
        {
            if (levels == null)
            {
                return string.Empty;
            }

            string joined = string.Empty;
            int cap = levels.Length < BoonCatalog.Count ? levels.Length : BoonCatalog.Count;
            for (int index = 0; index < cap; index++)
            {
                if (levels[index] <= 0)
                {
                    continue;
                }

                string piece = BoonCatalog.OwnedLabel(index, levels[index]);
                if (string.IsNullOrEmpty(joined))
                {
                    joined = piece;
                }
                else
                {
                    joined += " · " + piece;
                }
            }

            if (string.IsNullOrEmpty(joined))
            {
                return string.Empty;
            }

            return Loc.Tf("boon.row", "Bonuses  {0}", joined);
        }

        public static bool LineFits(string text, float boxWidth, int fontSize)
        {
            if (string.IsNullOrEmpty(text))
            {
                return true;
            }

            if (boxWidth <= 0f || fontSize <= 0)
            {
                return false;
            }

            float width = text.Length * 10f * fontSize / 18f;
            return width <= boxWidth;
        }

        /// <summary>
        /// Second line on Continue-to-World. Skip when the label plus subtitle
        /// would leave the button (narrowest hangar CTA is about 900×48).
        /// </summary>
        public static bool PrimarySubtitleFits(string label, string subtitle)
        {
            if (string.IsNullOrEmpty(subtitle))
            {
                return false;
            }

            if (!LineFits(label, PrimaryMinWidth, PrimaryFont))
            {
                return false;
            }

            if (!LineFits(subtitle, PrimaryMinWidth, PrimaryFont))
            {
                return false;
            }

            return PrimaryFont * 2f <= PrimaryMinHeight;
        }

        public static string ReachedLine(int wave)
        {
            int shownWave = wave < 1 ? 1 : wave;
            int worldNumber = WorldCatalog.NumberForWave(shownWave);
            string line = Loc.Tf(
                "run.reached",
                "Reached World {0}, wave {1}",
                worldNumber,
                shownWave);
            int loopNumber = WorldCatalog.LoopForWave(shownWave);
            if (loopNumber > WorldCatalog.FirstLoop)
            {
                line += "  ·  " + Loc.Tf("world.loop", "Loop {0}", loopNumber);
            }

            return line;
        }

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
                return CampaignCap.SectorClearTitle(CampaignCap.FinalWorld);
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

        /// <summary>
        /// Death card line, e.g. "Lance run — wave 7". Empty when there is no
        /// doctrine, or the ship was lost on wave 1 (before that wave cleared).
        /// Wave index only advances after a clear, so wave 1 is that case.
        /// </summary>
        public static string DoctrineRunLine(DoctrineId doctrine, int deathWave)
        {
            if (doctrine == DoctrineId.None || deathWave < DoctrineRules.UnlockWave)
            {
                return string.Empty;
            }

            return Loc.Tf(
                "run.doctrine_wave",
                "{0} run — wave {1}",
                DoctrineDisplay(doctrine),
                deathWave);
        }

        public static string DoctrineDisplay(DoctrineId doctrine)
        {
            switch (doctrine)
            {
                case DoctrineId.Barrage:
                    return Loc.T("ui.doctrine.barrage", "Barrage");
                case DoctrineId.Lance:
                    return Loc.T("ui.doctrine.lance", "Lance");
                case DoctrineId.Hunter:
                    return Loc.T("ui.doctrine.hunter", "Hunter");
                default:
                    return string.Empty;
            }
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
            int shown = 0;
            int hidden = 0;
            NoteOwned(ref names, ref shown, ref hidden, loadout.BodyUpgrade01, Loc.T("up.Body", "Body"));
            NoteOwned(ref names, ref shown, ref hidden, loadout.BodyUpgrade02, Loc.T("up.Hull02", "Hull 02"));
            NoteOwned(ref names, ref shown, ref hidden, loadout.NoseHardpoint, Loc.T("up.Nose", "Nose"));
            NoteOwned(ref names, ref shown, ref hidden, loadout.NoseUpgrade02, Loc.T("up.Nose02", "Nose 02"));
            NoteOwned(ref names, ref shown, ref hidden, loadout.NoseUpgrade03, Loc.T("up.Nose03", "Nose 03"));
            NoteOwned(ref names, ref shown, ref hidden, loadout.RapidFire, Loc.T("up.Rapid", "Rapid Fire"));
            NoteOwned(ref names, ref shown, ref hidden, loadout.EngineUpgrade02, Loc.T("up.Engine02", "Engine 02"));
            NoteOwned(ref names, ref shown, ref hidden, loadout.EngineUpgrade03, Loc.T("up.Engine03", "Engine 03"));
            NoteOwned(ref names, ref shown, ref hidden, loadout.Overcharger, Loc.T("up.Overcharger", "Overcharger"));
            NoteOwned(ref names, ref shown, ref hidden, loadout.Afterburner, Loc.T("up.Afterburner", "Afterburner"));
            NoteOwned(ref names, ref shown, ref hidden, loadout.SpreadBolt, Loc.T("up.Spread", "Spread"));
            NoteOwned(ref names, ref shown, ref hidden, loadout.Pierce, Loc.T("up.Pierce", "Pierce"));
            NoteOwned(ref names, ref shown, ref hidden, loadout.TwinGuns, Loc.T("up.Twin", "Twin"));
            NoteOwned(ref names, ref shown, ref hidden, loadout.Seeker, Loc.T("up.Seeker", "Seeker"));
            NoteOwned(ref names, ref shown, ref hidden, loadout.Ricochet, Loc.T("up.Ricochet", "Ricochet"));
            NoteOwned(ref names, ref shown, ref hidden, loadout.Rail, Loc.T("up.Rail", "Rail"));
            NoteOwned(ref names, ref shown, ref hidden, loadout.FlakFeed, Loc.T("up.FlakFeed", "Flak Feed"));
            NoteOwned(ref names, ref shown, ref hidden, loadout.Storm, Loc.T("up.Storm", "Storm"));
            NoteOwned(ref names, ref shown, ref hidden, loadout.OverchargeLance, Loc.T("up.Overcharge", "Overcharge Lance"));
            NoteOwned(ref names, ref shown, ref hidden, loadout.SeekerCadence, Loc.T("up.Cadence", "Seeker Cadence"));
            NoteOwned(ref names, ref shown, ref hidden, loadout.TwinSeek, Loc.T("up.TwinSeek", "Twin Seek"));
            if (loadout.ShieldCharges > 0)
            {
                NoteOwned(ref names, ref shown, ref hidden, true, Loc.Tf("up.Shield", "Shield x{0}", loadout.ShieldCharges));
            }

            NoteOwned(ref names, ref shown, ref hidden, loadout.ShieldMatrix, Loc.T("up.Matrix", "Matrix"));
            if (hidden > 0)
            {
                AppendOwned(ref names, true, Loc.Tf("run.upgrades_more", "+{0} more", hidden));
            }

            return string.IsNullOrEmpty(names)
                ? Loc.T("run.upgrades_none", "Upgrades —")
                : Loc.Tf("run.upgrades", "Upgrades  {0}", names);
        }

        private static void NoteOwned(ref string names, ref int shown, ref int hidden, bool owned, string label)
        {
            if (!owned)
            {
                return;
            }

            if (shown >= UpgradesLineMaxShown)
            {
                hidden += 1;
                return;
            }

            AppendOwned(ref names, true, label);
            shown += 1;
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

            return phase == GamePhase.WaveClear && lastResolvedWave >= 1;
        }

        public static bool ShowFailContinue(GamePhase phase)
        {
            return phase == GamePhase.Failed;
        }

        public static string CampaignWinHint()
        {
            return CampaignCap.SectorClearTitle(CampaignCap.FinalWorld);
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
                    "Next  ·  World 2 after wave {0}",
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

            if (lastResolvedWave >= 10)
            {
                string reached = ReachedLine(lastResolvedWave);
                return next != null
                    ? reached + "  ·  " + Loc.Tf("run.buy", "Buy {0}", next.Title)
                    : reached;
            }

            string buy = next != null
                ? Loc.Tf("run.buy", "Buy {0}", next.Title)
                : Loc.T("run.push_best", "Push for a new best.");
            if (lastResolvedWave == 1)
            {
                string seekerTip = loadout != null && loadout.Seeker
                    ? Loc.T("run.hold_lt", "Hold LT to fire utility")
                    : Loc.T("run.buy_seeker_lt", "Buy Seeker > hold LT");
                return NextUnlockLandmark(lastResolvedWave) + "  ·  " + seekerTip;
            }

            return NextUnlockLandmark(lastResolvedWave) + "  ·  " + buy;
        }

        public static string DeepOrbitTeaser(int lastResolvedWave)
        {
            if (lastResolvedWave == 5)
            {
                return Loc.Tf(
                    "run.deep_orbit_now",
                    "World 2  ·  \u2022 " + MedalCatalog.DeepOrbitTitle,
                    MedalCatalog.Title(MedalId.DeepOrbit));
            }

            return Loc.Tf(
                "run.deep_orbit_at",
                "\u2022 " + MedalCatalog.DeepOrbitTitle + " at wave " + World2StartsAtWave,
                MedalCatalog.Title(MedalId.DeepOrbit),
                World2StartsAtWave);
        }

        public static string FarDriftTeaser(int lastResolvedWave)
        {
            if (lastResolvedWave == 9)
            {
                return Loc.Tf(
                    "run.far_drift_clear",
                    "Clear wave 10  ·  \u2022 " + MedalCatalog.FarDriftTitle,
                    MedalCatalog.Title(MedalId.FarDrift));
            }

            return Loc.Tf(
                "run.far_drift_at",
                "\u2022 " + MedalCatalog.FarDriftTitle + " at wave " + MedalCatalog.FarDriftClearsAtWave,
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

            if (lastResolvedWave == World3StartsAtWave)
            {
                return phase == GamePhase.WaveClear || phase == GamePhase.Failed;
            }

            if (lastResolvedWave == MedalCatalog.MineFieldsClearsAtWave
                || lastResolvedWave == MedalCatalog.CrossGatesClearsAtWave
                || lastResolvedWave == MedalCatalog.DebrisIslandsClearsAtWave
                || lastResolvedWave == MedalCatalog.SpokeRingClearsAtWave)
            {
                return phase == GamePhase.WaveClear;
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
                return WorldCatalog.ClearedMedal(WorldCatalog.NumberForWave(lastResolvedWave));
            }

            if (lastResolvedWave == World2StartsAtWave)
            {
                return MedalCatalog.AwardLine(MedalId.DeepOrbit);
            }

            if (lastResolvedWave == MedalCatalog.FarDriftClearsAtWave)
            {
                return MedalCatalog.AwardLine(MedalId.FarDrift);
            }

            if (lastResolvedWave == World3StartsAtWave)
            {
                return MedalCatalog.World3HangarLine();
            }

            MedalId clearedMedal;
            if (MedalCatalog.TryForClearedWave(lastResolvedWave, out clearedMedal)
                && lastResolvedWave >= MedalCatalog.MineFieldsClearsAtWave)
            {
                return MedalCatalog.AwardLine(clearedMedal);
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
                    "Next  ·  \u2022 " + MedalCatalog.ScoutWingTitle
                        + " at wave " + MedalCatalog.ScoutWingClearsAtWave,
                    MedalCatalog.Title(MedalId.ScoutWing),
                    MedalCatalog.ScoutWingClearsAtWave);
            }

            if (nextWave <= CampaignCap.FinalWave)
            {
                return Loc.Tf(
                    "run.next_sector",
                    "Next  ·  World 2 after wave {0}",
                    CampaignCap.FinalWave);
            }

            if (nextWave <= World2StartsAtWave)
            {
                return Loc.Tf(
                    "run.next_medal",
                    "Next  ·  \u2022 " + MedalCatalog.DeepOrbitTitle
                        + " at wave " + World2StartsAtWave,
                    MedalCatalog.Title(MedalId.DeepOrbit),
                    World2StartsAtWave);
            }

            if (nextWave <= MedalCatalog.FarDriftClearsAtWave)
            {
                return Loc.Tf(
                    "run.next_medal",
                    "Next  ·  \u2022 " + MedalCatalog.FarDriftTitle
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
                    "\u2022 " + MedalCatalog.ScoutWingTitle + " at wave "
                        + MedalCatalog.ScoutWingClearsAtWave,
                    MedalCatalog.Title(MedalId.ScoutWing),
                    MedalCatalog.ScoutWingClearsAtWave);
            }

            if (lastResolvedWave >= World2StartsAtWave)
            {
                return ReachedLine(lastResolvedWave);
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
