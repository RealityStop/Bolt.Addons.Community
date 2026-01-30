using System;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community.CSharp
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

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            writer.WriteIndented("if ".ControlHighlight()).Parentheses(inner => inner.InvokeMember(Name.VariableHighlight(), "Changed", inner.Action(() => GenerateValue(Unit.input, data, inner)))).NewLine();
            writer.WriteLine("{");
            using (writer.IndentedScope(data)) 
            {
                GenerateChildControl(Unit.onChange, data, writer); 
            }
            writer.WriteLine("}");
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.GetMember(Name.VariableHighlight(), "PreviousValue");
        }
    }
}