using Bolt.Addons.Community.Math.Custom_Units;
using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Bolt.Addons.Community.Variables.Units
{
    [UnitCategory("Variables")]
    [UnitShortTitle("Decrement Variable")]
    [UnitTitle("Decrement")]
    public sealed class DecrementUnit : UnifiedVariableUnit
    {
        public DecrementUnit() : base() { }

        private float _preDecrementValue;
        private float _postDecrementValue;

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
        /// The value assigned to the variable before Decrementing.
        /// </summary>
        [DoNotSerialize]
        [PortLabel("pre")]
        public ValueOutput preDecrement { get; private set; }

        /// <summary>
        /// The value assigned to the variable after Decrementing.
        /// </summary>
        [DoNotSerialize]
        [PortLabel("post")]
        public ValueOutput postDecrement { get; private set; }

        protected override void Definition()
        {
            base.Definition();

            assign = ControlInput(nameof(assign), UpdateVariable);

            preDecrement = ValueOutput<float>(nameof(preDecrement), (x) => _preDecrementValue);
            postDecrement = ValueOutput<float>(nameof(postDecrement), (x) => _postDecrementValue);
            assigned = ControlOutput(nameof(assigned));



            Relation(name, assign);
            Relation(name, preDecrement);
            Relation(name, postDecrement);
            Relation(assign, assigned);
            Relation(assign, preDecrement);
            Relation(assign, postDecrement);

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

            _preDecrementValue = variables.Get<float>(name);
            _postDecrementValue = _preDecrementValue - 1;
            variables.Set(name, _postDecrementValue);

            flow.Invoke(assigned);
        }
    }
}