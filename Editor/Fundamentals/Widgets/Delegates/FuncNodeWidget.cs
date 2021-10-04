namespace Unity.VisualScripting.Community
{
    [Widget(typeof(FuncNode))]
    public sealed class FuncNodeWidget : DelegateNodeWidget<IFunc>
    {
        public FuncNodeWidget(FlowCanvas canvas, FuncNode unit) : base(canvas, unit)
        {
        }
    }
}