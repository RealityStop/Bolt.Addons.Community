using Bolt.Addons.Community.Fundamentals;
using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Addons.Community.Variables.Editor.UnitOptions
{
    [FuzzyOption(typeof(DecrementUnit))]
    public class DecrementUnitOption : UnifiedVariableUnitOption<DecrementUnit>
    {
        [Obsolete(Serialization.ConstructorWarning)]
        public DecrementUnitOption() : base() { }

        public DecrementUnitOption(VariableKind kind, string defaultName = null) : base(kind, defaultName) { }

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