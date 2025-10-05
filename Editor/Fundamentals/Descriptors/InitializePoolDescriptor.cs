using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community 
{
    [Descriptor(typeof(InitializePoolNode))]
    public sealed class InitializePoolNodeDescriptor : UnitDescriptor<InitializePoolNode>
    {
        public InitializePoolNodeDescriptor(InitializePoolNode target) : base(target)
        {
        }
        protected override EditorTexture DefinedIcon()
        {
            string iconFullPath = "Packages/dev.bolt.addons/Editor/Fundamentals/Resources/Pool.png";
            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconFullPath);
            return EditorTexture.Single(icon);
        }
        protected override string DefinedSummary()
        {
            return "Create Initial Pool";
        }
    } 
}