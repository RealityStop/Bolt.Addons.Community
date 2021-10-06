using System;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [RenamedFrom("Bolt.Addons.Community.DefinedEvents.Support.DefinedEventArgs")]
    public struct DefinedEventArgs
    {
        public object eventData;

        public DefinedEventArgs(object eventData)
        {
            this.eventData = eventData;
        }
    }
}