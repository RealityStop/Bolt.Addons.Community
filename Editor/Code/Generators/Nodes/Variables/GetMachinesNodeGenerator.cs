using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(GetMachinesNode))]
    public class GetMachineNodesGenerator : NodeGenerator<GetMachinesNode>
    {
        public GetMachineNodesGenerator(Unit unit) : base(unit) { }
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return CodeBuilder.CallCSharpUtilityMethod(Unit, MakeClickableForThisUnit("GetScriptMachines"), GenerateValue(Unit.target, data), GenerateValue(Unit.asset, data));
        }
    }
}