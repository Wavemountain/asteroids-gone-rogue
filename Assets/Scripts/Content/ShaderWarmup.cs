using System;
using UnityEngine;

namespace AsteroidsGoneRogue
{
    /// <summary>
    /// Keeps Shader.Find shaders in the player build. The .shader files live
    /// under Resources, and a material there references AGR/RimLit so the
    /// shader is not stripped. Missing shaders fall back to Standard.
    /// AGR/RimLit is a surface shader: it has ForwardBase, not pass type 0.
    /// Building a variant for that missing pass throws and used to abort boot.
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
            try
            {
                WarmShots();
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
            catch (Exception ex)
            {
                Debug.LogWarning("ShaderWarmup.Touch failed: " + ex.Message);
            }
        }

        /// <summary>
        /// WarmupAllShaders only. Pooled shots are instantiated once off-screen
        /// by ContentFactory.PrewarmProjectiles. A pass-typed variant list is not
        /// safe here: AGR/RimLit does not have pass type 0.
        /// </summary>
        public static void WarmShots()
        {
            try
            {
                Shader.WarmupAllShaders();
            }
            catch (Exception ex)
            {
                Debug.LogWarning("ShaderWarmup.WarmShots failed: " + ex.Message);
            }
        }
    }
}
