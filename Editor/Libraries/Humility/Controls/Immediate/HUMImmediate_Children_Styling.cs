using System;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMEditor_Immediate_Children
    {
        #region Line Style

        public static HUMEditor.LineStyle Thickness(this HUMEditor.LineStyle style, int amount)
        {
            style.thickness = amount;
            return style;
        }

        public static HUMEditor.LineStyle Color(this HUMEditor.LineStyle style, Color color)
        {
            style.color = color;
            return style;
        }

        #endregion

        #region Canvas Style

        public static HUMEditor.CanvasStyle Spacing(this HUMEditor.CanvasStyle style, int x = 60, int y = 60)
        {
            style.spacing = new Vector2(x, y);
            return style;
        }

        
            private static Texture2D GenerateMainTexture(Color line, Color bg)
            {
                Texture2D tex = new Texture2D(64, 64);
                Color[] cols = new Color[64 * 64];
                for (int y = 0; y < 64; y++)
                {
                    for (int x = 0; x < 64; x++)
                    {
                        Color col = bg;
                        if (y % 16 == 0 || x % 16 == 0) col = UnityEngine.Color.Lerp(line, bg, 0.65f);
                        if (y == 63 || x == 63) col = UnityEngine.Color.Lerp(line, bg, 0.35f);
                        cols[(y * 64) + x] = col;
                    }
                }
                tex.SetPixels(cols);
                tex.wrapMode = TextureWrapMode.Repeat;
                tex.filterMode = FilterMode.Bilinear;
                tex.name = "Grid";
                tex.Apply();
                return tex;
            }

            private static Texture2D GenerateSecondaryTexture(Color line)
            {
                Texture2D tex = new Texture2D(64, 64);
                Color[] cols = new Color[64 * 64];
                for (int y = 0; y < 64; y++)
                {
                    for (int x = 0; x < 64; x++)
                    {
                        Color col = line;
                        if (y != 31 && x != 31) col.a = 0;
                        cols[(y * 64) + x] = col;
                    }
                }
                tex.SetPixels(cols);
                tex.wrapMode = TextureWrapMode.Clamp;
                tex.filterMode = FilterMode.Bilinear;
                tex.name = "Grid";
                tex.Apply();
                return tex;
            }
        

        public static HUMEditor.CanvasStyle LineColor(this HUMEditor.CanvasStyle style, Color color)
        {
            style.lineColor = color;
            style.secondaryTexture = GenerateSecondaryTexture(color);
            return style;
        }

        public static HUMEditor.CanvasStyle BackgroundColor(this HUMEditor.CanvasStyle style, Color color)
        {
            style.backgroundColor = color;
            style.mainTexure = GenerateMainTexture(style.lineColor, color);
            return style;
        }

        #endregion
    }
}
