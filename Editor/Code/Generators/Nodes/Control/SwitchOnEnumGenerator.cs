using Unity.VisualScripting;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;

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
                var isLiteral = Unit.@enum.hasValidConnection && Unit.@enum.connection.source.unit as Literal != null;
                var localName = string.Empty;
                if (isLiteral) localName = data.AddLocalName("@enum");
                var newLiteral = isLiteral ? CodeBuilder.Indent(indent) + "var ".ConstructHighlight() + localName + " = " + ((Unit)Unit.@enum.connection.source.unit).GenerateValue(Unit.@enum.connection.source) : string.Empty;
                var @enum = Unit.@enum.hasValidConnection ? (isLiteral ? localName : ((Unit)Unit.@enum.connection.source.unit).GenerateValue(Unit.@enum.connection.source)) : base.GenerateControl(input, data, indent);

                if (isLiteral) output += newLiteral + "\n";
                output += CodeBuilder.Indent(indent) + "switch".ConstructHighlight() + " (" + @enum + ")";
                output += "\n";
                output += CodeBuilder.Indent(indent) + "{";
                output += "\n";

                for (int i = 0; i < values.Length; i++)
                {
                    var connection = ((ControlOutput)outputs[i])?.connection;

                    output += CodeBuilder.Indent(indent + 1) + "case ".ConstructHighlight() + Unit.enumType.Name.EnumHighlight() + "." + values.GetValue(i).ToString() + ":";
                    output += "\n";

                    var controlData = new ControlGenerationData();
                    controlData.returns = data.returns;
                    controlData.mustBreak = controlData.returns == typeof(Void);
                    controlData.mustReturn = !controlData.mustBreak;
                    controlData.localNames = data.localNames;

                    if (((ControlOutput)outputs[i]).hasValidConnection)
                    {
                        output += ((Unit)connection.destination.unit).GenerateControl(connection.destination as ControlInput, controlData, indent + 2);
                        output += "\n";
                    }

                    if (controlData.mustBreak && !controlData.hasBroke) output += CodeBuilder.Indent(indent + 2) + "/* Case Must Break */".WarningHighlight() + "\n";
                    if (controlData.mustReturn && !controlData.hasReturned) output += CodeBuilder.Indent(indent + 2) + "/* Case Must Return or Break */".WarningHighlight() + "\n";
                }

                output += CodeBuilder.Indent(indent) + "}";
                output += "\n";

                return output;
            }

            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input == Unit.@enum)
            {
                if (Unit.@enum.hasAnyConnection)
                {
                    return (Unit.@enum.connection.source.unit as Unit).GenerateValue(Unit.@enum.connection.source);
                }
            }

            return base.GenerateValue(input);
        }
    }
}