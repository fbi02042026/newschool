using UnityEngine;
using UnityEngine.UI;

namespace GaokaoSimulator.UI.Effects
{
    [RequireComponent(typeof(Graphic))]
    public class UiVerticalGradient : BaseMeshEffect
    {
        [SerializeField] private Color topColor = Color.white;
        [SerializeField] private Color bottomColor = Color.white;
        [SerializeField, Range(-1f, 1f)] private float bias = 0f;

        public void SetColors(Color top, Color bottom)
        {
            topColor = top;
            bottomColor = bottom;
            graphic?.SetVerticesDirty();
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || vh.currentVertCount == 0) return;

            var minY = float.PositiveInfinity;
            var maxY = float.NegativeInfinity;
            var v = new UIVertex();
            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref v, i);
                minY = Mathf.Min(minY, v.position.y);
                maxY = Mathf.Max(maxY, v.position.y);
            }

            var range = maxY - minY;
            if (range <= 0.0001f) return;

            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref v, i);
                var t = Mathf.Clamp01((v.position.y - minY) / range);
                t = Mathf.Clamp01(t + bias * 0.5f);
                var c = Color.Lerp(bottomColor, topColor, t);
                v.color = new Color32(
                    (byte)Mathf.Clamp(Mathf.RoundToInt(v.color.r * c.r), 0, 255),
                    (byte)Mathf.Clamp(Mathf.RoundToInt(v.color.g * c.g), 0, 255),
                    (byte)Mathf.Clamp(Mathf.RoundToInt(v.color.b * c.b), 0, 255),
                    (byte)Mathf.Clamp(Mathf.RoundToInt(v.color.a * c.a), 0, 255)
                );
                vh.SetUIVertex(v, i);
            }
        }
    }
}
