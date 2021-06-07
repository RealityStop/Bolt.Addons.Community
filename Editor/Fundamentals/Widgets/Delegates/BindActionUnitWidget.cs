using Unity.VisualScripting;
using Bolt.Addons.Community.Utility;

namespace Bolt.Addons.Community.Fundamentals.Editor
{
    [Widget(typeof(BindActionUnit))]
    public sealed class BindActionUnitWidget : BindDelegateUnitWidget<BindActionUnit, IAction>
    {
        public BindActionUnitWidget(FlowCanvas canvas, BindActionUnit unit) : base(canvas, unit)
        {
        }
    }
}