namespace Unity.VisualScripting.Community
{
    [Widget(typeof(StuffHappens))]
    public sealed class StuffHappensWidget : UnitWidget<StuffHappens>
    {

        public StuffHappensWidget(FlowCanvas canvas, StuffHappens unit) : base(canvas, unit)
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