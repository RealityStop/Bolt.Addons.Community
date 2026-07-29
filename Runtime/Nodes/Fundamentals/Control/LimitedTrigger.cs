namespace Unity.VisualScripting.Community
{
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.TriggerXTimes")]
    [UnitTitle("LimitedTrigger")]
    [UnitCategory("Community\\Control")]
    [TypeIcon(typeof(Once))]
    public class LimitedTrigger : Unit, IGraphElementWithData
    {
        private class Data : IGraphElementData
        {
            public int timesTriggered;
        }

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput Input;

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput Exit;

        [DoNotSerialize]
        public ControlOutput After;

        [DoNotSerialize]
        public ControlInput Reset;

        [DoNotSerialize]
        public ValueInput Times;

        protected override void Definition()
        {
            Input = ControlInput(nameof(Input), IncreaseTimes);
            Reset = ControlInput(nameof(Reset), ResetTimes);
            Exit = ControlOutput(nameof(Exit));
            After = ControlOutput(nameof(After));
            Times = ValueInput<int>(nameof(Times), 1);

            Succession(Reset, Exit);
            Succession(Input, Exit);
            Succession(Input, After);
            Succession(Reset, After);
        }

        private ControlOutput IncreaseTimes(Flow flow)
        {
            var data = flow.stack.GetElementData<Data>(this);
            int timesToTrigger = (int)flow.GetValue(Times);

            if (data.timesTriggered < timesToTrigger)
            {
                data.timesTriggered++;
                return Exit;
            }
            else
            {
                return After;
            }
        }

        private ControlOutput ResetTimes(Flow flow)
        {
            var data = flow.stack.GetElementData<Data>(this);

            data.timesTriggered = 0;
            return null;
        }

        public IGraphElementData CreateData()
        {
            return new Data();
        }
    }
}
