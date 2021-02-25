using System.Collections.Generic;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.DefinedEvents.Support.Internal
{
    interface IDefinedEventTriggerUnit
    {
        List<ValueInput> inputPorts { get; }
    }
}