using Bolt.Addons.Community.Variables.Units;
using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Addons.Community.Variables.UnitOptions
{
    [FuzzyOption(typeof(IncrementUnit))]
    public class IncrementUnitOption : UnifiedVariableUnitOption<IncrementUnit>
    {
        [Obsolete(Serialization.ConstructorWarning)]
        public IncrementUnitOption() : base() { }

        public IncrementUnitOption(VariableKind kind, string defaultName = null) : base(kind, defaultName) { }

        protected override string DefaultNameLabel()
        {
            return $"Increment {defaultName}";
        }

        protected override string GenericLabel()
        {
            return $"Increment {kind} Variable";
        }
    }
}