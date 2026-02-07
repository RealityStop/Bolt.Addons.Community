using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    public abstract class PerSecondGenerator<T> : NodeGenerator<PerSecond<T>>
    {
        public PerSecondGenerator(Unit unit) : base(unit) { }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            using (data.Expect(typeof(T)))
            {
                GenerateValue(Unit.input, data, writer);
            }
            writer.Write(" * ");
            writer.Write("Time".TypeHighlight());
            writer.Write(".");
            writer.Write("deltaTime".VariableHighlight());
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
                    writer.Object(val, true, true, true, true, "", false);
            }
            else
            {
                writer.Write($"/* \"{input.key} Requires Input\" */".ErrorHighlight());
            }
        }
    }
}
