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
            return PathUtil.Load("RetrieveObject", CommunityEditorPath.Fundamentals);
        }
        protected override string DefinedSummary()
        {
            return "Get Object From Pool";
        }
    }
}