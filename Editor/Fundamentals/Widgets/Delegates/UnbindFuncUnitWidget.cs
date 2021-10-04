using Unity.VisualScripting;
using Unity.VisualScripting.Community.Utility;

namespace Unity.VisualScripting.Community
{
    [Widget(typeof(UnbindFuncNode))]
    public sealed class UnbindFuncUnitWidget : UnbindDelegateUnitWidget<UnbindFuncNode, IFunc>
    {
        public UnbindFuncUnitWidget(FlowCanvas canvas, UnbindFuncNode unit) : base(canvas, unit)
        {
        }
    }
}