using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Keeps Shader.Find shaders in the player build. The .shader files live
    /// under Resources, and a material there references AGR/RimLit so the
    /// shader is not stripped. Missing shaders fall back to Standard.
    /// </summary>
    public static class ShaderWarmup
    {
        public const string RimResource = "Shaders/AgrRimLit";
        public const string RimMaterialResource = "Shaders/Mat_AgrRimLitRef";
        public const string PostResource = "Shaders/AgrPost";
        public const string PostMaterialResource = "Shaders/Mat_AgrPostRef";
        public const string RimShaderName = "AGR/RimLit";
        public const string PostShaderName = "AGR/Post";

        public static void Touch()
        {
            Material rimMaterial = Resources.Load<Material>(RimMaterialResource);
            Shader rimShader = null;
            if (rimMaterial != null)
            {
                rimShader = rimMaterial.shader;
            }

            if (rimShader == null)
            {
                rimShader = Resources.Load<Shader>(RimResource);
            }

            if (rimShader == null)
            {
                rimShader = Shader.Find(RimShaderName);
            }

            Material postMaterial = Resources.Load<Material>(PostMaterialResource);
            Shader postShader = null;
            if (postMaterial != null)
            {
                postShader = postMaterial.shader;
            }

            if (postShader == null)
            {
                postShader = Resources.Load<Shader>(PostResource);
            }

            if (postShader == null)
            {
                postShader = Shader.Find(PostShaderName);
            }

            if (rimShader == null && postShader == null)
            {
                return;
            }
        }
    }
}
