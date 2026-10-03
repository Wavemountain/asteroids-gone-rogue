namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Hangar end-credits roll copy. Pure C# so tests can lock the card without Play Mode.
    /// </summary>
    public static class EndCredits
    {
        public const int TitleSize = 32;
        public const int BodySize = 17;
        public const float ScrollSpeed = 28f;
        public const string TitleHex = "#FFD16F";
        public const string BodyHex = "#B8E8FF";

        public static readonly UnityEngine.Color TitleColor = new UnityEngine.Color32(255, 209, 111, 255);
        public static readonly UnityEngine.Color BodyColor = new UnityEngine.Color32(184, 232, 255, 255);

        public static string Title()
        {
            return "Asteroids gone rogue";
        }

        public static string Body()
        {
            return Loc.T(
                "credits.body",
                "Audio\nKenney.nl + yd\n"
                + "SFX 0.47 Kenney CC0: shield, armor, pickups, tells, agr_ricochet, swarm death.\n\n"
                + "Music\nJuhani Junkala, Kenney, MintoDog, HydroGene (CC0)\n\n"
                + "Fonts\nKenney Future\n\n"
                + "Team\nSpelPM / GameBot / BlenderBot / AtmosBot / Speltest");
        }
    }
}
