using Unity.VisualScripting;
using Bolt.Addons.Community.Utility;

namespace Bolt.Addons.Community.Fundamentals.Editor
{
    [Widget(typeof(ActionUnit))]
    public sealed class ActionUnitWidget : DelegateUnitWidget<IAction>
    {
        public ActionUnitWidget(FlowCanvas canvas, ActionUnit unit) : base(canvas, unit)
        {
        }
    }
}