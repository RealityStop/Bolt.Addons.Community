using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community.CSharp
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
        
        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.Write(Name);
            writer.Write(".ToggleBool(");
            GenerateValue(Unit.Value, data, writer);
            writer.Write(")");
        }
    }
}