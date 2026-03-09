namespace Unity.VisualScripting.Community
{
    [Widget(typeof(FuncInvokeNode))]
    public sealed class FuncInvokeNodeWidget : DelegateInvokeNodeWidget<FuncInvokeNode, IFunc>
    {
        public FuncInvokeNodeWidget(FlowCanvas canvas, FuncInvokeNode unit) : base(canvas, unit)
        {
        }
    }
}