using System;
using UnityEngine;

namespace AsteroidsGoneRogue
{
    public enum DifficultyGrade
    {
        Easy,
        Normal,
        Hard
    }

    /// <summary>
    /// Hangar difficulty. Scales HP / spawn / incoming damage / credits / lives.
    /// Persists in PlayerPrefs. Combat math stays Editor-free for Week 1 tests.
    /// </summary>
    public static class DifficultySettings
    {
        public const string PrefsKey = "agr.difficulty";
        public const string EasyCode = "easy";
        public const string NormalCode = "normal";
        public const string HardCode = "hard";
        public const int MaxLives = 5;
        public const int StartLivesCount = 3;
        public const int EasyStartLives = 3;
        public const int NormalStartLives = 3;
        public const int HardStartLives = 3;
        public const float EasyExtraLifeChance = 0.10f;
        public const float NormalExtraLifeChance = 0.045f;
        public const float HardExtraLifeChance = 0.02f;
        public const float ExtraLifeTimeoutSeconds = 9f;
        public const int EasyWaveClearCredits = 185;
        public const int NormalWaveClearCredits = 165;
        public const int HardWaveClearCredits = 140;
        public const int EasyPlayerHullBonus = 1;

        private static bool _loaded;
        private static DifficultyGrade _grade = DifficultyGrade.Normal;

        public static DifficultyGrade Current
        {
            get
            {
                EnsureLoaded();
                return _grade;
            }
        }

        public static void EnsureLoaded()
        {
            if (_loaded)
            {
                return;
            }

            ApplyCode(PlayerPrefs.GetString(PrefsKey, NormalCode), false);
            _loaded = true;
        }

        public static void SetGrade(DifficultyGrade grade)
        {
            _grade = grade;
            _loaded = true;
            PlayerPrefs.SetString(PrefsKey, CodeFor(grade));
            PlayerPrefs.Save();
        }

        public static string Code
        {
            get { return CodeFor(Current); }
        }

        public static string CodeFor(DifficultyGrade grade)
        {
            switch (grade)
            {
                case DifficultyGrade.Easy:
                    return EasyCode;
                case DifficultyGrade.Hard:
                    return HardCode;
                default:
                    return NormalCode;
            }
        }

        public static DifficultyGrade Parse(string code)
        {
            if (code == EasyCode)
            {
                return DifficultyGrade.Easy;
            }

            if (code == HardCode)
            {
                return DifficultyGrade.Hard;
            }

            return DifficultyGrade.Normal;
        }

        public static int StartLives
        {
            get { return StartLivesFor(Current); }
        }

        public static int StartLivesFor(DifficultyGrade grade)
        {
            switch (grade)
            {
                case DifficultyGrade.Easy:
                case DifficultyGrade.Hard:
                default:
                    return StartLivesCount;
            }
        }

        public static float ExtraLifeChance
        {
            get { return ExtraLifeChanceFor(Current); }
        }

        public static float ExtraLifeChanceFor(DifficultyGrade grade)
        {
            switch (grade)
            {
                case DifficultyGrade.Easy:
                    return EasyExtraLifeChance;
                case DifficultyGrade.Hard:
                    return HardExtraLifeChance;
                default:
                    return NormalExtraLifeChance;
            }
        }

        public static int WaveClearCredits
        {
            get
            {
                switch (Current)
                {
                    case DifficultyGrade.Easy:
                        return EasyWaveClearCredits;
                    case DifficultyGrade.Hard:
                        return HardWaveClearCredits;
                    default:
                        return NormalWaveClearCredits;
                }
            }
        }

        public static int PlayerHullBonus
        {
            get { return Current == DifficultyGrade.Easy ? EasyPlayerHullBonus : 0; }
        }

        public static int ExtraAsteroids
        {
            get
            {
                switch (Current)
                {
                    case DifficultyGrade.Easy:
                        return -1;
                    case DifficultyGrade.Hard:
                        return 1;
                    default:
                        return 0;
                }
            }
        }

        public static int ExtraEnemyCount
        {
            get { return Current == DifficultyGrade.Hard ? 1 : 0; }
        }

        public static int ScaleEnemyHp(int hp)
        {
            return ScaleEnemyHpFor(hp, Current);
        }

        public static int ScaleEnemyHpFor(int hp, DifficultyGrade grade)
        {
            if (hp < 1)
            {
                hp = 1;
            }

            switch (grade)
            {
                case DifficultyGrade.Easy:
                    return Math.Max(1, (hp * 4) / 5);
                case DifficultyGrade.Hard:
                    return Math.Max(hp + 1, (hp * 5) / 4);
                default:
                    return hp;
            }
        }

        public static int ScaleIncomingDamage(int amount, DamageCause cause)
        {
            return ScaleIncomingDamageFor(amount, cause, Current);
        }

        public static int ScaleIncomingDamageFor(int amount, DamageCause cause, DifficultyGrade grade)
        {
            if (amount < 1)
            {
                return 0;
            }

            switch (grade)
            {
                case DifficultyGrade.Easy:
                    return amount <= 1 ? 1 : amount - 1;
                case DifficultyGrade.Hard:
                    if (cause == DamageCause.HazardContact)
                    {
                        return amount;
                    }

                    return amount + 1;
                default:
                    return amount;
            }
        }

        public static string Title(DifficultyGrade grade)
        {
            switch (grade)
            {
                case DifficultyGrade.Easy:
                    return Loc.T("ui.diff.easy", "Easy");
                case DifficultyGrade.Hard:
                    return Loc.T("ui.diff.hard", "Hard");
                default:
                    return Loc.T("ui.diff.normal", "Normal");
            }
        }

        private static void ApplyCode(string code, bool save)
        {
            _grade = Parse(code);
            if (save)
            {
                PlayerPrefs.SetString(PrefsKey, CodeFor(_grade));
                PlayerPrefs.Save();
            }
        }
    }
}
