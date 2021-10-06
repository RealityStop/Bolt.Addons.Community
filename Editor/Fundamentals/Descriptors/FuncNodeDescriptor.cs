using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(FuncNode))]
    public sealed class FuncNodeDescriptor : DelegateNodeDescriptor<FuncNode>
    {
        public FuncNodeDescriptor(FuncNode target) : base(target)
        {
        }
        
        protected override EditorTexture DefaultIcon()
        {
            return PathUtil.Load("func", CommunityEditorPath.Fundamentals);
        }

        protected override string DefaultName()
        {
            return "Func";
        }

        protected override EditorTexture DefinedIcon()
        {
            return PathUtil.Load("func", CommunityEditorPath.Fundamentals);
        }

        protected override string DefinedName()
        {
            return "Func";
        }
    }
}