using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ludiq;
using Bolt;

namespace Bolt.Addons.Community.Events {
    [Descriptor(typeof(TriggerReturnEvent))]
    public class TriggerReturnEventDescriptor : UnitDescriptor<TriggerReturnEvent>
    {
        public TriggerReturnEventDescriptor(TriggerReturnEvent unit) : base(unit)
        {

        }
    }
}
