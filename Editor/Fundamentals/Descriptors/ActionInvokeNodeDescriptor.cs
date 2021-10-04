namespace Unity.VisualScripting.Community
{
    [Descriptor(typeof(ActionInvokeNode))]
    public sealed class ActionInvokeNodeDescriptor : DelegateInvokeNodeDesriptor<ActionInvokeNode, IAction>
    {
        protected override string Prefix => "Invoke";
        protected override string FallbackName => "Action";

        public ActionInvokeNodeDescriptor(ActionInvokeNode target) : base(target)
        {
        }

        protected override EditorTexture DefaultIcon()
        {
            return PathUtil.Load("action_invoke", CommunityEditorPath.Fundamentals);
        }

        protected override EditorTexture DefinedIcon()
        {
            return PathUtil.Load("action_invoke", CommunityEditorPath.Fundamentals);
        }
    }
}