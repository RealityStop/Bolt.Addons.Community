using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(TryCatch))]
    public sealed class TryCatchGenerator : LocalVariableGenerator
    {
        private TryCatch Unit => unit as TryCatch;
        public TryCatchGenerator(Unit unit) : base(unit)
        {
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (!Unit.@try.hasValidConnection)
                return;

            writer.WriteIndented("try".ControlHighlight()).NewLine();
            writer.WriteLine("{");

            using (writer.Indented())
            {
                data.NewScope();
                GenerateChildControl(Unit.@try, data, writer);
                data.ExitScope();
            }

            writer.WriteLine("}");

            if (!Unit.@catch.hasValidConnection && !Unit.@finally.hasValidConnection)
            {
                using (writer.CodeDiagnosticScope("Catch or Finally requires connection", CodeDiagnosticKind.Warning))
                    writer.Error("Expected catch or finally block");
                return;
            }

            if (Unit.@catch.hasValidConnection)
            {
                data.NewScope();
                writer.WriteIndented("catch".ControlHighlight());
                if (Unit.exception.hasValidConnection)
                {
                    variableName = data.AddLocalNameInScope("ex", Unit.exceptionType);
                    writer.Parentheses(w =>
                    {
                        w.Write(Unit.exceptionType.As().CSharpName(false, true).TypeHighlight());
                        w.Space();
                        w.Write(variableName.VariableHighlight());
                    });
                }
                writer.NewLine();
                writer.WriteLine("{");

                using (writer.Indented())
                {
                    GenerateChildControl(Unit.@catch, data, writer);
                }

                writer.WriteLine("}");
                data.ExitScope();
            }

            if (Unit.@finally.hasValidConnection)
            {
                writer.WriteIndented("finally".ControlHighlight()).NewLine();
                writer.WriteLine("{");

                using (writer.Indented())
                {
                    data.NewScope();
                    GenerateChildControl(Unit.@finally, data, writer);
                    data.ExitScope();
                }

                writer.WriteLine("}");
            }
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (data.ContainsNameInAnyScope(variableName))
            {
                writer.GetVariable(variableName);
            }
            else
            {
                writer.Error($"{variableName} is only accessible from a catch scope");
            }
        }
    }
}