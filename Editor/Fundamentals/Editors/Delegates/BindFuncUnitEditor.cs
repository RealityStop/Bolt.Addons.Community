namespace Unity.VisualScripting.Community
{
    [Editor(typeof(BindFuncNode))]
    public sealed class BindFuncNodeEditor : BindDelegateNodeEditor<BindFuncNode, IFunc>
    {
        public BindFuncNodeEditor(Metadata metadata) : base(metadata)
        {
        }

        protected override string DefaultName => "Bind";
    }
}