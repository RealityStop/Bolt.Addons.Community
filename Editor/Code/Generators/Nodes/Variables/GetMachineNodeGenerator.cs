using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(GetMachineNode))]
    public class GetMachineNodeGenerator : NodeGenerator<GetMachineNode>
    {
        public GetMachineNodeGenerator(Unit unit) : base(unit) { }
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if (data?.GetExpectedType().IsAssignableFrom(typeof(ScriptMachine)) ?? false)
            {
                data.SetCurrentExpectedTypeMet(true, typeof(ScriptMachine));
            }
            data.SetExpectedType(Unit.type == GraphSource.Macro ? typeof(ScriptGraphAsset) : typeof(string));
            var assetCode = GenerateValue(Unit.asset, data);
            data.RemoveExpectedType();
            var paramCode = Unit.type == GraphSource.Macro && CodeUtility.CleanCode(assetCode).Trim().RemoveHighlights().RemoveMarkdown() == "null" ? MakeClickableForThisUnit($"({"ScriptGraphAsset".TypeHighlight()}){"null".ConstructHighlight()}") : assetCode;
            return CodeBuilder.CallCSharpUtilityMethod(Unit, MakeClickableForThisUnit("GetScriptMachine"), GenerateValue(Unit.target, data), paramCode);
        }
    }
}