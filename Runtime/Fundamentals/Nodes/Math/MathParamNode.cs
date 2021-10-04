using System.Collections.Generic;

namespace Unity.VisualScripting.Community
{
    [UnitShortTitle("Math")]
    [UnitTitle("Math Op")]
    [UnitCategory("Community\\Math")]
    [TypeIcon(typeof(Absolute<float>))]
    [RenamedFrom("Bolt.Addons.Community.Fundamentals.MathParamNode")]
    public class MathParamNode : VariadicNode<float>
    {
        public enum MathType { Add, Subtract, Multiply, Divide }


        [SerializeAs(nameof(OperationType))]
        private MathType _operationType;

        [Serialize]
        [Inspectable]
        [InspectorLabel("Non-Numeric Inputs")]
        private bool nonNumeric = default(bool);

        [DoNotSerialize]
        [Inspectable, UnitHeaderInspectable("Operation")]
        public MathType OperationType { get { return _operationType; } set { _operationType = value; } }


        [PortLabel("Result")]
        [DoNotSerialize]
        public ValueOutput output { get; private set; }


        protected override void Definition()
        {
            arguments = new List<ValueInput>();
            if (nonNumeric)
            {
                output = ValueOutput<object>(nameof(output), GetObjectValue);
                for (var i = 0; i < argumentCount; i++)
                {
                    var argument = ValueInput<object>("Arg_" + i);
                    arguments.Add(argument);
                    BuildRelations(argument);
                }
            }
            else
            {
                output = ValueOutput<float>(nameof(output), GetFloatValue);
                for (var i = 0; i < argumentCount; i++)
                {
                    var argument = ValueInput<float>("Arg_" + i);
                    argument.SetDefaultValue(GetDefaultValue(typeof(float)));
                    arguments.Add(argument);
                    BuildRelations(argument);
                }
            }
        }

        protected override void BuildRelations(ValueInput arg)
        {
            Requirement(arg, output);
        }

        private float GetFloatValue(Flow flow)
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


        private object GetObjectValue(Flow flow)
        {

            object obj = flow.GetValue<object>(arguments[0]);

            for (int i = 1; i < argumentCount; i++)
            {
                switch (OperationType)
                {
                    case MathType.Add:
                        obj = OperatorUtility.Add(obj, flow.GetValue<object>(arguments[i]));
                        break;
                    case MathType.Subtract:
                        obj = OperatorUtility.Subtract(obj, flow.GetValue<object>(arguments[i]));
                        break;
                    case MathType.Multiply:
                        obj = OperatorUtility.Multiply(obj, flow.GetValue<object>(arguments[i]));
                        break;
                    case MathType.Divide:
                        obj = OperatorUtility.Divide(obj, flow.GetValue<object>(arguments[i]));
                        break;
                    default:
                        break;
                }
            }

            return obj;
        }
    }
}