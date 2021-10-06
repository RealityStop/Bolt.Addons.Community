namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(UnbindFuncNode))]
    public sealed class UnbindFuncNodeDescriptor : UnbindDelegateNodeDescriptor<UnbindFuncNode, IFunc>
    {
        public UnbindFuncNodeDescriptor(UnbindFuncNode target) : base(target)
        {
        }

        protected override EditorTexture DefaultIcon()
        {
            return PathUtil.Load("func_unbind", CommunityEditorPath.Fundamentals);
        }

        protected override string DefaultName()
        {
            return "Unbind";
        }

        protected override EditorTexture DefinedIcon()
        {
            return PathUtil.Load("func_unbind", CommunityEditorPath.Fundamentals);
        }

        protected override string DefinedName()
        {
            return "Unbind";
        }
    }
}