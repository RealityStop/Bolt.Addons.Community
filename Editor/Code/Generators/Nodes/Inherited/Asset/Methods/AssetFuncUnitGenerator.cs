using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(AssetFuncUnit))]
    public class AssetFuncUnitGenerator : NodeGenerator<AssetFuncUnit>
    {
        public AssetFuncUnitGenerator(Unit unit) : base(unit)
        {
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return MakeClickableForThisUnit(Unit.method.methodName);
        }
    }
}
