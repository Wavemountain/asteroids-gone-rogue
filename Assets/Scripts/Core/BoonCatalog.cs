namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Run-long passives that already have a stat hook. Magnet range, shield
    /// recharge, and pickup luck are not here: nothing in the sim reads them.
    /// </summary>
    public enum BoonId
    {
        FireRate = 0,
        MoveSpeed = 1,
        MaxHull = 2,
        WaveCredits = 3,
        RailCharge = 4,
        ExtraShard = 5,
        DamageResist = 6,
        WaveShield = 7,
        UtilityCooldown = 8
    }

    /// <summary>
    /// Deterministic 1-of-3 draw. Unity-free. Max level is never offered.
    /// </summary>
    public static class BoonCatalog
    {
        public const int Count = 9;
        public const int MaxLevel = 3;
        public const int OfferCount = 3;
        public const int FireRatePerLevel = 8;
        public const int MovePerLevel = 10;
        public const int HullPerLevel = 1;
        public const int CreditPerLevel = 15;
        public const int RailPerLevel = 15;
        public const int ShardPerLevel = 1;
        public const int ResistPerLevel = 10;
        public const int ShieldPerLevel = 1;
        public const int UtilityPerLevel = 10;

        public static int[] Draw(int runSeed, int worldNumber, int[] levels)
        {
            int[] pool = new int[Count];
            int available = 0;
            for (int index = 0; index < Count; index++)
            {
                int level = LevelAt(levels, index);
                if (level >= MaxLevel)
                {
                    continue;
                }

                pool[available] = index;
                available++;
            }

            if (available <= 0)
            {
                return new int[0];
            }

            int state = MixSeed(runSeed, worldNumber);
            for (int cursor = available - 1; cursor > 0; cursor--)
            {
                state = Lcg(state);
                int pick = Positive(state) % (cursor + 1);
                int swap = pool[cursor];
                pool[cursor] = pool[pick];
                pool[pick] = swap;
            }

            int take = available < OfferCount ? available : OfferCount;
            int[] offer = new int[take];
            for (int slot = 0; slot < take; slot++)
            {
                offer[slot] = pool[slot];
            }

            return offer;
        }

        public static string Name(int id)
        {
            switch (id)
            {
                case (int)BoonId.MoveSpeed:
                    return Loc.T("boon.move", "Move speed");
                case (int)BoonId.MaxHull:
                    return Loc.T("boon.hull", "Max hull");
                case (int)BoonId.WaveCredits:
                    return Loc.T("boon.credits", "Wave credits");
                case (int)BoonId.RailCharge:
                    return Loc.T("boon.rail", "Rail charge");
                case (int)BoonId.ExtraShard:
                    return Loc.T("boon.shard", "Extra shard");
                case (int)BoonId.DamageResist:
                    return Loc.T("boon.resist", "Less damage");
                case (int)BoonId.WaveShield:
                    return Loc.T("boon.shield", "Wave shield");
                case (int)BoonId.UtilityCooldown:
                    return Loc.T("boon.utility", "Utility cooldown");
                default:
                    return Loc.T("boon.fire", "Fire rate");
            }
        }

        public static string Effect(int id)
        {
            switch (id)
            {
                case (int)BoonId.MoveSpeed:
                    return Loc.T("boon.move.fx", "+10% move speed");
                case (int)BoonId.MaxHull:
                    return Loc.T("boon.hull.fx", "+1 max hull");
                case (int)BoonId.WaveCredits:
                    return Loc.T("boon.credits.fx", "+15% wave credits");
                case (int)BoonId.RailCharge:
                    return Loc.T("boon.rail.fx", "+15% rail charge");
                case (int)BoonId.ExtraShard:
                    return Loc.T("boon.shard.fx", "+1 shard on split");
                case (int)BoonId.DamageResist:
                    return Loc.T("boon.resist.fx", "10% chance to ignore a hit");
                case (int)BoonId.WaveShield:
                    return Loc.T("boon.shield.fx", "+1 shield each wave");
                case (int)BoonId.UtilityCooldown:
                    return Loc.T("boon.utility.fx", "-10% utility cooldown");
                default:
                    return Loc.T("boon.fire.fx", "+8% fire rate");
            }
        }

        /// <summary>
        /// Short hangar token. The full effect stays on the picker card.
        /// Nine stacks must fit the hangar row at 1280×800.
        /// </summary>
        public static string Tag(int id)
        {
            switch ((BoonId)id)
            {
                case BoonId.MoveSpeed:
                    return Loc.T("boon.move.tag", "Move");
                case BoonId.MaxHull:
                    return Loc.T("boon.hull.tag", "Hull");
                case BoonId.WaveCredits:
                    return Loc.T("boon.credits.tag", "Cred");
                case BoonId.RailCharge:
                    return Loc.T("boon.rail.tag", "Rail");
                case BoonId.ExtraShard:
                    return Loc.T("boon.shard.tag", "Shard");
                case BoonId.DamageResist:
                    return Loc.T("boon.resist.tag", "Dmg");
                case BoonId.WaveShield:
                    return Loc.T("boon.shield.tag", "Shld");
                case BoonId.UtilityCooldown:
                    return Loc.T("boon.utility.tag", "Util");
                default:
                    return Loc.T("boon.fire.tag", "Fire");
            }
        }

        /// <summary>
        /// 10% per level to ignore a hit. The roll is a seeded hash of the run
        /// and the hit index, so tests can pin hit 0.
        /// </summary>
        public static bool IgnoresHit(int level, int runSeed, int hitIndex)
        {
            int rank = level;
            if (rank < 0)
            {
                rank = 0;
            }

            if (rank > MaxLevel)
            {
                rank = MaxLevel;
            }

            if (rank <= 0)
            {
                return false;
            }

            int chance = rank * ResistPerLevel;
            if (chance > 100)
            {
                chance = 100;
            }

            int roll = HitRoll(runSeed, hitIndex) % 100;
            if (roll < 0)
            {
                roll = -roll;
            }

            return roll < chance;
        }

        public static int HitRoll(int runSeed, int hitIndex)
        {
            int state = runSeed;
            if (state < 0)
            {
                state = -state;
            }

            int index = hitIndex < 0 ? 0 : hitIndex;
            state = state * 1103515245 + 12345;
            state = state + index * 97;
            state = state * 1664525 + 1013904223;
            if (state == int.MinValue)
            {
                return 0;
            }

            if (state < 0)
            {
                state = -state;
            }

            return state;
        }

        public static string OwnedLabel(int id, int level)
        {
            int shown = level;
            if (shown < 1)
            {
                shown = 1;
            }

            if (shown > MaxLevel)
            {
                shown = MaxLevel;
            }

            return Loc.Tf("boon.stack", "{0} x{1}", Tag(id), shown);
        }

        public static string CardLine(int id, int ownedLevel)
        {
            string body = Name(id) + "\n" + Effect(id);
            if (ownedLevel > 0)
            {
                int next = ownedLevel + 1;
                if (next > MaxLevel)
                {
                    next = MaxLevel;
                }

                body += "\n" + Loc.Tf("boon.level", "Lv {0}", next);
            }

            return body;
        }

        public static int PackLevels(int[] levels)
        {
            int packed = 0;
            for (int index = 0; index < Count; index++)
            {
                int level = LevelAt(levels, index);
                if (level > MaxLevel)
                {
                    level = MaxLevel;
                }

                packed |= (level & 3) << (index * 2);
            }

            return packed;
        }

        public static void UnpackLevels(int packed, int[] levels)
        {
            if (levels == null)
            {
                return;
            }

            for (int index = 0; index < levels.Length && index < Count; index++)
            {
                levels[index] = (packed >> (index * 2)) & 3;
            }
        }

        public static int PackOffer(int[] offer)
        {
            int packed = 0;
            if (offer == null)
            {
                return 0;
            }

            int count = offer.Length;
            if (count > OfferCount)
            {
                count = OfferCount;
            }

            for (int slot = 0; slot < count; slot++)
            {
                int id = offer[slot];
                if (id < 0 || id >= Count)
                {
                    id = 0;
                }

                packed |= (id & 15) << (slot * 4);
            }

            return packed;
        }

        public static bool LevelsValid(int packed)
        {
            int limit = 1 << (Count * 2);
            return packed >= 0 && packed < limit;
        }

        public static bool OfferValid(int packed, int pending, int levelsPacked)
        {
            if (pending == 0)
            {
                return packed == 0;
            }

            if (packed < 0 || packed >= (1 << (OfferCount * 4)))
            {
                return false;
            }

            int[] levels = new int[Count];
            UnpackLevels(levelsPacked, levels);
            int[] seen = new int[Count];
            for (int slot = 0; slot < OfferCount; slot++)
            {
                int id = (packed >> (slot * 4)) & 15;
                if (id < 0 || id >= Count)
                {
                    return false;
                }

                if (seen[id] != 0)
                {
                    return false;
                }

                seen[id] = 1;
                if (levels[id] >= MaxLevel)
                {
                    return false;
                }
            }

            return true;
        }

        public static int LevelAt(int[] levels, int index)
        {
            if (levels == null || index < 0 || index >= levels.Length)
            {
                return 0;
            }

            int level = levels[index];
            if (level < 0)
            {
                return 0;
            }

            return level;
        }

        public static int MixSeed(int runSeed, int worldNumber)
        {
            int seed = runSeed < 1 ? 1 : runSeed;
            int world = worldNumber < 1 ? 1 : worldNumber;
            int mixed = seed * 73856093 + world * 19349663 + 83492791;
            if (mixed == 0)
            {
                mixed = 1;
            }

            return mixed;
        }

        public static int Lcg(int state)
        {
            return unchecked(state * 1103515245 + 12345);
        }

        public static int Positive(int state)
        {
            if (state == int.MinValue)
            {
                return 1;
            }

            int abs = state < 0 ? -state : state;
            return abs == 0 ? 1 : abs;
        }
    }

    /// <summary>
    /// Chosen levels plus a pending 3-card offer. Cleared on a new run or a fail.
    /// </summary>
    public sealed class BoonRun
    {
        private readonly int[] _levels = new int[BoonCatalog.Count];
        private readonly int[] _offer = new int[BoonCatalog.OfferCount];

        public bool Pending { get; private set; }

        public int OfferCount { get; private set; }

        public int[] Levels
        {
            get { return _levels; }
        }

        public int OfferAt(int index)
        {
            if (index < 0 || index >= OfferCount || index >= _offer.Length)
            {
                return -1;
            }

            return _offer[index];
        }

        public void Clear()
        {
            for (int index = 0; index < _levels.Length; index++)
            {
                _levels[index] = 0;
            }

            Pending = false;
            OfferCount = 0;
            for (int slot = 0; slot < _offer.Length; slot++)
            {
                _offer[slot] = 0;
            }
        }

        public void SetPending(int[] offer)
        {
            if (offer == null || offer.Length < BoonCatalog.OfferCount)
            {
                Pending = false;
                OfferCount = 0;
                return;
            }

            OfferCount = BoonCatalog.OfferCount;
            for (int slot = 0; slot < OfferCount; slot++)
            {
                _offer[slot] = offer[slot];
            }

            Pending = true;
        }

        public bool TryChoose(int index)
        {
            if (!Pending || index < 0 || index >= OfferCount)
            {
                return false;
            }

            int id = _offer[index];
            if (id < 0 || id >= BoonCatalog.Count)
            {
                return false;
            }

            if (_levels[id] >= BoonCatalog.MaxLevel)
            {
                return false;
            }

            _levels[id] += 1;
            Pending = false;
            OfferCount = 0;
            return true;
        }

        public void ReadSave(int packedLevels, int pending, int packedOffer)
        {
            BoonCatalog.UnpackLevels(packedLevels, _levels);
            Pending = pending == 1;
            OfferCount = 0;
            if (!Pending)
            {
                return;
            }

            OfferCount = BoonCatalog.OfferCount;
            for (int slot = 0; slot < OfferCount; slot++)
            {
                _offer[slot] = (packedOffer >> (slot * 4)) & 15;
            }
        }

        public int PackedLevels()
        {
            return BoonCatalog.PackLevels(_levels);
        }

        public int PackedOffer()
        {
            if (!Pending)
            {
                return 0;
            }

            return BoonCatalog.PackOffer(_offer);
        }
    }

    /// <summary>
    /// Gameplay reads these. GameManager syncs them from the live BoonRun.
    /// Defaults leave every stat on today's numbers.
    /// </summary>
    public static class BoonHooks
    {
        public static int FireRatePercent = 100;
        public static int MovePercent = 100;
        public static int HullBonus = 0;
        public static int CreditPercent = 100;
        public static int RailChargePercent = 100;
        public static int ExtraShards = 0;
        public static int IncomingPercent = 100;
        public static int StartingShield = 0;
        public static int UtilityCooldownPercent = 100;
        public static int ResistLevel = 0;
        public static int RunSeed = 1;
        public static int HitCounter = 0;

        public static void Reset()
        {
            FireRatePercent = 100;
            MovePercent = 100;
            HullBonus = 0;
            CreditPercent = 100;
            RailChargePercent = 100;
            ExtraShards = 0;
            IncomingPercent = 100;
            StartingShield = 0;
            UtilityCooldownPercent = 100;
            ResistLevel = 0;
        }

        public static void ResetRunCounters()
        {
            HitCounter = 0;
            RunSeed = 1;
        }

        public static bool TryIgnoreHit()
        {
            int index = HitCounter < 0 ? 0 : HitCounter;
            HitCounter = index + 1;
            return BoonCatalog.IgnoresHit(ResistLevel, RunSeed, index);
        }

        public static void Sync(BoonRun run)
        {
            Reset();
            if (run == null)
            {
                return;
            }

            int[] levels = run.Levels;
            FireRatePercent = 100 + Level(levels, BoonId.FireRate) * BoonCatalog.FireRatePerLevel;
            MovePercent = 100 + Level(levels, BoonId.MoveSpeed) * BoonCatalog.MovePerLevel;
            HullBonus = Level(levels, BoonId.MaxHull) * BoonCatalog.HullPerLevel;
            CreditPercent = 100 + Level(levels, BoonId.WaveCredits) * BoonCatalog.CreditPerLevel;
            RailChargePercent = 100 + Level(levels, BoonId.RailCharge) * BoonCatalog.RailPerLevel;
            ExtraShards = Level(levels, BoonId.ExtraShard) * BoonCatalog.ShardPerLevel;
            IncomingPercent = 100;
            ResistLevel = Level(levels, BoonId.DamageResist);
            StartingShield = Level(levels, BoonId.WaveShield) * BoonCatalog.ShieldPerLevel;
            UtilityCooldownPercent = 100 - (Level(levels, BoonId.UtilityCooldown) * BoonCatalog.UtilityPerLevel);
        }

        public static int ScaleCredits(int credits)
        {
            if (credits < 0)
            {
                credits = 0;
            }

            return credits * CreditPercent / 100;
        }

        private static int Level(int[] levels, BoonId id)
        {
            int level = BoonCatalog.LevelAt(levels, (int)id);
            if (level > BoonCatalog.MaxLevel)
            {
                return BoonCatalog.MaxLevel;
            }

            return level;
        }
    }
}
