using Unity.VisualScripting;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(SwitchOnEnum))]
    public sealed class SwitchOnEnumGenerator : NodeGenerator<SwitchOnEnum>
    {
        public SwitchOnEnumGenerator(SwitchOnEnum unit) : base(unit)
        {
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input == Unit.enter)
            {
                var values = Unit.enumType.GetEnumValues();
                var outputs = Unit.outputs.ToArray();
                var isLiteral = Unit.@enum.hasValidConnection && Unit.@enum.connection.source.unit is Literal;
                var localName = string.Empty;

                if (isLiteral)
                {
                    localName = data.AddLocalNameInScope("@enum", (Unit.@enum.connection.source.unit as Literal).type);
                    writer.WriteIndented();
                    writer.Write("var".ConstructHighlight());
                    writer.Space();
                    writer.Write(localName.VariableHighlight());
                    writer.Write(" = ");
                    GenerateValue(Unit.@enum, data, writer);
                    writer.Write(";");
                    writer.NewLine();
                }

                writer.WriteIndented();
                writer.Write("switch".ControlHighlight());
                writer.Write(" (");
                if (isLiteral)
                    writer.Write(localName.VariableHighlight());
                else
                    GenerateValue(Unit.@enum, data, writer);
                writer.Write(")");
                writer.NewLine();
                writer.WriteLine("{");

                using (writer.Indented())
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        writer.WriteIndented();
                        writer.Write("case".ControlHighlight());
                        writer.Space();
                        writer.Write(Unit.enumType.Name.TypeHighlight());
                        writer.Write(".");
                        writer.Write(values.GetValue(i).ToString());
                        writer.Write(":");
                        writer.NewLine();

                        data.SetMustBreak(true);
                        data.SetHasBroke(false);
                        if (outputs[i].hasValidConnection)
                        {
                            data.NewScope();
                            GenerateChildControl((ControlOutput)outputs[i], data, writer);
                            data.ExitScope();
                        }

                        if ((data.MustBreak && !data.HasBroke) || (data.MustReturn && !data.HasReturned))
                        {
                            writer.WriteIndented();
                            writer.Write("break".ControlHighlight());
                            writer.Write(";");
                            writer.NewLine();
                        }
                    }
                }

                writer.WriteLine("}");
            }
        }
    }
}