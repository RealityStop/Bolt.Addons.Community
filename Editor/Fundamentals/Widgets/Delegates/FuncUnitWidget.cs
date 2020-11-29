using Ludiq;
using Bolt.Addons.Community.Fundamentals.Units.logic;
using Bolt.Addons.Community.Utility;

namespace Bolt.Addons.Community.Fundamentals.Units.Utility.Editor
{
    [Widget(typeof(FuncUnit))]
    public sealed class FuncUnitWidget : DelegateUnitWidget<FuncUnit, IFunc>
    {
        public FuncUnitWidget(FlowCanvas canvas, FuncUnit unit) : base(canvas, unit)
        {
        }
    }
}