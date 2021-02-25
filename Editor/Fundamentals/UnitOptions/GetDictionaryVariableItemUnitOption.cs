using Bolt.Addons.Community.Fundamentals;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Variables.Editor.UnitOptions
{
    [FuzzyOption(typeof(GetDictionaryVariableItem))]
    public class GetDictionaryVariableItemUnitOption : UnifiedVariableUnitOption<GetDictionaryVariableItem>
    {
        [Obsolete(Serialization.ConstructorWarning)]
        public GetDictionaryVariableItemUnitOption() : base() { }

        public GetDictionaryVariableItemUnitOption(VariableKind kind, string defaultName = null) : base(kind, defaultName) { }

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