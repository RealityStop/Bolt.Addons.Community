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
            return PathUtil.Load("Pool", CommunityEditorPath.Fundamentals);
        }
        protected override string DefinedSummary()
        {
            return "Create Initial Pool";
        }
    } 
}