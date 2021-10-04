using Unity.VisualScripting;
using Unity.VisualScripting.Community.Utility;

namespace Unity.VisualScripting.Community
{
    [Widget(typeof(ActionInvokeNode))]
    public sealed class ActionInvokeUnitWidget : DelegateInvokeUnitWidget<ActionInvokeNode, IAction>
    {
        public ActionInvokeUnitWidget(FlowCanvas canvas, ActionInvokeNode unit) : base(canvas, unit)
        {
        }
    }
}