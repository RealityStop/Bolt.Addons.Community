using Unity;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;
[NodeGenerator(typeof(Unity.VisualScripting.For))]
public sealed class ForGenerator : LocalVariableGenerator<Unity.VisualScripting.For>
{
    public ForGenerator(Unity.VisualScripting.For unit) : base(unit)
    {
    }

    public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
    {
        var output = string.Empty;

        if (input == Unit.enter)
        {
            var initialization = GenerateValue(Unit.firstIndex, data);
            var condition = GenerateValue(Unit.lastIndex, data);
            var iterator = GenerateValue(Unit.step, data);

            variableName = data.AddLocalNameInScope("i", typeof(int));
            variableType = typeof(int);
            
            output += CodeUtility.MakeSelectable(Unit, CodeBuilder.Indent(indent) + $"for".ControlHighlight() + "(int".ConstructHighlight() + $" {variableName} ".VariableHighlight() + $"= {initialization}; " + variableName.VariableHighlight() + $" < {condition}; " + variableName.VariableHighlight() + $" += {iterator})");
            output += "\n";
            output += CodeUtility.MakeSelectable(Unit, CodeBuilder.OpenBody(indent));
            output += "\n";

            if (Unit.body.hasAnyConnection)
            {
                data.NewScope();
                output += GetNextUnit(Unit.body, data, indent + 1);
                data.ExitScope();
            }

            output += "\n";
            output += CodeUtility.MakeSelectable(Unit, CodeBuilder.CloseBody(indent));
        }

        if (Unit.exit.hasAnyConnection)
        {
            output += "\n";
            output += GetNextUnit(Unit.exit, data, indent);
        }


        return output;
    }

    public override string GenerateValue(ValueOutput output, ControlGenerationData data)
    {
        return variableName.VariableHighlight();
    }

    public override string GenerateValue(ValueInput input, ControlGenerationData data)
    {
        if (input.hasValidConnection)
        {
            return new ValueCode(GetNextValueUnit(input, data, true), input.type, ShouldCast(input, false, data));
        }
        else
        {
            return Unit.defaultValues[input.key].As().Code(false);
        }
    }
}