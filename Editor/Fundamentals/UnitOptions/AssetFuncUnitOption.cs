using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[FuzzyOption(typeof(AssetFuncUnit))]
public class AssetFuncUnitOption : UnitOption<AssetFuncUnit>
{
    public AssetFuncUnitOption(AssetFuncUnit assetFuncUnit) : base(assetFuncUnit)
    {
        sourceScriptGuids = sourceScriptGuids = Unity.VisualScripting.LinqUtility.ToHashSet(UnitBase.GetScriptGuids(unit.method.returnType));
    }

    protected override string Label(bool human)
    {
        return "this." + unit.method.methodName;
    }

    public override bool favoritable => false;

    protected override int Order()
    {
        return 0;
    }

    public override string SearchResultLabel(string query)
    {
        return "this." + unit.method.methodName;
    }

    protected override string Haystack(bool human)
    {
        return $"this{(human ? ": " : ".")}{Label(human)}";
    }
}
