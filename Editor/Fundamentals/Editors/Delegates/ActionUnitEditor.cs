namespace Unity.VisualScripting.Community
{
    [Editor(typeof(ActionNode))]
    public sealed class ActionNodeEditor : DelegateNodeEditor<ActionNode, IAction>
    {
        public ActionNodeEditor(Metadata metadata) : base(metadata)
        {
        }

        protected override string DefaultName => "Action";
    }
}