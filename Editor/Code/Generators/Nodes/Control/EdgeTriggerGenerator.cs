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

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var builder = Unit.CreateClickableString();
            builder.If(condition => condition.InvokeMember(Name.VariableHighlight(), "ShouldTrigger", p => p.Ignore(GenerateValue(Unit.inValue, data))),
            (body, indent) => body.Ignore(GetNextUnit(Unit.exit, data, indent).TrimEnd()), true, indent);
            return builder;
        }
    }
}