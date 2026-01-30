using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(NegativeValueNode))]
    public class NegativeValueNodeGenerator : NodeGenerator<NegativeValueNode>
    {
        public NegativeValueNodeGenerator(Unit unit) : base(unit) { }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            var valueInput = Unit.type switch
            {
                NegateType.Float => Unit.Float,
                NegateType.Int => Unit.Int,
                NegateType.Vector2 => Unit.Vector2,
                NegateType.Vector3 => Unit.Vector3,
                _ => throw new NotSupportedException($"Unsupported type: {Unit.type}")
            };
            writer.Write("-");
            GenerateValue(valueInput, data, writer);
        }

        protected override void GenerateValueInternal(ValueInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input.hasValidConnection)
            {
                GenerateConnectedValue(input, data, writer);
            }
            else if (input.hasDefaultValue)
            {
                var expectedType = data.GetExpectedType();
                var val = unit.defaultValues[input.key];

                if (expectedType == typeof(int))
                    writer.Write($"{val}".NumericHighlight());
                else if (expectedType == typeof(float))
                    writer.Write($"{val}f".Replace(",", ".").NumericHighlight());
                else if (expectedType == typeof(double))
                    writer.Write($"{val}d".Replace(",", ".").NumericHighlight());
                else if (expectedType == typeof(long))
                    writer.Write($"{val}L".Replace(",", ".").NumericHighlight());
                else
                    writer.Write(val.As().Code(true, true, true, "", false));
            }
            else
            {
                writer.Write($"/* \"{input.key} Requires Input\" */".ErrorHighlight());
            }
        }
    }
}
