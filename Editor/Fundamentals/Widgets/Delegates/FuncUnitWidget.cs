using Unity.VisualScripting;
using Bolt.Addons.Community.Utility;

namespace Bolt.Addons.Community.Fundamentals.Editor
{
    [Widget(typeof(FuncUnit))]
    public sealed class FuncUnitWidget : DelegateUnitWidget<IFunc>
    {
        public FuncUnitWidget(FlowCanvas canvas, FuncUnit unit) : base(canvas, unit)
        {
        }
    }
}