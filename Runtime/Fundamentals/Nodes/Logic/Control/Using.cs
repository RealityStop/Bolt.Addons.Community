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
            enter = ControlInputCoroutine(nameof(enter), Trigger, TriggerCoroutine);
            exit = ControlOutput(nameof(exit));
            body = ControlOutput(nameof(body));
            value = ValueInput<IDisposable>(nameof(value), default);
            Succession(enter, exit);
            Succession(enter, body);
        }
        public ControlOutput Trigger(Flow flow) 
        {
            var disposable = flow.GetValue<IDisposable>(value);
            using (disposable)
            {
                flow.Invoke(body);
            }
            return exit;
        }
        public IEnumerator TriggerCoroutine(Flow flow) 
        {
            var disposable = flow.GetValue<IDisposable>(value);
            using (disposable)
            {
                yield return body;
            }
            yield return exit;
        }
    } 
}