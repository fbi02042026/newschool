using UnityEngine;
using UnityEngine.UI;

namespace GaokaoSimulator.UI.Effects
{
    [RequireComponent(typeof(Graphic))]
    public class UiCornerGradient : BaseMeshEffect
    {
        [SerializeField] private Color topLeft = Color.white;
        [SerializeField] private Color topRight = Color.white;
        [SerializeField] private Color bottomLeft = Color.white;
        [SerializeField] private Color bottomRight = Color.white;

        public void SetColors(Color tl, Color tr, Color bl, Color br)
        {
            topLeft = tl;
            topRight = tr;
            bottomLeft = bl;
            bottomRight = br;
            graphic?.SetVerticesDirty();
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || vh.currentVertCount == 0) return;

            var minX = float.PositiveInfinity;
            var maxX = float.NegativeInfinity;
            var minY = float.PositiveInfinity;
            var maxY = float.NegativeInfinity;

            var v = new UIVertex();
            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref v, i);
                minX = Mathf.Min(minX, v.position.x);
                maxX = Mathf.Max(maxX, v.position.x);
                minY = Mathf.Min(minY, v.position.y);
                maxY = Mathf.Max(maxY, v.position.y);
            }

            var rangeX = maxX - minX;
            var rangeY = maxY - minY;
            if (rangeX <= 0.0001f || rangeY <= 0.0001f) return;

            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref v, i);
                var tx = Mathf.Clamp01((v.position.x - minX) / rangeX);
                var ty = Mathf.Clamp01((v.position.y - minY) / rangeY);

                var top = Color.Lerp(topLeft, topRight, tx);
                var bottom = Color.Lerp(bottomLeft, bottomRight, tx);
                var c = Color.Lerp(bottom, top, ty);

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

