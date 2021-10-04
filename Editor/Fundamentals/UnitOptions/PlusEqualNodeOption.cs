using System;

namespace Unity.VisualScripting.Community
{
    [FuzzyOption(typeof(PlusEquals))]
    public class PlusEqualNodeOption : UnifiedVariableUnitOption<PlusEquals>
    {
        [Obsolete(Serialization.ConstructorWarning)]
        public PlusEqualNodeOption() : base() { }

        public PlusEqualNodeOption(VariableKind kind, string defaultName = null) : base(kind, defaultName) { }

        protected override string NamedLabel(bool human)
        {
            return $"Plus Equal {name}";
        }

        protected override string UnnamedLabel(bool human)
        {
            return $"Plus Equal {kind} Variable";
        }
    }
}