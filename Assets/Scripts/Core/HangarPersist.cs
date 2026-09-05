namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Hangar medals (Scout Wing, Deep Orbit, Far Drift) persisted locally.
    /// Award rules stay in <see cref="MedalCatalog"/> so tests do not need PlayerPrefs.
    /// </summary>
    public sealed class HangarPersist
    {
        public const string MedalsKey = "agr.hangar.medals";

        public int MedalMask { get; private set; }

        public HangarPersist()
        {
        }

        public HangarPersist(int medalMask)
        {
            MedalMask = medalMask;
        }

        public bool Owns(MedalId id)
        {
            return MedalCatalog.Owns(MedalMask, id);
        }

        public bool HasAnyMedal
        {
            get { return MedalMask != 0; }
        }

        public bool TryAward(MedalId id)
        {
            int next = MedalCatalog.WithAward(MedalMask, id);
            if (next == MedalMask)
            {
                return false;
            }

            MedalMask = next;
            return true;
        }

        public string BadgeRow()
        {
            return MedalCatalog.BadgeRow(MedalMask);
        }

        public string LadderLine()
        {
            return MedalCatalog.LadderLine(MedalMask);
        }

        public static HangarPersist Load()
        {
            return new HangarPersist(UnityEngine.PlayerPrefs.GetInt(MedalsKey, 0));
        }

        public void Save()
        {
            UnityEngine.PlayerPrefs.SetInt(MedalsKey, MedalMask);
            UnityEngine.PlayerPrefs.Save();
        }
    }
}
