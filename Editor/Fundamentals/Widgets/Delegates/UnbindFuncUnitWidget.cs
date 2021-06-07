using Unity.VisualScripting;
using Bolt.Addons.Community.Utility;

namespace Bolt.Addons.Community.Fundamentals.Editor
{
    [Widget(typeof(UnbindFuncUnit))]
    public sealed class UnbindFuncUnitWidget : UnbindDelegateUnitWidget<UnbindFuncUnit, IFunc>
    {
        public UnbindFuncUnitWidget(FlowCanvas canvas, UnbindFuncUnit unit) : base(canvas, unit)
        {
        }
    }
}