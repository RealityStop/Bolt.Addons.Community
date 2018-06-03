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
        public VariableAdder() : base() { }

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

        protected abstract float GetAmount();

        protected override void Definition()
        {
            base.Definition();

            assign = ControlInput(nameof(assign), UpdateVariable);
            assigned = ControlOutput(nameof(assigned));



            Relation(name, assign);
            Relation(assign, assigned);

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
            _postIncrementValue = _preIncrementValue + GetAmount();
            variables.Set(name, _postIncrementValue);

            flow.Invoke(assigned);
        }
    }
}