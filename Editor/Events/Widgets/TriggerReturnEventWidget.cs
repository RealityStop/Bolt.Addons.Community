namespace Unity.VisualScripting.Community
{
    /// <summary>
    /// The visuals for the TriggerReturnEvent Unit.
    /// </summary>
    [Widget(typeof(TriggerReturnEvent))]
    public sealed class TriggerReturnEventWidget : UnitWidget<TriggerReturnEvent>
    {
        public TriggerReturnEventWidget(FlowCanvas canvas, TriggerReturnEvent unit) : base(canvas, unit)
        {
        }

        /// <summary>
        /// Sets the TriggerReturnEvent Units color to gray. Since it is an even unit under the hood, we need to make it look like it is not.
        /// </summary>
        protected override NodeColorMix baseColor => NodeColor.Gray;
    }
}