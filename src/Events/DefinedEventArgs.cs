using System;
using UnityEngine;

namespace Bolt.Addons.Community.DefinedEvents
{
    public struct DefinedEventArgs
    {
        public GameObject target;
        public object eventData;

        public DefinedEventArgs(GameObject target, object eventData)
        {
            this.target = target;
            this.eventData = eventData;
        }
    }
}