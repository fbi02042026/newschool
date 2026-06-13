using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GaokaoSimulator.UI
{
    public static class RuntimeArt
    {
        private static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();
        private static Sprite builtinRounded;
        private static Texture2D roundedTexture;

        public static Sprite LoadSprite(string resourcesPath)
        {
            if (string.IsNullOrWhiteSpace(resourcesPath)) return null;
            if (SpriteCache.TryGetValue(resourcesPath, out var cached) && cached != null) return cached;

            var loaded = Resources.Load<Sprite>(resourcesPath);
            SpriteCache[resourcesPath] = loaded;
            return loaded;
        }

        /// <summary>
        /// 加载 UI/BG/ 目录下的背景图，若不存在返回 null
        /// </summary>
        public static Sprite LoadBg(string bgName)
        {
            return LoadSprite($"UI/BG/{bgName}");
        }

        public static Sprite BuiltinRoundedSprite()
        {
            if (builtinRounded != null) return builtinRounded;

            const int size = 32;
            const float radius = 10f;
            const float feather = 1.5f;

            roundedTexture = new Texture2D(size, size, TextureFormat.ARGB32, false);
            roundedTexture.name = "RuntimeRoundedTexture";
            roundedTexture.wrapMode = TextureWrapMode.Clamp;
            roundedTexture.filterMode = FilterMode.Bilinear;

            var clear = new Color32(255, 255, 255, 0);
            var solid = new Color32(255, 255, 255, 255);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Min(x, size - 1 - x);
                    float dy = Mathf.Min(y, size - 1 - y);
                    float edgeDistance = Mathf.Min(dx, dy);

                    if (edgeDistance >= radius)
                    {
                        roundedTexture.SetPixel(x, y, solid);
                        continue;
                    }

                    float cornerDx = radius - dx;
                    float cornerDy = radius - dy;
                    float distanceToCorner = Mathf.Sqrt(cornerDx * cornerDx + cornerDy * cornerDy);
                    float alpha = Mathf.Clamp01((radius - distanceToCorner) / feather);

                    if (alpha <= 0f)
                    {
                        roundedTexture.SetPixel(x, y, clear);
                    }
                    else if (alpha >= 1f)
                    {
                        roundedTexture.SetPixel(x, y, solid);
                    }
                    else
                    {
                        roundedTexture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                    }
                }
            }

            roundedTexture.Apply();
            builtinRounded = Sprite.Create(
                roundedTexture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(radius, radius, radius, radius)
            );
            builtinRounded.name = "RuntimeRoundedSprite";
            return builtinRounded;
        }

        public static void ApplyRounded(Image image)
        {
            if (image == null) return;
            image.sprite = BuiltinRoundedSprite();
            image.type = Image.Type.Sliced;
        }
    }
}

