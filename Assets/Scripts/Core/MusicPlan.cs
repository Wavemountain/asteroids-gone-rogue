namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Wave-to-track map and the crossfade ramp. Unity-free so tests can walk
    /// waves 1-80 without an AudioSource. Playback lives on <see cref="AudioCues"/>.
    /// </summary>
    public static class MusicPlan
    {
        public const float WorldMusicScale = 0.65f;
        public const float BossMusicScale = 0.65f;
        public const float MusicCrossfadeSeconds = 0.5f;
        public const float EliteStingScale = 0.78f;
        public const float EliteStingDuckSeconds = 1.5f;
        public const float EliteStingDuckScale = 0.4f;
        public const string BossTrack = "boss_guardian";
        public const string BossFinalTrack = "boss_guardian_final";
        public const string EliteStingTrack = "jingles_NES00";

        public static readonly string[] WorldTrackNames =
        {
            "world1_launch_belt",
            "world2_deep_orbit",
            "world3_far_drift",
            "world4_mine_fields",
            "world5_cross_gates",
            "world6_debris_islands",
            "world7_spoke_ring"
        };

        public static string WorldTrackName(int waveIndex)
        {
            int layout = WorldCatalog.LayoutNumber(WorldCatalog.NumberForWave(waveIndex));
            int index = layout - 1;
            if (index < 0 || index >= WorldTrackNames.Length)
            {
                index = 0;
            }

            return WorldTrackNames[index];
        }

        public static string BossTrackName(int waveIndex)
        {
            int layout = WorldCatalog.LayoutForWave(waveIndex);
            int loopNumber = WorldCatalog.LoopForWave(waveIndex);
            if (layout == WorldCatalog.Count || loopNumber >= 2)
            {
                return BossFinalTrack;
            }

            return BossTrack;
        }

        public static bool UsesBossTrack(int waveIndex)
        {
            return BossRules.IsBossWave(waveIndex);
        }

        public static bool UsesEliteSting(int waveIndex)
        {
            return WaveModifier.IsElite(waveIndex);
        }

        /// <summary>
        /// A repeat whose clip is already playing, or is the clip an in-progress
        /// crossfade is heading toward, must not restart the track.
        /// </summary>
        public static bool ShouldStartCrossfade(bool sameAsCurrent, bool sameAsIncoming)
        {
            if (sameAsCurrent || sameAsIncoming)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 0 at the start of a crossfade, 1 at the end. Unscaled elapsed seconds.
        /// </summary>
        public static float CrossfadeRamp(float elapsed, float duration)
        {
            if (duration <= 0f)
            {
                return 1f;
            }

            if (elapsed <= 0f)
            {
                return 0f;
            }

            if (elapsed >= duration)
            {
                return 1f;
            }

            return elapsed / duration;
        }
    }
}
