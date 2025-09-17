using Unity.VisualScripting;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(SwitchOnInteger))]
    public sealed class SwitchOnIntegerGenerator : NodeGenerator<SwitchOnInteger>
    {
        public SwitchOnIntegerGenerator(SwitchOnInteger unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;

            if (input == Unit.enter)
            {
                var values = Unit.branches;
                var outputs = Unit.outputs.ToArray();
                var isLiteral = Unit.selector.hasValidConnection && Unit.selector.connection.source.unit is Literal;
                var localName = string.Empty;
                if (isLiteral) localName = data.AddLocalNameInScope("@int", typeof(int)).VariableHighlight();
                var newLiteral = isLiteral ? CodeBuilder.Indent(indent) + MakeClickableForThisUnit("var ".ConstructHighlight() + $"{localName} = ") + ((Unit)Unit.selector.connection.source.unit).GenerateValue(Unit.selector.connection.source, data) + MakeClickableForThisUnit(";") : string.Empty;
                var value = Unit.selector.hasValidConnection ? (isLiteral ? MakeClickableForThisUnit(localName) : ((Unit)Unit.selector.connection.source.unit).GenerateValue(Unit.selector.connection.source, data)) : base.GenerateControl(input, data, indent);

                if (isLiteral) output += newLiteral + "\n";
                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("switch".ConstructHighlight() + $" (") + value + MakeClickableForThisUnit($")");
                output += "\n";
                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("{");
                output += "\n";

                for (int i = 0; i < values.Count; i++)
                {
                    output += CodeBuilder.Indent(indent + 1) + MakeClickableForThisUnit("case ".ConstructHighlight() + $" {values[i].Key}".NumericHighlight() + ":");
                    output += "\n";

                    if (values[i].Value.hasValidConnection)
                    {
                        data.NewScope();
                        data.SetMustBreak(true);
                        output += GetNextUnit(values[i].Value, data, indent + 2);
                        output += "\n";
                        data.ExitScope();
                    }

                    if ((data.MustBreak && !data.HasBroke) || (data.MustReturn && !data.HasReturned)) output += CodeBuilder.Indent(indent + 2) + MakeClickableForThisUnit("break".ControlHighlight() + ";") + "\n";
                }
                output += CodeBuilder.Indent(indent + 1) + MakeClickableForThisUnit("default".ConstructHighlight() + ":");
                output += "\n";

                if (Unit.@default.hasValidConnection)
                {
                    data.NewScope();
                    data.SetMustBreak(true);
                    output += GetNextUnit(Unit.@default, data, indent + 2);
                    output += "\n";
                    data.ExitScope();
                }

                if ((data.MustBreak && !data.HasBroke) || (data.MustReturn && !data.HasReturned)) output += CodeBuilder.Indent(indent + 2) + MakeClickableForThisUnit("break".ControlHighlight() + ";") + "\n";
                output += "\n";

                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("}");
                output += "\n";

                return output;
            }

            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input == Unit.selector)
            {
                if (Unit.selector.hasAnyConnection)
                {
                    return (Unit.selector.connection.source.unit as Unit).GenerateValue(Unit.selector.connection.source, data);
                }
            }

            return base.GenerateValue(input, data);
        }
    }
}
