using System;

namespace Unity.VisualScripting.Community
{
    [FuzzyOption(typeof(SetWindowVariableNode))]
    public class SetWindowVariableNodeOption : UnitOption<SetWindowVariableNode>
    {
        private string name;
        private EditorWindowAsset asset;

        [Obsolete(Serialization.ConstructorWarning)]
        public SetWindowVariableNodeOption() : base() { }

        public SetWindowVariableNodeOption(SetWindowVariableNode unit) : base(unit)
        {
        }

        protected override UnitCategory Category()
        {
            if (unit.asset == null) return base.Category();
            return new UnitCategory($"Community/Editor/{unit.asset?.name}");
        }

        protected override string Label(bool human)
        {
            if (unit.asset == null) return base.Label(human);
            return $"{LudiqGUIUtility.DimString("Set")} {unit.defaultName}";
        }
    }
}
