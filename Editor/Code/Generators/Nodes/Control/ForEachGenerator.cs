using Unity;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;
using System.Collections;
[NodeGenerator(typeof(Unity.VisualScripting.ForEach))]
public sealed class ForEachGenerator : NodeGenerator<Unity.VisualScripting.ForEach>
{
    public ForEachGenerator(Unity.VisualScripting.ForEach unit) : base(unit)
    {
    }

    public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
    {
        var output = string.Empty;

        if (input == Unit.enter)
        {
            var collection = GenerateValue(Unit.collection);

            output += "\n" + CodeBuilder.Indent(indent) + $"foreach".ControlHighlight() + " (" + "var".ConstructHighlight() + " item".VariableHighlight() + " in ".ConstructHighlight() + $"{collection})";
            output += "\n";
            output += CodeBuilder.OpenBody(indent);
            output += "\n";

            if (Unit.body.hasAnyConnection)
            {
                output += (Unit.body.connection.destination.unit as Unit).GenerateControl(Unit.body.connection.destination, data, indent + 1);
                output += "\n";
            }

            output += CodeBuilder.CloseBody(indent);
        }

        if (Unit.exit.hasAnyConnection)
        {
            output += "\n";
            output += (Unit.exit.connection.destination.unit as Unit).GenerateControl(Unit.exit.connection.destination, data, indent);
            output += "\n";
        }

        return output;
    }

    public override string GenerateValue(ValueOutput output)
    {
        return "item".VariableHighlight();
    }

    public override string GenerateValue(ValueInput input)
    {
        if (input == Unit.collection)
        {
            if (input.hasValidConnection)
            {
                if(Unit.dictionary)
                {
                    return new ValueCode((input.connection.source.unit as Unit).GenerateValue(input.connection.source), typeof(IDictionary), ShouldCast(input));
                }
                else
                {
                    return new ValueCode((input.connection.source.unit as Unit).GenerateValue(input.connection.source), typeof(IEnumerable), ShouldCast(input));
                }
            }
        }

        return base.GenerateValue(input);
    }
}