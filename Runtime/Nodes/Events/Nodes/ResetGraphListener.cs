namespace Unity.VisualScripting.Community
{
    [TypeIcon(typeof(EventUnit<CustomEventArgs>))]
    [UnitCategory("Events/Community")]
    public sealed class ResetGraphListener : Unit
    {
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter;
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput exit;

        protected override void Definition()
        {
            enter = ControlInput("enter", (flow) =>
            {
                graph.StopListening(flow.stack.AsReference());
                graph.StartListening(flow.stack.AsReference());
                return exit;
            });

            exit = ControlOutput("exit");

            Succession(enter, exit);
        }
    }
}