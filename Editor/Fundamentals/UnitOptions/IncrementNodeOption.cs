using System;

namespace Unity.VisualScripting.Community
{
    [FuzzyOption(typeof(IncrementNode))]
    public class IncrementNodeOption : UnifiedVariableUnitOption<IncrementNode>
    {
        [Obsolete(Serialization.ConstructorWarning)]
        public IncrementNodeOption() : base() { }

        public IncrementNodeOption(VariableKind kind, string defaultName = null) : base(kind, defaultName) { }

        protected override string NamedLabel(bool human)
        {
            return $"Increment {name}";
        }

        protected override string UnnamedLabel(bool human)
        {
            return $"Increment {kind} Variable";
        }
    }
}