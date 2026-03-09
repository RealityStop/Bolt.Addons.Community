namespace Unity.VisualScripting.Community
{ 
    [Editor(typeof(UnbindActionNode))]
    public sealed class UnbindActionNodeEditor : UnbindDelegateNodeEditor<UnbindActionNode, IAction>
    {
        public UnbindActionNodeEditor(Metadata metadata) : base(metadata)
        {
        }

        protected override string DefaultName => "Unbind";
    }
}