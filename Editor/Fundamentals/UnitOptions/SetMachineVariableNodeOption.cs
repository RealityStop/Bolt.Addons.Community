using System;

namespace Unity.VisualScripting.Community
{
    [FuzzyOption(typeof(SetMachineVariableNode))]
    public class SetMachineVariableNodeOption : UnitOption<SetMachineVariableNode>
    {
        private string name;
        private ScriptGraphAsset asset;

        [Obsolete(Serialization.ConstructorWarning)]
        public SetMachineVariableNodeOption() : base() { }

        public SetMachineVariableNodeOption(SetMachineVariableNode unit) : base(unit)
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
            return $"{LudiqGUIUtility.DimString("Set")} {unit.defaultName}";
        }
    }
}