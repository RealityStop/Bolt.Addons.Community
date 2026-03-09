namespace Unity.VisualScripting.Community
{
    [Widget(typeof(BindFuncNode))]
    public sealed class BindFuncNodeWidget : BindDelegateNodeWidget<BindFuncNode, IFunc>
    {
        public BindFuncNodeWidget(FlowCanvas canvas, BindFuncNode unit) : base(canvas, unit)
        {
        }
    }
}