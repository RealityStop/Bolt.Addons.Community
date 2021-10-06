namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(BindActionNode))]
    public sealed class BindActionNodeDescriptor : BindDelegateNodeDescriptor<BindActionNode, IAction>
    {
        public BindActionNodeDescriptor(BindActionNode target) : base(target)
        {
        }

        protected override EditorTexture DefaultIcon()
        {
            return PathUtil.Load("action_bind", CommunityEditorPath.Fundamentals);
        }

        protected override string DefaultName()
        {
            return "Bind";
        }

        protected override EditorTexture DefinedIcon()
        {
            return PathUtil.Load("action_bind", CommunityEditorPath.Fundamentals);
        }

        protected override string DefinedName()
        {
            return "Bind";
        }
    }
}