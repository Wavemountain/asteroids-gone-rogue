namespace AsteroidsGoneRogue
{
    /// <summary>
    /// World identity for the long haul. Worlds 1–7 have names; later waves keep
    /// climbing (wave 36 is World 8) and reuse a layout look. Loop 1 is worlds
    /// 1–7. Unity-free so tests can walk waves without the Editor.
    /// HP steps use the unwrapped world number and the existing cap.
    /// </summary>
    public static class WorldCatalog
    {
        public const int Count = 7;
        public const int WavesPerWorld = 5;
        public const int FirstLoop = 1;

        public static int NumberForWave(int waveIndex)
        {
            int wave = waveIndex < 1 ? 1 : waveIndex;
            return ((wave - 1) / WavesPerWorld) + 1;
        }

        public static int LoopForWave(int waveIndex)
        {
            int number = NumberForWave(waveIndex);
            return ((number - 1) / Count) + FirstLoop;
        }

        public static int LoopForNumber(int worldNumber)
        {
            int number = worldNumber < 1 ? 1 : worldNumber;
            return ((number - 1) / Count) + FirstLoop;
        }

        /// <summary>
        /// Layout / art world, 1–7. World 8 uses the Launch Belt look of world 1.
        /// </summary>
        public static int LayoutNumber(int worldNumber)
        {
            int number = worldNumber < 1 ? 1 : worldNumber;
            int index = (number - 1) % Count;
            if (index < 0)
            {
                index += Count;
            }

            return index + 1;
        }

        public static int LayoutForWave(int waveIndex)
        {
            return LayoutNumber(NumberForWave(waveIndex));
        }

        public static string Name(int worldNumber)
        {
            switch (LayoutNumber(worldNumber))
            {
                case 2:
                    return Loc.T("world.deep", "Deep Orbit");
                case 3:
                    return Loc.T("world.far", "Far Drift");
                case 4:
                    return Loc.T("world.mines", "Mine Fields");
                case 5:
                    return Loc.T("world.cross", "Cross Gates");
                case 6:
                    return Loc.T("world.islands", "Debris Islands");
                case 7:
                    return Loc.T("world.spokes", "Spoke Ring");
                default:
                    return Loc.T("world.launch", "Launch Belt");
            }
        }

        public static string NameForWave(int waveIndex)
        {
            return Name(NumberForWave(waveIndex));
        }

        public static string Tip(int worldNumber)
        {
            switch (LayoutNumber(worldNumber))
            {
                case 2:
                    return Loc.T("world.tip.2", "Pylons bite. Keep moving.");
                case 3:
                    return Loc.T("world.tip.3", "Mind the trench. Don't sit in the gap.");
                case 4:
                    return Loc.T("world.tip.4", "Mines line the belt. Weave through.");
                case 5:
                    return Loc.T("world.tip.5", "Cross fire. Slip the gates.");
                case 6:
                    return Loc.T("world.tip.6", "Islands block shots. Use the gaps.");
                case 7:
                    return Loc.T("world.tip.7", "Spokes cut the ring. Stay off the lines.");
                default:
                    return Loc.T("world.tip.1", "Open lane. Learn the wrap.");
            }
        }

        public static string TipForWave(int waveIndex)
        {
            return Tip(NumberForWave(waveIndex));
        }

        public static string Headline(int waveIndex)
        {
            int number = NumberForWave(waveIndex);
            string line = Loc.Tf("world.banner", "World {0} - {1}", number, Name(number));
            int loopNumber = LoopForWave(waveIndex);
            if (loopNumber > FirstLoop)
            {
                line += "  ·  " + Loc.Tf("world.loop", "Loop {0}", loopNumber);
            }

            return line;
        }

        public static string BadgeLine(int waveIndex)
        {
            ArenaLayoutId layoutId = ArenaLayout.ForWave(waveIndex);
            string layoutTitle = ArenaLayout.Title(layoutId);
            string badge = ArenaLayout.Badge(layoutId);
            if (string.IsNullOrEmpty(badge))
            {
                badge = string.IsNullOrEmpty(layoutTitle) ? string.Empty : layoutTitle.ToUpperInvariant();
            }

            if (string.IsNullOrEmpty(layoutTitle))
            {
                return badge;
            }

            return badge + "  ·  " + layoutTitle.ToUpperInvariant();
        }

        /// <summary>
        /// Play-mode world intro: "World N - name" (plus Loop N past world 7),
        /// the layout badge, and one tip line.
        /// </summary>
        public static string IntroBanner(int waveIndex)
        {
            string headline = Headline(waveIndex);
            string badgeLine = BadgeLine(waveIndex);
            string tipLine = TipForWave(waveIndex);
            string banner = headline;
            if (!string.IsNullOrEmpty(badgeLine))
            {
                banner += "\n" + badgeLine;
            }

            if (!string.IsNullOrEmpty(tipLine))
            {
                banner += "\n" + tipLine;
            }

            string ruleLine = WorldRules.BannerLineForWave(waveIndex);
            if (!string.IsNullOrEmpty(ruleLine))
            {
                banner += "\n" + ruleLine;
            }

            return banner;
        }

        public static string ContinueSubtitle(int waveIndex)
        {
            int number = NumberForWave(waveIndex);
            return Loc.Tf("world.next", "Next: World {0} - {1}", number, Name(number));
        }

        /// <summary>
        /// Steps of +15% enemy HP. Unwrapped world 8 stays at the world-7 cap.
        /// </summary>
        public static int HpSteps(int worldNumber)
        {
            int steps = worldNumber - 1;
            if (steps < 0)
            {
                steps = 0;
            }

            int cap = DifficultySettings.MaxWorldHpSteps;
            if (steps > cap)
            {
                steps = cap;
            }

            return steps;
        }

        public static int HpMultiplierPercent(int worldNumber)
        {
            return 100 + (DifficultySettings.WorldHpPercent * HpSteps(worldNumber));
        }
    }
}
