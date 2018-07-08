using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Bolt.Addons.Community.Fundamentals
{
    [UnitShortTitle("Math")]
    [UnitTitle("Math Op")]
    [UnitCategory("Community\\Math")]
    [TypeIcon(typeof(Absolute<float>))]
    public class MathParamNode : VariadicNode<float>
    {
        public enum MathType { Add, Subtract, Multiply, Divide}


        [SerializeAs(nameof(OperationType))]
        private MathType _operationType;

        [DoNotSerialize]
        [Inspectable, UnitHeaderInspectable("Operation")]
        public MathType OperationType { get { return _operationType; } set { _operationType = value; } }


        [PortLabel("Result")]
        [DoNotSerialize]
        public ValueOutput output { get; private set; }


        protected override void Definition()
        {
            output = ValueOutput<float>(nameof(output), GetValue);

            base.Definition();
        }

        protected override void BuildRelations(ValueInput arg)
        {
            Requirement(arg, output);
        }

        private float GetValue(Flow flow)
        {
            float numeric = flow.GetValue<float>(arguments[0]);

            for (int i = 1; i < argumentCount; i++)
            {
                switch (OperationType)
                {
                    case MathType.Add:
                        numeric += flow.GetValue<float>(arguments[i]);
                        break;
                    case MathType.Subtract:
                        numeric -= flow.GetValue<float>(arguments[i]);
                        break;
                    case MathType.Multiply:
                        numeric *= flow.GetValue<float>(arguments[i]);
                        break;
                    case MathType.Divide:
                        numeric /= flow.GetValue<float>(arguments[i]);
                        break;
                    default:
                        break;
                }
            }

            return numeric;
        }
    }
}