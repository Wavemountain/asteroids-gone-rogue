namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Keeps the gameplay frustum at 16:9. Wider screens pillarbox and taller
    /// screens letterbox, so ultrawide neither reveals extra playfield nor
    /// crops it. Vertical FOV stays 54. Unity-free.
    /// </summary>
    public static class ArenaFraming
    {
        public const float ReferenceWidth = 16f;
        public const float ReferenceHeight = 9f;
        public const float BaseFov = 54f;

        public static float ReferenceAspect
        {
            get { return ReferenceWidth / ReferenceHeight; }
        }

        public static void Viewport(float screenWidth, float screenHeight, out float x, out float y, out float width, out float height)
        {
            float wide = screenWidth;
            float high = screenHeight;
            if (wide < 1f)
            {
                wide = 1f;
            }

            if (high < 1f)
            {
                high = 1f;
            }

            float aspect = wide / high;
            float reference = ReferenceAspect;
            if (aspect > reference + 0.0001f)
            {
                width = reference / aspect;
                height = 1f;
                x = (1f - width) * 0.5f;
                y = 0f;
                return;
            }

            if (aspect < reference - 0.0001f)
            {
                width = 1f;
                height = aspect / reference;
                x = 0f;
                y = (1f - height) * 0.5f;
                return;
            }

            x = 0f;
            y = 0f;
            width = 1f;
            height = 1f;
        }
    }
}
