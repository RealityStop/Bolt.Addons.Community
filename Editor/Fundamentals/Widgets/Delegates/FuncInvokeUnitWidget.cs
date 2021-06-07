using Unity.VisualScripting;
using Bolt.Addons.Community.Utility;

namespace Bolt.Addons.Community.Fundamentals.Editor
{
    [Widget(typeof(FuncInvokeUnit))]
    public sealed class FuncInvokeUnitWidget : DelegateInvokeUnitWidget<FuncInvokeUnit, IFunc>
    {
        public FuncInvokeUnitWidget(FlowCanvas canvas, FuncInvokeUnit unit) : base(canvas, unit)
        {
        }
    }
}