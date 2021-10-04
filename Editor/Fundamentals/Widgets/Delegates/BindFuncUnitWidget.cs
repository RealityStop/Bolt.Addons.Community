using Unity.VisualScripting;
using Unity.VisualScripting.Community.Utility;

namespace Unity.VisualScripting.Community
{
    [Widget(typeof(BindFuncNode))]
    public sealed class BindFuncUnitWidget : BindDelegateUnitWidget<BindFuncNode, IFunc>
    {
        public BindFuncUnitWidget(FlowCanvas canvas, BindFuncNode unit) : base(canvas, unit)
        {
        }
    }
}