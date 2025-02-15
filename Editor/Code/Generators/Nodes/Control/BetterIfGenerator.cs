using System.Text;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community 
{
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
                output.Append(cachedIndent)
                      .Append(MakeSelectableForThisUnit("if".ConstructHighlight() + " ("))
                      .Append(GenerateValue(Unit.Condition, data))
                      .Append(MakeSelectableForThisUnit(")"))
                      .AppendLine()
                      .Append(MakeSelectableForThisUnit(CodeBuilder.OpenBody(indent)))
                      .AppendLine();
    
                trueData.NewScope();
                if (TrueIsUnreachable())
                {
                    output.AppendLine(CodeBuilder.Indent(indent + 1) + MakeSelectableForThisUnit(CodeUtility.ToolTip($"The code in the 'True' branch is unreachable due to the output of the condition value: ({CodeUtility.CleanCode(GenerateValue(Unit.Condition, data))}). Don't worry this does not break your code.", $"Unreachable Code in 'True' Branch: {Unit.True.key}", "")));
                }
                trueCode = GetNextUnit(Unit.True, trueData, indent + 1);
                trueData.ExitScope();
    
                output.Append(trueCode).AppendLine();
    
                output.Append(MakeSelectableForThisUnit(CodeBuilder.CloseBody(indent)));
    
                if (Unit.False.hasAnyConnection)
                {
                    output.AppendLine()
                          .Append(cachedIndent)
                          .Append(MakeSelectableForThisUnit("else".ConstructHighlight()));
    
                    if (!Unit.True.hasValidConnection || string.IsNullOrEmpty(trueCode))
                    {
                        output.Append(MakeSelectableForThisUnit(CodeBuilder.MakeRecommendation(
                            "You should use the negate node and connect the true input instead")));
                    }
    
                    output.AppendLine()
                          .Append(MakeSelectableForThisUnit(CodeBuilder.OpenBody(indent)))
                          .AppendLine();
    
                    falseData.NewScope();
                    if (FalseIsUnreachable())
                    {
                        output.AppendLine(CodeBuilder.Indent(indent + 1) + MakeSelectableForThisUnit(CodeUtility.ToolTip($"The code in the 'False' branch is unreachable due to the output of the condition value: ({CodeUtility.CleanCode(GenerateValue(Unit.Condition, data))}). Don't worry this does not break your code.", $"Unreachable Code in 'False' Branch: {Unit.False.key}", "")));
                    }
                    output.Append(GetNextUnit(Unit.False, falseData, indent + 1))
                          .AppendLine();
                    falseData.ExitScope();
    
                    output.Append(MakeSelectableForThisUnit(CodeBuilder.CloseBody(indent)));
                }

                if (Unit.Finished.hasAnyConnection)
                {
                    output.AppendLine()
                          .Append((Unit.Finished.connection.destination.unit as Unit)
                              .GenerateControl(Unit.Finished.connection.destination, data, indent));
                }
            }
    
            data.hasBroke = trueData.hasBroke && falseData.hasBroke;
    
            return output.ToString();
        }
    
        private bool TrueIsUnreachable()
        {
            if (!Unit.Condition.hasValidConnection) return false;
    
            if (Unit.Condition.connection.source.unit is Literal literal && (bool)literal.value == false)
                return true;
    
            if (Unit.Condition.GetPesudoSource() is ValueInput valueInput &&
                valueInput.hasDefaultValue && !valueInput.hasValidConnection &&
                valueInput.unit.defaultValues[valueInput.key] is bool condition &&
                condition == false)
            {
                return true;
            }
    
            return false;
        }
    
        private bool FalseIsUnreachable()
        {
            if (!Unit.Condition.hasValidConnection) return false;
    
            if (Unit.Condition.connection.source.unit is Literal literal && (bool)literal.value == true)
                return true;
    
            if (Unit.Condition.GetPesudoSource() is ValueInput valueInput &&
                valueInput.hasDefaultValue && !valueInput.hasValidConnection &&
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
}
