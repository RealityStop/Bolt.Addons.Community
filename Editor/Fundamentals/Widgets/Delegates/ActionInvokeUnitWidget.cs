using Ludiq;
using Bolt.Addons.Community.Fundamentals.Units.logic;
using Bolt.Addons.Community.Utility;

namespace Bolt.Addons.Community.Fundamentals.Units.Utility.Editor
{
    [Widget(typeof(ActionInvokeUnit))]
    public sealed class ActionInvokeUnitWidget : DelegateInvokeUnitWidget<ActionInvokeUnit, IAction>
    {
        public ActionInvokeUnitWidget(FlowCanvas canvas, ActionInvokeUnit unit) : base(canvas, unit)
        {
        }
    }
}