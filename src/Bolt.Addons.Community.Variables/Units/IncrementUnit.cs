using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Bolt.Addons.Community.Variables.Units
{
    [UnitCategory("Variables")]
    [UnitShortTitle("Increment Variable")]
    [UnitTitle("Increment")]
    public sealed class IncrementUnit : UnifiedVariableUnit
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
        public ValueOutput preIncrement { get; private set; }

        /// <summary>
        /// The value assigned to the variable after incrementing.
        /// </summary>
        [DoNotSerialize]
        [PortLabel("post")]
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
                    variables = Bolt.Variables.Graph(graph);
                    break;
                case VariableKind.Object:
                    variables = Bolt.Variables.Object(@object.GetValue<GameObject>());
                    break;
                case VariableKind.Scene:
                    variables = Bolt.Variables.Scene(owner.GameObject().scene);
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

            _preIncrementValue = variables.Get<float>(name);
            _postIncrementValue = _preIncrementValue + 1;
            variables.Set(name, _postIncrementValue);

            flow.Invoke(assigned);
        }
    }
}