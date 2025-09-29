using System;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Gate))]
    public class GateGenerator : VariableNodeGenerator
    {
        private Gate Unit => unit as Gate;
        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override FieldModifier FieldModifier => FieldModifier.None;

        public override string Name => "gate" + count;

        public override Type Type => typeof(GateLogic);

        public override object DefaultValue => new GateLogic();

        public override bool HasDefaultValue => true;

        public GateGenerator(Unit unit) : base(unit) { }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var builder = Unit.CreateClickableString();
            builder.Indent(indent);
            if (input == Unit.enter)
            {
                builder.Body(before =>
                before.Clickable("if ".ControlHighlight()).Parentheses(inner =>
                inner.InvokeMember(Name.VariableHighlight(), "IsOpen", p1 => p1.Ignore(GenerateValue(Unit.initialState, data).TrimEnd()))),
                (body, indent) => body.Ignore(GetNextUnit(Unit.exit, data, indent)), true, indent);
            }
            else if (input == Unit.open)
            {
                builder.InvokeMember(Name.VariableHighlight(), "Open", Array.Empty<string>()).EndLine();
            }
            else if (input == Unit.close)
            {
                builder.InvokeMember(Name.VariableHighlight(), "Close", Array.Empty<string>()).EndLine();
            }
            else if (input == Unit.toggle)
            {
                builder.InvokeMember(Name.VariableHighlight(), "Toggle", Array.Empty<string>()).EndLine();
            }
            return builder;
        }
    }
}