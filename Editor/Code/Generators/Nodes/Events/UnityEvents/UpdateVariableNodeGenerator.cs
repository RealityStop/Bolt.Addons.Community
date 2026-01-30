using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    /// <summary>
    /// Used for generating nodes that require a Variable and the Update method to function.
    /// For example OnButtonClick.
    /// </summary>
    public abstract class UpdateVariableNodeGenerator : VariableNodeGenerator
    {
        protected UpdateVariableNodeGenerator(Unit unit) : base(unit)
        {
        }
        public override AccessModifier AccessModifier => AccessModifier.Private;
        public override string Name => unit.GetType().DisplayName().Replace(" ", "") + count;
        public abstract string GenerateUpdateCode(ControlGenerationData data, CodeWriter writer);

        protected sealed override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (!typeof(MonoBehaviour).IsAssignableFrom(data.ScriptType))
            {
                writer.WriteErrorDiagnostic($"{unit.GetType().DisplayName()} only works with ScriptGraphAssets, ScriptMachines or a ClassAsset that inherits MonoBehaviour", $"Could not generate {unit.GetType().DisplayName()}", WriteOptions.IndentedNewLineAfter);
                return;
            }

            GenerateCode(input, data, writer);
        }

        protected abstract string GenerateCode(ControlInput input, ControlGenerationData data, CodeWriter writer);
    }
}