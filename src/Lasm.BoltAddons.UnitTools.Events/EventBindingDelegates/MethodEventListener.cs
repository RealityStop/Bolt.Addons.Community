using System;
using System.Collections;
using System.Collections.Generic;
using Ludiq;
using Bolt;
using UnityEngine;

namespace Lasm.BoltAddons.UnitTools.Events
{
    [TypeIcon(typeof(Event))] [Inspectable]
    public class MethodEventListener
    {
        [Inspectable]
        public string name { get; set; }
        [Inspectable]
        public int instanceId { get; private set; }
        [Inspectable]
        public AotList methodEvents { get; private set; }

        public MethodEventListener(string name)
        {
            this.name = name;
            instanceId = new object().GetHashCode();
            methodEvents = new AotList();
        }

        public void AttachMethodEvent(MethodEvent @methodEvent, AotDictionary arguments)
        {
            methodEvent.methodEventData["arguments"] = arguments;
            methodEvents.Add(methodEvent);
        }

        public void DetachMethodEvent(MethodEvent @methodEvent)
        {
            methodEvents.Remove(methodEvent);
        }

        public void Clear(out MethodEventListener methodEventListener)
        {
            methodEventListener = this;
            methodEventListener.methodEvents.Clear();
        }

        public void AttachMultipleOfMethodEvent(int amount, MethodEvent methodEvent, AotList arguments)
        {
            for (int i = 0; i < amount; i++)
            {
                methodEvent.methodEventData["arguments"] = (AotDictionary)arguments[i];
                methodEvents.Add(methodEvent);
            }
        }

        public void DetachMultipleOfMethodEvent(int amount, MethodEvent methodEvent)
        {
            for (int i = 0; i < amount; i++)
            {
                DetachMethodEvent(methodEvent);
            }
        }

        public void AttachMultipleMethodEvents(AotList methodEvents, List<AotDictionary> argumentsList)
        {
            int count = 0;

            foreach(MethodEvent methodEvent in methodEvents)
            {
                methodEvent.methodEventData["arguments"] = argumentsList[count];
                methodEvents.Add(methodEvent);

                count++;
            }
        }

        public void DetachMultipleMethodEvents(AotList methodEvents)
        {
            foreach(MethodEvent methodEvent in methodEvents)
            {
                DetachMethodEvent(methodEvent);
            }
        }

        public void SetListenerToManager(string @name, MethodEventManager @manager)
        {
            manager.methodEventListeners.Add(name, manager);
        }

        public void UnsetListenerFromManager(string @name, MethodEventManager @manager)
        {
            manager.methodEventListeners.Remove(name);
        }

        public MethodEvent FindEvent(MethodEventListener methodEventListener, string @name, int @eventId)
        {
            MethodEvent newEvent = null;

            foreach (MethodEvent methodEvent in methodEventListener.methodEvents)
            {
                if (methodEvent.name == name && (int)methodEvent.methodEventData["eventId"] == eventId)
                {
                    newEvent = methodEvent;
                    break;
                } 
            }

            return newEvent;
        }

        public void Trigger()
        {
            foreach (MethodEvent methodEvent in methodEvents)
            {
                GameObject gameobject = (GameObject)methodEvent.methodEventData["gameObject"];
                CustomEvent.Trigger(gameobject, "TriggerMethodEvent",  (AotDictionary)methodEvent.methodEventData["info"]);
            }
        }
    }
}
