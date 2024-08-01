using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

public class LocalVariableGenerator<TUnit> : NodeGenerator<TUnit> where TUnit : Unit
{
    public Type variableType = typeof(object);
    public string variableName = "";
    public LocalVariableGenerator(Unit unit) : base(unit)
    {
    }
}
