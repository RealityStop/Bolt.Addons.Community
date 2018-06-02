using System;
using Ludiq;
using Bolt;

namespace Bolt.Addons.Community.Events
{
    [Descriptor(typeof(TriggerReturnEvent))]
    class TriggerReturnEventDescriptor : UnitDescriptor<TriggerReturnEvent>
    {
        public TriggerReturnEventDescriptor(TriggerReturnEvent unit) : base(unit)
        {

        }
    }
}
