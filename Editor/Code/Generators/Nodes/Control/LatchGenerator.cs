using System;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(Latch))]
    public class LatchGenerator : VariableNodeGenerator
    {
        public LatchGenerator(Latch unit) : base(unit)
        {
            NameSpaces = "Unity.VisualScripting.Community";
        }

        private Latch Unit => unit as Latch;

        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override FieldModifier FieldModifier => FieldModifier.None;

        public override string Name => "latch" + count;

        public override Type Type => typeof(LatchLogic);

        public override object DefaultValue => new LatchLogic();

        public override bool HasDefaultValue => true;

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            variableName = Name;

            if (input == Unit.enter)
            {
                string set = GenerateValue(Unit.set, data);
                string reset = GenerateValue(Unit.reset, data);
                string resetDom = GenerateValue(Unit.resetDominant, data);
                string methodCall = $"{MakeClickableForThisUnit(variableName.VariableHighlight())}{MakeClickableForThisUnit(".Update(")}{set}{MakeClickableForThisUnit(", ")}{reset}{MakeClickableForThisUnit(", ")}{resetDom}{MakeClickableForThisUnit(");")}";

                return CodeBuilder.Indent(indent) + methodCall + "\n" +
                       GetNextUnit(Unit.exit, data, indent);
            }

            return string.Empty;
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return MakeClickableForThisUnit(variableName.VariableHighlight() + "." + "Value".VariableHighlight());
        }
    }
}