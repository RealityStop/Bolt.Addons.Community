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
            Relation(arg, output);
        }

        private float GetValue(Recursion arg1)
        {
            float numeric = arguments[0].GetValue<float>();

            for (int i = 1; i < argumentCount; i++)
            {
                switch (OperationType)
                {
                    case MathType.Add:
                        numeric += arguments[i].GetValue<float>();
                        break;
                    case MathType.Subtract:
                        numeric -= arguments[i].GetValue<float>();
                        break;
                    case MathType.Multiply:
                        numeric *= arguments[i].GetValue<float>();
                        break;
                    case MathType.Divide:
                        numeric /= arguments[i].GetValue<float>();
                        break;
                    default:
                        break;
                }
            }

            return numeric;
        }
    }
}