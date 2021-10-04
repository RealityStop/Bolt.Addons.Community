using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMEditorTexture
    {
        /// <summary>
        /// Ensures the field is assigned a texture at a path.
        /// </summary>
        public static void Cache(ref Texture2D textureReference, string path)
        {
            if (textureReference == null) textureReference = (Texture2D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D));
        }
    }
}
