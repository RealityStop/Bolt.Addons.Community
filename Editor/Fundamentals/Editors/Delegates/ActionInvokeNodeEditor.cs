namespace Unity.VisualScripting.Community
{
    [Editor(typeof(ActionInvokeNode))]
    public sealed class ActionInvokeNodeEditor : DelegateInvokeNodeEditor<ActionInvokeNode, IAction>
    {
        public ActionInvokeNodeEditor(Metadata metadata) : base(metadata)
        {
        }

        protected override string DefaultName => "Action";
    }
}