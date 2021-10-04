using System;

namespace Unity.VisualScripting.Community
{
    [FuzzyOption(typeof(WindowIsNode))]
    public class WindowIsNodeOption : UnitOption<WindowIsNode>
    {
        private string name;
        private EditorWindowAsset asset;

        [Obsolete(Serialization.ConstructorWarning)]
        public WindowIsNodeOption() : base() { }

        public WindowIsNodeOption(WindowIsNode unit) : base(unit)
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
            return $"{LudiqGUIUtility.DimString("Is")} {unit.asset?.name}";
        }
    }
}
