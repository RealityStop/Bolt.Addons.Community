namespace Unity.VisualScripting.Community
{
    [Widget(typeof(ActionNode))]
    public sealed class ActionNodeWidget : DelegateNodeWidget<IAction>
    {
        public ActionNodeWidget(FlowCanvas canvas, ActionNode unit) : base(canvas, unit)
        {
        }
    }
}