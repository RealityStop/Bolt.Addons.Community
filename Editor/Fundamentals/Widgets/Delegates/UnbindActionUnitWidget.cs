using Unity.VisualScripting;
using Unity.VisualScripting.Community.Utility;

namespace Unity.VisualScripting.Community
{
    [Widget(typeof(UnbindActionNode))]
    public sealed class UnbindActionUnitWidget : UnbindDelegateUnitWidget<UnbindActionNode, IAction>
    {
        public UnbindActionUnitWidget(FlowCanvas canvas, UnbindActionNode unit) : base(canvas, unit)
        {
        }
    }
}