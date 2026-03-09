using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(MachineIsNode))]
    public class MachineIsNodeGenerator : NodeGenerator<MachineIsNode>
    {
        public MachineIsNodeGenerator(Unit unit) : base(unit) { }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.InvokeMember(typeof(CSharpUtility), "MachineIs", 
            writer.Action(() => GenerateValue(Unit.target, data, writer)),
            writer.Action(() => GenerateValue(Unit.asset, data, writer)));
        }
    }
}