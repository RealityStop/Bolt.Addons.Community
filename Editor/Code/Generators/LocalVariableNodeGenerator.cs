using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

public class LocalVariableGenerator : NodeGenerator
{
    public Type variableType = typeof(object);
    public LocalVariableGenerator(Unit unit) : base(unit)
    {
    }
}
