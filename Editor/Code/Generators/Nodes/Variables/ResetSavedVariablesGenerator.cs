namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(ResetSavedVariables))]
    public class ResetSavedVariablesGenerator : NodeGenerator<ResetSavedVariables>
    {
        public ResetSavedVariablesGenerator(Unit unit) : base(unit) { }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            foreach (var arg in Unit.arguments)
            {
                writer.WriteIndented();
                writer.CallCSharpUtilityMethod("ResetSavedVariable", writer.Action(() => GenerateValue(arg, data, writer)));
                writer.WriteEnd(EndWriteOptions.LineEnd);
            }
            GenerateExitControl(Unit.OnReset, data, writer);
        }
    }
}
