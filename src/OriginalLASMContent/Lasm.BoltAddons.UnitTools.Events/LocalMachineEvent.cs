using System;
using System.Collections;
using System.Collections.Generic;
using Ludiq;
using Bolt;
using UnityEngine;

namespace Lasm.BoltAddons.UnitTools.Events
{
    [UnitSurtitle("Local")]
    [UnitShortTitle("Machine Event")]
    [UnitTitle("Local Machine Event")]
    [UnitCategory("Events/Machine")]
    public class LocalMachineEvent : Event, IUnityUpdateLoop
    {
        private static readonly Dictionary<Graph, HashSet<LocalMachineEvent>> events;

        [DoNotSerialize]
        public ValueInput name;

        [SerializeAs("argumentCount")]
        private int _argumentCount;

        [DoNotSerialize, Inspectable, UnitHeaderInspectable("Arguments")]
        public int argumentCount
        {
            get
            {
                return this._argumentCount;
            }
            set
            {
                this._argumentCount = Mathf.Clamp(value, 0, 10);
            }
        }

        protected override void Definition()
        {
            base.Definition();

            name = ValueInput<string>("name", string.Empty);

            for (int i = 0; i < argumentCount; i++)
            {
                int _i = i;
                base.ValueOutput<object>("argument_" + i, (Recursion recursion) => this.arguments[1 + _i]);
            }
        }

        public virtual void Update()
        {
            UpdateTarget();
        }

        private void UpdateTarget()
        {
            
            
            foreach (Unit unit in graph.units)
            while (true)
            {

            }
        }
        

        public override void StartListening()
        {
            base.StartListening();
        }
        
        public override void StopListening()
        {
            base.StopListening();
        }

        protected override bool ShouldTrigger()
        {
            throw base.InvalidArgumentCount(0);
        }

        protected override bool ShouldTrigger(object argument)
        {
            return CompareNames(name, (string)argument);
        }

        protected override bool ShouldTrigger(object[] arguments)
        {
            return CompareNames(name, (string)arguments[0]);
        }

        public static void Trigger(int count)
        {
             
        }
    }

    
}
