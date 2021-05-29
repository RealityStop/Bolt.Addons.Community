using Unity.VisualScripting;
using Bolt.Addons.Community.Fundamentals.Units.logic;
using Bolt.Addons.Community.Utility;

namespace Bolt.Addons.Community.Fundamentals.Units.Utility.Editor
{
    [Widget(typeof(BindActionUnit))]
    public sealed class BindActionUnitWidget : BindDelegateUnitWidget<BindActionUnit, IAction>
    {
        public BindActionUnitWidget(FlowCanvas canvas, BindActionUnit unit) : base(canvas, unit)
        {
        }
    }
}