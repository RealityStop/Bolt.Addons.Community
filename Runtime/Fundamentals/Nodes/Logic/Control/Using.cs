using System;
using System.Collections;

namespace Unity.VisualScripting.Community
{
    [UnitCategory("Community/Control")]
    [UnitTitle("Using")]
    [TypeIcon(typeof(If))]
    public class Using : Unit
    {
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput enter;
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput exit;
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput body;
        [DoNotSerialize]
        [PortLabelHidden]
        public ValueInput value;
        protected override void Definition()
        {
            enter = ControlInput(nameof(enter), Trigger);
            exit = ControlOutput(nameof(exit));
            body = ControlOutput(nameof(body));
            value = ValueInput<IDisposable>(nameof(value), default);
            Succession(enter, exit);
            Succession(enter, body);
        }
        public ControlOutput Trigger(Flow flow)
        {
            if (flow.isCoroutine)
            {
                throw new NotSupportedException("The 'using' statement cannot be used with coroutines.");
            }
            var disposable = flow.GetValue<IDisposable>(value);
            using (disposable)
            {
                flow.Invoke(body);
            }
            return exit;
        }
    }
}