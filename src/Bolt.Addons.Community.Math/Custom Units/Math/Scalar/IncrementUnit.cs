using Bolt.Addons.Community.Math.Custom_Units;
using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Bolt.Addons.Community.Math.Scalar
{
    [UnitCategory("Math/Scalar")]
    [UnitShortTitle("Increment Variable")]
    [UnitTitle("Increment")]
    public sealed class IncrementUnit : VarUnit
    {
        public IncrementUnit() : base() { }

        private float _preIncrementValue;
        private float _postIncrementValue;

        /// <summary>
        /// The entry point to update the variable.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlInput assign { get; set; }

        /// <summary>
        /// The action to execute once the variable has been updated.
        /// </summary>
        [DoNotSerialize]
        [PortLabelHidden]
        public ControlOutput assigned { get; set; }


        /// <summary>
        /// The value assigned to the variable before incrementing.
        /// </summary>
        [DoNotSerialize]
        [PortLabel("pre")]
        [PortLabelHidden]
        public ValueOutput preIncrement { get; private set; }

        /// <summary>
        /// The value assigned to the variable after incrementing.
        /// </summary>
        [DoNotSerialize]
        [PortLabel("post")]
        [PortLabelHidden]
        public ValueOutput postIncrement { get; private set; }

        protected override void Definition()
        {
            base.Definition();

            assign = ControlInput(nameof(assign), UpdateVariable);

            preIncrement = ValueOutput<float>(nameof(preIncrement), (x) => _preIncrementValue);
            postIncrement = ValueOutput<float>(nameof(postIncrement), (x) => _postIncrementValue);
            assigned = ControlOutput(nameof(assigned));



            Relation(name, assign);
            Relation(name, preIncrement);
            Relation(name, postIncrement);
            Relation(assign, assigned);
            Relation(assign, preIncrement);
            Relation(assign, postIncrement);

            if (kind == VariableKind.Object)
            {
                Relation(@object, assign);
            }
        }

        private void UpdateVariable(Flow flow)
        {
            var name = this.name.GetValue<string>();
            VariableDeclarations variables;

            switch (kind)
            {
                case VariableKind.Graph:
                    variables = Variables.Graph(graph);
                    break;
                case VariableKind.Object:
                    variables = Variables.Object(@object.GetValue<GameObject>());
                    break;
                case VariableKind.Scene:
                    variables = Variables.Scene(owner.GameObject().scene);
                    break;
                case VariableKind.Application:
                    variables = Variables.Application;
                    break;
                case VariableKind.Saved:
                    variables = Variables.Saved;
                    break;
                default:
                    throw new UnexpectedEnumValueException<VariableKind>(kind);
            }

            _preIncrementValue = variables.Get<float>(name);
            _postIncrementValue = _preIncrementValue + 1;
            variables.Set(name, _postIncrementValue);

            flow.Invoke(assigned);
        }
    }
}