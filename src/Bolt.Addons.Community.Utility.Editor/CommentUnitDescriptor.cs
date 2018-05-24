using Ludiq;
using Bolt;
using UnityEngine;
using UnityEditor;

namespace Bolt.Addons.Community.Utility.Editor
{
    [RenamedFrom("Lasm.Bolt.Comments")]
    [Descriptor(typeof(CommentUnit))]
    public class CommentUnitDescriptor : UnitDescriptor<CommentUnit>
    {
        public CommentUnitDescriptor(CommentUnit unit) : base(unit)
        {

        }

        public Texture2D icon;

        protected override EditorTexture DefaultIcon()
        {
            if (icon == null)
            {
                if (icon == null)
                {
                    string[] resourcePath = AssetDatabase.FindAssets("CommentUnit");
                    Debug.Log(AssetDatabase.GUIDToAssetPath(resourcePath[0]));
                    icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(resourcePath[0]).Replace("CommentUnit.cs", string.Empty) + "Resources/CommentsIcon_32px.png");
                }
            }

            return EditorTexture.Single(icon);
        }

        protected override EditorTexture DefinedIcon()
        {
            if (icon == null)
            {
                if (icon == null)
                {
                    string[] resourcePath = AssetDatabase.FindAssets("CommentUnit");
                    Debug.Log(AssetDatabase.GUIDToAssetPath(resourcePath[0]));
                    icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(resourcePath[0]).Replace("CommentUnit.cs", string.Empty) + "Resources/CommentsIcon_32px.png");
                }
            }

            return EditorTexture.Single(icon);
        }
    }
}
