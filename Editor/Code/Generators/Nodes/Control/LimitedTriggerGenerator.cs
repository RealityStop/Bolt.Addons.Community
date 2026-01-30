using Unity.VisualScripting.Community.Libraries.CSharp;
using System;
using System.Collections.Generic;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(LimitedTrigger))]
    public sealed class LimitedTriggerGenerator : VariableNodeGenerator
    {
        public LimitedTriggerGenerator(LimitedTrigger unit) : base(unit)
        {
        }

        public override IEnumerable<string> GetNamespaces()
        {
            yield return "Unity.VisualScripting.Community";
        }

        private LimitedTrigger Unit => unit as LimitedTrigger;
        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override FieldModifier FieldModifier => FieldModifier.None;

        public override string Name => "limitedTrigger_" + count;

        public override object DefaultValue => new LimitedTriggerLogic();

        public override Type Type => typeof(LimitedTriggerLogic);

        public override bool HasDefaultValue => true;

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input == Unit.Input)
            {
                writer.WriteIndented().InvokeMember(Name.VariableHighlight(), "Initialize", writer.Action(() =>
                {
                    GenerateValue(Unit.Times, data, writer);
                })).WriteEnd(EndWriteOptions.LineEnd);

                writer.WriteIndented("if ".ControlHighlight()).Parentheses(w => w.InvokeMember(Name.VariableHighlight(), "Trigger")).NewLine();
                writer.WriteLine("{");

                using (writer.IndentedScope(data))
                {
                    GenerateChildControl(Unit.Exit, data, writer);
                }

                writer.WriteLine("}");

                GenerateExitControl(Unit.After, data, writer);
            }
            else if (input == Unit.Reset)
            {
                writer.WriteIndented().InvokeMember(Name.VariableHighlight(), "Reset").WriteEnd(EndWriteOptions.LineEnd);
            }
        }
    }
}