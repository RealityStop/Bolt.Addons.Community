using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(TriggerChannelEvent))]
    public class TriggerChannelEventGenerator : NodeGenerator<TriggerChannelEvent>
    {
        public TriggerChannelEventGenerator(Unit unit) : base(unit) { }
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return base.GenerateValue(output, data);
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return MakeClickableForThisUnit("EventBus".TypeHighlight() + ".Trigger(" + typeof(CommunityEvents).As().CSharpName(false, true) + "." + "ChannelEvent".VariableHighlight() + ", ") + GenerateValue(Unit.channel, data) + MakeClickableForThisUnit(");") + "\n" + GetNextUnit(Unit.OutputTrigger, data, indent);
        }
    }
}