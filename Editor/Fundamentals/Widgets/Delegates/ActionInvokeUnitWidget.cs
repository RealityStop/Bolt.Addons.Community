using Unity.VisualScripting;
using Bolt.Addons.Community.Utility;

namespace Bolt.Addons.Community.Fundamentals.Editor
{
    [Widget(typeof(ActionInvokeUnit))]
    public sealed class ActionInvokeUnitWidget : DelegateInvokeUnitWidget<ActionInvokeUnit, IAction>
    {
        public ActionInvokeUnitWidget(FlowCanvas canvas, ActionInvokeUnit unit) : base(canvas, unit)
        {
        }
    }
}