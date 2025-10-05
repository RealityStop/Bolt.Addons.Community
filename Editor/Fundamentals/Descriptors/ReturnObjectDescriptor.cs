using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(ReturnObjectNode))]
    public class ReturnObjectNodeDescriptor : UnitDescriptor<ReturnObjectNode>
    {
        public ReturnObjectNodeDescriptor(ReturnObjectNode target) : base(target)
        {
        }
        protected override EditorTexture DefinedIcon()
        {
            string iconFullPath = "Packages/dev.bolt.addons/Editor/Fundamentals/Resources/ReturnObject.png";
            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconFullPath);
            return EditorTexture.Single(icon);
        }
        protected override string DefinedSummary()
        {
            return "Return A Object To The Pool";
        }
    }
}
