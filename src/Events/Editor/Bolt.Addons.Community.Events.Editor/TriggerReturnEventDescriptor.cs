using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ludiq;
using Bolt;

namespace Bolt.Addons.Community.Events {
    [Widget(typeof(TriggerReturnEvent))]
    public class TriggerReturnEventWidget : UnitWidget<TriggerReturnEvent>
    {
        public TriggerReturnEventWidget(TriggerReturnEvent unit) : base(unit)
        {

        }

        protected override NodeColorMix baseColor
        {
            get
            {
                return NodeColor.Gray;
            }
        }

    }

    public class TriggerReturnEventDescriptor : UnitWidget<TriggerReturnEvent>
    {
        public TriggerReturnEventDescriptor(TriggerReturnEvent unit) : base(unit)
        {

        }
        

    }
}
