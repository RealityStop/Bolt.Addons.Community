namespace Unity.VisualScripting.Community
{
    [Editor(typeof(FuncNode))]
    public sealed class FuncNodeEditor : DelegateNodeEditor<FuncNode, IFunc>
    {
        public FuncNodeEditor(Metadata metadata) : base(metadata)
        {
        }

        protected override string DefaultName => "Func";
    }
}