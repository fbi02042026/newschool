using UnityEditor;
using UnityEngine;

namespace GaokaoSimulator.Editor
{
    public sealed class GuideCharacterImporter : AssetPostprocessor
    {
        private static readonly string[] FolderPrefixes =
        {
            "Assets/Resources/UI/Guide/",
            "Assets/Resources/UI/NPC/"
        };

        private void OnPreprocessTexture()
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return;
            }

            var matched = false;
            for (int i = 0; i < FolderPrefixes.Length; i++)
            {
                if (assetPath.StartsWith(FolderPrefixes[i]))
                {
                    matched = true;
                    break;
                }
            }

            if (!matched)
            {
                return;
            }

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Bilinear;
        }
    }
}

