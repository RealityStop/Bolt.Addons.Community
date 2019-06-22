using Bolt.Addons.Community.DefinedEvents.Units;
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
            DefinedEventUnit.Trigger(target, eventData);
        }

        /// <summary>
        /// Triggers Defined Event units listening for the passed eventData globally.  Note that triggering an event 
        /// globally will not trigger events listening for the event on a particular object.
        /// This is the scripting quivalent to the Trigger Global Defined Event unit.
        /// </summary>
        /// <param name="eventData">This is a filled object of the type of event you want to trigger.</param>
        public static void TriggerGlobal(object eventData)
        {
            GlobalDefinedEventUnit.Trigger(eventData);
        }

        /// <summary>
        /// Registers a C# listener for an event on the target object.  This is the scripting
        /// equivalent to the Defined Event unit.  Notice the IDisposable return value, which allows you
        /// to end the subscription for the event (via calling the .Dispose() method).
        /// </summary>
        /// <typeparam name="T">The type to listen for.</typeparam>
        /// <param name="target">The game object to listen on to receive the event.</param>
        /// <param name="onEvent">The action or method to call when the event occurs</param>
        /// <returns>A disposable that, when .Dispose is called, will unsubscribe from the
        /// event, essentially cancelling the call to RegisterListener.</returns>
        public static IDisposable RegisterListener<T>(GameObject target, Action<T> onEvent)
        {
            return DefinedEventUnit.RegisterListener<T>(target, onEvent);
        }

        /// <summary>
        /// Registers a C# listener for an event globally.  This is the scripting
        /// equivalent to the Global Defined Event unit.  Notice the IDisposable return
        /// value, which allows you to end the subscription for the event (via calling
        /// the .Dispose() method).
        /// </summary>
        /// <typeparam name="T">The type to listen for.</typeparam>
        /// <param name="onEvent">The action or method to call when the event occurs</param>
        /// <returns>A disposable that, when .Dispose is called, will unsubscribe from the
        /// event, essentially cancelling the call to RegisterListener.</returns>
        public static IDisposable RegisterGlobalListener<T>(Action<T> onEvent)
        {
            return GlobalDefinedEventUnit.RegisterListener<T>(onEvent);
        }
    }
}