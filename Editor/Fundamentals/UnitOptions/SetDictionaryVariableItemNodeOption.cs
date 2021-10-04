using System;

namespace Unity.VisualScripting.Community
{
    [FuzzyOption(typeof(SetDictionaryVariableItem))]
    public class SetDictionaryVariableItemNodeOption : UnifiedVariableUnitOption<SetDictionaryVariableItem>
    {
        [Obsolete(Serialization.ConstructorWarning)]
        public SetDictionaryVariableItemNodeOption() : base() { }

        public SetDictionaryVariableItemNodeOption(VariableKind kind, string defaultName = null) : base(kind, defaultName) { }

        protected override string NamedLabel(bool human)
        {
            return $"Set Dictionary Variable Item {name}";
        }

        protected override string UnnamedLabel(bool human)
        {
            return $"Set Dictionary {kind} Variable Item";
        }
    }
}