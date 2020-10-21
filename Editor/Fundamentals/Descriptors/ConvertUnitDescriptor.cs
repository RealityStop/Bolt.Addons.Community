using Bolt;
using Ludiq;
using UnityEngine;

namespace Bolt.Addons.Community.Fundamentals.Units.Utility
{
    [Descriptor(typeof(ConvertUnit))]
    public sealed class ConvertUnitDescriptor : UnitDescriptor<ConvertUnit>
    {
        public ConvertUnitDescriptor(ConvertUnit target) : base(target)
        {
        }

        protected override void DefinedPort(IUnitPort port, UnitPortDescription description)
        {
            base.DefinedPort(port, description);

            description.showLabel = false;
        }
    }
} 