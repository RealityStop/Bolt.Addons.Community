using System.Collections.Generic;
using Unity.VisualScripting;

namespace Bolt.Addons.Community.DefinedEvents.Support.Internal
{
    interface IDefinedEventUnit
    {
        List<ValueOutput> outputPorts { get; }
    }
}