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
        public abstract string GenerateUpdateCode(ControlGenerationData data, int indent);
        public sealed override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            if (!typeof(MonoBehaviour).IsAssignableFrom(data.ScriptType)) return CodeBuilder.Indent(indent) + MakeClickableForThisUnit(CodeUtility.ErrorTooltip($"{unit.GetType().DisplayName()} only works with ScriptGraphAssets, ScriptMachines or a ClassAsset that inherits MonoBehaviour", $"Could not generate {unit.GetType().DisplayName()}", "")) + "\n";
            return GenerateCode(input, data, indent);
        }

        protected abstract string GenerateCode(ControlInput input, ControlGenerationData data, int indent);
    }
}