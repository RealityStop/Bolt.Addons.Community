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
            return PathUtil.Load("ReturnObject", CommunityEditorPath.Fundamentals);
        }
        protected override string DefinedSummary()
        {
            return "Return A Object To The Pool";
        }
    }
}
