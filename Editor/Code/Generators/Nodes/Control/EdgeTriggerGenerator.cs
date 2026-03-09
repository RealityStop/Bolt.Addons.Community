using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(EdgeTrigger))]
    public class EdgeTriggerGenerator : VariableNodeGenerator
    {
        private EdgeTrigger Unit => unit as EdgeTrigger;
        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override FieldModifier FieldModifier => FieldModifier.None;

        public override string Name => "edgeTrigger" + count;

        public override Type Type => typeof(EdgeTriggerLogic);

        public override object DefaultValue => new EdgeTriggerLogic();

        public override bool HasDefaultValue => true;

        public EdgeTriggerGenerator(Unit unit) : base(unit) { }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            writer.WriteIndented("if ".ControlHighlight()).Parentheses(inner => inner.InvokeMember(Name.VariableHighlight(), "ShouldTrigger", 
            inner.Action(() => GenerateValue(Unit.inValue, data, inner))));
            writer.Braces((inner, indent) =>
            {
                GenerateChildControl(Unit.exit, data, writer);
            }).NewLine();
        }
    }
}