using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community 
{
    [NodeGenerator(typeof(For))]
    public sealed class ForGenerator : LocalVariableGenerator
    {
        private For Unit => unit as For;
        public ForGenerator(For unit) : base(unit)
        {
        }
    
        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;
    
            if (input == Unit.enter)
            {
                var initialization = GenerateValue(Unit.firstIndex, data);
                var condition = GenerateValue(Unit.lastIndex, data);
                var iterator = GenerateValue(Unit.step, data);
    
                variableName = data.AddLocalNameInScope("i", typeof(int));
                variableType = typeof(int);
    
                string varName = MakeSelectableForThisUnit(variableName.VariableHighlight());
                string iteratorCode = !Unit.step.hasValidConnection && (int)Unit.defaultValues[Unit.step.key] == 1 ? varName.VariableHighlight() + MakeSelectableForThisUnit("++") : varName.VariableHighlight() + MakeSelectableForThisUnit(" += ") + iterator;
                output += CodeBuilder.Indent(indent) + MakeSelectableForThisUnit($"for".ControlHighlight() + "(" + "int ".ConstructHighlight()) + $"{varName}".VariableHighlight() + MakeSelectableForThisUnit(" = ") + initialization + MakeSelectableForThisUnit("; ") + varName.VariableHighlight() + $"{MakeSelectableForThisUnit(" < ")}{condition}{MakeSelectableForThisUnit("; ")}" + $"{iteratorCode}{MakeSelectableForThisUnit(")")}";
                output += "\n";
                output += CodeBuilder.Indent(indent) + MakeSelectableForThisUnit("{");
                output += "\n";
    
                if (Unit.body.hasAnyConnection)
                {
                    data.NewScope();
                    output += GetNextUnit(Unit.body, data, indent + 1);
                    data.ExitScope();
                }
    
                output += "\n";
                output += CodeBuilder.Indent(indent) + MakeSelectableForThisUnit("}");
                output += "\n";
            }
    
            if (Unit.exit.hasAnyConnection)
            {
                output += GetNextUnit(Unit.exit, data, indent);
                output += "\n";
            }
    
    
            return output;
        }
    
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return MakeSelectableForThisUnit(variableName.VariableHighlight());
        }
    
        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input.hasValidConnection)
            {
                data.SetExpectedType(input.type);
                var connectedCode = GetNextValueUnit(input, data);
                data.RemoveExpectedType();
                return new ValueCode(connectedCode, input.type, ShouldCast(input, data, false));
            }
            else
            {
                return Unit.defaultValues[input.key].As().Code(false, unit);
            }
        }
    } 
}