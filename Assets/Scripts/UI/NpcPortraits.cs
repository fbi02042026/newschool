using UnityEngine;

namespace GaokaoSimulator.UI
{
    public enum NpcPortraitId
    {
        Expert
    }

    public static class NpcPortraits
    {
        public static Sprite Load(NpcPortraitId id)
        {
            var path = $"UI/NPC/{id}";
            var sprite = Resources.Load<Sprite>(path);
            if (sprite == null)
            {
                Debug.LogWarning($"[NpcPortraits] 未找到 NPC 肖像: {path}");
            }
            return sprite;
        }
    }
}