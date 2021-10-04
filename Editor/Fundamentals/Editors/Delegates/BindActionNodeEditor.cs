namespace Unity.VisualScripting.Community
{
    [Editor(typeof(BindActionNode))]
    public sealed class BindActionNodeEditor : BindDelegateNodeEditor<BindActionNode, IAction>
    {
        public BindActionNodeEditor(Metadata metadata) : base(metadata)
        {
        }

        protected override string DefaultName => "Bind";
    }
}