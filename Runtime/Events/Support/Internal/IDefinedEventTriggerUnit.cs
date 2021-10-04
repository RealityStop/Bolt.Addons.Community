using System.Collections.Generic;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("Bolt.Addons.Community.DefinedEvents.Support.Internal.IDefinedEventTriggerUnit")]
    interface IDefinedEventTriggerUnit
    {
        List<ValueInput> inputPorts { get; }
    }
}