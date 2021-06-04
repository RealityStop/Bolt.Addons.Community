using Unity.VisualScripting;
using Bolt.Addons.Community.Fundamentals.Units.logic;
using Bolt.Addons.Community.Utility;

namespace Bolt.Addons.Community.Fundamentals.Units.Utility.Editor
{
    [Widget(typeof(UnbindActionUnit))]
    public sealed class UnbindActionUnitWidget : UnbindDelegateUnitWidget<UnbindActionUnit, IAction>
    {
        public UnbindActionUnitWidget(FlowCanvas canvas, UnbindActionUnit unit) : base(canvas, unit)
        {
        }
    }
}