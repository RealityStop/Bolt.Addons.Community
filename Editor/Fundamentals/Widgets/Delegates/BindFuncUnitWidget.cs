using Unity.VisualScripting;
using Bolt.Addons.Community.Fundamentals.Units.logic;
using Bolt.Addons.Community.Utility;

namespace Bolt.Addons.Community.Fundamentals.Units.Utility.Editor
{
    [Widget(typeof(BindFuncUnit))]
    public sealed class BindFuncUnitWidget : BindDelegateUnitWidget<BindFuncUnit, IFunc>
    {
        public BindFuncUnitWidget(FlowCanvas canvas, BindFuncUnit unit) : base(canvas, unit)
        {
        }
    }
}