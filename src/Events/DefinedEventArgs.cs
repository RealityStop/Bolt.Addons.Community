using System;

namespace Bolt.Addons.Community.DefinedEvents
{
    public struct DefinedEventArgs
    {
        public object eventData;

        public DefinedEventArgs(object eventData)
        {
            this.eventData = eventData;
        }
    }
}