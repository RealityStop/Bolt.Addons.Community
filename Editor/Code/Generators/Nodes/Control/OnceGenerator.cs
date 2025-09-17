using Unity;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;
using System.Text;
using System;
[NodeGenerator(typeof(Unity.VisualScripting.Once))]
public sealed class OnceGenerator : VariableNodeGenerator
{
    public OnceGenerator(Unity.VisualScripting.Once unit) : base(unit)
    {
    }
    private Unity.VisualScripting.Once Unit => unit as Unity.VisualScripting.Once;
    public override AccessModifier AccessModifier => AccessModifier.Private;

    public override FieldModifier FieldModifier => FieldModifier.None;

    public override string Name => "Once_" + count;

    public override object DefaultValue => null;

    public override Type Type => typeof(bool);

    public override bool HasDefaultValue => false;

    public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
    {
        var output = string.Empty;

        if (input == Unit.enter)
        {
            output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit($"if".ConstructHighlight() + $"(!{Name.VariableHighlight()})");
            output += "\n";
            output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("{");
            output += "\n";

            if (Unit.once.hasAnyConnection)
            {
                output += GetNextUnit(Unit.once, data, indent + 1);
                output += "\n";
            }

            output += CodeBuilder.Indent(indent + 1) + MakeClickableForThisUnit($"{Name.VariableHighlight()} = " + "true".ConstructHighlight() + ";");
            output += "\n";
            output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit("}");
            output += "\n";

            if (Unit.after.hasValidConnection)
            {
                output += GetNextUnit(Unit.after, data, indent);
            }
        }

        else if (input == Unit.reset)
        {
            output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit($"{Name.VariableHighlight()} = " + "false".ConstructHighlight() + ";") + "\n";
        }

        return output;
    }
}