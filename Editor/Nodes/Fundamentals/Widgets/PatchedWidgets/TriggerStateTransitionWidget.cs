namespace Unity.VisualScripting.Community
{
    public sealed class TriggerStateTransitionWidget : UnitWidget<TriggerStateTransition>
    {
        public TriggerStateTransitionWidget(FlowCanvas canvas, TriggerStateTransition unit) : base(canvas, unit) { }

        protected override NodeColorMix baseColor => NodeColorMix.TealReadable;
    }
}