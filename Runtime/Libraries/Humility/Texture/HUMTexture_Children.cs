using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMTexture_Children
    {
        /// <summary>
        /// Colors a newly created texture.
        /// </summary>
        public static Texture2D Color(this HUMTexture.Data.Create texture, Color color)
        {
            return HUMTexture.Color(texture.texture, color);
        }

        /// <summary>
        /// Tints a newly created texture.
        /// </summary>
        public static Texture2D Tint(this HUMTexture.Data.Create texture, Color color, float amount)
        {
            return Tint(texture, color, amount);
        }
    }
}
