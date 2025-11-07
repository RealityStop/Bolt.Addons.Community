using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;
#if VISUAL_SCRIPTING_1_7
using SMachine = Unity.VisualScripting.ScriptMachine;
#else
using SMachine = Unity.VisualScripting.FlowMachine;
#endif
namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(GetMachineNode))]
    public class GetMachineNodeGenerator : NodeGenerator<GetMachineNode>
    {
        public GetMachineNodeGenerator(Unit unit) : base(unit) { }
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if (data?.GetExpectedType().IsAssignableFrom(typeof(SMachine)) ?? false)
            {
                data.SetCurrentExpectedTypeMet(true, typeof(SMachine));
            }
            data.SetExpectedType(Unit.type == GraphSource.Macro ? typeof(ScriptGraphAsset) : typeof(string));
            var assetCode = GenerateValue(Unit.asset, data);
            data.RemoveExpectedType();
            var paramCode = Unit.type == GraphSource.Macro && CodeUtility.CleanCode(assetCode).Trim().RemoveHighlights().RemoveMarkdown() == "null" ? MakeClickableForThisUnit($"({"ScriptGraphAsset".TypeHighlight()}){"null".ConstructHighlight()}") : assetCode;
            return CodeBuilder.CallCSharpUtilityMethod(Unit, MakeClickableForThisUnit("GetScriptMachine"), GenerateValue(Unit.target, data), paramCode);
        }
    }
}