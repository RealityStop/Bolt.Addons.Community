using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community 
{
    [NodeGenerator(typeof(WaitWhileUnit))]
    public sealed class WaitWhileGenerator : NodeGenerator<WaitWhileUnit>
    {
        public WaitWhileGenerator(Unit unit) : base(unit)
        {
        }
    
        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;
            data.SetExpectedType(typeof(bool));
            var condition = GenerateValue(Unit.condition, data);
            data.RemoveExpectedType();
            output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("yield return ".ControlHighlight() + "new ".ConstructHighlight() + "WaitWhile".TypeHighlight() + "(() => ") + condition + MakeClickableForThisUnit(");") + "\n";
            output += GetNextUnit(Unit.exit, data, indent);
            return output;
        }
    } 
}