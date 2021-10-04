using System;

namespace Unity.VisualScripting.Community
{
    [FuzzyOption(typeof(GetMachineVariableNode))]
    public class GetMachineVariableNodeOption : UnitOption<GetMachineVariableNode>
    {
        [Obsolete(Serialization.ConstructorWarning)]
        public GetMachineVariableNodeOption() : base() { }

        public GetMachineVariableNodeOption(GetMachineVariableNode unit) : base(unit)
        {
        }

        protected override UnitCategory Category()
        {
            if (unit.asset == null) return base.Category();
            return new UnitCategory($"Community/Graphs/{unit.asset?.name}");
        }

        protected override string Label(bool human)
        {
            if (unit.asset == null) return base.Label(human);
            return $"{LudiqGUIUtility.DimString("Get")} {unit.defaultName}";
        }
    }
}