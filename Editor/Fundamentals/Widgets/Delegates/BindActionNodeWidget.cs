namespace Unity.VisualScripting.Community
{
    [Widget(typeof(BindActionNode))]
    public sealed class BindActionNodeWidget : BindDelegateNodeWidget<BindActionNode, IAction>
    {
        public BindActionNodeWidget(FlowCanvas canvas, BindActionNode unit) : base(canvas, unit)
        {
        }
    }
}