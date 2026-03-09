using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    public abstract class BaseMoveTowardsGenerator<T> : NodeGenerator<MoveTowards<T>>
    {
        protected abstract Type MoveTowardsClass { get; }
        public BaseMoveTowardsGenerator(Unit unit) : base(unit) { }
        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.InvokeMember(MoveTowardsClass, "MoveTowards", writer.Action(() =>
            {
                using (data.Expect(typeof(T)))
                {
                    GenerateValue(Unit.current, data, writer);
                }
                writer.Write(", ");
                using (data.Expect(typeof(T)))
                {
                    GenerateValue(Unit.target, data, writer);
                }
                writer.Write(", ");
                using (data.Expect(typeof(float)))
                {
                    GenerateValue(Unit.maxDelta, data, writer);
                }
            }));
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