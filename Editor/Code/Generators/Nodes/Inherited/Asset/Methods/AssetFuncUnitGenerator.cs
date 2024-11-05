using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

[NodeGenerator(typeof(AssetFuncUnit))]
public class AssetFuncUnitGenerator : NodeGenerator<AssetFuncUnit>
{
    public AssetFuncUnitGenerator(Unit unit) : base(unit)
    {
    }

    public override string GenerateValue(ValueOutput output, ControlGenerationData data)
    {
        return MakeSelectableForThisUnit(Unit.method.methodName);
    }
}
