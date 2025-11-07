using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(WaitForFlow))]
    public class WaitForFlowGenerator : VariableNodeGenerator
    {
        private WaitForFlow Unit => unit as WaitForFlow;
        public override AccessModifier AccessModifier => AccessModifier.None;

        public override FieldModifier FieldModifier => FieldModifier.None;

        public override string Name => "waitForFlow" + count;

        public override Type Type => typeof(WaitForFlowLogic);

        public override object DefaultValue => new WaitForFlowLogic(Unit.inputCount, Unit.resetOnExit);

        public override bool HasDefaultValue => true;

        public WaitForFlowGenerator(Unit unit) : base(unit) { }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = Unit.CreateClickableString();
            output.Indent(indent);
            if (input != Unit.reset)
            {
                output.Body(before =>
                before.Clickable("if ".ControlHighlight()).Parentheses(inner => inner.InvokeMember(Name.VariableHighlight(), "Enter", Unit.controlInputs.ToList().IndexOf(input).As().Code(false))),
                (inner, indent) => inner.Ignore(GetNextUnit(Unit.exit, data, indent)), true, indent);
            }
            else
            {
                output.InvokeMember(Name.VariableHighlight(), "Reset", Array.Empty<string>()).Clickable(";").NewLine();
            }
            return output;
        }
    }
}