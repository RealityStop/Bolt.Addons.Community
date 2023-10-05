using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [UnitTitle("Assignment")]//Unit title
    [UnitCategory("Community/Code/Unit")]
    [TypeIcon(typeof(Flow))]//Unit icon
    public class Assignment : Unit
    {
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput Exit;

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput Enter;

        [DoNotSerialize]
        [PortLabel("ControlInput")]
        [AllowsNull]
        public ValueInput Input;

        [DoNotSerialize]
        [PortLabel("ValueOutput")]
        [AllowsNull]
        public ValueInput Output;

        protected override void Definition()
        {
            Enter = ControlInput(nameof(Enter), Logic);
            Exit = ControlOutput(nameof(Exit));

            Input = ValueInput<ControlInput>(nameof(Input)).AllowsNull();
            Output = ValueInput<ControlOutput>(nameof(Output)).AllowsNull();

            Requirement(Input, Enter);
            Requirement(Output, Enter);
            Succession(Enter, Exit);
        }

        public ControlOutput Logic(Flow flow)
        {
            Debug.LogWarning("This node is only for the code generators to understand what to do it does not work in a normal graph");
            return Exit;
        }
    }
}