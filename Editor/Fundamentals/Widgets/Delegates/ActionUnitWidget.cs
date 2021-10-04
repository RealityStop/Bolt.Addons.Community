using Unity.VisualScripting;
using Unity.VisualScripting.Community.Utility;

namespace Unity.VisualScripting.Community
{
    [Widget(typeof(ActionNode))]
    public sealed class ActionUnitWidget : DelegateUnitWidget<IAction>
    {
        public ActionUnitWidget(FlowCanvas canvas, ActionNode unit) : base(canvas, unit)
        {
        }
    }
}