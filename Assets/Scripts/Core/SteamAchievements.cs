using System;
using System.Reflection;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Ready-for-Steam API surface. When Steamworks.NET is present, unlocks
    /// forward to <c>SteamUserStats.SetAchievement</c>. Without the plugin this
    /// is a no-op — local <see cref="AchievementPersist"/> still unlocks.
    /// </summary>
    public static class SteamAchievements
    {
        public static bool SteamworksPresent
        {
            get { return FindUserStats() != null; }
        }

        public static void Unlock(AchievementId id)
        {
            UnlockApi(AchievementCatalog.SteamApiName(id));
        }

        public static void UnlockApi(string apiName)
        {
            if (string.IsNullOrEmpty(apiName))
            {
                return;
            }

            Type stats = FindUserStats();
            if (stats == null)
            {
                return;
            }

            MethodInfo set = stats.GetMethod("SetAchievement", new[] { typeof(string) });
            if (set != null)
            {
                set.Invoke(null, new object[] { apiName });
            }

            MethodInfo store = stats.GetMethod("StoreStats", Type.EmptyTypes);
            if (store != null)
            {
                store.Invoke(null, null);
            }
        }

        private static Type FindUserStats()
        {
            Type stats = Type.GetType("Steamworks.SteamUserStats, Assembly-CSharp-firstpass");
            if (stats != null)
            {
                return stats;
            }

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                stats = assemblies[i].GetType("Steamworks.SteamUserStats");
                if (stats != null)
                {
                    return stats;
                }
            }

            return null;
        }
    }
}
