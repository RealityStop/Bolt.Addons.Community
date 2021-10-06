namespace Unity.VisualScripting.Community
{
    [Widget(typeof(ActionInvokeNode))]
    public sealed class ActionInvokeNodeWidget : DelegateInvokeNodeWidget<ActionInvokeNode, IAction>
    {
        public ActionInvokeNodeWidget(FlowCanvas canvas, ActionInvokeNode unit) : base(canvas, unit)
        {
        }
    }
}