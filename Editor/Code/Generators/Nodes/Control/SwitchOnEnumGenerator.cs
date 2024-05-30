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
                var isLiteral = Unit.@enum.hasValidConnection && Unit.@enum.connection.source.unit is Literal literal;
                var localName = string.Empty;
                if (isLiteral) localName = data.AddLocalNameInScope("@enum");
                var newLiteral = isLiteral ? CodeUtility.MakeSelectable(((Unit)Unit.@enum.connection.source.unit), CodeBuilder.Indent(indent) + "var ".ConstructHighlight() + localName + " = " + ((Unit)Unit.@enum.connection.source.unit).GenerateValue(Unit.@enum.connection.source)) : string.Empty;
                var @enum = Unit.@enum.hasValidConnection ? CodeUtility.MakeSelectable(((Unit)Unit.@enum.connection.source.unit), isLiteral ? localName : ((Unit)Unit.@enum.connection.source.unit).GenerateValue(Unit.@enum.connection.source)) : base.GenerateControl(input, data, indent);

                if (isLiteral) output += CodeUtility.MakeSelectable(Unit, newLiteral) + "\n";
                output += CodeUtility.MakeSelectable(Unit, CodeBuilder.Indent(indent) + "switch".ConstructHighlight() + " (" + @enum + ")");
                output += "\n";
                output += CodeUtility.MakeSelectable(Unit, CodeBuilder.Indent(indent) + "{");
                output += "\n";

                for (int i = 0; i < values.Length; i++)
                {
                    output += CodeUtility.MakeSelectable(Unit, CodeBuilder.Indent(indent + 1) + "case ".ConstructHighlight() + Unit.enumType.Name.EnumHighlight() + "." + values.GetValue(i).ToString() + ":\n");
                    var controlData = new ControlGenerationData(data);
                    controlData.mustBreak = controlData.returns == typeof(Void);
                    controlData.mustReturn = !controlData.mustBreak;

                    var connection = ((ControlOutput)outputs[i])?.connection;
                    if (connection != null && connection.destination != null)
                    {
                        controlData.NewScope();
                        output += ((Unit)connection.destination.unit).GenerateControl(connection.destination as ControlInput, controlData, indent + 2);
                        output += "\n";
                        controlData.ExitScope();
                    }

                    if (controlData.mustBreak && !controlData.hasBroke) output += CodeUtility.MakeSelectable(Unit, CodeBuilder.Indent(indent + 2) + "break;".ControlHighlight() + "\n");
                    if (controlData.mustReturn && !controlData.hasReturned) output += CodeUtility.MakeSelectable(Unit, CodeBuilder.Indent(indent + 2) + "break".ControlHighlight() + ";\n");
                }

                output += CodeUtility.MakeSelectable(Unit, CodeBuilder.Indent(indent) + "}");
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