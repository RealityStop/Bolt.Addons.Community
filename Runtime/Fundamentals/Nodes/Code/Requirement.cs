using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [UnitTitle("Requirement")]//Unit title
    [UnitCategory("Community/Code/Unit")]
    [TypeIcon(typeof(Flow))]//Unit icon
    public class Requirement : Unit
    {
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput Exit;

        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput Enter;

        [DoNotSerialize]
        [PortLabel("ValueInput")]
        [AllowsNull]
        public ValueInput Input;

        [DoNotSerialize]
        [PortLabel("ControlInput")]
        [AllowsNull]
        public ValueInput _ControlInput;

        protected override void Definition()
        {
            Enter = ControlInput(nameof(Enter), Logic);
            Exit = ControlOutput(nameof(Exit));
            Input = ValueInput<ValueInput>(nameof(Input)).AllowsNull();
            _ControlInput = ValueInput<ControlInput>(nameof(_ControlInput)).AllowsNull();

            Requirement(Input, Enter);
            Requirement(_ControlInput, Enter);
            Succession(Enter, Exit);
        }

        public ControlOutput Logic(Flow flow)
        {
            Debug.LogWarning("This node is only for the code generators to understand what to do it does not work in a normal graph");
            return Exit;
        }
    }
}