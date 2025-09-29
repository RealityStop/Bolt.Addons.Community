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
            if (string.IsNullOrEmpty(unit.defaultName)) return base.Category();
            return new UnitCategory($"Community/Graphs/{unit.defaultName}");
        }

        protected override string Label(bool human)
        {
            if (string.IsNullOrEmpty(unit.defaultName)) return base.Label(human);
            return $"{LudiqGUIUtility.DimString("Get")} {unit.defaultName}";
        }
    }
}