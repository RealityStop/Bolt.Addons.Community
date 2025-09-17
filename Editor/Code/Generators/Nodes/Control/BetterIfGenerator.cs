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
            string trueCode = "";
    
            if (input == Unit.In)
            {
                output.Append(CodeBuilder.Indent(indent))
                      .Append(MakeClickableForThisUnit("if".ConstructHighlight() + " ("))
                      .Append(GenerateValue(Unit.Condition, data))
                      .Append(MakeClickableForThisUnit(")"))
                      .AppendLine()
                      .Append(MakeClickableForThisUnit(CodeBuilder.OpenBody(indent)))
                      .AppendLine();
    
                data.NewScope();
                if (TrueIsUnreachable())
                {
                    output.AppendLine(CodeBuilder.Indent(indent + 1) + MakeClickableForThisUnit(CodeUtility.ToolTip($"The code in the 'True' branch is unreachable due to the output of the condition value: ({CodeUtility.CleanCode(GenerateValue(Unit.Condition, data))}).", $"Unreachable Code in 'True' Branch: {Unit.True.key}", "")));
                }
                trueCode = GetNextUnit(Unit.True, data, indent + 1);
                data.ExitScope();
    
                output.Append(trueCode).AppendLine();
    
                output.Append(MakeClickableForThisUnit(CodeBuilder.CloseBody(indent)));
    
                if (Unit.False.hasAnyConnection)
                {
                    output.AppendLine()
                          .Append(CodeBuilder.Indent(indent))
                          .Append(MakeClickableForThisUnit("else".ConstructHighlight()));
    
                    if (!Unit.True.hasValidConnection || string.IsNullOrEmpty(trueCode))
                    {
                        output.Append(MakeClickableForThisUnit(CodeBuilder.MakeRecommendation(
                            "You should use the negate node and connect the true input instead")));
                    }
    
                    output.AppendLine()
                          .Append(MakeClickableForThisUnit(CodeBuilder.OpenBody(indent)))
                          .AppendLine();
    
                    data.NewScope();
                    if (FalseIsUnreachable())
                    {
                        output.AppendLine(CodeBuilder.Indent(indent + 1) + MakeClickableForThisUnit(CodeUtility.ToolTip($"The code in the 'False' branch is unreachable due to the output of the condition value: ({CodeUtility.CleanCode(GenerateValue(Unit.Condition, data))}).", $"Unreachable Code in 'False' Branch: {Unit.False.key}", "")));
                    }
                    output.Append(GetNextUnit(Unit.False, data, indent + 1))
                          .AppendLine();
                    data.ExitScope();
    
                    output.Append(MakeClickableForThisUnit(CodeBuilder.CloseBody(indent)));
                }

                if (Unit.Finished.hasAnyConnection)
                {
                    output.AppendLine()
                          .Append((Unit.Finished.connection.destination.unit as Unit)
                              .GenerateControl(Unit.Finished.connection.destination, data, indent));
                }
            }
    
    
            return output.ToString();
        }
    
        private bool TrueIsUnreachable()
        {
            if (!Unit.Condition.hasValidConnection) return false;
    
            if (Unit.Condition.GetPesudoSource().unit is Literal literal && (bool)literal.value == false)
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
    
            if (Unit.Condition.GetPesudoSource().unit is Literal literal && (bool)literal.value == true)
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
    
                return Unit.CreateIgnoreString(connectedCode).EndIgnoreContext().Cast(input.type, ShouldCast(input, data));
            }
    
            return base.GenerateValue(input, data);
        }
    } 
}
