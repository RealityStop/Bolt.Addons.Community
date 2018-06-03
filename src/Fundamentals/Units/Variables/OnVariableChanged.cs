using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Bolt.Addons.Community.Fundamentals
{
   /* [UnitCategory("Events")]
    public sealed class OnVariableChanged : UnifiedVariableUnit, 
    {
        public OnVariableChanged() : base() { }

        private object _previousValue;


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
        public ControlOutput onChange { get; private set; }

        protected override void Definition()
        {
            base.Definition();

            assign = ControlInput(nameof(assign), CheckVariable);
            onChange = ControlOutput(nameof(onChange));



            Relation(name, assign);
            Relation(assign, onChange);

            if (kind == VariableKind.Object)
            {
                Relation(@object, assign);
            }
        }

        private void CheckVariable(Flow flow)
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

            var currentValue = variables.Get<object>(name);

            if (_previousValue != currentValue)
            {
                _previousValue = currentValue;
                flow.Invoke(onChange);
            }
        }
    }*/
}