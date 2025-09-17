using System;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(CounterNode))]
    public class CounterNodeGenerator : VariableNodeGenerator
    {
        public CounterNodeGenerator(CounterNode unit) : base(unit)
        {
            NameSpaces = "Unity.VisualScripting.Community";
        }
        private CounterNode Unit => unit as CounterNode;
        public override AccessModifier AccessModifier => AccessModifier.Private;

        public override FieldModifier FieldModifier => FieldModifier.None;

        public override string Name => "Counter" + count;

        public override Type Type => typeof(CounterLogic);

        public override object DefaultValue => new CounterLogic();

        public override bool HasDefaultValue => true;

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            variableName = Name;
            var output = string.Empty;

            if (input == Unit.enter)
            {
                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit(variableName.VariableHighlight() + ".Update()\n");
                output += GetNextUnit(Unit.exit, data, indent);
            }
            else if (input == Unit.reset)
            {
                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit(variableName.VariableHighlight() + ".Reset()\n");
            }

            return output;
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return MakeClickableForThisUnit(variableName.VariableHighlight() + "." + "Count".VariableHighlight());
        }
    }
}