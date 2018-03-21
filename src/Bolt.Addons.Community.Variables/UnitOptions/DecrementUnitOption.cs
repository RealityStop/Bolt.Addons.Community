using Bolt.Addons.Community.Variables.Units;
using Ludiq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bolt.Addons.Community.Variables.UnitOptions
{
    [FuzzyOption(typeof(DecrementUnit))]
    public class DecrementUnitOption : UnifiedVariableUnitOption<DecrementUnit>
    {
        [Obsolete(Serialization.ConstructorWarning)]
        public DecrementUnitOption() : base() { }

        public DecrementUnitOption(VariableKind kind, string defaultName = null) : base(kind, defaultName) { }

        protected override string DefaultNameLabel()
        {
            return $"Decrement {defaultName}";
        }

        protected override string GenericLabel()
        {
            return $"Decrement {kind} Variable";
        }
    }
}