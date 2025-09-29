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
            if (string.IsNullOrEmpty(unit.defaultName)) return base.Category();
            return new UnitCategory($"Community/Graphs/{unit.defaultName}");
        }

        protected override string Label(bool human)
        {
            if (string.IsNullOrEmpty(unit.defaultName)) return base.Label(human);
            return $"{LudiqGUIUtility.DimString("Set")} {unit.defaultName}";
        }
    }
}