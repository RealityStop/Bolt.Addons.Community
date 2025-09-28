using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community 
{
    [NodeGenerator(typeof(While))]
    public sealed class WhileGenerator : NodeGenerator<While>
    {
        public WhileGenerator(Unit unit) : base(unit)
        {
        }
    
        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = "";
            output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("while".ControlHighlight() + " (") + GenerateValue(Unit.condition, data) + MakeClickableForThisUnit(")") + "\n";
            output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("{") + "\n";
            output += GetNextUnit(Unit.body, data, indent + 1);
            output += "\n" + CodeBuilder.Indent(indent) + MakeClickableForThisUnit("}") + "\n";
            output += GetNextUnit(Unit.exit, data, indent);
            return output;
        }
    } 
}