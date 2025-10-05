using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(RetrieveObjectNode))]
    public class RetrieveObjectNodeDescriptor : UnitDescriptor<RetrieveObjectNode>
    {
        public RetrieveObjectNodeDescriptor(RetrieveObjectNode target) : base(target)
        {
        }
        protected override EditorTexture DefinedIcon()
        {
            string iconFullPath = "Packages/dev.bolt.addons/Editor/Fundamentals/Resources/RetrievePool.png";
            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconFullPath);
            return EditorTexture.Single(icon);
        }
        protected override string DefinedSummary()
        {
            return "Get Object From Pool";
        }
    }
}