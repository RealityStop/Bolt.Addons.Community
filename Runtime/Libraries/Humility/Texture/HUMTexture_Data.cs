using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMTexture
    {
        /// <summary>
        /// Structs for passing data down a texture operation.
        /// </summary>
        public partial class Data
        {
            /// <summary>
            /// Texture operation data for creating a texture.
            /// </summary>
            public struct Create
            {
                public Texture2D texture;

                public Create(Texture2D texture)
                {
                    this.texture = texture;
                }
            }
        }
    }
}
