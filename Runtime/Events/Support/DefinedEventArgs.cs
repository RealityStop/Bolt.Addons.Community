using System;
using UnityEngine;

namespace Bolt.Addons.Community.DefinedEvents.Support
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