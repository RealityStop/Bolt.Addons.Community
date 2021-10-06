namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(BindFuncNode))]
    public sealed class BindFuncNodeDescriptor : BindDelegateNodeDescriptor<BindFuncNode, IFunc>
    {
        public BindFuncNodeDescriptor(BindFuncNode target) : base(target)
        {
        }

        protected override EditorTexture DefaultIcon()
        {
            return PathUtil.Load("func_bind", CommunityEditorPath.Fundamentals);
        }

        protected override string DefaultName()
        {
            return "Bind";
        }

        protected override EditorTexture DefinedIcon()
        {
            return PathUtil.Load("func_bind", CommunityEditorPath.Fundamentals);
        }

        protected override string DefinedName()
        {
            return "Bind";
        }
    }
}