using Unity.VisualScripting;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;

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
                var isLiteral = Unit.selector.hasValidConnection && Unit.selector.connection.source.unit as Literal != null;
                var localName = string.Empty;
                if (isLiteral) localName = data.AddLocalName("@int");
                var newLiteral = isLiteral ? CodeBuilder.Indent(indent) + "var ".ConstructHighlight() + $"{localName} = " + ((Unit)Unit.selector.connection.source.unit).GenerateValue(Unit.selector.connection.source) + ";" : string.Empty;
                var @enum = Unit.selector.hasValidConnection ? (isLiteral ? localName : ((Unit)Unit.selector.connection.source.unit).GenerateValue(Unit.selector.connection.source)) : base.GenerateControl(input, data, indent);

                if (isLiteral) output += newLiteral + "\n";
                output += CodeBuilder.Indent(indent) + "switch".ConstructHighlight() + $" ({@enum})";
                output += "\n";
                output += CodeBuilder.Indent(indent) + "{";
                output += "\n";

                for (int i = 0; i < values.Count; i++)
                {
                    var _connection = ((ControlOutput)outputs[i])?.connection;

                    output += CodeBuilder.Indent(indent + 1) + "case ".ConstructHighlight() + $" {values[i].Key}".NumericHighlight() + ":";
                    output += "\n";

                    var _controlData = new ControlGenerationData();
                    _controlData.returns = data.returns;
                    _controlData.mustBreak = _controlData.returns == typeof(Void);
                    _controlData.mustReturn = !_controlData.mustBreak;
                    _controlData.localNames = data.localNames;

                    if (((ControlOutput)outputs[i]).hasValidConnection)
                    {
                        output += ((Unit)_connection.destination.unit).GenerateControl(_connection.destination as ControlInput, _controlData, indent + 2);
                        output += "\n";
                    }

                    if (_controlData.mustBreak && !_controlData.hasBroke) output += CodeBuilder.Indent(indent + 2) + "/* Case Must Break */".WarningHighlight() + "\n";
                    if (_controlData.mustReturn && !_controlData.hasReturned) output += CodeBuilder.Indent(indent + 2) + "/* Case Must Return or Break */".WarningHighlight() + "\n";
                }

                var connection = Unit.@default.connection;

                output += CodeBuilder.Indent(indent + 1) + "default ".ConstructHighlight() + ":";
                output += "\n";

                var controlData = new ControlGenerationData();
                controlData.returns = data.returns;
                controlData.mustBreak = controlData.returns == typeof(Void);
                controlData.mustReturn = !controlData.mustBreak;
                controlData.localNames = data.localNames;

                if (Unit.@default.hasValidConnection)
                {
                    output += ((Unit)connection.destination.unit).GenerateControl(connection.destination as ControlInput, controlData, indent + 2);
                    output += "\n";
                }

                if (controlData.mustBreak && !controlData.hasBroke) output += CodeBuilder.Indent(indent + 2) + "/* Case Must Break */".WarningHighlight() + "\n";
                if (controlData.mustReturn && !controlData.hasReturned) output += CodeBuilder.Indent(indent + 2) + "/* Case Must Return or Break */".WarningHighlight() + "\n";

                output += CodeBuilder.Indent(indent) + "}";
                output += "\n";

                return output;
            }

            return base.GenerateControl(input, data, indent);
        }

        public override string GenerateValue(ValueInput input)
        {
            if (input == Unit.selector)
            {
                if (Unit.selector.hasAnyConnection)
                {
                    return (Unit.selector.connection.source.unit as Unit).GenerateValue(Unit.selector.connection.source);
                }
            }

            return base.GenerateValue(input);
        }
    }
}