﻿using Bolt.Addons.Community.Fundamentals;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Variables.Editor.UnitOptions
{
    [FuzzyOption(typeof(SetDictionaryVariableItem))]
    public class SetDictionaryVariableItemUnitOption : UnifiedVariableUnitOption<SetDictionaryVariableItem>
    {
        [Obsolete(Serialization.ConstructorWarning)]
        public SetDictionaryVariableItemUnitOption() : base() { }

        public SetDictionaryVariableItemUnitOption(VariableKind kind, string defaultName = null) : base(kind, defaultName) { }

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