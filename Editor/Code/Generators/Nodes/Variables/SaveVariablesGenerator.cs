using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(SaveVariables))]
    public class SaveVariablesGenerator : NodeGenerator<SaveVariables>
    {
        public SaveVariablesGenerator(Unit unit) : base(unit) { }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            writer.InvokeMember(typeof(SavedVariables), "SaveDeclarations", 
            writer.Action(() =>
            {
                writer.GetMember(typeof(SavedVariables), "merged");
            })).NewLine();
            GenerateExitControl(Unit.exit, data, writer);
        }
    }
}
