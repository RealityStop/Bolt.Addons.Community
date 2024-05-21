using Unity;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;
[NodeGenerator(typeof(Unity.VisualScripting.Community.BetterIf))]
public sealed class BetterIfGenerator : NodeGenerator<Unity.VisualScripting.Community.BetterIf>
{
    public BetterIfGenerator(Unity.VisualScripting.Community.BetterIf unit) : base(unit)
    {
    }

    public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
    {
        var output = string.Empty;

        if (input == Unit.In)
        {
            output += CodeBuilder.Indent(indent) + "if".ConstructHighlight() + " (" + GenerateValue(Unit.Condition) + ")";
            output += "\n";
            output += CodeBuilder.OpenBody(indent);
            output += "\n";
            output += Unit.True.hasAnyConnection ? (Unit.True.connection.destination.unit as Unit).GenerateControl(Unit.True.connection.destination, data, indent + 1) : string.Empty;
            output += "\n";
            output += CodeBuilder.CloseBody(indent);

            if (Unit.False.hasAnyConnection)
            {
                output += "\n";
                output += CodeBuilder.Indent(indent) + "else".ConstructHighlight();
                output += "\n";
                output += CodeBuilder.OpenBody(indent);
                output += "\n";

                output += Unit.False.hasAnyConnection ? (Unit.False.connection.destination.unit as Unit).GenerateControl(Unit.False.connection.destination, data, indent + 1) : string.Empty;
                output += "\n";
                output += CodeBuilder.CloseBody(indent);
            }

            if (Unit.Finished.hasAnyConnection)
            {
                output += "\n";
                output += (Unit.Finished.connection.destination.unit as Unit).GenerateControl(Unit.Finished.connection.destination, data, indent);
                output += "\n";
            }
        }

        return output;
    }
    public override string GenerateValue(ValueInput input)
    {
        if (input == Unit.Condition)
        {
            if (Unit.Condition.hasAnyConnection)
            {
                return new ValueCode((input.connection.source.unit as Unit).GenerateValue(input.connection.source), input.type, ShouldCast(input));
            }
        }
        return base.GenerateValue(input);
    }
}