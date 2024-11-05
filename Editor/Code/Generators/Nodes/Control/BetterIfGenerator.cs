using System.Text;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

[NodeGenerator(typeof(BetterIf))]
public sealed class BetterIfGenerator : NodeGenerator<BetterIf>
{
    public BetterIfGenerator(BetterIf unit) : base(unit) { }

    public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
    {
        var output = new StringBuilder();
        string cachedIndent = CodeBuilder.Indent(indent);
        string cachedIndentPlusOne = CodeBuilder.Indent(indent + 1);

        var trueData = new ControlGenerationData(data);
        var falseData = new ControlGenerationData(data);
        string trueCode = "";

        if (input == Unit.In)
        {
            // Construct "if" statement
            output.Append(cachedIndent)
                  .Append(MakeSelectableForThisUnit("if".ConstructHighlight() + " ("))
                  .Append(GenerateValue(Unit.Condition, data))
                  .Append(MakeSelectableForThisUnit(")"))
                  .AppendLine()
                  .Append(MakeSelectableForThisUnit(CodeBuilder.OpenBody(indent)))
                  .AppendLine();

            // Handle the true branch
            if (!ShouldSkipTrueBranch())
            {
                trueData.NewScope();
                trueCode = (Unit.True.connection.destination.unit as Unit)
                    .GenerateControl(Unit.True.connection.destination, trueData, indent + 1);
                trueData.ExitScope();

                output.Append(trueCode).AppendLine();
            }
            else
            {
                output.Append(cachedIndentPlusOne)
                      .AppendLine(MakeSelectableForThisUnit(
                          "/* Unreachable code skipping for performance */"));
            }

            output.Append(MakeSelectableForThisUnit( CodeBuilder.CloseBody(indent)));

            // Handle the "else" branch if present
            if (Unit.False.hasAnyConnection)
            {
                output.AppendLine()
                      .Append(cachedIndent)
                      .Append(MakeSelectableForThisUnit("else".ConstructHighlight()));

                if (!Unit.True.hasValidConnection || string.IsNullOrEmpty(trueCode))
                {
                    output.Append(CodeBuilder.MakeRecommendation(
                        "You should use the negate node and connect the true input instead"));
                }

                output.AppendLine()
                      .Append(MakeSelectableForThisUnit(CodeBuilder.OpenBody(indent)))
                      .AppendLine();

                if (!ShouldSkipFalseBranch())
                {
                    falseData.NewScope();
                    output.Append((Unit.False.connection.destination.unit as Unit)
                        .GenerateControl(Unit.False.connection.destination, falseData, indent + 1))
                          .AppendLine();
                    falseData.ExitScope();
                }
                else
                {
                    output.Append(cachedIndentPlusOne)
                          .AppendLine(MakeSelectableForThisUnit(
                              "/* Unreachable code skipping for performance */"));
                }

                output.Append(MakeSelectableForThisUnit(CodeBuilder.CloseBody(indent)));
            }

            // Handle the "finished" branch if present
            if (Unit.Finished.hasAnyConnection)
            {
                output.AppendLine()
                      .Append((Unit.Finished.connection.destination.unit as Unit)
                          .GenerateControl(Unit.Finished.connection.destination, data, indent));
            }
        }

        // Update break status in data
        data.hasBroke = trueData.hasBroke && falseData.hasBroke;

        return output.ToString();
    }

    private bool ShouldSkipTrueBranch()
    {
        if (!Unit.Condition.hasValidConnection) return false;

        if (Unit.Condition.connection.source.unit is Literal literal && (bool)literal.value == false)
            return true;

        if (Unit.Condition.GetPsudoSource() is ValueInput valueInput &&
            valueInput.hasDefaultValue &&
            valueInput.unit.defaultValues[valueInput.key] is bool condition &&
            condition == false)
        {
            return true;
        }

        return false;
    }

    private bool ShouldSkipFalseBranch()
    {
        if (!Unit.Condition.hasValidConnection) return false;

        if (Unit.Condition.connection.source.unit is Literal literal && (bool)literal.value == true)
            return true;

        if (Unit.Condition.GetPsudoSource() is ValueInput valueInput &&
            valueInput.hasDefaultValue &&
            valueInput.unit.defaultValues[valueInput.key] is bool condition &&
            condition == true)
        {
            return true;
        }

        return false;
    }

    public override string GenerateValue(ValueInput input, ControlGenerationData data)
    {
        if (input == Unit.Condition && Unit.Condition.hasAnyConnection)
        {
            data.SetExpectedType(input.type);
            var connectedCode = GetNextValueUnit(input, data);
            data.RemoveExpectedType();

            return new ValueCode(connectedCode, input.type, ShouldCast(input, data));
        }

        return base.GenerateValue(input, data);
    }
}
