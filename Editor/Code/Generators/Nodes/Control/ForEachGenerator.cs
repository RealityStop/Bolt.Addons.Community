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
    private string variable = "item";
    public ForEachGenerator(Unity.VisualScripting.ForEach unit) : base(unit)
    {
    }

    public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
    {
        var output = string.Empty;

        if (input == Unit.enter)
        {
            var collection = GenerateValue(Unit.collection);
            variable = data.AddLocalNameInScope("item");
            output += CodeUtility.MakeSelectable(Unit, "\n" + CodeBuilder.Indent(indent) + $"foreach".ControlHighlight() + " (" + "var".ConstructHighlight() + $" {variable}".VariableHighlight() + " in ".ConstructHighlight() + $"{collection})");
            output += "\n";
            output += CodeUtility.MakeSelectable(Unit, CodeBuilder.OpenBody(indent));
            output += "\n";

            if (Unit.body.hasAnyConnection)
            {
                data.NewScope();
                output += GetNextUnit(Unit.body, data, indent + 1);
                data.ExitScope();
                output += "\n";
            }

            output += CodeUtility.MakeSelectable(Unit, CodeBuilder.CloseBody(indent));
        }

        if (Unit.exit.hasAnyConnection)
        {
            output += "\n";
            output += GetNextUnit(Unit.exit, data, indent);
            output += "\n";
        }

        return output;
    }

    public override string GenerateValue(ValueOutput output)
    {
        if (output == Unit.currentItem)
        {
            return variable.VariableHighlight();
        }
        else
        {
            if (Unit.dictionary)
            {
                return GenerateValue(Unit.collection) + $".Values.IndexOf({variable.VariableHighlight()})";
            }
            else
            {
                return GenerateValue(Unit.collection) + $".IndexOf({variable.VariableHighlight()})";
            }
        }
    }

    public override string GenerateValue(ValueInput input)
    {
        if (input == Unit.collection)
        {
            if (input.hasValidConnection)
            {
                return CodeUtility.MakeSelectable(input.connection.source.unit as Unit, new ValueCode((input.connection.source.unit as Unit).GenerateValue(input.connection.source), Unit.dictionary ? typeof(IDictionary) : typeof(IEnumerable), ShouldCast(input)));
            }
        }

        return base.GenerateValue(input);
    }
}