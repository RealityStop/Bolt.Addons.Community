using System.Collections.Generic;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("Bolt.Addons.Community.DefinedEvents.Support.Internal.IDefinedEventUnit")]
    interface IDefinedEventNode
    {
        List<ValueOutput> outputPorts { get; }
    }
}