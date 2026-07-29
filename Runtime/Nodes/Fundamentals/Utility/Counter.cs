using UnityEngine;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.CounterNode")]
    [UnitTitle("Counter")]
    [UnitCategory("Community\\Utility")]
    [TypeIcon(typeof(Add<object>))]
    public class CounterNode : Unit, IGraphElementWithData
    {
        private class Data : IGraphElementData
        {
            public int counter;
        }

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter;

        [DoNotSerialize]
        public ControlInput reset;

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput exit;

        [DoNotSerialize]
        public ValueOutput timesTriggered;

        private int counter;

        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), OnEnter);
            reset = ControlInput(nameof(reset), OnReset);
            exit = ControlOutput(nameof(exit));

            timesTriggered = ValueOutput<int>(nameof(timesTriggered));

            Succession(enter, exit);
            Succession(reset, exit);
        }

        public ControlOutput OnEnter(Flow flow)
        {
            var data = flow.stack.GetElementData<Data>(this);
            data.counter++;
            flow.SetValue(timesTriggered, counter);
            return exit;
        }

        public ControlOutput OnReset(Flow flow)
        {
            var data = flow.stack.GetElementData<Data>(this);
            data.counter = 0;
            flow.SetValue(timesTriggered, counter);
            return null;
        }

        public IGraphElementData CreateData()
        {
            return new Data();
        }
    }
}
