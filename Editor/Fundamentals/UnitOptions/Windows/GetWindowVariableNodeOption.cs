using System;

namespace Unity.VisualScripting.Community
{
    [FuzzyOption(typeof(GetWindowVariableNode))]
    public class GetWindowVariableNodeOption : UnitOption<GetWindowVariableNode>
    {
        private string name;
        private EditorWindowAsset asset;

        [Obsolete(Serialization.ConstructorWarning)]
        public GetWindowVariableNodeOption() : base() { }

        public GetWindowVariableNodeOption(GetWindowVariableNode unit) : base(unit)
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
            return $"{LudiqGUIUtility.DimString("Get")} {unit.defaultName}";
        }
    }
}
