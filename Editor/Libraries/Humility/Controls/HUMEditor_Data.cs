using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMEditor
    {
        public static partial class Data
        {
            public struct Immediate
            {
                public Rect rect;

                public Immediate(Rect rect)
                {
                    this.rect = rect;
                }
            }

            public struct Render
            {

            }

            public struct Zoom
            {
                public Rect rect;

                public Zoom(Rect rect)
                {
                    this.rect = rect;
                }
            }
        }

        public class LineStyle
        {
            public float thickness;
            public Color color;
        }

        public class CanvasStyle
        {
            public Texture2D mainTexure;
            public Texture2D secondaryTexture;
            public Vector2 spacing;
            public Color backgroundColor;
            public Color lineColor;

            private CanvasStyle()
            {
                
            }

            public static CanvasStyle Create()
            {
                var style = new CanvasStyle();
                style.spacing = new Vector2(64, 64);
                style.backgroundColor = Color.black;
                style.lineColor = Color.white;
                style.mainTexure = GenerateMainTexture(style.lineColor, style.backgroundColor);
                style.secondaryTexture = GenerateSecondaryTexture(style.lineColor);
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
        }
    }
}
