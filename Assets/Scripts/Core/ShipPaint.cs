using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Profile paint on the hangar preview, and an optional player-bolt trail.
    /// Shared materials are not rewritten. No pulse.
    /// </summary>
    public static class ShipPaint
    {
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

        public static bool TryTrail(out float red, out float green, out float blue)
        {
            red = 0f;
            green = 0f;
            blue = 0f;
            int trailId = SinkRuntime.TrailId;
            if (!ShopSinkCatalog.IsTrail(trailId))
            {
                return false;
            }

            ShopSinkCatalog.TrailRgb(trailId, out red, out green, out blue);
            return true;
        }

        public static void TintRenderer(Renderer renderer, float red, float green, float blue, float emissionScale)
        {
            if (renderer == null)
            {
                return;
            }

            float scale = emissionScale < 0f ? 0f : emissionScale;
            Color tint = new Color(red, green, blue, 1f);
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetColor(ColorId, tint);
            block.SetColor(EmissionId, tint * scale);
            renderer.SetPropertyBlock(block);
        }

        public static void ClearRenderer(Renderer renderer)
        {
            if (renderer == null)
            {
                return;
            }

            renderer.SetPropertyBlock(null);
        }
    }
}
