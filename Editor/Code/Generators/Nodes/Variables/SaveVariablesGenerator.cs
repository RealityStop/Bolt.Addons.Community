using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(SaveVariables))]
    public class SaveVariablesGenerator : NodeGenerator<SaveVariables>
    {
        public SaveVariablesGenerator(Unit unit) : base(unit) { }
        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return MakeClickableForThisUnit($"{"SavedVariables".TypeHighlight()}.SaveDeclarations({"SavedVariables".TypeHighlight()}.{"merged".VariableHighlight()});") + "\n" + GetNextUnit(Unit.exit, data, indent);
        }
    }
}
