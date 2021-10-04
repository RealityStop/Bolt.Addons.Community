using Unity.VisualScripting;
using Unity.VisualScripting.Community.Utility;

namespace Unity.VisualScripting.Community
{
    [Widget(typeof(BindActionNode))]
    public sealed class BindActionUnitWidget : BindDelegateUnitWidget<BindActionNode, IAction>
    {
        public BindActionUnitWidget(FlowCanvas canvas, BindActionNode unit) : base(canvas, unit)
        {
        }
    }
}