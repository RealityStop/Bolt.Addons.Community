using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(TriggerChannelEvent))]
    public class TriggerChannelEventGenerator : NodeGenerator<TriggerChannelEvent>
    {
        public TriggerChannelEventGenerator(Unit unit) : base(unit) { }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            writer.InvokeMember(typeof(EventBus), "Trigger",
            writer.Action(() => writer.GetMember(typeof(CommunityEvents), "ChannelEvent")),
            writer.Action(() => GenerateValue(Unit.channel, data, writer)));

            writer.NewLine();

            GenerateExitControl(Unit.OutputTrigger, data, writer);
        }
    }
}