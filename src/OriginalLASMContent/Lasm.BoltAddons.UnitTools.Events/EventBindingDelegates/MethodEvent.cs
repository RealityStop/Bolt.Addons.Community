using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ludiq;
using Bolt;
using UnityEngine;

namespace Lasm.BoltAddons.UnitTools.Events
{
    [TypeIcon(typeof(Event))]
    public class MethodEvent
    {
        /// <summary>
        /// The name of the event.
        /// </summary>
        public string @name { get; set; }
        public string @eventId { get; set; }
        /// <summary>
        /// The instance id for this Method Event. It auto creates a GetHashCode value for itself.
        /// </summary>
        public int instanceId { get; private set; }

        public AotDictionary methodEventData { get; private set; }


        /// <summary>
        /// Create a new method events. Populates these triggerable events into an Event Method Listener, by binding them just like delegates.
        /// </summary>
        /// <param name="name">The name of this Method Event.</param>
        public MethodEvent(string @name, IGraph @graph, GameObject @gameobject, int eventId, AotDictionary arguments)
        {
            this.@name = @name;
            instanceId = new object().GetHashCode();
            methodEventData = new AotDictionary();
            methodEventData.Add("name", @name);
            methodEventData.Add("instanceId", instanceId);
            methodEventData.Add("graph", @graph);
            methodEventData.Add("gameObject", @gameobject);
            methodEventData.Add("arguments", @arguments);
            methodEventData.Add("eventId", eventId);
        }
    }
}