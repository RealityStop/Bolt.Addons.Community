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

    protected override string DefinedSurtitle()
    {
        return target.method.classAsset.title;
    }

    protected override EditorTexture DefinedIcon()
    {
        return target.method.returnType.Icon();
    }

    protected override string DefinedTitle()
    {
        return target.method.classAsset.title + "." + target.method.methodName;
    }

    protected override string DefinedShortTitle()
    {
        return target.method.methodName;
    }
}
