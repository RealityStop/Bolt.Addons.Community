using Unity.VisualScripting.Community.Libraries.CSharp;
using System;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(LimitedTrigger))]
    public sealed class TriggerXTimesGenerator : VariableNodeGenerator
    {
        public TriggerXTimesGenerator(LimitedTrigger unit) : base(unit)
        {
            NameSpaces = "Unity.VisualScripting.Community";
        }
        private LimitedTrigger Unit => unit as LimitedTrigger;
        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override FieldModifier FieldModifier => FieldModifier.None;

        public override string Name => "limitedTrigger_" + count;

        public override object DefaultValue => new LimitedTriggerLogic();

        public override Type Type => typeof(LimitedTriggerLogic);

        public override bool HasDefaultValue => true;

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = string.Empty;

            if (input == Unit.Input)
            {
                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit(Name.VariableHighlight() + ".Initialize(") + GenerateValue(Unit.Times, data).End(Unit);
                output += "\n";
                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit($"if".ControlHighlight() + $"({Name.VariableHighlight() + ".Trigger()"})");
                output += "\n";
                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("{");
                output += "\n";

                if (Unit.Exit.hasAnyConnection)
                {
                    output += GetNextUnit(Unit.Exit, data, indent + 1);
                    output += "\n";
                }
                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("}");
                output += "\n";
                if (Unit.After.hasValidConnection)
                {
                    output += GetNextUnit(Unit.After, data, indent);
                }
            }
            else if (input == Unit.Reset)
            {
                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit($"{Name.VariableHighlight()} = " + "false".ConstructHighlight() + ";") + "\n";
            }

            return output;
        }
    }
}