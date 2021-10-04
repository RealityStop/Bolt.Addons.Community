namespace Unity.VisualScripting.Community
{
    [Editor(typeof(UnbindFuncNode))]
    public sealed class UnbindFuncNodeEditor : UnbindDelegateNodeEditor<UnbindFuncNode, IFunc>
    {
        public UnbindFuncNodeEditor(Metadata metadata) : base(metadata)
        {
        }

        protected override string DefaultName => "Unbind";
    }
}