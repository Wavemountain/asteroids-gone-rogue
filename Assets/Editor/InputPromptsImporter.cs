using UnityEditor;
using UnityEngine;

namespace AsteroidsGoneRogue.EditorTools
{
    /// <summary>
    /// Safety net for Kenney Input Prompts. The committed .meta files already
    /// import as uncompressed UI sprites; this reapplies that if a PNG is
    /// reimported without them.
    /// </summary>
    public sealed class InputPromptsImporter : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (string.IsNullOrEmpty(assetPath) || assetPath.IndexOf("InputPrompts") < 0)
            {
                return;
            }

            TextureImporter importer = assetImporter as TextureImporter;
            if (importer == null)
            {
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.wrapMode = TextureWrapMode.Clamp;
        }
    }
}
