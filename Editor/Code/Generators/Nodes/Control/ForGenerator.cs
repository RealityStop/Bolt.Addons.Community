using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(For))]
    public sealed class ForGenerator : LocalVariableGenerator
    {
        private For Unit => unit as For;

        public ForGenerator(For unit) : base(unit)
        {
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input == Unit.enter)
            {
                using (writer.NewScope(data))
                {
                    variableName = data.AddLocalNameInScope("i", typeof(int), true);
                    variableType = typeof(int);

                    writer.WriteIndented("for".ControlHighlight());
                    writer.Parentheses(w =>
                    {
                        w.CreateVariable(typeof(int), variableName, writer.Action(() => GenerateValue(Unit.firstIndex, data, w)), WriteOptions.None, EndWriteOptions.Semicolon);
                        w.Space();
                        w.Write(variableName.VariableHighlight());
                        w.Write(" < ");
                        GenerateValue(Unit.lastIndex, data, w);
                        w.Write("; ");

                        if (!Unit.step.hasValidConnection && (int)Unit.defaultValues[Unit.step.key] == 1)
                        {
                            w.Write(variableName.VariableHighlight());
                            w.Write("++");
                        }
                        else
                        {
                            w.Write(variableName.VariableHighlight());
                            w.Write(" += ");
                            GenerateValue(Unit.step, data, w);
                        }
                    }).NewLine();

                    writer.WriteLine("{");

                    if (Unit.body.hasValidConnection)
                    {
                        using (writer.Indented())
                        {
                            GenerateChildControl(Unit.body, data, writer);
                        }
                    }

                    writer.WriteLine("}");
                }
            }

            GenerateExitControl(Unit.exit, data, writer);
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (!data.ContainsNameInAncestorScope(variableName))
            {
                writer.WriteErrorDiagnostic($"{variableName}, can only be used inside the loop.", $"Could not find or access {variableName}");
                return;
            }

            writer.GetVariable(variableName);
        }
    }
}