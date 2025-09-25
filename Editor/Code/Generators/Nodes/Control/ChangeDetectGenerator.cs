using System;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(ChangeDetect))]
    public class ChangeDetectGenerator : VariableNodeGenerator
    {
        private ChangeDetect Unit => unit as ChangeDetect;
        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override FieldModifier FieldModifier => FieldModifier.None;

        public override string Name => "changeDetect" + count;

        public override Type Type => typeof(ChangeDetectLogic);

        public override object DefaultValue => new ChangeDetectLogic();

        public override bool HasDefaultValue => true;

        public ChangeDetectGenerator(Unit unit) : base(unit) { }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var builder = Unit.CreateClickableString();
            builder.Body(before =>
            before.Clickable("if ".ControlHighlight()).Parentheses(inner =>
            inner.InvokeMember(Name.VariableHighlight(), "Changed", p1 => p1.Ignore(GenerateValue(Unit.input, data)))),
            (body, indent) => body.Ignore(GetNextUnit(Unit.onChange, data, indent).TrimEnd()), true, indent);

            return builder;
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return MakeClickableForThisUnit(Name.VariableHighlight() + "." + "PreviousValue".VariableHighlight());
        }
    }
}