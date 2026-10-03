namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Last device the player used. Prompts switch; the skip line always names both.
    /// </summary>
    public static class ControlLabels
    {
        public static bool PreferPad { get; private set; }

        public static void NotePad()
        {
            PreferPad = true;
        }

        public static void NoteKeyboard()
        {
            PreferPad = false;
        }
    }

    /// <summary>
    /// Guided first wave. Prompts advance one step at a time.
    /// </summary>
    public sealed class TutorialRun
    {
        public const int PromptMove = 0;
        public const int PromptFire = 1;
        public const int PromptPickup = 2;
        public const int PromptFinish = 3;

        public bool Active { get; private set; }

        public int Prompt { get; private set; }

        public bool Moved { get; private set; }

        public bool Fired { get; private set; }

        public bool Picked { get; private set; }

        public void Begin()
        {
            Active = true;
            Prompt = PromptMove;
            Moved = false;
            Fired = false;
            Picked = false;
        }

        public void Clear()
        {
            Active = false;
            Prompt = PromptMove;
            Moved = false;
            Fired = false;
            Picked = false;
        }

        public void NoteMove()
        {
            if (!Active || Prompt != PromptMove)
            {
                return;
            }

            Moved = true;
            Prompt = PromptFire;
        }

        public void NoteFire()
        {
            if (!Active || Prompt != PromptFire)
            {
                return;
            }

            Fired = true;
            Prompt = PromptPickup;
        }

        public void NotePickup()
        {
            Picked = true;
            if (!Active || Prompt != PromptPickup)
            {
                return;
            }

            Prompt = PromptFinish;
        }

        public void NoteThreats(int threatsLeft)
        {
            if (!Active || Prompt != PromptPickup)
            {
                return;
            }

            if (Picked || threatsLeft <= 1)
            {
                Prompt = PromptFinish;
            }
        }
    }

    /// <summary>
    /// First-run decisions that must stay out of the MonoBehaviours Roslyn does not compile.
    /// </summary>
    public static class FirstRunRules
    {
        public const int AsteroidCount = 3;
        public const float AsteroidSpeed = 1.1f;
        public const int WeakEnemyHp = 1;
        public const float WeakEnemySpeedScale = 0.35f;
        public const EnemyKind WeakEnemy = EnemyKind.Scout;

        /// <summary>
        /// A hangar visit takes a run id before any wave, so NextRunId is not "played".
        /// </summary>
        public static bool HasRunOrHighscore(MetaData meta)
        {
            if (meta == null)
            {
                return false;
            }

            if (meta.LegacyPoints > 0)
            {
                return true;
            }

            if (meta.CreditPerk > 0 || meta.DiscountPerk > 0 || meta.ShieldPerk > 0 || meta.HullPerk > 0)
            {
                return true;
            }

            if (meta.BestScore0 > 0 || meta.BestWave0 > 0 || meta.BestWorld0 > 0)
            {
                return true;
            }

            if (meta.BestScore1 > 0 || meta.BestWave1 > 0 || meta.BestWorld1 > 0)
            {
                return true;
            }

            if (meta.BestScore2 > 0 || meta.BestWave2 > 0 || meta.BestWorld2 > 0)
            {
                return true;
            }

            if (!string.IsNullOrEmpty(meta.Awarded))
            {
                return true;
            }

            if (!string.IsNullOrEmpty(meta.BankedRuns))
            {
                return true;
            }

            return false;
        }

        public static bool HasLocalBest(int score, int wave)
        {
            return score > 0 || wave > 0;
        }

        public static void Migrate(MetaData meta, bool sawTutorial, bool sawDifficulty)
        {
            if (meta == null)
            {
                return;
            }

            if (!sawTutorial)
            {
                meta.TutorialDone = HasRunOrHighscore(meta) ? 1 : 0;
            }
            else
            {
                meta.TutorialDone = meta.TutorialDone != 0 ? 1 : 0;
            }

            if (!sawDifficulty)
            {
                meta.DifficultyChosen = 0;
            }
            else
            {
                meta.DifficultyChosen = meta.DifficultyChosen != 0 ? 1 : 0;
            }

            if (meta.Version >= 1 && meta.Version < LegacyProgress.CurrentVersion)
            {
                meta.Version = LegacyProgress.CurrentVersion;
            }
        }

        public static bool TutorialPending(MetaData meta)
        {
            return meta == null || meta.TutorialDone == 0;
        }

        public static void MarkTutorialDone(MetaData meta)
        {
            if (meta == null)
            {
                return;
            }

            meta.TutorialDone = 1;
        }

        public static void MarkDifficultyChosen(MetaData meta)
        {
            if (meta == null)
            {
                return;
            }

            meta.DifficultyChosen = 1;
        }

        /// <summary>
        /// Overlay only for a profile that never picked a grade and never played.
        /// </summary>
        public static bool ShowDifficultyChooser(int chosenFlag, bool prefsPresent, bool hasProgress)
        {
            if (chosenFlag != 0 || prefsPresent || hasProgress)
            {
                return false;
            }

            return true;
        }

        public static bool CountsForLegacy(bool tutorialActive)
        {
            return !tutorialActive;
        }

        public static bool CountsForHighscore(bool tutorialActive)
        {
            return !tutorialActive;
        }

        public static bool CountsForCampaignWave(bool tutorialActive)
        {
            return !tutorialActive;
        }

        public static bool CanFailRun(bool tutorialActive)
        {
            return !tutorialActive;
        }

        public const float TutorialEmptySeconds = 6f;

        /// <summary>
        /// Empty arena during the guided wave. Finish once the last prompt is up,
        /// or after a short wait if the threats vanished before the prompts ended.
        /// </summary>
        public static bool ShouldFinishEmptyTutorial(int threatsLeft, int prompt, float emptySeconds)
        {
            if (threatsLeft > 0)
            {
                return false;
            }

            if (prompt >= TutorialRun.PromptFinish)
            {
                return true;
            }

            return emptySeconds >= TutorialEmptySeconds;
        }

        public static float NextEmptySeconds(float emptySeconds, int threatsLeft, float deltaSeconds)
        {
            if (threatsLeft > 0)
            {
                return 0f;
            }

            float step = deltaSeconds > 0f ? deltaSeconds : 0f;
            float next = emptySeconds + step;
            if (next < 0f)
            {
                return 0f;
            }

            return next;
        }

        public static bool CompletesShieldPrompt(Pickup.Kind kind)
        {
            return kind == Pickup.Kind.Shield;
        }

        public static int LivesAfterTutorial(int livesAtStart)
        {
            if (livesAtStart < 1)
            {
                return DifficultySettings.StartLivesCount;
            }

            return livesAtStart;
        }

        public static DifficultyGrade GradeOnFirstSkip(bool pickedExplicit, DifficultyGrade picked)
        {
            if (!pickedExplicit)
            {
                return DifficultyGrade.Normal;
            }

            return picked;
        }

        public static bool FirstStartSkipStored(int tutorialDone, int difficultyChosen)
        {
            return tutorialDone != 0 && difficultyChosen != 0;
        }

        public static int TutorialFlagAfterContinue()
        {
            return 1;
        }

        public static string PromptLine(int prompt, bool pad)
        {
            if (prompt <= TutorialRun.PromptMove)
            {
                return pad
                    ? Loc.T("tut.move.pad", "Fly with the stick. Dodge rocks.")
                    : Loc.T("tut.move.key", "Fly with WASD. Dodge rocks.");
            }

            if (prompt == TutorialRun.PromptFire)
            {
                return pad
                    ? Loc.T("tut.fire.pad", "Fire with RT.")
                    : Loc.T("tut.fire.key", "Fire with mouse or Space.");
            }

            if (prompt == TutorialRun.PromptPickup)
            {
                return Loc.T("tut.pickup", "Grab the shield pickup.");
            }

            return Loc.T("tut.finish", "Clear the wave.");
        }

        public static string SkipHint()
        {
            return Loc.T("tut.skip_hint", "Skip tutorial  ·  Esc / Start");
        }

        public static string SkipTutorialLabel()
        {
            return Loc.T("ui.skip_tutorial", "Skip tutorial");
        }

        public static string FirstStartLabel()
        {
            return Loc.T("ui.first_start", "Start");
        }

        public static string FirstDifficultyTitle()
        {
            return Loc.T("ui.first_diff_title", "Choose difficulty");
        }

        public static string EasyLine()
        {
            return Loc.T("ui.diff.easy_line", "Fewer rocks, gentler hits.");
        }

        public static string NormalLine()
        {
            return Loc.T("ui.diff.normal_line", "The intended fight.");
        }

        public static string HardLine()
        {
            return Loc.T("ui.diff.hard_line", "More rocks, harder hits.");
        }

        public static string EasyChoiceLabel()
        {
            return DifficultySettings.Title(DifficultyGrade.Easy) + "\n" + EasyLine();
        }

        public static string NormalChoiceLabel()
        {
            return DifficultySettings.Title(DifficultyGrade.Normal) + "\n" + NormalLine();
        }

        public static string HardChoiceLabel()
        {
            return DifficultySettings.Title(DifficultyGrade.Hard) + "\n" + HardLine();
        }

        public static bool OneMoreTryIsPrimary(GamePhase phase)
        {
            return phase == GamePhase.Failed;
        }

        public static bool OneMoreTryNeedsConfirm()
        {
            return false;
        }

        public static DifficultyGrade DifficultyAfterRetry(DifficultyGrade current)
        {
            return current;
        }

        public static int LegacyGrants(bool tutorialActive, bool failed, bool alreadyGranted)
        {
            if (tutorialActive || !failed || alreadyGranted)
            {
                return 0;
            }

            return 1;
        }

        public static int LegacyGrantsOnRetry()
        {
            return 0;
        }

        public static string OneMoreTryLabel()
        {
            return Loc.T("ui.one_more_try", "One more try");
        }

        public static string NewRunResetLabel()
        {
            return Loc.T("ui.new_run_reset", "New Run (reset)");
        }
    }
}
