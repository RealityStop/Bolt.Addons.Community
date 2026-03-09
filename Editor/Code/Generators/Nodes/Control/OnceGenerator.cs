using Unity.VisualScripting.Community.Libraries.CSharp;
using System;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Unity.VisualScripting.Once))]
    public sealed class OnceGenerator : VariableNodeGenerator
    {
        public OnceGenerator(Unity.VisualScripting.Once unit) : base(unit)
        {
        }
        private Unity.VisualScripting.Once Unit => unit as Unity.VisualScripting.Once;
        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override FieldModifier FieldModifier => FieldModifier.None;

        public override string Name => "Once_" + count;

        public override object DefaultValue => null;

        public override Type Type => typeof(bool);

        public override bool HasDefaultValue => false;

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input == Unit.enter)
            {
                writer.WriteIndented("if ".ControlHighlight()).Parentheses(w => w.Write($"!{Name.VariableHighlight()}")).NewLine();
                writer.WriteLine("{");

                using (writer.IndentedScope(data))
                {
                    GenerateChildControl(Unit.once, data, writer);
                    writer.SetVariable(Name.VariableHighlight(), writer.BoolString(true), WriteOptions.Indented, EndWriteOptions.LineEnd);
                }

                writer.WriteLine("}");

                GenerateExitControl(Unit.after, data, writer);
            }
            else if (input == Unit.reset)
            {
                writer.SetVariable(Name.VariableHighlight(), writer.BoolString(false), WriteOptions.Indented, EndWriteOptions.LineEnd);
            }
        }
    }
}