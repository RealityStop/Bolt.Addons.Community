namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(ActionNode))]
    public sealed class ActionNodeDescriptor : DelegateNodeDescriptor<ActionNode>
    {
        public ActionNodeDescriptor(ActionNode target) : base(target)
        {
        }

        protected override EditorTexture DefaultIcon()
        {
            return PathUtil.Load("action", CommunityEditorPath.Fundamentals);
        }

        protected override string DefaultName()
        {
            return "Action";
        }

        protected override EditorTexture DefinedIcon()
        {
            return PathUtil.Load("action", CommunityEditorPath.Fundamentals);
        }

        protected override string DefinedName()
        {
            return "Action";
        }
    }
}