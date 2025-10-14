using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(ReturnAllObjectsToPoolNode))]
    public class ReturnAllObjectsToPoolNodeDescriptor : UnitDescriptor<ReturnAllObjectsToPoolNode>
    {
        public ReturnAllObjectsToPoolNodeDescriptor(ReturnAllObjectsToPoolNode target) : base(target)
        {
        }

        protected override EditorTexture DefinedIcon()
        {
            return PathUtil.Load("ReturnObject", CommunityEditorPath.Fundamentals);
        }
        protected override string DefinedSummary()
        {
            return "Return All Object To The Pool";
        }
    }
}