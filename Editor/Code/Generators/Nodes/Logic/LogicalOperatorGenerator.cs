using System;

namespace Unity.VisualScripting.Community.CSharp
{
    public abstract class LogicalOperatorGenerator<TUnit> : NodeGenerator<TUnit> where TUnit : Unit
    {
        protected LogicalOperatorGenerator(TUnit unit) : base(unit) { }

        protected abstract ValueInput LeftInput { get; }
        protected abstract ValueInput RightInput { get; }
        protected abstract ValueOutput OutputPort { get; }
        protected abstract string OperatorToken { get; }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (output == OutputPort)
            {
                writer.Write("(");
                using (data.Expect(typeof(bool)))
                {
                    GenerateValue(LeftInput, data, writer);
                }
                writer.Write(OperatorToken);
                using (data.Expect(typeof(bool)))
                {
                    GenerateValue(RightInput, data, writer);
                }
                writer.Write(")");
            }
        }

        protected override void GenerateValueInternal(ValueInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input == LeftInput || input == RightInput)
            {
                if (input.hasValidConnection)
                {
                    Type actualSourceType = GetSourceType(input, data, writer, false);
                    if (actualSourceType != null && actualSourceType != typeof(bool))
                    {
                        writer.Write(writer.GetTypeNameHighlighted(typeof(bool)));
                    }
                    GenerateConnectedValue(input, data, writer);
                    return;
                }

                if (input.hasDefaultValue)
                {
                    WriteDefaultValue(input, data, writer);
                    return;
                }
                writer.Write("false");
            }
        }
    }
}