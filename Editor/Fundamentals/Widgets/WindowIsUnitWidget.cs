using Unity.VisualScripting;

namespace Bolt.Addons.Community.Utility.Editor
{
    [Widget(typeof(WindowIsUnit))]
    public class WindowIsUnitWidget : UnitWidget<WindowIsUnit>
    {
        public WindowIsUnitWidget(FlowCanvas canvas, WindowIsUnit unit) : base(canvas, unit)
        {
        }

        protected override NodeColorMix baseColor => NodeColorMix.TealReadable;
    }
}
