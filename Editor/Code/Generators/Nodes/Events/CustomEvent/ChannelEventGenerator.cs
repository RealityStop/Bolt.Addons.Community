using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.VisualScripting.Community.Utility;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(ChannelEvent))]
    public class ChannelEventGenerator : AwakeMethodNodeGenerator
    {
        private ChannelEvent Unit => unit as ChannelEvent;
        public override ControlOutput OutputPort => Unit.trigger;

        public override List<ValueOutput> OutputValues => new List<ValueOutput>();

        public override List<TypeParam> Parameters => new List<TypeParam>();

        public ChannelEventGenerator(Unit unit) : base(unit) { }
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return base.GenerateValue(output, data);
        }

        public override string GenerateAwakeCode(ControlGenerationData data, int indent)
        {
            return CodeBuilder.Indent(indent) + MakeClickableForThisUnit(typeof(EventBus).As().CSharpName(false, true) + $".Register<{typeof(Channel).As().CSharpName(false, true)}>(" + typeof(CommunityEvents).As().CSharpName(false, true) + $".{"ChannelEvent".VariableHighlight()}, ") + $"{MakeClickableForThisUnit("channel".VariableHighlight() + " => ") + (Unit.coroutine ? GetCoroutineCode(data, indent + 1) : GetNormalCode(data, indent + 1))}{MakeClickableForThisUnit(");")}" + "\n";
        }

        private string GetCoroutineCode(ControlGenerationData data, int indent)
        {
            string output = MakeClickableForThisUnit("{") + "\n";
            output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("if".ControlHighlight() + " (" + "channel".VariableHighlight() + " == " + GenerateValue(Unit.Channel, data) + ")") + "\n";
            output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("{") + "\n";
            output += CodeBuilder.Indent(indent + 1) + MakeClickableForThisUnit($"StartCoroutine({Name}());") + "\n";
            output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("}") + "\n";
            output += MakeClickableForThisUnit("}");
            return output;
        }


        private string GetNormalCode(ControlGenerationData data,int indent)
        {
            string output = MakeClickableForThisUnit("{") + "\n";
            output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("if".ControlHighlight() + " (" + "channel".VariableHighlight() + " == ") + GenerateValue(Unit.Channel, data) + MakeClickableForThisUnit(")") + "\n";
            output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("{") + "\n";
            output += CodeBuilder.Indent(indent + 1) + MakeClickableForThisUnit($"{Name}();") + "\n";
            output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("}") + "\n";
            output += MakeClickableForThisUnit("}");
            return output;
        }
        protected override string GenerateCode(ControlInput input, ControlGenerationData data, int indent)
        {
            return GetNextUnit(Unit.trigger, data, indent);
        }
    }
}