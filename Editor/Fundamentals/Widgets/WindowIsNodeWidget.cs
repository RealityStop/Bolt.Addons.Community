namespace Unity.VisualScripting.Community
{
    [Widget(typeof(WindowIsNode))]
    public class WindowIsNodeWidget : UnitWidget<WindowIsNode>
    {
        public WindowIsNodeWidget(FlowCanvas canvas, WindowIsNode unit) : base(canvas, unit)
        {
        }

        protected override NodeColorMix baseColor => NodeColorMix.TealReadable;
    }
}
