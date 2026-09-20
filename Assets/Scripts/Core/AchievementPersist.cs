namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Local achievement unlocks. Steamworks is optional via
    /// <see cref="SteamAchievements"/> — this mask is the source of truth.
    /// </summary>
    public sealed class AchievementPersist
    {
        public int Mask { get; private set; }

        public AchievementPersist()
        {
        }

        public AchievementPersist(int mask)
        {
            Mask = mask;
        }

        public bool Owns(AchievementId id)
        {
            return AchievementCatalog.Owns(Mask, id);
        }

        public bool TryUnlock(AchievementId id)
        {
            int next = AchievementCatalog.WithAward(Mask, id);
            if (next == Mask)
            {
                return false;
            }

            Mask = next;
            SteamAchievements.Unlock(id);
            return true;
        }

        public string LadderLine()
        {
            return AchievementCatalog.LadderLine(Mask);
        }

        public static AchievementPersist Load()
        {
            return new AchievementPersist(
                UnityEngine.PlayerPrefs.GetInt(AchievementCatalog.PrefsKey, 0));
        }

        public void Save()
        {
            UnityEngine.PlayerPrefs.SetInt(AchievementCatalog.PrefsKey, Mask);
            UnityEngine.PlayerPrefs.Save();
        }
    }
}
