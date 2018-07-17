using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Bolt.Addons.Community.Fundamentals
{
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
                    variables = Bolt.Variables.Graph(flow.stack);
                    break;
                case VariableKind.Object:
                    variables = Bolt.Variables.Object(@flow.GetValue<GameObject>(@object));
                    break;
                case VariableKind.Scene:
                    variables = Bolt.Variables.Scene(flow.stack.scene);
                    break;
                case VariableKind.Application:
                    variables = Bolt.Variables.Application;
                    break;
                case VariableKind.Saved:
                    variables = Bolt.Variables.Saved;
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