namespace Unity.VisualScripting.Community
{
    [Widget(typeof(UnbindActionNode))]
    public sealed class UnbindActionNodeWidget : UnbindDelegateNodeWidget<UnbindActionNode, IAction>
    {
        public UnbindActionNodeWidget(FlowCanvas canvas, UnbindActionNode unit) : base(canvas, unit)
        {
        }
    }
}