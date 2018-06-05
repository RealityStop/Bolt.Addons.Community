using System.Collections.Generic;
using Ludiq;
using UnityEngine;

namespace Bolt
{
    public abstract class MachineEvent : Event, IUnityUpdateLoop
    {
        protected MachineEvent() { }

        public FlowMachine target { get; private set; }

        /// <summary>
        /// The game object that listens for the event.
        /// </summary>
        [DoNotSerialize]
        [PortKey(nameof(target))]
        [PortLabel("Target")]
        [PortLabelHidden]
        public ValueInput targetPort { get; private set; }


        FlowMachine flowmachine;
        StateMachine statemachine;

        public override void Initialize()
        {
            base.Initialize();

            if ((FlowMachine)this.graph.owner != null)
            {
                flowmachine = (FlowMachine)this.graph.owner;
            }
            else
            {
                statemachine = (StateMachine)this.graph.owner;
            }

        }

        protected override void Definition()
        {
            base.Definition();

            targetPort = ValueInput<FlowMachine>(nameof(target), flowmachine);
        }

        public virtual void Update()
        {
            UpdateTarget();
        }
        

        private void UpdateTarget()
        {
            var wasListening = isListening;

            var newTarget = targetPort.GetValue<FlowMachine>();

            if (newTarget != target)
            {
                if (wasListening)
                {
                    StopListening();
                }

                target = newTarget;

                if (wasListening)
                {
                    StartListening(false);
                }
            }
        }

        protected void StartListening(bool updateTarget)
        {
            base.StartListening();

            if (updateTarget)
            {
                UpdateTarget();
            }

            if (target == null)
            {
                return;
            }

            

            if (!events.ContainsKey((FlowMachine)this.graph.parent))
            {
                events.Add(target, new HashSet<MachineEvent>());
            }

            events[target].Add(this);
        }

        public override void StartListening()
        {
            StartListening(true);
        }

        public override void StopListening()
        {
            base.StopListening();

            if (target == null)
            {
                return;
            }

            if (events.ContainsKey(target))
            {
                events[target].Remove(this);

                if (events[target].Count == 0)
                {
                    events.Remove(target);
                }
            }
        }

        static MachineEvent()
        {
            events = new Dictionary<FlowMachine, HashSet<MachineEvent>>();
        }

        private static readonly Dictionary<FlowMachine, HashSet<MachineEvent>> events;

        private static HashSet<TEvent> EventsToTrigger<TEvent>(FlowMachine target) where TEvent : MachineEvent
        {
            // Avoiding ToHashSetPooled because IEnumerable allocates memory,
            // whereas HashSet has a struct enumerator. Using HashSet instead
            // of array to avoid iterating twice for the count.

            var toTrigger = HashSetPool<TEvent>.New();

            if (events.ContainsKey(target))
            {
                foreach (var @event in events[target])
                {
                    if (@event is TEvent)
                    {
                        toTrigger.Add((TEvent)@event);
                    }
                }
            }

            return toTrigger;
        }

        private static void FreeEventsToTrigger<TEvent>(HashSet<TEvent> toTrigger) where TEvent : MachineEvent
        {
            HashSetPool<TEvent>.Free(toTrigger);
        }

        public static void Trigger<TEvent>(FlowMachine target) where TEvent : MachineEvent
        {
            var events = EventsToTrigger<TEvent>(target);

            foreach (var @event in events)
            {
                @event.Trigger();
            }

            FreeEventsToTrigger(events);
        }

        public static void Trigger<TEvent>(FlowMachine target, object argument) where TEvent : MachineEvent
        {
            var events = EventsToTrigger<TEvent>(target);

            foreach (var @event in events)
            {
                @event.Trigger(argument);
            }

            FreeEventsToTrigger(events);
        }

        public static void Trigger<TEvent>(FlowMachine target, params object[] arguments) where TEvent : MachineEvent
        {
            var events = EventsToTrigger<TEvent>(target);

            foreach (var @event in events)
            {
                @event.Trigger(arguments);
            }

            FreeEventsToTrigger(events);
        }
    }
}