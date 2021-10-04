namespace Unity.VisualScripting.Community
{
    [Widget(typeof(SomeValue))]
    public sealed class SomeValueWidget : UnitWidget<SomeValue>
    {

        public SomeValueWidget(FlowCanvas canvas, SomeValue unit) : base(canvas, unit)
        {
        }

        protected override NodeColorMix baseColor
        {
            get
            {
                return new NodeColorMix() { red = 0.6578709f, green = 1f };
            }
        }
    }
}