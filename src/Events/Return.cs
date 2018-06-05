using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ludiq;
using Bolt;
using System;
using System.Collections.ObjectModel;

namespace Bolt.Addons.Community.Events
{
    [UnitCategory("Events/Return")]
    public class Return : Unit
    { 
        [DoNotSerialize][PortLabelHidden]
        public ControlInput enter;
        [DoNotSerialize]
        public ValueInput returnUnit;
        [DoNotSerialize]
        public ValueInput returnValue;

        protected override void Definition()
        {
            enter = ControlInput("enter", new Action<Flow>(Enter));

            returnUnit = ValueInput<TriggerReturnEvent>("returnUnit");

            returnValue = ValueInput<object>("return");
        }

        private void Enter(Flow flow)
        {
            returnUnit.GetValue<TriggerReturnEvent>().returnValue = returnValue.GetValue<object>();
            returnUnit.GetValue<TriggerReturnEvent>().InvokeReturn();
        }
    }
}
