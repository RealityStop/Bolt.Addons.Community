using System;

namespace Unity.VisualScripting.Community
{
    [FuzzyOption(typeof(GetDictionaryVariableItem))]
    public class GetDictionaryVariableItemNodeOption : UnifiedVariableUnitOption<GetDictionaryVariableItem>
    {
        [Obsolete(Serialization.ConstructorWarning)]
        public GetDictionaryVariableItemNodeOption() : base() { }

        public GetDictionaryVariableItemNodeOption(VariableKind kind, string defaultName = null) : base(kind, defaultName) { }

        protected override string NamedLabel(bool human)
        {
            return $"Get Dictionary Variable Item {name}";
        }

        protected override string UnnamedLabel(bool human)
        {
            return $"Get Dictionary {kind} Variable Item";
        }
    }
}