using System.Globalization;
using System.Text;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Profile fittings bought with credits. Not the run file. Unity-free codec.
    /// </summary>
    public sealed class SinkProfileData
    {
        public int Version;
        public int OwnedMask;
        public int Paint = -1;
        public int Trail = -1;
        public int ShieldRank;
        public int ReachRank;

        public SinkProfileData Copy()
        {
            SinkProfileData copy = new SinkProfileData();
            copy.Version = Version;
            copy.OwnedMask = OwnedMask;
            copy.Paint = Paint;
            copy.Trail = Trail;
            copy.ShieldRank = ShieldRank;
            copy.ReachRank = ReachRank;
            return copy;
        }
    }

    public static class SinkProfileCodec
    {
        public const int CurrentVersion = 1;

        public static SinkProfileData Fresh()
        {
            SinkProfileData data = new SinkProfileData();
            data.Version = CurrentVersion;
            data.Paint = -1;
            data.Trail = -1;
            return data;
        }

        public static bool Owns(SinkProfileData data, int sinkId)
        {
            if (data == null || sinkId < 0 || sinkId > 30)
            {
                return false;
            }

            return (data.OwnedMask & (1 << sinkId)) != 0;
        }

        public static string ToJson(SinkProfileData data)
        {
            if (data == null)
            {
                return string.Empty;
            }

            StringBuilder builder = new StringBuilder(128);
            builder.Append('{');
            Append(builder, "Version", data.Version, true);
            Append(builder, "OwnedMask", data.OwnedMask, false);
            Append(builder, "Paint", data.Paint, false);
            Append(builder, "Trail", data.Trail, false);
            Append(builder, "ShieldRank", data.ShieldRank, false);
            Append(builder, "ReachRank", data.ReachRank, false);
            builder.Append('}');
            return builder.ToString();
        }

        public static bool TryParse(string json, out SinkProfileData data)
        {
            data = null;
            if (string.IsNullOrEmpty(json))
            {
                return false;
            }

            string trimmed = json.Trim();
            if (trimmed.Length < 2 || trimmed[0] != '{' || trimmed[trimmed.Length - 1] != '}')
            {
                return false;
            }

            SinkProfileData parsed = Fresh();
            parsed.Version = 0;
            bool sawVersion = false;
            int cursor = 1;
            while (cursor < trimmed.Length - 1)
            {
                SkipComma(trimmed, ref cursor);
                if (cursor >= trimmed.Length - 1)
                {
                    break;
                }

                string key;
                if (!ReadString(trimmed, ref cursor, out key))
                {
                    return false;
                }

                if (!SkipColon(trimmed, ref cursor))
                {
                    return false;
                }

                int number;
                if (!ReadInt(trimmed, ref cursor, out number))
                {
                    return false;
                }

                if (!Assign(parsed, key, number))
                {
                    return false;
                }

                if (key == "Version")
                {
                    sawVersion = true;
                }
            }

            if (!sawVersion || !IsValid(parsed))
            {
                return false;
            }

            data = parsed;
            return true;
        }

        public static bool IsValid(SinkProfileData data)
        {
            if (data == null)
            {
                return false;
            }

            if (data.Version < 1 || data.Version > CurrentVersion)
            {
                return false;
            }

            if (data.OwnedMask < 0 || data.OwnedMask >= (1 << ShopSinkCatalog.Count))
            {
                return false;
            }

            if (!CosmeticIdOk(data.Paint, true, data.OwnedMask))
            {
                return false;
            }

            if (!CosmeticIdOk(data.Trail, false, data.OwnedMask))
            {
                return false;
            }

            if (data.ShieldRank < 0 || data.ShieldRank > ShopSinkCatalog.ShieldCap)
            {
                return false;
            }

            if (data.ReachRank < 0 || data.ReachRank > ShopSinkCatalog.ReachCap)
            {
                return false;
            }

            return true;
        }

        private static bool CosmeticIdOk(int id, bool paint, int ownedMask)
        {
            if (id == -1)
            {
                return true;
            }

            bool kind = paint ? ShopSinkCatalog.IsPaint(id) : ShopSinkCatalog.IsTrail(id);
            if (!kind)
            {
                return false;
            }

            if (id < 0 || id > 30)
            {
                return false;
            }

            return (ownedMask & (1 << id)) != 0;
        }

        private static bool Assign(SinkProfileData data, string key, int number)
        {
            switch (key)
            {
                case "Version":
                    data.Version = number;
                    return true;
                case "OwnedMask":
                    data.OwnedMask = number;
                    return true;
                case "Paint":
                    data.Paint = number;
                    return true;
                case "Trail":
                    data.Trail = number;
                    return true;
                case "ShieldRank":
                    data.ShieldRank = number;
                    return true;
                case "ReachRank":
                    data.ReachRank = number;
                    return true;
                default:
                    return true;
            }
        }

        private static void Append(StringBuilder builder, string key, int value, bool first)
        {
            if (!first)
            {
                builder.Append(',');
            }

            builder.Append('"');
            builder.Append(key);
            builder.Append("\":");
            builder.Append(value.ToString(CultureInfo.InvariantCulture));
        }

        private static void SkipComma(string text, ref int cursor)
        {
            while (cursor < text.Length && (text[cursor] == ',' || text[cursor] == ' '))
            {
                cursor += 1;
            }
        }

        private static bool SkipColon(string text, ref int cursor)
        {
            while (cursor < text.Length && text[cursor] == ' ')
            {
                cursor += 1;
            }

            if (cursor >= text.Length || text[cursor] != ':')
            {
                return false;
            }

            cursor += 1;
            return true;
        }

        private static bool ReadString(string text, ref int cursor, out string value)
        {
            value = string.Empty;
            while (cursor < text.Length && text[cursor] == ' ')
            {
                cursor += 1;
            }

            if (cursor >= text.Length || text[cursor] != '"')
            {
                return false;
            }

            cursor += 1;
            int start = cursor;
            while (cursor < text.Length && text[cursor] != '"')
            {
                cursor += 1;
            }

            if (cursor >= text.Length)
            {
                return false;
            }

            value = text.Substring(start, cursor - start);
            cursor += 1;
            return true;
        }

        private static bool ReadInt(string text, ref int cursor, out int value)
        {
            value = 0;
            while (cursor < text.Length && text[cursor] == ' ')
            {
                cursor += 1;
            }

            int start = cursor;
            if (cursor < text.Length && (text[cursor] == '-' || text[cursor] == '+'))
            {
                cursor += 1;
            }

            int digits = 0;
            while (cursor < text.Length && text[cursor] >= '0' && text[cursor] <= '9')
            {
                cursor += 1;
                digits += 1;
            }

            if (digits < 1)
            {
                return false;
            }

            return int.TryParse(text.Substring(start, cursor - start), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
        }
    }

    public static class SinkRules
    {
        public const int StateBuy = 0;
        public const int StatePoor = 1;
        public const int StateOwned = 2;
        public const int StateFitted = 3;
        public const int StateCapped = 4;

        public static int StartingShieldBonus(SinkProfileData data)
        {
            if (data == null || data.ShieldRank < 1)
            {
                return 0;
            }

            if (data.ShieldRank > ShopSinkCatalog.ShieldCap)
            {
                return ShopSinkCatalog.ShieldCap;
            }

            return data.ShieldRank;
        }

        public static bool TryPurchase(SinkProfileData profile, int sinkId, int credits, out SinkProfileData next, out int price)
        {
            next = null;
            price = 0;
            if (profile == null || sinkId < 0 || sinkId >= ShopSinkCatalog.Count)
            {
                return false;
            }

            int purse = credits < 0 ? 0 : credits;
            SinkProfileData copy = profile.Copy();
            copy.Version = SinkProfileCodec.CurrentVersion;
            if (ShopSinkCatalog.IsPaint(sinkId))
            {
                return BuyCosmetic(copy, sinkId, purse, true, out next, out price);
            }

            if (ShopSinkCatalog.IsTrail(sinkId))
            {
                return BuyCosmetic(copy, sinkId, purse, false, out next, out price);
            }

            if (sinkId == ShopSinkCatalog.StartShield)
            {
                return BuyRank(copy, sinkId, purse, copy.ShieldRank, out next, out price);
            }

            if (sinkId == ShopSinkCatalog.PickupReach)
            {
                return BuyRank(copy, sinkId, purse, copy.ReachRank, out next, out price);
            }

            return false;
        }

        public static int TileState(SinkProfileData profile, int sinkId, int credits)
        {
            if (profile == null || sinkId < 0 || sinkId >= ShopSinkCatalog.Count)
            {
                return StateBuy;
            }

            int purse = credits < 0 ? 0 : credits;
            if (ShopSinkCatalog.IsPaint(sinkId))
            {
                return CosmeticState(profile, sinkId, profile.Paint, purse);
            }

            if (ShopSinkCatalog.IsTrail(sinkId))
            {
                return CosmeticState(profile, sinkId, profile.Trail, purse);
            }

            int rank = sinkId == ShopSinkCatalog.StartShield ? profile.ShieldRank : profile.ReachRank;
            if (rank >= ShopSinkCatalog.Cap(sinkId))
            {
                return StateCapped;
            }

            int price = ShopSinkCatalog.Price(sinkId, rank);
            if (purse < price)
            {
                return StatePoor;
            }

            return StateBuy;
        }

        private static int CosmeticState(SinkProfileData profile, int sinkId, int equipped, int credits)
        {
            if (!SinkProfileCodec.Owns(profile, sinkId))
            {
                int price = ShopSinkCatalog.Price(sinkId, 0);
                if (credits < price)
                {
                    return StatePoor;
                }

                return StateBuy;
            }

            if (equipped == sinkId)
            {
                return StateFitted;
            }

            return StateOwned;
        }

        private static bool BuyCosmetic(SinkProfileData copy, int sinkId, int credits, bool paint, out SinkProfileData next, out int price)
        {
            next = null;
            price = 0;
            if (SinkProfileCodec.Owns(copy, sinkId))
            {
                int equipped = paint ? copy.Paint : copy.Trail;
                if (equipped == sinkId)
                {
                    return false;
                }

                if (paint)
                {
                    copy.Paint = sinkId;
                }
                else
                {
                    copy.Trail = sinkId;
                }

                next = copy;
                return true;
            }

            price = ShopSinkCatalog.Price(sinkId, 0);
            if (price < 1 || credits < price)
            {
                return false;
            }

            copy.OwnedMask = copy.OwnedMask | (1 << sinkId);
            if (paint)
            {
                copy.Paint = sinkId;
            }
            else
            {
                copy.Trail = sinkId;
            }

            next = copy;
            return true;
        }

        private static bool BuyRank(SinkProfileData copy, int sinkId, int credits, int rank, out SinkProfileData next, out int price)
        {
            next = null;
            price = 0;
            if (rank >= ShopSinkCatalog.Cap(sinkId))
            {
                return false;
            }

            price = ShopSinkCatalog.Price(sinkId, rank);
            if (price < 1 || credits < price)
            {
                return false;
            }

            if (sinkId == ShopSinkCatalog.StartShield)
            {
                copy.ShieldRank = rank + 1;
            }
            else
            {
                copy.ReachRank = rank + 1;
            }

            next = copy;
            return true;
        }
    }

    /// <summary>
    /// Live ranks read by spawn and projectile code. Default rank 0.
    /// </summary>
    public static class SinkRuntime
    {
        public static int ReachRank;
        public static int TrailId = -1;
        public static int PaintId = -1;

        public static void Apply(SinkProfileData data)
        {
            if (data == null)
            {
                ReachRank = 0;
                TrailId = -1;
                PaintId = -1;
                return;
            }

            ReachRank = data.ReachRank;
            TrailId = data.Trail;
            PaintId = data.Paint;
        }

        public static float PickupReachMultiplier()
        {
            return ShopSinkCatalog.ReachMultiplier(ReachRank);
        }
    }
}
