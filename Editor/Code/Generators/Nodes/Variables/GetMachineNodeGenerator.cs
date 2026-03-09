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

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (data?.GetExpectedType().IsAssignableFrom(typeof(SMachine)) ?? false)
            {
                data.MarkExpectedTypeMet(typeof(SMachine));
            }

            var name = Unit.type == GraphSource.Macro ? "GetScriptMachineWithAsset" : "GetScriptMachineWithName";
            writer.CallCSharpUtilityMethod("name", writer.Action(() => GenerateValue(Unit.target, data, writer)), writer.Action(() =>
            {
                using (data.Expect(Unit.type == GraphSource.Macro ? typeof(ScriptGraphAsset) : typeof(string)))
                {
                    GenerateValue(Unit.asset, data, writer);
                }
            }));
        }
    }
}