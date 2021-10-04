using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.VariableAdder")]
    public abstract class VariableAdder : UnifiedVariableUnit
    {
        protected float _preIncrementValue;
        protected float _postIncrementValue;

        /// <summary>
        /// The entry point to update the variable.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput assign { get; private set; }

        /// <summary>
        /// The action to execute once the variable has been updated.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput assigned { get; private set; }



        /// <summary>
        /// The value to return if the variable is not defined.
        /// </summary>
        [DoNotSerialize]
        public ValueInput fallback { get; private set; }

        /// <summary>
        /// Whether a fallback value should be provided if the 
        /// variable is not defined.
        /// </summary>
        [Serialize]
        [Inspectable]
        [InspectorLabel("Fallback")]
        public bool specifyFallback { get; set; } = false;

        protected abstract float GetAmount(Flow flow);

        protected override void Definition()
        {
            base.Definition();

            assign = ControlInput(nameof(assign), UpdateVariable);
            assigned = ControlOutput(nameof(assigned));



            Requirement(name, assign);
            Succession(assign, assigned);

            if (kind == VariableKind.Object)
            {
                Requirement(@object, assign);
            }

            if (specifyFallback)
            {
                fallback = ValueInput<float>(nameof(fallback), 0);
                Requirement(fallback, assign);
            }
        }

        private ControlOutput UpdateVariable(Flow flow)
        {
            var name = flow.GetValue<string>(this.name);
            VariableDeclarations variables;

            switch (kind)
            {
                case VariableKind.Flow:
                    variables = flow.variables;
                    break;
                case VariableKind.Graph:
                    variables = Unity.VisualScripting.Variables.Graph(flow.stack);
                    break;
                case VariableKind.Object:
                    variables = Unity.VisualScripting.Variables.Object(@flow.GetValue<GameObject>(@object));
                    break;
                case VariableKind.Scene:
                    variables = Unity.VisualScripting.Variables.Scene(flow.stack.scene);
                    break;
                case VariableKind.Application:
                    variables = Unity.VisualScripting.Variables.Application;
                    break;
                case VariableKind.Saved:
                    variables = Unity.VisualScripting.Variables.Saved;
                    break;
                default:
                    throw new UnexpectedEnumValueException<VariableKind>(kind);
            }

            if (!variables.IsDefined(name) && specifyFallback)
                _preIncrementValue = flow.GetValue<float>(fallback);
            else
                _preIncrementValue = variables.Get<float>(name);


            _postIncrementValue = _preIncrementValue + GetAmount(flow);
            variables.Set(name, _postIncrementValue);

            return assigned;
        }
    }
}