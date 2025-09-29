using System;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(ToggleBool))]
    public class ToggleBoolGenerator : VariableNodeGenerator
    {
        private ToggleBool Unit => unit as ToggleBool;
        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override FieldModifier FieldModifier => FieldModifier.None;

        public override string Name => "toggleBool" + count;

        public override Type Type => typeof(ToggleBoolLogic);

        public override object DefaultValue => new ToggleBoolLogic();

        public override bool HasDefaultValue => true;

        public ToggleBoolGenerator(Unit unit) : base(unit) { }
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            var builder = Unit.CreateClickableString();
            builder.InvokeMember(Name.VariableHighlight(), "ToggleBool", p1 => p1.Ignore(GenerateValue(Unit.Value, data)));
            return builder;
        }
    }
}