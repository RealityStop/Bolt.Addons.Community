using System;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(Latch))]
    public class LatchGenerator : VariableNodeGenerator
    {
        public LatchGenerator(Latch unit) : base(unit)
        {
        }

        public override IEnumerable<string> GetNamespaces()
        {
            yield return "Unity.VisualScripting.Community";
        }

        private Latch Unit => unit as Latch;

        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override FieldModifier FieldModifier => FieldModifier.None;

        public override string Name => "latch" + count;

        public override Type Type => typeof(LatchLogic);

        public override object DefaultValue => new LatchLogic();

        public override bool HasDefaultValue => true;

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            variableName = Name;

            if (input == Unit.enter)
            {
                writer.WriteIndented().InvokeMember(variableName.VariableHighlight(), "Update",
                writer.Action(() => GenerateValue(Unit.set, data, writer)),
                writer.Action(() => GenerateValue(Unit.reset, data, writer)),
                writer.Action(() => GenerateValue(Unit.resetDominant, data, writer))).NewLine();
                GenerateExitControl(Unit.exit, data, writer);
            }

        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.GetVariable(variableName).GetMember("Value");
        }
    }
}