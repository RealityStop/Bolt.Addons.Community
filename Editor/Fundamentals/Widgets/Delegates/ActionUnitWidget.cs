using Ludiq;
using Bolt.Addons.Community.Fundamentals.Units.logic;
using Bolt.Addons.Community.Utility;

namespace Bolt.Addons.Community.Fundamentals.Units.Utility.Editor
{
    [Widget(typeof(ActionUnit))]
    public sealed class ActionUnitWidget : DelegateUnitWidget<ActionUnit, IAction>
    {
        public ActionUnitWidget(FlowCanvas canvas, ActionUnit unit) : base(canvas, unit)
        {
        }
    }
}