using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(CoroutineToFlow))]
    public class CoroutineToFlowGenerator : NodeGenerator<CoroutineToFlow>
    {
        public CoroutineToFlowGenerator(Unit unit) : base(unit) { }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            // This node will work like a sequence node because I don't think there is a way to
            // Stop the coroutine for a specific method and run the code there.
            if (!typeof(MonoBehaviour).IsAssignableFrom(data.ScriptType))
            {
                writer.WriteErrorDiagnostic($"{typeof(CoroutineToFlow).DisplayName()} only works with ScriptGraphAssets, ScriptMachines or a ClassAsset that inherits MonoBehaviour",
                $"Could not generate {typeof(CoroutineToFlow).DisplayName()}", WriteOptions.IndentedNewLineAfter);
                return;
            }

            GenerateExitControl(Unit.Converted, data, writer);
            GenerateExitControl(Unit.Corutine, data, writer);
        }
    }
}