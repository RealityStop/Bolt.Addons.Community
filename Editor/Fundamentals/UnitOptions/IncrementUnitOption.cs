using Bolt.Addons.Community.Fundamentals;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.Variables.Editor.UnitOptions
{
    [FuzzyOption(typeof(IncrementUnit))]
    public class IncrementUnitOption : UnifiedVariableUnitOption<IncrementUnit>
    {
        [Obsolete(Serialization.ConstructorWarning)]
        public IncrementUnitOption() : base() { }

        public IncrementUnitOption(VariableKind kind, string defaultName = null) : base(kind, defaultName) { }

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