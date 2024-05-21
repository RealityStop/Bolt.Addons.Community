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

            data.AddLocalName("i");

            if (data.localNames.Any(name => name.Contains("i") && name.Length > 1 && char.IsDigit(name[1])))
            {
                variable = data.localNames.Last(name => name.Contains("i") && name.Length > 1 && char.IsDigit(name[1]));
            }
            
            output += CodeBuilder.Indent(indent) + $"for".ControlHighlight() + "(int".ConstructHighlight() + $" {variable} ".VariableHighlight() + $"= {initialization}; " + variable.VariableHighlight() + $" < {condition}; " + variable.VariableHighlight() + $" += {iterator})";
            output += "\n";
            output += CodeBuilder.OpenBody(indent);
            output += "\n";

            if (Unit.body.hasAnyConnection)
            {
                data.NewScope();
                output += (Unit.body.connection.destination.unit as Unit).GenerateControl(Unit.body.connection.destination, data, indent + 1);
                data.ExitScope();
            }

            output += "\n" + CodeBuilder.CloseBody(indent);
        }

        if (Unit.exit.hasAnyConnection)
        {
            output += "\n";
            output += (Unit.exit.connection.destination.unit as Unit).GenerateControl(Unit.exit.connection.destination, data, indent);
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
            return input.connection.source.type == typeof(object) ? $"(({input.type.DisplayName().TypeHighlight()})" + (input.connection.source.unit as Unit).GenerateValue(input.connection.source) + ")" : string.Empty + (input.connection.source.unit as Unit).GenerateValue(input.connection.source);
        }
        else
        {
            return Unit.defaultValues[input.key].ToString();
        }
    }
}