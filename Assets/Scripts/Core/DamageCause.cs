namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Sources that can damage the player. Only causes that exist in play are listed.
    /// </summary>
    public enum DamageCause
    {
        Unknown,
        AsteroidCollision,
        EnemyContact,
        HazardContact,
        EnemyBolt,
        BossBolt
    }

    /// <summary>
    /// One recorded hit. Shield-only rows stay in the log but never become the killer.
    /// </summary>
    public struct DamageHit
    {
        public DamageCause Cause;
        public EnemyKind Kind;
        public int Amount;
        public int HullLost;
        public int Wave;
        public int World;
        public bool ShieldOnly;
        public bool Elite;
        public string Modifier;
    }

    /// <summary>
    /// Last three damage events. The killer is the newest hit that reached hull.
    /// Equal hull amounts still resolve to the later event.
    /// </summary>
    public sealed class DamageCauseLog
    {
        public const int Capacity = 3;

        private readonly DamageHit[] _items = new DamageHit[Capacity];
        private int _count;

        public int Count
        {
            get { return _count; }
        }

        public void Clear()
        {
            _count = 0;
        }

        public DamageHit At(int index)
        {
            if (index < 0 || index >= _count)
            {
                return default(DamageHit);
            }

            return _items[index];
        }

        public void Record(
            DamageCause cause,
            EnemyKind kind,
            int amount,
            int hullLost,
            int wave,
            int world,
            bool shieldOnly,
            bool elite,
            string modifier)
        {
            DamageHit hit = new DamageHit();
            hit.Cause = cause;
            hit.Kind = kind;
            hit.Amount = amount;
            hit.HullLost = hullLost;
            hit.Wave = wave;
            hit.World = world;
            hit.ShieldOnly = shieldOnly;
            hit.Elite = elite;
            hit.Modifier = modifier == null ? string.Empty : modifier;
            if (_count < Capacity)
            {
                _items[_count] = hit;
                _count += 1;
                return;
            }

            _items[0] = _items[1];
            _items[1] = _items[2];
            _items[2] = hit;
        }

        public bool TryKiller(out DamageHit killer)
        {
            for (int i = _count - 1; i >= 0; i--)
            {
                if (!_items[i].ShieldOnly && _items[i].HullLost > 0)
                {
                    killer = _items[i];
                    return true;
                }
            }

            killer = default(DamageHit);
            return false;
        }

        public string KillerLine()
        {
            DamageHit killer;
            if (!TryKiller(out killer))
            {
                return string.Empty;
            }

            string source = DamageCauseText.SourceLabel(killer.Cause, killer.Kind, killer.Elite, killer.Modifier);
            return Loc.Tf(
                "fail.killed",
                "Killed by: {0} (wave {1}, world {2}) — hull {3}",
                source,
                killer.Wave,
                killer.World,
                killer.HullLost);
        }

        public string LastHitsLine()
        {
            string joined = string.Empty;
            for (int i = _count - 1; i >= 0; i--)
            {
                if (_items[i].ShieldOnly || _items[i].HullLost <= 0)
                {
                    continue;
                }

                string source = DamageCauseText.SourceLabel(
                    _items[i].Cause,
                    _items[i].Kind,
                    _items[i].Elite,
                    _items[i].Modifier);
                string piece = source + " " + _items[i].HullLost.ToString();
                if (joined.Length == 0)
                {
                    joined = piece;
                }
                else
                {
                    joined = joined + "  ·  " + piece;
                }
            }

            if (joined.Length == 0)
            {
                return string.Empty;
            }

            return Loc.Tf("fail.last_hits", "Last hits: {0}", joined);
        }

        public string CardText()
        {
            string killer = KillerLine();
            if (killer.Length == 0)
            {
                return string.Empty;
            }

            string hits = LastHitsLine();
            if (hits.Length == 0)
            {
                return killer;
            }

            return killer + "\n" + hits;
        }
    }

    public static class DamageCauseText
    {
        public static string FailReason(DamageCause cause)
        {
            switch (cause)
            {
                case DamageCause.AsteroidCollision:
                    return Loc.T("fail.asteroid", "Asteroid collision");
                case DamageCause.EnemyContact:
                    return Loc.T("fail.enemy", "Enemy contact");
                case DamageCause.HazardContact:
                    return Loc.T("fail.hazard", "Arena hazard");
                case DamageCause.EnemyBolt:
                    return Loc.T("fail.bolt", "Enemy bolt");
                case DamageCause.BossBolt:
                    return Loc.T("fail.boss_bolt", "Boss Guardian bolt");
                default:
                    return Loc.T("fail.unknown", "Unknown cause");
            }
        }

        public static string FailReason(DamageCause cause, EnemyKind kind)
        {
            if (cause == DamageCause.EnemyContact)
            {
                string name = Loc.T("enemy." + kind, kind.ToString());
                return Loc.Tf("fail.enemy_kind", "Enemy contact ({0})", name);
            }

            return FailReason(cause);
        }

        public static string SourceLabel(DamageCause cause, EnemyKind kind, bool elite, string modifier)
        {
            string name = Loc.T("enemy." + kind, kind.ToString());
            string core;
            switch (cause)
            {
                case DamageCause.AsteroidCollision:
                    core = Loc.T("fail.src.asteroid", "Asteroid");
                    break;
                case DamageCause.HazardContact:
                    core = Loc.T("fail.src.spike", "Spike");
                    break;
                case DamageCause.BossBolt:
                    core = Loc.T("fail.src.boss_bolt", "Boss Guardian bolt");
                    break;
                case DamageCause.EnemyBolt:
                    core = Loc.Tf("fail.src.bolt", "{0} bolt", name);
                    break;
                case DamageCause.EnemyContact:
                    if (kind == EnemyKind.Brute)
                    {
                        core = Loc.Tf("fail.src.charge", "{0} charge", name);
                    }
                    else
                    {
                        core = name;
                    }

                    break;
                default:
                    core = Loc.T("fail.unknown", "Unknown cause");
                    break;
            }

            if (elite && !string.IsNullOrEmpty(modifier))
            {
                core = Loc.Tf("fail.src.elite", "{0}  ·  {1}", core, modifier);
            }

            return core;
        }
    }
}
