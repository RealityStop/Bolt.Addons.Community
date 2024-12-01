using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(BindFuncNode))]
    public class BindFuncNodeGenerator : NodeGenerator<BindFuncNode>
    {
        public BindFuncNodeGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return base.GenerateValue(output, data);
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;
            var variable = string.Empty;
            bool sourceIsDelgateNode = Unit.a.hasValidConnection && Unit.a.connection.source.unit is DelegateNode;
            if (sourceIsDelgateNode)
            {
                variable = data.AddLocalNameInScope("@delgate", Unit.a.connection.source.type).VariableHighlight();
                output += CodeBuilder.Indent(indent) + MakeSelectableForThisUnit((Unit.a.connection.source.unit as DelegateNode)._delegate.GetDelegateType().As().CSharpName(false, true) + " " + variable + " = ") + GenerateValue(Unit.a, data) + MakeSelectableForThisUnit(";") + "\n";
            }

            output += CodeBuilder.Indent(indent) + (sourceIsDelgateNode ? MakeSelectableForThisUnit(variable + " += ") + GenerateValue(Unit.b, data) + MakeSelectableForThisUnit(";") + "\n" : GenerateValue(Unit.a, data) + MakeSelectableForThisUnit(" += ") + GenerateValue(Unit.b, data) + MakeSelectableForThisUnit(";") + "\n");
            output += GetNextUnit(Unit.exit, data, indent);
            return output;
        }
    }
}
