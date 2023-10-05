using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;
using UnityEngine;

[TypeIcon(typeof(CodeBuilder))]
[UnitTitle("Code")]
public class CodeSlate : GeneratedUnit
{
    [Inspectable]
    [InspectorTextArea()]
    [UnitHeaderInspectable]
    public string CodeValue;

    protected override void Definition()
    {
        base.Definition();
    }

    public override string GeneratorLogic(int indent)
    {
        return CodeBuilder.Indent(indent) + CodeValue + "\n";
    }
}
