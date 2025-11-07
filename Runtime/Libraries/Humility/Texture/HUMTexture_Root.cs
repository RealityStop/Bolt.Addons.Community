using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMTexture
    {
        /// <summary>
        /// Checks if two colors are approximately equal to each other, by ignoring floating point inaccuracies.
        /// </summary>
        public static bool Approximately(this Color a, Color b)
        {
            return !(Mathf.Approximately(a.r, b.r) && Mathf.Approximately(a.g, b.g) && Mathf.Approximately(a.b, b.b) && Mathf.Approximately(a.a, b.a));
        }

        /// <summary>
        /// 
        /// </summary>
        public static Data.Create Create(int x, int y)
        {
            return new Data.Create(new Texture2D(x, y));
        }

        /// <summary>
        /// 
        /// </summary>
        public static Data.Create Create(Vector2 size)
        {
            return new Data.Create(new Texture2D((int)size.x, (int)size.y));
        }

        /// <summary>
        /// 
        /// </summary>
        public static Data.Create Create(Vector2Int size)
        {
            return new Data.Create(new Texture2D(size.x, size.y));
        }

        /// <summary>
        /// 
        /// </summary>
        public static Data.Create Create()
        {
            return new Data.Create(new Texture2D(1, 1));
        }

        /// <summary>
        /// Copies a texture.
        /// </summary>
        public static Texture2D Copy(this Texture2D texture)
        {
            return Texture2D.Instantiate<Texture2D>(texture);
        }

        /// <summary>
        /// Tints or recolors the entire texture with the given color.
        /// </summary>
        public static Texture2D Color(this Texture2D texture, Color color)
        {
            int width = texture.width;
            int height = texture.height;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            return texture;
        }

        /// <summary>
        /// Applies a color tint to the texture.
        /// </summary>
        public static Texture2D Tint(this Texture2D texture, Color tint, float strength = 1f, bool tintAlpha = false)
        {
            Color[] pixels = texture.GetPixels();

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i].r = Mathf.Lerp(pixels[i].r, tint.r, Mathf.Clamp(strength, 0, 1));
                pixels[i].g = Mathf.Lerp(pixels[i].g, tint.g, Mathf.Clamp(strength, 0, 1));
                pixels[i].b = Mathf.Lerp(pixels[i].b, tint.b, Mathf.Clamp(strength, 0, 1));

                if (tintAlpha)
                {
                    pixels[i].a = Mathf.Lerp(pixels[i].a, tint.a, Mathf.Clamp(strength, 0, 1));
                }
            }
            texture.SetPixels(pixels);
            texture.Apply();

            return texture;
        }

        /// <summary>
        /// Applies a color tint to the texture by multiplying each pixel's color with the given tint.
        /// </summary>
        public static Texture2D Tint(this Texture2D texture, Color tint)
        {
            Color[] pixels = texture.GetPixels();

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i].r *= tint.r;
                pixels[i].g *= tint.g;
                pixels[i].b *= tint.b;
            }

            texture.SetPixels(pixels);
            texture.Apply();

            return texture;
        }

        /// <summary>
        /// Blends the current texture with another at a set percentage.
        /// </summary>
        public static Texture2D Blend(this Texture2D texture, Texture2D other, float amount)
        {
            Texture2D tintedTexture = Texture2D.Instantiate(texture);
            var otherIsMax = other.GetPixels().Length > tintedTexture.GetPixels().Length;
            Color[] textureColors = texture.GetPixels();
            Color[] otherColors = other.GetPixels();
            var count = otherIsMax ? otherColors.Length : textureColors.Length;

            for (int i = 0; i < count; i++)
            {
                textureColors[i] = UnityEngine.Color.Lerp(textureColors[i], otherColors[i], Mathf.Clamp(amount, 0, 1));
            }

            tintedTexture.SetPixels(textureColors);
            tintedTexture.Apply();

            return tintedTexture;
        }

        /// <summary>
        /// Inverts all color values to there opposite color.
        /// </summary>
        public static Texture2D Invert(this Texture2D texture)
        {
            var pixels = texture.GetPixels();
            var countX = texture.texelSize.x;
            var countY = texture.texelSize.y;

            for (int x = 0; x < countX; x++)
            {
                for (int y = 0; y < countX; y++)
                {
                    var remainingR = 1 - texture.GetPixel(x, y).r;
                    var remainingG = 1 - texture.GetPixel(x, y).g;
                    var remainingB = 1 - texture.GetPixel(x, y).b;
                    var remainingA = 1 - texture.GetPixel(x, y).a;
                    texture.SetPixel(x, y, new UnityEngine.Color(remainingR, remainingB, remainingG, remainingA));
                }
            }

            texture.Apply();
            return texture;
        }
    }
}
