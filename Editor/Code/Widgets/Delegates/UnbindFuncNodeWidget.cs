namespace Unity.VisualScripting.Community
{
    [Widget(typeof(UnbindFuncNode))]
    public sealed class UnbindFuncNodeWidget : UnbindDelegateNodeWidget<UnbindFuncNode, IFunc>
    {
        public UnbindFuncNodeWidget(FlowCanvas canvas, UnbindFuncNode unit) : base(canvas, unit)
        {
        }
    }
}