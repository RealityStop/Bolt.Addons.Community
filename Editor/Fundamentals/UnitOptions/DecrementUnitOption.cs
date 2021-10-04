using System;

namespace Unity.VisualScripting.Community
{
    [FuzzyOption(typeof(DecrementNode))]
    public class DecrementNodeOption : UnifiedVariableUnitOption<DecrementNode>
    {
        [Obsolete(Serialization.ConstructorWarning)]
        public DecrementNodeOption() : base() { }

        public DecrementNodeOption(VariableKind kind, string defaultName = null) : base(kind, defaultName) { }

        protected override string NamedLabel(bool human)
        {
            return $"Decrement {name}";
        }

        protected override string UnnamedLabel(bool human)
        {
            return $"Decrement {kind} Variable";
        }
    }
}