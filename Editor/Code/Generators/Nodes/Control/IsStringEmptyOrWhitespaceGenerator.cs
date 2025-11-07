using System.Text;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(IsStringEmptyOrWhitespace))]
    public sealed class IsStringEmptyOrWhitespaceGenerator : NodeGenerator<IsStringEmptyOrWhitespace>
    {
        public IsStringEmptyOrWhitespaceGenerator(IsStringEmptyOrWhitespace unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            var output = new StringBuilder();

            if (input == Unit.Input)
            {
                output.Append(CodeBuilder.Indent(indent))
                      .Append(MakeClickableForThisUnit("if".ControlHighlight() + " ("))
                      .Append(MakeClickableForThisUnit("string".ConstructHighlight() + "." + "IsNullOrWhiteSpace(") + GenerateValue(Unit.String, data) + MakeClickableForThisUnit(")"))
                      .Append(MakeClickableForThisUnit(")"))
                      .AppendLine()
                      .Append(CodeBuilder.Indent(indent))
                      .AppendLine(MakeClickableForThisUnit("{"));

                string trueCode;

                data.NewScope();
                trueCode = GetNextUnit(Unit.True, data, indent + 1).TrimEnd();
                data.ExitScope();
                output.Append(trueCode);

                output.AppendLine()
                      .Append(CodeBuilder.Indent(indent))
                      .AppendLine(MakeClickableForThisUnit("}"));

                if (!Unit.False.hasAnyConnection)
                {
                    output.Append("\n");
                }

                if (Unit.False.hasAnyConnection)
                {
                    output.Append(CodeBuilder.Indent(indent))
                          .Append(MakeClickableForThisUnit("else".ConstructHighlight()));

                    if (!Unit.True.hasValidConnection || string.IsNullOrEmpty(trueCode))
                    {
                        output.Append(MakeClickableForThisUnit(CodeBuilder.MakeRecommendation(
                            "You should use the negate node and connect the true input instead")));
                    }

                    output.AppendLine()
                          .Append(CodeBuilder.Indent(indent))
                          .AppendLine(MakeClickableForThisUnit("{"));

                    data.NewScope();
                    output.Append(GetNextUnit(Unit.False, data, indent + 1).TrimEnd());
                    data.ExitScope();

                    output.AppendLine()
                          .Append(CodeBuilder.Indent(indent))
                          .AppendLine(MakeClickableForThisUnit("}") + "\n");
                }
            }

            return output.ToString();
        }

        public override string GenerateValue(ValueInput input, ControlGenerationData data)
        {
            if (input == Unit.String && !input.hasValidConnection && Unit.defaultValues[input.key] == null) return "".As().Code(false, Unit);
            return base.GenerateValue(input, data);
        }
    }
}