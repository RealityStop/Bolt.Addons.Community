using Unity.VisualScripting;
using Unity.VisualScripting.Community.Utility;

namespace Unity.VisualScripting.Community
{
    [Widget(typeof(FuncNode))]
    public sealed class FuncUnitWidget : DelegateUnitWidget<IFunc>
    {
        public FuncUnitWidget(FlowCanvas canvas, FuncNode unit) : base(canvas, unit)
        {
        }
    }
}