using UnityEngine;

namespace GaokaoSimulator.UI
{
    public static class UITheme
    {
        public static readonly Vector2 ReferenceResolution = new Vector2(1242f, 2760f);

        public static readonly Color32 Bg = FromHex("FFF8F0");
        public static readonly Color32 BgCard = FromHex("FFFFFF");
        public static readonly Color32 Text = FromHex("3D3D3D");
        public static readonly Color32 TextLight = FromHex("8D8D8D");
        public static readonly Color32 TextSoft = FromHex("6D6D6D");
        public static readonly Color32 Border = FromHex("E8E8E8");

        public static readonly Color32 Accent = FromHex("FF8A80");
        public static readonly Color32 AccentHover = FromHex("FF5252");
        public static readonly Color32 Confirm = FromHex("64B5F6");
        public static readonly Color32 ConfirmHover = FromHex("42A5F5");
        public static readonly Color32 Gold = FromHex("FFB300");
        public static readonly Color32 GoldLight = FromHex("FFF8E1");

        public static readonly Color32 CardMint = FromHex("E8F5E9");
        public static readonly Color32 CardLavender = FromHex("F3E5F5");
        public static readonly Color32 CardPeach = FromHex("FFF0E6");
        public static readonly Color32 CardButter = FromHex("FFFDE7");
        public static readonly Color32 CardSky = FromHex("E3F2FD");

        public static readonly float RadiusLarge = 58f;
        public static readonly float RadiusMedium = 40f;
        public static readonly float RadiusSmall = 28f;
        public static readonly float RadiusPill = 160f;

        public static readonly float SafePaddingHorizontal = ScaleX(24f, 430f);
        public static readonly float SafePaddingVertical = ScaleY(24f, 764f);

        public static float ScaleX(float value, float baseWidth = 430f)
        {
            return value * (ReferenceResolution.x / baseWidth);
        }

        public static float ScaleY(float value, float baseHeight = 764f)
        {
            return value * (ReferenceResolution.y / baseHeight);
        }

        public static Color32 FromHex(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
            {
                return new Color32(255, 255, 255, 255);
            }

            hex = hex.Trim().TrimStart('#');
            if (hex.Length == 6)
            {
                var r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                var g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                var b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                return new Color32(r, g, b, 255);
            }

            if (hex.Length == 8)
            {
                var r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                var g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                var b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                var a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
                return new Color32(r, g, b, a);
            }

            return new Color32(255, 255, 255, 255);
        }
    }
}

