using Unity.VisualScripting;
using Unity.VisualScripting.Community.Utility;

namespace Unity.VisualScripting.Community
{
    [Widget(typeof(FuncInvokeNode))]
    public sealed class FuncInvokeUnitWidget : DelegateInvokeUnitWidget<FuncInvokeNode, IFunc>
    {
        public FuncInvokeUnitWidget(FlowCanvas canvas, FuncInvokeNode unit) : base(canvas, unit)
        {
        }
    }
}