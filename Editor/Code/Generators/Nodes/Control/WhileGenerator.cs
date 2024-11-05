using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

[NodeGenerator(typeof(While))]
public class WhileGenerator : NodeGenerator<While>
{
    public WhileGenerator(Unit unit) : base(unit)
    {
    }

    public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
    {
        var output = "";
        output += CodeBuilder.Indent(indent) + MakeSelectableForThisUnit("while".ControlHighlight() + " (") + GenerateValue(Unit.condition, data) + MakeSelectableForThisUnit(")") + "\n";
        output += CodeBuilder.Indent(indent) + MakeSelectableForThisUnit("{") + "\n";
        output += GetNextUnit(Unit.body, data, indent + 1);
        output += "\n" + CodeBuilder.Indent(indent) + MakeSelectableForThisUnit("}") + "\n";
        output += GetNextUnit(Unit.exit, data, indent);
        return output;
    }
}
