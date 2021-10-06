using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public sealed class CommentPaletteRoutine : DeserializedRoutine
    {
        public override void Run()
        {
            if (CommentNode.style == null)
            {
                var path = "Assets/Unity.VisualScripting.Community.Generated/";
                HUMIO.Ensure(path).Path();
                CommentPalette style = AssetDatabase.LoadAssetAtPath<CommentPalette>(path + "GlobalCommentStyle.asset");

                if (style == null)
                {
                    style = ScriptableObject.CreateInstance<CommentPalette>();
                    style.name = "GlobalCommentStyle";
                    AssetDatabase.CreateAsset(style, path + "GlobalCommentStyle.asset");
                }

                CommentNode.style = style;
            }
        }
    }
} 