using UnityEngine;

namespace GaokaoSimulator.UI
{
    public static class UITheme
    {
        public static Color Bg = new Color32(245, 245, 250, 255);
        public static Color Text = new Color32(51, 51, 51, 255);
        public static Color TextLight = new Color32(120, 120, 130, 255);
        public static Color TextSoft = new Color32(160, 160, 170, 255);
        public static Color Confirm = new Color32(255, 183, 77, 255);
        public static Color ConfirmHover = new Color32(255, 200, 110, 255);
        public static Color Accent = new Color32(255, 152, 0, 255);
        public static Color CardPeach = new Color32(255, 228, 225, 255);
        public static Color CardSky = new Color32(227, 242, 253, 255);
        public static Color CardLavender = new Color32(237, 231, 246, 255);
        public static Color CardMint = new Color32(200, 230, 201, 255);
        public static Color CardButter = new Color32(255, 253, 231, 255);
        public static Color BgCard = new Color32(248, 248, 252, 255);
        public static Color Gold = new Color32(255, 193, 7, 255);
        public static Color GoldLight = new Color32(255, 243, 224, 255);
        public static Color Border = new Color32(200, 200, 210, 255);

        public static float ScaleY(float value) => value;

        public static Color FromHex(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out var color))
            {
                return color;
            }
            return Color.white;
        }
    }
}