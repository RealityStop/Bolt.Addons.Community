using Unity;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;
[NodeGenerator(typeof(Unity.VisualScripting.For))]
public sealed class ForGenerator : NodeGenerator<Unity.VisualScripting.For>
{
    private string variable = "i";
    public ForGenerator(Unity.VisualScripting.For unit) : base(unit)
    {
    }

    public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
    {
        var output = string.Empty;

        if (input == Unit.enter)
        {
            var initialization = GenerateValue(Unit.firstIndex);
            var condition = GenerateValue(Unit.lastIndex);
            var iterator = GenerateValue(Unit.step);

            variable = data.AddLocalNameInScope("i");
            
            output += CodeUtility.MakeSelectable(Unit, CodeBuilder.Indent(indent) + $"for".ControlHighlight() + "(int".ConstructHighlight() + $" {variable} ".VariableHighlight() + $"= {initialization}; " + variable.VariableHighlight() + $" < {condition}; " + variable.VariableHighlight() + $" += {iterator})");
            output += "\n";
            output += CodeUtility.MakeSelectable(Unit, CodeBuilder.OpenBody(indent));
            output += "\n";

            if (Unit.body.hasAnyConnection)
            {
                data.NewScope();
                output += GetNextUnit(Unit.body, data, indent + 1);
                data.ExitScope();
            }

            output += CodeUtility.MakeSelectable(Unit, CodeBuilder.CloseBody(indent));
        }

        if (Unit.exit.hasAnyConnection)
        {
            output += "\n";
            output += GetNextUnit(Unit.exit, data, indent);
        }


        return output;
    }

    public override string GenerateValue(ValueOutput output)
    {
        return variable.VariableHighlight();
    }

    public override string GenerateValue(ValueInput input)
    {
        if (input.hasValidConnection)
        {
            return CodeUtility.MakeSelectable(input.connection.source.unit as Unit, new ValueCode((input.connection.source.unit as Unit).GenerateValue(input.connection.source), input.type, ShouldCast(input)));
        }
        else
        {
            return Unit.defaultValues[input.key].ToString();
        }
    }
}