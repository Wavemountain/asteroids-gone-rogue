namespace AsteroidsGoneRogue
{
    /// <summary>
    /// First-run prompt cards. One card at a time, once per action, dismissed
    /// by doing that action. Dash is offered only when a boost upgrade is fitted.
    /// The seen mask lives in settings. A missing key on an existing profile
    /// migrates to all-seen so a veteran is not interrupted. A new profile
    /// (settings version below 1) starts unseen. Reset stores zero.
    /// </summary>
    public static class TutorialPrompt
    {
        public const int Move = 0;
        public const int Fire = 1;
        public const int Utility = 2;
        public const int Dash = 3;
        public const int Buy = 4;
        public const int Equip = 5;
        public const int Pause = 6;
        public const int Count = 7;
        public const int None = -1;
        public const int AllSeen = (1 << Count) - 1;

        public const float AnchorMinX = 0.28f;
        public const float AnchorMinY = 0.02f;
        public const float AnchorMaxX = 0.72f;
        public const float AnchorMaxY = 0.14f;
        public const int BodySize = 18;

        public struct Context
        {
            public bool Playing;
            public bool Shop;
            public bool Blocked;
            public bool UtilityPresent;
            public bool DashPresent;
        }

        public static int Normalize(int mask)
        {
            if (mask < 0)
            {
                return 0;
            }

            return mask & AllSeen;
        }

        public static int Clear()
        {
            return 0;
        }

        public static bool Seen(int mask, int id)
        {
            if (id < 0 || id >= Count)
            {
                return true;
            }

            return (Normalize(mask) & (1 << id)) != 0;
        }

        public static int Mark(int mask, int id)
        {
            if (id < 0 || id >= Count)
            {
                return Normalize(mask);
            }

            return Normalize(mask) | (1 << id);
        }

        /// <summary>
        /// Missing key plus a saved settings version means the player already
        /// has a profile. Treat every card as seen. Version below 1 is a
        /// first launch and stays unseen. A stored mask, including zero after
        /// reset, is kept.
        /// </summary>
        public static int Migrate(int settingsVersion, bool keyPresent, int stored)
        {
            if (keyPresent)
            {
                return Normalize(stored);
            }

            if (settingsVersion >= 1)
            {
                return AllSeen;
            }

            return 0;
        }

        public static bool DashAvailable(bool rapidFire, bool afterburner)
        {
            return rapidFire || afterburner;
        }

        public static bool InPlay(GamePhase phase)
        {
            return phase == GamePhase.Playing;
        }

        public static bool InShop(GamePhase phase)
        {
            return phase == GamePhase.Hangar
                || phase == GamePhase.WaveClear
                || phase == GamePhase.CampaignClear;
        }

        public static int NotePlay(
            int mask,
            bool moved,
            bool fired,
            bool firePressed,
            bool utilityUsed,
            bool paused,
            bool utilityPresent,
            bool dashPresent)
        {
            int next = Normalize(mask);
            if (moved)
            {
                next = Mark(next, Move);
            }

            bool fireAlready = Seen(next, Fire);
            if (fired)
            {
                next = Mark(next, Fire);
            }

            if (firePressed && fireAlready && dashPresent)
            {
                next = Mark(next, Dash);
            }

            if (utilityUsed && utilityPresent)
            {
                next = Mark(next, Utility);
            }

            if (paused)
            {
                next = Mark(next, Pause);
            }

            return next;
        }

        public static int NoteBuy(int mask)
        {
            return Mark(mask, Buy);
        }

        public static int NoteEquip(int mask)
        {
            return Mark(mask, Equip);
        }

        public static int Current(int mask, Context ctx)
        {
            if (ctx.Blocked)
            {
                return None;
            }

            if (ctx.Playing)
            {
                int playMove = Offer(mask, Move, true);
                if (playMove != None)
                {
                    return playMove;
                }

                int playFire = Offer(mask, Fire, true);
                if (playFire != None)
                {
                    return playFire;
                }

                int playUtility = Offer(mask, Utility, ctx.UtilityPresent);
                if (playUtility != None)
                {
                    return playUtility;
                }

                int playDash = Offer(mask, Dash, ctx.DashPresent);
                if (playDash != None)
                {
                    return playDash;
                }

                return Offer(mask, Pause, true);
            }

            if (ctx.Shop)
            {
                int shopBuy = Offer(mask, Buy, true);
                if (shopBuy != None)
                {
                    return shopBuy;
                }

                return Offer(mask, Equip, true);
            }

            return None;
        }

        public static string Token(int id)
        {
            if (id == Move)
            {
                return "move";
            }

            if (id == Fire || id == Dash)
            {
                return "fire";
            }

            if (id == Utility)
            {
                return "utility";
            }

            if (id == Buy || id == Equip)
            {
                return "confirm";
            }

            if (id == Pause)
            {
                return "pause";
            }

            return string.Empty;
        }

        private static int Offer(int mask, int id, bool present)
        {
            if (!present || Seen(mask, id))
            {
                return None;
            }

            return id;
        }
    }
}
