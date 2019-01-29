using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Bolt.Addons.Community
{
    public static class DefinedEvent
    {
        /// <summary>
        /// Triggers Defined Event units listening for the passed eventData on the target gameobject.
        /// This is the scripting quivalent to the Trigger Defined Event unit.
        /// </summary>
        /// <param name="target">The game object that event units should listen on to receive the event.</param>
        /// <param name="eventData">This is a filled object of the type of event you want to trigger.</param>
        public static void Trigger(GameObject target, object eventData)
        {
            Bolt.Addons.Community.DefinedEvents.Units.DefinedEvent.Trigger(target, eventData);
        }

        /// <summary>
        /// Triggers Defined Event units listening for the passed eventData globally.  Note that triggering an event 
        /// globally will not trigger events listening for the event on a particular object.
        /// This is the scripting quivalent to the Trigger Global Defined Event unit.
        /// </summary>
        /// <param name="eventData">This is a filled object of the type of event you want to trigger.</param>
        public static void TriggerGlobal(object eventData)
        {
            Bolt.Addons.Community.DefinedEvents.Units.GlobalDefinedEvent.Trigger(eventData);
        }
    }
}
