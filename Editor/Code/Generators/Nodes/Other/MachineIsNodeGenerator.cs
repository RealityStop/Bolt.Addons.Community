using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(MachineIsNode))]
    public class MachineIsNodeGenerator : NodeGenerator<MachineIsNode>
    {
        public MachineIsNodeGenerator(Unit unit) : base(unit) { }
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            data.SetExpectedType(typeof(GameObject));
            string target = GenerateValue(Unit.target, data);
            data.RemoveExpectedType();
            data.SetExpectedType(typeof(ScriptGraphAsset));
            string asset = GenerateValue(Unit.asset, data);
            data.RemoveExpectedType();
            var builder = Unit.CreateClickableString();
            builder.InvokeMember(typeof(CSharpUtility), "MachineIs", p1 => p1.Ignore(target), p2 => p2.Ignore(asset));
            return builder;
        }
    }
}