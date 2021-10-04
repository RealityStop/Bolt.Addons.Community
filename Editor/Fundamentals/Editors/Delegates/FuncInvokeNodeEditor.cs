
namespace Unity.VisualScripting.Community
{
    [Editor(typeof(FuncInvokeNode))]
    public sealed class FuncInvokeNodeEditor : DelegateInvokeNodeEditor<FuncInvokeNode, IFunc>
    {
        public FuncInvokeNodeEditor(Metadata metadata) : base(metadata)
        {
        }

        protected override string DefaultName => "Func";
    }
}