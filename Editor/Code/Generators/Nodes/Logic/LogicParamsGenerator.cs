namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(LogicParams))]
    public sealed class LogicParamsGenerator : NodeGenerator<LogicParams>
    {
        public LogicParamsGenerator(LogicParams unit) : base(unit) { }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (Unit.arguments.Count == 0)
                return;

            switch (Unit.BranchingType)
            {
                case LogicParamNode.BranchType.And:
                    WriteJoined(writer, data, " && ");
                    break;

                case LogicParamNode.BranchType.Or:
                    WriteJoined(writer, data, " || ");
                    break;

                case LogicParamNode.BranchType.GreaterThan:
                    WriteComparison(writer, data, Unit.AllowEquals ? ">=" : ">");
                    break;

                case LogicParamNode.BranchType.LessThan:
                    WriteComparison(writer, data, Unit.AllowEquals ? "<=" : "<");
                    break;

                case LogicParamNode.BranchType.Equal:
                    WriteEqual(writer, data);
                    break;
            }
        }

        private void WriteJoined(CodeWriter writer, ControlGenerationData data, string op)
        {
            for (int i = 0; i < Unit.arguments.Count; i++)
            {
                if (i != 0)
                    writer.Write(op);

                GenerateValue(Unit.arguments[i], data, writer);
            }
        }

        private void WriteComparison(CodeWriter writer, ControlGenerationData data, string op)
        {
            if (Unit.arguments.Count < 2)
                return;

            GenerateValue(Unit.arguments[0], data, writer);
            writer.Write(" " + op + " ");
            GenerateValue(Unit.arguments[1], data, writer);
        }

        private void WriteEqual(CodeWriter writer, ControlGenerationData data)
        {
            writer.CallCSharpUtilityMethod("Equal", writer.Action(() =>
            {
                for (int i = 1; i < Unit.arguments.Count; i++)
                {
                    if (i != 1)
                        writer.Write(", ");

                    GenerateValue(Unit.arguments[i], data, writer);
                }
            }));
        }
    }
}