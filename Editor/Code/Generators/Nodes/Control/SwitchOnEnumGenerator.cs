using Unity.VisualScripting;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(SwitchOnEnum))]
    public sealed class SwitchOnEnumGenerator : NodeGenerator<SwitchOnEnum>
    {
        public SwitchOnEnumGenerator(SwitchOnEnum unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;

            if (input == Unit.enter)
            {
                var values = Unit.enumType.GetEnumValues();
                var outputs = Unit.outputs.ToArray();
                var isLiteral = Unit.@enum.hasValidConnection && Unit.@enum.connection.source.unit is Literal;
                var localName = string.Empty;
                if (isLiteral) localName = data.AddLocalNameInScope("@enum", (Unit.@enum.connection.source.unit as Literal).type);
                var newLiteral = isLiteral ? CodeBuilder.Indent(indent) + MakeClickableForThisUnit("var ".ConstructHighlight() + localName + " = ") + ((Unit)Unit.@enum.connection.source.unit).GenerateValue(Unit.@enum.connection.source, data) + MakeClickableForThisUnit(";") : string.Empty;
                var @enum = Unit.@enum.hasValidConnection ? isLiteral ? MakeClickableForThisUnit(localName) : ((Unit)Unit.@enum.connection.source.unit).GenerateValue(Unit.@enum.connection.source, data) : base.GenerateControl(input, data, indent);

                if (isLiteral) output += MakeClickableForThisUnit(newLiteral) + "\n";
                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("switch".ConstructHighlight() + " (") + @enum + MakeClickableForThisUnit(")");
                output += "\n";
                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("{");
                output += "\n";

                for (int i = 0; i < values.Length; i++)
                {
                    output += CodeBuilder.Indent(indent + 1) + MakeClickableForThisUnit("case ".ConstructHighlight() + Unit.enumType.Name.EnumHighlight() + "." + values.GetValue(i).ToString() + ":\n");

                    if (outputs[i].hasValidConnection)
                    {
                        data.NewScope();
                        data.SetMustBreak(true);
                        output += GetNextUnit((ControlOutput)outputs[i], data, indent + 2);
                        output += "\n";
                        data.ExitScope();
                    }

                    if ((data.MustBreak && !data.HasBroke) || (data.MustReturn && !data.HasReturned)) output += CodeBuilder.Indent(indent + 2) + MakeClickableForThisUnit("break".ControlHighlight() + ";") + "\n";
                }

                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("}");
                output += "\n";

                return output;
            }

            return base.GenerateControl(input, data, indent);
        }


        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input == Unit.@enum)
            {
                if (Unit.@enum.hasAnyConnection)
                {
                    return (Unit.@enum.connection.source.unit as Unit).GenerateValue(Unit.@enum.connection.source, data);
                }
            }

            return base.GenerateValue(input, data);
        }
    }
}