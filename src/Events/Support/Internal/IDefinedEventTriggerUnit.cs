using System.Collections.Generic;

namespace Bolt.Addons.Community.DefinedEvents.Support.Internal
{
    interface IDefinedEventTriggerUnit
    {
        List<ValueInput> inputPorts { get; }
    }
}