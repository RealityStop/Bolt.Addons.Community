using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community 
{
    [Descriptor(typeof(Arrow))]
    [RenamedFrom("ArrowDescriptor")]
    public class ArrowDescriptor : UnitDescriptor<Arrow>
    {
        public ArrowDescriptor(Arrow target) : base(target)
        {
        }
        protected override EditorTexture DefinedIcon()
        {
            return PathUtil.Load("Arrow", CommunityEditorPath.Fundamentals);
        }
    } 
}
