using Bolt.Addons.Community.Fundamentals.Units.Documenting;
using Bolt.Addons.Libraries.Humility;
using UnityEditor;
using UnityEngine;

namespace Bolt.Addons.Community.Processing
{
    public sealed class CommentStyleRoutine : DeserializedRoutine
    {
        public override void Run()
        {
            if (CommentUnit.style == null)
            {
                var path = "Assets/Bolt.Addons.Generated/";
                HUMIO.Ensure(path).Path();
                CommentPalette style = AssetDatabase.LoadAssetAtPath<CommentPalette>(path + "GlobalCommentStyle.asset");

                if (style == null)
                {
                    style = ScriptableObject.CreateInstance<CommentPalette>();
                    style.name = "GlobalCommentStyle";
                    AssetDatabase.CreateAsset(style, path + "GlobalCommentStyle.asset");
                }

                CommentUnit.style = style;
            }
        }
    }
} 