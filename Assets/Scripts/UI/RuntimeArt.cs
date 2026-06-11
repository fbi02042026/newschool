using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GaokaoSimulator.UI
{
    public static class RuntimeArt
    {
        private static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();
        private static Sprite builtinRounded;

        public static Sprite LoadSprite(string resourcesPath)
        {
            if (string.IsNullOrWhiteSpace(resourcesPath)) return null;
            if (SpriteCache.TryGetValue(resourcesPath, out var cached) && cached != null) return cached;

            var loaded = Resources.Load<Sprite>(resourcesPath);
            SpriteCache[resourcesPath] = loaded;
            return loaded;
        }

        public static Sprite BuiltinRoundedSprite()
        {
            if (builtinRounded != null) return builtinRounded;
            builtinRounded = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
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

