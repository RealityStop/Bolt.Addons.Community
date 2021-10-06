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
        /// Changes the color of a texture.
        /// </summary>
        public static Texture2D Color(this Texture2D texture, Color color)
        {
            var count = texture.texelSize;

            for (int x = 0; x < count.x; x++)
            {
                for (int y = 0; y < count.y; y++)
                {
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();

            return texture;
        }

        /// <summary>
        /// Changes the color of a texture with a specific amount of tinting.
        /// </summary>
        public static Texture2D Tint(this Texture2D texture, Color color, float amount)
        {
            Color[] textureColors = texture.GetPixels();

            for (int i = 0; i < textureColors.Length; i++)
            {
                textureColors[i] = UnityEngine.Color.Lerp(textureColors[i], color, Mathf.Clamp(amount, 0, 1));
            }

            texture.SetPixels(textureColors);
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
