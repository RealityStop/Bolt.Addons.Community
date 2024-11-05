using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Descriptor(typeof(AssetFuncUnit))]
public class AssetFuncUnitDescriptor : UnitDescriptor<AssetFuncUnit>
{
    public AssetFuncUnitDescriptor(AssetFuncUnit target) : base(target)
    {
    }

    protected override EditorTexture DefinedIcon()
    {
        return target.method.returnType.Icon();
    }

    protected override string DefinedShortTitle()
    {
        if (target.method.classAsset != null)
        {
            return target.method.classAsset.name + "." + target.method.methodName;
        }
        else if (target.method.structAsset != null)
        {
            return target.method.structAsset.name + "." + target.method.methodName;
        }
        return "Asset Func Unit";
    }
}
