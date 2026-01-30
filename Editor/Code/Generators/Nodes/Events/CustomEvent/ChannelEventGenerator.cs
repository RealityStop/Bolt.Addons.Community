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

        public override void GenerateAwakeCode(ControlGenerationData data, CodeWriter writer)
        {
            writer.WriteIndented(typeof(EventBus).As().CSharpName(false, true) + $".Register<{typeof(Channel).As().CSharpName(false, true)}>(" + typeof(CommunityEvents).As().CSharpName(false, true) + $".{"ChannelEvent".VariableHighlight()}, " + "channel".VariableHighlight() + " => ");
            using (writer.Indented())
            {
                if (Unit.coroutine)
                {
                    GetCoroutineCode(data, writer);
                }
                else
                {
                    GetNormalCode(data, writer);
                }
            }
            writer.Write(");");
        }

        private void GetCoroutineCode(ControlGenerationData data, CodeWriter writer)
        {
            writer.Write("{").NewLine();
            writer.WriteIndented("if ".ControlHighlight() + "(" + "channel".VariableHighlight() + " == ");
            GenerateValue(Unit.Channel, data, writer);
            writer.Write(")").NewLine();

            writer.WriteLine("{");
            using (writer.Indented())
            {
                writer.WriteIndented($"StartCoroutine({Name}());").NewLine();
            }
            writer.WriteIndented("}");
        }

        private void GetNormalCode(ControlGenerationData data, CodeWriter writer)
        {
            writer.Write("{").NewLine();
            writer.WriteIndented("if ".ControlHighlight() + "(" + "channel".VariableHighlight() + " == ");
            GenerateValue(Unit.Channel, data, writer);
            writer.Write(")").NewLine();

            writer.WriteLine("{");
            using (writer.Indented())
            {
                writer.WriteIndented($"{Name}();").NewLine();
            }
            writer.WriteIndented("}");
        }

        protected override void GenerateCode(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            GenerateChildControl(Unit.trigger, data, writer);
        }
    }
}