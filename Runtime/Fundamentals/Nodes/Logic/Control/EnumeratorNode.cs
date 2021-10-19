using System.Collections;

namespace Unity.VisualScripting.Community
{
    [UnitTitle("Enumerator")]
    [UnitCategory("Community/Time")]
    [TypeIcon(typeof(WaitUnit))]
    public sealed class EnumeratorNode : Unit
    {
        public override bool isControlRoot => true;

        [PortLabelHidden]
        public ControlOutput trigger;
        [PortLabelHidden]
        public ValueOutput enumerator;

        protected override void Definition()
        {
            trigger = ControlOutput("trigger");
            enumerator = ValueOutput("enumerator", Run);
        }

        public IEnumerator Run(Flow flow)
        {
            yield return trigger;
        }
    }
}
