using Unity.VisualScripting;
using Bolt.Addons.Community.Utility;

namespace Bolt.Addons.Community.Fundamentals.Editor
{
    [Widget(typeof(UnbindActionUnit))]
    public sealed class UnbindActionUnitWidget : UnbindDelegateUnitWidget<UnbindActionUnit, IAction>
    {
        public UnbindActionUnitWidget(FlowCanvas canvas, UnbindActionUnit unit) : base(canvas, unit)
        {
        }
    }
}