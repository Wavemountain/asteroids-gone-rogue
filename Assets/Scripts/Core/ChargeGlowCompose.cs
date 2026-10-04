namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Reduce-effects invulnerability dims ship renderers with a property
    /// block. That block replaces ShipShooter's charge emission, so the
    /// rail charge glow is left undimmed and keeps its own block.
    /// </summary>
    public static class ChargeGlowCompose
    {
        public const string GlowName = "RailChargeGlow";

        public static bool ShouldDim(string objectName, string parentName)
        {
            if (objectName == GlowName || parentName == GlowName)
            {
                return false;
            }

            return true;
        }
    }
}
